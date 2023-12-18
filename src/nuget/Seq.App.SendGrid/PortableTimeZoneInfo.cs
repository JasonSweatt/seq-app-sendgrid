using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Seq.App.SendGrid;

/// <summary>
/// Class PortableTimeZoneInfo.
/// </summary>
public static class PortableTimeZoneInfo
{
    /// <summary>
    /// The UTC time zone name
    /// </summary>
    public const string UtcTimeZoneName = "Etc/UTC";

    /// <summary>
    /// Determines whether [is using NLS on windows].
    /// </summary>
    /// <returns><c>true</c> if [is using NLS on windows]; otherwise, <c>false</c>.</returns>
    public static bool IsUsingNlsOnWindows()
    {
        // Whether ICU is used on Windows depends on both the Windows version and the .NET version. When ICU is
        // unavailable, .NET falls back to NLS, which is only aware of Windows time zone names.
        // See: https://github.com/dotnet/docs/issues/30319

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        // https://learn.microsoft.com/en-us/dotnet/core/extensions/globalization-icu#determine-if-your-app-is-using-icu
        var sortVersion = CultureInfo.InvariantCulture.CompareInfo.Version;
        var bytes = sortVersion.SortId.ToByteArray();
        var version = (bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0];
        var isIcu = version != 0 && version == sortVersion.FullVersion;

        return !isIcu;
    }

    /// <summary>
    /// Finds the system time zone by identifier.
    /// </summary>
    /// <param name="timeZoneId">The time zone identifier.</param>
    /// <returns>TimeZoneInfo.</returns>
    public static TimeZoneInfo FindSystemTimeZoneById(string timeZoneId)
    {
        if (IsUsingNlsOnWindows() && timeZoneId == UtcTimeZoneName)
        {
            // Etc/UTC is the default; this keeps the default template working even without ICU.
            return TimeZoneInfo.Utc;
        }

        return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }
}