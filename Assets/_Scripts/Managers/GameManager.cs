using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour, IGameContext
{
    [Header("--- COMPONENTS ---")]
    [SerializeField] private GridManager playerGrid;
    [SerializeField] private GridManager enemyGrid;
    [SerializeField] private FleetManager fleetManager;

    [SerializeField] private GridInputController gridInputController;

    [Header("--- SETTINGS ---")]
    [SerializeField] private bool vsAI = true; // Biến này để mở rộng sau này (PvP hoặc PvE)

    // --- STATE MANAGEMENT ---
    private GameStateBase _currentState;

    // Cache các state để không phải new liên tục (Optimization)
    private SetupState _setupState;
    private BattleState _battleState;

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        // 1. Khởi tạo dữ liệu cho 2 Grid (Logic Core)

        playerGrid.Initialize(new GridSystem(10, 10), Owner.Player);
        enemyGrid.Initialize(new GridSystem(10, 10), Owner.Enemy);

        IEnemyAI aiImplementation = new EnemyAIController();
        // 2. Lắng nghe sự kiện click từ View (GridManager)

        playerGrid.OnGridClicked += HandleGridInteraction;
        enemyGrid.OnGridClicked += HandleGridInteraction;

        // 3. Khởi tạo các States 

        _setupState = new SetupState(this, playerGrid, fleetManager, gridInputController);
        _battleState = new BattleState(this, playerGrid, enemyGrid, aiImplementation, gridInputController); 

        // 4. Bắt đầu game bằng Setup State
        ChangeState(_setupState);
    }

    private void OnDestroy()
    {
        if (playerGrid != null) playerGrid.OnGridClicked -= HandleGridInteraction;
        if (enemyGrid != null) enemyGrid.OnGridClicked -= HandleGridInteraction;
    }

    // --- STATE MACHINE CONTROL ---

    public void ChangeState(GameStateBase newState)
    {
        if (_currentState == newState) return;

        if (_currentState != null)
        {
            _currentState.ExitState();
        }

        _currentState = newState;

        if (_currentState != null)
        {
            _currentState.EnterState();
        }
    }

    // Hàm trung gian nhận Input từ GridManager và đẩy vào State hiện tại
    private void HandleGridInteraction(IGridContext sourceGrid, Vector2Int gridPos)
    {
        if (_currentState != null)
        {
            _currentState.OnGridInteraction(sourceGrid, gridPos);
        }
    }

    // --- IMPLEMENT IGameContext ---

    // 1. State báo Setup xong -> Chuyển sang Battle
    public void EndSetupPhase()
    {
        Debug.Log("GameManager: Setup finished. Starting Battle...");

        // Có thể thêm delay hoặc animation chuyển cảnh ở đây
        StartCoroutine(TransitionToBattle());
    }

    private IEnumerator TransitionToBattle()
    {
        yield return new WaitForSeconds(1.0f); // Delay nhẹ cho mượt
        ChangeState(_battleState);
    }

    // 2. State báo Game Over -> Hiển thị kết quả
    public void EndGame(bool playerWon)
    {
        Debug.Log(playerWon ? "Game Over: VICTORY!" : "Game Over: DEFEAT!");

        // Logic hiển thị UI EndGame ở đây
        // if (playerWon) winPanel.SetActive(true); else losePanel.SetActive(true);

        // Ngắt input bằng cách set state về null hoặc một EndGameState
        _currentState = null;
    }

    // 3. Coroutine: MonoBehaviour đã có sẵn, nhưng Interface yêu cầu nên ta để tường minh
    // (Thực tế không cần viết lại vì MonoBehaviour đã implement rồi, nhưng để rõ ràng):
    public new Coroutine StartCoroutine(IEnumerator routine)
    {
        return base.StartCoroutine(routine);
    }
}