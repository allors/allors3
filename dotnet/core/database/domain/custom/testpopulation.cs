// <copyright file="TestPopulation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public class TestPopulation
    {
        private readonly ITransaction transaction;

        public TestPopulation(ITransaction transaction) => this.transaction = transaction;

        public void Apply()
        {
            new PersonBuilder(this.transaction).WithUserName("noacl").WithFirstName("no").WithLastName("acl").Build();

            var noperm = new PersonBuilder(this.transaction).WithUserName("noperm").WithFirstName("no").WithLastName("perm").Build();
            var emptyRole = new RoleBuilder(this.transaction).WithName("Empty").Build();
            var defaultSecurityToken = new SecurityTokens(this.transaction).DefaultSecurityToken;

            var acl = new GrantBuilder(this.transaction).WithRole(emptyRole).WithSubject(noperm).WithSecurityToken(defaultSecurityToken).Build();

            var c1A = new C1Builder(this.transaction).WithName("c1A").WithOrder(4).Build();
            var c1B = new C1Builder(this.transaction).WithName("c1B").WithOrder(3).Build();
            var c1C = new C1Builder(this.transaction).WithName("c1C").WithOrder(8).Build();
            var c1D = new C1Builder(this.transaction).WithName("c1D").WithOrder(7).Build();
            var c2A = new C2Builder(this.transaction).WithName("c2A").WithOrder(5).Build();
            var c2B = new C2Builder(this.transaction).WithName("c2B").WithOrder(6).Build();
            var c2C = new C2Builder(this.transaction).WithName("c2C").WithOrder(2).Build();
            var c2D = new C2Builder(this.transaction).WithName("c2D").WithOrder(1).Build();

            // class
            c1B.C1AllorsString = "ᴀbra";
            c1C.C1AllorsString = "ᴀbracadabra";
            c1D.C1AllorsString = "ᴀbracadabra";

            c2B.C2AllorsString = "ᴀbra";
            c2C.C2AllorsString = "ᴀbracadabra";
            c2D.C2AllorsString = "ᴀbracadabra";
            // exclusive interface
            c1B.I1AllorsString = "ᴀbra";
            c1C.I1AllorsString = "ᴀbracadabra";
            c1D.I1AllorsString = "ᴀbracadabra";

            // shared interface
            c1B.I12AllorsString = "ᴀbra";
            c1C.I12AllorsString = "ᴀbracadabra";
            c1D.I12AllorsString = "ᴀbracadabra";
            c2B.I12AllorsString = "ᴀbra";
            c2C.I12AllorsString = "ᴀbracadabra";
            c2D.I12AllorsString = "ᴀbracadabra";

            c1B.C1AllorsStringEquals = "bra";
            c1C.C1AllorsStringEquals = "ᴀbracadabra";
            c1D.C1AllorsStringEquals = "bracadabra";

            // Boolean
            c1B.C1AllorsBoolean = true;
            c1C.C1AllorsBoolean = false;
            c1D.C1AllorsBoolean = false;

            c1B.I1AllorsBoolean = true;
            c1C.I1AllorsBoolean = false;
            c1D.I1AllorsBoolean = false;

            c1B.I12AllorsBoolean = true;
            c1C.I12AllorsBoolean = false;
            c1D.I12AllorsBoolean = false;
            c2B.I12AllorsBoolean = true;
            c2C.I12AllorsBoolean = false;
            c2D.I12AllorsBoolean = false;

            // DateTime
            c1B.C1AllorsDateTime = new DateTime(2000, 1, 1, 0, 0, 4, DateTimeKind.Utc);
            c1C.C1AllorsDateTime = new DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc);
            c1D.C1AllorsDateTime = new DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc);

            c1B.I1AllorsDateTime = new DateTime(2000, 1, 1, 0, 0, 4, DateTimeKind.Utc);
            c1C.I1AllorsDateTime = new DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc);
            c1D.I1AllorsDateTime = new DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc);

            c1B.I12AllorsDateTime = new DateTime(2000, 1, 1, 0, 0, 4, DateTimeKind.Utc);
            c1C.I12AllorsDateTime = new DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc);
            c1D.I12AllorsDateTime = new DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc);
            c2B.I12AllorsDateTime = new DateTime(2000, 1, 1, 0, 0, 4, DateTimeKind.Utc);
            c2C.I12AllorsDateTime = new DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc);
            c2D.I12AllorsDateTime = new DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc);

            c1B.C1DateTimeLessThan = new DateTime(2000, 1, 1, 0, 0, 3, DateTimeKind.Utc);
            c1C.C1DateTimeLessThan = new DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc);
            c1D.C1DateTimeLessThan = new DateTime(2000, 1, 1, 0, 0, 7, DateTimeKind.Utc);

            c1B.C1DateTimeGreaterThan = new DateTime(2000, 1, 1, 0, 0, 3, DateTimeKind.Utc);
            c1C.C1DateTimeGreaterThan = new DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc);
            c1D.C1DateTimeGreaterThan = new DateTime(2000, 1, 1, 0, 0, 7, DateTimeKind.Utc);

            c1B.C1DateTimeBetweenA = new DateTime(2000, 1, 1, 0, 0, 1, DateTimeKind.Utc);
            c1B.C1DateTimeBetweenB = new DateTime(2000, 1, 1, 0, 0, 3, DateTimeKind.Utc);
            c1C.C1DateTimeBetweenA = new DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc);
            c1C.C1DateTimeBetweenB = new DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc);
            c1D.C1DateTimeBetweenA = new DateTime(2000, 1, 1, 0, 0, 3, DateTimeKind.Utc);
            c1D.C1DateTimeBetweenB = new DateTime(2000, 1, 1, 0, 0, 10, DateTimeKind.Utc);

            // Decimal
            c1B.C1AllorsDecimal = 1.1M;
            c1C.C1AllorsDecimal = 2.2M;
            c1D.C1AllorsDecimal = 2.2M;

            c1B.I1AllorsDecimal = 1.1M;
            c1C.I1AllorsDecimal = 2.2M;
            c1D.I1AllorsDecimal = 2.2M;

            c1B.I12AllorsDecimal = 1.1M;
            c1C.I12AllorsDecimal = 2.2M;
            c1D.I12AllorsDecimal = 2.2M;
            c2B.I12AllorsDecimal = 1.1M;
            c2C.I12AllorsDecimal = 2.2M;
            c2D.I12AllorsDecimal = 2.2M;

            c1B.C1DecimalLessThan = 0;
            c1C.C1DecimalLessThan = 2;
            c1D.C1DecimalLessThan = 4;

            c1B.C1DecimalGreaterThan = 0;
            c1C.C1DecimalGreaterThan = 2;
            c1D.C1DecimalGreaterThan = 4;

            c1B.C1DecimalBetweenA = -10;
            c1B.C1DecimalBetweenB = 0;
            c1C.C1DecimalBetweenA = 2;
            c1C.C1DecimalBetweenB = 2;
            c1D.C1DecimalBetweenA = 0;
            c1D.C1DecimalBetweenB = 10;

            // Double
            c1B.C1AllorsDouble = 1.1d;
            c1C.C1AllorsDouble = 2.2d;
            c1D.C1AllorsDouble = 2.2d;

            c1B.I1AllorsDouble = 1.1d;
            c1C.I1AllorsDouble = 2.2d;
            c1D.I1AllorsDouble = 2.2d;

            c1B.I12AllorsDouble = 1.1d;
            c1C.I12AllorsDouble = 2.2d;
            c1D.I12AllorsDouble = 2.2d;
            c2B.I12AllorsDouble = 1.1d;
            c2C.I12AllorsDouble = 2.2d;
            c2D.I12AllorsDouble = 2.2d;

            c1B.C1DoubleLessThan = 0;
            c1C.C1DoubleLessThan = 2;
            c1D.C1DoubleLessThan = 4;

            c1B.C1DoubleGreaterThan = 0;
            c1C.C1DoubleGreaterThan = 2;
            c1D.C1DoubleGreaterThan = 4;

            c1B.C1DoubleBetweenA = -10d;
            c1B.C1DoubleBetweenB = 0d;
            c1C.C1DoubleBetweenA = 2d;
            c1C.C1DoubleBetweenB = 2d;
            c1D.C1DoubleBetweenA = 0d;
            c1D.C1DoubleBetweenB = 10d;

            // Integer
            c1B.C1AllorsInteger = 1;
            c1C.C1AllorsInteger = 2;
            c1D.C1AllorsInteger = 2;

            c1B.I1AllorsInteger = 1;
            c1C.I1AllorsInteger = 2;
            c1D.I1AllorsInteger = 2;

            c1B.I12AllorsInteger = 1;
            c1C.I12AllorsInteger = 2;
            c1D.I12AllorsInteger = 2;
            c2B.I12AllorsInteger = 1;
            c2C.I12AllorsInteger = 2;
            c2D.I12AllorsInteger = 2;

            c1B.C1IntegerLessThan = 0;
            c1C.C1IntegerLessThan = 2;
            c1D.C1IntegerLessThan = 4;

            c1B.C1IntegerGreaterThan = 0;
            c1C.C1IntegerGreaterThan = 2;
            c1D.C1IntegerGreaterThan = 4;

            c1B.C1IntegerBetweenA = -10;
            c1B.C1IntegerBetweenB = 0;
            c1C.C1IntegerBetweenA = 2;
            c1C.C1IntegerBetweenB = 2;
            c1D.C1IntegerBetweenA = 0;
            c1D.C1IntegerBetweenB = 10;

            // Unique
            c1B.C1AllorsUnique = new Guid("8B3C4978-72D3-40BA-B302-114EB331FE04");
            c1C.C1AllorsUnique = new Guid("0FD4EC2C-08DB-46B9-B71A-10152EBE4569");
            c1D.C1AllorsUnique = new Guid("AF01C994-379A-449A-8C4D-8D3B7207EC91");

            c1B.I1AllorsUnique = new Guid("7F7BF8EF-DDF2-47E6-B33F-627BE7DEAD6D");
            c1C.I1AllorsUnique = new Guid("08BFF7DE-51B9-4A53-BFBA-1212F23A342D");
            c1D.I1AllorsUnique = new Guid("59A349CB-E197-43B7-A17E-90630A87873C");

            c1B.I12AllorsUnique = new Guid("FF240B61-52A1-42E3-8FB5-A2CFB243E4E8");
            c1C.I12AllorsUnique = new Guid("8E3C8B4D-C945-4814-9FED-F68E632ECF0A");
            c1D.I12AllorsUnique = new Guid("F0C49BFF-4B3A-48E8-A354-6201E8E20A00");
            c2B.I12AllorsUnique = new Guid("0D2FEA7B-549C-466D-91E8-92427309F83A");
            c2C.I12AllorsUnique = new Guid("3E94AA78-2101-42E9-87AC-87563B792DED");
            c2D.I12AllorsUnique = new Guid("5E12BE66-D083-450E-9AF3-C37A354701F8");

            // One2One
            c1A.C1C1One2One = c1B;
            c1B.C1C1One2One = c1C;
            c1C.C1C1One2One = c1D;

            c1B.C1C2One2One = c2B;
            c1C.C1C2One2One = c2C;
            c1D.C1C2One2One = c2D;

            c1B.I1I2One2One = c2B;
            c1C.I1I2One2One = c2C;
            c1D.I1I2One2One = c2D;

            c1B.I12C2One2One = c2B;
            c1C.I12C2One2One = c2C;
            c1D.I12C2One2One = c2D;
            c2A.I12C2One2One = c2A;

            c1B.C1I12One2One = c1B;
            c1C.C1I12One2One = c2B;
            c1D.C1I12One2One = c2C;

            c1B.I12I12One2One = c2B;
            c1C.I12I12One2One = c2C;
            c1D.I12I12One2One = c1D;
            c2B.I12I12One2One = c1B;
            c2C.I12I12One2One = c1C;
            c2D.I12I12One2One = c2D;

            c2A.C2C2One2One = c2B;
            c2B.C2C2One2One = c2C;
            c2D.C2C2One2One = c2D;

            // One2Many
            c1B.AddC1C1One2Many(c1B);
            c1C.AddC1C1One2Many(c1C);
            c1C.AddC1C1One2Many(c1D);

            c1B.AddC1C2One2Many(c2B);
            c1C.AddC1C2One2Many(c2C);
            c1C.AddC1C2One2Many(c2D);

            c1B.AddI1I2One2Many(c2B);
            c1C.AddI1I2One2Many(c2C);
            c1C.AddI1I2One2Many(c2D);

            c1B.AddC1I12One2Many(c1B);
            c1C.AddC1I12One2Many(c2C);
            c1C.AddC1I12One2Many(c2D);

            c2A.AddC2C2One2Many(c2B);
            c2B.AddC2C2One2Many(c2C);
            c2C.AddC2C2One2Many(c2D);

            // Many2One
            c1B.C1C1Many2One = c1B;
            c1C.C1C1Many2One = c1C;
            c1D.C1C1Many2One = c1C;

            c1B.C1C2Many2One = c2B;
            c1C.C1C2Many2One = c2C;
            c1D.C1C2Many2One = c2C;

            c1B.I1I2Many2One = c2B;
            c1C.I1I2Many2One = c2C;
            c1D.I1I2Many2One = c2C;

            c1B.I12C2Many2One = c2B;
            c2C.I12C2Many2One = c2C;
            c2D.I12C2Many2One = c2C;

            c1B.C1I12Many2One = c1B;
            c1C.C1I12Many2One = c2C;
            c1D.C1I12Many2One = c2C;

            c2A.C2C2Many2One = c2B;
            c2B.C2C2Many2One = c2C;
            c2C.C2C2Many2One = c2D;

            // Many2Many
            c1B.AddC1C1Many2Many(c1B);
            c1C.AddC1C1Many2Many(c1B);
            c1D.AddC1C1Many2Many(c1B);
            c1C.AddC1C1Many2Many(c1C);
            c1D.AddC1C1Many2Many(c1C);
            c1D.AddC1C1Many2Many(c1D);

            c1B.AddC1C2Many2Many(c2B);
            c1C.AddC1C2Many2Many(c2B);
            c1D.AddC1C2Many2Many(c2B);
            c1C.AddC1C2Many2Many(c2C);
            c1D.AddC1C2Many2Many(c2C);
            c1D.AddC1C2Many2Many(c2D);

            c1B.AddI1I2Many2Many(c2B);
            c1C.AddI1I2Many2Many(c2B);
            c1C.AddI1I2Many2Many(c2C);
            c1D.AddI1I2Many2Many(c2B);
            c1D.AddI1I2Many2Many(c2C);
            c1D.AddI1I2Many2Many(c2D);

            c1B.AddI12C2Many2Many(c2B);
            c1C.AddI12C2Many2Many(c2B);
            c1C.AddI12C2Many2Many(c2C);
            c1D.AddI12C2Many2Many(c2B);
            c1D.AddI12C2Many2Many(c2C);
            c1D.AddI12C2Many2Many(c2D);
            c2A.AddI12C2Many2Many(c2A);
            c2A.AddI12C2Many2Many(c2B);
            c2A.AddI12C2Many2Many(c2C);
            c2A.AddI12C2Many2Many(c2D);

            c1B.AddC1I12Many2Many(c1B);
            c1B.AddC1I12Many2Many(c2B);
            c1C.AddC1I12Many2Many(c2B);
            c1C.AddC1I12Many2Many(c2C);
            c1D.AddC1I12Many2Many(c2B);
            c1D.AddC1I12Many2Many(c2C);
            c1D.AddC1I12Many2Many(c2D);

            c2A.AddC2C2Many2Many(c2B);
            c2B.AddC2C2Many2Many(c2C);
            c2C.AddC2C2Many2Many(c2D);

            this.transaction.Derive();
        }
    }
}
