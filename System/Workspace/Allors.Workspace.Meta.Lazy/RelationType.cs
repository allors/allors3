// <copyright file="RelationType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RelationType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// A <see cref="RelationType"/> defines the state and behavior for
    /// a set of <see cref="AssociationType"/>s and <see cref="RoleType"/>s.
    /// </summary>
    public sealed class RelationType : IRelationTypeInternals
    {
        public RelationType(IAssociationTypeInternals associationType, IRoleTypeInternals roleType)
        {
            this.AssociationType = associationType;
            this.RoleType = roleType;
        }

        private IAssociationTypeInternals AssociationType { get; }
        private IRoleTypeInternals RoleType { get; }

        private Guid Id { get; set; }
        private string IdAsString { get; set; }
        private Multiplicity Multiplicity { get; set; }
        private Origin Origin { get; set; }
        private bool IsDerived { get; set; }
        private bool IsSynced { get; set; }

        #region IMetaObject

        public IMetaPopulation MetaPopulation { get; }

        Origin IMetaObject.Origin => this.Origin;

        bool IMetaObject.HasDatabaseOrigin => this.Origin == Origin.Database;

        bool IMetaObject.HasWorkspaceOrigin => this.Origin == Origin.Workspace;

        bool IMetaObject.HasSessionOrigin => this.Origin == Origin.Session;

        #endregion

        #region IMetaIdentifiableObject

        Guid IMetaObject.Id => this.Id;

        string IMetaObject.IdAsString => this.IdAsString;

        #endregion

        #region IRelationType

        IAssociationType IRelationType.AssociationType => this.AssociationType;

        IRoleType IRelationType.RoleType => this.RoleType;

        Multiplicity IRelationType.Multiplicity => this.Multiplicity;

        bool IRelationType.IsDerived => this.IsDerived;

        bool IRelationType.IsSynced => this.IsSynced;

        #endregion

        #region IRelationTypeInternals

        IAssociationTypeInternals IRelationTypeInternals.AssociationType => this.AssociationType;
        IRoleTypeInternals IRelationTypeInternals.RoleType => this.RoleType;
        #endregion

        public override string ToString() => $"{this.AssociationType.ObjectType.SingularName}{this.RoleType.Name}";

        public void Init(Guid id, Multiplicity multiplicity = Multiplicity.ManyToOne, Origin origin = Origin.Database, bool isDerived = false, bool isSynced = false)
        {
            this.Id = id;
            this.IdAsString = id.ToString("D");
            this.Multiplicity = this.RoleType.ObjectType.IsUnit ? Multiplicity.OneToOne : multiplicity;
            this.Origin = origin;
            this.IsDerived = isDerived;
            this.IsSynced = isSynced;
        }
    }
}
