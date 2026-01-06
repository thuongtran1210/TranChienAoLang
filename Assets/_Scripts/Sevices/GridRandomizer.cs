// _Scripts/Sevices/GridRandomizer.cs
using System.Collections.Generic;
using UnityEngine;

namespace Game.Services
{
    public static class GridRandomizer
    {
        // Pure function: Nhận vào Grid và List tàu, trả về Grid đã được điền
        public static void AutoPlaceFleet(IGridSystem gridSystem, List<DuckDataSO> fleetToPlace)
        {
            foreach (var duckData in fleetToPlace)
            {
                bool placed = false;
                int attempts = 0;
                while (!placed && attempts < 100) // Safety break
                {
                    // 1. Random vị trí và hướng
                    int x = Random.Range(0, gridSystem.Width);
                    int y = Random.Range(0, gridSystem.Height);
                    bool isHorizontal = Random.value > 0.5f; 
                    Vector2Int pos = new Vector2Int(x, y);

                    // 2. Kiểm tra xem đặt được không
                    if (gridSystem.CanPlaceUnit(duckData, pos, isHorizontal))
                    {
                        DuckUnit unit = new DuckUnit(duckData, isHorizontal);

                        // 3. Đặt vào Grid
                        gridSystem.PlaceUnit(unit, pos, isHorizontal);
                        placed = true;
                    }
                    attempts++;
                }
            }
        }
    }
}