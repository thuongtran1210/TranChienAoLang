using TMPro;
using UnityEngine;

public class PhaseTurnHUDView : MonoBehaviour
{
    [Header("Event Channel")]
    [SerializeField] private GameFlowEventChannelSO _flowEvents;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _phaseText;
    [SerializeField] private TextMeshProUGUI _turnText;

    private void OnEnable()
    {
        if (_flowEvents != null)
        {
            _flowEvents.OnPhaseChanged += SetPhase;
            _flowEvents.OnTurnChanged += SetTurn;
        }
    }

    private void OnDisable()
    {
        if (_flowEvents != null)
        {
            _flowEvents.OnPhaseChanged -= SetPhase;
            _flowEvents.OnTurnChanged -= SetTurn;
        }
    }

    private void SetPhase(GamePhase phase)
    {
        if (_phaseText != null)
            _phaseText.text = phase.ToString();
    }

    private void SetTurn(Owner owner)
    {
        if (_turnText != null)
            _turnText.text = owner == Owner.Player ? "PLAYER TURN" : "ENEMY TURN";
    }
}

