using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleState : GameStateBase
{
    private IGridContext _playerGrid;
    private IGridContext _enemyGrid;
    private IEnemyAI _enemyAI;

    private GridInputChannelSO _gridInputChannel;
    private BattleEventChannelSO _battleEvents;
    private GameFlowEventChannelSO _flowEvents;
    private DuckEnergySystem _playerEnergy;
    private DuckEnergySystem _enemyEnergy;
    private List<DuckSkillSO> _enemyAvailableSkills = new List<DuckSkillSO>();
    private DuckSkillSO _currentSelectedSkill;
    private readonly Dictionary<DuckSkillSO, int> _playerCooldowns = new Dictionary<DuckSkillSO, int>();

    private readonly GameBalanceConfigSO _balanceConfig;

    private bool _isPlayerTurn;
    private bool _isGameOver;

    private DuckSkillSO _pendingSkill; 

    public BattleState(IGameContext context,
                               IGridContext playerGrid,
                               IGridContext enemyGrid,
                               IEnemyAI enemyAI,
                               GridInputChannelSO gridInputChannel, 
                               BattleEventChannelSO battleEvents,
                               GameFlowEventChannelSO flowEvents,
                               GameBalanceConfigSO balanceConfig,
                               DuckEnergySystem playerEnergy,
                               DuckEnergySystem enemyEnergy)
                    : base(context)
    {
        _playerGrid = playerGrid;
        _enemyGrid = enemyGrid;
        _enemyAI = enemyAI;
        _gridInputChannel = gridInputChannel;
        _battleEvents = battleEvents;
        _flowEvents = flowEvents;
        _balanceConfig = balanceConfig;
        _playerEnergy = playerEnergy;
        _enemyEnergy = enemyEnergy;
    }

    public override void EnterState()
    {
        Debug.Log("--- BATTLE START ---");
        _isPlayerTurn = true;
        _isGameOver = false;
        _playerCooldowns.Clear();
        if (_flowEvents != null)
            _flowEvents.RaiseTurnChanged(Owner.Player);


        _gridInputChannel.OnGridCellClicked += HandleCellClicked;
        _gridInputChannel.OnRotateAction += HandleCancelSkill;
        _battleEvents.OnSkillRequested += HandleSkillRequested;

    }

    public override void ExitState()
    {

        _gridInputChannel.OnGridCellClicked -= HandleCellClicked;
        _gridInputChannel.OnRotateAction += HandleCancelSkill;
        _battleEvents.OnSkillRequested -= HandleSkillRequested;
    }
    // --- INPUT HANDLERS ---

    private void HandleSkillRequested(DuckSkillSO skill)
    {
        if (!_isPlayerTurn || _isGameOver) return;

        if (GetPlayerCooldownRemaining(skill) > 0)
        {
            _battleEvents.RaiseSkillFeedback("Skill is on cooldown!", Vector2Int.zero);
            return;
        }

        if (_playerEnergy.CurrentEnergy < skill.energyCost)
        {
            _battleEvents.RaiseSkillFeedback("Not enough energy!", Vector2Int.zero);
            return;
        }

        _pendingSkill = skill;
        _battleEvents.RaiseSkillSelected(skill);
    }
    private void HandleCancelSkill()
    {
        if (_pendingSkill == null) return;

        _pendingSkill = null;
        _battleEvents.RaiseSkillDeselected();
        _battleEvents.RaiseClearHighlight();
        Debug.Log("Skill cancelled.");
    }

    // Nếu đang PendingSkill -> Cast Skill. Nếu không -> Bắn thường. 
    private void HandleCellClicked(Vector2Int gridPos, Owner owner)
    {
        if (_isGameOver) return;
        if (!_isPlayerTurn)
        {
            _battleEvents.RaiseSkillFeedback("Not your turn!", Vector2Int.zero);
            return;
        }

        if (_pendingSkill != null)
        {
            if (!IsValidTargetGrid(owner, _pendingSkill.targetType))
            {
                _battleEvents.RaiseSkillFeedback("Invalid target grid!", gridPos);
                return;
            }

            IGridContext targetGrid = owner == Owner.Player ? _playerGrid : _enemyGrid;
            _gameContext.StartCoroutine(ExecuteSkillRoutine(targetGrid, owner, gridPos));
        }
        else
        {
            if (owner != Owner.Enemy)
            {
                _battleEvents.RaiseSkillFeedback("You can only shoot the enemy grid!", gridPos);
                return;
            }

            ExecuteNormalShot(gridPos);
        }
    }
    // --- GAMEPLAY LOGIC ---

    private IEnumerator ExecuteSkillRoutine(IGridContext targetGrid, Owner targetOwner, Vector2Int targetPos)
    {
        DuckSkillSO skillToExecute = _pendingSkill;
        if (skillToExecute == null)
            yield break;

        // Execute Logic
        bool success = skillToExecute.Execute(targetGrid.GridSystem, targetPos, _battleEvents, targetOwner);

        if (success)
        {
            _playerEnergy.TryConsumeEnergy(skillToExecute.energyCost);
            SetPlayerCooldown(skillToExecute, skillToExecute.cooldownTurns);
            _pendingSkill = null;

            // Sử dụng Config cho thời gian delay
            float skillDelay = _balanceConfig != null ? _balanceConfig.SkillExecutionDelay : 2.0f;
            yield return new WaitForSeconds(skillDelay);

            _battleEvents.RaiseSkillDeselected();
            _battleEvents.RaiseClearHighlight();

            // Nếu skill tốn lượt (hiện tại logic bạn comment SwitchTurn, tôi giữ nguyên)
            // SwitchTurn(); 
        }
        else
        {
            _battleEvents.RaiseSkillFeedback("Invalid Target!", targetPos);
        }
    }

    private void ExecuteNormalShot(Vector2Int targetPos)
    {
        GridCell cell = _enemyGrid.GridSystem.GetCell(targetPos);
        if (cell == null) return;
        if (cell.IsHit)
        {
            _battleEvents.RaiseSkillFeedback("Cell already shot!", targetPos);
            return;
        }
        ProcessShot(_enemyGrid, targetPos, Owner.Player);
    }

    private bool IsValidTargetGrid(Owner gridOwner, SkillTargetType skillTargetType)
    {
        return skillTargetType switch
        {
            SkillTargetType.Self => gridOwner == Owner.Player,
            SkillTargetType.Enemy => gridOwner == Owner.Enemy,
            SkillTargetType.Any => true,
            _ => false
        };
    }


    public override void OnGridInteraction(IGridContext source, Vector2Int gridPos)
    {
        // Không dùng nữa, xử lý trong HandleCellClicked
    }


    // Bắn thường 
    private void ProcessShot(IGridContext targetGrid, Vector2Int pos, Owner shooter)
    {

        ShotResult result = targetGrid.ProcessShot(pos, shooter);

        // Nếu bắn vào ô không hợp lệ (đã bắn rồi), thì return luôn
        if (result == ShotResult.Invalid || result == ShotResult.None) return;

        // --- GAME FLOW LOGIC  ---

        // 1. Check Win
        if (CheckWinCondition(targetGrid))
        {
            EndBattle(targetGrid == _enemyGrid);
            return;
        }

        // 2. Turn Logic
        if (result == ShotResult.Miss)
        {
            SwitchTurn();
        }
        else
        {
            // Hit/Sunk -> Bonus Turn
            Debug.Log($"{shooter} Hit! Bonus Turn.");

            // Riêng AI cần thông báo để nó cập nhật Heatmap
            if (shooter == Owner.Enemy)
            {
                _enemyAI.NotifyHit(pos, targetGrid.GridSystem);
                _gameContext.StartCoroutine(EnemyRoutine());
            }
        }
    }

    private void SwitchTurn()
    {
        _isPlayerTurn = !_isPlayerTurn;
        Debug.Log($"Turn Switch: {(_isPlayerTurn ? "PLAYER" : "ENEMY")}");
        if (_flowEvents != null)
            _flowEvents.RaiseTurnChanged(_isPlayerTurn ? Owner.Player : Owner.Enemy);

        if (_isPlayerTurn)
            TickPlayerCooldowns();

        if (!_isPlayerTurn)
        {
            _gameContext.StartCoroutine(EnemyRoutine());
        }
    }
    private void EndBattle(bool isPlayerWon)
    {
        _isGameOver = true;
        _gameContext.EndGame(isPlayerWon);
    }

    private IEnumerator EnemyRoutine()
    {
        float aiDelay = _balanceConfig != null ? _balanceConfig.EnemyTurnDelay : 1.0f;
        yield return new WaitForSeconds(aiDelay);

        // Kiểm tra lại Game Over vì trạng thái có thể thay đổi trong lúc chờ
        if (!_isGameOver)
        {
            // Truyền context: Grid người chơi, Năng lượng hiện tại của AI, List Skill AI có
            AIAction action = _enemyAI.GetDecision(_playerGrid.GridSystem, _enemyEnergy, _enemyAvailableSkills);

            // Debug log để bạn dễ theo dõi hành vi AI
            if (action.Type == AIActionType.CastSkill)
                Debug.Log($"[AI TURN] Casting Skill: {action.SkillToCast.skillName} at {action.TargetPosition}");
            else
                Debug.Log($"[AI TURN] Normal Attack at {action.TargetPosition}");

            // Xử lý hành động dựa trên quyết định
            switch (action.Type)
            {
                case AIActionType.CastSkill:
                    // Nếu AI muốn dùng Skill -> Gọi Coroutine xử lý Skill
                    _gameContext.StartCoroutine(ExecuteAISkillRoutine(action.SkillToCast, action.TargetPosition));
                    break;

                case AIActionType.NormalAttack:
                default:
                    // Nếu AI bắn thường -> Gọi hàm bắn thường cũ
                    ProcessShot(_playerGrid, action.TargetPosition, Owner.Enemy);
                    break;
            }
        }
    }
    private IEnumerator ExecuteAISkillRoutine(DuckSkillSO skill, Vector2Int targetPos)
    {
        // Double-check: Trừ năng lượng
        if (_enemyEnergy.TryConsumeEnergy(skill.energyCost))
        {
            // --- EXECUTE SKILL ---
            // Lưu ý tham số: targetGrid là Grid của Player, targetOwner là Player
            bool success = skill.Execute(_playerGrid.GridSystem, targetPos, _battleEvents, Owner.Player);

            if (success)
            {
                // Delay chờ hiệu ứng skill (VFX/SFX) hoàn tất
                float skillDelay = _balanceConfig != null ? _balanceConfig.SkillExecutionDelay : 2.0f;
                yield return new WaitForSeconds(skillDelay);

                // Clear highlight sau khi skill xong (để bàn cờ sạch sẽ)
                _battleEvents.RaiseClearHighlight();
                _battleEvents.RaiseSkillDeselected();

                // LOGIC LƯỢT ĐI SAU KHI DÙNG SKILL:
                // Tùy Game Design: Dùng skill xong có được bắn tiếp không?
                // Case A: Dùng skill xong mất lượt -> SwitchTurn()
                // Case B: Dùng skill xong vẫn được bắn tiếp nếu trúng -> Check logic Hit/Miss

                // Ở đây tôi giả định dùng Skill xong là HẾT LƯỢT (để cân bằng game)
                SwitchTurn();
            }
            else
            {
                // Fallback (Phòng hờ): Nếu Skill fail logic (ví dụ target không hợp lệ), 
                // chuyển về bắn thường để không bị kẹt game.
                Debug.LogWarning("AI Skill Execution Failed! Fallback to Normal Shot.");
                ProcessShot(_playerGrid, targetPos, Owner.Enemy);
            }
        }
        else
        {
            // Fallback: Nếu tính toán sai năng lượng, bắn thường
            ProcessShot(_playerGrid, targetPos, Owner.Enemy);
        }
    }

    private bool CheckWinCondition(IGridContext gridContext)
    {
        return gridContext.GridSystem.IsAllShipsSunk;
    }
    public void SelectSkill(DuckSkillSO skill)
    {
        if (!_isPlayerTurn) return;

        if (GetPlayerCooldownRemaining(skill) > 0)
        {
            _battleEvents.RaiseSkillFeedback("Skill is on cooldown!", Vector2Int.zero);
            return;
        }

        if (_playerEnergy.CurrentEnergy < skill.energyCost)
        {
            _battleEvents.RaiseSkillFeedback("Not enough energy!", Vector2Int.zero);
            return;
        }

        _pendingSkill = skill;
        _battleEvents.RaiseSkillSelected(skill);
    }

    private int GetPlayerCooldownRemaining(DuckSkillSO skill)
    {
        if (skill == null)
            return 0;

        return _playerCooldowns.TryGetValue(skill, out int remaining) ? remaining : 0;
    }

    private void SetPlayerCooldown(DuckSkillSO skill, int cooldownTurns)
    {
        if (skill == null)
            return;

        int clamped = Mathf.Max(0, cooldownTurns);
        if (clamped <= 0)
            _playerCooldowns.Remove(skill);
        else
            _playerCooldowns[skill] = clamped;

        _battleEvents.RaiseSkillCooldownChanged(skill, clamped);
    }

    private void TickPlayerCooldowns()
    {
        if (_playerCooldowns.Count == 0)
            return;

        var keys = new List<DuckSkillSO>(_playerCooldowns.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            DuckSkillSO skill = keys[i];
            int remaining = GetPlayerCooldownRemaining(skill);
            if (remaining <= 0)
                continue;

            SetPlayerCooldown(skill, remaining - 1);
        }
    }
}
