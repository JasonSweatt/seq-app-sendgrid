using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Seq.App.SendGrid.Mailer.Defaults;
using Seq.App.SendGrid.Mailer.Interfaces;
using Seq.App.SendGrid.Mailer.Models;

namespace Seq.App.SendGrid.Mailer;

/// <summary>
/// Class Email.
/// Implements the <see cref="Seq.App.SendGrid.Mailer.IEmail" />
/// </summary>
/// <seealso cref="Seq.App.SendGrid.Mailer.IEmail" />
public class Email : IEmail
{
    /// <summary>
    /// The default renderer
    /// </summary>
    public static ITemplateRenderer DefaultRenderer = new ReplaceRenderer();

    /// <summary>
    /// The default sender
    /// </summary>
    public static ISender DefaultSender = new SaveToDiskSender("/");

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> class.
    /// </summary>
    public Email()
        : this(DefaultRenderer, DefaultSender)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> class.
    /// </summary>
    /// <param name="renderer">The renderer.</param>
    /// <param name="sender">The sender.</param>
    public Email(ITemplateRenderer renderer, ISender sender)
        : this(renderer, sender, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> class.
    /// </summary>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="name">The name.</param>
    public Email(string emailAddress, string name = "")
        : this(DefaultRenderer, DefaultSender, emailAddress, name)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> class.
    /// </summary>
    /// <param name="renderer">The template rendering engine</param>
    /// <param name="sender">The email sending implementation</param>
    /// <param name="emailAddress">Email address to send from</param>
    /// <param name="name">Name to send from</param>
    public Email(ITemplateRenderer renderer, ISender sender, string emailAddress, string name = "")
    {
        Data = new EmailData { FromAddress = new Address { EmailAddress = emailAddress, Name = name } };
        Renderer = renderer;
        Sender = sender;
    }

    /// <inheritdoc />
    public EmailData Data { get; set; }

    /// <inheritdoc />
    public ITemplateRenderer Renderer { get; set; }

    /// <inheritdoc />
    public ISender Sender { get; set; }

    /// <summary>
    /// Set the send from email address
    /// </summary>
    /// <param name="emailAddress">Email address of sender</param>
    /// <param name="name">Name of sender</param>
    /// <returns>Instance of the Email class</returns>
    public IEmail SetFrom(string emailAddress, string? name = null)
    {
        Data.FromAddress = new Address(emailAddress, name ?? string.Empty);
        return this;
    }

    /// <inheritdoc />
    public IEmail To(string emailAddress, string? name = null)
    {
        //if (name == null)
        //{
        //    To(emailAddress);
        //}

        if (emailAddress.Contains(";"))
        {
            // email address has semi-colon, try split
            var nameSplit = name?.Split(';') ?? Array.Empty<string>();
            var addressSplit = emailAddress.Split(';');
            for (var i = 0; i < addressSplit.Length; i++)
            {
                var currentName = string.Empty;
                if (nameSplit.Length - 1 >= i)
                {
                    currentName = nameSplit[i];
                }

                Data.ToAddresses.Add(new Address(addressSplit[i].Trim(), currentName.Trim()));
            }
        }
        else
        {
            Data.ToAddresses.Add(new Address(emailAddress.Trim(), name?.Trim()));
        }

        return this;
    }

    ///// <summary>
    ///// To the specified email address.
    ///// </summary>
    ///// <param name="emailAddress">The email address.</param>
    ///// <returns>IEmail.</returns>
    //private IEmail To(string emailAddress)
    //{
    //    if (emailAddress.Contains(";"))
    //    {
    //        foreach (var address in emailAddress.Split(';'))
    //        {
    //            Data.ToAddresses.Add(new Address(address));
    //        }
    //    }
    //    else
    //    {
    //        Data.ToAddresses.Add(new Address(emailAddress));
    //    }

    //    return this;
    //}

    /// <inheritdoc />
    public IEmail To(IEnumerable<Address> mailAddresses)
    {
        foreach (var address in mailAddresses)
        {
            Data.ToAddresses.Add(address);
        }

        return this;
    }

    /// <inheritdoc />
    public IEmail CC(string emailAddress, string name = "")
    {
        Data.CcAddresses.Add(new Address(emailAddress, name));
        return this;
    }

    /// <inheritdoc />
    public IEmail CC(IEnumerable<Address> mailAddresses)
    {
        foreach (var address in mailAddresses)
        {
            Data.CcAddresses.Add(address);
        }

        return this;
    }

    /// <inheritdoc />
    public IEmail BCC(string emailAddress, string name = "")
    {
        Data.BccAddresses.Add(new Address(emailAddress, name));
        return this;
    }

    /// <inheritdoc />
    public IEmail BCC(IEnumerable<Address> mailAddresses)
    {
        foreach (var address in mailAddresses)
        {
            Data.BccAddresses.Add(address);
        }

        return this;
    }

    /// <inheritdoc />
    public IEmail ReplyTo(string address)
    {
        Data.ReplyToAddresses.Add(new Address(address));

        return this;
    }

    /// <inheritdoc />
    public IEmail ReplyTo(string address, string name)
    {
        Data.ReplyToAddresses.Add(new Address(address, name));

        return this;
    }

    /// <inheritdoc />
    public IEmail Subject(string subject)
    {
        Data.Subject = subject;
        return this;
    }

    /// <inheritdoc />
    public IEmail Body(string body, bool isHtml = false)
    {
        Data.IsHtml = isHtml;
        Data.Body = body;
        return this;
    }

    /// <inheritdoc />
    public IEmail PlaintextAlternativeBody(string body)
    {
        Data.PlaintextAlternativeBody = body;
        return this;
    }

    /// <inheritdoc />
    public IEmail HighPriority()
    {
        Data.Priority = Priority.High;
        return this;
    }

    /// <inheritdoc />
    public IEmail LowPriority()
    {
        Data.Priority = Priority.Low;
        return this;
    }

    /// <inheritdoc />
    public IEmail UsingTemplateEngine(ITemplateRenderer renderer)
    {
        Renderer = renderer;
        return this;
    }

    /// <inheritdoc />
    public IEmail UsingTemplateFromEmbedded<TModel>(string path, TModel model, Assembly assembly, bool isHtml = true)
    {
        var template = EmbeddedResourceHelper.GetResourceAsString(assembly, path);
        var result = Renderer.Parse(template, model, isHtml);
        Data.IsHtml = isHtml;
        Data.Body = result;

        return this;
    }

    /// <inheritdoc />
    public IEmail PlaintextAlternativeUsingTemplateFromEmbedded<TModel>(string path, TModel model, Assembly assembly)
    {
        var template = EmbeddedResourceHelper.GetResourceAsString(assembly, path);
        var result = Renderer.Parse(template, model, false);
        Data.PlaintextAlternativeBody = result;

        return this;
    }

    /// <inheritdoc />
    public IEmail UsingTemplateFromFile<TModel>(string filename, TModel model, bool isHtml = true)
    {
        var template = string.Empty;

        using (var reader = new StreamReader(File.OpenRead(filename)))
        {
            template = reader.ReadToEnd();
        }

        var result = Renderer.Parse(template, model, isHtml);
        Data.IsHtml = isHtml;
        Data.Body = result;

        return this;
    }

    /// <inheritdoc />
    public IEmail PlaintextAlternativeUsingTemplateFromFile<TModel>(string filename, TModel model)
    {
        var template = string.Empty;

        using (var reader = new StreamReader(File.OpenRead(filename)))
        {
            template = reader.ReadToEnd();
        }

        var result = Renderer.Parse(template, model, false);
        Data.PlaintextAlternativeBody = result;

        return this;
    }

    /// <inheritdoc />
    public IEmail UsingCultureTemplateFromFile<TModel>(string filename, TModel model, CultureInfo culture, bool isHtml = true)
    {
        var cultureFile = GetCultureFileName(filename, culture);
        return UsingTemplateFromFile(cultureFile, model, isHtml);
    }

    /// <inheritdoc />
    public IEmail PlaintextAlternativeUsingCultureTemplateFromFile<TModel>(string filename, TModel model, CultureInfo culture)
    {
        var cultureFile = GetCultureFileName(filename, culture);
        return PlaintextAlternativeUsingTemplateFromFile(cultureFile, model);
    }

    /// <inheritdoc />
    public IEmail UsingTemplate<TModel>(string template, TModel model, bool isHtml = true)
    {
        var result = Renderer.Parse(template, model, isHtml);
        Data.IsHtml = isHtml;
        Data.Body = result;

        return this;
    }

    /// <inheritdoc />
    public IEmail PlaintextAlternativeUsingTemplate<TModel>(string template, TModel model)
    {
        var result = Renderer.Parse(template, model, false);
        Data.PlaintextAlternativeBody = result;

        return this;
    }

    /// <inheritdoc />
    public IEmail Attach(Attachment attachment)
    {
        if (!Data.Attachments.Contains(attachment))
        {
            Data.Attachments.Add(attachment);
        }

        return this;
    }

    /// <inheritdoc />
    public IEmail Attach(IEnumerable<Attachment> attachments)
    {
        foreach (var attachment in attachments.Where(attachment => !Data.Attachments.Contains(attachment)))
        {
            Data.Attachments.Add(attachment);
        }

        return this;
    }

    /// <inheritdoc />
    public IEmail AttachFromFilename(string filename, string? contentType = null, string? attachmentName = null)
    {
        var stream = File.OpenRead(filename);
        Attach(new Attachment { Data = stream, Filename = attachmentName ?? filename, ContentType = contentType, });

        return this;
    }

    /// <inheritdoc />
    public IEmail Tag(string tag)
    {
        Data.Tags.Add(tag);

        return this;
    }

    /// <inheritdoc />
    public IEmail Header(string header, string body)
    {
        Data.Headers.Add(header, body);

        return this;
    }

    /// <inheritdoc />
    public virtual SendResponse Send(CancellationToken? token = null)
    {
        return Sender.Send(this, token);
    }

    /// <inheritdoc />
    public virtual Task<SendResponse> SendAsync(CancellationToken? token = null)
    {
        return Sender.SendAsync(this, token);
    }

    /// <summary>
    /// Creates a new Email instance and sets the from property
    /// </summary>
    /// <param name="emailAddress">Email address to send from</param>
    /// <param name="name">Name to send from</param>
    /// <returns>Instance of the Email class</returns>
    public static IEmail From(string emailAddress, string? name = null)
    {
        var email = new Email { Data = { FromAddress = new Address(emailAddress, name ?? string.Empty) } };

        return email;
    }

    /// <summary>
    /// Gets the name of the culture file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>System.String.</returns>
    private static string GetCultureFileName(string fileName, CultureInfo culture)
    {
        var extension = Path.GetExtension(fileName);
        var cultureExtension = $"{culture.Name}{extension}";

        var cultureFile = Path.ChangeExtension(fileName, cultureExtension);
        if (File.Exists(cultureFile))
        {
            return cultureFile;
        }

        return fileName;
    }
}