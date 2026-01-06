using UnityEngine;
using System.Collections;
public interface IGridContext
{
    // --- DATA & SYSTEMS ---
    IGridSystem GridSystem { get; }
    DuckDataSO SelectedDuck { get; }
    GridInputController InputController { get; }
    Vector2Int GetGridPosition(Vector3 worldPos);
    Owner GridOwner { get; }
    // --- STATES PROPERTIES ---
    bool IsGhostHorizontal { get; }

    // --- VISUAL HELPERS  ---
    void UpdateGhostPosition(Vector3 worldPos);
    void SetGhostValidation(bool isValid);
    void ToggleGhostRotation();
    void HideGhost();
    void ShowGhost(DuckDataSO data);
    // --- GAME FLOW LOGIC ---
    void OnDuckPlacedSuccess(DuckUnit unit, Vector2Int pos);
    void OnSetupPhaseCompleted();

    //--- COROUTINE SUPPORT ---
    Coroutine StartCoroutine(IEnumerator routine);
}