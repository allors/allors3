namespace Integration.Tests.custom
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Integration.Source;
    using Allors.Integration.Transform;
    using CsvHelper;
    using CsvHelper.Configuration;
    using HtmlAgilityPack;
    using Integration.Extract;
    using Microsoft.Extensions.Logging.Abstractions;
    using NUnit.Framework;

    public class GeneralLedgerAccountTests : Test
    {

        [Test]
        public async System.Threading.Tasks.Task ValidGeneralLedgerAccountIntegrationTest()
        {
            var generalLedgerAccounts = new Allors.Database.Domain.GeneralLedgerAccounts(this.Transaction).Extent().ToArray();
            var generalLedgerAccountTypes = new GeneralLedgerAccountTypes(this.Transaction).Extent().ToArray();
            var generalLedgerAccountClassifications = new GeneralLedgerAccountClassifications(this.Transaction).Extent().ToArray();

            var generalLedgerAccountLevel2 = new Allors.Integration.Source.GeneralLedgerAccount()
            {
                ReferenceCode = "BIva",
                SortCode = "A",
                ReferenceNumber = "01",
                Name = "Immateriële vaste activa",
                Description = "Immateriële vaste activa",
                BalanceSide = "D",
                Level = 2,
                IsRgsBase = true,
                IsRgsExtended = true,
                IsRgsUseWithEZ = true,
                IsRgsUseWithZzp = true,
                IsRgsUseWithWoco = true,
            };

            var generalLedgerAccountLevel3 = new Allors.Integration.Source.GeneralLedgerAccount()
            {
                ReferenceCode = "BIvaKou",
                SortCode = "A.A",
                ReferenceNumber = "0101000",
                Name = "Kosten van oprichting en van uitgifte van aandelen",
                Description = "Kosten van oprichting en van uitgifte van aandelen",
                BalanceSide = "D",
                Level = 3,
                IsRgsBase = true,
                IsRgsExtended = true,
                ExcludeRgsBV = true,
            };

            var generalLedgerAccountLevel4 = new Allors.Integration.Source.GeneralLedgerAccount()
            {
                ReferenceCode = "BlvaKouVvp",
                SortCode = "A.A.A",
                ReferenceNumber = "0101010",
                Name = "Verkrijgings- of vervaardigingsprijs kosten van oprichting en van uitgifte van aandelen",
                Description = "Verkrijgings- of vervaardigingsprijs kosten van oprichting en van uitgifte van aandelen",
                BalanceSide = "D",
                Level = 4,
                IsRgsBase = true,
                ExcludeRgsUitbr5 = true,
                ExcludeRgsBV = true,
            };

            var source = new Source()
            {
                GeneralLedgerAccounts = new[] {
                    generalLedgerAccountLevel2,
                    generalLedgerAccountLevel3,
                    generalLedgerAccountLevel4,
                },
                MarGeneralLedgerAccounts = Array.Empty<MarGeneralLedgerAccount>(),
            };

            var integration = new Allors.Integration.Integration(this.Database, new DirectoryInfo("C:/Temp"), new NullLoggerFactory());

            integration.Integrate(source);

            var newGeneralLedgerAccounts = new Allors.Database.Domain.GeneralLedgerAccounts(this.Transaction).Extent().ToArray();
            var newGeneralLedgerAccountTypes = new GeneralLedgerAccountTypes(this.Transaction).Extent().ToArray();
            var newGeneralLedgerAccountClassifications = new GeneralLedgerAccountClassifications(this.Transaction).Extent().ToArray();

            Assert.AreEqual(generalLedgerAccounts.Length + 1, newGeneralLedgerAccounts.Length);
            Assert.AreEqual(generalLedgerAccountTypes.Length + 1, newGeneralLedgerAccountTypes.Length);
            Assert.AreEqual(generalLedgerAccountClassifications.Length + 2, newGeneralLedgerAccountClassifications.Length);

            var newGeneralLedgerAccount = newGeneralLedgerAccounts.Except(generalLedgerAccounts).First();
            var newGeneralLedgerAccountType = newGeneralLedgerAccountTypes.Except(generalLedgerAccountTypes).First();
            var newGeneralLedgerAccountClassificationLevel2 = newGeneralLedgerAccountClassifications.Except(generalLedgerAccountClassifications).First(v => v.RgsLevel == 2);
            var newGeneralLedgerAccountClassificationLevel3 = newGeneralLedgerAccountClassifications.Except(generalLedgerAccountClassifications).First(v => v.RgsLevel == 3);

            //Level 2 Assert
            Assert.AreEqual(generalLedgerAccountLevel2.Name, newGeneralLedgerAccountType.Description);

            Assert.AreEqual(generalLedgerAccountLevel2.Level, newGeneralLedgerAccountClassificationLevel2.RgsLevel);
            Assert.AreEqual(generalLedgerAccountLevel2.ReferenceCode, newGeneralLedgerAccountClassificationLevel2.ReferenceCode);
            Assert.AreEqual(generalLedgerAccountLevel2.SortCode, newGeneralLedgerAccountClassificationLevel2.SortCode);
            Assert.AreEqual(generalLedgerAccountLevel2.ReferenceNumber, newGeneralLedgerAccountClassificationLevel2.ReferenceNumber); // TODO:
            Assert.AreEqual(generalLedgerAccountLevel2.Name, newGeneralLedgerAccountClassificationLevel2.Name);
            Assert.IsNull(newGeneralLedgerAccountClassificationLevel2.Parent);

            //Level 3 Asserts
            Assert.AreEqual(generalLedgerAccountLevel3.Level, newGeneralLedgerAccountClassificationLevel3.RgsLevel);
            Assert.AreEqual(generalLedgerAccountLevel3.ReferenceCode, newGeneralLedgerAccountClassificationLevel3.ReferenceCode);
            Assert.AreEqual(generalLedgerAccountLevel3.SortCode, newGeneralLedgerAccountClassificationLevel3.SortCode);
            Assert.AreEqual(generalLedgerAccountLevel3.ReferenceNumber, newGeneralLedgerAccountClassificationLevel3.ReferenceNumber); // TODO:
            Assert.AreEqual(generalLedgerAccountLevel3.Name, newGeneralLedgerAccountClassificationLevel3.Name);
            Assert.AreEqual(newGeneralLedgerAccountClassificationLevel2, newGeneralLedgerAccountClassificationLevel3.Parent);

            //Level 4 Asserts
            Assert.AreEqual(generalLedgerAccountLevel4.ReferenceCode, newGeneralLedgerAccount.ReferenceCode);
            Assert.AreEqual(generalLedgerAccountLevel4.SortCode, newGeneralLedgerAccount.SortCode);
            Assert.AreEqual(generalLedgerAccountLevel4.ReferenceNumber, newGeneralLedgerAccount.ReferenceNumber);
            Assert.AreEqual(generalLedgerAccountLevel4.Name, newGeneralLedgerAccount.Name);
            Assert.AreEqual(generalLedgerAccountLevel4.Description, newGeneralLedgerAccount.Description);
            Assert.AreEqual(new BalanceSides(this.Transaction).Debit, newGeneralLedgerAccount.BalanceSide);
            Assert.AreEqual(generalLedgerAccountLevel4.Level, newGeneralLedgerAccount.RgsLevel);
            Assert.AreEqual(generalLedgerAccountLevel4.IsRgsBase, newGeneralLedgerAccount.IsRgsBase);
            Assert.AreEqual(generalLedgerAccountLevel4.ExcludeRgsUitbr5, newGeneralLedgerAccount.ExcludeRgsUitbr5);
            Assert.AreEqual(generalLedgerAccountLevel4.ExcludeRgsBV, newGeneralLedgerAccount.ExcludeRgsBV);
            Assert.AreEqual(newGeneralLedgerAccountType, newGeneralLedgerAccount.GeneralLedgerAccountType);
            Assert.AreEqual(newGeneralLedgerAccountClassificationLevel3, newGeneralLedgerAccount.GeneralLedgerAccountClassification);
            Assert.IsNull(newGeneralLedgerAccount.CounterPartAccount);
        }

        [Test]
        public async System.Threading.Tasks.Task IntegrationWithXMLReaderTest()
        {
            var generalLedgerAccounts = new Allors.Database.Domain.GeneralLedgerAccounts(this.Transaction).Extent().ToArray();
            var generalLedgerAccountTypes = new GeneralLedgerAccountTypes(this.Transaction).Extent().ToArray();
            var generalLedgerAccountClassifications = new GeneralLedgerAccountClassifications(this.Transaction).Extent().ToArray();

            var integration = new Allors.Integration.Integration(this.Database, new System.IO.DirectoryInfo("Data"), new NullLoggerFactory());

            integration.Integrate();

            var newGeneralLedgerAccounts = new Allors.Database.Domain.GeneralLedgerAccounts(this.Transaction).Extent().ToArray();
            var newGeneralLedgerAccountTypes = new GeneralLedgerAccountTypes(this.Transaction).Extent().ToArray();
            var newGeneralLedgerAccountClassifications = new GeneralLedgerAccountClassifications(this.Transaction).Extent().ToArray();

            Assert.Greater(newGeneralLedgerAccounts.Length, generalLedgerAccounts.Length);
            Assert.Greater(newGeneralLedgerAccountTypes.Length, generalLedgerAccountTypes.Length);
            Assert.Greater(newGeneralLedgerAccountClassifications.Length, generalLedgerAccountClassifications.Length);
        }

        [Test]
        public async System.Threading.Tasks.Task MarGeneralLedgerAccountExtractorTest()
        {
            var docBalansNL = new HtmlDocument();
            docBalansNL.Load("c:/Temp/MarBalansNL.html");

            var docProfitLossNL = new HtmlDocument();
            docProfitLossNL.Load("c:/Temp/MarProfitLossNL.html");

            var extractor = new MarGeneralLedgerAccountExtractor(docBalansNL, docProfitLossNL, new NullLoggerFactory());
            var result = extractor.Execute();

            Assert.IsNotEmpty(result);
        }

        [Test]
        public System.Threading.Tasks.Task MarGeneralLedgerAccountTransformerTest()
        {
            var integration = new Allors.Integration.Integration(this.Database, new DirectoryInfo("C:/Temp"), new NullLoggerFactory());
            integration.Integrate();
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public sealed class FooMap : ClassMap<MarGeneralLedgerAccount>
        {
            public FooMap()
            {
                this.Map(m => m.ReferenceCode);
                this.Map(m => m.Name);
                this.Map(m => m.IsActiva);
                this.Map(m => m.IsPassiva);
                this.Map(m => m.BalanceType);

            }
        }

        [Test]
        public async System.Threading.Tasks.Task RunDezeTestNiet()
        {
            var docBalansNL = new HtmlDocument();
            docBalansNL.Load("c:/Temp/MarVerenigingenEnStichtingenBalansNL.html");

            var docProfitLossNL = new HtmlDocument();
            docProfitLossNL.Load("c:/Temp/MarVerenigingenEnStichtingenProfitLossNL.html");

            var extractor = new MarGeneralLedgerAccountExtractor(docBalansNL, docProfitLossNL, new NullLoggerFactory());
            var result = extractor.Execute();

            var transformer = new MarGeneralLedgerAccountTransformer(new Source() { MarGeneralLedgerAccounts = result }, new Allors.Integration.Population(),new NullLoggerFactory());
            transformer.Execute(out var marGeneralLedgerAccount);

            using (var writer = new StreamWriter("c:/Temp/mar.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
            {
                //csv.Context.RegisterClassMap<FooMap>();
                csv.WriteRecords(marGeneralLedgerAccount);
            }
        }

    }
}
