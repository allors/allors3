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
    using Microsoft.Extensions.Configuration;

    public class FileMediaContentStorage : IMediaContentStorage
    {
        // Configuration key for the directory that backs file media content (a media subfolder of the data path).
        public const string DirectoryConfigurationKey = "Media:Directory";

        private readonly DirectoryInfo directory;

        public FileMediaContentStorage(DirectoryInfo directory) => this.directory = directory;

        // Single resolution point shared by every host (server and commands) so they never diverge.
        // An empty/whitespace value (e.g. an unset "Media__Directory=" env override) falls back to "media"
        // rather than throwing on new DirectoryInfo("").
        public static DirectoryInfo ResolveDirectory(IConfiguration configuration)
        {
            var configured = configuration?[DirectoryConfigurationKey];
            return new DirectoryInfo(string.IsNullOrWhiteSpace(configured) ? "media" : configured);
        }

        public bool Exists(long id) => File.Exists(this.PathFor(id));

        public long Length(long id)
        {
            var path = this.PathFor(id);
            return File.Exists(path) ? new FileInfo(path).Length : -1;
        }

        public byte[] Read(long id)
        {
            var path = this.PathFor(id);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public void Write(long id, byte[] data)
        {
            // CreateDirectory is idempotent and queries the live filesystem; DirectoryInfo.Exists is cached at
            // first access and would go stale for this process-lifetime singleton.
            Directory.CreateDirectory(this.directory.FullName);

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
