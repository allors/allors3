namespace Integration.Tests.custom
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Integration.Source;
    using Microsoft.Extensions.Logging.Abstractions;
    using NUnit.Framework;

    public class ProductQuoteTest : Test
    {
        public ProductQuoteTest() : base()
        {
        }

        [Test]
        public async System.Threading.Tasks.Task TestTest()
        {
            var generalLedgerAccounts = new GeneralLedgerAccounts(this.Transaction).Extent().ToArray();

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
                GeneralLedgerAccounts = new[] { generalLedgerAccountLevel2, generalLedgerAccountLevel3, generalLedgerAccountLevel4 }
            };

            var integration = new Allors.Integration.Integration(this.Database, new System.IO.DirectoryInfo("C:/Temp"), new NullLoggerFactory());

            integration.Integrate(source);

            var newGeneralLedgerAccounts = new GeneralLedgerAccounts(this.Transaction).Extent().ToArray();

            Assert.AreEqual(generalLedgerAccounts.Length + 1, newGeneralLedgerAccounts.Length);

            var newGeneralLedgerAccount = newGeneralLedgerAccounts.Except(generalLedgerAccounts).First(x => x.RgsLevel == 4);
            
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

        }
    }
}
