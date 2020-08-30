// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class OrderValueCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
               var createdOrderValues = changeSet.Created.Select(v=>v.GetObject()).OfType<OrderValue>();

                foreach(var orderValue in createdOrderValues)
                {
                    validation.AssertAtLeastOne(orderValue, M.OrderValue.FromAmount, M.OrderValue.ThroughAmount);
                }
            }
        }

        public static void OrderValueRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("b9356494-1a3a-4f3f-90b0-cd47e7786e34")] = new OrderValueCreationDerivation();
        }
    }
}
