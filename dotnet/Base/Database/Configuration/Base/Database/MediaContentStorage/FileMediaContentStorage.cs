// <copyright file="FileMediaContentStorage.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
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

        private string PathFor(long id) => Path.Combine(this.directory.FullName, id.ToString());
    }
}
