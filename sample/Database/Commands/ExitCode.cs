// <copyright file="ExitCode.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    // Core's commands save, load and index return these exit codes, so a tool that includes them defines them.
    public static class ExitCode
    {
        public static readonly int Success = 0;

        public static readonly int Error = 1;
    }
}
