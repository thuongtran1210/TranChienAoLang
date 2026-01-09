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
    [SerializeField] private BattleUIManager _battleUIManager;

    [SerializeField] private CameraController cameraController;
    [SerializeField] private GridInputController gridInputController;
    [Header("--- ENERGY SYSTEMS ---")]
    [SerializeField] private DuckEnergySystem _playerEnergySystem;
    [SerializeField] private DuckEnergySystem _enemyEnergySystem;

    [SerializeField] private DuckDataSO _tempPlayerData;

    //Event Channels
    [SerializeField] private BattleEventChannelSO battleEventChannel;



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
        _battleState = new BattleState(
        this,
        _playerGrid,
        _enemyGrid,
        aiImplementation,
        gridInputController,
        battleEventChannel,
        _playerEnergySystem, // <--- Inject Player System
        _enemyEnergySystem   // <--- Inject Enemy System
    );

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
        OnSetupComplete();

        // Có thể thêm delay hoặc animation chuyển cảnh ở đây
        StartCoroutine(TransitionToBattle());
    }

    public void OnSetupComplete()
    {
        Debug.Log("Setup Complete! Initializing Battle Phase...");

        // 1. LẤY DỮ LIỆU TỪ FLEET MANAGER
        DuckDataSO activeDuckData = fleetManager.GetPlayerActiveDuckData();

        // 2. CHECK NULL (Fail Fast Principle)
        if (activeDuckData == null)
        {
            Debug.LogError("GameManager: Cannot start Battle UI because Duck Data is missing!");
            // Có thể return hoặc xử lý lỗi tùy game design
        }

        // 3. KHỞI TẠO BATTLE UI VỚI DỮ LIỆU VỪA LẤY
        if (_battleUIManager != null)
        {
            // Truyền Data vào hàm Initialize mà chúng ta đã viết ở bước trước
            _battleUIManager.InitializeBattleUI(activeDuckData);
        }


   
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
    public void TriggerSkillSelection(DuckSkillSO skill)
    {
        // Kiểm tra an toàn: Chỉ cho dùng skill khi đang ở BattleState
        // Cách 1: So sánh biến state
        if (_currentState == _battleState && _battleState != null)
        {
            _battleState.SelectSkill(skill);
        }
        // Cách 2 (An toàn hơn): Kiểm tra kiểu dữ liệu
        else if (_currentState is BattleState currentBattleState)
        {
            currentBattleState.SelectSkill(skill);
        }
        else
        {
            Debug.LogWarning("Không thể chọn Skill: Không phải lượt Battle hoặc State chưa khởi tạo!");
        }
    }
}