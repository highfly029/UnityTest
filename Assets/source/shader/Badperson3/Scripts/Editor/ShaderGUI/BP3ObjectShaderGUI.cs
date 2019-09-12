using UnityEngine;
using UnityEditor;
using System;

public class BP3ObjectShaderGUI : ShaderGUI {
    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader) {
        //if (material.HasProperty("_Emission")) {
        //    material.SetColor("_EmissionColor", material.GetColor("_Emission"));
        //}

        base.AssignNewShaderToMaterial(material, oldShader, newShader);

        if (oldShader.name.Contains("/Transparent/Cutout/")) {
            material.SetFloat(BP3ShaderUtility.blendModePID, (int)BP3ShaderUtility.BlendMode.Cutout);
        }
        else if (oldShader.name.Contains("/Transparent/")) {
            material.SetFloat(BP3ShaderUtility.blendModePID, (int)BP3ShaderUtility.BlendMode.Fade);
        }
        else {
            material.SetFloat(BP3ShaderUtility.blendModePID, (int)BP3ShaderUtility.BlendMode.Opaque);
        }

        BP3ShaderGUIUtility.SetupBlendMode(material);
        MaterialChanged(material);
    }

    void MaterialChanged(Material material) {
        BP3ShaderGUIUtility.SetKeyword(material, BP3ShaderUtility.highlightsOffKID, material.GetFloat(BP3ShaderUtility.highlightsPID) == 0.0);
        BP3ShaderGUIUtility.SetKeyword(material, BP3ShaderUtility.normalMapKID, material.GetTexture(BP3ShaderUtility.normalMapPID));
        BP3ShaderGUIUtility.SetKeyword(material, BP3ShaderUtility.doubleSidedLightingKID, material.GetFloat(BP3ShaderUtility.doubleSidedLightingPID) != 0.0);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        EditorGUIUtility.labelWidth = 0f;

        BP3ShaderGUIUtility.OnBlendModeGUI(materialEditor, properties);
        EditorGUILayout.Space();

        GUILayout.Label(BP3ShaderGUIUtility.Styles.primaryProps, EditorStyles.boldLabel);
        BP3ShaderGUIUtility.OnAlbedoGUI(materialEditor, properties);
        BP3ShaderGUIUtility.OnNormalMapGUI(materialEditor, properties);
        EditorGUILayout.Space();

        GUILayout.Label(BP3ShaderGUIUtility.Styles.advancedProps, EditorStyles.boldLabel);
        BP3ShaderGUIUtility.OnHighLightsGUI(materialEditor, properties);
        BP3ShaderGUIUtility.OnDoubleSidedGUI(materialEditor, properties);

        materialEditor.EnableInstancingField();
        materialEditor.DoubleSidedGIField();
    }
}
