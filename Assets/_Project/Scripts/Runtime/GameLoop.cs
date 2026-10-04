using System;
using AnvilClicker.Core;
using UnityEngine;

namespace AnvilClicker.Runtime
{
    /// <summary>Drives passive production every frame and saves periodically and whenever the game may close.</summary>
    [RequireComponent(typeof(GameBootstrap))]
    public sealed class GameLoop : MonoBehaviour, IGameContextConsumer
    {
        GameContext _context;
        SaveSystem _saveSystem;
        float _secondsSinceSave;

        public void Bind(GameContext context) => _context = context;

        void Start() => _saveSystem = GetComponent<GameBootstrap>().SaveSystem;

        void Update()
        {
            if (_context == null) return;

            _context.Tick(Time.deltaTime);

            _secondsSinceSave += Time.unscaledDeltaTime;
            if (_secondsSinceSave >= _context.Balance.AutosaveIntervalSeconds) SaveNow();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) SaveNow();
        }

        void OnApplicationFocus(bool focused)
        {
            if (!focused) SaveNow();
        }

        void OnApplicationQuit() => SaveNow();

        public void SaveNow()
        {
            _secondsSinceSave = 0f;
            if (_context == null || _saveSystem == null) return;

            try
            {
                _saveSystem.Save(_context.State);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
