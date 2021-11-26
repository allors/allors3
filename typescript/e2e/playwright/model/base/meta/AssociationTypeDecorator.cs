// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Workspace.Meta;

namespace Autotest
{
    public partial class AssociationTypeDecorator
    {
        private readonly IAssociationType subject;

        public AssociationTypeDecorator(IAssociationType subject)
        {
            this.subject = subject;
        }

        public ObjectTypeDecorator ObjectType => new ObjectTypeDecorator(subject.ObjectType);
    }
}
