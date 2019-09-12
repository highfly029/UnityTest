using UnityEngine;
using UnityEditor;

public class CharacterBPShaderGUI : BaseShaderGUI {
    MaterialProperty color = null;
    MaterialProperty mainTex = null;
    MaterialProperty maskTex = null;
    MaterialProperty normalTex = null;
    MaterialProperty cubeTex = null;
    MaterialProperty reflectScale = null;
    MaterialProperty incidentIndensity = null;
    MaterialProperty specularColor = null;
    MaterialProperty specularPower = null;
    MaterialProperty specularScale = null;
    MaterialProperty rim = null;
    MaterialProperty rimColor = null;
    MaterialProperty rimTex = null;
    MaterialProperty rimPower = null;
    MaterialProperty rimRange = null;
    MaterialProperty hitGlow = null;
    MaterialProperty hitGlowColor = null;
    MaterialProperty hitGlowPower = null;
    MaterialProperty xrayTex = null;

    MaterialProperty highlights = null;
    MaterialProperty reflections = null;

    static class Styles {
        public static string primaryText = "Primary Options";
        public static string advancedText = "Advanced Options";

        public static GUIContent mainTexText = new GUIContent("Base (RGB)", "Base (RGB)");
        public static GUIContent maskTexText = new GUIContent("Mask (RG)", "Gloss(R) Reflect(G)");
        public static GUIContent normalTexText = new GUIContent("Normal Map", "Normal Map");
        public static GUIContent cubeTexText = new GUIContent("Reflect Cubemap", "Reflect Cubemap");
        public static GUIContent incidentIndensityText = new GUIContent("Light indensity", "Light incident indensity");
        public static GUIContent incidentIndensityDiffuseText = new GUIContent("Light indensity 2", "Light incident indensity");
        public static GUIContent specularColorText = new GUIContent("Specular Color", "Specular Color");
        public static GUIContent specularPowerText = new GUIContent("Specular Power", "Specular Power");
        public static GUIContent specularScaleText = new GUIContent("Specular Scale", "Specular Scale");
        public static GUIContent rimText = new GUIContent("Rim Light", "Rim Light");
        public static GUIContent rimColorText = new GUIContent("Rim Color", "Rim Color");
        public static GUIContent rimTexText = new GUIContent("Rim Texture", "Rim Texture");
        public static GUIContent rimPowerText = new GUIContent("Rim Power", "Rim Power");
        public static GUIContent rimRangeText = new GUIContent("Rim Range", "Rim Range");
        public static GUIContent hitGlowText = new GUIContent("Hit Glow", "Hit Glow");
        public static GUIContent hitGlowColorText = new GUIContent("Hit Glow Color", "Hit Glow Color");
        public static GUIContent hitGlowPowerText = new GUIContent("Hit Glow Power", "Hit Glow Power");
        public static GUIContent xrayTexText = new GUIContent("X-Ray", "X-Ray Texture");

        public static GUIContent highlightsText = new GUIContent("Specular Highlight", "Specular Highlight");
        public static GUIContent reflectionsText = new GUIContent("Gloss Reflection", "Gloss Reflection");
    }

    public override void FindProperties(MaterialProperty[] props) {
        color = FindProperty("_Color", props);
        mainTex = FindProperty("_MainTex", props);
        maskTex = FindProperty("_MaskTex", props);
        normalTex = FindProperty("_NormalTex", props);
        cubeTex = FindProperty("_CubeTex", props);
        reflectScale = FindProperty("_ReflectScale", props);
        incidentIndensity = FindProperty("_Indensity", props);
        specularColor = FindProperty("_SpecColor", props);
        specularPower = FindProperty("_SpecPower", props);
        specularScale = FindProperty("_SpecScale", props);
        rim = FindProperty("_Rim", props);
        rimColor = FindProperty("_RimColor", props);
        rimTex = FindProperty("_RimTex", props);
        rimPower = FindProperty("_RimPower", props);
        rimRange = FindProperty("_RimRange", props);
        hitGlow = FindProperty("_HitGlow", props);
        hitGlowColor = FindProperty("_HitGlowColor", props);
        hitGlowPower = FindProperty("_HitGlowPower", props);
        xrayTex = FindProperty("_XrayTex", props, false);
        highlights = FindProperty("_Highlights", props);
        reflections = FindProperty("_Reflections", props);

        base.FindProperties(props);
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader) {
        base.AssignNewShaderToMaterial(material, oldShader, newShader);

        var rens = Optimize.PropertiesModifier.FindRendererWithMaterial<Renderer>(material);
        for (var i = 0; i < rens.Length; ++i) {
            var r = rens[i];
            Optimize.PropertiesModifier.ModifyRendererProperties(r);
        }
    }

    public override void MaterialChanged(Material material) {
        // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
        // (MaterialProperty value might come from renderer material property block)
        SetKeyword(material, "_NORMALMAP", material.GetTexture("_NormalTex"));
        SetKeyword(material, "_REFLECT_CUBEMAP", material.GetTexture("_CubeTex"));
        SetKeyword(material, "_RIM_LIGHT", rim.floatValue != 0.0);
        SetKeyword(material, "_HIT_GLOW", hitGlow.floatValue != 0.0);
        SetKeyword(material, "_SPEC_HIGHLIGHTS_OFF", highlights.floatValue != 1.0);
        SetKeyword(material, "_GLOSS_REFLECTIONS_OFF", reflections.floatValue != 1.0);

        base.MaterialChanged(material);
    }

    public override void ShaderPropertiesGUI(Material material) {
        EditorGUIUtility.labelWidth = 0f;
        EditorGUI.BeginChangeCheck();
        {
            base.ShaderPropertiesGUI(material);

            GUILayout.Label(Styles.primaryText, EditorStyles.boldLabel);
            materialEditor.TexturePropertySingleLine(Styles.mainTexText, mainTex, color);
            materialEditor.TexturePropertySingleLine(Styles.maskTexText, maskTex);
            materialEditor.TexturePropertySingleLine(Styles.normalTexText, normalTex);
            var hasCubeTex = cubeTex.textureValue != null;
            materialEditor.TexturePropertySingleLine(Styles.cubeTexText, cubeTex, hasCubeTex ? reflectScale : null);

            materialEditor.TextureScaleOffsetProperty(mainTex);

            materialEditor.ShaderProperty(incidentIndensity, Styles.incidentIndensityText);
            materialEditor.ShaderProperty(specularColor, Styles.specularColorText);
            materialEditor.ShaderProperty(specularPower, Styles.specularPowerText);
            materialEditor.ShaderProperty(specularScale, Styles.specularScaleText);

            materialEditor.ShaderProperty(rim, Styles.rimText);
            if (rim.floatValue != 0.0) {
                materialEditor.TexturePropertySingleLine(Styles.rimText, rimTex, rimColor);
                materialEditor.ShaderProperty(rimPower, Styles.rimPowerText, 2);
                materialEditor.ShaderProperty(rimRange, Styles.rimRangeText, 2);
            }

            materialEditor.ShaderProperty(hitGlow, Styles.hitGlowText);
            if (hitGlow.floatValue != 0.0) {
                materialEditor.ShaderProperty(hitGlowColor, Styles.hitGlowColorText, 2);
                materialEditor.ShaderProperty(hitGlowPower, Styles.hitGlowPowerText, 2);
            }

            GUILayout.Label(Styles.advancedText, EditorStyles.boldLabel);
            if (xrayTex != null) {
                materialEditor.TexturePropertySingleLine(Styles.xrayTexText, xrayTex);
            }
            materialEditor.ShaderProperty(highlights, Styles.highlightsText);
            materialEditor.ShaderProperty(reflections, Styles.reflectionsText);
        }
        if (EditorGUI.EndChangeCheck()) {
            foreach (var obj in blendMode.targets) {
                MaterialChanged((Material)obj);
            }
        }
    }
}
