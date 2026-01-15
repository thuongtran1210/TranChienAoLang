using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;

public class GameManager : MonoBehaviour, IGameContext
{
    [Header("--- COMPONENTS ---")]
    [SerializeField] private GridController _playerGridManager;
    [SerializeField] private GridController _enemyGridManager;
    [SerializeField] private FleetManager _fleetManager;
    [SerializeField] private GridRandomizer gridRandomizer;
    [SerializeField] private BattleUIManager _battleUIManager;
 

    [SerializeField] private CameraController cameraController;
    [SerializeField] private GridInputController _gridInputController;
    [Header("--- ENERGY SYSTEMS ---")]
    [SerializeField] private DuckEnergySystem _playerEnergySystem;
    [SerializeField] private DuckEnergySystem _enemyEnergySystem;


    [Header("--- EVENTS CHANEL ---")]
    [SerializeField] private BattleEventChannelSO _battleEventChannel;
    [SerializeField] private GridInputChannelSO _gridInputChannel;

    [Header("--- LAYOUT CONFIG ---")]
    [Tooltip("Khoảng cách giữa 2 bàn cờ")]
    [SerializeField] private float _distanceBetweenBoards = 15f;

    [Header("--- GAME BALANCE CONFIG ---")]
    [SerializeField] private GameBalanceConfigSO _gameBalanceConfig;

    private IGridContext _playerGrid => _playerGridManager;
    private IGridContext _enemyGrid => _enemyGridManager;

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
            _gridInputController.Initialize(cameraController.GetCamera());
        }
        else
        {
            Debug.LogError("GameManager: Chưa gán CameraController trong Inspector!");
            _gridInputController.Initialize(Camera.main);
        }


        _gridInputController.RegisterGrid(_playerGrid);
        _gridInputController.RegisterGrid(_enemyGrid);

        float offset = _distanceBetweenBoards / 2f;
        // 2. Khởi tạo dữ liệu Grid

        _enemyGridManager.transform.position = new Vector3(offset, 0, 0);
        _enemyGridManager.InitializeGrid(new GridSystem(10, 10), Owner.Enemy);

        _playerGridManager.transform.position = new Vector3(-offset, 0, 0);
        _playerGridManager.InitializeGrid(new GridSystem(10, 10), Owner.Player);

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

        _setupState = new SetupState(this, _playerGrid, _fleetManager, _gridInputController, _gridInputChannel);
        _battleState = new BattleState(this, _playerGrid, _enemyGrid, aiController, _gridInputChannel, _battleEventChannel,_gameBalanceConfig, _playerEnergySystem, _enemyEnergySystem);
        // 6. Setup Enemy Fleet 
        List<DuckDataSO> enemyFleet = _fleetManager.GetFleetData();
        gridRandomizer.RandomizePlacement(_enemyGridManager, enemyFleet);

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
        Debug.Log("GameManager: Setup finished. Starting Battle..."); // Log 1

        // Thêm try-catch để đảm bảo lỗi logic bên trong không chặn luồng
        try
        {
            OnSetupComplete();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Error in OnSetupComplete: {e.Message}");
        }

        StartCoroutine(TransitionToBattle());
    }

    public void OnSetupComplete()
    {
        Debug.Log("Setup Complete! Initializing Battle Phase...");

        List<DuckSkillSO> playerSkills = _fleetManager.GetPlayerSkillsForBattle();

        if (playerSkills == null || playerSkills.Count == 0)
        {
            Debug.LogError("GameManager: Cannot start Battle UI because no skills were found for player fleet!");
        }

        if (_battleUIManager != null)
        {
            _battleUIManager.InitializeBattleUI(playerSkills);
        }
    }
    private IEnumerator TransitionToBattle()
    {
        yield return new WaitForSeconds(1.0f); 
        ChangeState(_battleState);
    }

    // 2. State báo Game Over -> Hiển thị kết quả
    public void EndGame(bool playerWon)
    {
        Debug.Log(playerWon ? "Game Over: VICTORY!" : "Game Over: DEFEAT!");

        _currentState = null;
    }

    // 3. Coroutine

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