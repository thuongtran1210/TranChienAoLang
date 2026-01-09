// _Scripts/AI/EnemyAIController.cs
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIController : IEnemyAI
{
    private enum AIState { Searching, Targeting }
    private AIState _currentState = AIState.Searching;

    private List<Vector2Int> _availableParityMoves;
    private Stack<Vector2Int> _potentialTargets = new Stack<Vector2Int>();

    private Vector2Int _lastHitPos;

    private readonly List<Vector2Int> _cachedNeighbors = new List<Vector2Int>(4);


    // --- INITIALIZATION ---
    public void Initialize(int width, int height)
    {
        _availableParityMoves = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if ((x + y) % 2 == 0)
                {
                    _availableParityMoves.Add(new Vector2Int(x, y));
                }
            }
        }
        ShuffleList(_availableParityMoves);
    }
    // --- CORE LOGIC ---

    public Vector2Int GetNextTarget(IGridSystem playerGrid)
    {
        // Ưu tiên bắn các ô trong Stack (Chế độ Targeting)
        if (_currentState == AIState.Targeting && _potentialTargets.Count > 0)
        {
            return GetTargetingShot(playerGrid);
        }

        // Nếu hết mục tiêu tiềm năng, quay lại chế độ săn tìm (Searching)
        _currentState = AIState.Searching;
        return GetParityHuntingShot(playerGrid);
    }

    // Parity Hunting 
    private Vector2Int GetParityHuntingShot(IGridSystem grid)
    {
        // Lấy từ túi (List) ra thay vì Random.Range trong vòng lặp while(true)
        while (_availableParityMoves != null && _availableParityMoves.Count > 0)
        {
            // Lấy phần tử cuối (hiệu suất O(1) khi remove)
            int lastIndex = _availableParityMoves.Count - 1;
            Vector2Int candidate = _availableParityMoves[lastIndex];
            _availableParityMoves.RemoveAt(lastIndex);

            // Kiểm tra: Chỉ bắn nếu ô này CHƯA bị bắn (có thể đã bị bắn trong lúc Targeting)
            if (grid.IsValidPosition(candidate) && !grid.GetCell(candidate).IsHit)
            {
                return candidate;
            }
        }

        // Fallback an toàn: Nếu hết nước đi Parity (hiếm khi xảy ra), 
        // quét tuyến tính tìm bất kỳ ô trống nào còn lại
        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!grid.GetCell(pos).IsHit) return pos;
            }
        }

        return Vector2Int.zero; // Should not happen if game flow is correct
    }

    // Chế độ 2: Nhắm bắn (Targeting)
    private Vector2Int GetTargetingShot(IGridSystem grid)
    {
        while (_potentialTargets.Count > 0)
        {
            Vector2Int candidate = _potentialTargets.Pop();

            // Chỉ trả về nếu ô đó hợp lệ và chưa bắn
            if (grid.IsValidPosition(candidate) && !grid.GetCell(candidate).IsHit)
            {
                return candidate;
            }
        }

        // Nếu Stack rỗng mà vẫn gọi hàm này -> quay về Search
        _currentState = AIState.Searching;
        return GetParityHuntingShot(grid);
    }

    // Gọi hàm này khi AI bắn trúng (để chuyển state)
    public void NotifyHit(Vector2Int hitPos, IGridSystem grid)
    {
        _currentState = AIState.Targeting;
        _lastHitPos = hitPos;

        _cachedNeighbors.Clear(); // Xóa sạch dữ liệu cũ
        _cachedNeighbors.Add(hitPos + Vector2Int.up);
        _cachedNeighbors.Add(hitPos + Vector2Int.down);
        _cachedNeighbors.Add(hitPos + Vector2Int.left);
        _cachedNeighbors.Add(hitPos + Vector2Int.right);

        ShuffleList(_cachedNeighbors); // Shuffle trực tiếp trên cached list

        foreach (var neighbor in _cachedNeighbors)
        {
            if (grid.IsValidPosition(neighbor) && !grid.GetCell(neighbor).IsHit)
            {
                _potentialTargets.Push(neighbor);
            }
        }
    }
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}