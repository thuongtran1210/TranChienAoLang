using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Camera targetCamera; 

    [Header("Settings")]
    [SerializeField] private float padding = 1.0f;

    private const float BOARDS_OFFSET_X = 10f;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }


    public void SetupCamera(int gridWidth, int gridHeight)
    {
        Vector3 centerPos = new Vector3((float)gridWidth / 2 - 0.5f, (float)gridHeight / 2 - 0.5f, -10f);
        transform.position = centerPos;

        // 2. Tính toán Zoom (Orthographic Size)
        float screenRatio = (float)Screen.width / Screen.height;

        // Chiều rộng cần hiển thị = Chiều rộng 1 bàn cờ + Khoảng cách giữa 2 gốc tọa độ (10 unit)
        float totalTargetWidth = gridWidth + BOARDS_OFFSET_X;

        float targetRatio = totalTargetWidth / gridHeight;

        if (screenRatio >= targetRatio)
        {
            // Màn hình rộng hơn so với tỉ lệ game -> Fit theo chiều cao (giữ nguyên logic cũ)
            targetCamera.orthographicSize = (gridHeight / 2.0f) + padding;
        }
        else
        {
            // Màn hình hẹp hơn (ví dụ điện thoại dọc) -> Fit theo chiều rộng
            // Phải đảm bảo chiều rộng bao được cả 'totalTargetWidth'
            float differenceInSize = targetRatio / screenRatio;
            targetCamera.orthographicSize = ((gridHeight / 2.0f) + padding) * differenceInSize;
        }
    }

    // [Optional] Helper để các class khác không cần gọi trực tiếp Camera.main
    public Camera GetCamera() => targetCamera;
}