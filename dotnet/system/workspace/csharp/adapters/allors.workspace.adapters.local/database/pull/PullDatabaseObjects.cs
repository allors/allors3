// <copyright file="PullResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using Database.Meta;
    using Database.Services;

    using IObject = Database.IObject;

    public class PullDatabaseObjects
    {
        private readonly IDependencies dependencies;
        private readonly HashSet<IObject> objects;

        public PullDatabaseObjects(IDependencies dependencies)
        {
            this.dependencies = dependencies;
            this.objects = new HashSet<IObject>();
        }

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
                if (this.dependencies != null)
                {
                    foreach (var dependency in this.dependencies.GetDependencies(objectToAdd.Strategy.Class))
                    {
                        if (dependency is IRoleType roleType)
                        {
                            if (roleType.IsOne)
                            {
                                this.Add(objectToAdd.Strategy.GetCompositeRole(roleType));
                            }
                            else
                            {
                                this.Add(objectToAdd.Strategy.GetCompositesRole<IObject>(roleType));
                            }
                        }
                        else
                        {
                            var associationType = (IAssociationType)dependency;
                            if (associationType.IsOne)
                            {
                                this.Add(objectToAdd.Strategy.GetCompositeAssociation(associationType));
                            }
                            else
                            {
                                this.Add(objectToAdd.Strategy.GetCompositesAssociation<IObject>(associationType));
                            }
                        }
                    }
                }
            }
        }
    }
}
