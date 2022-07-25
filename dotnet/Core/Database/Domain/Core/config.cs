// <copyright file="Config.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.IO;

    public class Config
    {
        public DirectoryInfo DataPath { get; set; }

        public bool SetupSecurity { get; set; } = true;

        public bool SetupAccounting { get; set; } = true;
    }
}
