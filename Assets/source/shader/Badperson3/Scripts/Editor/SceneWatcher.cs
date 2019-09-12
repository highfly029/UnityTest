using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace Optimize {
    public class SceneWatcher {
        [InitializeOnLoadMethod]
        static void StartupEditorDelegate() {
            EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate() {
        }

        static void OnHierarchyWindowChanged() {
            ModifyRendererProperties();
        }

        static void ModifyRendererProperties() {
            var rs = UnityEngine.Object.FindObjectsOfType<Renderer>();
            for (var i = 0; i < rs.Length; ++i) {
                var r = rs[i];
                PropertiesModifier.ModifyRendererProperties(r);
            }
        }
    }
}