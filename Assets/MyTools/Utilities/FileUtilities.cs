#if UNITY_EDITOR
using System.IO;
using UnityEditor;

namespace VS.Utilities {
    public static class FileUtilities {
        public static void CreateValidFolder(string path) {
            string parentPath = Path.GetDirectoryName(path);
            string folderName = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parentPath, folderName);
        }
    }
}
#endif