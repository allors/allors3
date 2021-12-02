// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Angular.Components
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database;
    using Allors.Database.Meta;
    using Microsoft.Playwright;

    public abstract class ObjectControl<T> : IComponent where T : IObject
    {
        private static readonly char[] CssEscapeCharacters = new char[] { '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '`', '{', '|', '}', '~' };
        private static readonly IDictionary<char, string> CssReplacements = CssEscapeCharacters.ToDictionary(v => v, v => $"\\{v}");

        protected ObjectControl(IComponent container, T @object, string elementName)
        {
            this.Container = container;
            this.Object = @object;
            this.Locator = this.Page.Locator($"{elementName}[data-allors-component-type='{this.GetType().Name}']:has([data-allors-id='{@object.Id}'])");
        }

        public IComponent Container { get; }

        public IPage Page => this.Container.Page;

        public MetaPopulation M => this.Container.M;

        public ILocator Locator { get; }

        public T Object { get; }

        public string CssEscape(string value) => string.Join("", value.Select(v => CssReplacements.TryGetValue(v, out var replacement) ? replacement : v.ToString()));
    }
}
