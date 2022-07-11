// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectsBase.cs" company="Allors bvba">
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

namespace Allors.Integration.Transform
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;

    public partial class CountryNameTransformer
    {
        public CountryNameTransformer(Population population, ILoggerFactory loggerFactory)
        {
            this.Population = population;
            this.CountryNameByUpperCaseName = population.Countries.ToDictionary(v => v.Name.ToUpperInvariant(), v => v.Name);
            this.Logger = loggerFactory.CreateLogger<CountryNameTransformer>();
        }

        public Population Population { get; }

        public Dictionary<string, string> CountryNameByUpperCaseName;

        public ILogger<CountryNameTransformer> Logger { get; set; }

        public string Execute(string sourceName)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                return null;
            }

            var sourceUpperCaseName = sourceName.ToUpper().Trim();

            if (sourceUpperCaseName == "TAIWAN")
            {

            }

            if (sourceUpperCaseName == "USA FOR USE IN ITALY") { return "United States of America"; }
            if (sourceUpperCaseName.Contains("MARTINI")) { return "France"; }
            if (sourceUpperCaseName.Contains("ITAL")) { return "Italy"; }
            if (sourceUpperCaseName.Contains("UAE")) { return "United Arab Emirates"; }
            if (sourceUpperCaseName.Contains("EMI")) { return "United Arab Emirates"; }
            if (sourceUpperCaseName.Contains("DUBA")) { return "United Arab Emirates"; }
            if (sourceUpperCaseName.Contains("U.A.E.")) { return "United Arab Emirates"; }
            if (sourceUpperCaseName.Contains("MACED")) { return "Macedonia (the former Yugoslav Republic of)"; }
            if (sourceUpperCaseName.Contains("LIBIA")) { return "Libya"; }
            if (sourceUpperCaseName.Contains("LIBYA")) { return "Libya"; }
            if (sourceUpperCaseName.Contains("TROPL")) { return "Libya"; }
            if (sourceUpperCaseName.Contains("LYBIA")) { return "Libya"; }
            if (sourceUpperCaseName.Contains("SAUDI")) { return "Saudi Arabia"; }
            if (sourceUpperCaseName.Contains("USA")) { return "United States of America"; }
            if (sourceUpperCaseName.Contains("U.S.A.")) { return "United States of America"; }
            if (sourceUpperCaseName.Contains("UNITED STATE")) { return "United States of America"; }
            if (sourceUpperCaseName.Contains("UNITED STATES")) { return "United States of America"; }
            if (sourceUpperCaseName.Contains("UNIDOS")) { return "United States of America"; }
            if (sourceUpperCaseName.Contains("NEDER")) { return "Netherlands"; }
            if (sourceUpperCaseName.Contains("HOLLAND")) { return "Netherlands"; }
            if (sourceUpperCaseName.Contains("PAYS")) { return "Netherlands"; }
            if (sourceUpperCaseName.Contains("NETHER")) { return "Netherlands"; }
            if (sourceUpperCaseName.Contains("CONGO")) { return "Congo (Democratic Republic of the)"; }
            if (sourceUpperCaseName.Contains("RDC")) { return "Congo (Democratic Republic of the)"; }
            if (sourceUpperCaseName.Contains("Congo [DRC]")) { return "Congo (Democratic Republic of the)"; }
            if (sourceUpperCaseName.Contains("VINCENT")) { return "Saint Vincent and the Grenadines"; }
            if (sourceUpperCaseName.Contains("CHAD")) { return "Chad"; }
            if (sourceUpperCaseName.Contains("ANGOLA")) { return "Angola"; }
            if (sourceUpperCaseName.Contains("KOSOVO")) { return "Kosovo"; }
            if (sourceUpperCaseName.Contains("TUNIS")) { return "Tunisia"; }
            if (sourceUpperCaseName.Contains("DEUTSCH")) { return "Germany"; }
            if (sourceUpperCaseName.Contains("BOSNA")) { return "Bosnia and Herzegovina"; }
            if (sourceUpperCaseName.Contains("BOSNIA")) { return "Bosnia and Herzegovina"; }
            if (sourceUpperCaseName.Contains("SLOVAK")) { return "Slovakia"; }
            if (sourceUpperCaseName.Contains("MAROC")) { return "Morocco"; }
            if (sourceUpperCaseName.Contains("ESPAÑA")) { return "Spain"; }
            if (sourceUpperCaseName.Contains("ESPAGNE")) { return "Spain"; }
            if (sourceUpperCaseName.Contains("SPAIN")) { return "Spain"; }
            if (sourceUpperCaseName.Contains("MADRID")) { return "Spain"; }
            if (sourceUpperCaseName.Contains("BELGI")) { return "Belgium"; }
            if (sourceUpperCaseName.Contains("MAURITA")) { return "Mauritania"; }
            if (sourceUpperCaseName == "UK") { return "United Kingdom of Great Britain and Northern Ireland"; }
            if (sourceUpperCaseName.Contains("BEDS")) { return "United Kingdom of Great Britain and Northern Ireland"; }
            if (sourceUpperCaseName.Contains("SUSSEX")) { return "United Kingdom of Great Britain and Northern Ireland"; }
            if (sourceUpperCaseName.Contains("LEICESTER")) { return "United Kingdom of Great Britain and Northern Ireland"; }
            if (sourceUpperCaseName.Contains("UNITED KING")) { return "United Kingdom of Great Britain and Northern Ireland"; }
            if (sourceUpperCaseName.Contains("UNITED  KINGDOM")) { return "United Kingdom of Great Britain and Northern Ireland"; }
            if (sourceUpperCaseName.Contains("SCOTLAND")) { return "United Kingdom of Great Britain and Northern Ireland"; }
            if (sourceUpperCaseName.Contains("ENGLAND")) { return "United Kingdom of Great Britain and Northern Ireland"; }
            if (sourceUpperCaseName.Contains("SIERRA")) { return "Sierra Leone"; }
            if (sourceUpperCaseName.Contains("ALGE")) { return "Algeria"; }
            if (sourceUpperCaseName.Contains("ALGÉR")) { return "Algeria"; }
            if (sourceUpperCaseName == "TURQUIE") { return "Turkey"; }
            if (sourceUpperCaseName == "TÜRKIYE") { return "Turkey"; }
            if (sourceUpperCaseName.Contains("TAHIT")) { return "French Polynesia"; }
            if (sourceUpperCaseName.Contains("POLYN")) { return "French Polynesia"; }
            if (sourceUpperCaseName.Contains("POLSK")) { return "Poland"; }
            if (sourceUpperCaseName.Contains("SURIN")) { return "Suriname"; }
            if (sourceUpperCaseName.Contains("BRASIL")) { return "Brazil"; }
            if (sourceUpperCaseName.Contains("BAHR")) { return "Bahrain"; }
            if (sourceUpperCaseName.Contains("SOMA")) { return "Somalia"; }
            if (sourceUpperCaseName.Contains("SLOVE")) { return "Slovenia"; }
            if (sourceUpperCaseName.Contains("MOLDOVA")) { return "Moldova (Republic of)"; }
            if (sourceUpperCaseName.Contains("MOLDO")) { return "Moldova (Republic of)"; }
            if (sourceUpperCaseName.Contains("MOLDA")) { return "Moldova (Republic of)"; }
            if (sourceUpperCaseName.Contains("REUNION")) { return "Réunion"; }
            if (sourceUpperCaseName.Contains("IVOIR")) { return "Côte d'Ivoire"; }
            if (sourceUpperCaseName.Contains("IVORY")) { return "Côte d'Ivoire"; }
            if (sourceUpperCaseName.Contains("SRI")) { return "Sri Lanka"; }
            if (sourceUpperCaseName.Contains("NORGE")) { return "Norway"; }
            if (sourceUpperCaseName.Contains("KENYA")) { return "Kenya"; }
            if (sourceUpperCaseName.Contains("SWISS")) { return "Switzerland"; }
            if (sourceUpperCaseName.Contains("SUISS")) { return "Switzerland"; }
            if (sourceUpperCaseName.Contains("SCHWEIZ")) { return "Switzerland"; }
            if (sourceUpperCaseName.Contains("LATV")) { return "Latvia"; }
            if (sourceUpperCaseName.Contains("PERÚ")) { return "Peru"; }
            if (sourceUpperCaseName.Contains("BARTH")) { return "Saint Barthélemy"; }
            if (sourceUpperCaseName.Contains("TALLIN")) { return "Estonia"; }
            if (sourceUpperCaseName.Contains("HONGR")) { return "Hungary"; }
            if (sourceUpperCaseName.Contains("MALDIV")) { return "Maldives"; }
            if (sourceUpperCaseName.Contains("GANDA")) { return "Uganda"; }
            if (sourceUpperCaseName.Contains("BENIN")) { return "Benin"; }
            if (sourceUpperCaseName.Contains("BÉNIN")) { return "Benin"; }
            if (sourceUpperCaseName.Contains("GUYAN")) { return "French Guiana"; }
            if (sourceUpperCaseName.Contains("JORDAN")) { return "Jordan"; }
            if (sourceUpperCaseName.Contains("BAHAMA")) { return "Bahamas"; }
            if (sourceUpperCaseName.Contains("CANAD")) { return "Canada"; }
            if (sourceUpperCaseName.Contains("SÚEDE")) { return "Sweden"; }
            if (sourceUpperCaseName.Contains("SUÈDE")) { return "Sweden"; }
            if (sourceUpperCaseName.Contains("CESK")) { return "Czechia"; }
            if (sourceUpperCaseName.Contains("ESKÁ")) { return "Czechia"; }
            if (sourceUpperCaseName == "CZ REPUBLIC") { return "Czechia"; }
            if (sourceUpperCaseName == "CZECH REPUBLIC") { return "Czechia"; }
            if (sourceUpperCaseName.Contains("SRPSK")) { return "Serbia"; }
            if (sourceUpperCaseName.Contains("CORSE")) { return "France"; }
            if (sourceUpperCaseName.Contains("FRANCE")) { return "France"; }
            if (sourceUpperCaseName.Contains("LIBAN")) { return "Lebanon"; }
            if (sourceUpperCaseName.Contains("EGYP")) { return "Egypt"; }
            if (sourceUpperCaseName.Contains("DOMINICAN")) { return "Dominican Republic"; }
            if (sourceUpperCaseName.Contains("SAMOA")) { return "Samoa"; }
            if (sourceUpperCaseName.Contains("TRINIDAD")) { return "Trinidad and Tobago"; }
            if (sourceUpperCaseName.Contains("RUSSIA")) { return "Russian Federation"; }
            if (sourceUpperCaseName.Contains("ZANZI")) { return "Tanzania, United Republic of"; }
            if (sourceUpperCaseName.Contains("TANZANIA")) { return "Tanzania, United Republic of"; }
            if (sourceUpperCaseName.Contains("IRELAND")) { return "Ireland"; }
            if (sourceUpperCaseName.Contains("PAPOU")) { return "Papua New Guinea"; }
            if (sourceUpperCaseName.Contains("LUCIA")) { return "Saint Lucia"; }
            if (sourceUpperCaseName.Contains("CAMER")) { return "Cameroon"; }
            if (sourceUpperCaseName.Contains("KOREA")) { return "Korea (Republic of)"; }
            if (sourceUpperCaseName.Contains("DARFUR")) { return "Sudan"; }
            if (sourceUpperCaseName.Contains("PANAMA")) { return "Panama"; }
            if (sourceUpperCaseName.Contains("CARAI")) { return "Guadeloupe"; }
            if (sourceUpperCaseName.Contains("MAMIBIA")) { return "Namibia"; }
            if (sourceUpperCaseName.Contains("BELARUS")) { return "Belarus"; }
            if (sourceUpperCaseName.Contains("KAZAK")) { return "Kazakhstan"; }
            if (sourceUpperCaseName.Contains("MARIANA")) { return "Northern Mariana Islands"; }
            if (sourceUpperCaseName.Contains("FIJI")) { return "Fiji"; }
            if (sourceUpperCaseName.Contains("OMAN")) { return "Oman"; }
            if (sourceUpperCaseName.Contains("CAYMAN")) { return "Cayman Islands"; }
            if (sourceUpperCaseName.Contains("IRAN")) { return "Iran (Islamic Republic of)"; }
            if (sourceUpperCaseName.Contains("BOLIVIA")) { return "Bolivia (Plurinational State of)"; }
            if (sourceUpperCaseName.Contains("SYRIA")) { return "Syrian Arab Republic"; }
            if (sourceUpperCaseName.Contains("VIETNAM")) { return "Viet Nam"; }
            if (sourceUpperCaseName.Contains("BISS")) { return "Guinea-Bissau"; }
            if (sourceUpperCaseName.Contains("République de Guinée".ToUpperInvariant())) { return "Guinea"; }
            if (sourceUpperCaseName.Contains("Republic de Guinee".ToUpperInvariant())) { return "Guinea"; }
            if (sourceUpperCaseName.Contains("St. Maarten".ToUpperInvariant())) { return "Netherlands"; }
            if (sourceUpperCaseName.Contains("Sénégal".ToUpperInvariant())) { return "Senegal"; }
            if (sourceUpperCaseName.Contains("Taiwan".ToUpperInvariant())) { return "Taiwan (Province of China)"; }

            if (this.CountryNameByUpperCaseName.TryGetValue(sourceUpperCaseName, out var name))
            {
                return name;
            }

            foreach (var kvp in this.CountryNameByUpperCaseName)
            {
                if (sourceUpperCaseName.Contains(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            this.Logger.LogError("Can not find country " + sourceName);

            throw new Exception(this.GetType().Name);
        }
    }
}
