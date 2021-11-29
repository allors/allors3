// <copyright file="SelectorComponent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Microsoft.Playwright;

    public abstract class SelectorComponent : Component
    {
        private static readonly char[] CssEscapeCharacters = new char[] { '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '`', '{', '|', '}', '~' };
        private static readonly IDictionary<char, string> CssReplacements = CssEscapeCharacters.ToDictionary(v => v, v => $"\\{v}");

        protected SelectorComponent(IPage page, MetaPopulation m) : base(page, m)
        {
        }

        public abstract string Selector { get; }

        public string CssEscape(string value) => string.Concat(value.Select(v => CssReplacements.TryGetValue(v, out var replacement) ? replacement : v.ToString()));
    }
}
