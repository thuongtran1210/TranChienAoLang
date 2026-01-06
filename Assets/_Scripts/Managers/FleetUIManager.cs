using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FleetUIManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private FleetManager fleetManager; // Sửa reference từ GridManager sang FleetManager
    [SerializeField] private FleetDuckButton duckButtonPrefab;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Button startBattleButton;

    void Start()
    {
        if (fleetManager == null) return;

        fleetManager.OnFleetChanged += UpdateFleetUI;
        UpdateFleetUI(fleetManager.GetCurrentFleet());
    }

    private void OnDestroy()
    {
        if (fleetManager != null)
            fleetManager.OnFleetChanged -= UpdateFleetUI;
    }

    private void UpdateFleetUI(List<DuckDataSO> currentFleet)
    {
        if (currentFleet == null) return;

        // Clear cũ
        foreach (Transform child in buttonContainer) Destroy(child.gameObject);

        // Tạo nút mới
        foreach (var duckData in currentFleet)
        {
            // Kiểm tra data rác
            if (duckData == null) continue;

            FleetDuckButton btn = Instantiate(duckButtonPrefab, buttonContainer);
            btn.Setup(duckData, (selectedDuck) => {
                fleetManager.SelectShip(selectedDuck);
            });
        }

        if (startBattleButton != null)
            startBattleButton.interactable = (currentFleet.Count == 0);
    }
}