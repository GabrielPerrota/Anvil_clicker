using System.Collections.Generic;
using AnvilClicker.Core;
using AnvilClicker.Runtime;
using UnityEngine;

namespace AnvilClicker.Presentation
{
    /// <summary>
    /// Shows hired apprentices at work in the workshop: one worker per apprentice type that has been hired,
    /// standing at a fixed spot and hammering, with a counter above. (Not one sprite per unit: at 200 units
    /// that would fill the room.)
    /// </summary>
    public sealed class ApprenticeWorkers : MonoBehaviour, IGameContextConsumer
    {
        [SerializeField] Sprite workerSprite;
        [Tooltip("One spot per apprentice type, in catalog order.")]
        [SerializeField] Transform[] spots;
        [SerializeField] Color[] tints =
        {
            new Color(0.95f, 0.85f, 0.7f), new Color(0.7f, 0.85f, 1f), new Color(0.8f, 1f, 0.75f), new Color(1f, 0.8f, 0.5f)
        };
        [SerializeField] float hammerHeight = 0.07f;
        [SerializeField] float hammerSpeed = 9f;
        [SerializeField] string sortingLayerName = "World";

        sealed class Worker
        {
            public Transform Root;
            public Transform Body;
            public TextMesh Counter;
            public IApprenticeDefinition Apprentice;
            public float Phase;
        }

        readonly List<Worker> _workers = new List<Worker>();
        GameContext _context;
        Font _font;

        public void Bind(GameContext context) => _context = context;

        void Start()
        {
            if (_context == null) return;

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var apprentices = _context.Catalog.Apprentices;
            for (var i = 0; i < apprentices.Count && i < spots.Length; i++) _workers.Add(CreateWorker(apprentices[i], spots[i], i));

            _context.Workforce.WorkforceChanged += OnWorkforceChanged;
            RefreshAll();
        }

        void OnDestroy()
        {
            if (_context != null) _context.Workforce.WorkforceChanged -= OnWorkforceChanged;
        }

        void OnWorkforceChanged(IApprenticeDefinition apprentice, int count) => RefreshAll();

        Worker CreateWorker(IApprenticeDefinition apprentice, Transform spot, int index)
        {
            var root = new GameObject($"Worker - {apprentice.DisplayName}").transform;
            root.SetParent(spot, false);

            var body = new GameObject("Body").AddComponent<SpriteRenderer>();
            body.transform.SetParent(root, false);
            body.sprite = workerSprite;
            body.color = tints[index % tints.Length];
            body.sortingLayerName = sortingLayerName;

            var label = new GameObject("Counter");
            label.transform.SetParent(root, false);
            label.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            var text = label.AddComponent<TextMesh>();
            text.font = _font;
            text.fontSize = 48;
            text.characterSize = 0.05f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color(1f, 0.85f, 0.35f);
            var renderer = label.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = _font.material;
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = 5;

            return new Worker { Root = root, Body = body.transform, Counter = text, Apprentice = apprentice, Phase = index * 0.9f };
        }

        void RefreshAll()
        {
            foreach (var worker in _workers)
            {
                var count = _context.Workforce.GetCount(worker.Apprentice);
                worker.Root.gameObject.SetActive(count > 0);
                worker.Counter.text = "×" + NumberFormatter.Format(count);
            }
        }

        void Update()
        {
            foreach (var worker in _workers)
            {
                if (!worker.Root.gameObject.activeSelf) continue;
                var hit = Mathf.Abs(Mathf.Sin(Time.time * hammerSpeed + worker.Phase));
                worker.Body.localPosition = new Vector3(0f, hit * hammerHeight, 0f);
            }
        }
    }
}
