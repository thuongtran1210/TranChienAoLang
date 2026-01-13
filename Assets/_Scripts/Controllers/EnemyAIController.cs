// _Scripts/AI/EnemyAIController.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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

    public AIAction GetDecision(IGridSystem playerGrid, DuckEnergySystem myEnergy, List<DuckSkillSO> availableSkills)
    {
        // 1. Tính toán vị trí bắn tốt nhất (Logic cũ của bạn)
        Vector2Int bestNormalShotPos = GetNextTargetPosition(playerGrid);
        // 2. Logic dùng Skill nâng cao
        if (availableSkills != null && availableSkills.Count > 0)
        {
            // Cố gắng tìm một hành động dùng Skill hợp lý
            if (TryGetBestSkillAction(playerGrid, myEnergy, availableSkills, bestNormalShotPos, out AIAction skillAction))
            {
                return skillAction;
            }
        }

        // 3. Fallback: Nếu không dùng skill, bắn thường
        return AIAction.Attack(bestNormalShotPos);
    }
    private bool TryGetBestSkillAction(IGridSystem grid, DuckEnergySystem energy, List<DuckSkillSO> skills, Vector2Int targetPos, out AIAction action)
    {
        action = default;

        // Lọc ra các skill đủ năng lượng để dùng
        var affordableSkills = skills.Where(s => energy.CurrentEnergy >= s.energyCost).ToList();

        if (affordableSkills.Count == 0) return false;

        // --- TRƯỜNG HỢP 1: ĐANG SĂN TÌM (SEARCHING) ---
        if (_currentState == AIState.Searching)
        {
            // Ưu tiên dùng Skill có vùng ảnh hưởng lớn (Sonar/Radar) để tìm địch
            // Giả sử: Skill có AreaSize > 1x1 là skill diện rộng
            var scoutSkill = affordableSkills
                .OrderByDescending(s => s.areaSize.x * s.areaSize.y) // Ưu tiên vùng lớn nhất
                .FirstOrDefault();

            // Chỉ dùng nếu skill đó thực sự lớn (ví dụ 3x3) và AI đang bí nước đi hoặc chỉ muốn check map
            if (scoutSkill != null && (scoutSkill.areaSize.x > 1 || scoutSkill.areaSize.y > 1))
            {
                // Chọn một vị trí ngẫu nhiên trên bản đồ chưa bị bắn để dùng skill soi
                Vector2Int randomScanPos = GetRandomValidCell(grid);
                action = AIAction.Skill(randomScanPos, scoutSkill);
                Debug.Log($"[AI - Searching] Casting scout skill: {scoutSkill.skillName}");
                return true;
            }
        }

        // --- TRƯỜNG HỢP 2: ĐANG NHẮM BẮN (TARGETING) ---
        if (_currentState == AIState.Targeting)
        {
            // Ưu tiên skill gây sát thương (TargetType == Enemy hoặc Any)
            var attackSkills = affordableSkills
                .Where(s => s.targetType == SkillTargetType.Enemy || s.targetType == SkillTargetType.Any)
                .OrderByDescending(s => s.energyCost) // Dùng skill mạnh nhất (thường là tốn mana nhất)
                .ToList();

            foreach (var skill in attackSkills)
            {
                // Logic thông minh: Kiểm tra xem dùng skill tại vị trí targetPos có hiệu quả không?
                // (Ví dụ: Skill 3x3 bắn vào targetPos liệu có trùm lên các ô chưa bắn không?)

                // Ở đây ta đơn giản hóa: Nếu là skill damage, cứ nã vào vị trí đang nhắm
                action = AIAction.Skill(targetPos, skill);
                Debug.Log($"[AI - Targeting] Casting attack skill: {skill.skillName}");
                return true;
            }
        }

        return false;
    }

    // --- CORE LOGIC ---
    private Vector2Int GetNextTargetPosition(IGridSystem playerGrid)
    {
        // Ưu tiên bắn các ô trong Stack (Chế độ Targeting)
        if (_currentState == AIState.Targeting && _potentialTargets.Count > 0)
        {
            return GetTargetFromStack(playerGrid);
        }

        // Nếu không có target trong stack, bắn theo Parity (Searching)
        return GetParityHuntingShot(playerGrid);
    }
    private Vector2Int GetTargetFromStack(IGridSystem grid)
    {
        while (_potentialTargets.Count > 0)
        {
            Vector2Int candidate = _potentialTargets.Pop();

            // Validate: Chỉ trả về nếu ô đó hợp lệ và chưa bắn
            if (grid.IsValidPosition(candidate) && !grid.GetCell(candidate).IsHit)
            {
                return candidate;
            }
        }

        // Nếu Stack rỗng -> Quay về Search
        _currentState = AIState.Searching;
        return GetParityHuntingShot(grid);
    }
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
        for (int i = _availableParityMoves.Count - 1; i >= 0; i--)
        {
            Vector2Int pos = _availableParityMoves[i];
            _availableParityMoves.RemoveAt(i); // O(1) swap remove would be better but List remove is O(N)

            if (grid.IsValidPosition(pos) && !grid.GetCell(pos).IsHit)
            {
                return pos;
            }
        }

        // Fallback khẩn cấp: Quét tuyến tính nếu hết Parity moves (trường hợp hiếm)
        return GetRandomValidCell(grid);
    }
    private Vector2Int GetRandomValidCell(IGridSystem grid)
    {
        // Simple random fallback
        int w = 10; // Nên lấy từ GridSystem
        int h = 10;
        int attempts = 100;
        while (attempts > 0)
        {
            int x = Random.Range(0, w);
            int y = Random.Range(0, h);
            Vector2Int pos = new Vector2Int(x, y);
            if (!grid.GetCell(pos).IsHit) return pos;
            attempts--;
        }
        return Vector2Int.zero;
    }

    // Chế độ 2: Nhắm bắn 
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