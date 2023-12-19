using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Seq.App.SendGrid.Mailer;
using Seq.App.SendGrid.Mailer.Models;
using Attachment = Seq.App.SendGrid.Mailer.Models.Attachment;
using SendGridAttachment = SendGrid.Helpers.Mail.Attachment;

namespace Seq.App.SendGrid;

/// <summary>
/// Class SendGridSender.
/// Implements the <see cref="ISendGridSender" />
/// </summary>
/// <seealso cref="ISendGridSender" />
public class SendGridSender : ISendGridSender
{
    /// <summary>
    /// The API key
    /// </summary>
    private readonly string _apiKey;

    /// <summary>
    /// The sand box mode
    /// </summary>
    private readonly bool _sandBoxMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendGridSender"/> class.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <param name="sandBoxMode">if set to <c>true</c> [sand box mode].</param>
    public SendGridSender(string apiKey, bool sandBoxMode = false)
    {
        _apiKey = apiKey;
        _sandBoxMode = sandBoxMode;
    }

    /// <inheritdoc />
    public SendResponse Send(IEmail email, CancellationToken? token = null)
    {
        return SendAsync(email, token).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<SendResponse> SendAsync(IEmail email, CancellationToken? token = null)
    {
        var mailMessage = await BuildSendGridMessage(email);

        if (email.Data.IsHtml)
        {
            mailMessage.HtmlContent = email.Data.Body;
        }
        else
        {
            mailMessage.PlainTextContent = email.Data.Body;
        }

        if (!string.IsNullOrEmpty(email.Data.PlaintextAlternativeBody))
        {
            mailMessage.PlainTextContent = email.Data.PlaintextAlternativeBody;
        }

        var sendResponse = await SendViaSendGrid(mailMessage, token);

        return sendResponse;
    }

    /// <inheritdoc />
    public async Task<SendResponse> SendWithTemplateAsync(IEmail email, string templateId, object templateData, CancellationToken? token = null)
    {
        var mailMessage = await BuildSendGridMessage(email);

        mailMessage.SetTemplateId(templateId);
        mailMessage.SetTemplateData(templateData);

        var sendResponse = await SendViaSendGrid(mailMessage, token);

        return sendResponse;
    }

    /// <summary>
    /// Builds the send grid message.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <returns>SendGridMessage.</returns>
    private async Task<SendGridMessage> BuildSendGridMessage(IEmail email)
    {
        var mailMessage = new SendGridMessage();
        mailMessage.SetSandBoxMode(_sandBoxMode);

        mailMessage.SetFrom(ConvertAddress(email.Data.FromAddress));

        if (email.Data.ToAddresses.Any(a => !string.IsNullOrWhiteSpace(a.EmailAddress)))
        {
            mailMessage.AddTos(email.Data.ToAddresses.Select(ConvertAddress).ToList());
        }

        if (email.Data.CcAddresses.Any(a => !string.IsNullOrWhiteSpace(a.EmailAddress)))
        {
            mailMessage.AddCcs(email.Data.CcAddresses.Select(ConvertAddress).ToList());
        }

        if (email.Data.BccAddresses.Any(a => !string.IsNullOrWhiteSpace(a.EmailAddress)))
        {
            mailMessage.AddBccs(email.Data.BccAddresses.Select(ConvertAddress).ToList());
        }

        if (email.Data.ReplyToAddresses.Any(a => !string.IsNullOrWhiteSpace(a.EmailAddress)))
        {
            // SendGrid does not support multiple ReplyTo addresses
            mailMessage.SetReplyTo(email.Data.ReplyToAddresses.Select(ConvertAddress).First());
        }

        mailMessage.SetSubject(email.Data.Subject);

        if (email.Data.Headers.Any())
        {
            mailMessage.AddHeaders(email.Data.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }

        if (email.Data.Tags != null && email.Data.Tags.Any())
        {
            mailMessage.Categories = email.Data.Tags.ToList();
        }

        if (email.Data.IsHtml)
        {
            mailMessage.HtmlContent = email.Data.Body;
        }
        else
        {
            mailMessage.PlainTextContent = email.Data.Body;
        }

        switch (email.Data.Priority)
        {
            case Priority.High:
                // https://stackoverflow.com/questions/23230250/set-email-priority-with-sendgrid-api
                mailMessage.AddHeader("Priority", "Urgent");
                mailMessage.AddHeader("Importance", "High");

                // https://docs.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxcmail/2bb19f1b-b35e-4966-b1cb-1afd044e83ab
                mailMessage.AddHeader("X-Priority", "1");
                mailMessage.AddHeader("X-MSMail-Priority", "High");
                break;

            case Priority.Normal:
                // Do not set anything.
                // Leave default values. It means Normal Priority.
                break;

            case Priority.Low:
                // https://stackoverflow.com/questions/23230250/set-email-priority-with-sendgrid-api
                mailMessage.AddHeader("Priority", "Non-Urgent");
                mailMessage.AddHeader("Importance", "Low");

                // https://docs.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxcmail/2bb19f1b-b35e-4966-b1cb-1afd044e83ab
                mailMessage.AddHeader("X-Priority", "5");
                mailMessage.AddHeader("X-MSMail-Priority", "Low");
                break;
        }

        if (email.Data.Attachments.Any())
        {
            foreach (var attachment in email.Data.Attachments)
            {
                var sendGridAttachment = await ConvertAttachment(attachment);
                mailMessage.AddAttachment(sendGridAttachment.Filename, sendGridAttachment.Content, sendGridAttachment.Type, sendGridAttachment.Disposition, sendGridAttachment.ContentId);
            }
        }

        return mailMessage;
    }

    /// <summary>
    /// Sends the via send grid.
    /// </summary>
    /// <param name="mailMessage">The mail message.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>SendResponse.</returns>
    private async Task<SendResponse> SendViaSendGrid(SendGridMessage mailMessage, CancellationToken? token = null)
    {
        var sendGridClient = new SendGridClient(_apiKey);
        var sendGridResponse = await sendGridClient.SendEmailAsync(mailMessage, token.GetValueOrDefault());

        var sendResponse = new SendResponse();

        if (sendGridResponse.Headers.TryGetValues("X-Message-ID", out var messageIds))
        {
            sendResponse.MessageId = messageIds.FirstOrDefault();
        }

        if (IsHttpSuccess((int)sendGridResponse.StatusCode))
        {
            return sendResponse;
        }

        sendResponse.ErrorMessages.Add($"{sendGridResponse.StatusCode}");
        var messageBodyDictionary = await sendGridResponse.DeserializeResponseBodyAsync();

        if (messageBodyDictionary.ContainsKey("errors"))
        {
            var errors = messageBodyDictionary["errors"];

            foreach (var error in errors)
            {
                sendResponse.ErrorMessages.Add($"{error}");
            }
        }

        return sendResponse;
    }

    /// <summary>
    /// Converts the address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>EmailAddress.</returns>
    private EmailAddress ConvertAddress(Address address)
    {
        return new EmailAddress(address.EmailAddress, address.Name);
    }

    /// <summary>
    /// Converts the attachment.
    /// </summary>
    /// <param name="attachment">The attachment.</param>
    /// <returns>SendGridAttachment.</returns>
    private async Task<SendGridAttachment> ConvertAttachment(Attachment attachment)
    {
        return new SendGridAttachment { Content = await GetAttachmentBase64String(attachment.Data), Filename = attachment.Filename, Type = attachment.ContentType, };
    }

    /// <summary>
    /// Gets the attachment base64 string.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>System.String.</returns>
    private async Task<string> GetAttachmentBase64String(Stream stream)
    {
        using (var ms = new MemoryStream())
        {
            await stream.CopyToAsync(ms);
            return Convert.ToBase64String(ms.ToArray());
        }
    }

    /// <summary>
    /// Determines whether [is HTTP success] [the specified status code].
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <returns><c>true</c> if [is HTTP success] [the specified status code]; otherwise, <c>false</c>.</returns>
    private bool IsHttpSuccess(int statusCode)
    {
        return statusCode is >= 200 and < 300;
    }
}