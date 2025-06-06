// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceProxyTest.cs" company="Allors bv">
//   Copyright 2002-2012 Allors bv.
// Dual Licensed under
//   a) the Lesser General Public Licence v3 (LGPL)
//   b) the Allors License
// The LGPL License is included in the file lgpl.txt.
// The Allors License is an addendum to your contract.
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Adapters
{
    using Meta;

    public abstract class ReferenceProxyTest : ReferenceTest
    {
        private readonly ReferenceTest subject;

        protected ReferenceProxyTest(ReferenceTest referenceTest) => this.subject = referenceTest;

        public override void Dispose() => this.subject.Dispose();

        public override IObject[] CreateArray(IObjectType objectType, int count) => this.subject.CreateArray(objectType, count);

        public override IDatabase CreateMemoryPopulation() => this.subject.CreateMemoryPopulation();

        public override int[] GetAssertRepeats() => this.subject.GetAssertRepeats();

        public override int GetAssociationCount() => this.subject.GetAssociationCount();

        public override bool[] GetBooleanFlags() => this.subject.GetBooleanFlags();

        public override MetaPopulation GetMetaPopulation() => this.subject.GetMetaPopulation();

        public override MetaPopulation GetMetaPopulation2() => this.subject.GetMetaPopulation2();

        public override IClass GetMetaType(IObject allorsObject) => this.subject.GetMetaType(allorsObject);

        public override IDatabase GetPopulation() => this.subject.GetPopulation();

        public override IDatabase GetPopulation2() => this.subject.GetPopulation2();

        public override int[] GetRepeats() => this.subject.GetRepeats();

        public override int GetRoleCount() => this.subject.GetRoleCount();

        public override int GetRoleGroupCount() => this.subject.GetRoleGroupCount();

        public override int GetRolesPerGroup() => this.subject.GetRolesPerGroup();

        public override ITransaction GetTransaction() => this.subject.GetTransaction();

        public override ITransaction GetTransaction2() => this.subject.GetTransaction2();

        public override int[] GetTestRepeats() => this.subject.GetTestRepeats();

        public override bool IsRollbackSupported() => this.subject.IsRollbackSupported();
    }
}
