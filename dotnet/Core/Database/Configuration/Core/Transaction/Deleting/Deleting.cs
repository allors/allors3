// <copyright file="ICaches.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;

    public class Deleting : IDeleting
    {
        private readonly ISet<long> deleting;

        public Deleting() => this.deleting = new HashSet<long>();

        public void OnBeginDelete(long id) => this.deleting.Add(id);

        public void OnEndDelete(long id) => this.deleting.Remove(id);

        public bool IsDeleting(long id) => this.deleting.Contains(id);
    }
}
