// _Scripts/AI/EnemyAIController.cs
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIController : IEnemyAI
{
    private enum AIState { Searching, Targeting }
    private AIState _currentState = AIState.Searching;

    // Stack lưu các ô tiềm năng khi ở chế độ Targeting.
    private Stack<Vector2Int> _potentialTargets = new Stack<Vector2Int>();
    private Vector2Int _lastHitPos;

    public Vector2Int GetNextTarget(IGridSystem playerGrid)
    {
        if (_currentState == AIState.Targeting && _potentialTargets.Count > 0)
        {
            return GetTargetingShot(playerGrid);
        }

        // Fallback về Searching nếu hết target hoặc đang searching
        _currentState = AIState.Searching;
        return GetParityHuntingShot(playerGrid);
    }

    // Parity Hunting 
    private Vector2Int GetParityHuntingShot(IGridSystem grid)
    {
        // Logic tìm ô ngẫu nhiên thỏa mãn (x + y) % 2 == 0 và chưa bị bắn
        // Viết đơn giản: Random until valid (Cần tối ưu trong thực tế bằng List available moves)
        int w = grid.Width;
        int h = grid.Height;

        while (true)
        {
            int x = Random.Range(0, w);
            int y = Random.Range(0, h);
            if ((x + y) % 2 == 0 && !grid.GetCell(new Vector2Int(x, y)).IsHit)
            {
                return new Vector2Int(x, y);
            }
            // Safety break cần thiết...
        }
    }

    // GDD: Targeting (Bắn lân cận)
    private Vector2Int GetTargetingShot(IGridSystem grid)
    {
        while (_potentialTargets.Count > 0)
        {
            Vector2Int candidate = _potentialTargets.Pop();
            if (grid.IsValidPosition(candidate) && !grid.GetCell(candidate).IsHit)
            {
                return candidate;
            }
        }

        // Nếu stack rỗng mà vẫn gọi, quay về search
        _currentState = AIState.Searching;
        return GetParityHuntingShot(grid);
    }

    // Gọi hàm này khi AI bắn trúng (để chuyển state)
    public void NotifyHit(Vector2Int hitPos, IGridSystem grid)
    {
        _currentState = AIState.Targeting;
        _lastHitPos = hitPos;

        // Push 4 ô lân cận vào Stack
        _potentialTargets.Push(hitPos + Vector2Int.up);
        _potentialTargets.Push(hitPos + Vector2Int.down);
        _potentialTargets.Push(hitPos + Vector2Int.left);
        _potentialTargets.Push(hitPos + Vector2Int.right);

        // Mẹo: Shuffle stack để AI bắn ngẫu nhiên các hướng, khó đoán hơn
    }
}