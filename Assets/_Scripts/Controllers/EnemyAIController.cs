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

    private AIDifficulty _currentDifficulty = AIDifficulty.Normal;
    private AIDifficultyConfigSO _difficultyConfig;
    private float _decisionRandomness;

   // --- INITIALIZATION ---
    public void Initialize(int width, int height)
       {
           Initialize(width, height, null);
       }
    public void Initialize(int width, int height, AIDifficultyConfigSO difficultyConfig)
    {
        _difficultyConfig = difficultyConfig;
        if (_difficultyConfig != null)
        {
            _currentDifficulty = _difficultyConfig.CurrentDifficulty;
            _decisionRandomness = _difficultyConfig.GetSettings(_currentDifficulty).DecisionRandomness;
        }

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
        Vector2Int bestNormalShotPos = GetNextTargetPosition(playerGrid);
        AIAction bestAction = AIAction.Attack(bestNormalShotPos);
        float bestScore = ScoreNormalAttack(playerGrid, bestNormalShotPos);

        if (availableSkills != null && availableSkills.Count > 0 && myEnergy != null)
        {
            var affordableSkills = availableSkills.Where(s => s != null && myEnergy.CurrentEnergy >= s.energyCost);

            foreach (var skill in affordableSkills)
            {
                if (TryEvaluateSkill(playerGrid, myEnergy, skill, bestNormalShotPos, out AIAction skillAction, out float skillScore))
                {
                    if (skillScore > bestScore)
                    {
                        bestScore = skillScore;
                        bestAction = skillAction;
                    }
                }
            }
        }

        return bestAction;
    }

    private float ScoreNormalAttack(IGridSystem grid, Vector2Int targetPos)
    {
        float baseScore = 10f;

        if (_currentState == AIState.Targeting && _potentialTargets.Count > 0)
        {
            baseScore += 2f;
        }

        if (_decisionRandomness > 0f)
        {
            float noise = Random.Range(-_decisionRandomness, _decisionRandomness) * baseScore;
            baseScore += noise;
        }

        return baseScore;
    }

    private bool TryEvaluateSkill(IGridSystem grid, DuckEnergySystem energy, DuckSkillSO skill, Vector2Int defaultTargetPos, out AIAction action, out float score)
    {
        action = default;
        score = 0f;

        if (grid == null || energy == null || skill == null)
        {
            return false;
        }

        if (energy.CurrentEnergy < skill.energyCost)
        {
            return false;
        }

        if (_currentState == AIState.Searching)
        {
            return TryEvaluateSkillInSearching(grid, skill, out action, out score);
        }

        return TryEvaluateSkillInTargeting(grid, skill, defaultTargetPos, energy, out action, out score);
    }

    private bool TryEvaluateSkillInSearching(IGridSystem grid, DuckSkillSO skill, out AIAction action, out float score)
    {
        action = default;
        score = 0f;

        int skillArea = skill.areaSize.x * skill.areaSize.y;
        if (skillArea <= 1)
        {
            return false;
        }

        int bestCoverage = -1;
        Vector2Int bestPos = Vector2Int.zero;

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                Vector2Int pivot = new Vector2Int(x, y);
                if (!grid.IsValidPosition(pivot))
                {
                    continue;
                }

                List<Vector2Int> affected = skill.GetAffectedPositions(pivot, grid);
                int coverage = 0;

                for (int i = 0; i < affected.Count; i++)
                {
                    GridCell cell = grid.GetCell(affected[i]);
                    if (cell != null && !cell.IsHit)
                    {
                        coverage++;
                    }
                }

                if (coverage > bestCoverage)
                {
                    bestCoverage = coverage;
                    bestPos = pivot;
                }
            }
        }

        if (bestCoverage <= 0)
        {
            return false;
        }

        action = AIAction.Skill(bestPos, skill);
        score = 10f + bestCoverage;
        Debug.Log($"[AI - Searching] Evaluated skill {skill.skillName} at {bestPos} with coverage {bestCoverage} and score {score}");
        return true;
    }

    private bool TryEvaluateSkillInTargeting(IGridSystem grid, DuckSkillSO skill, Vector2Int targetPos, DuckEnergySystem energy, out AIAction action, out float score)
    {
        action = default;
        score = 0f;

        if (skill.targetType == SkillTargetType.Self)
        {
            return false;
        }

        if (!grid.IsValidPosition(targetPos))
        {
            return false;
        }

        List<Vector2Int> affected = skill.GetAffectedPositions(targetPos, grid);
        int potentialHits = 0;

        for (int i = 0; i < affected.Count; i++)
        {
            GridCell cell = grid.GetCell(affected[i]);
            if (cell != null && !cell.IsHit)
            {
                potentialHits++;
            }
        }

        if (potentialHits <= 0)
        {
            return false;
        }

        float baseScore = 12f;
        float hitWeight = 2f;
        float energyBias = Mathf.Clamp((float)energy.CurrentEnergy / Mathf.Max(1, skill.energyCost), 0.5f, 2f);

        score = baseScore + potentialHits * hitWeight * energyBias;
        action = AIAction.Skill(targetPos, skill);
        Debug.Log($"[AI - Targeting] Evaluated skill {skill.skillName} at {targetPos} with potentialHits {potentialHits} and score {score}");
        return true;
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
