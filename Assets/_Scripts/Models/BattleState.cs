using System.Collections;
using UnityEngine;

public class BattleState : GameStateBase
{
    private IGridContext _playerGrid;
    private IGridContext _enemyGrid;
    private IEnemyAI _enemyAI;
    private GridInputController _inputController;

    private BattleEventChannelSO _battleEvents;
    private DuckEnergySystem _playerEnergy;
    private DuckEnergySystem _enemyEnergy;
    private DuckSkillSO _currentSelectedSkill;

    private bool _isPlayerTurn;
    private bool _isGameOver;

    private DuckSkillSO _pendingSkill; // Skill đang được chọn chờ target

    public BattleState(IGameContext context,
                           IGridContext playerGrid,
                           IGridContext enemyGrid,
                           IEnemyAI enemyAI,
                           GridInputController inputController,
                           BattleEventChannelSO battleEvents, DuckEnergySystem playerEnergy, DuckEnergySystem enemyEnergy)
                : base(context)
    {
        _playerGrid = playerGrid;
        _enemyGrid = enemyGrid;
        _enemyAI = enemyAI;
        _inputController = inputController;
        _battleEvents = battleEvents;
        _playerEnergy = playerEnergy;
        _enemyEnergy = enemyEnergy;
    }

    public override void EnterState()
    {
        Debug.Log("--- BATTLE START ---");
        _isPlayerTurn = true;
        _isGameOver = false;


        _inputController.OnGridCellClicked += HandleInput;
        _battleEvents.OnSkillRequested += SelectSkill;
    }

    public override void ExitState()
    {
      
        _inputController.OnGridCellClicked -= HandleInput;
        _battleEvents.OnSkillRequested -= SelectSkill;
    }
    private void HandleInput(Vector2Int gridPos, Owner owner)
    {
        if (_isGameOver || !_isPlayerTurn) return;

        // Validate cơ bản: Luôn phải click vào Enemy Grid
        if (owner != Owner.Enemy)
        {
            // Nếu click vào nhà mình thì hủy chọn skill (UX)
            if (_pendingSkill != null)
            {
                _pendingSkill = null;
                Debug.Log("Cancelled Skill.");
            }
            return;
        }

        // --- LOGIC MỚI: XỬ LÝ SKILL ---
        if (_pendingSkill != null)
        {
            CastSkill(gridPos);
            return; // Dừng, không xử lý bắn thường
        }

        // --- LOGIC CŨ: BẮN THƯỜNG ---
        if (_enemyGrid.GridSystem.GetCell(gridPos).IsHit) return;
        ProcessShot(_enemyGrid, gridPos, Owner.Player);
    }
    private void CastSkill(Vector2Int targetPos)
    {
        // 1. Thực thi logic Skill (Nằm trong SO)
        bool success = _pendingSkill.Execute(_enemyGrid.GridSystem, targetPos, _battleEvents);

        if (success)
        {
            // 2. Trừ Energy
            _playerEnergy.TryConsumeEnergy(_pendingSkill.energyCost); // Bạn cần viết hàm này bên DuckEnergySystem

            // 3. Reset trạng thái
            Debug.Log($"Casted {_pendingSkill.skillName} at {targetPos}");
            _pendingSkill = null;

            // 4. (Optional) Skill có tốn lượt không?
            // Nếu tốn lượt -> SwitchTurn();
            // Nếu không tốn lượt -> Player bắn tiếp.
        }
        else
        {
            // Skill thất bại (do logic trong SO trả về false), giữ nguyên trạng thái chọn skill
            Debug.Log("Skill cast failed (Invalid target?). Try again.");
        }
    }


    public override void OnGridInteraction(IGridContext source, Vector2Int gridPos)
    { 
        if (_isGameOver || !_isPlayerTurn) return;

        // Player chỉ được bắn vào Enemy Grid
        if (source != _enemyGrid) return;

        // Validate: Ô đã bắn chưa?
        if (_enemyGrid.GridSystem.GetCell(gridPos).IsHit) return;

        ProcessShot(_enemyGrid, gridPos, Owner.Player);
    }

    private void ProcessShot(IGridContext targetGrid, Vector2Int pos, Owner shooter)
    {
        // 1. Logic bắn
        ShotResult result = targetGrid.GridSystem.ShootAt(pos);

        _battleEvents.RaiseShotFired(shooter, result, pos);

        // 2. Check Win Condition
        if (CheckWinCondition(targetGrid))
        {
            _isGameOver = true;
            bool playerWon = (targetGrid == _enemyGrid);
            _gameContext.EndGame(playerWon);
            return;
        }

        // 3. Xử lý lượt
        if (result == ShotResult.Miss)
        {
            SwitchTurn();
        }
        else // HIT hoặc SUNK
        {
            Debug.Log($"{shooter} Hit! Shoot again."); // Log rõ ai bắn trúng

            // Logic bắn bồi (Shoot again)
            if (shooter == Owner.Enemy) // Nếu Enemy bắn trúng
            {
                _enemyAI.NotifyHit(pos, targetGrid.GridSystem);
                _gameContext.StartCoroutine(EnemyRoutine());
            }
            // Nếu Player bắn trúng thì không cần làm gì, đợi Input tiếp theo
        }
    }

    private void SwitchTurn()
    {
        _isPlayerTurn = !_isPlayerTurn;
        Debug.Log($"Turn Switch: {(_isPlayerTurn ? "PLAYER" : "ENEMY")}");

        if (!_isPlayerTurn)
        {
            Debug.Log("Enemy's turn...");
            _gameContext.StartCoroutine(EnemyRoutine());
        }
    }

    private IEnumerator EnemyRoutine()
    {
        yield return new WaitForSeconds(1.0f);

      
        Vector2Int aiTarget = _enemyAI.GetNextTarget(_playerGrid.GridSystem);

        ProcessShot(_playerGrid, aiTarget, Owner.Enemy);
    }

    private bool CheckWinCondition(IGridContext gridContext)
    {
        return gridContext.GridSystem.IsAllShipsSunk;
    }
    public void SelectSkill(DuckSkillSO skill)
    {
        if (!_isPlayerTurn) return;

        // Check Energy trước khi cho phép chọn (Fail Fast)
        if (_playerEnergy.CurrentEnergy < skill.energyCost)
        {
            Debug.Log("Not enough energy!");
            _battleEvents.RaiseSkillFeedback("Not enough energy!", Vector2Int.zero);
            return;
        }

        _pendingSkill = skill;
        Debug.Log($"Selected Skill: {skill.skillName}. Select target on Enemy Grid.");
        // TODO: Đổi cursor chuột hoặc highlight grid enemy để báo hiệu đang aim skill
    }
}