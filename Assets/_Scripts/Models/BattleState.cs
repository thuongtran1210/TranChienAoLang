using System.Collections;
using UnityEngine;

public class BattleState : GameStateBase
{
    private IGridContext _playerGrid;
    private IGridContext _enemyGrid;
    private IEnemyAI _enemyAI;
    private GridInputController _inputController;
    private bool _isPlayerTurn;
    private bool _isGameOver;

    public BattleState(IGameContext context,
                           IGridContext playerGrid,
                           IGridContext enemyGrid,
                           IEnemyAI enemyAI,
                           GridInputController inputController) 
                : base(context)
    {
        _playerGrid = playerGrid;
        _enemyGrid = enemyGrid;
        _enemyAI = enemyAI;
        _inputController = inputController;
    }

    public override void EnterState()
    {
        Debug.Log("--- BATTLE START ---");
        _isPlayerTurn = true;
        _isGameOver = false;


        _inputController.OnGridCellClicked += HandleInput;
    }

    public override void ExitState()
    {
      
        _inputController.OnGridCellClicked -= HandleInput;
    }
    private void HandleInput(Vector2Int gridPos, Owner owner)
    {
        if (_isGameOver || !_isPlayerTurn) return;

        // Validation: Player chỉ được click vào bảng Enemy để bắn
        if (owner != Owner.Enemy)
        {
            Debug.LogWarning("Phải bắn vào bảng đối thủ (Enemy)!");
            return;
        }

        // Logic cũ của bạn giữ nguyên, chỉ thay đổi đầu vào
        if (_enemyGrid.GridSystem.GetCell(gridPos).IsHit)
        {
            Debug.Log("Ô này bắn rồi!");
            return;
        }

        ProcessShot(_enemyGrid, gridPos);
    }

    public override void OnGridInteraction(IGridContext source, Vector2Int gridPos)
    {
        if (_isGameOver || !_isPlayerTurn) return;

        // Player chỉ được bắn vào Enemy Grid
        if (source != _enemyGrid) return;

        // Validate: Ô đã bắn chưa?
        if (_enemyGrid.GridSystem.GetCell(gridPos).IsHit) return;

        ProcessShot(_enemyGrid, gridPos);
    }

    private void ProcessShot(IGridContext targetGrid, Vector2Int pos)
    {
        // 1. Logic bắn (Data)
        ShotResult result = targetGrid.GridSystem.ShootAt(pos);

        // 2. Check Win Condition
        if (CheckWinCondition(targetGrid))
        {
            _isGameOver = true;
            bool playerWon = (targetGrid == _enemyGrid); // Nếu Grid bị bắn nát là Enemy -> Player thắng
            _gameContext.EndGame(playerWon);
            return;
        }

        // 3. Xử lý lượt (Logic Game Design)
        if (result == ShotResult.Miss)
        {
            SwitchTurn();
        }
        else // HIT hoặc SUNK
        {
            Debug.Log("Hit! Shoot again.");

           
            if (!_isPlayerTurn)
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

        if (!_isPlayerTurn)
        {
            // Đến lượt Enemy -> Gọi Coroutine qua GameContext
            _gameContext.StartCoroutine(EnemyRoutine());
        }
    }

    private IEnumerator EnemyRoutine()
    {
        yield return new WaitForSeconds(1.0f);

        // State không quan tâm AI là ai, chỉ cần biết nó trả về target
        Vector2Int aiTarget = _enemyAI.GetNextTarget(_playerGrid.GridSystem);

        ProcessShot(_playerGrid, aiTarget);
    }

    private bool CheckWinCondition(IGridContext grid)
    {
        // Logic check tất cả thuyền chìm
        // Đây là ví dụ, bạn nên cài đặt biến đếm trong GridSystem
        return false;
    }
}