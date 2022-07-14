namespace Integration.Tests.custom
{
    using System.IO;
    using System.Linq;
    using Allors.Integration.Source;
    using Microsoft.Extensions.Logging.Abstractions;
    using NUnit.Framework;

    public class XmlReader : Test
    {
        public XmlReader() : base()
        {
        }

        [Test]
        public async System.Threading.Tasks.Task XMLReaderTest()
        {
            var test = this.DeserializeToObject<GeneralLedgerAccounts>("c:/Temp/RGS.xml");

            var firstElement = test.generalLedgerAccounts[2];
            Assert.AreEqual("BIvaKou", firstElement.ReferenceCode);
            Assert.AreEqual("", firstElement.CounterPartAccount);
            Assert.AreEqual("A.A", firstElement.SortCode);
            Assert.AreEqual("0101000", firstElement.ReferenceNumber);
            Assert.AreEqual("Kosten van oprichting en van uitgifte van aandelen", firstElement.Name);
            Assert.AreEqual("Kosten van oprichting en van uitgifte van aandelen", firstElement.Description);
            Assert.AreEqual("D", firstElement.BalanceSide);
            Assert.AreEqual(3, firstElement.Level);
            Assert.IsFalse(firstElement.IsRgsExcluded);
            Assert.IsTrue(firstElement.IsRgsBase);
            Assert.IsTrue(firstElement.IsRgsExtended);
            Assert.IsFalse(firstElement.IsRgsUseWithEZ);
            Assert.IsFalse(firstElement.IsRgsUseWithZzp);
            Assert.IsFalse(firstElement.IsRgsUseWithWoco);

        }

        public T DeserializeToObject<T>(string filepath) where T : class
        {
            var ser = new System.Xml.Serialization.XmlSerializer(typeof(T));

            using (var sr = new StreamReader(filepath))
            {
                return (T)ser.Deserialize(sr);
            }
        }
    }
}
