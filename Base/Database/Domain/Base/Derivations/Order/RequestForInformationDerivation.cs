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

    public class RequestForInformationDerivations : IDomainDerivation
    {
        public Guid Id => new Guid("5BCE8864-6EC2-4672-A29D-CA49A6C49718");

        public IEnumerable<Pattern> Patterns { get; } = new[] { new CreatedPattern(M.RequestForInformation.Class) };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var requestForInformation in matches.Cast<RequestForInformation>())
            {
                foreach (RequestItem requestItem in requestForInformation.RequestItems)
                {
                    requestItem.Sync(requestForInformation);
                }

                var deletePermission = new Permissions(requestForInformation.Strategy.Session).Get(requestForInformation.Meta.ObjectType, requestForInformation.Meta.Delete, Operations.Execute);
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
