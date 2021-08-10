// <copyright file="BarcodeTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.IO;
    using Xunit;

    public class BarcodeTest : DomainTest, IClassFixture<Fixture>
    {
        public BarcodeTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void Default()
        {
            var barcodeService = this.Session.Database.Services.Get<IBarcodeGenerator>();
            var image = barcodeService.Generate("Allors", BarcodeType.CODE_128);
            File.WriteAllBytes("barcode.png", image);
        }
    }
}
