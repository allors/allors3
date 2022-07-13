//// --------------------------------------------------------------------------------------------------------------------
//// <copyright file="ObjectsBase.cs" company="Allors bvba">
////   Copyright 2002-2012 Allors bvba.
//// 
//// Dual Licensed under
////   a) the General Public Licence v3 (GPL)
////   b) the Allors License
//// 
//// The GPL License is included in the file gpl.txt.
//// The Allors License is an addendum to your contract.
//// 
//// Allors Applications is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//// GNU General Public License for more details.
//// 
//// For more information visit http://www.allors.com/legal
//// </copyright>
//// --------------------------------------------------------------------------------------------------------------------

//using System;
//using System.Linq;

//namespace Allors.Integration.Extract
//{
//    using System.Collections.Generic;
//    using OfficeOpenXml;
//    using Allors.Integration.Source;
//    using Microsoft.Extensions.Logging;

//    public partial class CustomerExtractor
//    {
//        public CustomerExtractor(ExcelPackage customerList, ILoggerFactory loggerFactory)
//        {
//            this.CustomerList = customerList;
//            this.Logger = loggerFactory.CreateLogger<CustomerExtractor>();
//        }

//        public ExcelPackage CustomerList { get; }

//        public ILogger<CustomerExtractor> Logger { get; set; }

//        public Customer[] Execute()
//        {
//            var worksheet = this.CustomerList.Workbook.Worksheets["Sheet1"];
//            var dimension = worksheet.Dimension;
//            var col = dimension.Start.Column;
//            var skipColumns = 1;

//            var records = new List<Customer>();
//            for (var row = dimension.Start.Row + skipColumns; row <= dimension.End.Row; row++)
//            {
//                var yes = new string[] { "Y","y" };

//                var record = new Customer
//                {
//                    Bedrijfsnaam = worksheet.Cells[row, col].Text.Trim(),
//                    IsCustomer = yes.Contains(worksheet.Cells[row, col + 1].Text.Trim()),
//                    IsSupplier = yes.Contains(worksheet.Cells[row, col + 2].Text.Trim()),
//                    IsSupplierForAM = yes.Contains(worksheet.Cells[row, col + 3].Text.Trim()),
//                    IsSupplierForBvba = yes.Contains(worksheet.Cells[row, col + 4].Text.Trim()),
//                    IsSupplierForGR = yes.Contains(worksheet.Cells[row, col + 5].Text.Trim()),
//                    IsSupplierForEs = yes.Contains(worksheet.Cells[row, col + 6].Text.Trim()),
//                    IsSupplierForNl = yes.Contains(worksheet.Cells[row, col + 7].Text.Trim()),
//                    IsSupplierForRM = yes.Contains(worksheet.Cells[row, col + 8].Text.Trim()),
//                    Contact = worksheet.Cells[row, col + 9].Text.Trim(),
//                    Adres = worksheet.Cells[row, col + 10].Text.Trim(),
//                    Plaats = worksheet.Cells[row, col + 11].Text.Trim(),
//                    Land = worksheet.Cells[row, col + 12].Text.Trim(),
//                    Postcode = worksheet.Cells[row, col + 13].Text.Trim(),
//                    BtwNummerKant = worksheet.Cells[row, col + 14].Text.Trim(),
//                    Email = worksheet.Cells[row, col + 15].Text.Trim(),
//                };

//                records.Add(record);
//            }

//            var doubles = records.GroupBy(v => v.Bedrijfsnaam.ToUpperInvariant()).Where(v => v.Count() > 1).Select(v => v.Key).ToArray();
            
//            if (doubles.Length > 0)
//            {
//                foreach (var @double in doubles)
//                {
//                    this.Logger.LogError("Double: " + @double);
//                }

//                throw new Exception("Customer doubles");
//            }
            
//            return records.ToArray();
//        }
//    }
//}
