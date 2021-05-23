// <copyright file="Company.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("b1b6361e-5ee5-434c-9c92-46c6166195c4")]
    #endregion
    public partial class Company : System.Object, Named
    {
        #region inherited properties
        public string Name { get; set; }

        public int Index { get; set; }

        #endregion

        #region Allors
        [Id("08ab248d-bdb1-49c5-a2da-d6485f49239f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public Person Manager { get; set; }

        #region Allors
        [Id("1a4087de-f116-4f79-9441-31faee8054f3")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        public Person[] Employees { get; set; }

        #region Allors
        [Id("28021756-f15f-4671-aa01-a40d3707d61a")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public Person FirstPerson { get; set; }

        #region Allors
        [Id("2f9fc05e-c904-4056-83f0-a7081762594a")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public Named[] NamedsOneSort2 { get; set; }

        #region Allors
        [Id("44abca14-9fb2-42a7-b8ab-a1ca87d87b2e")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        #endregion
        public Person[] Owners { get; set; }

        #region Allors
        [Id("509c5341-3d87-4da4-a807-5567d897169b")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        #endregion
        public Person[] IndexedMany2ManyPersons { get; set; }

        #region Allors
        [Id("62b4ddac-efd7-4fc9-bbed-91c831a62f01")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public Person[] PersonsOneSort1 { get; set; }

        #region Allors
        [Id("64c1be0a-0636-4da0-8404-2a93ab600cd9")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public Person[] PersonsManySort1 { get; set; }

        #region Allors
        [Id("996d27ff-3615-4a51-9214-944fac566a11")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public Named[] NamedsManySort1 { get; set; }

        #region Allors
        [Id("a9f60154-6bd1-4c76-94eb-edfd5beb6749")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public Person[] PersonsManySort2 { get; set; }

        #region Allors
        [Id("bdf71d38-8082-4a99-9636-4f4ec26fd45c")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public Person[] PersonsOneSort2 { get; set; }

        #region Allors
        [Id("c1f68661-4999-4851-9224-1878258b6a58")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public Named NamedManySort2 { get; set; }

        #region Allors
        [Id("c53bdaea-c0a5-4179-bfbb-e12de45e2ae0")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public Person[] Many2ManyPersons { get; set; }

        #region Allors
        [Id("cde0a8e7-1a14-4f1a-a0ca-a305f0548df8")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public Company[] Children { get; set; }

        #region Allors
        [Id("cdf04399-aa37-4ea2-9ac8-bf6d19884933")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public Named[] NamedsOneSort1 { get; set; }

        #region inherited methods
        public void InheritedDoIt()
        {
        }
        #endregion
    }
}
