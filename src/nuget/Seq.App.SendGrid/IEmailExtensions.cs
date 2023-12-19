using System.Threading.Tasks;
using Seq.App.SendGrid.Mailer;
using Seq.App.SendGrid.Mailer.Models;

namespace Seq.App.SendGrid;

/// <summary>
/// Class IEmailExtensions.
/// </summary>
public static class IEmailExtensions
{
    /// <summary>
    /// Send with template as an asynchronous operation.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="templateId">The template identifier.</param>
    /// <param name="templateData">The template data.</param>
    /// <returns>A Task&lt;SendResponse&gt; representing the asynchronous operation.</returns>
    public static async Task<SendResponse> SendWithTemplateAsync(this IEmail email, string templateId, object templateData)
    {
        var sendGridSender = email.Sender as ISendGridSender;
        return await sendGridSender.SendWithTemplateAsync(email, templateId, templateData);
    }
}