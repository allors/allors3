// <copyright file="ContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Text;
    using Xunit;

    public abstract class ContentTests : DomainTest, IClassFixture<Fixture>
    {
        protected ContentTests(Fixture fixture) : base(fixture) { }

        protected static byte[] GetByteArray() => GetByteArray("Some string");

        protected static byte[] GetByteArray(string v) => Encoding.UTF8.GetBytes(v);
    }
}
