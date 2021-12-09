// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Role
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using System.Xml;
    using Allors;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialInputComponent : RoleComponent
    {
        public AllorsMaterialInputComponent(IComponent container, IRoleType roleType) : base(container, roleType, "a-mat-input")
        {
        }

        public ILocator InputLocator => this.Locator.Locator("input");

        public async Task<string> GetValueAsync()
        {
            await this.Page.WaitForAngular();
            return await this.InputLocator.InputValueAsync();
        }

        public async Task SetValueAsync(string value)
        {
            await this.Page.WaitForAngular();
            await this.InputLocator.FillAsync(value);
        }

        public async Task<object> GetAsync()
        {
            var locale = await this.Page.Locale();
            var cultureInfo = new CultureInfo(locale);

            var text = await this.GetValueAsync();
            if (text == null)
            {
                return null;
            }

            var unit = (IUnit)this.RoleType.ObjectType;
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

        public async Task SetAsync(object value)
        {
            if (value is string stringValue)
            {
                await this.SetValueAsync(stringValue);
                return;
            }

            var locale = await this.Page.Locale();
            var cultureInfo = new CultureInfo(locale);

            var unit = (IUnit)this.RoleType.ObjectType;
            switch (unit.Tag)
            {
                case UnitTags.String:
                    await this.SetValueAsync((string)value);
                    break;
                case UnitTags.Integer:
                    await this.SetValueAsync(Convert.ToString((int)value, cultureInfo));
                    break;
                case UnitTags.Decimal:
                    await this.SetValueAsync(Convert.ToString((decimal)value, cultureInfo));
                    break;
                case UnitTags.Float:
                    await this.SetValueAsync(Convert.ToString((double)value, cultureInfo));
                    break;
                case UnitTags.Boolean:
                    await this.SetValueAsync(Convert.ToString((bool)value, cultureInfo));
                    break;
                case UnitTags.DateTime:
                    await this.SetValueAsync(XmlConvert.ToString((System.DateTime)value,
                        XmlDateTimeSerializationMode.Utc));
                    break;
                case UnitTags.Unique:
                    await this.SetValueAsync(Convert.ToString((Guid)value, cultureInfo));
                    break;
                case UnitTags.Binary:
                    await this.SetValueAsync(Convert.ToBase64String((byte[])value));
                    break;
                default:
                    throw new ArgumentException("Unknown Unit ObjectType: " + unit);
            }
        }
    }
}
