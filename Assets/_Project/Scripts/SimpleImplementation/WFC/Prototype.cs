using System.Collections.Generic;
using UnityEngine;
using static VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation.Orientations;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

namespace VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation {
    [CreateAssetMenu(fileName = "Prototype", menuName = "WFC/Prototype")]
    public class Prototype : ScriptableObject {
        public new string name; 
        
        [Space, Header("WFC")]
        public GameObject prefab;
        public float prototypeWeight;
        public PossibleConnections possibleConnections;
        
        [HideInInspector] public Mesh mesh;
        [HideInInspector] public Material material;

        public GameObject GetPrefab(bool isCollapsed) => isCollapsed ? prefab : null;
        

#if UNITY_EDITOR
        private void OnValidate() {
            SetupScriptableObjectName();
            SetupGPUInstancingData();
        }

        private void SetupGPUInstancingData() {
            mesh = GetMeshFromPrefab();
            material = GetMaterialFromPrefab();
        }

        private Material GetMaterialFromPrefab() => prefab != null ? prefab.GetComponent<MeshRenderer>()?.sharedMaterial : null;
        private Mesh GetMeshFromPrefab() => prefab != null ? prefab.GetComponent<MeshFilter>()?.sharedMesh : null;

        private void SetupScriptableObjectName() {
            if (string.IsNullOrEmpty(name)) return;

            string path = AssetDatabase.GetAssetPath(this);
            string currentFileName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (currentFileName != name) {
                string newPath = System.IO.Path.GetDirectoryName(path) + "/" + name + ".asset";
                AssetDatabase.RenameAsset(path, name);
                AssetDatabase.SaveAssets();
            }
        }
#endif
    }
    
    [System.Serializable]
    public class PossibleConnections {
        public List<Prototype> posX = new();
        public List<Prototype> negX = new();
        public List<Prototype> posZ = new();
        public List<Prototype> negZ = new();
        
        // TODO: Indexer swapped because I swapped directional constraints. Fix later.
        public List<Prototype> this[Orientation direction] {
            get {
                return direction switch {
                    Orientation.Left => posX,
                    Orientation.Right => negX,
                    Orientation.Back => posZ,
                    Orientation.Forward => negZ,
                    _ => new List<Prototype>()
                };
            }
            set {
                switch (direction) {
                    case Orientation.Left:    negX = value; break;
                    case Orientation.Right:   posX = value; break;
                    case Orientation.Back:    negZ = value; break;
                    case Orientation.Forward: posZ = value; break;
                }
            }
        }
    }
}