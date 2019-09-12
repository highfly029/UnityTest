using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.U2D;
namespace Optimize
{
    public class UITextureImport : AssetPostprocessor
    {

        void OnPreprocessTexture()
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            if (assetPath.Contains("OriginalRes/UI"))//UI图集切图导入
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.wrapMode = TextureWrapMode.Repeat;

            }
        }
    }
}
