// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Workspace.Meta;

namespace Autotest
{
    public partial class RoleTypeDecorator
    {
        private readonly IRoleType subject;

        public RoleTypeDecorator(IRoleType subject)
        {
            this.subject = subject;
        }

        public string Name => this.subject.Name;

        public string SingularName => this.subject.SingularName;

        public AssociationTypeDecorator AssociationType => new AssociationTypeDecorator(this.subject.AssociationType);
    }
}
