using UnityEngine;
using UnityEditor;

public class BP3MeshTerrainShaderGUI : ShaderGUI {
    static class Styles {
        internal static readonly string primaryProps = "Primary";
        internal static readonly GUIContent splat0 = new GUIContent("Layer 1");
        internal static readonly GUIContent splat1 = new GUIContent("Layer 2");
        internal static readonly GUIContent splat2 = new GUIContent("Layer 3");
        internal static readonly GUIContent splat3 = new GUIContent("Layer 4");

        internal static readonly GUIContent bumpSplat0 = new GUIContent("Layer 1 Normal");
        internal static readonly GUIContent bumpSplat1 = new GUIContent("Layer 2 Normal");
        internal static readonly GUIContent bumpSplat2 = new GUIContent("Layer 3 Normal");
        internal static readonly GUIContent bumpSplat3 = new GUIContent("Layer 4 Normal");

        internal static readonly GUIContent shininess0 = new GUIContent("Layer 1 Shininess");
        internal static readonly GUIContent shininess1 = new GUIContent("Layer 2 Shininess");
        internal static readonly GUIContent shininess2 = new GUIContent("Layer 3 Shininess");
        internal static readonly GUIContent shininess3 = new GUIContent("Layer 4 Shininess");

        internal static readonly GUIContent control = new GUIContent("Control");


        internal static readonly GUIContent specular = new GUIContent("Specular");
        internal static readonly GUIContent specColor = new GUIContent("Specular Color");
        internal static readonly GUIContent specPower = new GUIContent("Specular Power");

        internal static readonly GUIContent advancedProps = new GUIContent("Advanced");
    }

    const int length = 4;
    MaterialProperty[] splat = new MaterialProperty[length];
    MaterialProperty[] bumpSplat = new MaterialProperty[length];
    MaterialProperty[] bumpScale = new MaterialProperty[length];
    MaterialProperty[] shininess = new MaterialProperty[length];
    MaterialProperty control;
    MaterialProperty specular;
    MaterialProperty specColor;
    MaterialProperty specPower;
    bool firstTimeApply = true;

    void FindProperties(MaterialProperty[] properties) {
        for (var i = 0; i < length; ++i) {
            splat[i] = FindProperty("_Splat" + i, properties);
            bumpSplat[i] = FindProperty("_BumpSplat" + i, properties);
            bumpScale[i] = FindProperty("_BumpScale" + i, properties);
            shininess[i] = FindProperty("_ShininessL" + i, properties);
        }

        control = FindProperty("_Control", properties);
        specular = FindProperty("_Specular", properties);
        specColor = FindProperty("_SpecColor", properties);
        specPower = FindProperty("_SpecPower", properties);
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
        EditorGUI.BeginChangeCheck();
        {
            GUILayout.Label(Styles.primaryProps, EditorStyles.boldLabel);
            materialEditor.TexturePropertySingleLine(Styles.control, control);
            EditorGUILayout.Space();

            materialEditor.TexturePropertySingleLine(Styles.splat0, splat[0]);
            materialEditor.TextureScaleOffsetProperty(splat[0]);
            materialEditor.TexturePropertySingleLine(Styles.splat1, splat[1]);
            materialEditor.TextureScaleOffsetProperty(splat[1]);
            materialEditor.TexturePropertySingleLine(Styles.splat2, splat[2]);
            materialEditor.TextureScaleOffsetProperty(splat[2]);
            materialEditor.TexturePropertySingleLine(Styles.splat3, splat[3]);
            materialEditor.TextureScaleOffsetProperty(splat[3]);
            EditorGUILayout.Space();

            materialEditor.TexturePropertySingleLine(Styles.bumpSplat0, bumpSplat[0], bumpSplat[0].textureValue ? bumpScale[0] : null);
            materialEditor.TexturePropertySingleLine(Styles.bumpSplat1, bumpSplat[1], bumpSplat[1].textureValue ? bumpScale[1] : null);
            materialEditor.TexturePropertySingleLine(Styles.bumpSplat2, bumpSplat[2], bumpSplat[2].textureValue ? bumpScale[2] : null);
            materialEditor.TexturePropertySingleLine(Styles.bumpSplat3, bumpSplat[3], bumpSplat[3].textureValue ? bumpScale[3] : null);
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(specular, Styles.specular);
            if (specular.floatValue != 0) {
                materialEditor.ShaderProperty(shininess[0], Styles.shininess0, 1);
                materialEditor.ShaderProperty(shininess[1], Styles.shininess1, 1);
                materialEditor.ShaderProperty(shininess[2], Styles.shininess2, 1);
                materialEditor.ShaderProperty(shininess[3], Styles.shininess3, 1);

                materialEditor.ShaderProperty(specColor, Styles.specColor, 1);
                materialEditor.ShaderProperty(specPower, Styles.specPower, 1);
                EditorGUILayout.Space();
            }

        }
        if (EditorGUI.EndChangeCheck()) {
            for (var i = 0; i < materialEditor.targets.Length; ++i) {
                MaterialChanged((Material)materialEditor.targets[i]);
            }
        }

        GUILayout.Label(Styles.advancedProps, EditorStyles.boldLabel);
        materialEditor.EnableInstancingField();
        materialEditor.DoubleSidedGIField();
    }

    void MaterialChanged(Material material) {
        SetKeyword(material, Optimize.RenderParams.SpecularKeyword, material.GetFloat("_Specular") != 0.0);
        for (var i = 0; i < length; ++i) {
            SetKeyword(material, Optimize.RenderParams.MeshTerrainSplatMap[i], material.GetTexture("_Splat" + i));
            SetKeyword(material, Optimize.RenderParams.MeshTerrainNormalMap[i], material.GetTexture("_BumpSplat" + i));
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
