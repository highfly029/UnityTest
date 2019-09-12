using System;
using UnityEngine;
using UnityEditor;

public class BaseShaderGUI : ShaderGUI {
    protected MaterialEditor materialEditor;
    protected MaterialProperty blendMode;
    protected MaterialProperty alphaTex;
    protected MaterialProperty alphaCutoff;

    bool firstTimeApply = true;

    protected enum BlendMode {
        Opaque,
        Cutout,
        Fade,
        Transparent
    }

    static class Styles {
        public static string renderingModeText = "Rendering Mode";
        public static readonly string[] blendNames = Enum.GetNames(typeof(BlendMode));
        public static GUIContent alphaTexText = new GUIContent("Alpha (A)", "Alpha (A)");
        public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props) {
        // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
        FindProperties(props);

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

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader) {
        base.AssignNewShaderToMaterial(material, oldShader, newShader);

        if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/")) {
            SetupBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
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

    public virtual void ShaderPropertiesGUI(Material material) {
        EditorGUIUtility.labelWidth = 0f;

        EditorGUI.showMixedValue = blendMode.hasMixedValue;
        var mode = (BlendMode)blendMode.floatValue;

        EditorGUI.BeginChangeCheck();
        mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingModeText, (int)mode, Styles.blendNames);
        if (EditorGUI.EndChangeCheck()) {
            materialEditor.RegisterPropertyChangeUndo("Rendering Mode");
            blendMode.floatValue = (float)mode;
        }
        EditorGUI.showMixedValue = false;

        EditorGUI.BeginChangeCheck();
        if (mode != BlendMode.Opaque) {
            materialEditor.TexturePropertySingleLine(Styles.alphaTexText, alphaTex);
        }
        if (mode == BlendMode.Cutout) {
            materialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText);
        }
        if (EditorGUI.EndChangeCheck()) {
            foreach (var obj in blendMode.targets) {
                MaterialChanged((Material)obj);
            }
        }
    }

    public virtual void FindProperties(MaterialProperty[] props) {
        blendMode = FindProperty("_Mode", props, false);
        alphaTex = FindProperty("_AlphaTex", props, false);
        alphaCutoff = FindProperty("_Cutoff", props, false);
    }

    public virtual void MaterialChanged(Material material) {
        var blendMode = (BlendMode)material.GetFloat("_Mode");
        SetupBlendMode(material, blendMode);
    }

    protected void SetKeyword(Material m, string keyword, bool state) {
        if (state) {
            m.EnableKeyword(keyword);
        }
        else {
            m.DisableKeyword(keyword);
        }
    }

    void SetupBlendMode(Material material, BlendMode blendMode) {
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
}
