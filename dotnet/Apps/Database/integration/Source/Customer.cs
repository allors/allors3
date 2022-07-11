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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Allors.Integration.Source
{
    public partial class Customer
    {
        public string ExternalPrimaryKey => $"{this.Bedrijfsnaam}{this.BtwNummerKant}";

        public string ExternalPersonKey => $"{this.Contact}";

        public string Bedrijfsnaam { get; set; } // ex. "European Cleaning & Maintenance"

        public bool IsCustomer { get; set; } // 'Y' or blank

        public bool IsSupplier { get; set; } // 'Y' or blank

        public bool IsSupplierForAM { get; set; } // supplier for Aviaco Asset Management; 'Y' or blank

        public bool IsSupplierForBvba { get; set; } // supplier for Aviaco bvba; 'Y' or blank

        public bool IsSupplierForGR { get; set; } // supplier for Aviaco green support; 'Y' or blank

        public bool IsSupplierForEs { get; set; } // supplier for Aviaco spain; 'Y' or blank

        public bool IsSupplierForNl { get; set; } // supplier for Aviaco nederland; 'Y' or blank

        public bool IsSupplierForRM { get; set; } // supplier for Aviaco repair & maintenance; 'Y' or blank

        public string Contact { get; set; } // ex. "Mr. Fabian Fantauzzo"

        public string Adres { get; set; } // ex. "Rue Maréchal Foch 21"

        public string Plaats { get; set; } // ex. "Flemalle"

        public string Land { get; set; } // ex. "Belgium"

        public string Postcode { get; set; } // ex. "B-4400"

        public string BtwNummerKant { get; set; } // ex. "BE 0436 088 739"

        public string Email { get; set; } // ex. "john@doe.org"

        public string Stage2Salutation(HashSet<string> populationSalutations)
        {
            if (!string.IsNullOrEmpty(this.Contact))
            {
                var split = this.Contact.Split(' ');
                var salutation = split[0];
                if (populationSalutations.Contains(salutation))
                {
                    return salutation;
                }
            }

            return null;
        }

        public string Stage2FirstName(HashSet<string> populationSalutations)
        {
            if (!string.IsNullOrEmpty(this.Contact))
            {
                var split = this.Contact.Split(' ');
                if (populationSalutations.Contains(split[0]))
                {
                    split = split.Skip(1).ToArray();
                }

                return split[0];
            }

            return null;
        }

        public string Stage2LastName(HashSet<string> populationSalutations)
        {
            if (!string.IsNullOrEmpty(this.Contact))
            {
                var split = this.Contact.Split(' ');
                if (populationSalutations.Contains(split[0]))
                {
                    split = split.Skip(1).ToArray();
                }

                split = split.Skip(1).ToArray();

                return string.Join(" ", split);
            }

            return null;
        }
        
        public override string ToString()
        {
            return $"{this.Bedrijfsnaam}";
        }
    }
}
