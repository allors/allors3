// <copyright file="FileReader.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.IO;

    public static class FileInfoExtensions
    {
        public static Media CreateMedia(this FileInfo fileInfo, ITransaction transaction)
        {
            fileInfo.Refresh();
            var content = File.ReadAllBytes(fileInfo.FullName);
            return new MediaBuilder(transaction).WithInFileName(fileInfo.FullName).WithInData(content).Build();
        }
    }
}
