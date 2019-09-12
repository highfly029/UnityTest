using UnityEngine;
using UnityEditor;
using System;

public class BP3CharacterShaderGUI : ShaderGUI {
    enum BlendMode { Body, Hair }
    enum CullMode { Off, Back } // In fact, the back culling is 2
    const int cullingOff = 0;
    const int cullingBack = 2;

    MaterialEditor materialEditor;
    MaterialProperty blendMode;
    MaterialProperty alphaTex;
    MaterialProperty color;
    MaterialProperty mainTex;
    MaterialProperty glossScale;
    MaterialProperty maskTex;
    MaterialProperty bumpScale;
    MaterialProperty bumpMap;
    MaterialProperty reflecRadio;
    MaterialProperty cubeMap;
    MaterialProperty cubeMapRotation;
    MaterialProperty specular;
    MaterialProperty specColor;
    MaterialProperty specPower;
    MaterialProperty rim;
    MaterialProperty rimColor;
    MaterialProperty rimMap;
    MaterialProperty rimRange;
    MaterialProperty rimPower;
    MaterialProperty hitGlow;
    MaterialProperty hitGlowColor;
    MaterialProperty hitGlowPower;
    MaterialProperty cullMode;

    MaterialProperty hasMask;
    MaterialProperty maskRect;

    bool firstTimeApply = true;

    static class Styles {
        internal static readonly string primaryProps = "Primary";
        internal static readonly string blendMode = "Blend Mode";
        internal static readonly string[] blendModeNames = Enum.GetNames(typeof(BlendMode));
        internal static readonly GUIContent mainTex = new GUIContent("Base(RGB)");
        internal static readonly GUIContent alphaTex = new GUIContent("Alpha(A)");
        internal static readonly GUIContent maskTex = new GUIContent("Gloss(R), Reflective(G)");
        internal static readonly GUIContent bumpMap = new GUIContent("Normal Map");
        internal static readonly GUIContent cubeMap = new GUIContent("Cube Map");
        internal static readonly GUIContent cubeMapRotation = new GUIContent("Cube Map Rotation");
        internal static readonly string specular = "Specular";
        internal static readonly string specColor = "Specular Color";
        internal static readonly string specPower = "Specular Power";
        internal static readonly string rim = "Rim Lighting";
        internal static readonly GUIContent rimMap = new GUIContent("Rim Tex");
        internal static readonly string rimRange = "Rim Range";
        internal static readonly string rimPower = "Rim Power";
        internal static readonly string hitGlow  = "Hit Glow";
        internal static readonly string hitGlowColor = "Hit Color";
        internal static readonly string hitGlowPower = "Hit Power";

        internal static readonly GUIContent advancedProps = new GUIContent("Advanced");
        internal static readonly string cullMode = "Cull Mode";
        internal static readonly string[] cullModeNames = Enum.GetNames(typeof(CullMode));

     //   internal static readonly string mask = "mask";
    }

    void FindProperties(MaterialProperty[] properties) {
        blendMode = FindProperty("_Mode", properties, false);
        alphaTex = FindProperty("_AlphaTex", properties, false);
        color = FindProperty("_Color", properties);
        mainTex = FindProperty("_MainTex", properties);
        glossScale = FindProperty("_GlossScale", properties);
        maskTex = FindProperty("_MaskTex", properties);
        bumpScale = FindProperty("_BumpScale", properties);
        bumpMap = FindProperty("_BumpMap", properties);
        cubeMapRotation = FindProperty("_CubeMapRotation", properties);
        reflecRadio = FindProperty("_ReflecRadio", properties);
        cubeMap = FindProperty("_CubeMap", properties);
        specular = FindProperty("_Specular", properties);
        specColor = FindProperty("_SpecColor", properties);
        specPower = FindProperty("_SpecPower", properties);
        rim = FindProperty("_Rim", properties);
        rimColor = FindProperty("_RimColor", properties);
        rimMap = FindProperty("_RimMap", properties);
        rimRange = FindProperty("_RimRange", properties);
        rimPower = FindProperty("_RimPower", properties);
        hitGlow = FindProperty("_HitGlow", properties);
        hitGlowColor = FindProperty("_HitGlowColor", properties);
        hitGlowPower = FindProperty("_HitGlowPower", properties);
        cullMode = FindProperty("_CullMode", properties);

        hasMask = FindProperty("_HasMask", properties);
        maskRect = FindProperty("_MaskRect", properties);
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader) {
        base.AssignNewShaderToMaterial(material, oldShader, newShader);

        material.SetFloat("_Mode", (float)BlendMode.Body);
        material.SetFloat("_CullMode", cullingBack);
        SetupBlendMode(material);

        var renderers = Optimize.PropertiesModifier.FindRendererWithMaterial<SkinnedMeshRenderer>(material);
        for (var i = 0; i < renderers.Length; ++i) {
            var r = renderers[i];
            Optimize.PropertiesModifier.ModifyRendererProperties(r);
        }
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        FindProperties(properties);
        var material = materialEditor.target as Material;

        if (firstTimeApply) {
            MaterialChanged(material);
            firstTimeApply = false;
        }

        ShaderPropertiesGUI(materialEditor, material);
    }

    void ShaderPropertiesGUI(MaterialEditor materialEditor, Material material) {
        EditorGUIUtility.labelWidth = 0f;

        BlendModePopup(materialEditor, material);
        EditorGUILayout.Space();

        var mode = (BlendMode)blendMode.floatValue;

        EditorGUI.BeginChangeCheck();
        {
            GUILayout.Label(Styles.primaryProps, EditorStyles.boldLabel);

            materialEditor.TexturePropertySingleLine(Styles.mainTex, mainTex, color);
            if (mode == BlendMode.Hair) {
                materialEditor.TexturePropertySingleLine(Styles.alphaTex, alphaTex);
            }
            EditorGUILayout.Space();

            if (specular.floatValue != 0 || cubeMap.textureValue) {
                materialEditor.TexturePropertySingleLine(Styles.maskTex, maskTex, maskTex.textureValue ? glossScale : null);
            }
            materialEditor.TexturePropertySingleLine(Styles.bumpMap, bumpMap, bumpMap.textureValue ? bumpScale : null);
            materialEditor.TexturePropertySingleLine(Styles.cubeMap, cubeMap, cubeMap.textureValue ? reflecRadio : null);
            if (cubeMap.textureValue) {
                materialEditor.ShaderProperty(cubeMapRotation, Styles.cubeMapRotation, 1);
            }
            materialEditor.TextureScaleOffsetProperty(mainTex);
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(specular, Styles.specular);
            if (specular.floatValue != 0) {
                materialEditor.ShaderProperty(specColor, Styles.specColor, 1);
                materialEditor.ShaderProperty(specPower, Styles.specPower, 1);
                EditorGUILayout.Space();
            }

            materialEditor.ShaderProperty(rim, Styles.rim);
            if (rim.floatValue != 0) {
                materialEditor.TexturePropertySingleLine(Styles.rimMap, rimMap, rimColor);
                materialEditor.ShaderProperty(rimRange, Styles.rimRange, 1);
                materialEditor.ShaderProperty(rimPower, Styles.rimPower, 1);
                EditorGUILayout.Space();
            }

           
            materialEditor.ShaderProperty(hitGlow, Styles.hitGlow);

            if (hitGlow.floatValue != 0.0) {
                materialEditor.ShaderProperty(hitGlowColor, Styles.hitGlowColor, 1);
                materialEditor.ShaderProperty(hitGlowPower, Styles.hitGlowPower, 1);
                EditorGUILayout.Space();
            }

            GUILayout.Label(Styles.advancedProps, EditorStyles.boldLabel);
            CullModePopup(materialEditor, material);
           
        }
        if (EditorGUI.EndChangeCheck()) {
            for (var i = 0; i < blendMode.targets.Length; ++i) {
                MaterialChanged((Material)blendMode.targets[i]);
            }
        }
        EditorGUILayout.Space();
        materialEditor.ShaderProperty(hasMask, "hasMask");
        if (hasMask.floatValue != 0.0)
        {
            materialEditor.VectorProperty(maskRect, "maskRect");
            material.EnableKeyword(Optimize.RenderParams.Shader_Keyword_Mask);
        }else
        {
            material.DisableKeyword(Optimize.RenderParams.Shader_Keyword_Mask);
        }
        materialEditor.EnableInstancingField();
    }

    void BlendModePopup(MaterialEditor materialEditor, Material material) {
        EditorGUI.showMixedValue = blendMode.hasMixedValue;
        var mode = (BlendMode)blendMode.floatValue;

        EditorGUI.BeginChangeCheck();
        mode = (BlendMode)EditorGUILayout.Popup(Styles.blendMode, (int)mode, Styles.blendModeNames);
        if (EditorGUI.EndChangeCheck()) {
            materialEditor.RegisterPropertyChangeUndo("Character Blend Mode");
            blendMode.floatValue = (float)mode;
            SetupBlendMode(material);
        }
        EditorGUI.showMixedValue = false;
    }

    void CullModePopup(MaterialEditor materialEditor, Material material) {
        EditorGUI.showMixedValue = cullMode.hasMixedValue;
        var mode = (int)cullMode.floatValue;
        if (mode == cullingBack) { mode = (int)CullMode.Back; }

        EditorGUI.BeginChangeCheck();
        mode = EditorGUILayout.Popup(Styles.cullMode, mode, Styles.cullModeNames);

        if (mode == (int)CullMode.Back) { mode = cullingBack; }
        if (EditorGUI.EndChangeCheck()) {
            materialEditor.RegisterPropertyChangeUndo("Character Cull Mode");
            cullMode.floatValue = mode;
        }
        EditorGUI.showMixedValue = false;
    }

    void MaterialChanged(Material material) {
        SetupBlendMode(material);

        SetKeyword(material, Optimize.RenderParams.NormalKeyword, material.GetTexture("_BumpMap"));
        SetKeyword(material, Optimize.RenderParams.CubeMapKeyword, material.GetTexture("_CubeMap"));
        SetKeyword(material, Optimize.RenderParams.SpecularKeyword, specular.floatValue != 0.0);
        SetKeyword(material, Optimize.RenderParams.CharacterRimLightKeyword, rim.floatValue != 0.0);
        SetKeyword(material, Optimize.RenderParams.CharacterHitGlowKeyword, hitGlow.floatValue != 0.0);
        SetKeyword(material, Optimize.RenderParams.Shader_Keyword_Mask, hasMask.floatValue != 0.0);
    }

    void SetupBlendMode(Material material) {
        var mode = (BlendMode)material.GetFloat("_Mode");
        switch (mode) {
        case BlendMode.Hair:
            material.renderQueue = Optimize.RenderParams.CharacterHairQueue;
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword(Optimize.RenderParams.CharacterHairKeyword);
            break;
        case BlendMode.Body:
            material.renderQueue = Optimize.RenderParams.CharacterBodyQueue;
                //     material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                //     material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.DisableKeyword(Optimize.RenderParams.CharacterHairKeyword);
            break;
        }
    }

    void SetKeyword(Material material, string keyword, bool state) {
        if (state) {
            material.EnableKeyword(keyword);
        }
        else {
            material.DisableKeyword(keyword);
        }
    }
}
