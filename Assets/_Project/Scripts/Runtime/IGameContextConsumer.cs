using AnvilClicker.Core;

namespace AnvilClicker.Runtime
{
    /// <summary>
    /// Implemented by scene components that need the game services. <see cref="GameBootstrap"/> calls
    /// <see cref="Bind"/> during its Awake, before any other component's Start runs.
    /// Store the context in Bind; subscribe to events in Start and unsubscribe in OnDestroy.
    /// </summary>
    public interface IGameContextConsumer
    {
        void Bind(GameContext context);
    }
}
