namespace Integration.Extract
{
    using System.IO;
    using System.Linq;
    using Allors.Integration.Source;
    using Microsoft.Extensions.Logging;

    public partial class MarAccountingObligeeEnterprisesGeneralLedgerAccountExtractor
    {
        public MarAccountingObligeeEnterprisesGeneralLedgerAccountExtractor(StreamReader streamReader, ILoggerFactory loggerFactory)
        {
            this.StreamReader = streamReader;
            this.Logger = loggerFactory.CreateLogger<MarAccountingObligeeEnterprisesGeneralLedgerAccountExtractor>();
        }

        public StreamReader StreamReader { get; set; }

        public ILogger<MarAccountingObligeeEnterprisesGeneralLedgerAccountExtractor> Logger { get; set; }

        public MarGeneralLedgerAccount[] Execute()
        {
            var ser = new System.Xml.Serialization.XmlSerializer(typeof(MarGeneralLedgerAccounts));

            return ((MarGeneralLedgerAccounts)ser.Deserialize(StreamReader)).marGeneralLedgerAccount.Select(x => x).ToArray();
        }
    }
}
