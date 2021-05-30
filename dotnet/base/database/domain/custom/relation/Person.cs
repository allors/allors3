// <copyright file="Person.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Person type.</summary>

namespace Allors.Database.Domain
{
    /// <summary>
    /// A living human being.
    /// </summary>
    public partial class Person
    {
        public override string ToString()
        {
            if (this.ExistLastName)
            {
                if (this.ExistFirstName)
                {
                    return string.Concat(this.LastName, " ", this.FirstName);
                }

                return this.LastName;
            }

            return this.UserName;
        }

        public void CustomOnInit(ObjectOnInit method)
        {
            if (this.ExistOrganisationWhereManager)
            {
                this.OrganisationWhereManager.AddEmployee(this);
            }
        }
    }
}
