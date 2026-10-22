using UnityEngine;

namespace ReLocate.Debug
{
    [AddComponentMenu("ReLocate/Debug/FPS Monitor")]
    public class FPSMonitor : MonoBehaviour
    {
        [Header("Обновление")]
        [SerializeField, Tooltip("Интервал пересчёта FPS в секундах")]
        private float updateInterval = 0.5f;

        [Header("Позиция на экране")]
        [SerializeField]
        private Vector2 position = new Vector2(10, 10);
        [SerializeField, Tooltip("Размер шрифта в пикселях")]
        private int fontSize = 20;
        [SerializeField, Tooltip("Смещение строк между показателями")]
        private int lineSpacing = 5;

        [Header("Пороги цвета (FPS)")]
        [SerializeField, Tooltip("Ниже этого значения — красный")]
        private float lowThreshold = 30f;
        [SerializeField, Tooltip("Ниже этого значения — жёлтый, выше — зелёный")]
        private float mdiumThreshold = 60f;

        [Header("Отображать")]
        [SerializeField] private bool showCurrentFPS = true;
        [SerializeField] private bool showMinMax = true;
        [SerializeField] private bool showFrameTime = true;

        private int _frameCount;
        private float _timeAccumulator;
        private float _currentFPS;
        private float _minFPS = float.MaxValue;
        private float _maxFPS = float.MinValue;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            _frameCount++;
            _timeAccumulator += Time.unscaledDeltaTime;

            if (_timeAccumulator < updateInterval)
            {
               return;
            }
            _currentFPS = _frameCount / _timeAccumulator;

            if (_currentFPS < _minFPS) _minFPS = _currentFPS;
            if (_currentFPS > _maxFPS) _maxFPS = _currentFPS;

            _frameCount = 0;
            _timeAccumulator = 0f;
        }

        private void OnGUI()
        {
            if (!enabled) return;

            var previousColor = GUI.color;
            var previousAlignment = GUI.skin.label.alignment;
            var previousFontSize = GUI.skin.label.fontSize;

            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.skin.label.fontSize = fontSize;

            var y = position.y;

            if (showCurrentFPS)
            {
                GUI.color = GetFPSColor(_currentFPS);
                GUI.Label(new Rect(position.x, y, 200, fontSize + lineSpacing), $"FPS: {_currentFPS:F1}");
                y += fontSize + lineSpacing;
            }

            if (showFrameTime)
            {
                GUI.color = previousColor;
                GUI.Label(new Rect(position.x, y, 200, fontSize + lineSpacing), $"Frame time: {(1000f / Mathf.Max(_currentFPS, 0.001f)):F1} ms");
                y += fontSize + lineSpacing;
            }

            if (showMinMax)
            {
                GUI.color = previousColor;
                GUI.Label(new Rect(position.x, y, 200, fontSize + lineSpacing), $"Min: {_minFPS:F1} | Max: {_maxFPS:F1}");
            }

            GUI.skin.label.fontSize = previousFontSize;
            GUI.skin.label.alignment = previousAlignment;
            GUI.color = previousColor;
        }

        private Color GetFPSColor(float fps)
        {
            if (fps < lowThreshold) return Color.red;
            if (fps < mdiumThreshold) return Color.yellow;
            return Color.green;
        }
    }
}