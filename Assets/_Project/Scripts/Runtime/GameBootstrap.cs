using AnvilClicker.Core;
using AnvilClicker.Data;
using UnityEngine;

namespace AnvilClicker.Runtime
{
    /// <summary>
    /// Composition root: loads the save, builds the game services, credits offline progress and hands the
    /// context to every <see cref="IGameContextConsumer"/> in the scene. The only place where systems are wired.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] GameDatabase database;

        [Tooltip("0 = different rolls every session. Any other value makes crits reproducible.")]
        [SerializeField] int randomSeed;

        [Tooltip("Turn off to always start a fresh game (handy while testing).")]
        [SerializeField] bool loadSave = true;

        /// <summary>For editor debug tools only. Gameplay code receives the context through Bind.</summary>
        public GameContext Context { get; private set; }

        public SaveSystem SaveSystem { get; private set; }

        void Awake()
        {
            if (database == null || database.Balance == null)
            {
                Debug.LogError("GameBootstrap needs a GameDatabase with a GameBalanceConfig.", this);
                enabled = false;
                return;
            }

            var clock = new SystemClock();
            SaveSystem = new SaveSystem(SaveSystem.DefaultDirectory, new SaveSerializer(), clock);

            var state = new GameState();
            System.DateTime? savedAtUtc = null;
            if (loadSave && SaveSystem.TryLoad(out var envelope))
            {
                state = envelope.State;
                savedAtUtc = envelope.SavedAtUtc;
            }

            var random = randomSeed == 0 ? new SystemRandom() : new SystemRandom(randomSeed);
            Context = new GameContext(state, database.Balance, database, random);

            // Applied before Bind so the burst of weapons does not trigger strike feedback.
            if (savedAtUtc.HasValue) Context.ApplyOfflineProgress(clock.UtcNow - savedAtUtc.Value);

            BindConsumers(Context);
        }

        static void BindConsumers(GameContext context)
        {
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var behaviour in behaviours)
            {
                if (behaviour is IGameContextConsumer consumer)
                    consumer.Bind(context);
            }
        }
    }
}
