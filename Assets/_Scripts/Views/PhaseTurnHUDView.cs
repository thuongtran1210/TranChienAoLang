using TMPro;
using UnityEngine;

public class PhaseTurnHUDView : MonoBehaviour
{
    [Header("Event Channel")]
    [SerializeField] private GameFlowEventChannelSO _flowEvents;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _phaseText;
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _turnTimerText;
    [SerializeField] private Color _normalTimerColor = Color.white;
    [SerializeField] private Color _warningTimerColor = Color.red;
    [SerializeField] private Vector3 _normalTimerScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private Vector3 _warningTimerScale = new Vector3(1.2f, 1.2f, 1f);

    private void OnEnable()
    {
        if (_flowEvents != null)
        {
            _flowEvents.OnPhaseChanged += SetPhase;
            _flowEvents.OnTurnChanged += SetTurn;
            _flowEvents.OnTurnTimerChanged += SetTurnTimer;
        }
    }

    private void OnDisable()
    {
        if (_flowEvents != null)
        {
            _flowEvents.OnPhaseChanged -= SetPhase;
            _flowEvents.OnTurnChanged -= SetTurn;
            _flowEvents.OnTurnTimerChanged -= SetTurnTimer;
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

    private void SetTurnTimer(int secondsRemaining)
    {
        if (_turnTimerText == null)
            return;

        if (secondsRemaining <= 0)
        {
            _turnTimerText.text = string.Empty;
            _turnTimerText.color = _normalTimerColor;
            _turnTimerText.transform.localScale = _normalTimerScale;
            return;
        }

        _turnTimerText.text = secondsRemaining.ToString();

        if (secondsRemaining <= 5)
        {
            _turnTimerText.color = _warningTimerColor;
            _turnTimerText.transform.localScale = _warningTimerScale;
        }
        else
        {
            _turnTimerText.color = _normalTimerColor;
            _turnTimerText.transform.localScale = _normalTimerScale;
        }
    }
}
