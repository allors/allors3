// <copyright file="AssemblyExtension.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;

    public static class AssemblyExtensions
    {
        public static string Fingerprint(this Assembly assembly)
        {
            using var md5 = MD5.Create();
            var assemblyBytes = File.ReadAllBytes(assembly.Location);
            var assemblyHash = md5.ComputeHash(assemblyBytes);
            return string.Concat(assemblyHash.Select(v => v.ToString("X2")));
        }
    }
}
