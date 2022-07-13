// <copyright file="Journal.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Allors.Database.Domain
{
    public partial class RgsFilter
    {
        public void AppsOnInit(ObjectOnInit method)
        {
            this.ExcludeLevel5 = true;
            this.ExcludeLevel5Extension = true;
        }
    }
}
