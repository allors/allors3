// <copyright file="RoleType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RoleType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Linq;

    public abstract class RoleType : IRoleTypeInternals
    {
        public MetaPopulation MetaPopulation { get; set; }

        internal IRelationTypeInternals RelationType { get; set; }
        internal IAssociationTypeInternals AssociationType { get; set; }

        private IObjectType ObjectType { get; set; }
        private string SingularName { get; set; }
        private string PluralName { get; set; }
        private string Name { get; set; }
        private bool IsMany { get; set; }
        private bool IsOne { get; set; }
        private int? Size { get; set; }
        private int? Precision { get; set; }
        private int? Scale { get; set; }
        private bool IsRequired { get; set; }
        private bool IsUnique { get; set; }
        private string MediaType { get; set; }
        
        #region IComparable
        int IComparable<IPropertyType>.CompareTo(IPropertyType other) => string.Compare(this.Name, other.Name, StringComparison.InvariantCulture);
        #endregion

        #region IOperandType
        Guid IOperandType.OperandId => this.RelationType.Id;
        #endregion

        #region IPropertyType

        string IPropertyType.Name => this.Name;

        string IPropertyType.SingularName => this.SingularName;

        string IPropertyType.PluralName => this.PluralName;

        IObjectType IPropertyType.ObjectType => this.ObjectType;

        bool IPropertyType.IsOne => this.IsOne;

        bool IPropertyType.IsMany => this.IsMany;

        object IPropertyType.Get(IStrategy strategy, IComposite ofType)
        {
            if (this.IsOne)
            {
                var association = strategy.GetComposite<IObject>(this);

                if (ofType == null || association == null)
                {
                    return association;
                }

                return !ofType.IsAssignableFrom(((IObject)association).Strategy.Class) ? null : association;
            }
            else
            {
                var association = strategy.GetComposites<IObject>(this);

                if (ofType == null || association == null)
                {
                    return association;
                }

                return association.Where(v => ofType.IsAssignableFrom(v.Strategy.Class));
            }
        }
        #endregion

        #region IRoleType
        IAssociationType IRoleType.AssociationType => this.AssociationType;

        IRelationType IRoleType.RelationType => this.RelationType;

        int? IRoleType.Size => this.Size;

        int? IRoleType.Precision => this.Precision;

        int? IRoleType.Scale => this.Scale;

        bool IRoleType.IsRequired => this.IsRequired;

        bool IRoleType.IsUnique => this.IsUnique;

        string IRoleType.MediaType => this.MediaType;
        #endregion

        /// <summary>
        /// The maximum size value.
        /// </summary>
        public const int MaximumSize = -1;

        ///// <summary>
        ///// Instantiate the value of the role on this object.
        ///// </summary>
        ///// <param name="strategy">
        ///// The strategy.
        ///// </param>
        ///// <returns>
        ///// The role value.
        ///// </returns>
        public object Get(IStrategy strategy, IComposite ofType = null)
        {
            if (this.IsOne)
            {
                var association = strategy.GetComposite<IObject>(this);

                if (ofType == null || association == null)
                {
                    return association;
                }

                return !ofType.IsAssignableFrom(((IObject)association).Strategy.Class) ? null : association;
            }
            else
            {
                var association = strategy.GetComposites<IObject>(this);

                if (ofType == null || association == null)
                {
                    return association;
                }

                return association.Where(v => ofType.IsAssignableFrom(v.Strategy.Class));
            }
        }

        ///// <summary>
        ///// Set the value of the role on this object.
        ///// </summary>
        ///// <param name="strategy">
        ///// The strategy.
        ///// </param>
        ///// <param name="value">
        ///// The role value.
        ///// </param>
        public void Set(IStrategy strategy, object value) => strategy.Set(this, value);

        public override string ToString() => $"{this.AssociationType.ObjectType.SingularName}.{this.Name}";

        public void Init(IObjectType objectType, string singularName, string pluralName, int? size = null, int? precision = null, int? scale = null, bool isRequired = false, bool isUnique = false, string mediaType = null)
        {
            this.ObjectType = objectType;
            this.SingularName = singularName;
            this.PluralName = pluralName ?? Pluralizer.Pluralize(singularName);

            this.IsMany = this.RelationType.Multiplicity == Multiplicity.OneToMany ||
                          this.RelationType.Multiplicity == Multiplicity.ManyToMany;
            this.IsOne = !this.IsMany;
            this.Name = this.IsMany ? this.PluralName : this.SingularName;

            if (this.ObjectType is IUnit unitType)
            {
                switch (unitType.UnitTag)
                {
                    case UnitTags.String:
                        this.Size = size ?? 256;
                        break;

                    case UnitTags.Binary:
                        this.Size = size ?? MaximumSize;
                        break;

                    case UnitTags.Decimal:
                        this.Precision = precision ?? 19;
                        this.Scale = scale ?? 2;
                        break;
                }
            }

            this.IsRequired = isRequired;
            this.IsUnique = isUnique;
            this.MediaType = mediaType;
        }
    }
}
