using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform PlayerBody;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        QualitySettings.vSyncCount = 0; 
    }
}