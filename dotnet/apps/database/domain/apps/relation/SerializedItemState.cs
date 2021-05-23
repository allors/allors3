// <copyright file="GenderType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class GenderType
    {
        public bool IsMale => this.Equals(new GenderTypes(this.Strategy.Transaction).Male);

        public bool IsFemale => this.Equals(new GenderTypes(this.Strategy.Transaction).Female);

        public bool IsOther => this.Equals(new GenderTypes(this.Strategy.Transaction).Other);
    }
}
