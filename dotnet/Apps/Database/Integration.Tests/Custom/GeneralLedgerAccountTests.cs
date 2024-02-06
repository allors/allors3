namespace Integration.Tests.custom
{
    using System;
    using System.IO;
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Integration.Source;
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
                MarAccountingObligeeEnterprisesGeneralLedgerAccounts = Array.Empty<MarGeneralLedgerAccount>(),
                MarAssociationsAndFoundationsGeneralLedgerAccounts = Array.Empty<MarGeneralLedgerAccount>(),
            };

            var integration = new Allors.Integration.Integration(this.Database, new DirectoryInfo("C:/Temp"), new NullLoggerFactory());

            integration.Integrate(source);

            var newGeneralLedgerAccounts = new Allors.Database.Domain.GeneralLedgerAccounts(this.Transaction).Extent().ToArray();
            var newGeneralLedgerAccountTypes = new GeneralLedgerAccountTypes(this.Transaction).Extent().ToArray();
            var newGeneralLedgerAccountClassifications = new GeneralLedgerAccountClassifications(this.Transaction).Extent().ToArray();

            Assert.That(newGeneralLedgerAccounts.Length, Is.EqualTo(generalLedgerAccounts.Length + 1));
            Assert.That(newGeneralLedgerAccountTypes.Length, Is.EqualTo(generalLedgerAccountTypes.Length + 1));
            Assert.That(newGeneralLedgerAccountClassifications.Length, Is.EqualTo(generalLedgerAccountClassifications.Length + 2));

            var newGeneralLedgerAccount = newGeneralLedgerAccounts.Except(generalLedgerAccounts).First();
            var newGeneralLedgerAccountType = newGeneralLedgerAccountTypes.Except(generalLedgerAccountTypes).First();
            var newGeneralLedgerAccountClassificationLevel2 = newGeneralLedgerAccountClassifications.Except(generalLedgerAccountClassifications).First(v => v.RgsLevel == 2);
            var newGeneralLedgerAccountClassificationLevel3 = newGeneralLedgerAccountClassifications.Except(generalLedgerAccountClassifications).First(v => v.RgsLevel == 3);

            //Level 2 Assert
            Assert.That(newGeneralLedgerAccountType.Description, Is.EqualTo(generalLedgerAccountLevel2.Name));

            Assert.That(newGeneralLedgerAccountClassificationLevel2.RgsLevel, Is.EqualTo(generalLedgerAccountLevel2.Level));
            Assert.That(newGeneralLedgerAccountClassificationLevel2.ReferenceCode, Is.EqualTo(generalLedgerAccountLevel2.ReferenceCode));
            Assert.That(newGeneralLedgerAccountClassificationLevel2.SortCode, Is.EqualTo(generalLedgerAccountLevel2.SortCode));
            Assert.That(newGeneralLedgerAccountClassificationLevel2.ReferenceNumber, Is.EqualTo(generalLedgerAccountLevel2.ReferenceNumber)); // TODO:
            Assert.That(newGeneralLedgerAccountClassificationLevel2.Name, Is.EqualTo(generalLedgerAccountLevel2.Name));
            Assert.That(newGeneralLedgerAccountClassificationLevel2.Parent, Is.Null);

            //Level 3 Asserts
            Assert.That(newGeneralLedgerAccountClassificationLevel3.RgsLevel, Is.EqualTo(generalLedgerAccountLevel3.Level));
            Assert.That(newGeneralLedgerAccountClassificationLevel3.ReferenceCode, Is.EqualTo(generalLedgerAccountLevel3.ReferenceCode));
            Assert.That(newGeneralLedgerAccountClassificationLevel3.SortCode, Is.EqualTo(generalLedgerAccountLevel3.SortCode));
            Assert.That(newGeneralLedgerAccountClassificationLevel3.ReferenceNumber, Is.EqualTo(generalLedgerAccountLevel3.ReferenceNumber)); // TODO:
            Assert.That(newGeneralLedgerAccountClassificationLevel3.Name, Is.EqualTo(generalLedgerAccountLevel3.Name));
            Assert.That(newGeneralLedgerAccountClassificationLevel3.Parent, Is.EqualTo(newGeneralLedgerAccountClassificationLevel2));

            //Level 4 Asserts
            Assert.That(newGeneralLedgerAccount.ReferenceCode, Is.EqualTo(generalLedgerAccountLevel4.ReferenceCode));
            Assert.That(newGeneralLedgerAccount.SortCode, Is.EqualTo(generalLedgerAccountLevel4.SortCode));
            Assert.That(newGeneralLedgerAccount.ReferenceNumber, Is.EqualTo(generalLedgerAccountLevel4.ReferenceNumber));
            Assert.That(newGeneralLedgerAccount.Name, Is.EqualTo(generalLedgerAccountLevel4.Name));
            Assert.That(newGeneralLedgerAccount.Description, Is.EqualTo(generalLedgerAccountLevel4.Description));
            Assert.That(newGeneralLedgerAccount.BalanceSide, Is.EqualTo(new BalanceSides(this.Transaction).Debit));
            Assert.That(newGeneralLedgerAccount.RgsLevel, Is.EqualTo(generalLedgerAccountLevel4.Level));
            Assert.That(newGeneralLedgerAccount.IsRgsBase, Is.EqualTo(generalLedgerAccountLevel4.IsRgsBase));
            Assert.That(newGeneralLedgerAccount.ExcludeRgsUitbr5, Is.EqualTo(generalLedgerAccountLevel4.ExcludeRgsUitbr5));
            Assert.That(newGeneralLedgerAccount.ExcludeRgsBV, Is.EqualTo(generalLedgerAccountLevel4.ExcludeRgsBV));
            Assert.That(newGeneralLedgerAccount.GeneralLedgerAccountType, Is.EqualTo(newGeneralLedgerAccountType));
            Assert.That(newGeneralLedgerAccount.GeneralLedgerAccountClassification, Is.EqualTo(newGeneralLedgerAccountClassificationLevel3));
            Assert.That(newGeneralLedgerAccount.CounterPartAccount, Is.Null);
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

            Assert.That(newGeneralLedgerAccounts.Length, Is.GreaterThan(generalLedgerAccounts.Length));
            Assert.That(newGeneralLedgerAccountTypes.Length, Is.GreaterThan(generalLedgerAccountTypes.Length));
            Assert.That(newGeneralLedgerAccountClassifications.Length, Is.GreaterThan(generalLedgerAccountClassifications.Length));
        }

        [Test]
        public async System.Threading.Tasks.Task MarGeneralLedgerAccountExtractorTest()
        {
            using (var marAccountingObligeeEnterprisesGeneralLedgerAccountList = new StreamReader("C:/Temp/MarBoekhoudplichtigeOndernemingen.xml"))
            using (var marAssociationsAndFoundationsGeneralLedgerAccountGeneralLedgerAccountList = new StreamReader("C:/Temp/MarVerenigingenEnStichtingen.xml"))
            {
                var accountingObligeeEnterprisesExtractor = new MarAccountingObligeeEnterprisesGeneralLedgerAccountExtractor(marAccountingObligeeEnterprisesGeneralLedgerAccountList, new NullLoggerFactory());
                var accountingObligeeEnterprisesResult = accountingObligeeEnterprisesExtractor.Execute();

                var associationsAndFoundationsExtractor = new MarAssociationsAndFoundationsGeneralLedgerAccountExtractor(marAssociationsAndFoundationsGeneralLedgerAccountGeneralLedgerAccountList, new NullLoggerFactory());
                var associationsAndFoundationsResult = associationsAndFoundationsExtractor.Execute();

                Assert.That(accountingObligeeEnterprisesResult, Is.Not.Empty);
                Assert.That(associationsAndFoundationsResult, Is.Not.Empty);
            }


        }

        [Test]
        public System.Threading.Tasks.Task MarGeneralLedgerAccountTransformerTest()
        {
            var integration = new Allors.Integration.Integration(this.Database, new DirectoryInfo("C:/Temp"), new NullLoggerFactory());
            integration.Integrate();
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
