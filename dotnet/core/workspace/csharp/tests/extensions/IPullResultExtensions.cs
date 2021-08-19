// <copyright file="Test.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using Allors.Workspace;

    public static class IPullResultExtensions
    {
        public static PullResultAssert Assert(this IPullResult @this) => new PullResultAssert(@this);
    }
}
