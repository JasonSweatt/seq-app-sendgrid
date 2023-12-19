using System.Reflection;
using System.Threading.Tasks;
using Seq.App.SendGrid.Mailer.Interfaces;

namespace Seq.App.SendGrid.Mailer.Defaults;

/// <summary>
/// Class ReplaceRenderer.
/// Implements the <see cref="ITemplateRenderer" />
/// </summary>
/// <seealso cref="ITemplateRenderer" />
public class ReplaceRenderer : ITemplateRenderer
{
    /// <inheritdoc />
    public string Parse<TModel>(string template, TModel model, bool isHtml = true)
    {
        foreach (var propertyInfo in model.GetType().GetRuntimeProperties())
        {
            template = template.Replace($"##{propertyInfo.Name}##", propertyInfo.GetValue(model, null).ToString());
        }

        return template;
    }

    /// <inheritdoc />
    public Task<string> ParseAsync<TModel>(string template, TModel model, bool isHtml = true)
    {
        return Task.FromResult(Parse(template, model, isHtml));
    }
}