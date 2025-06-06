// <copyright file="Settingses.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Settingses
    {
        protected override void CustomSetup(Setup setup)
        {
            var singleton = this.Transaction.GetSingleton();
            singleton.Settings ??= new SettingsBuilder(this.Transaction)
                .WithUseProductNumberCounter(true)
                .WithUsePartNumberCounter(true)
                .Build();

            var settings = singleton.Settings;

            var inventoryStrategy = new InventoryStrategies(this.Transaction).Standard;
            var preferredCurrency = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");

            settings.InventoryStrategy ??= inventoryStrategy;
            settings.SkuPrefix ??= "Sku-";
            settings.SerialisedItemPrefix ??= "S-";
            settings.ProductNumberPrefix ??= "Art-";
            settings.PartNumberPrefix ??= "Part-";
            settings.PreferredCurrency ??= preferredCurrency;

            settings.SkuCounter ??= new CounterBuilder(this.Transaction).Build();
            settings.SerialisedItemCounter ??= new CounterBuilder(this.Transaction).Build();
            settings.ProductNumberCounter ??= new CounterBuilder(this.Transaction).Build();
            settings.PartNumberCounter ??= new CounterBuilder(this.Transaction).Build();
            settings.TransactionNumberCounter ??= new CounterBuilder(this.Transaction).Build();
        }
    }
}
