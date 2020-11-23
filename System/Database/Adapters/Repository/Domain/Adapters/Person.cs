// <copyright file="Person.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("6a082a25-a8f2-4acd-a1a3-ba4461b729f1")]
    #endregion
    public partial class Person : System.Object, Named
    {
        #region inherited properties
        public string Name { get; set; }

        public int Index { get; set; }

        #endregion

        #region Allors
        [Id("25ff791d-9547-41ba-ac34-f2fe501ef217")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public Person NextPerson { get; set; }

        #region Allors
        [Id("6cc83cb8-cb94-4716-bb7d-e25201f06b20")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        public Company Company { get; set; }

        #region inherited methods
        public void InheritedDoIt()
        {
        }
        #endregion
    }
}
