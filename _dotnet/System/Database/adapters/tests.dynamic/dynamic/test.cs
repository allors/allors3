// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Test.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Meta;

    public abstract class Test : IDisposable
    {
        protected readonly int ObjectsPerClass = 5;
        protected readonly TestValueGenerator ValueGenerator = new TestValueGenerator();
        private readonly bool[] boolFlags = { false, true };

        // Thorough
        // int[] repeats = { 1, 2 };
        // int[] testRepeats = { 1, 2 };
        // int[] assertRepeats = { 1, 2 };
        // private int objectsPerClass = 100;

        // Quick
        private readonly int[] repeats = { 1 };
        private readonly int[] testRepeats = { 1 };
        private readonly int[] assertRepeats = { 1 };

        public void Commit(bool transactionFlag)
        {
            if (transactionFlag)
            {
                this.GetTransaction().Commit();
            }
        }

        public void Rollback(bool rollbackFlag)
        {
            if (rollbackFlag)
            {
                this.GetTransaction().Rollback();
            }
        }

        public virtual int[] GetRepeats() => this.repeats;

        public virtual int[] GetAssertRepeats() => this.assertRepeats;

        public virtual int[] GetTestRepeats() => this.testRepeats;

        public abstract IObject[] CreateArray(IObjectType objectType, int count);

        public abstract IDatabase CreateMemoryPopulation();

        public virtual bool[] GetBooleanFlags() => this.boolFlags;

        public abstract bool IsRollbackSupported();

        public abstract MetaPopulation GetMetaPopulation();

        public abstract MetaPopulation GetMetaPopulation2();

        public abstract IDatabase GetPopulation();

        public abstract IDatabase GetPopulation2();

        public abstract ITransaction GetTransaction();

        public abstract ITransaction GetTransaction2();

        public virtual IClass GetMetaType(IObject allorsObject) => allorsObject.Strategy.Class;

        public IRelationType[] GetOne2OneRelations(MetaPopulation metaPopulation)
        {
            var relations = new ArrayList();
            foreach (var metaRelation in metaPopulation.RelationTypes)
            {
                if (metaRelation.RoleType.ObjectType.IsComposite && metaRelation.IsOneToOne)
                {
                    relations.Add(metaRelation);
                }
            }

            return (RelationType[])relations.ToArray(typeof(RelationType));
        }

        public IRelationType[] GetOne2ManyRelations(MetaPopulation metaPopulation)
        {
            var relations = new ArrayList();
            foreach (var metaRelation in metaPopulation.RelationTypes)
            {
                if (metaRelation.RoleType.ObjectType.IsComposite && metaRelation.IsOneToMany)
                {
                    relations.Add(metaRelation);
                }
            }

            return (RelationType[])relations.ToArray(typeof(RelationType));
        }

        public IRelationType[] GetMany2OneRelations(MetaPopulation metaPopulation)
        {
            var relations = new ArrayList();
            foreach (var metaRelation in metaPopulation.RelationTypes)
            {
                if (metaRelation.RoleType.ObjectType.IsComposite && metaRelation.IsManyToOne)
                {
                    relations.Add(metaRelation);
                }
            }

            return (RelationType[])relations.ToArray(typeof(RelationType));
        }

        public IRelationType[] GetMany2ManyRelations(MetaPopulation metaPopulation)
        {
            var relations = new ArrayList();
            foreach (var metaRelation in metaPopulation.RelationTypes)
            {
                if (metaRelation.RoleType.ObjectType.IsComposite && metaRelation.IsManyToMany)
                {
                    relations.Add(metaRelation);
                }
            }

            return (RelationType[])relations.ToArray(typeof(RelationType));
        }

        public IClass[] GetClasses(IRelationType relationType) => ((IComposite)relationType.RoleType.ObjectType).DatabaseClasses.ToArray();

        public IRelationType[] GetBinaryRoles(IComposite type)
        {
            var roleList = new List<IRelationType>();
            foreach (var metaRole in type.DatabaseRoleTypes)
            {
                if (metaRole.ObjectType is Unit unit && unit.IsBinary)
                {
                    roleList.Add(metaRole.RelationType);
                }
            }

            return roleList.ToArray();
        }

        public IRelationType[] GetBooleanRoles(IComposite type)
        {
            var roleList = new List<IRelationType>();
            foreach (var metaRole in type.DatabaseRoleTypes)
            {
                if (metaRole.ObjectType is Unit unit && unit.IsBoolean)
                {
                    roleList.Add(metaRole.RelationType);
                }
            }

            return roleList.ToArray();
        }

        public IRelationType[] GetDateTimeRoles(IComposite type)
        {
            var roleList = new List<IRelationType>();
            foreach (var metaRole in type.DatabaseRoleTypes)
            {
                if (metaRole.ObjectType is Unit unit && unit.IsDateTime)
                {
                    roleList.Add(metaRole.RelationType);
                }
            }

            return roleList.ToArray();
        }

        public IRelationType[] GetDecimalRoles(IComposite type)
        {
            var roleList = new List<IRelationType>();
            foreach (var metaRole in type.DatabaseRoleTypes)
            {
                if (metaRole.ObjectType is Unit unit && unit.IsDecimal)
                {
                    roleList.Add(metaRole.RelationType);
                }
            }

            return roleList.ToArray();
        }

        public IRelationType[] GetFloatRoles(IComposite type)
        {
            var roleList = new List<IRelationType>();
            foreach (var metaRole in type.DatabaseRoleTypes)
            {
                if (metaRole.ObjectType is Unit unit && unit.IsFloat)
                {
                    roleList.Add(metaRole.RelationType);
                }
            }

            return roleList.ToArray();
        }

        public IRelationType[] GetIntegerRoles(IComposite type)
        {
            var roleList = new List<IRelationType>();
            foreach (var metaRole in type.DatabaseRoleTypes)
            {
                if (metaRole.ObjectType is Unit unit && unit.IsInteger)
                {
                    roleList.Add(metaRole.RelationType);
                }
            }

            return roleList.ToArray();
        }

        public IRelationType[] GetStringRoles(IComposite type)
        {
            var roleList = new List<IRelationType>();
            foreach (var metaRole in type.DatabaseRoleTypes)
            {
                if (metaRole.ObjectType is Unit unit && unit.IsString)
                {
                    roleList.Add(metaRole.RelationType);
                }
            }

            return roleList.ToArray();
        }

        public IRelationType[] GetUniqueRoles(IComposite type)
        {
            var roleList = new List<IRelationType>();
            foreach (var metaRole in type.DatabaseRoleTypes)
            {
                if (metaRole.ObjectType is Unit unit && unit.IsUnique)
                {
                    roleList.Add(metaRole.RelationType);
                }
            }

            return roleList.ToArray();
        }

        public IRelationType[] GetUnitRelations(IMetaPopulation metaPopulation)
        {
            var relations = new ArrayList();
            foreach (var metaRelation in metaPopulation.DatabaseRelationTypes)
            {
                if (metaRelation.RoleType.ObjectType.IsUnit)
                {
                    relations.Add(metaRelation);
                }
            }

            return (RelationType[])relations.ToArray(typeof(RelationType));
        }

        public IRelationType[] GetUnitRoles(IComposite type)
        {
            var roleList = new List<IRelationType>();
            foreach (var metaRole in type.DatabaseRoleTypes)
            {
                if (metaRole.ObjectType.IsUnit)
                {
                    roleList.Add(metaRole.RelationType);
                }
            }

            return roleList.ToArray();
        }

        public void Load(ITransaction transaction, string xml)
        {
            using (var stringReader = new StringReader(xml))
            {
                var reader = new XmlTextReader(stringReader);
                transaction.Database.Load(reader);
                reader.Close();
            }
        }

        protected string Save(ITransaction transaction)
        {
            using (var stringWriter = new StringWriter())
            {
                var writer = new XmlTextWriter(stringWriter);
                transaction.Database.Save(writer);
                writer.Close();
                return stringWriter.ToString();
            }
        }

        public abstract void Dispose();
    }
}
