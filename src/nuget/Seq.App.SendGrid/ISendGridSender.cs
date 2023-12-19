using System.Threading;
using System.Threading.Tasks;
using Seq.App.SendGrid.Mailer;
using Seq.App.SendGrid.Mailer.Interfaces;
using Seq.App.SendGrid.Mailer.Models;

namespace Seq.App.SendGrid;

/// <summary>
/// Interface ISendGridSender
/// Implements the <see cref="ISender" />
/// </summary>
/// <seealso cref="ISender" />
public interface ISendGridSender : ISender
{
    /// <summary>
    /// SendGrid specific extension method that allows you to use a template instead of a message body.
    /// For more information, see: https://sendgrid.com/docs/ui/sending-email/how-to-send-an-email-with-dynamic-transactional-templates/.
    /// </summary>
    /// <param name="email">email.</param>
    /// <param name="templateId">SendGrid template ID.</param>
    /// <param name="templateData">SendGrid template data.</param>
    /// <param name="token">Optional cancellation token.</param>
    /// <returns>A SendResponse object.</returns>
    Task<SendResponse> SendWithTemplateAsync(IEmail email, string templateId, object templateData, CancellationToken? token = null);
}