using System.Threading;
using System.Threading.Tasks;
using Seq.App.SendGrid.Mailer.Models;

namespace Seq.App.SendGrid.Mailer.Interfaces;

/// <summary>
/// Interface ISender
/// </summary>
public interface ISender
{
    /// <summary>
    /// Sends the specified email.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>SendResponse.</returns>
    SendResponse Send(IEmail email, CancellationToken? token = null);

    /// <summary>
    /// Sends the asynchronous.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task&lt;SendResponse&gt;.</returns>
    Task<SendResponse> SendAsync(IEmail email, CancellationToken? token = null);
}