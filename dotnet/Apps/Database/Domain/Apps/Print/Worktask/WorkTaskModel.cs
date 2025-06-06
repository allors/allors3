// <copyright file="WorkTaskModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.WorkTaskModel
{
    using System.Globalization;
    using System.Linq;

    public class WorkTaskModel
    {
        public WorkTaskModel(WorkTask workTask)
        {
            this.Number = workTask.WorkEffortNumber;
            this.Name = workTask.Name;
            this.Description = workTask.Description?.Split('\n');
            this.WorkDone = workTask.WorkDone?.Split('\n');

            this.Date = (workTask.ThroughDate() ?? workTask.Strategy.Transaction.Now()).ToString("yyyy-MM-dd");
            this.Purpose = string.Join(", ", workTask.WorkEffortPurposes.Select(v => v.Name));
            this.Facility = workTask.Facility?.Name;
            this.ContactName = workTask.ContactPerson?.DisplayName;
            this.ContactTelephone = workTask.ContactPerson?.CellPhoneNumber?.Description ?? workTask.ContactPerson?.GeneralPhoneNumber?.Description;

            this.TotalLabour = workTask.TotalLabourRevenue.ToString("N2", new CultureInfo("nl-BE"));

            this.TotalParts = workTask.TotalMaterialRevenue.ToString("N2", new CultureInfo("nl-BE"));

            this.TotalSubContractor = workTask.TotalSubContractedRevenue.ToString("N2", new CultureInfo("nl-BE"));

            this.TotalOther = workTask.TotalOtherRevenue.ToString("N2", new CultureInfo("nl-BE"));

            this.Total = workTask.GrandTotal.ToString("N2", new CultureInfo("nl-BE"));
        }

        public string Number { get; }

        public string Name { get; }

        public string[] Description { get; }

        public string[] WorkDone { get; }

        public string Purpose { get; }

        public string Date { get; }

        public string PurchaseOrder { get; }

        public string ContactName { get; }

        public string ContactTelephone { get; }

        public string PaymentTerms { get; }

        public string Facility { get; }

        public string TotalLabour { get; }

        public string TotalParts { get; }

        public string TotalSubContractor { get; }

        public string TotalOther { get; }

        public string Total { get; }
    }
}
