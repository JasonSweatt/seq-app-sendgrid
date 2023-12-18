using System.IO;
using System.Reflection;

namespace Seq.App.SendGrid.Mailer;

/// <summary>
/// Class EmbeddedResourceHelper.
/// </summary>
internal static class EmbeddedResourceHelper
{
    /// <summary>
    /// Gets the resource as string.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="path">The path.</param>
    /// <returns>System.String.</returns>
    internal static string GetResourceAsString(Assembly assembly, string path)
    {
        string result;

        using (var stream = assembly.GetManifestResourceStream(path))
        {
            using (var reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }
        }

        return result;
    }
}