using System.Collections.Generic;

namespace Seq.App.SendGrid.Mailer.Models;

/// <summary>
/// Class EmailData.
/// </summary>
public class EmailData
{
    /// <summary>
    /// Converts to addresses.
    /// </summary>
    /// <value>To addresses.</value>
    public IList<Address> ToAddresses { get; set; } = new List<Address>();

    /// <summary>
    /// Gets or sets the cc addresses.
    /// </summary>
    /// <value>The cc addresses.</value>
    public IList<Address> CcAddresses { get; set; } = new List<Address>();

    /// <summary>
    /// Gets or sets the BCC addresses.
    /// </summary>
    /// <value>The BCC addresses.</value>
    public IList<Address> BccAddresses { get; set; } = new List<Address>();

    /// <summary>
    /// Gets or sets the reply to addresses.
    /// </summary>
    /// <value>The reply to addresses.</value>
    public IList<Address> ReplyToAddresses { get; set; } = new List<Address>();

    /// <summary>
    /// Gets or sets the attachments.
    /// </summary>
    /// <value>The attachments.</value>
    public IList<Attachment> Attachments { get; set; } = new List<Attachment>();

    /// <summary>
    /// Gets or sets from address.
    /// </summary>
    /// <value>From address.</value>
    public Address FromAddress { get; set; }

    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    /// <value>The subject.</value>
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets the body.
    /// </summary>
    /// <value>The body.</value>
    public string Body { get; set; }

    /// <summary>
    /// Gets or sets the plaintext alternative body.
    /// </summary>
    /// <value>The plaintext alternative body.</value>
    public string PlaintextAlternativeBody { get; set; }

    /// <summary>
    /// Gets or sets the priority.
    /// </summary>
    /// <value>The priority.</value>
    public Priority Priority { get; set; }

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    /// <value>The tags.</value>
    public IList<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets a value indicating whether this instance is HTML.
    /// </summary>
    /// <value><c>true</c> if this instance is HTML; otherwise, <c>false</c>.</value>
    public bool IsHtml { get; set; }

    /// <summary>
    /// Gets or sets the headers.
    /// </summary>
    /// <value>The headers.</value>
    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
}