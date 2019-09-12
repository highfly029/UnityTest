using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Optimize {
    public class PropertiesModifier {
        public static void ModifyRendererProperties(Renderer renderer) {
            Assert.IsNotNull(renderer);

            if (HasAttachCharacterShader(renderer)) {
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = false;
            }
        }

        static bool HasAttachCharacterShader<T>(T renderer) where T : Renderer {
            var shaderName = "BadPerson3/CharacterBP";
            return HasAttachShader(renderer, shaderName);
        }

        static bool HasAttachShader<T>(T renderer, string shaderName) where T : Renderer {
            Assert.IsNotNull(renderer);

            var mats = renderer.sharedMaterials;
            for (var i = 0; i < mats.Length; ++i) {
                var m = mats[i];
                if (m != null && m.shader != null && m.shader.name == shaderName) {
                    return true;
                }
            }

            return false;
        }

        public static T[] FindRendererWithMaterial<T>(Material material) where T : Renderer {
            Assert.IsNotNull(material);
            var rs = Object.FindObjectsOfType<T>();
            var retrs = new List<T>();

            for (var i = 0; i < rs.Length; ++i) {
                var r = rs[i];
                var mats = r.sharedMaterials;
                Assert.IsNotNull(mats);

                for (var j = 0; j < mats.Length; ++j) {
                    var m = mats[j];
                    if (m == material) {
                        retrs.Add(r);
                        break;
                    }
                }
            }

            return retrs.ToArray();
        }
    }
}
