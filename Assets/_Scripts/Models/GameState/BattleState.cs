using System.Collections;
using UnityEngine;

public class BattleState : GameStateBase
{
    private IGridContext _playerGrid;
    private IGridContext _enemyGrid;
    private IEnemyAI _enemyAI;

    private GridInputChannelSO _gridInputChannel;
    private BattleEventChannelSO _battleEvents;
    private DuckEnergySystem _playerEnergy;
    private DuckEnergySystem _enemyEnergy;
    private DuckSkillSO _currentSelectedSkill;

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
        _balanceConfig = balanceConfig;
        _playerEnergy = playerEnergy;
        _enemyEnergy = enemyEnergy;
    }

    public override void EnterState()
    {
        Debug.Log("--- BATTLE START ---");
        _isPlayerTurn = true;
        _isGameOver = false;


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
        if (_isGameOver || !_isPlayerTurn) return;
        if (owner != Owner.Enemy) return;

        if (_pendingSkill != null)
        {
            _gameContext.StartCoroutine(ExecuteSkillRoutine(gridPos));
        }
        else
        {
            ExecuteNormalShot(gridPos);
        }
    }
    // --- GAMEPLAY LOGIC ---

    private IEnumerator ExecuteSkillRoutine(Vector2Int targetPos)
    {
        // Execute Logic
        bool success = _pendingSkill.Execute(_enemyGrid.GridSystem, targetPos, _battleEvents, Owner.Enemy);

        if (success)
        {
            _playerEnergy.TryConsumeEnergy(_pendingSkill.energyCost);
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
        if (_enemyGrid.GridSystem.GetCell(targetPos).IsHit) return;
        ProcessShot(_enemyGrid, targetPos, Owner.Player);
    }


    public override void OnGridInteraction(IGridContext source, Vector2Int gridPos)
    {
        // Không dùng nữa, xử lý trong HandleCellClicked
    }


    // Bắn thường 
    private void ProcessShot(IGridContext targetGrid, Vector2Int pos, Owner shooter)
    {
        ShotResult result = targetGrid.GridSystem.ShootAt(pos);
        _battleEvents.RaiseShotFired(shooter, result, pos);

        if (CheckWinCondition(targetGrid))
        {
            EndBattle(targetGrid == _enemyGrid);
            return;
        }

        if (result == ShotResult.Miss)
        {
            SwitchTurn();
        }
        else
        {
            // HIT logic: Bắn tiếp
            Debug.Log($"{shooter} Hit! Shoot again.");
            if (shooter == Owner.Enemy)
            {
                _enemyAI.NotifyHit(pos, targetGrid.GridSystem);
                // Enemy bắn trúng thì bắn tiếp sau 1 khoảng delay nhỏ
                _gameContext.StartCoroutine(EnemyRoutine());
            }
        }
    }

    private void SwitchTurn()
    {
        _isPlayerTurn = !_isPlayerTurn;
        Debug.Log($"Turn Switch: {(_isPlayerTurn ? "PLAYER" : "ENEMY")}");

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
        // Delay trước khi AI bắn để tạo cảm giác tự nhiên
        float aiDelay = _balanceConfig != null ? _balanceConfig.EnemyTurnDelay : 1.0f;
        yield return new WaitForSeconds(aiDelay);

        if (!_isGameOver) // Check lại vì có thể game over trong lúc wait
        {
            Vector2Int aiTarget = _enemyAI.GetNextTarget(_playerGrid.GridSystem);
            ProcessShot(_playerGrid, aiTarget, Owner.Enemy);
        }
    }

    private bool CheckWinCondition(IGridContext gridContext)
    {
        return gridContext.GridSystem.IsAllShipsSunk;
    }
    public void SelectSkill(DuckSkillSO skill)
    {
        if (!_isPlayerTurn) return;

        if (_playerEnergy.CurrentEnergy < skill.energyCost)
        {
            _battleEvents.RaiseSkillFeedback("Not enough energy!", Vector2Int.zero);
            return;
        }

        _pendingSkill = skill;
        _battleEvents.RaiseSkillSelected(skill);
    }
}