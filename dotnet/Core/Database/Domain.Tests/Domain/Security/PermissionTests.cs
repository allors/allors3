// <copyright file="PermissionTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the PermissionTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Linq;
    using Meta;
    using Services;
    using Xunit;

    public class PermissionTests : DomainTest, IClassFixture<Fixture>
    {
        public PermissionTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        // [Fact]
        // public void SyncMethod()
        // {
        //    var domain = (Domain)this.DatabaseTransaction.Population.MetaPopulation.Find(new Guid("AB41FD0C-C887-4A1D-BEDA-CED69527E69A"));

        // var methodType = new MethodTypeBuilder(domain, Guid.NewGuid()).Build();
        //    methodType.ObjectType = M.Organisation.ObjectType;
        //    methodType.Name = "Method";

        // var count = new Permissions(this.DatabaseTransaction).Extent().Count;

        // new Permissions(this.DatabaseTransaction).Sync();

        // Assert.Equal(count + 1, new Permissions(this.DatabaseTransaction).Extent().Count);

        // var methodPermission = new Permissions(this.DatabaseTransaction).FindBy(M.Permission.OperandTypePointer, methodType.Id);
        //    Assert.NotNull(methodPermission);
        //    Assert.Equal(Operation.Execute, methodPermission.Operation);
        // }

        // [Fact]
        // public void SyncRelation()
        // {
        //    var domain = (Domain)this.DatabaseTransaction.Population.MetaPopulation.Find(new Guid("AB41FD0C-C887-4A1D-BEDA-CED69527E69A"));

        // var count = new Permissions(this.DatabaseTransaction).Extent().Count;

        // var relationType = new RelationTypeBuilder(domain, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Build();

        // relationType.AssociationType.ObjectType = M.Organisation.ObjectType;
        //    relationType.RoleType.ObjectType = Persons.Meta.ObjectType;
        //    relationType.RoleType.AssignedSingularName = "Relation";
        //    relationType.RoleType.AssignedPluralName = "Relations";

        // new Permissions(this.DatabaseTransaction).Sync();

        // this.DatabaseTransaction.Derive(true);

        // Assert.Equal(count + 3, new Permissions(this.DatabaseTransaction).Extent().Count);

        // var roleTypePermissions = new Permissions(this.DatabaseTransaction).Extent();
        //    roleTypePermissions.Filter.AddEquals(M.Permission.OperandTypePointer, relationType.RoleType.Id);
        //    Assert.Equal(2, roleTypePermissions.Count);

        // var associationTypePermissions = new Permissions(this.DatabaseTransaction).Extent();
        //    associationTypePermissions.Filter.AddEquals(M.Permission.OperandTypePointer, relationType.AssociationType.Id);
        //    Assert.Equal(1, associationTypePermissions.Count);
        // }

        [Fact]
        public void NoPermissionsForAssociationsWhenUnitType()
        {
            this.Transaction.Database.Services.Get<IPermissions>().Sync(this.Transaction);

            var permissions = new Permissions(this.Transaction).Extent().ToArray();

            Assert.Empty(permissions.Where(v => v.OperandType is IAssociationType associationType && associationType.RoleType.ObjectType.IsUnit));
        }

        [Fact]
        public void WhenSyncingPermissionsThenObsoletePermissionsAreDeleted()
        {
            var domain = (Domain)this.Transaction.Database.MetaPopulation.FindById(new Guid("AB41FD0C-C887-4A1D-BEDA-CED69527E69A"));

            var count = new Permissions(this.Transaction).Extent().Count;

            var permission = new ExecutePermissionBuilder(this.Transaction).WithClassPointer(new Guid()).WithMethodTypePointer(new Guid()).Build();

            this.Transaction.Database.Services.Get<IPermissions>().Sync(this.Transaction);

            Assert.True(permission.Strategy.IsDeleted);
        }

        [Fact]
        public void WhenSyncingPermissionsThenDanglingPermissionsAreDeleted()
        {
            var permission = new ReadPermissionBuilder(this.Transaction).Build();

            this.Transaction.Database.Services.Get<IPermissions>().Sync(this.Transaction);

            Assert.True(permission.Strategy.IsDeleted);
        }

        [Fact]
        public void GivenSyncedPermissionsWhenRemovingAnOperationThenThatPermissionIsInvalid()
        {
            // TODO: Permission members should be write once
            // var permission = (Permission)this.DatabaseTransaction.Extent<Permission>().First;
            // permission.RemoveOperationEnum();

            // var validation = this.DatabaseTransaction.Derive(false);

            // Assert.True(validation.HasErrors);
            // Assert.Equal(1, validation.Errors.Length);

            // var derivationError = validation.Errors[0];

            // Assert.Equal(1, derivationError.Relations.Length);
            // Assert.Equal(typeof(DerivationErrorRequired), derivationError.GetType());
            // Assert.Equal((RoleType)M.Permission.OperationEnum, derivationError.Relations[0].RoleType);
        }

        [Fact]
        public void GivenSyncedPermissionsWhenRemovingAnAccessControlledMemberThenThatPermissionIsInvalid()
        {
            // TODO: Permission members should be write once
            // var permission = this.DatabaseTransaction.Extent<Permission>().First;
            // permission.RemoveOperandTypePointer();

            // var validation = this.DatabaseTransaction.Derive(false);

            // Assert.True(validation.HasErrors);
            // Assert.Equal(1, validation.Errors.Length);

            // var derivationError = validation.Errors[0];

            // Assert.Equal(1, derivationError.Relations.Length);
            // Assert.Equal(typeof(DerivationErrorRequired), derivationError.GetType());
            // Assert.Equal((RoleType)M.Permission.OperandTypePointer, derivationError.Relations[0].RoleType);
        }
    }
}
