namespace Integration.Extract
{
    using System.IO;
    using System.Linq;
    using Allors.Integration.Source;
    using Microsoft.Extensions.Logging;

    public partial class MarAssociationsAndFoundationsGeneralLedgerAccountExtractor
    {
        public MarAssociationsAndFoundationsGeneralLedgerAccountExtractor(StreamReader streamReader, ILoggerFactory loggerFactory)
        {
            this.StreamReader = streamReader;
            this.Logger = loggerFactory.CreateLogger<MarAssociationsAndFoundationsGeneralLedgerAccountExtractor>();
        }

        public StreamReader StreamReader { get; set; }

        public ILogger<MarAssociationsAndFoundationsGeneralLedgerAccountExtractor> Logger { get; set; }

        public MarGeneralLedgerAccount[] Execute()
        {
            var ser = new System.Xml.Serialization.XmlSerializer(typeof(MarGeneralLedgerAccounts));

            return ((MarGeneralLedgerAccounts)ser.Deserialize(StreamReader)).marGeneralLedgerAccount.Select(x => x).ToArray();
        }
    }
}
