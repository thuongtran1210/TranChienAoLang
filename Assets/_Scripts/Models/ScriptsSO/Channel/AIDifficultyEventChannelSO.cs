using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Duck Battle/Events/AI Difficulty Event Channel")]
public class AIDifficultyEventChannelSO : ScriptableObject
{
    public UnityAction<AIDifficulty> OnDifficultyChanged;

    public void RaiseDifficultyChanged(AIDifficulty difficulty)
    {
        if (OnDifficultyChanged != null)
        {
            OnDifficultyChanged.Invoke(difficulty);
        }
    }
}

