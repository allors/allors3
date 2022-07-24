// <copyright file="Security.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;

    public partial class Security
    {
        public void Grantemployee(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.EmployeeId, objectType, operations);

        public void GrantCustomerContact(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.CustomerContactId, objectType, operations);

        private void CustomOnPreSetup()
        {
            var m = this.transaction.Database.Services.Get<MetaPopulation>();

            var security = new Security(this.transaction);

            var full = new[] { Operations.Read, Operations.Write, Operations.Execute };

            foreach (ObjectType @class in this.transaction.Database.MetaPopulation.DatabaseClasses)
            {
                security.GrantAdministrator(@class, full);
                security.Grantemployee(@class, Operations.Read);
                security.GrantCreator(@class, full);

                if (@class.Equals(m.Currency) ||
                    @class.Equals(m.Locale) ||
                    @class.Equals(m.UnitOfMeasure) ||
                    @class.Equals(m.GenderType) ||
                    @class.Equals(m.Hobby) ||
                    @class.Equals(m.PersonalTitle) ||
                    @class.Equals(m.MaritalStatus) ||
                    @class.Equals(m.Salutation) ||
                    @class.Equals(m.SerialisedItemCharacteristicType) ||
                    @class.Equals(m.WorkEffortType) ||
                    @class.Equals(m.WorkEffortState) ||
                    @class.Equals(m.WorkEffortPurpose))
                {
                    security.GrantCustomerContact(@class, Operations.Read);
                }
                else if (@class.Equals(m.Catalogue) ||
                    @class.Equals(m.ProductCategory) ||
                    @class.Equals(m.Brand) ||
                    @class.Equals(m.Model) ||
                    @class.Equals(m.UnifiedGood))
                {
                    security.GrantCustomerContact(@class, Operations.Read);
                }
                else if (@class.Equals(m.WorkTask))
                {
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.WorkEffortNumber, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.SortableWorkEffortNumber, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.TakenBy, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.ExecutedBy, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.WorkDone, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.WorkEffortPurposes, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.WorkEffortType, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.WorkEffortState, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.ActualStart, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.ActualCompletion, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.ScheduledStart, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.ScheduledCompletion, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.ActualHours, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.EstimatedHours, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.Comment, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.LastModifiedBy, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.LastModifiedDate, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.CreatedBy, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.CreationDate, Operations.Read);

                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.Customer, Operations.Read, Operations.Write);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.Name, Operations.Read, Operations.Write);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.Description, Operations.Read, Operations.Write);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.FullfillContactMechanism, Operations.Read, Operations.Write);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.ContactPerson, Operations.Read, Operations.Write);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.ContactPerson, Operations.Read, Operations.Write);
                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.PublicElectronicDocuments, Operations.Read, Operations.Write);

                    security.Grant(Roles.CustomerContactId, @class, m.WorkTask.Print, Operations.Execute);
                }
                else if (@class.Equals(m.Person))
                {
                    security.Grant(Roles.OwnerId, @class, m.Person.FirstName, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.MiddleName, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.LastName, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Salutation, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Titles, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.BirthDate, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.MothersMaidenName, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Height, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Gender, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Weight, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Hobbies, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.MaritalStatus, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Picture, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.EmailFrequency, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.SocialSecurityNumber, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Citizenship, Operations.Read, Operations.Write);
                }
                else if (@class.Equals(m.Organisation))
                {
                    security.Grant(Roles.CustomerContactId, @class, m.Organisation.Name, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.Organisation.DisplayName, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.Organisation.CurrentContacts, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.Organisation.CurrentOrganisationContactRelationships, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.Organisation.CurrentPartyContactMechanisms, Operations.Read);
                    security.Grant(Roles.CustomerContactId, @class, m.Organisation.CurrentPartyRelationships, Operations.Read);
                }
                else if (@class.Equals(m.OrganisationContactRelationship) ||
                        @class.Equals(m.PartyContactMechanism) ||
                        @class.Equals(m.PostalAddress) ||
                        @class.Equals(m.EmailAddress) ||
                        @class.Equals(m.TelecommunicationsNumber) ||
                        @class.Equals(m.WebAddress) ||
                        @class.Equals(m.Media) ||
                        @class.Equals(m.MediaContent) ||
                        @class.Equals(m.LocalisedMedia) ||
                        @class.Equals(m.LocalisedText))
                {
                    security.GrantCustomerContact(@class, Operations.Read, Operations.Write);
                }
            }
        }

        private void CustomOnPostSetup()
        {
        }
    }
}
