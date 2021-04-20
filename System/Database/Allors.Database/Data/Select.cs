// <copyright file="Select.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System;
    using System.Linq;
    using Meta;
    using Security;

    public class Select : IVisitable
    {
        public Select()
        {
        }

        public Select(params IPropertyType[] propertyTypes)
        {
            if (propertyTypes.Length > 0)
            {
                this.Step = new Step(propertyTypes, 0);
            }
        }

        public Select(IMetaPopulation metaPopulation, params Guid[] propertyTypeIds)
            : this(propertyTypeIds.Select(v => (IPropertyType)metaPopulation.FindById(v)).ToArray())
        {
        }

        public Node[] Include { get; set; }

        public Step Step { get; set; }

        public object Get(IObject allorsObject, IAccessControlLists acls) => this.Step == null ? allorsObject : this.Step.Get(allorsObject, acls);

        public bool Set(IObject allorsObject, IAccessControlLists acls, object value) => this.Step != null && this.Step.Set(allorsObject, acls, value);

        public void Ensure(IObject allorsObject, IAccessControlLists acls) => this.Step.Ensure(allorsObject, acls);

        public static bool TryParse(IComposite composite, string selectString, out Select @select)
        {
            var propertyType = Resolve(composite, selectString);
            @select = propertyType == null ? null : new Select(propertyType);
            return @select != null;
        }
        
        private static IPropertyType Resolve(IComposite composite, string propertyName)
        {
            var lowerCasePropertyName = propertyName.ToLowerInvariant();

            foreach (var roleType in composite.DatabaseRoleTypes)
            {
                if (roleType.SingularName.ToLowerInvariant().Equals(lowerCasePropertyName) ||
                    roleType.SingularFullName.ToLowerInvariant().Equals(lowerCasePropertyName) ||
                    roleType.PluralName.ToLowerInvariant().Equals(lowerCasePropertyName) ||
                    roleType.PluralFullName.ToLowerInvariant().Equals(lowerCasePropertyName))
                {
                    return roleType;
                }
            }

            foreach (var associationType in composite.DatabaseAssociationTypes)
            {
                if (associationType.SingularName.ToLowerInvariant().Equals(lowerCasePropertyName) ||
                    associationType.SingularFullName.ToLowerInvariant().Equals(lowerCasePropertyName) ||
                    associationType.PluralName.ToLowerInvariant().Equals(lowerCasePropertyName) ||
                    associationType.PluralFullName.ToLowerInvariant().Equals(lowerCasePropertyName))
                {
                    return associationType;
                }
            }

            return null;
        }

        public override string ToString() => this.Step?.ToString() ?? base.ToString();

        public void Accept(IVisitor visitor) => visitor.VisitSelect(this);
    }
}
