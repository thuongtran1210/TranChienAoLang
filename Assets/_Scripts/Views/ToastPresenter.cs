using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToastPresenter : MonoBehaviour
{
    [Header("Event Channel")]
    [SerializeField] private UIFeedbackChannelSO _uiFeedback;

    [Header("UI")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _text;

    [Header("Timing")]
    [SerializeField] private float _fadeDuration = 0.15f;
    [SerializeField] private float _defaultDuration = 1.5f;

    private readonly Queue<UIFeedbackPayload> _queue = new Queue<UIFeedbackPayload>();
    private Coroutine _runner;

    private void OnEnable()
    {
        if (_uiFeedback != null)
            _uiFeedback.OnToastRequested += Enqueue;
    }

    private void OnDisable()
    {
        if (_uiFeedback != null)
            _uiFeedback.OnToastRequested -= Enqueue;
    }

    private void Enqueue(UIFeedbackPayload payload)
    {
        _queue.Enqueue(payload);
        if (_runner == null)
            _runner = StartCoroutine(RunQueue());
    }

    private IEnumerator RunQueue()
    {
        while (_queue.Count > 0)
        {
            UIFeedbackPayload payload = _queue.Dequeue();
            yield return ShowOnce(payload);
        }

        _runner = null;
    }

    private IEnumerator ShowOnce(UIFeedbackPayload payload)
    {
        if (_text != null)
            _text.text = payload.Message;

        if (_canvasGroup != null)
        {
            _canvasGroup.gameObject.SetActive(true);
            yield return Fade(0f, 1f, _fadeDuration);
        }

        float duration = payload.Duration > 0 ? payload.Duration : _defaultDuration;
        yield return new WaitForSeconds(duration);

        if (_canvasGroup != null)
        {
            yield return Fade(1f, 0f, _fadeDuration);
            _canvasGroup.gameObject.SetActive(false);
        }
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (_canvasGroup == null)
            yield break;

        float t = 0f;
        _canvasGroup.alpha = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);
            _canvasGroup.alpha = Mathf.Lerp(from, to, p);
            yield return null;
        }

        _canvasGroup.alpha = to;
    }
}

