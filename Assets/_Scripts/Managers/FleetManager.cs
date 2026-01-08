using System;
using System.Collections.Generic;
using UnityEngine;

public class FleetManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private List<DuckDataSO> levelFleetConfig;
    [SerializeField] private List<DuckDataSO> levelFleetData;
    // Runtime
    private List<DuckDataSO> _availableShips;

    // Events
    public event Action<List<DuckDataSO>> OnFleetChanged;
    public event Action<DuckDataSO> OnDuckSelected;
    public event Action OnFleetEmpty;
    public event Action OnDuckPlaced;

    private DuckDataSO _currentSelectedDuck;

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

    public void SelectDuck(DuckDataSO duckData)
    {
        if (_availableShips.Contains(duckData))
        {
            _currentSelectedDuck = duckData;
            OnDuckSelected?.Invoke(duckData); 
            Debug.Log($"FleetManager: Đã chọn {duckData.duckName}");
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