// <copyright file="Select.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System.Text;
    using Meta;

    public class Select : IVisitable
    {
        public Select()
        {
        }

        public Select(params IPropertyType[] propertyTypes) : this(propertyTypes, 0)
        {
        }

        private Select(IPropertyType[] propertyTypes, int index)
        {
            if (propertyTypes?.Length > 0)
            {
                this.PropertyType = propertyTypes[index];

                var nextIndex = index + 1;
                if (nextIndex < propertyTypes.Length)
                {
                    this.Next = new Select(propertyTypes, nextIndex);
                }
            }
        }

        public Node[] Include { get; set; }

        public IPropertyType PropertyType { get; set; }

        public IComposite OfType { get; set; }

        public Select Next { get; set; }

        public bool IsOne
        {
            get
            {
                if (this.PropertyType.IsMany)
                {
                    return false;
                }

                return this.ExistNext ? this.Next.IsOne : this.PropertyType.IsOne;
            }
        }

        public bool ExistNext => this.Next != null;

        public Select End => this.ExistNext ? this.Next.End : this;

        public void Accept(IVisitor visitor) => visitor.VisitSelect(this);

        public IObjectType GetObjectType()
        {
            if (this.ExistNext)
            {
                return this.Next.GetObjectType();
            }

            return this.PropertyType?.ObjectType;
        }

        public override string ToString()
        {
            var name = new StringBuilder();
            name.Append(this.PropertyType.Name);
            if (this.ExistNext)
            {
                this.Next.ToStringAppendToName(name);
            }

            return name.ToString();
        }

        private void ToStringAppendToName(StringBuilder name)
        {
            name.Append("." + this.PropertyType.Name);

            if (this.ExistNext)
            {
                this.Next.ToStringAppendToName(name);
            }
        }

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
    }
}
