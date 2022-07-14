namespace Integration.Extract
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Allors.Integration.Source;
    using Microsoft.Extensions.Logging;

    public partial class GeneralLedgerAccountExtractor
    {
        public GeneralLedgerAccountExtractor(StreamReader streamReader, ILoggerFactory loggerFactory)
        {
            this.StreamReader = streamReader;
            this.Logger = loggerFactory.CreateLogger<GeneralLedgerAccountExtractor>();
        }

        public StreamReader StreamReader { get; set; }

        public ILogger<GeneralLedgerAccountExtractor> Logger { get; set; }

        public GeneralLedgerAccount[] Execute()
        {
            var ser = new System.Xml.Serialization.XmlSerializer(typeof(GeneralLedgerAccounts));

            return ((GeneralLedgerAccounts) ser.Deserialize(StreamReader)).generalLedgerAccounts.Select(x => x).ToArray();
        }
    }
}
