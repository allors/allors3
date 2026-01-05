// <copyright file="Test.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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
