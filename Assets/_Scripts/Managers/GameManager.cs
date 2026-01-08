using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;

public class GameManager : MonoBehaviour, IGameContext
{
    [Header("--- COMPONENTS ---")]
    [SerializeField] private GridController playerGridManager;
    [SerializeField] private GridController enemyGridManager;
    [SerializeField] private FleetManager fleetManager;
    [SerializeField] private GridRandomizer gridRandomizer;

    [SerializeField] private CameraController cameraController;
    [SerializeField] private GridInputController gridInputController;

    private IGridContext _playerGrid => playerGridManager;
    private IGridContext _enemyGrid => enemyGridManager;

    [Header("--- SETTINGS ---")]
    [SerializeField] private bool vsAI = true; 

    // --- STATE MANAGEMENT ---
    private GameStateBase _currentState;

    // Cache state 
    private SetupState _setupState;
    private BattleState _battleState;

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        // 1. SETUP HỆ THỐNG INPUT 
        if (cameraController != null)
        {
            gridInputController.Initialize(cameraController.GetCamera());
        }
        else
        {
            Debug.LogError("GameManager: Chưa gán CameraController trong Inspector!");
            gridInputController.Initialize(Camera.main);
        }


        gridInputController.RegisterGrid(_playerGrid);
        gridInputController.RegisterGrid(_enemyGrid);

        // 2. Khởi tạo dữ liệu Grid
        playerGridManager.Initialize(new GridSystem(10, 10), Owner.Player);
        enemyGridManager.Initialize(new GridSystem(10, 10), Owner.Enemy);

        // 3. Setup AI
        EnemyAIController aiController = new EnemyAIController();
        aiController.Initialize(10, 10);
        IEnemyAI aiImplementation = aiController;

        // 4. Setup Camera 
        if (cameraController != null)
        {
            cameraController.SetupCamera(10, 10);
        }

        // 5. Khởi tạo States

        _setupState = new SetupState(this, _playerGrid, fleetManager, gridInputController);

        // BattleState 
        _battleState = new BattleState(this, _playerGrid, _enemyGrid, aiImplementation, gridInputController);

        // 6. Setup Enemy Fleet 
        List<DuckDataSO> enemyFleet = fleetManager.GetFleetData();
        gridRandomizer.RandomizePlacement(enemyGridManager, enemyFleet);

        // 7. Start Game
        ChangeState(_setupState);

    }

    private void OnDestroy()
    {
        if (_playerGrid != null) _playerGrid.OnGridClicked -= HandleGridInteraction;
        if (_enemyGrid != null) _enemyGrid.OnGridClicked -= HandleGridInteraction;
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
    private void HandleGridInteraction(IGridLogic sourceGrid, Vector2Int gridPos)
    {
        // SENIOR PATTERN: Pattern Matching & Casting
        // Chúng ta kiểm tra xem sourceGrid (Logic) có phải là IGridContext (Logic + Visuals) không.
        // Nếu đúng, ta cast nó sang biến 'context' và truyền vào State.
        if (sourceGrid is IGridContext context)
        {
            if (_currentState != null)
            {
                _currentState.OnGridInteraction(context, gridPos);
            }
        }
        else
        {
            Debug.LogError("Grid Logic received does not implement IGridContext!");
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