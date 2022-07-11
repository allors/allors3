// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Integration.cs" company="Allors bvba">
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

using System;

namespace Allors.Integration
{
    using System.IO;
    using System.Linq;
    using Allors.Database;
    using Allors.Database.Domain;
    using Microsoft.Extensions.Logging;
    using OfficeOpenXml;

    public partial class Integration
    {
        public Integration(IDatabase databaseServiceDatabase, DirectoryInfo dataPath, ILoggerFactory loggerFactory)
        {
            this.DataPath = dataPath;
            this.LoggerFactory = loggerFactory;
            this.Database = databaseServiceDatabase;
            this.Logger = loggerFactory.CreateLogger<Integration>();
        }

        public IDatabase Database { get; }

        public DirectoryInfo DataPath { get; }

        public ILoggerFactory LoggerFactory { get; }

        public ILogger<Integration> Logger { get; }

        public void Integrate()
        {
            // Extract
            Source.Source source;
            using (var equipmentStockList = new ExcelPackage(this.GetFile("Equipment Stock List.xlsx")))
            using (var customerList = new ExcelPackage(this.GetFile("AVIACO_Customers_new.xlsx")))
            using (var partsList = new ExcelPackage(this.GetFile("FICHERO RECAMBIO.xlsx")))
            using (var partCategoryList = new ExcelPackage(this.GetFile("SPARE PARTS - ITEM CATEGORIES DESCRIPTION.xlsx")))
            {
                var extraction = new Extract.Extract(equipmentStockList, customerList, partsList, partCategoryList, this.LoggerFactory);
                source = extraction.Execute();
            }

            using (var transaction = this.Database.CreateTransaction())
            {
                //transaction.SetUser(new AutomatedAgents(transaction).System);

                var population = new Population { Transaction = transaction };

                // Transform
                var transform = new Transform.Transform(source, population, this.LoggerFactory);
                var staging = transform.Execute();

                // Load
                var load = new Load.Load(staging, population, this.LoggerFactory, this.DataPath);
                load.Execute();

                this.Logger.LogInformation("Start Derive");

                transaction.Derive();

                this.Logger.LogInformation("End Derivation");

                transaction.Commit();

                var invalidFileNameChars = Path.GetInvalidFileNameChars();
                var directory = new DirectoryInfo(Path.Combine(this.DataPath.FullName, "images/employees"));

                //foreach (Employment employment in new Employments(transaction).Extent())
                //{
                //    if (!employment.Employee.ExistPicture)
                //    {
                //        var fileName = string.Join("_", employment.Employee.PartyName.Split(invalidFileNameChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
                //        var fileInfo = directory.GetFiles(fileName + "*.*").FirstOrDefault();
                //        if (fileInfo != null)
                //        {
                //            var content = File.ReadAllBytes(fileInfo.FullName);
                //            var image = new MediaBuilder(transaction).WithInFileName(fileInfo.Name).WithInData(content).Build();
                //            employment.Employee.Picture = image;
                //        }
                //    }
                //}

                transaction.Derive();
                transaction.Commit();

                var productImagesRootPath = new DirectoryInfo(Path.Combine(this.DataPath.FullName, "images/products"));

                foreach (UnifiedGood unifiedGood in new UnifiedGoods(transaction).Extent())
                {
                    var folderName = unifiedGood.Name
                       .Replace("-", "")
                       .Replace(" ", "")
                       .Replace("/", "")
                       .Replace(".", "")
                       .Replace("(", "")
                       .Replace(")", "")
                       .Replace("---", "")
                       .Replace("--", "")
                       .Replace(",", "");

                    if (!unifiedGood.ExistPhotos)
                    {
                        var productImagePath = Path.Combine(productImagesRootPath.FullName, folderName);
                        var imageDirectoryInfo = new DirectoryInfo(productImagePath);

                        if (imageDirectoryInfo.Exists)
                        {
                            var imageFiles = Directory.EnumerateFiles(productImagePath, "*.*")
                                .Where(s => s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".png") ||
                                            s.EndsWith(".JPG") || s.EndsWith(".JPEG") || s.EndsWith(".PNG"));

                            foreach (var fullPathFileName in imageFiles)
                            {
                                var fileInfo = new FileInfo(fullPathFileName);
                                var content = File.ReadAllBytes(fileInfo.FullName);

                                var fileName = fileInfo.Name;
                                var image = new MediaBuilder(transaction).WithInFileName(fileName).WithInData(content).Build();

                                if (fileName.Contains("_0.") || fileName.Contains("-0.") || fileName.StartsWith("0."))
                                {
                                    unifiedGood.PrimaryPhoto = image;
                                }
                                else 
                                {
                                    unifiedGood.AddPhoto(image);
                                }
                            }
                        }

                        transaction.Derive();
                        transaction.Commit();

                        if (!unifiedGood.ExistPrimaryPhoto)
                        {
                            var photo = unifiedGood.SerialisedItems.FirstOrDefault(v => v.ExistPrimaryPhoto)?.PrimaryPhoto;

                            if (photo != null)
                            {
                                var copy = new MediaBuilder(transaction).WithInFileName(photo.FileName).WithInData(photo.MediaContent.Data).Build();
                                unifiedGood.PrimaryPhoto = copy;
                                transaction.Derive();
                            }
                        }
                    }

                    if (unifiedGood.ExistPrimaryPhoto)
                    {
                        foreach (var item in unifiedGood.SerialisedItems.Where(v => !v.ExistPrimaryPhoto))
                        {
                            var photo = unifiedGood.PrimaryPhoto;
                            var copy = new MediaBuilder(transaction).WithInFileName(photo.FileName).WithInData(photo.MediaContent.Data).Build();

                            item.PrimaryPhoto = copy;
                        }
                    }

                    if (!unifiedGood.ExistPrimaryPhoto)
                    {
                        Console.WriteLine("Missing product image " + folderName);
                    }
                    //                    Console.Write(unifiedGood.ExistPrimaryPhoto? "+" : "-");
                }

                transaction.Derive();
                transaction.Commit();
            }
        }

        private FileInfo GetFile(params string[] fileName)
        {
            var paths = fileName.Prepend(this.DataPath.FullName).ToArray();
            var combine = Path.Combine(paths);
            return new FileInfo(combine);
        }
    }
}
