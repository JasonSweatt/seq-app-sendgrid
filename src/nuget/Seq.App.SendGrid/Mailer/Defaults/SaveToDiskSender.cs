using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Seq.App.SendGrid.Mailer.Interfaces;
using Seq.App.SendGrid.Mailer.Models;

namespace Seq.App.SendGrid.Mailer.Defaults;

/// <summary>
/// Class SaveToDiskSender.
/// Implements the <see cref="ISender" />
/// </summary>
/// <seealso cref="ISender" />
public class SaveToDiskSender : ISender
{
    /// <summary>
    /// The directory
    /// </summary>
    private readonly string _directory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveToDiskSender"/> class.
    /// </summary>
    /// <param name="directory">The directory.</param>
    public SaveToDiskSender(string directory)
    {
        _directory = directory;
    }

    /// <inheritdoc />
    public SendResponse Send(IEmail email, CancellationToken? token = null)
    {
        return SendAsync(email, token).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<SendResponse> SendAsync(IEmail email, CancellationToken? token = null)
    {
        var response = new SendResponse();
        await SaveEmailToDisk(email);
        return response;
    }

    /// <summary>
    /// Saves the email to disk.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    private async Task<bool> SaveEmailToDisk(IEmail email)
    {
        var random = new Random();
        var filename = Path.Combine(_directory, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{random.Next(1000)}");

        using (var sw = new StreamWriter(File.OpenWrite(filename)))
        {
            await sw.WriteLineAsync($"From: {email.Data.FromAddress.Name} <{email.Data.FromAddress.EmailAddress}>");
            await sw.WriteLineAsync($"To: {string.Join(",", email.Data.ToAddresses.Select(x => $"{x.Name} <{x.EmailAddress}>"))}");
            await sw.WriteLineAsync($"Cc: {string.Join(",", email.Data.CcAddresses.Select(x => $"{x.Name} <{x.EmailAddress}>"))}");
            await sw.WriteLineAsync($"Bcc: {string.Join(",", email.Data.BccAddresses.Select(x => $"{x.Name} <{x.EmailAddress}>"))}");
            await sw.WriteLineAsync($"ReplyTo: {string.Join(",", email.Data.ReplyToAddresses.Select(x => $"{x.Name} <{x.EmailAddress}>"))}");
            await sw.WriteLineAsync($"Subject: {email.Data.Subject}");
            foreach (var dataHeader in email.Data.Headers)
            {
                await sw.WriteLineAsync($"{dataHeader.Key}:{dataHeader.Value}");
            }

            await sw.WriteLineAsync();
            await sw.WriteAsync(email.Data.Body);
        }

        return true;
    }
}