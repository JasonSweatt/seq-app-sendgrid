using System;

namespace Seq.App.SendGrid;

/// <summary>
/// Class SystemClock.
/// Implements the <see cref="Seq.App.SendGrid.IClock" />
/// </summary>
/// <seealso cref="Seq.App.SendGrid.IClock" />
public class SystemClock : IClock
{
    /// <summary>
    /// Gets the UTC now.
    /// </summary>
    /// <value>The UTC now.</value>
    public DateTime UtcNow => DateTime.UtcNow;
}