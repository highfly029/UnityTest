using UnityEngine;

namespace Optimize {
    public static class RenderParams {
        public const string NormalKeyword = "_NORMALMAP";
        public const string SpecularKeyword = "_SPECULAR";
        public const string CubeMapKeyword = "_CUBEMAP";


        public const int CharacterHairQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry - 99;
        public const int CharacterBodyQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry - 100;
        public const string CharacterHairKeyword = "_BP3_CHARACTER_HAIR";
        public const string CharacterRimLightKeyword = "_RIM_LIGHT";
        public const string CharacterHitGlowKeyword = "_HIT_GLOW";
        public const string Shader_Keyword_Mask = "_HAS_MASK";

        public const string ObjectAlphaTestKeyword = "_ALPHATEST_ON";
        public const string ObjectAlphaBlendKeyword = "_ALPHABLEND_ON";

        public static readonly string[] MeshTerrainSplatMap = new string[4] { "_SPLATMAP0", "_SPLATMAP1", "_SPLATMAP2", "_SPLATMAP3" };
        public static readonly string[] MeshTerrainNormalMap = new string[4] { "_NORMALMAP0", "_NORMALMAP1", "_NORMALMAP2", "_NORMALMAP3" };
    }
}
