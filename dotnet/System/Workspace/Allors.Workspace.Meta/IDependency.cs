// <copyright file="IClass.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Workspace.Meta
{
    public interface IDependency
    {
        IComposite ObjectType { get; }

        IPropertyType PropertyType { get; }
    }

    public class Dependency : IDependency
    {
        public Dependency(IComposite objectType, IPropertyType propertyType)
        {
            this.ObjectType = objectType;
            this.PropertyType = propertyType;
        }

        public IComposite ObjectType { get; }

        public IPropertyType PropertyType { get; }
    }
}
