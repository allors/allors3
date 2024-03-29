// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Role
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Info;
    using Microsoft.Playwright;

    public abstract class RoleComponent : IComponent
    {
        private static readonly char[] CssEscapeCharacters = new char[] { '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '`', '{', '|', '}', '~' };
        private static readonly IDictionary<char, string> CssReplacements = CssEscapeCharacters.ToDictionary(v => v, v => $"\\{v}");

        protected RoleComponent(IComponent container, IRoleType roleType, string elementName)
        {
            this.Container = container;
            this.RoleType = roleType;
            this.Locator = this.Container.Locator.Locator($"{elementName}[data-allors-roletype='{roleType.RelationType.Tag}']");
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;

        public IRoleType RoleType { get; }

        public string CssEscape(string value) => string.Join("", value.Select(v => CssReplacements.TryGetValue(v, out var replacement) ? replacement : v.ToString()));
    }
}
