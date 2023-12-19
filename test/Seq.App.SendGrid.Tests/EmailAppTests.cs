using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Seq.App.SendGrid.Tests.Support;
using Seq.Apps;
using Seq.Apps.LogEvents;
using Xunit;

namespace Seq.App.SendGrid.Tests;

/// <summary>
/// Class EmailAppTests.
/// </summary>
public class EmailAppTests
{
    /// <summary>
    /// TODO must have a valid SendGrid account to generate an api key
    /// The API key <see href="https://app.sendgrid.com/settings/api_keys"/> 
    /// </summary>
    private const string ApiKey = "";

    /// <summary>
    /// TODO must have a valid SendGrid From account
    /// From email
    /// </summary>
    private const string FromEmail = "donot.reply@email.com";

    /// <summary>
    /// The name email
    /// </summary>
    private const string NameEmail = "benomip765";

    /// <summary>
    /// The domain email
    /// </summary>
    private const string DomainEmail = "beeplush.com";

    /// <summary>
    /// TODO must have a valid to email use https://temp-mail.org/en/
    /// To email
    /// </summary>
    private const string ToEmail = $"{NameEmail}@{DomainEmail}";

    /// <summary>
    /// Initializes static members of the <see cref="EmailAppTests"/> class.
    /// </summary>
    static EmailAppTests()
    {
        // Ensure the handlebars helpers are registered, since we test them before we're
        // sure of having created an instance of the app.
        GC.KeepAlive(new EmailApp());
    }

    /// <summary>
    /// Defines the test method BuiltInPropertiesAreRenderedInTemplates.
    /// </summary>
    [Fact]
    public void BuiltInPropertiesAreRenderedInTemplates()
    {
        var template = Handlebars.Compile("{{$Level}}");
        var data = Error.LogEvent();
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal(data.Data.Level.ToString(), result);
    }

    /// <summary>
    /// Defines the test method PayloadPropertiesAreRenderedInTemplates.
    /// </summary>
    [Fact]
    public void PayloadPropertiesAreRenderedInTemplates()
    {
        var template = Handlebars.Compile("See {{What}}");
        var data = Error.LogEvent(includedProperties: new Dictionary<string, object> { { "What", 10 } });
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal("See 10", result);
    }

    /// <summary>
    /// Defines the test method NoPropertiesAreRequiredOnASourceEvent.
    /// </summary>
    [Fact]
    public void NoPropertiesAreRequiredOnASourceEvent()
    {
        var template = Handlebars.Compile("No properties");
        var id = Error.EventId();
        var timestamp = Error.UtcTimestamp();
        var data = new Event<LogEventData>(
            id, Error.EventType(), timestamp, new LogEventData
            {
                Exception = null,
                Id = id,
                Level = LogEventLevel.Fatal,
                LocalTimestamp = new DateTimeOffset(timestamp),
                MessageTemplate = "Error text",
                RenderedMessage = "Error text",
                Properties = null
            });
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal("No properties", result);
    }

    /// <summary>
    /// Defines the test method IfEqHelperDetectsEquality.
    /// </summary>
    [Fact]
    public void IfEqHelperDetectsEquality()
    {
        var template = Handlebars.Compile("{{#if_eq $Level \"Fatal\"}}True{{/if_eq}}");
        var data = Error.LogEvent();
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal("True", result);
    }

    /// <summary>
    /// Defines the test method IfEqHelperDetectsInequality.
    /// </summary>
    [Fact]
    public void IfEqHelperDetectsInequality()
    {
        var template = Handlebars.Compile("{{#if_eq $Level \"Warning\"}}True{{/if_eq}}");
        var data = Error.LogEvent();
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal("", result);
    }

    /// <summary>
    /// Defines the test method TrimStringHelper0Args.
    /// </summary>
    [Fact]
    public void TrimStringHelper0Args()
    {
        var template = Handlebars.Compile("{{substring}}");
        var data = Error.LogEvent();
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal("", result);
    }

    /// <summary>
    /// Defines the test method TrimStringHelper1Arg.
    /// </summary>
    [Fact]
    public void TrimStringHelper1Arg()
    {
        var template = Handlebars.Compile("{{substring $Level}}");
        var data = Error.LogEvent();
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal("Fatal", result);
    }

    /// <summary>
    /// Defines the test method TrimStringHelper2Args.
    /// </summary>
    [Fact]
    public void TrimStringHelper2Args()
    {
        var template = Handlebars.Compile("{{substring $Level 2}}");
        var data = Error.LogEvent();
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal("tal", result);
    }

    /// <summary>
    /// Defines the test method TrimStringHelper3Args.
    /// </summary>
    [Fact]
    public void TrimStringHelper3Args()
    {
        var template = Handlebars.Compile("{{substring $Level 2 1}}");
        var data = Error.LogEvent();
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal("t", result);
    }

    /// <summary>
    /// Defines the test method ToAddressesAreTemplated.
    /// </summary>
    [Fact]
    public async Task ToAddressesAreTemplated()
    {
        var app = new EmailApp(new SystemClock())
        {
            From = FromEmail,
            To = $"{{Name}}@{DomainEmail}",
            ApiKey = ApiKey,
        };


        app.Attach(new TestAppHost());

        var data = Error.LogEvent(includedProperties: new Dictionary<string, object> { { "Name", NameEmail } });
        await app.OnAsync(data);

        Assert.Equal(ToEmail, app.To);
    }

    /// <summary>
    /// Defines the test method EventsAreSuppressedWithinWindow.
    /// </summary>
    [Fact]
    public async Task EventsAreSuppressedWithinWindow()
    {
        var clock = new TestClock();
        var app = new EmailApp(clock)
        {
            From = FromEmail,
            To = ToEmail,
            SuppressionMinutes = 10,
            ApiKey = ApiKey,
        };

        app.Attach(new TestAppHost());

        await app.OnAsync(Error.LogEvent(eventType: 99));
        clock.Advance(TimeSpan.FromMinutes(1));
        await app.OnAsync(Error.LogEvent(eventType: 99));
        await app.OnAsync(Error.LogEvent(eventType: 99));

        Assert.Equal(3, app.MessageCount);

        app.MessageCount = 0; // reset message count

        clock.Advance(TimeSpan.FromHours(1));

        await app.OnAsync(Error.LogEvent(eventType: 99));

        Assert.Equal(1, app.MessageCount);
    }

    /// <summary>
    /// Defines the test method EventsAreSendGridTemplateId.
    /// </summary>
    [Fact]
    public async Task EventsAreSendGridTemplateId()
    {
        var clock = new TestClock();
        var app = new EmailApp(clock)
        {
            From = FromEmail,
            To = ToEmail,
            ApiKey = ApiKey,
            TemplateId = "1",
        };

        app.Attach(new TestAppHost());

        await app.OnAsync(Error.LogEvent(eventType: 99));
        await app.OnAsync(Error.LogEvent(eventType: 99));
        await app.OnAsync(Error.LogEvent(eventType: 99));

        Assert.Equal(3, app.MessageCount);
    }

    /// <summary>
    /// Defines the test method EventsAreSuppressedByType.
    /// </summary>
    [Fact]
    public async Task EventsAreSuppressedByType()
    {
        var app = new EmailApp(new SystemClock())
        {
            From = FromEmail,
            To = ToEmail,
            SuppressionMinutes = 10,
            ApiKey = ApiKey,
        };

        app.Attach(new TestAppHost());

        await app.OnAsync(Error.LogEvent(eventType: 1));
        await app.OnAsync(Error.LogEvent(eventType: 2));
        await app.OnAsync(Error.LogEvent(eventType: 1));

        Assert.Equal(2, app.MessageCount);
    }

    /// <summary>
    /// Defines the test method ToAddressesCanBeCommaSeparated.
    /// </summary>
    [Fact]
    public async Task ToAddressesCanBeCommaSeparated()
    {
        var app = new EmailApp(new SystemClock())
        {
            From = FromEmail,
            To = "{{To}}",
            ApiKey = ApiKey,
        };

        app.Attach(new TestAppHost());

        var data = Error.LogEvent(includedProperties: new Dictionary<string, object> { { "To", $"jason.sweatt@gmail.com;{ToEmail}" } });
        await app.OnAsync(data);

        Assert.Equal(1, app.MessageCount);
    }

    /// <summary>
    /// Defines the test method DateTimeHelperAppliesFormatting.
    /// </summary>
    [Fact]
    public void DateTimeHelperAppliesFormatting()
    {
        var template = Handlebars.Compile("{{datetime When 'R'}}");
        var data = Error.LogEvent(includedProperties: new Dictionary<string, object> { ["When"] = new DateTime(2023, 3, 1, 17, 30, 11, DateTimeKind.Utc) });
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal("Mon, 01 Mar 2023 17:30:11 GMT", result);
    }

    /// <summary>
    /// Defines the test method DateTimeHelperParsesDateTimeStrings.
    /// </summary>
    [Fact]
    public void DateTimeHelperParsesDateTimeStrings()
    {
        var template = Handlebars.Compile("{{datetime When 'R'}}");
        var data = Error.LogEvent(includedProperties: new Dictionary<string, object> { ["When"] = new DateTime(2023, 3, 1, 17, 30, 11, DateTimeKind.Utc).ToString("o") });
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal("Mon, 01 Mar 2023 17:30:11 GMT", result);
    }

    /// <summary>
    /// Defines the test method DateTimeHelperSwitchesTimeZone.
    /// </summary>
    [Fact]
    public void DateTimeHelperSwitchesTimeZone()
    {
        var australiaBrisbane = IanaTimeZonesSupported() ? "Australia/Brisbane" : "E. Australia Standard Time";

        var template = Handlebars.Compile("{{datetime When 'o' '" + australiaBrisbane + "'}}");
        var data = Error.LogEvent(includedProperties: new Dictionary<string, object> { ["When"] = new DateTime(2023, 3, 1, 17, 30, 11, DateTimeKind.Utc) });
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal("2023-03-02T03:30:11.0000000+10:00", result);
    }

    /// <summary>
    /// Defines the test method DateTimeHelperAcceptsDefaultTemplateVariables.
    /// </summary>
    [Fact]
    public void DateTimeHelperAcceptsDefaultTemplateVariables()
    {
        if (!IanaTimeZonesSupported())
        {
            return;
        }

        var template = Handlebars.Compile("{{datetime When $DateTimeFormat $TimeZoneName}}");
        var data = Error.LogEvent(includedProperties: new Dictionary<string, object> { ["When"] = new DateTime(2023, 3, 1, 17, 30, 11, DateTimeKind.Utc) });
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal("2023-03-02T03:30:11.0000000+10:00", result);
    }

    /// <summary>
    /// Defines the test method UtcFormatsWithZNotation.
    /// </summary>
    [Fact]
    public void UtcFormatsWithZNotation()
    {
        var template = Handlebars.Compile("{{datetime When 'o' 'Etc/UTC'}}");
        var data = Error.LogEvent(includedProperties: new Dictionary<string, object> { ["When"] = new DateTime(2023, 3, 1, 17, 30, 11, DateTimeKind.Utc) });
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Equal("2023-03-01T17:30:11.0000000Z", result);
    }

    /// <summary>
    /// Defines the test method DateTimeHelperRecognizesDefaultUsedInBodyTemplate.
    /// </summary>
    [Fact]
    public void DateTimeHelperRecognizesDefaultUsedInBodyTemplate()
    {
        // `G` is dependent on the server's current culture; maintained for backwards-compatibility
        var template = Handlebars.Compile("{{datetime When 'G' 'Etc/UTC'}}");
        var data = Error.LogEvent(includedProperties: new Dictionary<string, object> { ["When"] = new DateTime(2023, 3, 1, 17, 30, 11, DateTimeKind.Utc) });
        var result = EmailApp.TestFormatTemplate(template, data, Error.Host());
        Assert.Contains("2023", result);
    }

    /// <summary>
    /// Ianas the time zones supported.
    /// </summary>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    private static bool IanaTimeZonesSupported()
    {
        // Only Windows 10, despite Server 2019 having icu.dll
        return !PortableTimeZoneInfo.IsUsingNlsOnWindows();
    }
}