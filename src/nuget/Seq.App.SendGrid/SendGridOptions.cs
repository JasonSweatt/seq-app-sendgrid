namespace Seq.App.SendGrid;

/// <summary>
/// Class SendGridOptions.
/// </summary>
public class SendGridOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SendGridOptions"/> class.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <param name="sandBoxMode">if set to <c>true</c> [sand box mode].</param>
    public SendGridOptions(string? apiKey, bool sandBoxMode = false)
    {
        ApiKey = apiKey;
        SandBoxMode = sandBoxMode;
    }

    /// <summary>
    /// Gets the API key.
    /// </summary>
    /// <value>The API key.</value>
    public string? ApiKey { get; }

    /// <summary>
    /// Gets a value indicating whether [sand box mode].
    /// </summary>
    /// <value><c>true</c> if [sand box mode]; otherwise, <c>false</c>.</value>
    public bool SandBoxMode { get; }
}