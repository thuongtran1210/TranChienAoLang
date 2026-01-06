using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

[CreateAssetMenu(fileName = "DuckDataSO", menuName = "Scriptable Objects/DuckDataSO")]
public class DuckDataSO : ScriptableObject
{
    [Header("General info")]
    public string duckName;
 
    [TextArea(3,5)]
    public string description;

    [Header("Stats")]
    public List<Vector2Int> structure = new List<Vector2Int>() { Vector2Int.zero };
    public int size => structure.Count;

    [Header("Visuals")]
    public Sprite icon;
    public GameObject unitPrefab;
    /// <summary>
    /// Trả về danh sách tọa độ thực tế trên Grid dựa trên vị trí gốc và hướng xoay.
    /// </summary>
    public IEnumerable<Vector2Int> GetOccupiedCells(Vector2Int pivot, bool isHorizontal)
    {
        foreach (var offset in structure)
        {
            int x, y;

            if (isHorizontal)
            {
                x = offset.x;
                y = offset.y;
            }
            else
            {
                // Xoay 90 độ: Biến chiều dài (x) thành chiều cao (y)
                x = offset.y;
                y = offset.x;
            }

            yield return new Vector2Int(pivot.x + x, pivot.y + y);
        }
    }

}
