// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class PersonBuilderExtensions
    {
        public static PersonBuilder WithDefaults(this PersonBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            var emailAddress = new EmailAddressBuilder(@this.Transaction).WithDefaults().Build();
            var email = emailAddress.ElectronicAddressString;

            @this.WithFirstName(faker.Name.FirstName())
                .WithLastName(faker.Name.LastName())
                .WithSalutation(faker.Random.ListItem(@this.Transaction.Extent<Salutation>()))
                .WithGender(faker.Random.ListItem(@this.Transaction.Extent<GenderType>()))
                .WithTitle(faker.Random.ListItem(@this.Transaction.Extent<PersonalTitle>()))
                .WithLocale(@this.Transaction.GetSingleton().DefaultLocale)
                .WithPicture(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(200, 200)).Build())
                .WithComment(faker.Lorem.Paragraph())
                .WithUserName(email);

            foreach (var additionalLocale in @this.Transaction.GetSingleton().AdditionalLocales)
            {
                @this.WithLocalisedComment(new LocalisedTextBuilder(@this.Transaction).WithLocale(additionalLocale).WithText(faker.Lorem.Paragraph()).Build());
            }

            return @this;
        }
    }
}
