using UnityEngine;
using UnityEditor;

namespace Optimize {
    public class ModelImportProcessor : AssetPostprocessor {
        void OnPreprocessModel() {
            var mi = assetImporter as ModelImporter;

            // @TODO:
            mi.importMaterials = false;
        }
    }
}