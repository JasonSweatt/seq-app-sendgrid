using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HandlebarsDotNet;
using Newtonsoft.Json;

namespace Seq.App.SendGrid.Helpers;

/// <summary>
/// Class HandlebarsHelpers.
/// </summary>
public static class HandlebarsHelpers
{
    /// <summary>
    /// Registers this instance.
    /// </summary>
    public static void Register()
    {
        Handlebars.RegisterHelper("pretty", PrettyPrintHelper);
        Handlebars.RegisterHelper("if_eq", IfEqHelper);
        Handlebars.RegisterHelper("substring", SubstringHelper);
        Handlebars.RegisterHelper("datetime", DateTimeHelper);
    }

    /// <summary>
    /// Pretties the print helper.
    /// </summary>
    /// <param name="output">The output.</param>
    /// <param name="context">The context.</param>
    /// <param name="arguments">The arguments.</param>
    private static void PrettyPrintHelper(EncodedTextWriter output, Context context, Arguments arguments)
    {
        var argument = arguments.FirstOrDefault();
        if (argument == null)
        {
            output.WriteSafeString("null");
        }
        else if (argument is IEnumerable<object> || argument is IEnumerable<KeyValuePair<string, object>>)
        {
            output.Write(JsonConvert.SerializeObject(FromDynamic(argument)));
        }
        else
        {
            var value = argument.ToString();
            if (string.IsNullOrWhiteSpace(value))
            {
                output.WriteSafeString("&nbsp;");
            }
            else
            {
                output.Write(value);
            }
        }
    }

    /// <summary>
    /// Ifs the eq helper.
    /// </summary>
    /// <param name="output">The output.</param>
    /// <param name="options">The options.</param>
    /// <param name="context">The context.</param>
    /// <param name="arguments">The arguments.</param>
    private static void IfEqHelper(EncodedTextWriter output, BlockHelperOptions options, Context context, Arguments arguments)
    {
        if (arguments.Length != 2)
        {
            options.Inverse(output, context);
            return;
        }

        var lhs = (arguments[0]?.ToString() ?? string.Empty).Trim();
        var rhs = (arguments[1]?.ToString() ?? string.Empty).Trim();

        if (lhs.Equals(rhs, StringComparison.Ordinal))
        {
            options.Template(output, context);
        }
        else
        {
            options.Inverse(output, context);
        }
    }

    /// <summary>
    /// From dynamic.
    /// </summary>
    /// <param name="object">The object.</param>
    /// <returns>System.Object.</returns>
    private static object FromDynamic(object @object)
    {
        if (@object is IEnumerable<KeyValuePair<string, object>> dictionary)
        {
            return dictionary.ToDictionary(kvp => kvp.Key, kvp => FromDynamic(kvp.Value));
        }

        if (@object is IEnumerable<object> enumerable)
        {
            return enumerable.Select(FromDynamic).ToArray();
        }

        return @object;
    }

    /// <summary>
    /// Substrings the helper.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>System.Object.</returns>
    private static object? SubstringHelper(Context context, Arguments arguments)
    {
        // {{ substring value 0 30 }}
        var value = arguments.FirstOrDefault();

        var toString = value?.ToString();
        if (toString == null)
        {
            return value;
        }

        if (arguments.Length < 2)
        {
            // No start or length arguments provided
            return value;
        }

        int start;
        if (arguments.Length < 3)
        {
            // just a start position provided
            if (!int.TryParse(arguments[1].ToString(), out start) || start > toString.Length)
            {
                // start of substring after end of string.
                return string.Empty;
            }

            return toString.Substring(start);
        }

        // Start & length provided.
        int.TryParse(arguments[1].ToString(), out start);
        int.TryParse(arguments[2].ToString(), out var end);

        if (start > toString.Length)
        {
            // start of substring after end of string.
            return string.Empty;
        }

        // ensure the length is still in the string to avoid ArgumentOutOfRangeException
        if (end > toString.Length - start)
        {
            end = toString.Length - start;
        }

        return toString.Substring(start, end);
    }

    /// <summary>
    /// Dates the time helper.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>System.Object.</returns>
    private static object? DateTimeHelper(Context context, Arguments arguments)
    {
        if (arguments.Length < 1)
        {
            return null;
        }

        // Using `DateTimeOffset` avoids ending up with `DateTimeKind.Unspecified` after time zone conversion.
        DateTimeOffset dateTimeOffset;
        if (arguments[0] is DateTimeOffset dto)
        {
            dateTimeOffset = dto;
        }
        else if (arguments[0] is DateTime rdt)
        {
            dateTimeOffset = rdt.Kind == DateTimeKind.Unspecified ? new DateTime(rdt.Ticks, DateTimeKind.Utc) : rdt;
        }
        else if (arguments[0] is not string input || !DateTimeOffset.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeOffset))
        {
            return null;
        }

        var format = default(string?);
        if (arguments.Length >= 2 && arguments[1] is string f)
        {
            format = f;
        }

        if (arguments.Length >= 3 && arguments[2] is string timeZoneId)
        {
            var timeZoneInfo = PortableTimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            dateTimeOffset = TimeZoneInfo.ConvertTime(dateTimeOffset, timeZoneInfo);
        }

        if (dateTimeOffset.Offset == TimeSpan.Zero)
        {
            // Use the idiomatic trailing `Z` formatting for ISO-8601 in UTC.
            return dateTimeOffset.UtcDateTime.ToString(format);
        }

        return dateTimeOffset.ToString(format);
    }
}