using UnityEngine;
using UnityEditor;

public class BP3CharacterPBRShaderGUI : ShaderGUI {
    public enum BlendMode {
        Opaque,
        Cutout,
        Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
        Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
    }

    static class Styles {
        public static GUIContent albedo = new GUIContent("Albedo", "Albedo (RGB)");
        public static GUIContent alpha = new GUIContent("Alpha", "Transparency");
        public static GUIContent alphaCutoff = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");
        public static GUIContent mask = new GUIContent("Mask", "OA (R) and Smothness (G) and Metallic (B)");
        public static GUIContent smoothnessScale = new GUIContent("Smoothness", "Smoothness scale factor");
        public static GUIContent occlusionStrength = new GUIContent("OA Strength", "Occlusion Strength value");
        public static GUIContent normal = new GUIContent("Normal", "Normal (OpenGL)");
        public static GUIContent emission = new GUIContent("Emission", "Emission (RGB)");
        public static GUIContent emissiveWarning = new GUIContent("Emissive value is animated but the material has not been configured to support emissive. Please make sure the material itself has some amount of emissive.");
        public static GUIContent highlights = new GUIContent("Specular Highlights", "Specular Highlights");
        public static GUIContent reflections = new GUIContent("Reflections", "Glossy Reflections");

        public static string renderingMode = "Rendering Mode";
        public static string primaryMaps = "Primary Maps";
        public static string forward = "Forward Rendering Options";
        public static string advanced = "Advanced Options";

        public static readonly string[] blendNames = System.Enum.GetNames(typeof(BlendMode));
    }

    MaterialProperty blendMode = null;
    MaterialProperty albedoMap = null;
    MaterialProperty albedoColor = null;
    MaterialProperty alphaMap = null;
    MaterialProperty alphaCutoff = null;
    MaterialProperty maskMap = null;
    MaterialProperty smoothnessScale = null;
    MaterialProperty occlusionStrength = null;
    MaterialProperty bumpMap = null;
    MaterialProperty bumpScale = null;
    MaterialProperty emissionMap = null;
    MaterialProperty emissionColor = null;
    MaterialProperty highlights = null;
    MaterialProperty reflections = null;

    const float kMaxfp16 = 65536f; // Clamp to a value that fits into fp16.
    ColorPickerHDRConfig colorPickerHDRConfig = new ColorPickerHDRConfig(0f, kMaxfp16, 1 / kMaxfp16, 3f);
    MaterialEditor materialEditor;
    bool firstTimeApply = true;

    public void FindProperties(MaterialProperty[] props) {
        blendMode = FindProperty("_Mode", props);
        albedoMap = FindProperty("_MainTex", props);
        albedoColor = FindProperty("_Color", props);
        alphaMap = FindProperty("_AlphaTex", props, false);
        alphaCutoff = FindProperty("_Cutoff", props, false);
        maskMap = FindProperty("_MetallicGlossMap", props);
        smoothnessScale = FindProperty("_GlossMapScale", props);
        occlusionStrength = FindProperty("_OcclusionStrength", props);
        bumpMap = FindProperty("_BumpMap", props);
        bumpScale = FindProperty("_BumpScale", props);
        emissionMap = FindProperty("_EmissionMap", props);
        emissionColor = FindProperty("_EmissionColor", props);
        highlights = FindProperty("_SpecularHighlights", props, false);
        reflections = FindProperty("_GlossyReflections", props, false);
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader) {
        // _Emission property is lost after assigning Standard shader to the material thus transfer it before assigning the new shader
        if (material.HasProperty("_Emission")) {
            material.SetColor("_EmissionColor", material.GetColor("_Emission"));
        }

        base.AssignNewShaderToMaterial(material, oldShader, newShader);

        if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/")) {
            SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
            return;
        }

        var blendMode = BlendMode.Opaque;
        if (oldShader.name.Contains("/Transparent/Cutout/")) {
            blendMode = BlendMode.Cutout;
        }
        else if (oldShader.name.Contains("/Transparent/")) {
            // NOTE: legacy shaders did not provide physically based transparency therefore Fade mode
            blendMode = BlendMode.Fade;
        }
        material.SetFloat("_Mode", (float)blendMode);
        MaterialChanged(material);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props) {
        FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
        this.materialEditor = materialEditor;
        var material = materialEditor.target as Material;

        // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing material to a standard shader.
        // Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
        if (firstTimeApply) {
            MaterialChanged(material);
            firstTimeApply = false;
        }

        ShaderPropertiesGUI(material);
    }

    public void ShaderPropertiesGUI(Material material) {
        EditorGUIUtility.labelWidth = 0f; // Use default labelWidth

        EditorGUI.BeginChangeCheck(); // Detect any changes to the material
        {
            BlendModePopup();

            GUILayout.Label(Styles.primaryMaps, EditorStyles.boldLabel); // Primary properties
            DoAlbedoArea(material);
            DoMetallicArea();
            materialEditor.TexturePropertySingleLine(Styles.normal, bumpMap, bumpMap.textureValue != null ? bumpScale : null);
            DoEmissionArea(material);

            EditorGUI.BeginChangeCheck();
            materialEditor.TextureScaleOffsetProperty(albedoMap);
            if (EditorGUI.EndChangeCheck()) {
                // Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake
                emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset;
            }

            EditorGUILayout.Space();

            GUILayout.Label(Styles.forward, EditorStyles.boldLabel);
            if (highlights != null) {
                materialEditor.ShaderProperty(highlights, Styles.highlights);
            }
            if (reflections != null) {
                materialEditor.ShaderProperty(reflections, Styles.reflections);
            }
        }
        if (EditorGUI.EndChangeCheck()) {
            var targets = blendMode.targets;
            for (var i = 0; i < targets.Length; ++i) {
                var m = (Material) targets[i];
                MaterialChanged(m);
            }
        }

        EditorGUILayout.Space();

        // NB renderqueue editor is not shown on purpose: we want to override it based on blend mode
        GUILayout.Label(Styles.advanced, EditorStyles.boldLabel);
        materialEditor.EnableInstancingField();
        materialEditor.DoubleSidedGIField();
    }

    void BlendModePopup() {
        EditorGUI.showMixedValue = blendMode.hasMixedValue;
        var mode = (BlendMode)blendMode.floatValue;

        EditorGUI.BeginChangeCheck();
        mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
        if (EditorGUI.EndChangeCheck()) {
            materialEditor.RegisterPropertyChangeUndo("Rendering Mode");
            blendMode.floatValue = (float)mode;
        }

        EditorGUI.showMixedValue = false;
    }

    void DoAlbedoArea(Material material) {
        materialEditor.TexturePropertySingleLine(Styles.albedo, albedoMap, albedoColor);
        if (((BlendMode)material.GetFloat("_Mode") > BlendMode.Opaque)) {
            materialEditor.TexturePropertySingleLine(Styles.alpha, alphaMap);
        }

        if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout)) {
            materialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoff, MaterialEditor.kMiniTextureFieldLabelIndentLevel);
        }
    }

    void DoMetallicArea() {
        materialEditor.TexturePropertySingleLine(Styles.mask, maskMap);
        var indentation = 2; // align with labels of texture properties
        materialEditor.ShaderProperty(smoothnessScale, Styles.smoothnessScale, indentation);
        materialEditor.ShaderProperty(occlusionStrength, Styles.occlusionStrength, indentation);
    }

    void DoEmissionArea(Material material) {
        if (materialEditor.EmissionEnabledProperty()) { // Emission for GI?
            // Texture and HDR color controls
            materialEditor.TexturePropertyWithHDRColor(Styles.emission, emissionMap, emissionColor, colorPickerHDRConfig, false);

            // If texture was assigned and color was black set color to white
            var brightness = emissionColor.colorValue.maxColorComponent;
            if (emissionMap.textureValue != null && brightness <= 0f) {
                emissionColor.colorValue = Color.white;
            }

            // change the GI flag and fix it up with emissive as black if necessary
            materialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
        }
    }

    void MaterialChanged(Material material) {
        SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
        SetMaterialKeywords(material);
    }

    void SetupMaterialWithBlendMode(Material material, BlendMode blendMode) {
        switch (blendMode) {
        case BlendMode.Opaque:
            material.SetOverrideTag("RenderType", "");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = -1;
            break;
        case BlendMode.Cutout:
            material.SetOverrideTag("RenderType", "TransparentCutout");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.EnableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            break;
        case BlendMode.Fade:
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            break;
        case BlendMode.Transparent:
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            break;
        }
    }

    void SetMaterialKeywords(Material material) {
        // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
        // (MaterialProperty value might come from renderer material property block)
        SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));
        SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));

        // A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
        // or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
        // The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
        MaterialEditor.FixupEmissiveFlag(material);
        var shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
        SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);
    }

    void SetKeyword(Material m, string keyword, bool state) {
        if (state)
            m.EnableKeyword(keyword);
        else
            m.DisableKeyword(keyword);
    }
}