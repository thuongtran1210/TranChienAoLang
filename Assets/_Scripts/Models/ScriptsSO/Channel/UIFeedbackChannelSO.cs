using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Duck Battle/UI Feedback Channel", order = 3)]
public class UIFeedbackChannelSO : ScriptableObject
{
    public UnityAction<UIFeedbackPayload> OnToastRequested;
    public UnityAction<UIFeedbackPayload> OnLogRequested;

    public void RaiseToast(UIFeedbackPayload payload) => OnToastRequested?.Invoke(payload);
    public void RaiseLog(UIFeedbackPayload payload) => OnLogRequested?.Invoke(payload);
}

