using UnityEngine;

public class GridCellView : MonoBehaviour, IGridInteractable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite; // Hình mặt nước
    [SerializeField] private Sprite hitSprite;  // Hình 'Nổ/Trúng' 
    [SerializeField] private Sprite missSprite; // Hình 'Nước bắn/Trượt' 
    private Color _originalColor = Color.white;

    // Properties
    public Owner CellOwner { get; private set; }
    public GridCell _cellLogic { get; private set; }
    public Vector2Int GridPosition => _cellLogic != null ? _cellLogic.GridPosition : Vector2Int.zero;

    public void Setup(GridCell cellLogic, Owner owner)
    {
        _cellLogic = cellLogic;
        CellOwner = owner;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = defaultSprite;
            _originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError($"[GridCellView] Missing SpriteRenderer on {gameObject.name}", this);
        }

#if UNITY_EDITOR
        gameObject.name = $"{owner}_Cell_{cellLogic.GridPosition.x}_{cellLogic.GridPosition.y}";
#endif
    }

    public void UpdateVisual(ShotResult shotResult)
    {
        if (spriteRenderer == null) return;

        switch (shotResult)
        {
            case ShotResult.Hit:
            case ShotResult.Sunk:
                spriteRenderer.sprite = hitSprite;
                break;

            case ShotResult.Miss:
                spriteRenderer.sprite = missSprite;
                break;
        }
    }
    /// <summary>
    /// Điều chỉnh Scale của view để vừa khít với kích thước ô lưới logic
    /// </summary>
    /// <param name="targetSize">Kích thước mong muốn (cellSize)</param>
    public void SetVisualSize(float targetSize)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        // 1. Reset scale về 1 để lấy kích thước gốc chính xác
        transform.localScale = Vector3.one;

        // 2. Lấy kích thước gốc của Sprite (Local Bounds)
        // bounds.size trả về kích thước tính bằng World Unit (dựa trên PPU settings)
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        if (spriteSize.x == 0 || spriteSize.y == 0)
        {
            Debug.LogWarning($"[GridCellView] Sprite {gameObject.name} có kích thước 0!");
            return;
        }

        // 3. Tính toán tỷ lệ cần thiết
        // Công thức: Scale = Target / Original
        float scaleFactorX = targetSize / spriteSize.x;
        float scaleFactorY = targetSize / spriteSize.y;

        // 4. Áp dụng Scale mới
        transform.localScale = new Vector3(scaleFactorX, scaleFactorY, 1f);
    }
    /// <summary>
    /// Hàm xử lý trạng thái Highlight thống nhất.
    /// Đáp ứng yêu cầu API từ GridView.
    /// </summary>
    /// <param name="isActive">Bật hay tắt highlight</param>
    /// <param name="color">Màu highlight (nếu bật)</param>
    public void SetHighlightState(bool isActive, Color color)
    {
        if (isActive)
        {
            SetColor(color);
        }
        else
        {
            ResetColor();
        }
    }
    /// <summary>
    /// Đặt màu trực tiếp cho Cell (Dùng cho Highlight)
    /// </summary>
    /// <param name="color">Màu cần hiển thị</param>
    public void SetColor(Color color)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = color;
    }

    /// <summary>
    /// Đưa màu về trạng thái ban đầu (Tắt Highlight)
    /// </summary>
    public void ResetColor()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = _originalColor;
    }
}