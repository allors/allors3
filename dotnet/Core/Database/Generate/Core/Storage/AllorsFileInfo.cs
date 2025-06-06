// <copyright file="AllorsFileInfo.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Meta.Generation.Storage
{
    using System.IO;

    public class AllorsFileInfo
    {
        private readonly FileInfo fileInfo;

        public AllorsFileInfo(FileInfo fileInfo) => this.fileInfo = fileInfo;

        public string GetRelativeName(DirectoryInfo baseDirectoryInfo)
        {
            var baseDirectory = new AllorsDirectoryInfo(baseDirectoryInfo);
            var directory = new AllorsDirectoryInfo(this.fileInfo.Directory);

            var relativePath = directory.GetRelativeName(baseDirectory);
            return relativePath == null ? null : Path.Combine(relativePath, this.fileInfo.Name);
        }

        public string GetRelativeOrFullName(DirectoryInfo baseDirectoryInfo)
        {
            var relativeName = this.GetRelativeName(baseDirectoryInfo);
            return relativeName ?? this.fileInfo.FullName;
        }
    }
}
