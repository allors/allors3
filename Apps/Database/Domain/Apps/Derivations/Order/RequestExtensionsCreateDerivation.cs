// <copyright file="PartyFinancialRelationshipCreationDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class RequestExtensionsCreateDerivation : DomainDerivation
    {
        public RequestExtensionsCreateDerivation(M m) : base(m, new Guid("1961ff84-b0ba-4736-b99e-fe17ea71c2aa")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.Request.Interface)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Request>())
            {
                if (!@this.ExistRequestState && !@this.ExistOriginator)
                {
                    @this.RequestState = new RequestStates(@this.Session()).Anonymous;
                }

                if (!@this.ExistRequestState && @this.ExistOriginator)
                {
                    @this.RequestState = new RequestStates(@this.Session()).Submitted;
                }
            }
        }
    }
}
