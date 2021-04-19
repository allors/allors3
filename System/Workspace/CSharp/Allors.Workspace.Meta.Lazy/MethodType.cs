// <copyright file="MethodType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Linq;

    public sealed class MethodType : IMethodTypeInternals
    {
        public MetaPopulation MetaPopulation { get; set; }

        private Guid Id { get; set; }
        private string IdAsString { get; set; }
        private ICompositeInternals ObjectType { get; set; }
        private string Name { get; set; }

        #region IMetaObject
        IMetaPopulation IMetaObject.MetaPopulation => this.MetaPopulation;

        Guid IMetaObject.Id => this.Id;

        string IMetaObject.IdAsString => this.IdAsString;

        Origin IMetaObject.Origin => Origin.Database;

        bool IMetaObject.HasDatabaseOrigin => true;

        bool IMetaObject.HasWorkspaceOrigin => false;

        bool IMetaObject.HasSessionOrigin => false;
        #endregion

        #region IOperandType
        Guid IOperandType.OperandId => this.Id;
        #endregion

        #region IMethodType
        IComposite IMethodType.ObjectType => this.ObjectType;
        #endregion

        public override string ToString() => this.Name;

        public MethodType Init(Guid id, ICompositeInternals objectType, string name)
        {
            this.Id = id;
            this.IdAsString = id.ToString("D");
            this.ObjectType = objectType;
            this.Name = name;

            return this;
        }
    }
}
