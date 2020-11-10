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

    public class RequestForInformationDerivation : DomainDerivation
    {
        public RequestForInformationDerivation(M m) : base(m, new Guid("5BCE8864-6EC2-4672-A29D-CA49A6C49718")) =>
            this.Patterns = new[]
            {
                new ChangedPattern(this.M.RequestForInformation.RequestItems)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<RequestForInformation>())
            {
                foreach (RequestItem requestItem in @this.RequestItems)
                {
                    requestItem.Sync(@this);
                }
            }
        }
    }
}
