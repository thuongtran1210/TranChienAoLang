using UnityEngine;

public class GridCellView : MonoBehaviour, IGridInteractable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite; // Hình mặt nước
    [SerializeField] private Sprite hitSprite;  // Hình 'Nổ/Trúng' 
    [SerializeField] private Sprite missSprite; // Hình 'Nước bắn/Trượt' 
    private Color _originalColor = Color.white;
    public Owner CellOwner { get; private set; }

    public GridCell _cellLogic { get; private set; }

    public Vector2Int GridPosition => _cellLogic.GridPosition;

    private void Awake()
    {
        // BEST PRACTICE: Luôn validate reference quan trọng
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Lưu lại màu ban đầu của Sprite nếu cần (Optional)
        if (spriteRenderer != null)
            _originalColor = spriteRenderer.color;
    }

    public void Setup(GridCell cellLogic, Owner owner)
    {
        _cellLogic = cellLogic;
        CellOwner = owner;

        if (spriteRenderer != null)
            spriteRenderer.sprite = defaultSprite;

#if UNITY_EDITOR
        gameObject.name = $"{owner}_Cell_{cellLogic.GridPosition.x}_{cellLogic.GridPosition.y}";
#endif
    }

    public void UpdateVisual(ShotResult shotResult)
    {
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
    /// Hàm đổi màu ô để highlight (được GridView gọi)
    /// </summary>
    public void SetHighlightState(bool isActive, Color color)
    {
        if (isActive)
            SetColor(color);
        else
            ResetColor();
    }
    /// <summary>
    /// Đặt màu trực tiếp cho ô (Dùng cho Highlight)
    /// </summary>
    public void SetColor(Color color)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = color;
    }

    /// <summary>
    /// Đưa ô về màu mặc định ban đầu
    /// </summary>
    public void ResetColor()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = _originalColor;
    }
}