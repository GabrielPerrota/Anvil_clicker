using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace AnvilClicker.Core
{
    /// <summary>What a save file contains: the game state plus metadata for migrations and offline progress.</summary>
    public sealed class SaveEnvelope
    {
        public int SaveVersion;
        public string GameVersion;
        public DateTime SavedAtUtc;
        public GameState State;
    }

    /// <summary>
    /// Upgrades a save from <see cref="FromVersion"/> to <see cref="FromVersion"/> + 1 by editing the raw JSON,
    /// so old files keep loading after fields are renamed or restructured.
    /// </summary>
    public interface ISaveMigration
    {
        int FromVersion { get; }

        void Migrate(JObject envelope);
    }

    public sealed class SaveFormatException : Exception
    {
        public SaveFormatException(string message, Exception inner = null) : base(message, inner) { }
    }

    /// <summary>Every migration shipped with the game, in any order.</summary>
    public static class SaveMigrations
    {
        public static readonly IReadOnlyList<ISaveMigration> All = Array.Empty<ISaveMigration>();
    }

    /// <summary>Converts game state to and from versioned JSON. Pure: no file access.</summary>
    public sealed class SaveSerializer
    {
        const string VersionProperty = "saveVersion";

        static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            // camelCase fields, but leave dictionary keys (definition ids) exactly as they are.
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false } }
        };

        readonly Dictionary<int, ISaveMigration> _migrations = new Dictionary<int, ISaveMigration>();
        readonly int _currentVersion;

        public SaveSerializer() : this(SaveMigrations.All, GameState.CurrentSaveVersion) { }

        public SaveSerializer(IEnumerable<ISaveMigration> migrations, int currentVersion)
        {
            if (currentVersion < 1) throw new ArgumentOutOfRangeException(nameof(currentVersion));
            _currentVersion = currentVersion;

            foreach (var migration in migrations ?? throw new ArgumentNullException(nameof(migrations)))
            {
                if (!_migrations.TryAdd(migration.FromVersion, migration))
                    throw new ArgumentException($"Two migrations start at version {migration.FromVersion}.", nameof(migrations));
            }
        }

        public string Serialize(GameState state, DateTime savedAtUtc, string gameVersion)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            state.SaveVersion = _currentVersion;
            var envelope = new SaveEnvelope
            {
                SaveVersion = _currentVersion,
                GameVersion = gameVersion,
                SavedAtUtc = DateTime.SpecifyKind(savedAtUtc, DateTimeKind.Utc),
                State = state
            };
            return JsonConvert.SerializeObject(envelope, Settings);
        }

        /// <exception cref="SaveFormatException">The text is not a valid save for this game version.</exception>
        public SaveEnvelope Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new SaveFormatException("Save is empty.");

            JObject root;
            try
            {
                // Keep dates as strings until the typed pass so their UTC kind is not lost.
                using var reader = new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None };
                root = JObject.Load(reader);
            }
            catch (JsonException exception)
            {
                throw new SaveFormatException("Save is not valid JSON.", exception);
            }

            var version = root.Value<int?>(VersionProperty) ?? throw new SaveFormatException("Save has no version.");
            if (version > _currentVersion)
                throw new SaveFormatException($"Save version {version} is newer than this game ({_currentVersion}).");

            while (version < _currentVersion)
            {
                if (!_migrations.TryGetValue(version, out var migration))
                    throw new SaveFormatException($"No migration from save version {version}.");

                migration.Migrate(root);
                version++;
                root[VersionProperty] = version;
            }

            SaveEnvelope envelope;
            try
            {
                envelope = root.ToObject<SaveEnvelope>(JsonSerializer.Create(Settings));
            }
            catch (JsonException exception)
            {
                throw new SaveFormatException("Save content does not match the game state.", exception);
            }

            if (envelope?.State == null) throw new SaveFormatException("Save has no game state.");

            envelope.State.SaveVersion = _currentVersion;
            envelope.State.UpgradeLevels ??= new Dictionary<string, int>();
            envelope.State.Apprentices ??= new Dictionary<string, int>();
            envelope.SavedAtUtc = DateTime.SpecifyKind(envelope.SavedAtUtc, DateTimeKind.Utc);
            return envelope;
        }
    }
}
