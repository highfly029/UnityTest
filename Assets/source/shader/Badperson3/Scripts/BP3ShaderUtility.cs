using UnityEngine;

public class BP3ShaderUtility {
    public enum BlendMode {
        Opaque,
        Cutout,
        Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
        //Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
    }

    public enum CharacterPart {
        Body,
        Hair
    }

    public const string doubleSidedLightingPID = "_DoubleSidedLighting";
    public const string doubleSidedLightingKID = "_DOUBLE_SIDED_LIGHTING";
    public const string doubleSidedPID = "_DoubleSided";
    public const string cullingModePID = "_Cull";

    public const string blendModePID = "_Mode";
    public const string partPID = "_Mode";
    public const string srcBlendPID = "_SrcBlend";
    public const string dstBlendPID = "_DstBlend";
    public const string zwritePID = "_ZWrite";

    public const string alphaTestKID = "_ALPHATEST_ON";
    public const string alphaBlendKID = "_ALPHABLEND_ON";
    public const string alphaPremultiplyKID = "_ALPHAPREMULTIPLY_ON";

    public const string highlightsPID = "_SpecularHighlights";
    public const string reflectionsPID = "_GlossyReflections";
    public const string highlightsOffKID = "_SPECULARHIGHLIGHTS_OFF";
    public const string reflectionsOffKID = "_GLOSSYREFLECTIONS_OFF";

    public static string specColorPID = "_SpecColor";
    public static string specShinePID = "_SpecShine";
    public static string specGlossnessPID = "_SpecGlossness";

    public const string normalMapPID = "_BumpMap";
    public const string normalMapKID = "_NORMALMAP";
    public const string normalScalePID = "_BumpScale";

    public const string colorPID = "_Color";
    public const string mainTexPID = "_MainTex";
    public const string alphaTexPID = "_AlphaTex";
    public const string alphaCutoffPID = "_Cutoff";
}