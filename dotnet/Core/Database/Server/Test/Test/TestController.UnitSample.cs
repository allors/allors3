// <copyright file="TestController.UnitSample.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System;
    using Database.Domain;
    using MetaPopulation = Allors.Database.Meta.MetaPopulation;
    using Database.Services;
    using Microsoft.AspNetCore.Mvc;

    public partial class TestController
    {
        [HttpPost]
        public IActionResult UnitSample([FromBody] UnitSampleRequest request)
        {
            try
            {
                using var transaction = this.Database.CreateTransaction();
                var m = this.Database.Services.Get<MetaPopulation>();
                transaction.Services.Get<IUserService>().User = new Users(transaction).FindBy(m.User.UniqueId, Users.AdministratorId);
                var unitSample = new UnitSamples(transaction).Extent().First;
                if (unitSample == null)
                {
                    unitSample = new UnitSampleBuilder(transaction).Build();
                    transaction.Commit();
                }

                switch (request.Step)
                {
                    case 0:
                        unitSample.RemoveAllorsBinary();
                        unitSample.RemoveAllorsBoolean();
                        unitSample.RemoveAllorsDateTime();
                        unitSample.RemoveAllorsDecimal();
                        unitSample.RemoveAllorsDouble();
                        unitSample.RemoveAllorsInteger();
                        unitSample.RemoveAllorsString();
                        unitSample.RemoveAllorsUnique();

                        break;

                    case 1:
                        unitSample.AllorsBinary = new byte[] { 1, 2, 3 };
                        unitSample.AllorsBoolean = true;
                        unitSample.AllorsDateTime = new DateTime(1973, 3, 27, 0, 0, 0, DateTimeKind.Utc);
                        unitSample.AllorsDecimal = 12.34m;
                        unitSample.AllorsDouble = 123d;
                        unitSample.AllorsInteger = 1000;
                        unitSample.AllorsString = "a string";
                        unitSample.AllorsUnique = new Guid("2946CF37-71BE-4681-8FE6-D0024D59BEFF");

                        break;
                }

                transaction.Commit();

                return this.Ok(new { id = unitSample.Id });
            }
            catch (Exception e)
            {
                this.LogActionFailed(e, nameof(this.UnitSample));
                return this.BadRequest(e.Message);
            }
        }

        public class UnitSampleRequest
        {
            public int Step { get; set; }
        }
    }
}
