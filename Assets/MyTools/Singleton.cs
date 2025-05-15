using UnityEngine;


namespace VS.Utilities.Singletons {
    public class Singleton<T> : MonoBehaviour where T : Component {
        protected static T instance;

        public static bool HasInstance => instance != null;
        public static T TryGetInstance() => HasInstance ? instance : null;

        public static T Instance {
            get {
                if (instance == null) {
                    instance = FindAnyObjectByType<T>();

                    if (instance == null) {
                        instance = new GameObject(typeof(T).Name + " Auto Generated").AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        public virtual void Awake() {
            InitializeSingleton();
        }

        private void InitializeSingleton() {
            if (!Application.isPlaying) return;

            instance = this as T;
        }
    }
}