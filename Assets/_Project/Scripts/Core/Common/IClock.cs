using System;

namespace AnvilClicker.Core
{
    /// <summary>Wall-clock time source, injectable for save timestamps and offline progress tests.</summary>
    public interface IClock
    {
        DateTime UtcNow { get; }
    }

    public sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
