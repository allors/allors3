// <copyright file="PullRequest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Direct.Api.Pull
{
    using Workspace.Data;

    public class PullRequest
    {
        public Pull[] Pulls { get; set; }
    }
}
