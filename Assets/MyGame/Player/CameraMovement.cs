using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float CameraSensitivity = 100f;
    public Transform PlayerBody;
    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Application.targetFrameRate = 200; 
        QualitySettings.vSyncCount = 0; 
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * CameraSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * CameraSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        PlayerBody.Rotate(Vector3.up * mouseX);

    }
    void OnGUI()
{
    // Display the FPS 
    GUI.Label(new Rect(10, 10, 100, 20), "FPS: " + (1.0f / Time.deltaTime).ToString("F0"));
}
}