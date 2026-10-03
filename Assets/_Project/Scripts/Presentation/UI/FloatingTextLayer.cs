using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnvilClicker.Presentation
{
    public enum FloatingTextStyle
    {
        Normal,
        Critical,
        Reward
    }

    /// <summary>Pooled UI Toolkit labels that rise and fade from a world position.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class FloatingTextLayer : MonoBehaviour
    {
        const string LayerName = "floating-layer";
        const string BaseClass = "floating-text";

        [SerializeField] Camera worldCamera;
        [SerializeField] float lifetime = 0.9f;
        [Tooltip("World units travelled upwards over the lifetime.")]
        [SerializeField] float rise = 1.2f;
        [SerializeField] int prewarm = 24;

        sealed class Entry
        {
            public Label Label;
            public Vector3 WorldOrigin;
            public float Age;
        }

        readonly List<Entry> _active = new List<Entry>();
        readonly Stack<Entry> _pool = new Stack<Entry>();
        VisualElement _layer;

        void Start()
        {
            _layer = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(LayerName);
            if (_layer == null)
            {
                Debug.LogError($"UI document has no element named '{LayerName}'.", this);
                enabled = false;
                return;
            }

            for (var i = 0; i < prewarm; i++) _pool.Push(CreateEntry());
        }

        public void Show(Vector3 worldPosition, string text, FloatingTextStyle style)
        {
            if (_layer == null) return;

            var entry = _pool.Count > 0 ? _pool.Pop() : CreateEntry();
            entry.Label.text = text;
            entry.Label.ClearClassList();
            entry.Label.AddToClassList(BaseClass);
            entry.Label.AddToClassList($"{BaseClass}--{style.ToString().ToLowerInvariant()}");
            entry.Label.style.display = DisplayStyle.Flex;
            entry.WorldOrigin = worldPosition;
            entry.Age = 0f;

            entry.Label.BringToFront();
            _active.Add(entry);
            Place(entry);
        }

        void Update()
        {
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var entry = _active[i];
                entry.Age += Time.deltaTime;

                if (entry.Age >= lifetime)
                {
                    entry.Label.style.display = DisplayStyle.None;
                    _active.RemoveAt(i);
                    _pool.Push(entry);
                    continue;
                }

                Place(entry);
            }
        }

        void Place(Entry entry)
        {
            var t = entry.Age / lifetime;
            var eased = 1f - (1f - t) * (1f - t); // ease-out: fast start, gentle stop
            var world = entry.WorldOrigin + Vector3.up * (rise * eased);

            var cam = worldCamera != null ? worldCamera : Camera.main;
            if (cam == null || _layer.panel == null) return;

            var panelPosition = RuntimePanelUtils.CameraTransformWorldToPanel(_layer.panel, world, cam);
            entry.Label.style.left = panelPosition.x;
            entry.Label.style.top = panelPosition.y;
            entry.Label.style.opacity = 1f - t * t;
        }

        Entry CreateEntry()
        {
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.style.position = Position.Absolute;
            label.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));
            label.style.display = DisplayStyle.None;
            _layer.Add(label);
            return new Entry { Label = label };
        }
    }
}
