// <copyright file="Sidenav.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Sidenav
{
    using System.Threading.Tasks;
    using Person;

    public partial class Sidenav
    {
        public async Task<PersonListComponent> NavigateToPeople()
        {
            await this.NavigateAsync("Contacts", "People");
            return new PersonListComponent(this.AppRoot);
        }
    }
}
