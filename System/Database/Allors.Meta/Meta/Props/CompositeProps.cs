// <copyright file="MethodClassProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MethodClass type.</summary>

namespace Allors.Database.Meta
{
    using System.Collections.Generic;

    public abstract partial class CompositeProps : ObjectTypeProps
    {
        public IEnumerable<IInterface> Supertypes => this.AsComposite.Supertypes;

        public IEnumerable<IClass> Classes => this.AsComposite.Classes;

        public bool ExistExclusiveClass => this.AsComposite.ExistExclusiveClass;

        public IEnumerable<IAssociationType> DatabaseAssociationTypes => this.AsComposite.DatabaseAssociationTypes;

        public IEnumerable<IRoleType> DatabaseRoleTypes => this.AsComposite.DatabaseRoleTypes;

        public IEnumerable<IMethodType> MethodTypes => this.AsComposite.MethodTypes;

        public bool ExistDatabaseClass => this.AsComposite.ExistDatabaseClass;

        public IEnumerable<IClass> DatabaseClasses => this.AsComposite.DatabaseClasses;

        public bool ExistExclusiveDatabaseClass => this.AsComposite.ExistExclusiveDatabaseClass;

        public IClass ExclusiveDatabaseClass => this.AsComposite.ExclusiveDatabaseClass;

        public bool IsSynced => this.AsComposite.IsSynced;

        protected abstract ICompositeBase AsComposite { get; }
    }
}
