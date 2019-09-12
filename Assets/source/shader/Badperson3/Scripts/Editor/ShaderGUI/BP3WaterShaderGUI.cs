using UnityEngine;
using UnityEditor;
using System;

public class BP3WaterShaderGUI : ShaderGUI {
    static class Styles {
        internal static readonly string waterMode = "Water Mode";
        internal static readonly string[] waterModeNames = Enum.GetNames(typeof(BP3WaterMode));
        internal static readonly string waveScale = "Wave Scale";
        internal static readonly string waveSpeed = "Wave Speed";
        internal static readonly GUIContent reflectiveTex = new GUIContent("Reflective (A)");
        internal static readonly GUIContent bumpMap = new GUIContent("Normal Map");
        internal static readonly GUIContent reflectionTex = new GUIContent("Reflection Tex");
    }

    MaterialProperty waterMode;
    MaterialProperty waveScale;
    MaterialProperty waveSpeed;
    MaterialProperty reflectiveTex;
    MaterialProperty reflectiveColor;
    MaterialProperty bumpMap;
    MaterialProperty bumpScale;
    MaterialProperty reflectionTex;
    MaterialProperty reflectionScale;
    bool firstTimeApply = true;

    void FindProperties(MaterialProperty[] properties) {
        waterMode = FindProperty("_WaterMode", properties);
        waveScale = FindProperty("_WaveScale", properties);
        waveSpeed = FindProperty("_WaveSpeed", properties);
        reflectiveTex = FindProperty("_ReflectiveTex", properties);
        reflectiveColor = FindProperty("_ReflectiveColor", properties);
        bumpMap = FindProperty("_BumpMap", properties);
        bumpScale = FindProperty("_BumpScale", properties); 
        reflectionTex = FindProperty("_ReflectionTex", properties);
        reflectionScale = FindProperty("_ReflectionScale", properties);
    }


    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        FindProperties(properties);

        var material = materialEditor.target as Material;
        if (firstTimeApply) {
            MaterialChanged(material);
            firstTimeApply = false;
        }

        ShaderPropertiesGUI(materialEditor);
    }

    void ShaderPropertiesGUI(MaterialEditor materialEditor) {
        EditorGUIUtility.labelWidth = 0.0f;

        EditorGUI.BeginChangeCheck();
        {
            var mode = (BP3WaterMode)waterMode.floatValue;

            EditorGUI.showMixedValue = waterMode.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            mode = (BP3WaterMode)EditorGUILayout.Popup(Styles.waterMode, (int)mode, Styles.waterModeNames);
            if (EditorGUI.EndChangeCheck()) {
                materialEditor.RegisterPropertyChangeUndo("Water Mode");
                waterMode.floatValue = (float)mode;

                for (var i = 0; i < Selection.transforms.Length; ++i) {
                    var ws = Selection.transforms[i].GetComponentsInChildren<BP3Water>();
                    for (var j = 0; j < ws.Length; ++j) {
                        ws[j].enabled = (mode == BP3WaterMode.RealTimeReflective);
                    }
                }
            }
            EditorGUI.showMixedValue = false;

            EditorGUILayout.Space();
            materialEditor.ShaderProperty(waveScale, Styles.waveScale);
            materialEditor.ShaderProperty(waveSpeed, Styles.waveSpeed);

            EditorGUILayout.Space();
            materialEditor.TexturePropertySingleLine(Styles.reflectiveTex, reflectiveTex, reflectiveColor);
            materialEditor.TextureScaleOffsetProperty(reflectiveTex);
            materialEditor.TexturePropertySingleLine(Styles.bumpMap, bumpMap, bumpScale);

            if (mode >= BP3WaterMode.Reflective) {
                EditorGUILayout.Space();
                materialEditor.TexturePropertySingleLine(Styles.reflectionTex, reflectionTex, reflectionScale);
            }
        }
        if (EditorGUI.EndChangeCheck()) {
            for (var i = 0; i < waterMode.targets.Length; ++i) {
                var mat = (Material)waterMode.targets[i];
                MaterialChanged(mat);
            }
        }
    }

    void MaterialChanged(Material material) {
        var mode = (BP3WaterMode)waterMode.floatValue;
        if (mode == BP3WaterMode.Simple) {
            SetKeyword(material, "BP3_WATER_REFLECTIVE_ON", false);
            SetKeyword(material, "BP3_WATER_REALTIME_REFLECTIVE_ON", false);
        }
        else if (mode == BP3WaterMode.Reflective) {
            SetKeyword(material, "BP3_WATER_REFLECTIVE_ON", true);
            SetKeyword(material, "BP3_WATER_REALTIME_REFLECTIVE_ON", false);
        }
        else if (mode == BP3WaterMode.RealTimeReflective) {
            SetKeyword(material, "BP3_WATER_REFLECTIVE_ON", false);
            SetKeyword(material, "BP3_WATER_REALTIME_REFLECTIVE_ON", true);
        }
    }

    void BumpMapWasChanged() {
        if (bumpMap.textureValue) {
            var tex = AssetDatabase.GetAssetPath(bumpMap.textureValue);
            var importer = AssetImporter.GetAtPath(tex) as TextureImporter;
            importer.textureType = TextureImporterType.NormalMap;
            AssetDatabase.ImportAsset(tex, ImportAssetOptions.ForceUpdate);
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