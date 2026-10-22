using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ReLocate.Debug
{
    [AddComponentMenu("ReLocate/Debug/Damage Monitor")]
    public class DamageMonitor : MonoBehaviour
    {
        [Header("Отслеживание")]
        [SerializeField, Tooltip("Как часто искать новые Damageable на сцене (сек)")]
        private float rescanInterval = 1f;
        [SerializeField, Tooltip("Окно расчёта DPS в секундах")]
        private float dpsWindow = 3f;
        [SerializeField, Tooltip("Сколько последних ударов хранить в логе")]
        private int logSize = 5;
        [SerializeField] private bool persistBetweenScenes;

        [Header("Позиция на экране")]
        [SerializeField]
        private Vector2 position = new (10, 100);
        [SerializeField, Tooltip("Размер шрифта в пикселях")]
        private int fontSize = 20;
        [SerializeField, Tooltip("Смещение строк между показателями")]
        private int lineSpacing = 5;
        [SerializeField] private float labelWidth = 400f;

        [Header("Пороги цвета (DPS)")]
        [SerializeField, Tooltip("Ниже этого значения — красный")]
        private float lowDps = 10f;
        [SerializeField, Tooltip("Ниже этого значения — жёлтый, выше — зелёный")]
        private float mediumDps = 30f;

        [Header("Отображать")]
        [SerializeField] private bool showDps = true;
        [SerializeField] private bool showTotals = true;
        [SerializeField] private bool showTargets = true;
        [SerializeField] private bool showLog = true;

        private struct HitRecord
        {
            public string Target;
            public float Amount;
            public float Time;
            public bool Killed;
        }

        private class Tracked
        {
            public float LastHealth;
            public UnityAction<float, float> Handler;
        }

        private readonly Dictionary<Damageable, Tracked> _tracked = new Dictionary<Damageable, Tracked>();
        private readonly List<Damageable> _removeBuffer = new List<Damageable>();
        private readonly Queue<(float time, float amount)> _window = new Queue<(float, float)>();
        private readonly List<HitRecord> _log = new List<HitRecord>();

        private float _scanTimer;
        private float _currentDps;
        private float _maxDps;
        private float _totalDamage;
        private float _maxHit;
        private int _hitCount;
        private int _kills;

        private void Awake()
        {
            if (persistBetweenScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnEnable()
        {
            Rescan();
        }

        private void OnDisable()
        {
            foreach (var pair in _tracked)
            {
                if (pair.Key != null)
                {
                    pair.Key.HealthChanged.RemoveListener(pair.Value.Handler);
                }
            }

            _tracked.Clear();
        }

        private void Update()
        {
            _scanTimer += Time.unscaledDeltaTime;
            if (_scanTimer >= rescanInterval)
            {
                _scanTimer = 0f;
                Rescan();
            }

            var now = Time.time;
            while (_window.Count > 0 && now - _window.Peek().time > dpsWindow)
            {
                _window.Dequeue();
            }

            var sum = 0f;
            foreach (var entry in _window)
            {
                sum += entry.amount;
            }

            _currentDps = sum / Mathf.Max(dpsWindow, 0.001f);
            if (_currentDps > _maxDps) _maxDps = _currentDps;
        }

        private void Rescan()
        {
            _removeBuffer.Clear();
            foreach (var pair in _tracked)
            {
                if (!pair.Key)
                {
                    _removeBuffer.Add(pair.Key);
                }
            }

            foreach (var destroyed in _removeBuffer)
            {
                _tracked.Remove(destroyed);
            }

            var found = FindObjectsByType<Damageable>(FindObjectsSortMode.None);
            foreach (var damageable in found)
            {
                if (!_tracked.ContainsKey(damageable))
                {
                    Register(damageable);
                }
            }
        }

        private void Register(Damageable damageable)
        {
            var tracked = new Tracked { LastHealth = damageable.CurrentHealth };
            tracked.Handler = (current, max) => OnHealthChanged(damageable, tracked, current);
            damageable.HealthChanged.AddListener(tracked.Handler);
            _tracked.Add(damageable, tracked);
        }

        private void OnHealthChanged(Damageable damageable, Tracked tracked, float current)
        {
            var delta = tracked.LastHealth - current;
            tracked.LastHealth = current;

            if (delta <= 0f)
            {
                return;
            }

            var killed = current <= 0f;

            _totalDamage += delta;
            _hitCount++;
            if (delta > _maxHit) _maxHit = delta;
            if (killed) _kills++;

            _window.Enqueue((Time.time, delta));

            _log.Add(new HitRecord
            {
                Target = damageable.name,
                Amount = delta,
                Time = Time.time,
                Killed = killed
            });

            while (_log.Count > logSize)
            {
                _log.RemoveAt(0);
            }
        }

        [ContextMenu("Reset Stats")]
        public void ResetStats()
        {
            _window.Clear();
            _log.Clear();
            _currentDps = 0f;
            _maxDps = 0f;
            _totalDamage = 0f;
            _maxHit = 0f;
            _hitCount = 0;
            _kills = 0;
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

            if (showDps)
            {
                DrawLine(ref y, $"DPS: {_currentDps:F1} (max {_maxDps:F1})", GetDpsColor(_currentDps));
            }

            if (showTotals)
            {
                DrawLine(ref y, $"Total: {_totalDamage:F0} | Hits: {_hitCount} | Max hit: {_maxHit:F0}", previousColor);
            }

            if (showTargets)
            {
                var alive = 0;
                foreach (var pair in _tracked)
                {
                    if (pair.Key != null && pair.Key.IsAlive) alive++;
                }

                DrawLine(ref y, $"Targets: {_tracked.Count} (alive {alive}) | Kills: {_kills}", previousColor);
            }

            if (showLog)
            {
                for (var i = _log.Count - 1; i >= 0; i--)
                {
                    var record = _log[i];
                    var suffix = record.Killed ? " [KILL]" : string.Empty;
                    DrawLine(ref y, $"{record.Target}: -{record.Amount:F0}{suffix}", record.Killed ? Color.red : Color.yellow);
                }
            }

            GUI.skin.label.fontSize = previousFontSize;
            GUI.skin.label.alignment = previousAlignment;
            GUI.color = previousColor;
        }

        private void DrawLine(ref float y, string text, Color color)
        {
            GUI.color = color;
            GUI.Label(new Rect(position.x, y, labelWidth, fontSize + lineSpacing), text);
            y += fontSize + lineSpacing;
        }

        private Color GetDpsColor(float dps)
        {
            if (dps < lowDps) return Color.red;
            if (dps < mediumDps) return Color.yellow;
            return Color.green;
        }
    }
}