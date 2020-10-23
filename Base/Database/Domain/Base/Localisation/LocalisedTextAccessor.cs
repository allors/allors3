// <copyright file="LocalisedText.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using Meta;

    public partial class LocalisedTextAccessor
    {
        private readonly RoleType roleType;

        public LocalisedTextAccessor(RoleType roleType) => this.roleType = roleType;

        public string Get(IObject @object, Locale locale)
        {
            var localisedTexts = @object.Strategy.GetCompositeRoles(this.roleType);
            foreach (LocalisedText localisedText in localisedTexts)
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
            var localisedTexts = @object.Strategy.GetCompositeRoles(this.roleType);
            foreach (LocalisedText existingLocalisedText in localisedTexts)
            {
                if (existingLocalisedText?.Locale?.Equals(locale) == true)
                {
                    existingLocalisedText.Text = text;
                    return;
                }
            }

            var newLocalisedText = new LocalisedTextBuilder(@object.Strategy.Session)
                .WithLocale(locale)
                .WithText(text)
                .Build();
            @object.Strategy.AddCompositeRole(this.roleType, newLocalisedText);
        }
    }
}
