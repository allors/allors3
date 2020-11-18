// <copyright file="Organisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Person type.</summary>

namespace Allors.Database.Domain
{
   

    public partial class Organisations
    {
        private UniquelyIdentifiableCache<Organisation> cache;

        public UniquelyIdentifiableCache<Organisation> Cache => this.cache ??= new UniquelyIdentifiableCache<Organisation>(this.Session);
    }
}
