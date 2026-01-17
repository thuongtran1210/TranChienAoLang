using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToastPresenter : MonoBehaviour
{
    [Header("Event Channel")]
    [SerializeField] private UIFeedbackChannelSO _uiFeedback;

    [Header("UI")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("UI (Text)")]
    [SerializeField] private GameObject _textRoot;
    [SerializeField] private TextMeshProUGUI _text;

    [Header("UI (Duck Sunk Visual)")]
    [SerializeField] private GameObject _duckSunkRoot;
    [SerializeField] private Image _duckSunkImage;
    [SerializeField] private Sprite _duckSunkSprite;
    [SerializeField] private RectTransform _fxRoot;

    [Header("FX (Duck Sunk)")]
    [SerializeField] private bool _enableDuckSunkFx = true;
    [SerializeField] private Sprite _bubbleSprite;
    [SerializeField] private Sprite _rippleSprite;
    [SerializeField] private Color _bubbleColor = new Color(1f, 1f, 1f, 0.55f);
    [SerializeField] private Color _rippleColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private int _bubblePoolSize = 8;
    [SerializeField] private int _ripplePoolSize = 2;
    [SerializeField] private float _bubbleSpawnInterval = 0.16f;
    [SerializeField] private Vector2 _bubbleSpeedRange = new Vector2(40f, 90f);
    [SerializeField] private Vector2 _bubbleSizeRange = new Vector2(10f, 18f);
    [SerializeField] private Vector2 _bubbleSpawnArea = new Vector2(80f, 40f);
    [SerializeField] private float _rippleInterval = 0.35f;
    [SerializeField] private Vector2 _rippleSizeRange = new Vector2(28f, 70f);
    [SerializeField] private float _duckSinkDistance = 24f;
    [SerializeField] private float _duckSinkTime = 0.55f;
    [SerializeField] private float _duckBobAmplitude = 3f;
    [SerializeField] private float _duckBobFrequency = 6f;

    [Header("Layout (Duck Sunk)")]
    [SerializeField] private float _duckMaxHeightRatio = 0.78f;
    [SerializeField] private float _duckMaxWidthRatio = 0.62f;
    [SerializeField] private Vector2 _duckAnchoredOffset = new Vector2(0f, -2f);

    [Header("Timing")]
    [SerializeField] private float _fadeDuration = 0.15f;
    [SerializeField] private float _defaultDuration = 1.5f;

    private readonly Queue<UIFeedbackPayload> _queue = new Queue<UIFeedbackPayload>();
    private Coroutine _runner;
    private Coroutine _duckSunkFxRunner;
    private Vector2? _duckInitialAnchoredPos;

    private sealed class FxItem
    {
        public RectTransform Rect;
        public Image Image;
        public bool Active;
        public float Age;
        public float Duration;
        public Vector2 Velocity;
        public float StartSize;
        public float EndSize;
        public Color Color;
    }

    private FxItem[] _bubblePool;
    private FxItem[] _ripplePool;
    private Vector2 _bubbleSpawnAreaRuntime;
    private Vector2 _rippleSizeRangeRuntime;

    private void OnEnable()
    {
        if (_uiFeedback != null)
            _uiFeedback.OnToastRequested += Enqueue;
    }
    void Awake()
    {
        if (_textRoot == null && _text != null)
            _textRoot = _text.gameObject;

        if (_duckSunkRoot != null)
            _duckSunkRoot.SetActive(false);

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(false);
        }
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
        bool showDuckSunkVisual = payload.ToastVisual == UIFeedbackToastVisual.DuckSunk;
        if (showDuckSunkVisual)
        {
            if (_textRoot != null)
                _textRoot.SetActive(false);

            if (_duckSunkRoot != null)
                _duckSunkRoot.SetActive(true);

            if (_duckSunkImage != null && _duckSunkSprite != null)
                _duckSunkImage.sprite = _duckSunkSprite;

            ApplyDuckSunkLayout();
        }
        else
        {
            if (_duckSunkRoot != null)
                _duckSunkRoot.SetActive(false);

            if (_textRoot != null)
                _textRoot.SetActive(true);

            if (_text != null)
                _text.text = payload.Message;
        }

        float duration = payload.Duration > 0 ? payload.Duration : _defaultDuration;

        if (_canvasGroup != null)
        {
            _canvasGroup.gameObject.SetActive(true);
            yield return Fade(0f, 1f, _fadeDuration);
        }

        if (showDuckSunkVisual && _enableDuckSunkFx)
            StartDuckSunkFx(duration);

        yield return new WaitForSeconds(duration);

        StopDuckSunkFx();

        if (_canvasGroup != null)
        {
            yield return Fade(1f, 0f, _fadeDuration);
            _canvasGroup.gameObject.SetActive(false);
        }
    }

    private void StartDuckSunkFx(float duration)
    {
        if (_duckSunkFxRunner != null)
            StopCoroutine(_duckSunkFxRunner);

        if (_duckSunkRoot == null || _duckSunkImage == null)
            return;

        EnsureFxPool();
        _duckSunkFxRunner = StartCoroutine(RunDuckSunkFx(duration));
    }

    private void StopDuckSunkFx()
    {
        if (_duckSunkFxRunner != null)
        {
            StopCoroutine(_duckSunkFxRunner);
            _duckSunkFxRunner = null;
        }

        if (_duckSunkImage != null && _duckInitialAnchoredPos.HasValue)
            _duckSunkImage.rectTransform.anchoredPosition = _duckInitialAnchoredPos.Value;

        if (_bubblePool != null)
        {
            for (int i = 0; i < _bubblePool.Length; i++)
                DeactivateFxItem(_bubblePool[i]);
        }

        if (_ripplePool != null)
        {
            for (int i = 0; i < _ripplePool.Length; i++)
                DeactivateFxItem(_ripplePool[i]);
        }
    }

    private IEnumerator RunDuckSunkFx(float duration)
    {
        RectTransform duckRt = _duckSunkImage.rectTransform;
        if (!_duckInitialAnchoredPos.HasValue)
            _duckInitialAnchoredPos = duckRt.anchoredPosition;

        Vector2 basePos = _duckInitialAnchoredPos.Value;
        float sinkT = Mathf.Max(0.01f, _duckSinkTime);
        float bubbleTimer = 0f;
        float rippleTimer = 0f;

        for (int i = 0; i < (_bubblePool?.Length ?? 0); i++)
            DeactivateFxItem(_bubblePool[i]);
        for (int i = 0; i < (_ripplePool?.Length ?? 0); i++)
            DeactivateFxItem(_ripplePool[i]);

        float t = 0f;
        while (t < duration)
        {
            float dt = Time.deltaTime;
            t += dt;

            float sinkP = Mathf.Clamp01(t / sinkT);
            float sink = Mathf.SmoothStep(0f, _duckSinkDistance, sinkP);
            float bob = Mathf.Sin(t * _duckBobFrequency) * _duckBobAmplitude * (1f - sinkP);
            duckRt.anchoredPosition = basePos + Vector2.down * sink + Vector2.up * bob;

            bubbleTimer += dt;
            while (bubbleTimer >= _bubbleSpawnInterval)
            {
                bubbleTimer -= _bubbleSpawnInterval;
                TrySpawnBubble(basePos);
            }

            rippleTimer += dt;
            while (rippleTimer >= _rippleInterval)
            {
                rippleTimer -= _rippleInterval;
                TrySpawnRipple(basePos);
            }

            UpdateFxPool(_bubblePool, dt);
            UpdateFxPool(_ripplePool, dt);

            yield return null;
        }
    }

    private void EnsureFxPool()
    {
        if (_bubblePool != null && _ripplePool != null)
            return;

        if (_fxRoot == null)
        {
            if (_duckSunkRoot == null)
                return;

            var fxRootGo = new GameObject("FxRoot", typeof(RectTransform));
            fxRootGo.transform.SetParent(_duckSunkRoot.transform, false);
            _fxRoot = fxRootGo.GetComponent<RectTransform>();
            _fxRoot.anchorMin = Vector2.zero;
            _fxRoot.anchorMax = Vector2.one;
            _fxRoot.offsetMin = Vector2.zero;
            _fxRoot.offsetMax = Vector2.zero;
            _fxRoot.anchoredPosition = Vector2.zero;
        }

        _bubblePool = BuildFxPool("Bubble", Mathf.Max(0, _bubblePoolSize), _bubbleSprite, _bubbleColor);
        _ripplePool = BuildFxPool("Ripple", Mathf.Max(0, _ripplePoolSize), _rippleSprite != null ? _rippleSprite : _bubbleSprite, _rippleColor);
    }

    private FxItem[] BuildFxPool(string prefix, int count, Sprite sprite, Color color)
    {
        var pool = new FxItem[count];
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"{prefix}_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(_fxRoot, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;

            var item = new FxItem
            {
                Rect = rt,
                Image = img,
                Active = false,
                Age = 0f,
                Duration = 0f,
                Velocity = Vector2.zero,
                StartSize = 0f,
                EndSize = 0f,
                Color = color
            };

            pool[i] = item;
            go.SetActive(false);
        }

        return pool;
    }

    private void TrySpawnBubble(Vector2 basePos)
    {
        if (_bubblePool == null || _bubblePool.Length == 0)
            return;
        if (_bubbleSprite == null)
            return;

        FxItem item = FindInactiveItem(_bubblePool);
        if (item == null)
            return;

        float size = Random.Range(_bubbleSizeRange.x, _bubbleSizeRange.y);
        float life = Random.Range(0.45f, 0.75f);
        float speed = Random.Range(_bubbleSpeedRange.x, _bubbleSpeedRange.y);
        Vector2 area = _bubbleSpawnAreaRuntime == Vector2.zero ? _bubbleSpawnArea : _bubbleSpawnAreaRuntime;
        float x = Random.Range(-area.x * 0.5f, area.x * 0.5f);
        float y = Random.Range(-area.y * 0.5f, area.y * 0.5f);

        ActivateFxItem(item);
        item.Age = 0f;
        item.Duration = life;
        item.Velocity = new Vector2(0f, speed);
        item.StartSize = size;
        item.EndSize = size * 0.65f;
        item.Color = _bubbleColor;
        item.Rect.anchoredPosition = basePos + new Vector2(x, y);
        item.Rect.sizeDelta = new Vector2(size, size);
        item.Image.color = _bubbleColor;
    }

    private void TrySpawnRipple(Vector2 basePos)
    {
        if (_ripplePool == null || _ripplePool.Length == 0)
            return;
        if (_rippleSprite == null && _bubbleSprite == null)
            return;

        FxItem item = FindInactiveItem(_ripplePool);
        if (item == null)
            return;

        Vector2 range = _rippleSizeRangeRuntime == Vector2.zero ? _rippleSizeRange : _rippleSizeRangeRuntime;
        float startSize = range.x;
        float endSize = range.y;
        float life = 0.55f;

        ActivateFxItem(item);
        item.Age = 0f;
        item.Duration = life;
        item.Velocity = Vector2.zero;
        item.StartSize = startSize;
        item.EndSize = endSize;
        item.Color = _rippleColor;
        item.Rect.anchoredPosition = basePos;
        item.Rect.sizeDelta = new Vector2(startSize, startSize);
        item.Image.color = _rippleColor;
    }

    private void UpdateFxPool(FxItem[] pool, float dt)
    {
        if (pool == null)
            return;

        for (int i = 0; i < pool.Length; i++)
        {
            FxItem item = pool[i];
            if (item == null || !item.Active)
                continue;

            item.Age += dt;
            float p = item.Duration <= 0f ? 1f : Mathf.Clamp01(item.Age / item.Duration);
            float size = Mathf.Lerp(item.StartSize, item.EndSize, p);
            item.Rect.sizeDelta = new Vector2(size, size);
            item.Rect.anchoredPosition += item.Velocity * dt;

            var c = item.Color;
            c.a = item.Color.a * (1f - p);
            item.Image.color = c;

            if (p >= 1f)
                DeactivateFxItem(item);
        }
    }

    private FxItem FindInactiveItem(FxItem[] pool)
    {
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != null && !pool[i].Active)
                return pool[i];
        }

        return null;
    }

    private void ActivateFxItem(FxItem item)
    {
        if (item == null || item.Rect == null)
            return;

        item.Active = true;
        item.Rect.gameObject.SetActive(true);
    }

    private void DeactivateFxItem(FxItem item)
    {
        if (item == null || item.Rect == null)
            return;

        item.Active = false;
        item.Rect.gameObject.SetActive(false);
    }

    private void ApplyDuckSunkLayout()
    {
        if (_canvasGroup == null || _duckSunkImage == null)
            return;

        var panelRt = _canvasGroup.GetComponent<RectTransform>();
        if (panelRt == null)
            return;

        float panelW = Mathf.Max(1f, panelRt.rect.width);
        float panelH = Mathf.Max(1f, panelRt.rect.height);

        _bubbleSpawnAreaRuntime = new Vector2(panelW * 0.55f, panelH * 0.45f);
        _rippleSizeRangeRuntime = new Vector2(panelH * 0.22f, panelH * 0.62f);

        if (_fxRoot != null)
        {
            _fxRoot.anchorMin = Vector2.zero;
            _fxRoot.anchorMax = Vector2.one;
            _fxRoot.offsetMin = Vector2.zero;
            _fxRoot.offsetMax = Vector2.zero;
        }

        var duckRt = _duckSunkImage.rectTransform;
        duckRt.anchorMin = new Vector2(0.5f, 0.5f);
        duckRt.anchorMax = new Vector2(0.5f, 0.5f);
        duckRt.pivot = new Vector2(0.5f, 0.5f);
        duckRt.anchoredPosition = _duckAnchoredOffset;

        float maxW = panelW * Mathf.Clamp01(_duckMaxWidthRatio);
        float maxH = panelH * Mathf.Clamp01(_duckMaxHeightRatio);

        Sprite s = _duckSunkImage.sprite;
        if (s == null)
        {
            duckRt.sizeDelta = new Vector2(maxW, maxH);
            return;
        }

        float aspect = s.rect.width / Mathf.Max(1f, s.rect.height);
        float w = maxW;
        float h = w / Mathf.Max(0.01f, aspect);
        if (h > maxH)
        {
            h = maxH;
            w = h * aspect;
        }

        duckRt.sizeDelta = new Vector2(w, h);
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
