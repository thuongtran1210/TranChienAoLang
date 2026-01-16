using System;
using UnityEngine;

[Serializable]
public struct UIFeedbackPayload
{
    public string Message;
    public UIFeedbackType Type;
    public UIFeedbackSource Source;

    public bool HasOwner;
    public Owner Owner;

    public bool HasGridPos;
    public Vector2Int GridPos;

    public float Duration;

    public static UIFeedbackPayload Info(string message, UIFeedbackSource source, float duration = 1.5f)
    {
        return new UIFeedbackPayload
        {
            Message = message,
            Type = UIFeedbackType.Info,
            Source = source,
            HasOwner = false,
            HasGridPos = false,
            Duration = duration
        };
    }
}

