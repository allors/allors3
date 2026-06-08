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
        private const string DirectoryConfigurationKey = "Media:Directory";

        private readonly DirectoryInfo directory;

        // Optional service: returns null when Media:Directory is not configured. External (file-backed) media
        // storage is opt-in, so an embedded-only install never configures it and callers (e.g.
        // ExternalMediaContents.ReconcileFiles) simply skip a null storage.
        public static IMediaContentStorage CreateOrNull(IConfiguration configuration)
        {
            var configured = configuration?[DirectoryConfigurationKey];
            return string.IsNullOrWhiteSpace(configured) ? null : new FileMediaContentStorage(configuration);
        }

        // Constructed with a configured directory (normally via CreateOrNull). There is no fallback and the
        // directory is never created here: an unset/empty setting or a directory that does not exist is a
        // configuration error and fails hard. The directory must be provisioned before the service is used.
        public FileMediaContentStorage(IConfiguration configuration)
        {
            var configured = configuration?[DirectoryConfigurationKey];
            if (string.IsNullOrWhiteSpace(configured))
            {
                throw new InvalidOperationException(
                    $"The '{DirectoryConfigurationKey}' setting is not configured. Set it to the directory that backs external media content.");
            }

            this.directory = new DirectoryInfo(configured);
            if (!System.IO.Directory.Exists(this.directory.FullName))
            {
                throw new DirectoryNotFoundException(
                    $"The media directory '{this.directory.FullName}' (from the '{DirectoryConfigurationKey}' setting) does not exist. Create it or correct the setting.");
            }
        }

        // The resolved backing directory (read-only); exposed for diagnostics and tests.
        public DirectoryInfo Directory => this.directory;

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
            // No auto-create: the directory is a validated precondition (see constructor). Writing into a
            // directory removed at runtime throws DirectoryNotFoundException — fail hard.
            => File.WriteAllBytes(this.PathFor(id), data ?? Array.Empty<byte>());

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
            if (!System.IO.Directory.Exists(this.directory.FullName))
            {
                yield break;
            }

            foreach (var path in System.IO.Directory.EnumerateFiles(this.directory.FullName))
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
