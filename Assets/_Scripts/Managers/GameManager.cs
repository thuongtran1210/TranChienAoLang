using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;

public class GameManager : MonoBehaviour, IGameContext
{
    [Header("--- COMPONENTS ---")]
    [SerializeField] private GridManager playerGrid;
    [SerializeField] private GridManager enemyGrid;
    [SerializeField] private FleetManager fleetManager;
    [SerializeField] private GridRandomizer gridRandomizer;

    [SerializeField] private CameraController cameraController;
    [SerializeField] private GridInputController gridInputController;

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

      
        gridInputController.RegisterGrid(playerGrid);
        gridInputController.RegisterGrid(enemyGrid);

        // 2. Khởi tạo dữ liệu Grid
        playerGrid.Initialize(new GridSystem(10, 10), Owner.Player);
        enemyGrid.Initialize(new GridSystem(10, 10), Owner.Enemy);

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
  
        _setupState = new SetupState(this, playerGrid, fleetManager, gridInputController);

        // BattleState 
        _battleState = new BattleState(this, playerGrid, enemyGrid, aiImplementation, gridInputController);

        // 6. Setup Enemy Fleet 
        List<DuckDataSO> enemyFleet = fleetManager.GetFleetData();
        gridRandomizer.RandomizePlacement(enemyGrid, enemyFleet);

        // 7. Start Game
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