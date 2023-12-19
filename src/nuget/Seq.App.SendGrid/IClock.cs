using System;

namespace Seq.App.SendGrid;

/// <summary>
/// Interface IClock
/// </summary>
internal interface IClock
{
    /// <summary>
    /// Gets the UTC now.
    /// </summary>
    /// <value>The UTC now.</value>
    DateTime UtcNow { get; }
}