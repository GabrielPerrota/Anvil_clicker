using AnvilClicker.Core;
using AnvilClicker.Data;
using UnityEngine;

namespace AnvilClicker.Runtime
{
    /// <summary>
    /// Composition root: builds the game state and services, then hands them to every
    /// <see cref="IGameContextConsumer"/> in the scene. The only place where systems are wired.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] GameDatabase database;

        [Tooltip("0 = different rolls every session. Any other value makes crits reproducible.")]
        [SerializeField] int randomSeed;

        void Awake()
        {
            if (database == null || database.Balance == null)
            {
                Debug.LogError("GameBootstrap needs a GameDatabase with a GameBalanceConfig.", this);
                enabled = false;
                return;
            }

            // M2 replaces this with the save system.
            var state = new GameState();
            var random = randomSeed == 0 ? new SystemRandom() : new SystemRandom(randomSeed);
            var context = new GameContext(state, database.Balance, database, random);

            BindConsumers(context);
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
