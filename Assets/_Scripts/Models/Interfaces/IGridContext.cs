// 3. Interface gom nhóm (Optional - dùng cho chính GridManager implement)
using System;
using UnityEngine;

public interface IGridContext : IGridLogic, IGridVisuals
{
    // Có thể chứa thêm Event chung
    event Action<IGridContext, Vector2Int> OnGridClicked;
}