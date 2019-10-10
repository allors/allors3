// <copyright file="SessionObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System.Collections.Generic;
    using System.Linq;
    using Protocol;
    using Server;

    public class PushResponseContext
    {
        public PushResponseContext(Workspace workspace)
        {
            this.Workspace = workspace;
            this.Decompressor = new Decompressor();
            this.MetaObjectDecompressor = new MetaObjectDecompressor(this.Decompressor, this.Workspace);
        }

        public Workspace Workspace { get; }

        internal Decompressor Decompressor { get; set; }

        internal MetaObjectDecompressor MetaObjectDecompressor { get; }
    }
}