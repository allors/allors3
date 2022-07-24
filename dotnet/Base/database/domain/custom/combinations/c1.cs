// <copyright file="C1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class C1
    {
        public void CustomSum(C1Sum method) => method.Result = method.A + method.B;

        public override string ToString() => this.Name;
    }
}
