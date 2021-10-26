// <copyright file="PullResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;

    using IObject = Database.IObject;

    public class PullDatabaseObjects
    {
        private readonly HashSet<IObject> objects;

        public PullDatabaseObjects() => this.objects = new HashSet<IObject>();

        public IEnumerable<IObject> Objects => this.objects;

        internal void Add(IEnumerable<IObject> objectsToAdd)
        {
            foreach (var @object in objectsToAdd)
            {
                this.Add(@object);
            }
        }

        public void Add(IObject objectToAdd)
        {
            if (objectToAdd == null)
            {
                return;
            }

            if (!this.objects.Contains(objectToAdd))
            {
                this.objects.Add(objectToAdd);
            }
        }
    }
}
