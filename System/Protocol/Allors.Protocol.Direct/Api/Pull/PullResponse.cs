// <copyright file="PullResponse.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Direct.Api.Pull
{
    using System.Collections.Generic;
    using Database;

    public class PullResponse
    {
        public Dictionary<string, IStrategy[]> NamedCollections { get; set; }

        public Dictionary<string, IStrategy> NamedObjects { get; set; }

        public Dictionary<string, string> NamedValues { get; set; }

        public ISet<IStrategy> Objects { get; set; }

        public IDictionary<IStrategy, AccessControl> AccessControlByStrategy { get; set; }
    }
}
