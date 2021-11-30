// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Angular.Components
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Microsoft.Playwright;

    public abstract class RoleControl : IComponent
    {
        private static readonly char[] CssEscapeCharacters = new char[] { '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '`', '{', '|', '}', '~' };
        private static readonly IDictionary<char, string> CssReplacements = CssEscapeCharacters.ToDictionary(v => v, v => $"\\{v}");

        protected RoleControl(IComponent container, RoleType roleType, string elementName)
        {
            this.Container = container;
            this.RoleType = roleType;
            this.Locator = this.Page.Locator($"{elementName}[data-allors-component-type='{this.GetType().Name}']:has([data-allors-roletype='{roleType.RelationType.Tag}'])");
        }

        public IComponent Container { get; }

        public IPage Page => this.Container.Page;

        public MetaPopulation M => this.Container.M;

        public ILocator Locator { get; }

        public RoleType RoleType { get; }

        public string CssEscape(string value) => string.Join("", value.Select(v => CssReplacements.TryGetValue(v, out var replacement) ? replacement : v.ToString()));
    }
}