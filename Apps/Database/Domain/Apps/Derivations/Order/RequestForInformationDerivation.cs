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
                new CreatedPattern(this.M.RequestForInformation.Class)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var requestForInformation in matches.Cast<RequestForInformation>())
            {
                foreach (RequestItem requestItem in requestForInformation.RequestItems)
                {
                    requestItem.Sync(requestForInformation);
                }

                var deletePermission = new Permissions(requestForInformation.Strategy.Session).Get(requestForInformation.Meta.ObjectType, requestForInformation.Meta.Delete);
                if (requestForInformation.IsDeletable())
                {
                    requestForInformation.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    requestForInformation.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}