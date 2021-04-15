// <copyright file="Method.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Meta
{
    public partial class Method
    {
        protected Method(IObject @object, MethodType methodType)
        {
            this.Object = @object;
            this.MethodType = methodType;
        }

        public IObject Object { get; }

        public MethodType MethodType { get; }
    }
}
