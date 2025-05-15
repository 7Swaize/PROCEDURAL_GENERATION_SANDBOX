#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;

namespace VS.Utilities.Files
{
    [CreateAssetMenu(fileName = "JsonDataServiceTest", menuName = "JsonDataServiceTest/JsonDataServiceTest")]
    public class JsonDataServiceTest : ScriptableObject
    {
        public TestConfigs testConfigs;
        private static readonly IDataServiceStrategy _dataServiceStrategy = DataServiceStrategyFactory.GetStrategy<JsonDataServiceTest>();
        

        [CustomEditor(typeof(JsonDataServiceTest), true)]
        private class JsonDataServiceTestEditor : Editor
        {
            private JsonDataServiceTest _target;
            
            public override void OnInspectorGUI()
            {
                _target = (JsonDataServiceTest)target;
                
                DrawDefaultInspector();
                
                if (GUILayout.Button("Store Values"))
                {
                    _ = StoreMapValues();
                }

                if (GUILayout.Button("Refresh Values"))
                {
                    _ = PullMapValues();
                }

                if (GUILayout.Button("Clear Values"))
                {
                    ClearMapValues();
                }
            }
            
            private async Task StoreMapValues()
            {
                await _dataServiceStrategy.SaveDataWithOverrideAsync(relativePath: "/test-values.json",
                    data: _target.testConfigs   , 
                    encrypter: null);
            }

            private async Task PullMapValues()
            {
                var result = await _dataServiceStrategy.LoadDataAsync<TestConfigs>(relativePath: "/test-values.json",
                    encrypter: null);
                
                foreach (var property in result.GetType().GetProperties())
                {
                    var value = property.GetValue(result);
                    Debug.Log($"{property.Name}: {value}"); 
                }
            }
        
            private void ClearMapValues() => _dataServiceStrategy.ClearData(relativePath: "/test-values.json");
        }
    }
    
    
    [CreateAssetMenu(fileName = "TestConfigs", menuName = "JsonDataServiceTest/TestConfigs")]
    public class TestConfigs : ScriptableObject
    {
        [Header("Test Variables")]
        public string testString = "hello world";
        public int testInt = 3;
        public float testFloat = 3.14f;
        public bool testBool = true;
    }
}
#endif