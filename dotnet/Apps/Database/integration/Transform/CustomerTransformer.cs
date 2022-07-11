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

using System.Linq;
using Allors.Database.Domain;
using Allors.Integration.Staging;
using Microsoft.Extensions.Logging;

namespace Allors.Integration.Transform
{
    public partial class CustomerTransformer
    {
        public CustomerTransformer(Source.Source source, Population population, ILoggerFactory loggerFactory)
        {
            this.Source = source;
            this.Population = population;
            this.LoggerFactory = loggerFactory;
            //this.Logger = loggerFactory.CreateLogger<StockTransformer>();
        }

        public Source.Source Source { get; }

        public Population Population { get; }

        public ILoggerFactory LoggerFactory { get; }

        //public ILogger<StockTransformer> Logger { get; set; }

        public void Execute(out PostalAddress[] postalAddresses, out Database.Domain.Person[] people, out Organisation[] organisations)
        {
            var countryNameTransformer = new CountryNameTransformer(this.Population, this.LoggerFactory);

            postalAddresses = null;
            people = null;
            organisations = null;

            //postalAddresses = this.Source.Customers
            //    .Select(v => new PostalAddress
            //    {
            //        ExternalPrimaryKey = v.ExternalPrimaryKey,
            //        Address1 = !string.IsNullOrWhiteSpace(v.Adres) ? v.Adres : ".",
            //        Locality = v.Plaats,
            //        PostalCode = v.Postcode,
            //        CountryByName = countryNameTransformer.Execute(v.Land)
            //    }).ToArray();

            //var salutationNames = this.Population.SalutationNames;
            //people = this.Source.Customers
            //    .Where(v => !string.IsNullOrWhiteSpace(v.Contact))
            //    .Select(v => new Person
            //    {
            //        ExternalPrimaryKey = v.ExternalPrimaryKey,
            //        ExternalPersonKey = v.ExternalPersonKey,
            //        Salutation = v.Stage2Salutation(salutationNames),
            //        FirstName = v.Stage2FirstName(salutationNames),
            //        LastName = v.Stage2LastName(salutationNames),
            //    }).ToArray();

            //organisations = this.Source.Customers
            //    .Select(v => new Organisation
            //    {
            //        ExternalPrimaryKey = v.ExternalPrimaryKey,
            //        ExternalPersonKey= v.ExternalPersonKey,
            //        Name = v.Bedrijfsnaam,
            //        TaxNumber = v.BtwNummerKant,
            //        PostalAddressByExternalPrimaryKey = v.ExternalPrimaryKey,
            //        Email = v.Email,
            //        IsCustomer = v.IsCustomer,
            //        IsSupplier = v.IsSupplier,
            //        IsSupplierForAM = v.IsSupplierForAM,
            //        IsSupplierForBvba = v.IsSupplierForBvba,
            //        IsSupplierForEs = v.IsSupplierForEs,
            //        IsSupplierForGR = v.IsSupplierForGR,
            //        IsSupplierForRM = v.IsSupplierForRM,
            //        IsSupplierForNl = v.IsSupplierForNl
            //    }).ToArray();
        }
    }
}
