using System.Collections.Generic;
using System.Linq;

namespace Seq.App.SendGrid.Mailer.Models;

/// <summary>
/// Class SendResponse.
/// </summary>
public class SendResponse
{
    /// <summary>
    /// Gets or sets the message identifier.
    /// </summary>
    /// <value>The message identifier.</value>
    public string? MessageId { get; set; }

    /// <summary>
    /// Gets or sets the error messages.
    /// </summary>
    /// <value>The error messages.</value>
    public IList<string> ErrorMessages { get; set; } = new List<string>();

    /// <summary>
    /// Gets a value indicating whether this <see cref="SendResponse"/> is successful.
    /// </summary>
    /// <value><c>true</c> if successful; otherwise, <c>false</c>.</value>
    public bool Successful => !ErrorMessages.Any();
}

/// <summary>
/// Class SendResponse.
/// Implements the <see cref="Platform.Mailer.Models.SendResponse" />
/// </summary>
/// <typeparam name="TData">The type of the t data.</typeparam>
/// <seealso cref="Platform.Mailer.Models.SendResponse" />
public class SendResponse<TData> : SendResponse
{
    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    public TData Data { get; set; }
}