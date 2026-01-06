using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Camera targetCamera; // Camera mà controller này quản lý

    [Header("Settings")]
    [SerializeField] private float padding = 1.0f;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    // Hàm này CHỈ chịu trách nhiệm đặt vị trí và zoom
    public void SetupCamera(int gridWidth, int gridHeight)
    {
        // 1. Tính tâm
        Vector3 centerPos = new Vector3((float)gridWidth / 2 - 0.5f, (float)gridHeight / 2 - 0.5f, -10f);
        transform.position = centerPos; // Di chuyển GameObject chứa CameraController (thường gắn chung Camera)

        // 2. Tính Zoom (Orthographic)
        float screenRatio = (float)Screen.width / Screen.height;
        float targetRatio = (float)gridWidth / gridHeight;

        if (screenRatio >= targetRatio)
        {
            targetCamera.orthographicSize = (gridHeight / 2.0f) + padding;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            targetCamera.orthographicSize = ((gridHeight / 2.0f) + padding) * differenceInSize;
        }
    }

    // [Optional] Helper để các class khác không cần gọi trực tiếp Camera.main
    public Camera GetCamera() => targetCamera;
}