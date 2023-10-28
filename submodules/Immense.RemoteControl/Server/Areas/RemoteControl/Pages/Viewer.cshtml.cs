using Immense.RemoteControl.Server.Abstractions;
using Immense.RemoteControl.Server.Filters;
using Immense.RemoteControl.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Immense.RemoteControl.Server.Areas.RemoteControl.Pages
{
    [ServiceFilter(typeof(ViewerAuthorizationFilter))]
    public class ViewerModel : PageModel
    {
        private readonly IViewerPageDataProvider _viewerDataProvider;

        public ViewerModel(IViewerPageDataProvider viewerDataProvider)
        {
            _viewerDataProvider = viewerDataProvider;
        }

        public string ThemeUrl { get; private set; } = string.Empty;
        public string UserDisplayName { get; private set; } = string.Empty;

        public void OnGet()
        {
            var theme = _viewerDataProvider.GetTheme(this);

            ThemeUrl = theme switch
            {
                ViewerPageTheme.Dark => "/_content/Immense.RemoteControl.Server/css/remote-control-dark.css",
                ViewerPageTheme.Light => "/_content/Immense.RemoteControl.Server/css/remote-control-light.css",
                _ => "/_content/Immense.RemoteControl.Server/css/remote-control-dark.css"
            };
            UserDisplayName = _viewerDataProvider.GetUserDisplayName(this);
        }
    }
}
