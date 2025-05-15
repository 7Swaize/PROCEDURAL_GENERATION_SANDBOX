using UnityEngine;
using UnityEngine.SceneManagement;

namespace VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation {
    public class TitleScreenManager : MonoBehaviour {
        [Space, Header("References")]
        [SerializeField] private GameObject titleScreen;
        [SerializeField] private GameObject controls;
        [SerializeField] private GameObject instructions;
        
        private int _currentSlide;
        
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Return) && _currentSlide == 0) {
                titleScreen.SetActive(false);
                controls.SetActive(true);
                _currentSlide++;
            }
            else if (Input.GetKeyDown(KeyCode.Return) && _currentSlide == 1) {
                controls.SetActive(false);
                instructions.SetActive(true);
                _currentSlide++;
            }
            else if (Input.GetKeyDown(KeyCode.Return) && _currentSlide == 2) {
                instructions.SetActive(false);
                _currentSlide++;
                
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }
}