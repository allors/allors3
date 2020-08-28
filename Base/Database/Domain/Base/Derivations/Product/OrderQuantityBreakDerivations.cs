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
        public class OrderQuantityBreakCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
               var createdOrderQuantityBreaks = changeSet.Created.Select(session.Instantiate).OfType<OrderQuantityBreak>();

                foreach(var orderQuantityBreak in createdOrderQuantityBreaks)
                {
                    validation.AssertAtLeastOne(orderQuantityBreak, M.OrderQuantityBreak.FromAmount, M.OrderQuantityBreak.ThroughAmount);
                }
            }
        }

        public static void OrderQuantityBreakRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("2374a17e-9d35-4b1b-a6ac-f0715332c805")] = new OrderQuantityBreakCreationDerivation();
        }
    }
}
