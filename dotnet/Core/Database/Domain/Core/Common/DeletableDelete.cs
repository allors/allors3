// <copyright file="DeletableDelete.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;

    public partial class DeletableDelete
    {
        public ISet<Deletable> Deleting { get; set; }

        public override void Execute()
        {
            this.Deleting ??= new HashSet<Deletable>();
            this.Deleting.Add((Deletable)this.Object);
            base.Execute();

            this.Object.Strategy.Delete();
        }
    }
}
