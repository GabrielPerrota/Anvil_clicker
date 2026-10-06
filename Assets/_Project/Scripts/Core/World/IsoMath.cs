using System;

namespace AnvilClicker.Core
{
    /// <summary>The four screen directions a character sprite can face on an isometric map.</summary>
    public enum FacingDirection
    {
        DownRight,
        DownLeft,
        UpLeft,
        UpRight
    }

    /// <summary>Movement helpers for a 2:1 isometric view. Pure math: no engine types.</summary>
    public static class IsoMath
    {
        /// <summary>World units are drawn twice as wide as tall, so vertical movement is squashed by this factor.</summary>
        public const double VerticalSquash = 0.5;

        /// <summary>
        /// Turns a "screen-relative" input (W = up on screen, D = right) into a world velocity direction.
        /// Vertical movement is squashed so the player crosses the screen at a similar visual speed
        /// in every direction. The result is normalised by the visual length, so diagonals are not faster.
        /// </summary>
        public static (double x, double y) ScreenInputToWorld(double inputX, double inputY)
        {
            if (double.IsNaN(inputX) || double.IsNaN(inputY)) return (0d, 0d);

            var length = Math.Sqrt(inputX * inputX + inputY * inputY);
            if (length < 1e-9) return (0d, 0d);

            // Direction on screen, normalised, then squashed vertically.
            return (inputX / length, inputY / length * VerticalSquash);
        }

        /// <summary>The sprite direction closest to a screen-space movement vector, keeping <paramref name="current"/> when idle.</summary>
        public static FacingDirection GetFacing(double inputX, double inputY, FacingDirection current)
        {
            if (Math.Abs(inputX) < 1e-9 && Math.Abs(inputY) < 1e-9) return current;

            if (inputX >= 0) return inputY <= 0 ? FacingDirection.DownRight : FacingDirection.UpRight;
            return inputY <= 0 ? FacingDirection.DownLeft : FacingDirection.UpLeft;
        }
    }
}
