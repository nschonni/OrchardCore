using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;
using OrchardCore.Mvc.LocationExpander;

namespace OrchardCore.DisplayManagement.LocationExpander
{
    public class ThemeViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        private readonly IExtensionManager _extensionManager;

        public ThemeViewLocationExpanderProvider(IExtensionManager extensionManager)
        {
            _extensionManager = extensionManager;
        }

        public int Priority => 15;

        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            var themeManager = context
                .ActionContext
                .HttpContext
                .RequestServices
                .GetService<IThemeManager>();

            if (themeManager != null)
            {
                var currentTheme = themeManager.GetThemeAsync().GetAwaiter().GetResult();

                if (currentTheme != null)
                {
                    context.Values["Theme"] = currentTheme.Id;
                }
            }
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations)
        {
            if (!context.Values.ContainsKey("Theme"))
            {
                return viewLocations;
            }

            var currentThemeId = context.Values["Theme"];

            var currentThemeAndBaseThemesOrdered = _extensionManager
                .GetFeatures(new[] { currentThemeId })
                .Where(x => x.Extension.IsTheme())
                .Reverse();

            var result = new List<string>();

                foreach (var theme in currentThemeAndBaseThemesOrdered)
                {
                if (context.AreaName != theme.Id)
                {
                    var themePagesPath = '/' + theme.Extension.SubPath + "/Pages";
                    var themeViewsPath = '/' + theme.Extension.SubPath + "/Views";

                    if (context.AreaName != null)
                    {
                        if (context.PageName != null)
                        {
                            result.Add(themePagesPath + "/{2}/{0}" + RazorViewEngine.ViewExtension);
                        }
                        else
                        {
                            result.Add(themeViewsPath + "/{2}/{1}/{0}" + RazorViewEngine.ViewExtension);
                        }
                    }

                    if (context.PageName != null)
                    {
                        result.Add(themePagesPath + "/Shared/{0}" + RazorViewEngine.ViewExtension);
                    }

                    if (context.AreaName != null)
                    {
                        result.Add(themeViewsPath + "/{2}/Shared/{0}" + RazorViewEngine.ViewExtension);
                    }

                    result.Add(themeViewsPath + "/Shared/{0}" + RazorViewEngine.ViewExtension);
                }
            }

            result.AddRange(viewLocations);
            return result;
        }
    }
}
