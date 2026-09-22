//──────────────────────────────────────────────────────────────
// ICarInputSource.cs
// Defines the contract that every input source must satisfy.
// CarInputAdapter reads from whichever source is active.
// Adding a new source (AI replay, network spectator, etc.) means
// implementing this interface only – nothing else changes.
//──────────────────────────────────────────────────────────────

namespace ALIyerEdon.RemoteInput
{
    /// <summary>
    /// Any object that can provide throttle/steer/handbrake values
    /// to a car each frame.
    /// </summary>
    public interface ICarInputSource
    {
        /// <summary>Throttle/brake axis.  -1 = full brake/reverse, +1 = full throttle.</summary>
        float Motor     { get; }

        /// <summary>Steering axis.  -1 = full left, +1 = full right.</summary>
        float Steer     { get; }

        /// <summary>Hand-brake state.</summary>
        bool  HandBrake { get; }

        /// <summary>
        /// Whether this source is currently providing valid input.
        /// CarInputAdapter uses this to decide whether to fall back.
        /// </summary>
        bool  IsActive  { get; }
    }
}
