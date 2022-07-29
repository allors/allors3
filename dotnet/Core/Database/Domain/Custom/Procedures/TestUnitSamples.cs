// <copyright file="TestUnitSamplesController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public class TestUnitSamples : IProcedure
    {
        public void Execute(IProcedureContext context, IProcedureInput input, IProcedureOutput output)
        {
            var step = input.GetValueAsInteger("step");

            var unitSample = new UnitSamples(context.Transaction).Extent().First;
            if (unitSample == null)
            {
                unitSample = new UnitSampleBuilder(context.Transaction).Build();
                context.Transaction.Commit();
            }

            switch (step)
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

            context.Transaction.Commit();

            output.AddObject("unitSample", unitSample);
        }
    }
}
