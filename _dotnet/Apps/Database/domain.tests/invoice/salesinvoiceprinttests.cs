// <copyright file="SalesInvoicePrintTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class SalesInvoicePrintTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoicePrintTests(Fixture fixture) : base(fixture) { }

        //[Fact]
        //public void GivenSalesInvoice_WhenCreatingPrintModel_ThenPrintModelIsNotNull()
        //{
        //    // Arrange
        //    var invoice = new SalesInvoices(this.Transaction).Extent().FirstOrDefault();

        //    // Act
        //    var printModel = new Print.SalesInvoiceModel.Model(invoice);

        //    // Assert
        //    Assert.NotNull(printModel);
        //}

        //[Fact]
        //public void GivenSalesInvoice_WhenDeriving_ThenPrintDocumentWithoutMediaCreated()
        //{
        //    // Arrange
        //    var invoice = new SalesInvoices(this.Transaction).Extent().FirstOrDefault();

        //    // Act

        //    // Assert
        //    Assert.True(invoice.ExistPrintDocument);
        //    Assert.False(invoice.PrintDocument.ExistMedia);
        //}

        //[Fact]
        //public void GivenSalesInvoicePrintDocument_WhenPrinting_ThenMediaCreated()
        //{
        //    // Arrange
        //    var invoice = new SalesInvoices(this.Transaction).Extent().FirstOrDefault();

        //    // Act
        //    invoice.Print();

        //    this.Transaction.Derive();
        //    this.Transaction.Commit();

        //    // Assert
        //    Assert.True(invoice.PrintDocument.ExistMedia);

        //    var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //    var outputFile = System.IO.File.Create(System.IO.Path.Combine(desktopDir, "salesInvoice.odt"));
        //    var stream = new System.IO.MemoryStream(invoice.PrintDocument.Media.MediaContent.Data);

        //    stream.CopyTo(outputFile);
        //    stream.Close();
        //}
    }
}
