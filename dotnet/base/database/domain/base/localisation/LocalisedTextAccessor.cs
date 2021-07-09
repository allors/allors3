// <copyright file="LocalisedText.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;

    public partial class LocalisedTextAccessor
    {
        private readonly IRoleType roleType;

        public LocalisedTextAccessor(IRoleType roleType) => this.roleType = roleType;

        public string Get(IObject @object, Locale locale)
        {
            foreach (var localisedText in @object.Strategy.GetCompositesRole<LocalisedText>(this.roleType))
            {
                if (localisedText?.Locale?.Equals(locale) == true)
                {
                    return localisedText.Text;
                }
            }

            return null;
        }

        public void Set(IObject @object, Locale locale, string text)
        {
            foreach (var existingLocalisedText in @object.Strategy.GetCompositesRole<LocalisedText>(this.roleType))
            {
                if (existingLocalisedText?.Locale?.Equals(locale) == true)
                {
                    existingLocalisedText.Text = text;
                    return;
                }
            }

            var newLocalisedText = new LocalisedTextBuilder(@object.Strategy.Transaction)
                .WithLocale(locale)
                .WithText(text)
                .Build();
            @object.Strategy.AddCompositesRole(this.roleType, newLocalisedText);
        }
    }
}
