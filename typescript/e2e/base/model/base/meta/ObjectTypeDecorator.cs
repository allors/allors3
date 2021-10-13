// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Allors.Workspace.Meta;

namespace Autotest
{
    public partial class ObjectTypeDecorator
    {
        private readonly IObjectType subject;

        public ObjectTypeDecorator(IObjectType subject)
        {
            this.subject = subject;
        }

        public IEnumerable<ObjectTypeDecorator> Classes => (this.subject as IComposite)?.Classes.Select(v => new ObjectTypeDecorator(v));

        public string Tag => this.subject.Tag;

        public string SingularName => this.subject.SingularName;
    }
}
