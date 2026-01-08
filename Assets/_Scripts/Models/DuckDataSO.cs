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
        List<Vector2Int> cells = new List<Vector2Int>();
        for (int i = 0; i < size; i++)
        {
            int x = pivot.x + (isHorizontal ? i : 0);
            int y = pivot.y + (isHorizontal ? 0 : i); 
            cells.Add(new Vector2Int(x, y));
        }
        return cells;
    }

}
