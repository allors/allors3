// <copyright file="Cities.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class Cities
    {
        private Cache<string, City> cityByName;

        public Cache<string, City> CityByName => this.cityByName ??= new Cache<string, City>(this.Session, this.M.City.Name);
    }
}
