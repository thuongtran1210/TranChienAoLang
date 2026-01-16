using UnityEngine;

public class UIFlowManager : MonoBehaviour
{
    [Header("Root Panels")]
    [SerializeField] private GameObject _mainMenuRoot;
    [SerializeField] private GameObject _setupRoot;
    [SerializeField] private BattleUIManager _battleUIManager;

    public void ShowMainMenu()
    {
        if (_mainMenuRoot != null)
            _mainMenuRoot.SetActive(true);

        if (_setupRoot != null)
            _setupRoot.SetActive(false);

        if (_battleUIManager != null)
            _battleUIManager.Hide();
    }

    public void ShowSetup()
    {
        if (_mainMenuRoot != null)
            _mainMenuRoot.SetActive(false);

        if (_setupRoot != null)
            _setupRoot.SetActive(true);

        if (_battleUIManager != null)
            _battleUIManager.Hide();
    }

    public void ShowBattle()
    {
        if (_mainMenuRoot != null)
            _mainMenuRoot.SetActive(false);

        if (_setupRoot != null)
            _setupRoot.SetActive(false);

        if (_battleUIManager != null)
            _battleUIManager.Show();
    }
}

