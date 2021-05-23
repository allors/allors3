// <copyright file="ContactMechanismPurpose.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class ContactMechanismPurpose
    {
        public bool IsHeadQuarters => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).HeadQuarters);

        public bool IsSalesOffice => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).SalesOffice);

        public bool IsHomeAddress => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).HomeAddress);

        public bool IsGeneralPhoneNumber => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).GeneralPhoneNumber);

        public bool IsGeneralFaxNumber => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).GeneralFaxNumber);

        public bool IsGeneralEmail => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).GeneralEmail);

        public bool IsGeneralCorrespondence => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).GeneralCorrespondence);

        public bool IsBillingAddress => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).BillingAddress);

        public bool IsInternetAddress => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).InternetAddress);

        public bool IsOrderAddress => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).OrderAddress);

        public bool IsShippingAddress => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).ShippingAddress);

        public bool IsBillingInquiriesPhone => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).BillingInquiriesPhone);

        public bool IsOrderInquiriesPhone => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).OrderInquiriesPhone);

        public bool IsShippingInquiriesPhone => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).ShippingInquiriesPhone);

        public bool IsBillingInquiriesFax => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).BillingInquiriesFax);

        public bool IsOrderInquiriesFax => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).OrderInquiriesFax);

        public bool IsShippingInquiriesFax => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).ShippingInquiriesFax);

        public bool IsPersonalEmailAddress => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).PersonalEmailAddress);

        public bool IsCellPhoneNumber => this.Equals(new ContactMechanismPurposes(this.Strategy.Transaction).MobilePhoneNumber);
    }
}
