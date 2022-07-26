namespace Integration.Extract
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Allors.Integration.Source;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Logging;

    public partial class MarGeneralLedgerAccountExtractor
    {
        public MarGeneralLedgerAccountExtractor(StreamReader streamReader, HtmlDocument htmlDocumentBalans, HtmlDocument htmlDocumentProfitLoss, ILoggerFactory loggerFactory)
        {
            this.StreamReader = streamReader;
            this.HtmlDocumentBalans = htmlDocumentBalans;
            this.HtmlDocumentProfitLoss = htmlDocumentProfitLoss;
            this.Logger = loggerFactory.CreateLogger<GeneralLedgerAccountExtractor>();
        }

        public HtmlDocument HtmlDocumentBalans { get; set; }
        public HtmlDocument HtmlDocumentProfitLoss { get; set; }
        public StreamReader StreamReader { get; set; }

        public ILogger<GeneralLedgerAccountExtractor> Logger { get; set; }

        public MarGeneralLedgerAccount[] Execute()
        {
            //var ser = new System.Xml.Serialization.XmlSerializer(typeof(MarGeneralLedgerAccounts));

            //return ((MarGeneralLedgerAccounts)ser.Deserialize(StreamReader)).marGeneralLedgerAccount.Select(x => x).ToArray();

            var marGeneralLedgerAccounts = this.ExtractItems(this.HtmlDocumentBalans, "Balans");
            marGeneralLedgerAccounts.AddRange(this.ExtractItems(this.HtmlDocumentProfitLoss, "Profit Loss"));

            return marGeneralLedgerAccounts.ToArray();
        }

        private List<MarGeneralLedgerAccount> ExtractItems(HtmlDocument htmlDocument, string balanceType)
        {
            var marGeneralLedgerAccounts = new List<MarGeneralLedgerAccount>();

            var rows = htmlDocument.DocumentNode.SelectNodes("/html/body/table/tbody/tr");

            foreach (var row in rows.Skip(2))
            {

                var paragraphs = row.SelectNodes("*/p").ToArray();

                var temp = "";

                if (paragraphs.Length > 4)
                {
                    paragraphs = paragraphs.Skip(Math.Max(0, paragraphs.Length - 4)).ToArray();
                }

                var marGeneralLedgerAccount = new MarGeneralLedgerAccount
                {
                    ReferenceCode = paragraphs[0].InnerText.Replace("&nbsp;", string.Empty).Replace(".", ""), // temp.Equals("") ? temp : 
                    Name = paragraphs[1].InnerText.Replace("&nbsp;", string.Empty),
                    IsActiva = paragraphs[2].InnerText.Replace("&nbsp;", string.Empty),
                    IsPassiva = paragraphs[3].InnerText.Replace("&nbsp;", string.Empty),
                    BalanceType = balanceType
                };

                if (!marGeneralLedgerAccount.IsEmpty())
                {
                    marGeneralLedgerAccounts.Add(marGeneralLedgerAccount);
                }
            }

            return marGeneralLedgerAccounts;
        }
    }
}
