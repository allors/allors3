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

using Microsoft.Extensions.Logging;

namespace Allors.Integration.Load
{
    using System.Linq;
    using Allors.Database.Domain;
    
    public partial class PersonLoader : Loader
    {
        public PersonLoader(Staging.Staging staging, Population population, ILoggerFactory loggerFactory)
            : base(staging, population)
        {
            this.Logger = loggerFactory.CreateLogger<PersonLoader>();
        }

        public ILogger<PersonLoader> Logger { get; set; }

        public override void OnBuild()
        {
            //var personByExternalPersonKey = this.Population.PersonByExternalPersonKey;

            //var stagingPeople = this.Staging.People
            //    .Where(v => v.ExternalPersonKey != null)
            //    .GroupBy(v => v.ExternalPersonKey)
            //    .Select(v => v.First());

            //foreach (var stagingPerson in stagingPeople.Where(v => !personByExternalPersonKey.ContainsKey(v.ExternalPersonKey)))
            //{
            //    new PersonBuilder(this.Transaction).WithExternalPrimaryKey(stagingPerson.ExternalPrimaryKey).WithExternalPersonKey(stagingPerson.ExternalPersonKey).Build();
            //}
        }

        public override void OnUpdate()
        {
            var salutationByName = this.Population.SalutationByName;

            //var personByExternalPersonKey = this.Population.PersonByExternalPersonKey;
            foreach (var stagingPerson in this.Staging.People)
            {
                //var person = personByExternalPersonKey[stagingPerson.ExternalPersonKey];
                //person.FirstName = stagingPerson.FirstName;
                //person.LastName = stagingPerson.LastName;
                //person.Salutation = salutationByName.Get(stagingPerson.Salutation);
            }
        }
    }
}
