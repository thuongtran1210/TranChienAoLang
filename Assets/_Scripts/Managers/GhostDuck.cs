using System.Collections.Generic;
using UnityEngine;

public class GhostDuck : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer duckRenderer;
    // [CHANGE] Inject Generator vào thay vì tự làm
    [SerializeField] private DuckFootprintGenerator footprintGenerator;

    [Header("Settings")]
    [SerializeField] private GameObject leafPrefab;
    [SerializeField] private Color validColor = new Color(1, 1, 1, 0.6f);
    [SerializeField] private Color invalidColor = new Color(1, 0, 0, 0.6f);
    [SerializeField] private float cellSize = 1f; // Vẫn cần để tính toán Snap chuột
    [SerializeField] private int duckSortingOrder = 10;
    [SerializeField] private int leafSortingOrder = 5;

    // [CHANGE] Lưu trữ List GameObject thay vì SpriteRenderer để tương thích với Generator
    private List<GameObject> _currentFootprints = new List<GameObject>();

    public DuckDataSO CurrentData { get; private set; }
    public bool IsHorizontal { get; private set; } = true;
    private void Awake()
    {
        if (footprintGenerator == null)
            footprintGenerator = GetComponent<DuckFootprintGenerator>();

        // Validate component
        if (footprintGenerator == null)
            Debug.LogError("[GhostDuck] Missing DuckFootprintGenerator component!");
    }
    public void Show(DuckDataSO data)
    {
        CurrentData = data;
        IsHorizontal = true;

        // 1. Setup Vịt (Duck Visual)
        if (duckRenderer != null)
        {
            duckRenderer.sprite = data.icon;
            duckRenderer.sortingOrder = duckSortingOrder;
            duckRenderer.transform.localRotation = Quaternion.identity;
        }

        // 2. Setup Lá (Footprint Visual) -> [CHANGE] Ủy quyền cho Generator
        GenerateFootprint(data);

        // 3. Reset Transform
        transform.localRotation = Quaternion.identity;
        SetValidationState(true);
        gameObject.SetActive(true);
    }
    private void GenerateFootprint(DuckDataSO data)
    {
        // [SOLID] Single Responsibility: GhostDuck không cần biết cách spawn hay pool object.
        // Nó chỉ cần ra lệnh cho Generator: "Hãy tạo cho tôi hình dáng này".
        if (footprintGenerator != null)
        {
            _currentFootprints = footprintGenerator.Generate(data, leafPrefab, leafSortingOrder);
        }
    }
    public void Rotate()
    {
        IsHorizontal = !IsHorizontal;

        // Xoay cả Parent (GhostDuck), các lá con và vịt sẽ xoay theo
        float zRot = IsHorizontal ? 0 : 90;
        transform.localRotation = Quaternion.Euler(0, 0, zRot);

        // QUAN TRỌNG: Icon Vịt thường có ánh sáng/bóng đổ cố định. 
        // Nếu không muốn Vịt bị xoay (chỉ xoay đội hình lá), bạn cần xoay ngược Vịt lại:
        // duckRenderer.transform.localRotation = Quaternion.Euler(0, 0, -zRot); 
        // -> Bỏ comment dòng trên nếu muốn Vịt luôn đứng thẳng.
    }

    /// <summary>
    /// Hàm này nhận vào vị trí chuột (World Position), nhưng sẽ tự Snap vào lưới
    /// </summary>
    public void SetPosition(Vector3 pointerWorldPos)
    {
        if (CurrentData == null) return;

        // 1. SNAP TO GRID: Làm tròn vị trí chuột về tọa độ nguyên gần nhất
        int gridX = Mathf.RoundToInt(pointerWorldPos.x / cellSize);
        int gridY = Mathf.RoundToInt(pointerWorldPos.y / cellSize);

        Vector3 snappedPos = new Vector3(gridX * cellSize, gridY * cellSize, 0);

        // 2. CALCULATE OFFSET: Tính toán độ lệch tâm
        // Vì Pivot của tàu thường là (0,0), nhưng Sprite hiển thị lại cần nằm giữa các ô.
        Vector3 visualOffset = CalculateCenterOffset(CurrentData.size);

        // Áp dụng Offset dựa trên hướng xoay hiện tại
        if (!IsHorizontal)
        {
            // Khi xoay 90 độ, trục X/Y của Local đổi chỗ
            // Cần hoán đổi offset hoặc xoay vector offset
            visualOffset = new Vector3(visualOffset.y, visualOffset.x, 0);

            // Điều chỉnh nhỏ cho Pivot xoay (tùy thuộc vào việc xoay quanh tâm hay góc)
            // Với Sprite Sliced, xoay thường cần dịch chuyển thêm để khớp lưới
            // Trick: Nếu size chẵn, cần dịch 0.5. Nếu lẻ, giữ nguyên.
            if (CurrentData.size % 2 == 0)
            {
                // Logic bù trừ cho số chẵn khi xoay (thường là 0.5 unit)
                // Bạn có thể cần tinh chỉnh số này tùy vào Pivot của Sprite gốc
                snappedPos.y += 0.5f * cellSize;
                snappedPos.x += 0.5f * cellSize;
            }
        }

        transform.position = snappedPos + visualOffset;
    }
    /// <summary>
    /// Tính toán offset để đưa Pivot (0,0) về giữa con tàu
    /// </summary>
    private Vector3 CalculateCenterOffset(int size)
    {
        // Giả sử Pivot của Sprite Sliced nằm ở Center-Left (0, 0.5) hoặc Bottom-Left (0,0)
        // Đây là công thức cho Pivot Center (0.5, 0.5) của từng ô đơn lẻ

        // Nếu tàu dài 3 ô -> Center nằm ở 1.5 -> Offset từ gốc (0) là +1.5 ô (nếu Pivot Sprite ở tâm)
        // Nhưng Grid System thường tính từ góc dưới trái mỗi ô.

        // Cách đơn giản nhất cho Sliced Sprite (Pivot Center):
        // Không cần offset nếu Sprite đã set Pivot Center.
        // Chỉ cần offset nếu Sprite set Pivot Left.

        // Tại đây tôi giả định bạn đang setup Sprite Pivot = Center.
        // Nếu kích thước là chẵn (2,4), tâm nằm trên đường lưới -> Cần dịch 0.5
        // Nếu kích thước là lẻ (1,3), tâm nằm giữa ô -> Không cần dịch (nếu đã Snap)

        // TUY NHIÊN, với GridSystem, thường tọa độ (x,y) là TÂM ô.

        return Vector3.zero; // Thử trả về 0 trước, nếu lệch hãy dùng công thức dưới:

        /* Nếu tàu bị lệch, hãy dùng logic này:
           float xOffset = (size % 2 == 0) ? 0.5f * cellSize : 0f;
           return new Vector3(xOffset, 0, 0);
        */
    }

    public void SetValidationState(bool isValid)
    {
        Color targetColor = isValid ? validColor : invalidColor;

        if (duckRenderer) duckRenderer.color = targetColor;

        // [CHANGE] Duyệt qua List GameObject trả về từ Generator để đổi màu
        foreach (var segment in _currentFootprints)
        {
            // Cache GetComponent hoặc chấp nhận gọi lại (nếu số lượng ít < 10 thì không sao)
            if (segment.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr.color = targetColor;
            }
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        CurrentData = null;
    }
}