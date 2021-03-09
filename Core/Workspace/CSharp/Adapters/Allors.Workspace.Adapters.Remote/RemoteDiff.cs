// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Push;
    using Meta;

    public sealed class RemoteDiff
    {
        public RemoteDiff(IReadOnlyDictionary<IRelationType, object> x, IReadOnlyDictionary<IRelationType, object> y)
        {
            var relationTypes = new List<IRelationType>();

            foreach (var kvp in x)
            {
                var relationType = kvp.Key;
                var role = kvp.Value;

                if (y.TryGetValue(relationType, out var otherRole))
                {
                    var roleType = relationType.RoleType;
                    if (!roleType.IsMany)
                    {
                        if (Equals(role, otherRole))
                        {
                            continue;
                        }
                    }
                    else
                    {

                    }
                }

                relationTypes.Add(relationType);
            }


            this.RelationTypes = relationTypes.ToArray();
        }

        public IRelationType[] RelationTypes { get; }
    }
}
