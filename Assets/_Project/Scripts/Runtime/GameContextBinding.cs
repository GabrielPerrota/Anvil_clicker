using AnvilClicker.Core;
using UnityEngine;

namespace AnvilClicker.Runtime
{
    /// <summary>Hands the game context to every consumer under an object created after startup (stations spawned with a room).</summary>
    public static class GameContextBinding
    {
        public static void BindAll(GameObject root, GameContext context)
        {
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour is IGameContextConsumer consumer) consumer.Bind(context);
            }
        }
    }
}
