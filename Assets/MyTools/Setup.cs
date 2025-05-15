#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Utilities {
    public static class Setup {
        [MenuItem("Tools/Setup/Create Default Folders")]
        public static void CreateDefaultFolder() {
            Folders.CreateDefault("_Project", "Animation", "Art", "Materials", "Prefabs", "ScriptableObjects",
                "Scripts", "Settings");
            UnityEditor.AssetDatabase.Refresh();
        }

        private static class Folders {
            public static void CreateDefault(string root, params string[] folders) {
                // Takes a root folder and an array of subfolders to use 
                var fullPath = Path.Combine(Application.dataPath, root);

                foreach (var folder in folders) {
                    var path = Path.Combine(fullPath, folder);
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                }
            }
        }
    }
}
#endif