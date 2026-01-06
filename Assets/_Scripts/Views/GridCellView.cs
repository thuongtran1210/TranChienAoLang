using UnityEngine;

public class GridCellView : MonoBehaviour, IGridInteractable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite; // Hình mặt nước
    [SerializeField] private Sprite hitSprite;  // Hình 'Nổ/Trúng' 
    [SerializeField] private Sprite missSprite; // Hình 'Nước bắn/Trượt' 

    public Owner CellOwner { get; private set; }

    public GridCell _cellLogic { get; private set; }

    public Vector2Int GridPosition => _cellLogic.GridPosition;

    public void Setup(GridCell cellLogic, Owner owner)
    {
        _cellLogic = cellLogic;
        CellOwner = owner; 

        spriteRenderer.sprite = defaultSprite;
  
        gameObject.name = $"{owner}_Cell_{cellLogic.GridPosition.x}_{cellLogic.GridPosition.y}";
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
}