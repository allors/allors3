// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class FaxCommunicationDerivation : DomainDerivation
    {
        public FaxCommunicationDerivation(M m) : base(m, new Guid("A6D89A8A-641F-4D11-8E92-CC10A7A2A89E")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.FaxCommunication.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<FaxCommunication>())
            {
                @this.WorkItemDescription = $"Fax to {@this.ToParty.PartyName} about {@this.Subject}";
            }
        }
    }
}
