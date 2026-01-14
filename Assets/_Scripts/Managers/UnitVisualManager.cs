using UnityEngine;
// Cập nhật Hiển thị Vịt trên Lưới
public class UnitVisualManager : MonoBehaviour
{
    private TilemapGridView _tilemapGridView;

    public void Initialize(TilemapGridView tilemapGridView)
    {
        _tilemapGridView = tilemapGridView;
    }

    public void SpawnDuck(Vector2Int gridPos, bool isHorizontal, DuckDataSO data)
    {
        if (data == null || data.unitPrefab == null) return;

        // Lấy vị trí từ TilemapGridView để đảm bảo đồng bộ
        Vector3 worldPos = _tilemapGridView.GetWorldCenterPosition(gridPos);

        // Điều chỉnh vị trí tâm nếu con vịt chiếm nhiều ô (tùy thuộc vào pivot của prefab vịt)
        // Đây là logic hiển thị đơn giản hóa
        GameObject duckObj = Instantiate(data.unitPrefab, transform);
        duckObj.transform.position = worldPos;

        Quaternion rotation = isHorizontal ? Quaternion.identity : Quaternion.Euler(0, 0, 90);
        duckObj.transform.rotation = rotation;

        // Setup logic hiển thị cho vịt (nếu cần)
        DuckView view = duckObj.GetComponent<DuckView>();
        if (view != null)
        {
            view.Bind(data, isHorizontal);
        }
    }
}