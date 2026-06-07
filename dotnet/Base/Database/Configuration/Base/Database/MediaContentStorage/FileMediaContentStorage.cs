// <copyright file="FileMediaContentStorage.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Domain;

    public class FileMediaContentStorage : IMediaContentStorage
    {
        private readonly DirectoryInfo directory;

        public FileMediaContentStorage(DirectoryInfo directory) => this.directory = directory;

        public bool Exists(long id) => File.Exists(this.PathFor(id));

        public byte[] Read(long id)
        {
            var path = this.PathFor(id);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public void Write(long id, byte[] data)
        {
            if (!this.directory.Exists)
            {
                this.directory.Create();
            }

            File.WriteAllBytes(this.PathFor(id), data ?? Array.Empty<byte>());
        }

        public void Delete(long id)
        {
            var path = this.PathFor(id);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public IEnumerable<long> Enumerate()
        {
            // Query the filesystem freshly; DirectoryInfo.Exists is cached and may be stale.
            if (!Directory.Exists(this.directory.FullName))
            {
                yield break;
            }

            foreach (var path in Directory.EnumerateFiles(this.directory.FullName))
            {
                // File names are object ids (see PathFor); ignore anything else in the directory.
                if (long.TryParse(Path.GetFileName(path), out var id))
                {
                    yield return id;
                }
            }
        }

        private string PathFor(long id) => Path.Combine(this.directory.FullName, id.ToString());
    }
}
