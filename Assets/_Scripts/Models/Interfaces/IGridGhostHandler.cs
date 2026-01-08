
using UnityEngine;

public interface IGridGhostHandler
{
    bool IsGhostHorizontal { get; }
    void ShowGhost(DuckDataSO data);
    void HideGhost();
    void UpdateGhostPosition(Vector3 worldPos);
    void SetGhostValidation(bool isValid);
    void ToggleGhostRotation();
    void OnDuckPlacedSuccess(DuckUnit unit, Vector2Int pos);
    void OnSetupPhaseCompleted();
}