using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float CameraSensitivity = 100f;
    public Transform PlayerBody;
    private float xRotation = 0f;
        void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * CameraSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * CameraSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        PlayerBody.Rotate(Vector3.up * mouseX);
    }
}