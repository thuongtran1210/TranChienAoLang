using System;
using System.Collections.Generic;
using UnityEngine;

public class FleetManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private List<DuckDataSO> levelFleetConfig;
    [SerializeField] private List<DuckDataSO> levelFleetData;
    // Runtime
    private List<DuckDataSO> _availableDucks;

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
            _availableDucks = new List<DuckDataSO>(levelFleetConfig);
        }
        else
        {
            _availableDucks = new List<DuckDataSO>();
        }

        _currentSelectedDuck = null;

 
        OnFleetChanged?.Invoke(_availableDucks);
    }

    public List<DuckDataSO> GetCurrentFleet()
    { 
        if (_availableDucks == null) _availableDucks = new List<DuckDataSO>();
        return _availableDucks;
    }
    public List<DuckDataSO> GetFleetData()
    {
   
        return new List<DuckDataSO>(levelFleetData);
    }

    public void SelectDuck(DuckDataSO duckData)
    {
        if (_availableDucks.Contains(duckData))
        {
            _currentSelectedDuck = duckData;
            OnDuckSelected?.Invoke(duckData); 
            Debug.Log($"FleetManager: Đã chọn {duckData.duckName}");
        }
    }


    public void OnDuckPlacedSuccess()
    {
        if (_currentSelectedDuck != null)
        {
            _availableDucks.Remove(_currentSelectedDuck);
            _currentSelectedDuck = null;

            // Cập nhật UI
            OnFleetChanged?.Invoke(_availableDucks);

            // Check xem hết tàu chưa
            if (_availableDucks.Count == 0)
            {
                OnFleetEmpty?.Invoke();
                Debug.Log("FleetManager: Hết tàu trong kho!");
            }
        }
    }
    public DuckDataSO GetPlayerActiveDuckData()
    {
        if (levelFleetConfig != null && levelFleetConfig.Count > 0)
        {
            return levelFleetConfig[0];
        }

        return null;
    }

    public List<DuckSkillSO> GetPlayerSkillsForBattle()
    {
        List<DuckSkillSO> result = new List<DuckSkillSO>();

        if (levelFleetConfig == null) return result;

        foreach (var duck in levelFleetConfig)
        {
            if (duck != null && duck.activeSkill != null && !result.Contains(duck.activeSkill))
            {
                result.Add(duck.activeSkill);
            }
        }

        return result;
    }

    public DuckDataSO GetSelectedDuck() => _currentSelectedDuck;
}