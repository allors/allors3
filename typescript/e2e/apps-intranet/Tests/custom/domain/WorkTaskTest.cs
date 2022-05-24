// <copyright file="PersonEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Objects
{
    using System;
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.E2E.Angular;
    using Allors.E2E.Angular.Html;
    using Allors.E2E.Angular.Material.Factory;
    using Allors.E2E.Test;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class WorkTaskTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateMinimal()
        {
            var before = new WorkTasks(this.Transaction).Extent().ToArray();
            var name = "WorkTask 1";
            var customer = new Organisations(this.Transaction).Extent().First();

            var @class = this.M.WorkTask;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new WorktaskCreateFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync(name);
            await form.CustomerAutocomplete.SelectAsync(customer.Name);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new WorkTasks(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var workTask = after.Except(before).First();

            Assert.AreEqual(name, workTask.Name);
            Assert.AreEqual(customer, workTask.Customer);
        }

        [Test]
        public async Task CreateMaximum()
        {
            var before = new WorkTasks(this.Transaction).Extent().ToArray();
            var name = "WorkTask 1";
            var customer = new Organisations(this.Transaction).Extent().First(v => v.Name == "Allors BV");
            var executedBy = customer.ActiveSubContractors.First();
            var fullfillContactMechanism = customer.CurrentPartyContactMechanisms.First().ContactMechanism;
            var contactPerson = customer.CurrentContacts.First();
            var description = "WorkTask 1 description";

            var now = DateTime.Now;

            var issueDate = DateTimeFactory.CreateDate(now);
            var scheduledStart = DateTimeFactory.CreateDateTime(now.Year, now.Month, now.Day + 1, now.Hour, now.Minute, 0, 0);
            var scheduledCompletion = DateTimeFactory.CreateDateTime(now.Year, now.Month, now.Day + 2, now.Hour, now.Minute, 0, 0);

            var @class = this.M.WorkTask;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new WorktaskCreateFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync(name);
            await form.CustomerAutocomplete.SelectAsync(customer.Name);
            await form.ExecutedByAutocomplete.SelectAsync(executedBy.Name);
            await form.FullfillContactMechanismSelect.SelectAsync(fullfillContactMechanism);
            await form.ContactPersonSelect.SelectAsync(contactPerson);
            await form.DescriptionTextarea.SetAsync(description);
            await form.IssueDateDatepicker.SetAsync(issueDate);
            await form.ScheduledStartDatetimepicker.SetAsync(scheduledStart);
            await form.ScheduledCompletionDatetimepicker.SetAsync(scheduledCompletion);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new WorkTasks(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var workTask = after.Except(before).First();

            Assert.AreEqual(name, workTask.Name);
            Assert.AreEqual(customer, workTask.Customer);
            Assert.AreEqual(executedBy, workTask.ExecutedBy);
            Assert.AreEqual(fullfillContactMechanism, workTask.FullfillContactMechanism);
            Assert.AreEqual(contactPerson, workTask.ContactPerson);
            Assert.AreEqual(description, workTask.Description);
            Assert.AreEqual(issueDate, workTask.IssueDate.Value.Date);
            Assert.AreEqual(scheduledStart, workTask.ScheduledStart?.ToLocalTime());
            Assert.AreEqual(scheduledCompletion, workTask.ScheduledCompletion?.ToLocalTime());
        }
    }
}
