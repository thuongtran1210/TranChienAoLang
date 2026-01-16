using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class BattleLogPresenter : MonoBehaviour
{
    [Header("Event Channel")]
    [SerializeField] private UIFeedbackChannelSO _uiFeedback;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _text;

    [Header("Settings")]
    [SerializeField] private int _maxLines = 40;

    private readonly Queue<string> _lines = new Queue<string>();
    private readonly StringBuilder _builder = new StringBuilder(2048);

    private void OnEnable()
    {
        if (_uiFeedback != null)
            _uiFeedback.OnLogRequested += Append;
    }

    private void OnDisable()
    {
        if (_uiFeedback != null)
            _uiFeedback.OnLogRequested -= Append;
    }

    private void Append(UIFeedbackPayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.Message))
            return;

        _lines.Enqueue(payload.Message);
        while (_lines.Count > Mathf.Max(1, _maxLines))
            _lines.Dequeue();

        if (_text == null)
            return;

        _builder.Clear();
        foreach (var line in _lines)
        {
            _builder.AppendLine(line);
        }

        _text.text = _builder.ToString();
    }
}

