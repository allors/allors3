// <copyright file="ICaches.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Collections;

    public class Deleting : IDeleting
    {
        private readonly ISet<Deletable> deleting;

        public Deleting() => this.deleting = new HashSet<Deletable>();

        public void OnBeginDelete(Deletable deletable) => this.deleting.Add(deletable);

        public void OnEndDelete(Deletable deletable) => this.deleting.Remove(deletable);

        public bool IsDeleting(Deletable deletable) => this.deleting.Contains(deletable);
    }
}
