//-------------------------------------------------------------------------------------------------
// <copyright file="Default.cs" company="Allors bv">
// Copyright 2002-2012 Allors bv.
//
// Dual Licensed under
//   a) the Lesser General Public Licence v3 (LGPL)
//   b) the Allors License
//
// The LGPL License is included in the file lgpl.txt.
// The Allors License is an addendum to your contract.
//
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// For more information visit http://www.allors.com/legal
// </copyright>
// <summary>Defines the Default type.</summary>
//------------------------------------------------------------------------------------------------

namespace Allors.Database.Adapters.Memory
{
    using Meta;

    public class ServicesTest : Adapters.ServicesTest
    {
        private readonly Profile profile = new Profile();

        public ServicesTest() => this.profile.Init();

        public override void Dispose() => this.profile.Dispose();

        public override IObject[] CreateArray(IObjectType objectType, int count) => this.profile.CreateArray(objectType, count);

        public override IDatabase CreateMemoryPopulation() => this.profile.CreateMemoryDatabase();

        public override MetaPopulation GetMetaPopulation() => (MetaPopulation)this.profile.GetDatabase().MetaPopulation;

        public override MetaPopulation GetMetaPopulation2() => (MetaPopulation)this.profile.GetDatabase2().MetaPopulation;

        public override IDatabase GetPopulation() => this.profile.GetDatabase();

        public override IDatabase GetPopulation2() => this.profile.GetDatabase2();

        public override ITransaction GetTransaction() => this.profile.GetTransaction();

        public override ITransaction GetTransaction2() => this.profile.GetTransaction2();

        public override bool IsRollbackSupported() => this.profile.IsRollbackSupported();
    }
}
