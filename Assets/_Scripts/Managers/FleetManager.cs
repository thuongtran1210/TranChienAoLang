using System;
using System.Collections.Generic;
using UnityEngine;

public class FleetManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private List<DuckDataSO> levelFleetConfig;
    [SerializeField] private List<DuckDataSO> levelFleetData;
    // State Runtime
    private List<DuckDataSO> _availableShips;
    private DuckDataSO _currentSelectedDuck;

    // Events
    public event Action<List<DuckDataSO>> OnFleetChanged;
    public event Action<DuckDataSO> OnShipSelected;
    public event Action OnFleetEmpty;

    private void Awake()
    {
        InitializeFleet();
    }
    public void InitializeFleet()
    {
  
        if (levelFleetConfig != null)
        {
            _availableShips = new List<DuckDataSO>(levelFleetConfig);
        }
        else
        {
            _availableShips = new List<DuckDataSO>();
        }

        _currentSelectedDuck = null;

        // Bắn event update UI
        OnFleetChanged?.Invoke(_availableShips);
    }

    public List<DuckDataSO> GetCurrentFleet()
    { 
        if (_availableShips == null) _availableShips = new List<DuckDataSO>();
        return _availableShips;
    }
    public List<DuckDataSO> GetFleetData()
    {
   
        return new List<DuckDataSO>(levelFleetData);
    }

    public void SelectShip(DuckDataSO shipData)
    {
        if (_availableShips.Contains(shipData))
        {
            _currentSelectedDuck = shipData;
            OnShipSelected?.Invoke(shipData); 
            Debug.Log($"FleetManager: Đã chọn {shipData.duckName}");
        }
    }


    public void OnShipPlacedSuccess()
    {
        if (_currentSelectedDuck != null)
        {
            _availableShips.Remove(_currentSelectedDuck);
            _currentSelectedDuck = null;

            // Cập nhật UI
            OnFleetChanged?.Invoke(_availableShips);

            // Check xem hết tàu chưa
            if (_availableShips.Count == 0)
            {
                OnFleetEmpty?.Invoke();
                Debug.Log("FleetManager: Hết tàu trong kho!");
            }
        }
    }

    public DuckDataSO GetSelectedDuck() => _currentSelectedDuck;
}