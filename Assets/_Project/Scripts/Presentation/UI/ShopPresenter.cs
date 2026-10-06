using System.Collections.Generic;
using AnvilClicker.Core;
using AnvilClicker.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnvilClicker.Presentation
{
    /// <summary>
    /// The upgrade desk's panel: upgrades, apprentices (bought ×1, ×10, ×100 or as many as affordable) and
    /// rooms. Rows are built from the catalog; purchases go through the Core services only.
    /// Showing and hiding the panel is <see cref="StationPanelsPresenter"/>'s job.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ShopPresenter : MonoBehaviour, IGameContextConsumer
    {
        const string ActiveTabClass = "shop-tab--active";
        const string ActiveAmountClass = "buy-amount--active";
        const string HiddenClass = "hidden";
        const int MaxAmount = -1;

        enum Tab
        {
            Upgrades,
            Apprentices,
            Rooms
        }

        [SerializeField] VisualTreeAsset rowTemplate;
        [Tooltip("Seconds between affordability refreshes while gold changes every frame.")]
        [SerializeField] float refreshInterval = 0.1f;

        [Header("Texts")]
        [SerializeField] string levelFormat = "Nv. {0}";
        [SerializeField] string levelWithMaxFormat = "Nv. {0}/{1}";
        [SerializeField] string countFormat = "×{0}";
        [SerializeField] string apprenticeDetailFormat = "{0} PF/s cada · total {1} PF/s";
        [SerializeField] string quantityFormat = "×{0}";
        [SerializeField] string maxedText = "MÁX";
        [SerializeField] string builtText = "Construída";
        [SerializeField] string roomBuiltDetail = "Já faz parte da ferraria.";

        sealed class Row
        {
            public VisualElement Root;
            public Label Level;
            public Label Detail;
            public Label Quantity;
            public Label Cost;
            public Button Buy;
            public IUpgradeDefinition Upgrade;
            public IApprenticeDefinition Apprentice;
            public IRoomDefinition Room;
        }

        readonly List<Row> _upgradeRows = new List<Row>();
        readonly List<Row> _apprenticeRows = new List<Row>();
        readonly List<Row> _roomRows = new List<Row>();
        readonly List<(Button button, int amount)> _amountButtons = new List<(Button, int)>();

        GameContext _context;
        ScrollView _list;
        Label _empty;
        VisualElement _amountBar;
        Button _upgradesTab;
        Button _apprenticesTab;
        Button _roomsTab;
        Tab _tab = Tab.Upgrades;
        int _buyAmount = 1;
        bool _dirty = true;
        float _sinceRefresh;

        public void Bind(GameContext context) => _context = context;

        void Start()
        {
            if (_context == null || rowTemplate == null)
            {
                if (rowTemplate == null) Debug.LogError("ShopPresenter needs a row template.", this);
                enabled = false;
                return;
            }

            var root = GetComponent<UIDocument>().rootVisualElement;
            _list = root.Q<ScrollView>("shop-list");
            _empty = root.Q<Label>("shop-empty");
            _amountBar = root.Q<VisualElement>("buy-amounts");
            _upgradesTab = BindTab(root, "tab-upgrades", Tab.Upgrades);
            _apprenticesTab = BindTab(root, "tab-apprentices", Tab.Apprentices);
            _roomsTab = BindTab(root, "tab-rooms", Tab.Rooms);

            BindAmount(root, "amount-1", 1);
            BindAmount(root, "amount-10", 10);
            BindAmount(root, "amount-100", 100);
            BindAmount(root, "amount-max", MaxAmount);

            foreach (var upgrade in _context.Catalog.Upgrades) _upgradeRows.Add(CreateRow(upgrade, null, null));
            foreach (var apprentice in _context.Catalog.Apprentices) _apprenticeRows.Add(CreateRow(null, apprentice, null));
            foreach (var room in _context.Catalog.Rooms)
            {
                // The workshop you start in is never for sale.
                if (room.Cost > 0) _roomRows.Add(CreateRow(null, null, room));
            }

            _context.Wallet.GoldChanged += OnGoldChanged;
            _context.Upgrades.UpgradePurchased += OnUpgradePurchased;
            _context.Workforce.WorkforceChanged += OnWorkforceChanged;
            _context.Rooms.RoomUnlocked += OnRoomUnlocked;
            _context.Modifiers.Changed += MarkDirty;

            ShowTab(Tab.Upgrades);
        }

        void OnDestroy()
        {
            if (_context == null) return;
            _context.Wallet.GoldChanged -= OnGoldChanged;
            _context.Upgrades.UpgradePurchased -= OnUpgradePurchased;
            _context.Workforce.WorkforceChanged -= OnWorkforceChanged;
            _context.Rooms.RoomUnlocked -= OnRoomUnlocked;
            _context.Modifiers.Changed -= MarkDirty;
        }

        void Update()
        {
            _sinceRefresh += Time.unscaledDeltaTime;
            if (!_dirty || _sinceRefresh < refreshInterval) return;
            Refresh();
        }

        void OnGoldChanged(double _) => MarkDirty();
        void OnUpgradePurchased(IUpgradeDefinition _, int __) => Refresh();
        void OnWorkforceChanged(IApprenticeDefinition _, int __) => Refresh();
        void OnRoomUnlocked(IRoomDefinition _) => Refresh();
        void MarkDirty() => _dirty = true;

        // --- Building ------------------------------------------------------------------------------

        Row CreateRow(IUpgradeDefinition upgrade, IApprenticeDefinition apprentice, IRoomDefinition room)
        {
            var element = rowTemplate.Instantiate();
            var row = new Row
            {
                Root = element,
                Level = element.Q<Label>("row-level"),
                Detail = element.Q<Label>("row-detail"),
                Quantity = element.Q<Label>("row-quantity"),
                Cost = element.Q<Label>("row-cost"),
                Buy = element.Q<Button>("row-buy"),
                Upgrade = upgrade,
                Apprentice = apprentice,
                Room = room
            };

            element.Q<Label>("row-name").text = upgrade != null ? upgrade.DisplayName : apprentice != null ? apprentice.DisplayName : room.DisplayName;
            if (upgrade != null) row.Detail.text = upgrade.Description;
            if (room != null) row.Detail.text = room.Description;

            MakeMouseOnly(row.Buy);
            row.Buy.clicked += () => Purchase(row);
            return row;
        }

        Button BindTab(VisualElement root, string name, Tab tab)
        {
            var button = root.Q<Button>(name);
            MakeMouseOnly(button);
            button.clicked += () => ShowTab(tab);
            return button;
        }

        void BindAmount(VisualElement root, string name, int amount)
        {
            var button = root.Q<Button>(name);
            MakeMouseOnly(button);
            button.clicked += () => SetAmount(amount);
            _amountButtons.Add((button, amount));
        }

        /// <summary>Space strikes the anvil, so shop buttons must never take keyboard focus.</summary>
        static void MakeMouseOnly(Button button) => button.focusable = false;

        // --- Interaction ---------------------------------------------------------------------------

        void ShowTab(Tab tab)
        {
            _tab = tab;
            _upgradesTab.EnableInClassList(ActiveTabClass, tab == Tab.Upgrades);
            _apprenticesTab.EnableInClassList(ActiveTabClass, tab == Tab.Apprentices);
            _roomsTab.EnableInClassList(ActiveTabClass, tab == Tab.Rooms);
            _amountBar.EnableInClassList(HiddenClass, tab == Tab.Rooms);

            _list.Clear();
            foreach (var row in CurrentRows()) _list.Add(row.Root);
            Refresh();
        }

        List<Row> CurrentRows() => _tab == Tab.Upgrades ? _upgradeRows : _tab == Tab.Apprentices ? _apprenticeRows : _roomRows;

        void SetAmount(int amount)
        {
            _buyAmount = amount;
            foreach (var (button, value) in _amountButtons) button.EnableInClassList(ActiveAmountClass, value == amount);
            Refresh();
        }

        void Purchase(Row row)
        {
            if (row.Room != null)
            {
                _context.Rooms.TryBuy(row.Room);
                return;
            }

            var quantity = GetQuantity(row);
            if (quantity <= 0) return;

            if (row.Upgrade != null) _context.Upgrades.TryBuy(row.Upgrade, quantity);
            else _context.Workforce.TryHire(row.Apprentice, quantity);
        }

        /// <summary>Units the buy button would purchase right now (Max falls back to 1 so a price is always shown).</summary>
        int GetQuantity(Row row)
        {
            if (row.Room != null) return 1;

            if (row.Upgrade != null)
            {
                var remaining = _context.Upgrades.GetRemainingLevels(row.Upgrade);
                if (remaining == 0) return 0;
                var wanted = _buyAmount == MaxAmount ? Mathf.Max(1, _context.Upgrades.GetMaxAffordable(row.Upgrade)) : _buyAmount;
                return Mathf.Min(wanted, remaining);
            }

            return _buyAmount == MaxAmount ? Mathf.Max(1, _context.Workforce.GetMaxAffordable(row.Apprentice)) : _buyAmount;
        }

        // --- Refresh -------------------------------------------------------------------------------

        void Refresh()
        {
            _dirty = false;
            _sinceRefresh = 0f;

            var anyVisible = false;
            foreach (var row in CurrentRows())
            {
                var unlocked = IsVisible(row);
                row.Root.EnableInClassList(HiddenClass, !unlocked);
                if (!unlocked) continue;

                anyVisible = true;
                if (row.Upgrade != null) RefreshUpgrade(row);
                else if (row.Apprentice != null) RefreshApprentice(row);
                else RefreshRoom(row);
            }

            _empty.EnableInClassList(HiddenClass, anyVisible);
        }

        bool IsVisible(Row row) =>
            row.Upgrade != null ? _context.Upgrades.IsUnlocked(row.Upgrade)
            : row.Apprentice != null ? _context.Workforce.IsUnlocked(row.Apprentice)
            : _context.Rooms.IsAvailable(row.Room);

        void RefreshUpgrade(Row row)
        {
            var upgrade = row.Upgrade;
            var level = _context.Upgrades.GetLevel(upgrade);
            row.Level.text = upgrade.MaxLevel > 0 ? string.Format(levelWithMaxFormat, level, upgrade.MaxLevel) : string.Format(levelFormat, level);

            if (_context.Upgrades.IsMaxed(upgrade))
            {
                row.Quantity.text = string.Empty;
                row.Cost.text = maxedText;
                row.Buy.SetEnabled(false);
                return;
            }

            var quantity = GetQuantity(row);
            RefreshPrice(row, quantity, _context.Upgrades.GetCost(upgrade, quantity));
        }

        void RefreshApprentice(Row row)
        {
            var apprentice = row.Apprentice;
            row.Level.text = string.Format(countFormat, _context.Workforce.GetCount(apprentice));

            var each = apprentice.BaseForgePointsPerSecond * _context.Modifiers.PassiveMultiplier;
            row.Detail.text = string.Format(apprenticeDetailFormat,
                NumberFormatter.Format(each),
                NumberFormatter.Format(_context.Workforce.GetForgePointsPerSecond(apprentice)));

            var quantity = GetQuantity(row);
            RefreshPrice(row, quantity, _context.Workforce.GetCost(apprentice, quantity));
        }

        void RefreshRoom(Row row)
        {
            var room = row.Room;
            row.Quantity.text = string.Empty;

            if (_context.Rooms.IsUnlocked(room))
            {
                row.Level.text = string.Empty;
                row.Detail.text = roomBuiltDetail;
                row.Cost.text = builtText;
                row.Buy.SetEnabled(false);
                return;
            }

            row.Level.text = string.Empty;
            row.Detail.text = room.Description;
            row.Cost.text = NumberFormatter.Format(_context.Rooms.GetCost(room));
            row.Buy.SetEnabled(_context.Rooms.CanBuy(room));
        }

        void RefreshPrice(Row row, int quantity, double cost)
        {
            row.Quantity.text = quantity > 1 ? string.Format(quantityFormat, quantity) : string.Empty;
            row.Cost.text = NumberFormatter.Format(cost);
            row.Buy.SetEnabled(_context.Wallet.CanAfford(cost));
        }
    }
}
