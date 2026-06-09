// <copyright file="BarcodeTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class BarcodeTest : DomainTest, IClassFixture<Fixture>
    {
        public BarcodeTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void Default()
        {
            var barcodeService = this.Transaction.Database.Services.Get<IBarcodeGenerator>();

            var image = barcodeService.Generate("Allors", BarcodeType.CODE_128);

            Assert.NotNull(image);
            Assert.NotEmpty(image);

            // The generator encodes to PNG (SKEncodedImageFormat.Png); verify the PNG file signature.
            var pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            Assert.True(image.Length >= pngSignature.Length);
            Assert.Equal(pngSignature, image[..pngSignature.Length]);
        }
    }
}
