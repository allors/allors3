// <copyright file="Upgrade.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.IO;

    public class Upgrade
    {
        private readonly ITransaction transaction;

        private readonly DirectoryInfo dataPath;

        public Upgrade(ITransaction transaction, DirectoryInfo dataPath)
        {
            this.transaction = transaction;
            this.dataPath = dataPath;
        }

        public void Execute()
        {
        }
    }
}
