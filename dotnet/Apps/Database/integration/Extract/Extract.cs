// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectsBase.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Integration.Extract
{
    using Microsoft.Extensions.Logging;
    using OfficeOpenXml;

    public partial class Extract
    {
        public Extract(ExcelPackage equipmentStockList, ExcelPackage customerList, ExcelPackage partsList, ExcelPackage partCategoryList, ILoggerFactory loggerFactory)
        {
            this.PartsList = partsList;
            this.PartCategoryList = partCategoryList;
            this.EquipmentStockList = equipmentStockList;
            this.CustomerList = customerList;
            this.LoggerFactory = loggerFactory;
            this.Logger = loggerFactory.CreateLogger<Extract>();
        }

        public ExcelPackage EquipmentStockList { get; }

        public ExcelPackage CustomerList { get; }

        public ExcelPackage PartsList { get; }

        public ExcelPackage PartCategoryList { get; }

        public ILoggerFactory LoggerFactory { get; }

        public ILogger<Extract> Logger { get; set; }

        public Source.Source Execute()
        {
            //var customerExtractor = new CustomerExtractor(this.CustomerList, this.LoggerFactory);

            return new Source.Source
            {
                //Customers = customerExtractor.Execute(),
            };
        }
    }
}
