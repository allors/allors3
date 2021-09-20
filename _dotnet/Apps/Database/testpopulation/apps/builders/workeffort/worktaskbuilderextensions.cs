// <copyright file="WorkTaskBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Collections.Generic;
    using System.Linq;

    public static partial class WorkTaskBuilderExtensions
    {
        public static WorkTaskBuilder WithScheduledWorkForExternalCustomer(this WorkTaskBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var customer = internalOrganisation.ActiveCustomers.FirstOrDefault(v => v.GetType().Name == nameof(Organisation));

            @this.WithTakenBy(internalOrganisation)
                .WithExecutedBy(internalOrganisation)
                .WithCustomer(customer)
                .WithFacility(faker.Random.ListItem(internalOrganisation.FacilitiesWhereOwner.ToArray()))
                .WithContactPerson(customer.CurrentContacts.FirstOrDefault())
                .WithName(string.Join(" ", faker.Lorem.Words(3)))
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithWorkDone(faker.Lorem.Sentence())
                .WithPriority(faker.Random.ListItem(@this.Transaction.Extent<Priority>()))
                .WithWorkEffortPurpose(faker.Random.ListItem(@this.Transaction.Extent<WorkEffortPurpose>()))
                .WithScheduledStart(@this.Transaction.Now().AddDays(7))
                .WithScheduledCompletion(@this.Transaction.Now().AddDays(10))
                .WithEstimatedHours(faker.Random.Int(7, 30))
                .WithElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc1.en.pdf").WithInData(faker.Random.Bytes(1000)).Build());

            return @this;
        }

        public static WorkTaskBuilder WithScheduledInternalWork(this WorkTaskBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var otherInternalOrganization = @this.Transaction.Extent<Organisation>().Except(new List<Organisation> { internalOrganisation }).FirstOrDefault();

            @this.WithTakenBy(internalOrganisation)
                .WithExecutedBy(internalOrganisation)
                .WithCustomer(otherInternalOrganization)
                .WithFacility(faker.Random.ListItem(internalOrganisation.FacilitiesWhereOwner.ToArray()))
                .WithName(string.Join(" ", faker.Lorem.Words(3)))
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithWorkDone(faker.Lorem.Sentence())
                .WithPriority(faker.Random.ListItem(@this.Transaction.Extent<Priority>()))
                .WithWorkEffortPurpose(faker.Random.ListItem(@this.Transaction.Extent<WorkEffortPurpose>()))
                .WithScheduledStart(@this.Transaction.Now().AddDays(7))
                .WithScheduledCompletion(@this.Transaction.Now().AddDays(10))
                .WithEstimatedHours(faker.Random.Int(7, 30))
                .WithElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc1.en.pdf").WithInData(faker.Random.Bytes(1000)).Build());

            return @this;
        }

        // WithScheduledWorkForSubcontractedWork
        // WithWorkStartedForExternalCustomer
        // WithWorkStartedForInternalWork
        // WithWorkStartedForSubcontractedWork
    }
}
