using System;
using AnvilClicker.Core;
using AnvilClicker.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnvilClicker.Presentation
{
    /// <summary>"Welcome back" modal summarising what the apprentices produced while the game was closed.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class OfflineSummaryPresenter : MonoBehaviour, IGameContextConsumer
    {
        const string HiddenClass = "hidden";

        [SerializeField] string awayFormat = "Você esteve fora por {0}.";
        [SerializeField] string weaponsFormat = "Seus aprendizes forjaram {0} armas.";
        [SerializeField] string goldFormat = "+{0} ouro";
        [SerializeField] string capFormat = "Produção limitada às primeiras {0} ({1}% de eficiência).";
        [SerializeField] string efficiencyFormat = "Fora do jogo, os aprendizes rendem {0}% do normal.";

        GameContext _context;
        VisualElement _modal;
        Label _time;
        Label _weapons;
        Label _gold;
        Label _cap;

        public void Bind(GameContext context) => _context = context;

        void Start()
        {
            if (_context == null) return;

            var root = GetComponent<UIDocument>().rootVisualElement;
            _modal = root.Q<VisualElement>("offline-modal");
            _time = root.Q<Label>("offline-time");
            _weapons = root.Q<Label>("offline-weapons");
            _gold = root.Q<Label>("offline-gold");
            _cap = root.Q<Label>("offline-cap");

            var button = root.Q<Button>("offline-continue");
            button.focusable = false;
            button.clicked += Hide;

            _context.OfflineProgressApplied += Show;
            Show(_context.LastOfflineReport);
        }

        void OnDestroy()
        {
            if (_context != null) _context.OfflineProgressApplied -= Show;
        }

        void Show(OfflineReport report)
        {
            if (!report.HasProgress) return;

            var efficiency = Mathf.RoundToInt((float)(_context.Balance.OfflineEfficiency * 100d));
            _time.text = string.Format(awayFormat, FormatDuration(report.Away));
            _weapons.text = string.Format(weaponsFormat, NumberFormatter.Format(report.WeaponsForged));
            _gold.text = string.Format(goldFormat, NumberFormatter.Format(report.GoldEarned));
            _cap.text = report.WasCapped
                ? string.Format(capFormat, FormatDuration(report.Credited), efficiency)
                : string.Format(efficiencyFormat, efficiency);

            _modal.RemoveFromClassList(HiddenClass);
        }

        void Hide() => _modal.AddToClassList(HiddenClass);

        /// <summary>"2 h 13 min", "45 min" or "3 dias 4 h".</summary>
        public static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1) return $"{(int)duration.TotalDays} {((int)duration.TotalDays == 1 ? "dia" : "dias")} {duration.Hours} h";
            if (duration.TotalHours >= 1) return duration.Minutes > 0 ? $"{(int)duration.TotalHours} h {duration.Minutes} min" : $"{(int)duration.TotalHours} h";
            return $"{Math.Max(1, (int)duration.TotalMinutes)} min";
        }
    }
}
