﻿using System;
using System.ComponentModel;

namespace Seq.App.SendGrid.Mailer;

/// <summary>
/// Interface IHideObjectMembers
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IHideObjectMembers
{
    /// <summary>
    /// Gets the type.
    /// </summary>
    /// <returns>Type.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    Type GetType();

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    int GetHashCode();

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    string ToString();

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
    /// </summary>
    /// <param name="object">The <see cref="System.Object" /> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    bool Equals(object @object);
}