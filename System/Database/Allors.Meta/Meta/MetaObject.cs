// <copyright file="MetaObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Meta
{
    /// <summary>
    /// Base class for Meta objects.
    /// </summary>
    public abstract partial class MetaObjectBase : IMetaObject
    {
        protected MetaObjectBase(MetaPopulation metaPopulation) => this.MetaPopulation = metaPopulation;

        IMetaPopulation IMetaObject.MetaPopulation => this.MetaPopulation;

        public MetaPopulation MetaPopulation { get; private set; }

        public abstract Origin Origin { get; }

        public int OriginAsInt => (int)this.Origin;

        public bool HasDatabaseOrigin => this.Origin == Origin.Database;

        public bool HasWorkspaceOrigin => this.Origin == Origin.Workspace;

        public bool HasSessionOrigin => this.Origin == Origin.Session;

        /// <summary>
        /// Gets the validation name.
        /// </summary>
        public abstract string ValidationName { get; }
    }
}
