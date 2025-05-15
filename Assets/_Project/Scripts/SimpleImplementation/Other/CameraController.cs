using UnityEngine;

namespace VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation {
    // I just asked ChatGPT to write a quick character controller because I didn't want to.
    // That's why its so trash.
    public class SimpleCameraController : MonoBehaviour {
        public float moveSpeed = 5f;
        public float lookSpeed = 2f;

        float yaw, pitch;

        void Start() => Cursor.lockState = CursorLockMode.Locked;

        void Update() {
            yaw += Input.GetAxis("Mouse X") * lookSpeed;
            pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
            
            pitch = Mathf.Clamp(pitch, -90f, 90f);
            transform.eulerAngles = new Vector3(pitch, yaw, 0f);

            Vector3 move = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");

            if (Input.GetKey(KeyCode.Space)) move += Vector3.up;
            if (Input.GetKey(KeyCode.LeftControl)) move += Vector3.down;

            transform.position += move * moveSpeed * Time.deltaTime;
        }
    }
}