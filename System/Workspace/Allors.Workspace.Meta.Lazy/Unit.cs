// <copyright file="Unit.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class Unit : IUnitInternals
    {
        public MetaPopulation MetaPopulation { get; set; }

        private Guid Id { get; set; }

        private string IdAsString { get; set; }

        private UnitTags UnitTag { get; set; }

        private string SingularName { get; set; }

        private string PluralName { get; set; }

        private Type ClrType { get; set; }

        #region IComparable
        int IComparable<IObjectType>.CompareTo(IObjectType other) => string.Compare(this.SingularName, other.SingularName, StringComparison.InvariantCulture);
        #endregion

        #region IMetaObject

        IMetaPopulation IMetaObject.MetaPopulation => this.MetaPopulation;

        Origin IMetaObject.Origin => Origin.Database;

        bool IMetaObject.HasDatabaseOrigin => true;

        bool IMetaObject.HasWorkspaceOrigin => false;

        bool IMetaObject.HasSessionOrigin => false;
        #endregion

        #region IMetaIdentifiableObject
        Guid IMetaObject.Id => this.Id;

        string IMetaObject.IdAsString => this.IdAsString;
        #endregion

        #region IObjectType
        bool IObjectType.IsUnit => true;

        bool IObjectType.IsComposite => false;

        bool IObjectType.IsInterface => false;

        bool IObjectType.IsClass => false;

        string IObjectType.SingularName => this.SingularName;

        string IObjectType.PluralName => this.PluralName;

        Type IObjectType.ClrType => this.ClrType;
        #endregion

        #region IUnit
        UnitTags IUnit.UnitTag => this.UnitTag;

        bool IUnit.IsBinary => this.UnitTag == UnitTags.Binary;

        bool IUnit.IsBoolean => this.UnitTag == UnitTags.Boolean;

        bool IUnit.IsDateTime => this.UnitTag == UnitTags.DateTime;

        bool IUnit.IsDecimal => this.UnitTag == UnitTags.Decimal;

        bool IUnit.IsFloat => this.UnitTag == UnitTags.Float;

        bool IUnit.IsInteger => this.UnitTag == UnitTags.Integer;

        bool IUnit.IsString => this.UnitTag == UnitTags.String;

        bool IUnit.IsUnique => this.UnitTag == UnitTags.Unique;
        #endregion

        public Unit Init(Guid id, UnitTags tag, string singularName)
        {
            this.Id = id;
            this.IdAsString = id.ToString("D");
            this.UnitTag = tag;
            this.SingularName = singularName;
            this.PluralName = Pluralizer.Pluralize(singularName);

            return this;
        }

        void IUnitInternals.Bind()
        {
            switch (this.UnitTag)
            {
                case UnitTags.Binary:
                    this.ClrType = typeof(byte[]);
                    break;

                case UnitTags.Boolean:
                    this.ClrType = typeof(bool);
                    break;

                case UnitTags.DateTime:
                    this.ClrType = typeof(DateTime);
                    break;

                case UnitTags.Decimal:
                    this.ClrType = typeof(decimal);
                    break;

                case UnitTags.Float:
                    this.ClrType = typeof(double);
                    break;

                case UnitTags.Integer:
                    this.ClrType = typeof(int);
                    break;

                case UnitTags.String:
                    this.ClrType = typeof(string);
                    break;

                case UnitTags.Unique:
                    this.ClrType = typeof(Guid);
                    break;
            }
        }
    }
}
