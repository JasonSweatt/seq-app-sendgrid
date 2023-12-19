using System.IO;

namespace Seq.App.SendGrid.Mailer.Models;

/// <summary>
/// Class Attachment.
/// </summary>
public class Attachment
{
    /// <summary>
    /// Gets or sets whether the attachment is intended to be used for inline images (changes the parameter name for providers such as MailGun)
    /// </summary>
    /// <value><c>true</c> if this instance is inline; otherwise, <c>false</c>.</value>
    public bool IsInline { get; set; }

    /// <summary>
    /// Gets or sets the filename.
    /// </summary>
    /// <value>The filename.</value>
    public string? Filename { get; set; }

    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    public Stream Data { get; set; }

    /// <summary>
    /// Gets or sets the type of the content.
    /// </summary>
    /// <value>The type of the content.</value>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the content identifier.
    /// </summary>
    /// <value>The content identifier.</value>
    public string? ContentId { get; set; }
}