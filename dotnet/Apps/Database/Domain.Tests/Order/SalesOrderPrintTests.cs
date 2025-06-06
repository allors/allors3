// <copyright file="SalesOrderPrintTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class SalesOrderPrintTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderPrintTests(Fixture fixture) : base(fixture)
        {

        }

        //[Fact]
        //public void GivenSalesOrder_WhenCreatingPrintModel_ThenPrintModelIsNotNull()
        //{
        //    // Arrange
        //    var order = new SalesOrders(this.Transaction).Extent().FirstOrDefault();

        //    // Act
        //    var printModel = new Print.SalesOrderModel.Model(order);

        //    // Assert
        //    Assert.NotNull(printModel);
        //}

        //[Fact]
        //public void GivenSalesOrder_WhenDeriving_henPrintDocumentWithoutMediaCreated()
        //{
        //    // Arrange

        //    // Act
        //    this.Transaction.Derive(true);

        //    // Assert
        //    var order = new SalesOrders(this.Transaction).Extent().FirstOrDefault();

        //    Assert.True(order.ExistPrintDocument);
        //    Assert.False(order.PrintDocument.ExistMedia);
        //}

        //[Fact]
        //public void GivenSalesOrderPrintDocument_WhenPrinting_ThenMediaCreated()
        //{
        //    // Arrange
        //    var order = new SalesOrders(this.Transaction).Extent().FirstOrDefault();

        //    // Act
        //    order.Print();

        //    this.Transaction.Derive();
        //    this.Transaction.Commit();

        //    // Assert
        //    Assert.True(order.PrintDocument.ExistMedia);

        //    var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //    var outputFile = System.IO.File.Create(System.IO.Path.Combine(desktopDir, "salesOrder.odt"));
        //    var stream = new System.IO.MemoryStream(order.PrintDocument.Media.MediaContent.Data);

        //    stream.CopyTo(outputFile);
        //    stream.Close();
        //}
    }
}
