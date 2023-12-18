using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Seq.App.SendGrid.Helpers;
using Seq.App.SendGrid.Mailer;
using Seq.Apps;
using Seq.Apps.LogEvents;
using Template = HandlebarsDotNet.HandlebarsTemplate<object, object>;

// ReSharper disable UnusedAutoPropertyAccessor.Global, MemberCanBePrivate.Global

namespace Seq.App.SendGrid;

/// <summary>
/// Class EmailApp.
/// Implements the <see cref="SeqApp" />
/// Implements the <see cref="LogEventData" />
/// </summary>
/// <seealso cref="SeqApp" />
/// <seealso cref="LogEventData" />
[SeqApp("SendGrid HTML Email", Description = "Uses Handlebars templates to format events and notifications into SendGrid HTML email.")]
public class EmailApp : SeqApp, ISubscribeToAsync<LogEventData>
{
    /// <summary>
    /// The default subject template
    /// </summary>
    private const string DefaultSubjectTemplate = @"[{{$Level}}] {{{$Message}}} (via Seq)";

    /// <summary>
    /// The maximum subject length
    /// </summary>
    private const int MaxSubjectLength = 130;

    /// <summary>
    /// The clock
    /// </summary>
    private readonly IClock _clock;

    /// <summary>
    /// The suppressions
    /// </summary>
    private readonly Dictionary<uint, DateTime> _suppressions = new();

    /// <summary>
    /// The body template
    /// </summary>
    private Template? _bodyTemplate, _subjectTemplate, _toAddressesTemplate;

    /// <summary>
    /// The options
    /// </summary>
    private SendGridOptions? _options;

    /// <summary>
    /// Initializes static members of the <see cref="EmailApp"/> class.
    /// </summary>
    static EmailApp()
    {
        HandlebarsHelpers.Register();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailApp"/> class.
    /// </summary>
    /// <param name="clock">The clock.</param>
    /// <exception cref="System.ArgumentNullException">mailGateway</exception>
    /// <exception cref="System.ArgumentNullException">clock</exception>
    internal EmailApp(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailApp"/> class.
    /// </summary>
    public EmailApp()
        : this(new SystemClock())
    {
    }

    /// <summary>
    /// Gets or sets the message count.
    /// </summary>
    /// <value>The message count.</value>
    public int MessageCount { get; set; }

    /// <summary>
    /// Gets or sets the API key.
    /// </summary>
    /// <value>The API key.</value>
    [SeqAppSetting(DisplayName = "API Key", HelpText = "The API Key generated within SendGrid for an application, see here: (https://docs.sendgrid.com/ui/account-and-settings/api-keys).")]
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the template identifier.
    /// </summary>
    /// <value>The template identifier.</value>
    [SeqAppSetting(IsOptional = true, DisplayName = "Template Id", HelpText = "The template id for the email. see here: (https://docs.sendgrid.com/ui/sending-email/how-to-send-an-email-with-dynamic-templates).")]
    public string? TemplateId { get; set; }

    /// <summary>
    /// Gets or sets from.
    /// </summary>
    /// <value>From.</value>
    [SeqAppSetting(DisplayName = "From address", HelpText = "The account from which the email is being sent.")]
    public string? From { get; set; }

    /// <summary>
    /// Gets or sets from name.
    /// </summary>
    /// <value>From name.</value>
    [SeqAppSetting(IsOptional = true, DisplayName = "From Name", HelpText = "The account name from which the email is being sent.")]
    public string? FromName { get; set; }

    /// <summary>
    /// Gets or sets to.
    /// </summary>
    /// <value>To.</value>
    [SeqAppSetting(DisplayName = "To address(es)", HelpText = "The account(s) to which the email(s) will be sent. Multiple addresses are separated by a semi-colon. Handlebars syntax is supported.")]
    public string? To { get; set; }

    /// <summary>
    /// Converts to name.
    /// </summary>
    /// <value>To name.</value>
    [SeqAppSetting(IsOptional = true, DisplayName = "To Name(s)", HelpText = "The account name(s) that will be addressed. Keep same order as To. Multiple names are separated by a semi-colon. Handlebars syntax is supported.")]
    public string? ToName { get; set; }

    /// <summary>
    /// Gets or sets Cc.
    /// </summary>
    /// <value>To.</value>
    [SeqAppSetting(IsOptional = true, DisplayName = "cc address(es)", HelpText = "The account(s) to which the email(s) will be sent. Multiple addresses are separated by a semi-colon. Handlebars syntax is supported.")]
    public string? Cc { get; set; }

    /// <summary>
    /// Converts to cc name.
    /// </summary>
    /// <value>To name.</value>
    [SeqAppSetting(IsOptional = true, DisplayName = "Cc Name(s)", HelpText = "The account name(s) that will be addressed. Keep same order as To. Multiple names are separated by a semi-colon. Handlebars syntax is supported.")]
    public string? CcName { get; set; }

    /// <summary>
    /// Gets or sets the subject template.
    /// </summary>
    /// <value>The subject template.</value>
    [SeqAppSetting(IsOptional = true, DisplayName = "Subject template", HelpText = "The subject of the email, using Handlebars syntax. If blank, a default subject will be generated.")]
    public string? SubjectTemplate { get; set; }

    /// <summary>
    /// Gets or sets the body template.
    /// </summary>
    /// <value>The body template.</value>
    [SeqAppSetting(
        IsOptional = true,
        InputType = SettingInputType.LongText,
        DisplayName = "Body template",
        HelpText = "The template to use when generating the email body, using Handlebars syntax. Leave this blank to use " +
            "the default template that includes the message and " +
            "properties (https://github.com/JasonSweatt/seq-app-sendgrid/blob/main/src/nuget/Seq.App.SendGrid/Resources/DefaultBodyTemplate.html).")]
    public string? BodyTemplate { get; set; }

    /// <summary>
    /// Gets or sets the suppression minutes.
    /// </summary>
    /// <value>The suppression minutes.</value>
    [SeqAppSetting(IsOptional = true, DisplayName = "Suppression time (minutes)", HelpText = "Once an event type has been sent, the time to wait before sending again. The default is zero.")]
    public int SuppressionMinutes { get; set; }

    /// <summary>
    /// Gets or sets the name of the time zone.
    /// </summary>
    /// <value>The name of the time zone.</value>
    [SeqAppSetting(IsOptional = true, DisplayName = "Time zone name", HelpText = "The IANA name of the time zone to use when formatting dates and times. The default is `Etc/UTC`. On Windows versions before Server 2019, and Seq versions before 2023.1, only Windows time zone names are accepted.")]
    public string? TimeZoneName { get; set; }

    /// <summary>
    /// Gets or sets the date time format.
    /// </summary>
    /// <value>The date time format.</value>
    [SeqAppSetting(IsOptional = true, DisplayName = "Date/time format", HelpText = "A format string controlling how dates and times are formatted. Supports .NET date/time formatting syntax. The default is `MMMM DD, YYYY h:mm:ss A` with TemplateId and 'o' for Handlerbars, producing ISO-8601.")]
    public string? DateTimeFormat { get; set; }

    /// <summary>
    /// On as an asynchronous operation.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task OnAsync(Event<LogEventData> @event)
    {
        if (ShouldSuppress(@event))
        {
            return;
        }

        var to = FormatTemplate(_toAddressesTemplate!, @event, base.Host)
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        if (to.Length == 0)
        {
            Log.Warning("Email 'to' address template did not evaluate to one or more recipient addresses");
            return;
        }

        var body = FormatTemplate(_bodyTemplate!, @event, base.Host);
        var subject = FormatTemplate(_subjectTemplate!, @event, base.Host).Trim().Replace("\r", string.Empty).Replace("\n", string.Empty);
        if (subject.Length > MaxSubjectLength)
        {
            subject = subject.Substring(0, MaxSubjectLength);
        }

        if (string.IsNullOrWhiteSpace(_options?.ApiKey))
        {
            throw new ArgumentException("SendGrid Api Key needs to be supplied");
        }

        var sender = new SendGridSender(_options.ApiKey, _options.SandBoxMode);
        Email.DefaultSender = sender;

        // all required components
        var email = Email
            .From(From, FromName)
            .To(To, ToName)
            .Subject(subject)
            .Body(body, true);

        // TODO: maybe handle a different way for Cc
        if (!string.IsNullOrEmpty(Cc))
        {
            if (Cc.Contains(";"))
            {
                // email address has semi-colon, try split
                var nameSplit = CcName?.Split(';') ?? Array.Empty<string>();
                var addressSplit = Cc.Split(';');
                for (var i = 0; i < addressSplit.Length; i++)
                {
                    var currentName = string.Empty;
                    if (nameSplit.Length - 1 >= i)
                    {
                        currentName = nameSplit[i];
                    }

                    email.CC(addressSplit[i].Trim(), currentName.Trim());
                }
            }
            else
            {
                email.CC(Cc, CcName);
            }
        }

        var response = string.IsNullOrEmpty(TemplateId)
        ? await email.SendAsync()
        : await email.SendWithTemplateAsync(TemplateId, BuildTemplateData(@event, base.Host, string.IsNullOrEmpty(DateTimeFormat) ? "MMMM DD, YYYY h:mm:ss A" : DateTimeFormat!.Trim(), string.IsNullOrEmpty(TimeZoneName) ? PortableTimeZoneInfo.UtcTimeZoneName : TimeZoneName!.Trim()));

        if (response.Successful)
        {
            MessageCount += 1;
        }
    }

    /// <summary>
    /// Called after all configuration has completed, but before any
    /// events are sent to the app and before ingestion begins. The
    /// app should use this event to validate its configuration.
    /// </summary>
    protected override void OnAttached()
    {
        _options = _options = new SendGridOptions(ApiKey, false);

        _subjectTemplate = Handlebars.Compile(string.IsNullOrEmpty(SubjectTemplate) ? DefaultSubjectTemplate : SubjectTemplate);
        _bodyTemplate = Handlebars.Compile(string.IsNullOrEmpty(BodyTemplate) ? Resources.DefaultBodyTemplate : BodyTemplate);

        _toAddressesTemplate = string.IsNullOrEmpty(To) ? (_, _) => To : Handlebars.Compile(To);
    }

    /// <summary>
    /// Should suppress.
    /// </summary>
    /// <param name="event">The evt.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    private bool ShouldSuppress(Event<LogEventData> @event)
    {
        if (SuppressionMinutes == 0)
        {
            return false;
        }

        var now = _clock.UtcNow;
        if (!_suppressions.TryGetValue(@event.EventType, out var suppressedSince) ||
            suppressedSince.AddMinutes(SuppressionMinutes) < now)
        {
            // Not suppressed, or suppression expired

            // Clean up old entries
            var expired = _suppressions.FirstOrDefault(kvp => kvp.Value.AddMinutes(SuppressionMinutes) < now);
            while (expired.Value != default)
            {
                _suppressions.Remove(expired.Key);
                expired = _suppressions.FirstOrDefault(kvp => kvp.Value.AddMinutes(SuppressionMinutes) < now);
            }

            // Start suppression again
            suppressedSince = now;
            _suppressions[@event.EventType] = suppressedSince;
            return false;
        }

        // Suppressed
        return true;
    }

    /// <summary>
    /// Builds the template data.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="host">The host.</param>
    /// <param name="dateTimeFormat">The date time format.</param>
    /// <param name="timeZoneName">Name of the time zone.</param>
    /// <returns>System.Object.</returns>
    /// <exception cref="System.ArgumentNullException">event</exception>
    internal static object BuildTemplateData(Event<LogEventData> @event, Host host, string dateTimeFormat, string timeZoneName)
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        var properties = (IDictionary<string, object?>)ToDynamic(@event.Data.Properties ?? new Dictionary<string, object?>());

        var payload = (IDictionary<string, object?>)ToDynamic(
            new Dictionary<string, object?>
            {
                { "Id", @event.Id },
                { "UtcTimestamp", @event.TimestampUtc },
                { "LocalTimestamp", @event.Data.LocalTimestamp },
                { "Level", @event.Data.Level.ToString() },
                { "MessageTemplate", @event.Data.MessageTemplate },
                { "Message", @event.Data.RenderedMessage },
                { "Exception", @event.Data.Exception },
                { "Properties", properties },
                { "EventType", "$" + @event.EventType.ToString("X8") },
                { "Instance", host.InstanceName },
                { "ServerUri", host.BaseUri },

                // Note, this will only be valid when events are streamed directly to the app, and not when the app is sending an alert notification.
                { "EventUri", string.Concat(host.BaseUri, "#/events?filter=@Id%20%3D%20'", @event.Id, "'&amp;show=expanded") },
                { "DateTimeFormat", dateTimeFormat },
                { "TimeZoneName", timeZoneName },
            });

        foreach (var property in properties)
        {
            payload[property.Key] = property.Value;
        }

        return payload;
    }

    /// <summary>
    /// Formats the template.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="event">The event.</param>
    /// <param name="host">The host.</param>
    /// <param name="dateTimeFormat">The date time format.</param>
    /// <param name="timeZoneName">Name of the time zone.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.ArgumentNullException">template</exception>
    /// <exception cref="System.ArgumentNullException">evt</exception>
    internal static string FormatTemplate(Template template, Event<LogEventData> @event, Host host, string dateTimeFormat, string timeZoneName)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        var properties = (IDictionary<string, object?>)ToDynamic(@event.Data.Properties ?? new Dictionary<string, object?>());

        var payload = (IDictionary<string, object?>)ToDynamic(
            new Dictionary<string, object?>
            {
                { "$Id", @event.Id },
                { "$UtcTimestamp", @event.TimestampUtc },
                { "$LocalTimestamp", @event.Data.LocalTimestamp },
                { "$Level", @event.Data.Level },
                { "$MessageTemplate", @event.Data.MessageTemplate },
                { "$Message", @event.Data.RenderedMessage },
                { "$Exception", @event.Data.Exception },
                { "$Properties", properties },
                { "$EventType", "$" + @event.EventType.ToString("X8") },
                { "$Instance", host.InstanceName },
                { "$ServerUri", host.BaseUri },

                // Note, this will only be valid when events are streamed directly to the app, and not when the app is sending an alert notification.
                { "$EventUri", string.Concat(host.BaseUri, "#/events?filter=@Id%20%3D%20'", @event.Id, "'&amp;show=expanded") },
                { "$DateTimeFormat", dateTimeFormat },
                { "$TimeZoneName", timeZoneName },
            });

        foreach (var property in properties)
        {
            payload[property.Key] = property.Value;
        }

        return template(payload);
    }

    /// <summary>
    /// Formats the template.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="event">The event.</param>
    /// <param name="host">The host.</param>
    /// <returns>System.String.</returns>
    private string FormatTemplate(Template template, Event<LogEventData> @event, Host host)
    {
        return FormatTemplate(template, @event, host, string.IsNullOrEmpty(DateTimeFormat) ? "o" : DateTimeFormat!.Trim(), string.IsNullOrEmpty(TimeZoneName) ? PortableTimeZoneInfo.UtcTimeZoneName : TimeZoneName!.Trim());
    }

    /// <summary>
    /// Tests the format template.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="event">The event.</param>
    /// <param name="host">The host.</param>
    /// <returns>System.String.</returns>
    internal static string TestFormatTemplate(Template template, Event<LogEventData> @event, Host host)
    {
        return FormatTemplate(template, @event, host, "o", "Australia/Brisbane");
    }

    /// <summary>
    /// Converts to dynamic.
    /// </summary>
    /// <param name="object">The object.</param>
    /// <returns>System.Object.</returns>
    private static object ToDynamic(object @object)
    {
        if (@object is IEnumerable<KeyValuePair<string, object>> dictionary)
        {
            var result = new ExpandoObject();
            var results = (IDictionary<string, object?>)result;
            foreach (var kvp in dictionary)
            {
                results.Add(kvp.Key, ToDynamic(kvp.Value));
            }

            return result;
        }

        if (@object is IEnumerable<object> enumerable)
        {
            return enumerable.Select(ToDynamic).ToArray();
        }

        return @object;
    }
}