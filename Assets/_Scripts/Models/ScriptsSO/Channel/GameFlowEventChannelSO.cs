using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Duck Battle/Game Flow Event Channel", order = 2)]
public class GameFlowEventChannelSO : ScriptableObject
{
    public UnityAction<GamePhase> OnPhaseChanged;
    public UnityAction<Owner> OnTurnChanged;
    public UnityAction<int> OnTurnTimerChanged;

    public void RaisePhaseChanged(GamePhase phase) => OnPhaseChanged?.Invoke(phase);
    public void RaiseTurnChanged(Owner turnOwner) => OnTurnChanged?.Invoke(turnOwner);
    public void RaiseTurnTimerChanged(int secondsRemaining) => OnTurnTimerChanged?.Invoke(secondsRemaining);
}
