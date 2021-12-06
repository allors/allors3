// <copyright file="Four.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    /// <summary>
    /// Shared.
    /// </summary>
    public partial class Data
    {
        public void CustomOnInit(ObjectOnInit method)
        {
            var singleton = this.strategy.Transaction.GetSingleton();

            if (!this.ExistAutocompleteDerivedFilter)
            {
                this.AutocompleteDerivedFilter ??= singleton.AutocompleteDefault;
            }

            if (!this.ExistSelectDerived)
            {
                this.SelectDerived ??= singleton.SelectDefault;
            }
        }

        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            var singleton = this.strategy.Transaction.GetSingleton();

            var people = new People(this.strategy.Transaction);
            var john = people.FindBy(people.Meta.FirstName, "John");

            this.AutocompleteDerivedFilter ??= john;
            this.AutocompleteDerivedOptions ??= john;
            this.SelectDerived ??= john;
        }
    }
}
