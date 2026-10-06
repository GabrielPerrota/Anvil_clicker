using System;
using System.Collections.Generic;

namespace AnvilClicker.Core
{
    /// <summary>Reads simple "-name value" pairs from the command line used to start the game.</summary>
    public static class LaunchArguments
    {
        /// <summary>Returns the value after <paramref name="name"/> (case-insensitive), or null if absent or without a value.</summary>
        public static string GetValue(IReadOnlyList<string> args, string name)
        {
            if (args == null || string.IsNullOrEmpty(name)) return null;

            for (var i = 0; i < args.Count - 1; i++)
            {
                if (!string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) continue;

                var value = args[i + 1];
                return string.IsNullOrWhiteSpace(value) || value.StartsWith("-", StringComparison.Ordinal) ? null : value;
            }

            return null;
        }
    }
}
