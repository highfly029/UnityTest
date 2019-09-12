using UnityEngine;
using UnityEditor;
using System;

public class BP3ShaderGUIUtility {
    public class Styles {
        public static readonly string blendMode = "Rendering Mode";
        public static readonly string[] blendNames = System.Enum.GetNames(typeof(BP3ShaderUtility.BlendMode));
        public static readonly string primaryProps = "Primary";
        public static readonly GUIContent advancedProps = new GUIContent("Advanced");
        public static readonly GUIContent main = new GUIContent("Base", "Base (RGB)");
        public static readonly GUIContent alpha = new GUIContent("Alpha", "Transparency");
        public static readonly GUIContent alphaCutoff = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");
        public static readonly GUIContent normal = new GUIContent("Normal", "Normal (OpenGL)");
        public static readonly GUIContent emission = new GUIContent("Emission", "Emission (RGB)");
        public static readonly GUIContent emissiveWarning = new GUIContent("Emissive value is animated but the material has not been configured to support emissive. Please make sure the material itself has some amount of emissive.");
        public static readonly GUIContent highlights = new GUIContent("Specular Highlights", "Specular Highlights");
        public static readonly GUIContent reflections = new GUIContent("Reflections", "Glossy Reflections");
        public static readonly GUIContent specColor = new GUIContent("Specular Color", "Specular Color");
        public static readonly GUIContent specShine = new GUIContent("Specular Shines", "Specular Shines");
        public static readonly GUIContent specGlossness = new GUIContent("Specular Glossness", "Specular Glossness");
        public static readonly GUIContent doubleSided = new GUIContent("Double Sided", "Double Sided Rendering");
        public static readonly GUIContent doubleSidedLighting = new GUIContent("Double Sided Lighting", "Double Sided Lighting");
    }

    public static void OnBlendModeGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        var blendMode = FindProperty(BP3ShaderUtility.blendModePID, properties);
        EditorGUI.BeginChangeCheck();
        blendMode.floatValue = EditorGUILayout.Popup(Styles.blendMode, (int)blendMode.floatValue, Styles.blendNames);
        if (EditorGUI.EndChangeCheck()) {
            materialEditor.RegisterPropertyChangeUndo("BP3 Blend Mode");
            for (var i = 0; i < blendMode.targets.Length; ++i) {
                var mat = (Material)blendMode.targets[i];
                SetupBlendMode(mat);
            }
        }
    }

    public static void SetupBlendMode(Material material) {
        var blendMode = (BP3ShaderUtility.BlendMode)material.GetFloat(BP3ShaderUtility.blendModePID);
        switch (blendMode) {
        case BP3ShaderUtility.BlendMode.Opaque:
            material.renderQueue = -1;
            material.SetOverrideTag("RenderType", "");
            material.SetInt(BP3ShaderUtility.srcBlendPID, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(BP3ShaderUtility.dstBlendPID, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt(BP3ShaderUtility.zwritePID, 1);
            material.DisableKeyword(BP3ShaderUtility.alphaTestKID);
            material.DisableKeyword(BP3ShaderUtility.alphaBlendKID);
            material.DisableKeyword(BP3ShaderUtility.alphaPremultiplyKID);
            break;
        case BP3ShaderUtility.BlendMode.Cutout:
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            material.SetOverrideTag("RenderType", "TransparentCutout");
            material.SetInt(BP3ShaderUtility.srcBlendPID, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(BP3ShaderUtility.dstBlendPID, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt(BP3ShaderUtility.zwritePID, 1);
            material.EnableKeyword(BP3ShaderUtility.alphaTestKID);
            material.DisableKeyword(BP3ShaderUtility.alphaBlendKID);
            material.DisableKeyword(BP3ShaderUtility.alphaPremultiplyKID);
            break;
        case BP3ShaderUtility.BlendMode.Fade:
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt(BP3ShaderUtility.srcBlendPID, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt(BP3ShaderUtility.dstBlendPID, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt(BP3ShaderUtility.zwritePID, 0);
            material.DisableKeyword(BP3ShaderUtility.alphaTestKID);
            material.EnableKeyword(BP3ShaderUtility.alphaBlendKID);
            material.DisableKeyword(BP3ShaderUtility.alphaPremultiplyKID);
            break;
        }
    }

    public static void OnAlbedoGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        var blendMode = FindProperty(BP3ShaderUtility.blendModePID, properties);
        var color = FindProperty(BP3ShaderUtility.colorPID, properties);
        var mainTex = FindProperty(BP3ShaderUtility.mainTexPID, properties);
        var alphaTex = FindProperty(BP3ShaderUtility.alphaTexPID, properties);
        var alphaCutoff = FindProperty(BP3ShaderUtility.alphaCutoffPID, properties);

        materialEditor.TexturePropertySingleLine(Styles.main, mainTex, color);
        materialEditor.TextureScaleOffsetProperty(mainTex);

        if (blendMode.floatValue > (int)BP3ShaderUtility.BlendMode.Opaque) {
            materialEditor.TexturePropertySingleLine(Styles.alpha, alphaTex);
            if (blendMode.floatValue == (int)BP3ShaderUtility.BlendMode.Cutout) {
                materialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoff, 2);
            }
        }
    }

    public static void OnNormalMapGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        var bumpMap = FindProperty(BP3ShaderUtility.normalMapPID, properties);
        var bumpMapScale = FindProperty(BP3ShaderUtility.normalScalePID, properties, false);

        EditorGUI.BeginChangeCheck();
        materialEditor.TexturePropertySingleLine(Styles.normal, bumpMap, (bumpMapScale != null && bumpMap.textureValue) ? bumpMapScale : null);
        if (EditorGUI.EndChangeCheck()) {
            for (var i = 0; i < bumpMap.targets.Length; ++i) {
                var mat = (Material)bumpMap.targets[i];
                SetKeyword(mat, BP3ShaderUtility.normalMapKID, bumpMap.textureValue);
            }
        }
    }

    public static void OnHighLightsGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        var highLights = FindProperty(BP3ShaderUtility.highlightsPID, properties);
        var specColor = FindProperty(BP3ShaderUtility.specColorPID, properties);
        var specShine = FindProperty(BP3ShaderUtility.specShinePID, properties);
        var specGlossness = FindProperty(BP3ShaderUtility.specGlossnessPID, properties);

        EditorGUI.BeginChangeCheck();
        materialEditor.ShaderProperty(highLights, Styles.highlights);
        if (highLights.floatValue != 0) {
            materialEditor.ShaderProperty(specColor, Styles.specColor, 1);
            materialEditor.ShaderProperty(specShine, Styles.specShine, 1);
            materialEditor.ShaderProperty(specGlossness, Styles.specGlossness, 1);
        }
        if (EditorGUI.EndChangeCheck()) {
            for (var i = 0; i < highLights.targets.Length; ++i) {
                var mat = (Material)highLights.targets[i];
                SetKeyword(mat, BP3ShaderUtility.highlightsOffKID, highLights.floatValue == 0);
            }
        }
    }

    public static void OnDoubleSidedGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        var doubleSided = FindProperty(BP3ShaderUtility.doubleSidedPID, properties);
        var doubleSidedLighting = FindProperty(BP3ShaderUtility.doubleSidedLightingPID, properties);

        EditorGUI.BeginChangeCheck();
        materialEditor.ShaderProperty(doubleSidedLighting, Styles.doubleSidedLighting);
        if (doubleSidedLighting.floatValue == 0) {
            materialEditor.ShaderProperty(doubleSided, Styles.doubleSided);
        }
        if (EditorGUI.EndChangeCheck()) {
            for (var i = 0; i < doubleSided.targets.Length; ++i) {
                var mat = (Material)doubleSided.targets[i];
                SetKeyword(mat, BP3ShaderUtility.doubleSidedLightingKID, doubleSidedLighting.floatValue != 0);
                if (doubleSidedLighting.floatValue != 0) {
                    doubleSided.floatValue = 1;
                }

                if (doubleSided.floatValue != 0 || doubleSidedLighting.floatValue != 0) {
                    mat.SetInt(BP3ShaderUtility.cullingModePID, (int)UnityEngine.Rendering.CullMode.Off);
                }
                else {
                    mat.SetInt(BP3ShaderUtility.cullingModePID, (int)UnityEngine.Rendering.CullMode.Back);
                }
            }
        }
    }

    public static void SetKeyword(Material material, string keyword, bool state) {
        if (state) {
            material.EnableKeyword(keyword);
        }
        else {
            material.DisableKeyword(keyword);
        }
    }

    public static MaterialProperty FindProperty(string propertyName, MaterialProperty[] properties, bool propertyIsMandatory = true) {
        MaterialProperty result;
        for (int i = 0; i < properties.Length; i++) {
            if (properties[i] != null && properties[i].name == propertyName) {
                result = properties[i];
                return result;
            }
        }
        if (propertyIsMandatory) {
            throw new ArgumentException(string.Concat(new object[] { "Could not find MaterialProperty: '", propertyName, "', Num properties: ", properties.Length }));
        }
        result = null;
        return result;
    }
}
