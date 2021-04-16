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
    public sealed class RelationType : IRelationType
    {
        internal Origin Origin { get; private set; }
        internal Guid Id { get; private set; }
        internal Multiplicity Multiplicity { get; private set; }

        private string IdAsString { get; set; }
        private IAssociationTypeInternals AssociationType { get; set; }
        private IRoleTypeInternals RoleType { get; set; }

        private bool IsDerived { get; set; }
        private bool IsSynced { get; set; }
        
        #region IMetaObject

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

        public override string ToString() => $"{this.AssociationType.ObjectType.SingularName}{this.RoleType.Name}";

        public void Init(Guid id, IAssociationTypeInternals associationType, IRoleTypeInternals roleType, Origin origin = Origin.Database) => this.Init(id, associationType, roleType, Multiplicity.ManyToOne, origin);

        public void Init(Guid id, IAssociationTypeInternals associationType, IRoleTypeInternals roleType, Multiplicity multiplicity = Multiplicity.ManyToOne, Origin origin = Origin.Database)
        {
            this.Id = id;
            this.IdAsString = id.ToString("D");
            this.AssociationType = associationType;
            this.RoleType = roleType;
            this.Origin = origin;

            this.Multiplicity = this.RoleType.ObjectType.IsUnit ? Multiplicity.OneToOne : multiplicity;
        }
    }
}
