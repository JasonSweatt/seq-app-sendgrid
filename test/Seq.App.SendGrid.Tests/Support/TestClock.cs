using System;

namespace Seq.App.SendGrid.Tests.Support;

/// <summary>
/// Class TestClock.
/// Implements the <see cref="Seq.App.SendGrid.IClock" />
/// </summary>
/// <seealso cref="Seq.App.SendGrid.IClock" />
internal class TestClock : IClock
{
    /// <summary>
    /// Gets the UTC now.
    /// </summary>
    /// <value>The UTC now.</value>
    public DateTime UtcNow { get; set; }

    /// <summary>
    /// Advances the specified duration.
    /// </summary>
    /// <param name="duration">The duration.</param>
    /// <returns>DateTime.</returns>
    public DateTime Advance(TimeSpan duration)
    {
        UtcNow = UtcNow.Add(duration);
        return UtcNow;
    }
}