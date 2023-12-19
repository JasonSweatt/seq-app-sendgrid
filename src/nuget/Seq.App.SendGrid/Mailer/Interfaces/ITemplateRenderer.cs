using System.Threading.Tasks;

namespace Seq.App.SendGrid.Mailer.Interfaces;

/// <summary>
/// Interface ITemplateRenderer
/// </summary>
public interface ITemplateRenderer
{
    /// <summary>
    /// Parses the specified template.
    /// </summary>
    /// <typeparam name="TModel">The type of the t model.</typeparam>
    /// <param name="template">The template.</param>
    /// <param name="model">The model.</param>
    /// <param name="isHtml">if set to <c>true</c> [is HTML].</param>
    /// <returns>System.String.</returns>
    string Parse<TModel>(string template, TModel model, bool isHtml = true);

    /// <summary>
    /// Parses the asynchronous.
    /// </summary>
    /// <typeparam name="TModel">The type of the t model.</typeparam>
    /// <param name="template">The template.</param>
    /// <param name="model">The model.</param>
    /// <param name="isHtml">if set to <c>true</c> [is HTML].</param>
    /// <returns>Task&lt;System.String&gt;.</returns>
    Task<string> ParseAsync<TModel>(string template, TModel model, bool isHtml = true);
}