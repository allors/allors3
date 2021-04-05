// <copyright file="MethodInterfaceProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MethodInterface type.</summary>

namespace Allors.Database.Meta
{
    using System.Collections.Generic;

    public sealed partial class MethodInterfaceProps : MethodTypeProps
    {
        private readonly IMethodInterfaceBase methodInterface;

        internal MethodInterfaceProps(IMethodInterfaceBase relationInterface) => this.methodInterface = relationInterface;
        
        #region As
        protected override IMetaObjectBase AsMetaObject => this.methodInterface;

        protected override IOperandTypeBase AsOperandType => this.methodInterface;

        protected override IMethodTypeBase AsMethodType => this.methodInterface;
        #endregion
    }
}
