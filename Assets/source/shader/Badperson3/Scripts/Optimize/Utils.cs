using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace Optimize {
    public static class Utils {
        public static string GetValidFileNameWithSufixNum(string dirName, string fileName, string fileExt) {
            var path = string.Format("{0}{1}{2}.{3}", dirName, Path.AltDirectorySeparatorChar, fileName, fileExt);
            if (!File.Exists(path)) {
                return path;
            }

            var num = 1;
            while (true) {
                path = string.Format("{0}{1}{2}_{3}.{4}", dirName, Path.AltDirectorySeparatorChar, fileName, num, fileExt);
                if (!File.Exists(path)) {
                    return path;
                }

                num++;
            }
        }

        public static T[] FindRendererWithMaterial<T>(Material material) where T : Renderer {
            Assert.IsNotNull(material);
            var renderers = Object.FindObjectsOfType<T>();
            var retRenderers = new List<T>();
            for (var i = 0; i < renderers.Length; ++i) {
                var ren = renderers[i];
                var mats = ren.sharedMaterials;
                Assert.IsNotNull(mats);

                for (var j = 0; j < mats.Length; ++j) {
                    var m = mats[j];
                    if (m == material) {
                        retRenderers.Add(ren);
                        break;
                    }
                }
            }
            return retRenderers.ToArray();
        }
    }
}