// <copyright file="MethodClassProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MethodClass type.</summary>

namespace Allors.Database.Meta
{
    using System;

    public abstract partial class MethodTypeProps : OperandTypeProps, IMetaIdentifiableObjectProps
    {
        public IComposite ObjectType => ((IMethodType)this.AsMethodType).ObjectType;

        public Guid Id => this.AsMethodType.Id;

        public string IdAsString => this.AsMethodType.IdAsString;

        public string Name => this.AsMethodType.Name;

        public string FullName => this.AsMethodType.FullName;

        public string[] WorkspaceNames => this.AsMethodType.WorkspaceNames;

        #region As
        protected abstract IMethodTypeBase AsMethodType { get; }
        #endregion
    }
}
