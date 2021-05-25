using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace TTSCore.Org
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            // We want to add our menus to the "admin" menu only.
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            // Adding our menu items to the builder.
            // The builder represents the full admin menu tree.
            builder
                .Add(S["组织架构"], S["组织架构"].PrefixPosition(), rootView => rootView
                   .Add(S["组织机构"], S["组织机构"].PrefixPosition(), childOne => childOne
                       .Action("Index", "Admin", new { area = "TTSCore.Org" })));

            return Task.CompletedTask;
        }
    }
}
