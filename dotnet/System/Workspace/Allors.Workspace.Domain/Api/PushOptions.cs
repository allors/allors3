// <copyright file="PushOptions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using Data;

    public sealed class PushOptions
    {
        public bool Refresh { get; set; } = true;
        public Pull[] Discover { get; set; }
    }
}
