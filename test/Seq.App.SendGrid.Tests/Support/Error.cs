using System;
using System.Collections.Generic;
using Seq.Apps;
using Seq.Apps.LogEvents;

// ReSharper disable MemberCanBePrivate.Global

namespace Seq.App.SendGrid.Tests.Support;

/// <summary>
/// Class Error.
/// </summary>
public static class Error
{
    /// <summary>
    /// Strings this instance.
    /// </summary>
    /// <returns>System.String.</returns>
    public static string String()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Uints this instance.
    /// </summary>
    /// <returns>System.UInt32.</returns>
    public static uint Uint()
    {
        return 5417u;
    }

    /// <summary>
    /// Events the type.
    /// </summary>
    /// <returns>System.UInt32.</returns>
    public static uint EventType()
    {
        return Uint();
    }

    /// <summary>
    /// Logs the event.
    /// </summary>
    /// <param name="eventType">Type of the event.</param>
    /// <param name="includedProperties">The included properties.</param>
    /// <returns>Event&lt;LogEventData&gt;.</returns>
    public static Event<LogEventData> LogEvent(uint? eventType = null, IDictionary<string, object> includedProperties = null)
    {
        var id = EventId();
        var timestamp = UtcTimestamp();
        var properties = new Dictionary<string, object>
        {
            { "Application", "Seq - App" },
            { "Number", 99 },
            { "Region", "Western" },
            { "State", "WA" },
        };

        if (includedProperties != null)
        {
            foreach (var includedProperty in includedProperties)
            {
                properties.Add(includedProperty.Key, includedProperty.Value);
            }
        }

        return new Event<LogEventData>(
            id, eventType ?? EventType(), timestamp, new LogEventData
            {
                Exception = null,
                Id = id,
                Level = LogEventLevel.Fatal,
                LocalTimestamp = new DateTimeOffset(timestamp),
                MessageTemplate = "Hello, {Application}",
                RenderedMessage = "Hello, Seq - App",
                Properties = properties
            });
    }

    /// <summary>
    /// Events the identifier.
    /// </summary>
    /// <returns>System.String.</returns>
    public static string EventId()
    {
        return $"event-{String()}";
    }

    /// <summary>
    /// UTCs the timestamp.
    /// </summary>
    /// <returns>DateTime.</returns>
    public static DateTime UtcTimestamp()
    {
        return DateTime.UtcNow;
    }

    /// <summary>
    /// Hosts this instance.
    /// </summary>
    /// <returns>Host.</returns>
    public static Host Host()
    {
        return new Host("https://seq.example.com", String());
    }
}