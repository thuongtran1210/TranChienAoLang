using UnityEngine;

public class UIFeedbackRouter : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private BattleEventChannelSO _battleEvents;
    [SerializeField] private GameFlowEventChannelSO _flowEvents;
    [SerializeField] private UIFeedbackChannelSO _uiFeedback;

    private void OnEnable()
    {
        if (_battleEvents != null)
        {
            _battleEvents.OnShotFired += HandleShotFired;
            _battleEvents.OnSkillFeedback += HandleSkillFeedback;
        }

        if (_flowEvents != null)
        {
            _flowEvents.OnPhaseChanged += HandlePhaseChanged;
            _flowEvents.OnTurnChanged += HandleTurnChanged;
        }
    }

    private void OnDisable()
    {
        if (_battleEvents != null)
        {
            _battleEvents.OnShotFired -= HandleShotFired;
            _battleEvents.OnSkillFeedback -= HandleSkillFeedback;
        }

        if (_flowEvents != null)
        {
            _flowEvents.OnPhaseChanged -= HandlePhaseChanged;
            _flowEvents.OnTurnChanged -= HandleTurnChanged;
        }
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        if (_uiFeedback == null) return;

        _uiFeedback.RaiseLog(new UIFeedbackPayload
        {
            Message = $"Phase: {phase}",
            Type = UIFeedbackType.Info,
            Source = UIFeedbackSource.System,
            Duration = 0f
        });
    }

    private void HandleTurnChanged(Owner turnOwner)
    {
        if (_uiFeedback == null) return;

        _uiFeedback.RaiseToast(new UIFeedbackPayload
        {
            Message = turnOwner == Owner.Player ? "Lượt của bạn" : "Lượt đối thủ",
            Type = UIFeedbackType.Info,
            Source = UIFeedbackSource.System,
            HasOwner = true,
            Owner = turnOwner,
            Duration = 1.0f
        });
    }

    private void HandleSkillFeedback(string message, Vector2Int position)
    {
        if (_uiFeedback == null) return;

        var payload = new UIFeedbackPayload
        {
            Message = message,
            Type = UIFeedbackType.Warning,
            Source = UIFeedbackSource.Skill,
            HasGridPos = position != Vector2Int.zero,
            GridPos = position,
            Duration = 1.5f
        };

        _uiFeedback.RaiseToast(payload);
        _uiFeedback.RaiseLog(payload);
    }

    private void HandleShotFired(Owner shooter, Owner target, ShotResult result, Vector2Int pos)
    {
        if (_uiFeedback == null) return;

        string shooterLabel = shooter == Owner.Player ? "Player" : "Enemy";
        string resultLabel = result.ToString();
        string message = $"{shooterLabel} bắn {resultLabel}";

        UIFeedbackType type = result == ShotResult.Miss ? UIFeedbackType.Info : UIFeedbackType.Success;
        if (result == ShotResult.Invalid || result == ShotResult.None) type = UIFeedbackType.Warning;

        var payload = new UIFeedbackPayload
        {
            Message = message,
            Type = type,
            Source = UIFeedbackSource.Shot,
            HasOwner = true,
            Owner = shooter,
            HasGridPos = true,
            GridPos = pos,
            Duration = 1.2f
        };

        _uiFeedback.RaiseToast(payload);
        _uiFeedback.RaiseLog(payload);
    }
}

