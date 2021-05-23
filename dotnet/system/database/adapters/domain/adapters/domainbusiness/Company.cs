// <copyright file="Company.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Company
    {
        public static Company Create(ITransaction transaction, string name)
        {
            var company = Create(transaction);
            company.Name = name;
            return company;
        }

        public static Company Create(ITransaction transaction, string name, int index)
        {
            var company = Create(transaction);
            company.Name = name;
            company.Index = index;
            return company;
        }

        public override string ToString() => this.Name;
    }
}
