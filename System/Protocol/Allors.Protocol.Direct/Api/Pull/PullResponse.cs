// <copyright file="PullResponse.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Direct.Api.Pull
{
    using System.Collections.Generic;

    public class PullResponse
    {
        public AccessControl[] AccessControls { get; set; }

        public Dictionary<string, Object[]> NamedCollections { get; set; }

        public Dictionary<string, Object> NamedObjects { get; set; }

        public Dictionary<string, string> NamedValues { get; set; }

        public Object[] Objects { get; set; }
    }
}
