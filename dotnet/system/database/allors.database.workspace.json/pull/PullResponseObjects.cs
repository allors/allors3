// <copyright file="PullResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;
    using Meta;

    public class PullResponseObjects
    {
        private readonly IDictionary<IClass, ISet<IPropertyType>> dependencies;
        private readonly HashSet<IObject> objects;

        public PullResponseObjects(IDictionary<IClass, ISet<IPropertyType>> dependencies)
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
                if (this.dependencies != null && this.dependencies.TryGetValue(objectToAdd.Strategy.Class, out var propertyTypes))
                {
                    foreach (var propertyType in propertyTypes)
                    {
                        if (propertyType is IRoleType roleType)
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
                            var associationType = (IAssociationType)propertyType;
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
