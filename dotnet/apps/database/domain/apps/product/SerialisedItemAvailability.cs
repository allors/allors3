// <copyright file="SerialisedItemAvailability.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SerialisedItemAvailability
    {
        public bool IsAvailable => this.Equals(new SerialisedItemAvailabilities(this.Strategy.Transaction).Available);

        public bool IsSold => this.Equals(new SerialisedItemAvailabilities(this.Strategy.Transaction).Sold);

        public bool IsInRent => this.Equals(new SerialisedItemAvailabilities(this.Strategy.Transaction).InRent);
    }
}
