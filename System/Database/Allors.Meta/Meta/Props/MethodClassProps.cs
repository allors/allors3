// <copyright file="MethodClassProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MethodClass type.</summary>

namespace Allors.Database.Meta
{
    using System;
    using System.Collections.Generic;

    public sealed partial class MethodClassProps : MethodTypeProps
    {
        private readonly IMethodClassBase methodClass;

        internal MethodClassProps(IMethodClassBase relationClass) => this.methodClass = relationClass;

        public IMethodInterface MethodInterface => this.methodClass.MethodInterface;

        private IEnumerable<Action<object, object>> Actions => this.methodClass.Actions;

        #region As
        protected override IMetaObjectBase AsMetaObject => this.methodClass;

        protected override IOperandTypeBase AsOperandType => this.methodClass;

        protected override IMethodTypeBase AsMethodType => this.methodClass;
        #endregion
    }
}