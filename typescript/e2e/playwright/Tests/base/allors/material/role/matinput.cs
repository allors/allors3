// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System;

    using System.Globalization;
    using System.Threading.Tasks;
    using System.Xml;
    using Allors;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;
    using DateTime = System.DateTime;
    using Task = System.Threading.Tasks.Task;

    public class MatInput : SelectorComponent
    {
        public MatInput(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes)
        : base(page, m)
        {
            this.Selector = $"a-mat-input{this.ByScopesPredicate(scopes)} [@data-allors-roletype='{roleType.RelationType.Tag}']//input";
            this.RoleType = roleType;
        }

        public MatInput(IPage page, MetaPopulation m, string selector, RoleType roleType)
            : base(page, m)
        {
            this.Selector = selector;
            this.RoleType = roleType;
        }

        public override string Selector { get; }

        public RoleType RoleType { get; }

        public async Task<string> GetText()
        {
            await this.Page.WaitForAngular();

            var element = this.Page.Locator(this.Selector);
            return await element.GetAttributeAsync("value");
        }

        public async Task SetText(string value)
        {
            await this.Page.WaitForAngular();

            var element = this.Page.Locator(this.Selector);
            await element.FillAsync(value);
        }

        public async Task<object> GetValue()
        {
            var locale = await this.Page.Locale();
            var cultureInfo = new CultureInfo(locale);

            var text = await this.GetText();
            if (text == null)
            {
                return null;
            }

            var unit = (Unit)this.RoleType.ObjectType;
            return unit.Tag switch
            {
                UnitTags.String => text,
                UnitTags.Integer => Convert.ToInt32(text),
                UnitTags.Decimal => Convert.ToDecimal(text, cultureInfo),
                UnitTags.Float => Convert.ToDouble(text, cultureInfo),
                UnitTags.Boolean => Convert.ToBoolean(text),
                UnitTags.DateTime => XmlConvert.ToDateTime(text, XmlDateTimeSerializationMode.Utc),
                UnitTags.Unique => Guid.Parse(text),
                UnitTags.Binary => Convert.FromBase64String(text),
                _ => throw new ArgumentException("Unknown Unit ObjectType: " + unit),
            };
        }

        public async Task SetValue(object value)
        {
            if (value is string stringValue)
            {
                await this.SetText(stringValue);
                return;
            }

            var locale = await this.Page.Locale();
            var cultureInfo = new CultureInfo(locale);

            var unit = (Unit)this.RoleType.ObjectType;
            switch (unit.Tag)
            {
                case UnitTags.String:
                    await this.SetText((string)value);
                    break;
                case UnitTags.Integer:
                    await this.SetText(Convert.ToString((int)value, cultureInfo));
                    break;
                case UnitTags.Decimal:
                    await this.SetText(Convert.ToString((decimal)value, cultureInfo));
                    break;
                case UnitTags.Float:
                    await this.SetText(Convert.ToString((double)value, cultureInfo));
                    break;
                case UnitTags.Boolean:
                    await this.SetText(Convert.ToString((bool)value, cultureInfo));
                    break;
                case UnitTags.DateTime:
                    await this.SetText(XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.Utc));
                    break;
                case UnitTags.Unique:
                    await this.SetText(Convert.ToString((Guid)value, cultureInfo));
                    break;
                case UnitTags.Binary:
                    await this.SetText(Convert.ToBase64String((byte[])value));
                    break;
                default:
                    throw new ArgumentException("Unknown Unit ObjectType: " + unit);
            }
        }
    }

    public class MatInput<T> : MatInput where T : Component
    {
        public MatInput(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> SetText(string value)
        {
            await base.SetText(value);
            return this.Page;
        }

        public new async Task<T> SetValue(object value)
        {
            await base.SetValue(value);
            return this.Page;
        }
    }
}
