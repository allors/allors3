// <copyright file="DomainProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RelationType type.</summary>

namespace Allors.Database.Meta
{
    using System;

    public sealed partial class DomainProps : MetaObjectProps, IMetaIdentifiableObjectProps
    {
        private readonly IDomainBase inheritance;

        internal DomainProps(IDomainBase @class) => this.inheritance = @class;

        public Guid Id => this.inheritance.Id;

        public string IdAsString => this.inheritance.IdAsString;

        #region As
        protected override IMetaObjectBase AsMetaObject => this.inheritance;
        #endregion
    }
}
