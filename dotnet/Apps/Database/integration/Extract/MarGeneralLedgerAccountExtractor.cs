namespace Integration.Extract
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Allors.Integration.Source;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Logging;

    public partial class MarGeneralLedgerAccountExtractor
    {
        public MarGeneralLedgerAccountExtractor(HtmlDocument htmlDocumentBalans, HtmlDocument htmlDocumentProfitLoss, ILoggerFactory loggerFactory)
        {
            this.HtmlDocumentBalans = htmlDocumentBalans;
            this.HtmlDocumentProfitLoss = htmlDocumentProfitLoss;
            this.Logger = loggerFactory.CreateLogger<GeneralLedgerAccountExtractor>();
        }

        public HtmlDocument HtmlDocumentBalans { get; set; }
        public HtmlDocument HtmlDocumentProfitLoss { get; set; }

        public ILogger<GeneralLedgerAccountExtractor> Logger { get; set; }

        public MarGeneralLedgerAccount[] Execute()
        {
            var marGeneralLedgerAccounts = this.ExtractItems(this.HtmlDocumentBalans, "Balans");
            marGeneralLedgerAccounts.AddRange(this.ExtractItems(this.HtmlDocumentProfitLoss, "Profit Loss"));

            return marGeneralLedgerAccounts.ToArray();
        }

        private List<MarGeneralLedgerAccount> ExtractItems(HtmlDocument htmlDocument, string balanceType)
        {
            var MarGeneralLedgerAccounts = new List<MarGeneralLedgerAccount>();

            var rows = htmlDocument.DocumentNode.SelectNodes("/html/body/table/tbody/tr");

            foreach (var row in rows.Skip(2))
            {

                var paragraphs = row.SelectNodes("*/p").ToArray();

                //var temp = "";
                //if (paragraphs.Length > 4)
                //{
                //    paragraphs = paragraphs.Skip(Math.Max(0, paragraphs.Length - 4)).ToArray();

                //    if (!Regex.IsMatch(paragraphs[0].InnerText, "[0-9]"))
                //    {
                //        temp = row.SelectNodes("*/p").ToArray().Where(x => Regex.IsMatch(x.InnerText, "[0-9]")).FirstOrDefault()?.InnerText;
                //    }
                //}

                var marGeneralLedgerAccount = new MarGeneralLedgerAccount
                {
                    ReferenceCode = paragraphs[0].InnerText, // temp.Equals("") ? temp : 
                    Name = paragraphs[1].InnerText,
                    IsActiva = paragraphs[2].InnerText,
                    IsPassiva = paragraphs[3].InnerText,
                    BalanceType = balanceType
                };


                MarGeneralLedgerAccounts.Add(marGeneralLedgerAccount);
            }

            return MarGeneralLedgerAccounts;
        }
    }
}
