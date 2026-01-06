using UnityEngine;

public class GridCellView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite; // Hình mặt nước
    [SerializeField] private Sprite hitSprite;  // Kéo hình 'Nổ/Trúng' vào đây
    [SerializeField] private Sprite missSprite; // Kéo hình 'Nước bắn/Trượt' vào đây

    public Owner CellOwner { get; private set; }
    public GridCell _cellLogic { get; private set; }
    public void Setup(GridCell cellLogic, Owner owner)
    {
        _cellLogic = cellLogic;
        CellOwner = owner; // Lưu lại danh tính

        spriteRenderer.sprite = defaultSprite;
        // Đặt tên kèm Owner để dễ debug trong Hierarchy
        gameObject.name = $"{owner}_Cell_{cellLogic.GridPosition.x}_{cellLogic.GridPosition.y}";
    }

    public void UpdateVisual(ShotResult shotResult)
    {
        switch (shotResult)
        {
            case ShotResult.Hit:
            case ShotResult.Sunk:
                // Cả Hit và Sunk đều hiện hình trúng đạn
                spriteRenderer.sprite = hitSprite;
                break;

            case ShotResult.Miss:
                // Hiện hình bắn trượt
                spriteRenderer.sprite = missSprite;
                break;

                // ShotResult.Invalid thì giữ nguyên, không làm gì
        }
    }
}