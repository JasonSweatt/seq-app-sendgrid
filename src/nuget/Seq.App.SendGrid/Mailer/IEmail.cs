using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Seq.App.SendGrid.Mailer.Interfaces;
using Seq.App.SendGrid.Mailer.Models;

namespace Seq.App.SendGrid.Mailer;

/// <summary>
/// Interface IEmail
/// Implements the <see cref="Seq.App.SendGrid.Mailer.IHideObjectMembers" />
/// </summary>
/// <seealso cref="Seq.App.SendGrid.Mailer.IHideObjectMembers" />
public interface IEmail : IHideObjectMembers
{
    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    EmailData Data { get; set; }

    /// <summary>
    /// Gets or sets the renderer.
    /// </summary>
    /// <value>The renderer.</value>
    ITemplateRenderer Renderer { get; set; }

    /// <summary>
    /// Gets or sets the sender.
    /// </summary>
    /// <value>The sender.</value>
    ISender Sender { get; set; }

    /// <summary>
    /// Adds a recipient to the email, Splits name and address on ';'
    /// </summary>
    /// <param name="emailAddress">Email address of recipient (allows multiple splitting on ';')</param>
    /// <param name="name">Name of recipient</param>
    /// <returns>Instance of the Email class</returns>
    IEmail To(string emailAddress, string? name = null);

    /// <summary>
    /// Set the send from email address
    /// </summary>
    /// <param name="emailAddress">Email address of sender</param>
    /// <param name="name">Name of sender</param>
    /// <returns>Instance of the Email class</returns>
    IEmail SetFrom(string emailAddress, string? name = null);

    /// <summary>
    /// Adds all recipients in list to email
    /// </summary>
    /// <param name="mailAddresses">List of recipients</param>
    /// <returns>Instance of the Email class</returns>
    IEmail To(IEnumerable<Address> mailAddresses);

    /// <summary>
    /// Adds a Carbon Copy to the email
    /// </summary>
    /// <param name="emailAddress">Email address to cc</param>
    /// <param name="name">Name to cc</param>
    /// <returns>Instance of the Email class</returns>
    IEmail CC(string emailAddress, string name = "");

    /// <summary>
    /// Adds all Carbon Copy in list to an email
    /// </summary>
    /// <param name="mailAddresses">List of recipients to CC</param>
    /// <returns>Instance of the Email class</returns>
    IEmail CC(IEnumerable<Address> mailAddresses);

    /// <summary>
    /// Adds a blind carbon copy to the email
    /// </summary>
    /// <param name="emailAddress">Email address of bcc</param>
    /// <param name="name">Name of bcc</param>
    /// <returns>Instance of the Email class</returns>
    IEmail BCC(string emailAddress, string name = "");

    /// <summary>
    /// Adds all blind carbon copy in list to an email
    /// </summary>
    /// <param name="mailAddresses">List of recipients to BCC</param>
    /// <returns>Instance of the Email class</returns>
    IEmail BCC(IEnumerable<Address> mailAddresses);

    /// <summary>
    /// Sets the ReplyTo address on the email
    /// </summary>
    /// <param name="address">The ReplyTo Address</param>
    /// <returns>IEmail.</returns>
    IEmail ReplyTo(string address);

    /// <summary>
    /// Sets the ReplyTo address on the email
    /// </summary>
    /// <param name="address">The ReplyTo Address</param>
    /// <param name="name">The Display Name of the ReplyTo</param>
    /// <returns>IEmail.</returns>
    IEmail ReplyTo(string address, string name);

    /// <summary>
    /// Sets the subject of the email
    /// </summary>
    /// <param name="subject">email subject</param>
    /// <returns>Instance of the Email class</returns>
    IEmail Subject(string subject);

    /// <summary>
    /// Adds a Body to the Email
    /// </summary>
    /// <param name="body">The content of the body</param>
    /// <param name="isHtml">True if Body is HTML, false for plain text (Optional)</param>
    /// <returns>IEmail.</returns>
    IEmail Body(string body, bool isHtml = false);

    /// <summary>
    /// Marks the email as High Priority
    /// </summary>
    /// <returns>IEmail.</returns>
    IEmail HighPriority();

    /// <summary>
    /// Marks the email as Low Priority
    /// </summary>
    /// <returns>IEmail.</returns>
    IEmail LowPriority();

    /// <summary>
    /// Set the template rendering engine to use, defaults to RazorEngine
    /// </summary>
    /// <param name="renderer">The renderer.</param>
    /// <returns>IEmail.</returns>
    IEmail UsingTemplateEngine(ITemplateRenderer renderer);

    /// <summary>
    /// Adds template to email from embedded resource
    /// </summary>
    /// <typeparam name="TModel">The type of the t model.</typeparam>
    /// <param name="path">Path the the embedded resource eg [YourAssembly].[YourResourceFolder].[YourFilename.txt]</param>
    /// <param name="model">Model for the template</param>
    /// <param name="assembly">The assembly your resource is in. Defaults to calling assembly.</param>
    /// <param name="isHtml">if set to <c>true</c> [is HTML].</param>
    /// <returns>IEmail.</returns>
    IEmail UsingTemplateFromEmbedded<TModel>(string path, TModel model, Assembly assembly, bool isHtml = true);

    /// <summary>
    /// Adds the template file to the email
    /// </summary>
    /// <typeparam name="TModel">The type of the t model.</typeparam>
    /// <param name="filename">The path to the file to load</param>
    /// <param name="model">Model for the template</param>
    /// <param name="isHtml">True if Body is HTML, false for plain text (Optional)</param>
    /// <returns>Instance of the Email class</returns>
    IEmail UsingTemplateFromFile<TModel>(string filename, TModel model, bool isHtml = true);

    /// <summary>
    /// Adds a culture specific template file to the email
    /// </summary>
    /// <typeparam name="TModel">The type of the t model.</typeparam>
    /// <param name="filename">The path to the file to load</param>
    /// <param name="model">The razor model</param>
    /// <param name="culture">The culture of the template (Default is the current culture)</param>
    /// <param name="isHtml">True if Body is HTML, false for plain text (Optional)</param>
    /// <returns>Instance of the Email class</returns>
    IEmail UsingCultureTemplateFromFile<TModel>(string filename, TModel model, CultureInfo culture, bool isHtml = true);

    /// <summary>
    /// Adds razor template to the email
    /// </summary>
    /// <typeparam name="TModel">The type of the t model.</typeparam>
    /// <param name="template">The razor template</param>
    /// <param name="model">Model for the template</param>
    /// <param name="isHtml">True if Body is HTML, false for plain text (Optional)</param>
    /// <returns>Instance of the Email class</returns>
    IEmail UsingTemplate<TModel>(string template, TModel model, bool isHtml = true);

    /// <summary>
    /// Adds an Attachment to the Email
    /// </summary>
    /// <param name="attachment">The Attachment to add</param>
    /// <returns>Instance of the Email class</returns>
    IEmail Attach(Attachment attachment);

    /// <summary>
    /// Adds Multiple Attachments to the Email
    /// </summary>
    /// <param name="attachments">The List of Attachments to add</param>
    /// <returns>Instance of the Email class</returns>
    IEmail Attach(IEnumerable<Attachment> attachments);

    /// <summary>
    /// Sends email synchronously
    /// </summary>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Instance of the Email class</returns>
    SendResponse Send(CancellationToken? token = null);

    /// <summary>
    /// Sends email asynchronously
    /// </summary>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task&lt;SendResponse&gt;.</returns>
    Task<SendResponse> SendAsync(CancellationToken? token = null);

    /// <summary>
    /// Attaches from filename.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <param name="contentType">Type of the content.</param>
    /// <param name="attachmentName">Name of the attachment.</param>
    /// <returns>IEmail.</returns>
    IEmail AttachFromFilename(string filename, string? contentType = null, string? attachmentName = null);

    /// <summary>
    /// Adds a Plaintext alternative Body to the Email. Used in conjunction with an HTML email,
    /// this allows for email readers without html capability, and also helps avoid spam filters.
    /// </summary>
    /// <param name="body">The content of the body</param>
    /// <returns>IEmail.</returns>
    IEmail PlaintextAlternativeBody(string body);

    /// <summary>
    /// Adds template to email from embedded resource
    /// </summary>
    /// <typeparam name="TModel">The type of the t model.</typeparam>
    /// <param name="path">Path the the embedded resource eg [YourAssembly].[YourResourceFolder].[YourFilename.txt]</param>
    /// <param name="model">Model for the template</param>
    /// <param name="assembly">The assembly your resource is in. Defaults to calling assembly.</param>
    /// <returns>IEmail.</returns>
    IEmail PlaintextAlternativeUsingTemplateFromEmbedded<TModel>(string path, TModel model, Assembly assembly);

    /// <summary>
    /// Adds the template file to the email
    /// </summary>
    /// <typeparam name="TModel">The type of the t model.</typeparam>
    /// <param name="filename">The path to the file to load</param>
    /// <param name="model">Model for the template</param>
    /// <returns>Instance of the Email class</returns>
    IEmail PlaintextAlternativeUsingTemplateFromFile<TModel>(string filename, TModel model);

    /// <summary>
    /// Adds a culture specific template file to the email
    /// </summary>
    /// <typeparam name="TModel">The type of the t model.</typeparam>
    /// <param name="filename">The path to the file to load</param>
    /// <param name="model">The razor model</param>
    /// <param name="culture">The culture of the template (Default is the current culture)</param>
    /// <returns>Instance of the Email class</returns>
    IEmail PlaintextAlternativeUsingCultureTemplateFromFile<TModel>(string filename, TModel model, CultureInfo culture);

    /// <summary>
    /// Adds razor template to the email
    /// </summary>
    /// <typeparam name="TModel">The type of the t model.</typeparam>
    /// <param name="template">The razor template</param>
    /// <param name="model">Model for the template</param>
    /// <returns>Instance of the Email class</returns>
    IEmail PlaintextAlternativeUsingTemplate<TModel>(string template, TModel model);

    /// <summary>
    /// Adds tag to the Email. This is currently only supported by the Mailgun provider. <see href="https://documentation.mailgun.com/en/latest/user_manual.html#tagging" />
    /// </summary>
    /// <param name="tag">Tag name, max 128 characters, ASCII only</param>
    /// <returns>Instance of the Email class</returns>
    IEmail Tag(string tag);

    /// <summary>
    /// Adds header to the Email.
    /// </summary>
    /// <param name="header">Header name, only printable ASCII allowed.</param>
    /// <param name="body">value of the header</param>
    /// <returns>Instance of the Email class</returns>
    IEmail Header(string header, string body);
}