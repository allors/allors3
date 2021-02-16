// <copyright file="PushTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    
    using Allors.Database.Domain;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Database.Protocol.Json;
    using Xunit;

    public class PushNewObjectTests : ApiTest, IClassFixture<Fixture>
    {
        public PushNewObjectTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void WorkspaceX1ObjectInWorkspaceX()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                NewObjects = new[] { new PushRequestNewObject { ObjectType = this.M.WorkspaceXObject1.Class.IdAsString, NewWorkspaceId = "-1" }, },
            };

            var api = new Api(this.Transaction, "X");
            var pushResponse = api.Push(pushRequest);

            this.Transaction.Rollback();

            var x1 = (WorkspaceXObject1)this.Transaction.Instantiate(pushResponse.NewObjects[0].DatabaseId);

            Assert.NotNull(x1);
        }

        [Fact]
        public void WorkspaceX1ObjectInWorkspaceY()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                NewObjects = new[] { new PushRequestNewObject { ObjectType = this.M.WorkspaceXObject1.Class.IdAsString, NewWorkspaceId = "-1" }, },
            };

            var api = new Api(this.Transaction, "Y");
            var pushResponse = api.Push(pushRequest);

            this.Transaction.Rollback();

            var x1 = (WorkspaceXObject1)this.Transaction.Instantiate(pushResponse.NewObjects[0].DatabaseId);

            Assert.Null(x1);
        }

        [Fact]
        public void WorkspaceX1ObjectInWorkspaceNone()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                NewObjects = new[] { new PushRequestNewObject { ObjectType = this.M.WorkspaceXObject1.Class.IdAsString, NewWorkspaceId = "-1" }, },
            };

            var api = new Api(this.Transaction, "None");
            var pushResponse = api.Push(pushRequest);

            this.Transaction.Rollback();

            var x1 = (WorkspaceNoneObject1)this.Transaction.Instantiate(pushResponse.NewObjects[0].DatabaseId);

            Assert.Null(x1);
        }

        public void WorkspaceY1ObjectInWorkspaceNone()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                NewObjects = new[] { new PushRequestNewObject { ObjectType = this.M.WorkspaceYObject1.Class.IdAsString, NewWorkspaceId = "-1" }, },
            };

            var api = new Api(this.Transaction, "None");
            var pushResponse = api.Push(pushRequest);

            this.Transaction.Rollback();

            var y1 = (WorkspaceNoneObject1)this.Transaction.Instantiate(pushResponse.NewObjects[0].DatabaseId);

            Assert.Null(y1);
        }

        [Fact]
        public void WorkspaceNoneObjectInWorkspaceX()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                NewObjects = new[] { new PushRequestNewObject { ObjectType = this.M.WorkspaceNoneObject1.Class.IdAsString, NewWorkspaceId = "-1" }, },
            };

            var api = new Api(this.Transaction, "X");
            var pushResponse = api.Push(pushRequest);

            this.Transaction.Rollback();

            var none1 = (WorkspaceNoneObject1)this.Transaction.Instantiate(pushResponse.NewObjects[0].DatabaseId);

            Assert.Null(none1);
        }

        [Fact]
        public void WorkspaceNoneObjectInWorkspaceY()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                NewObjects = new[] { new PushRequestNewObject { ObjectType = this.M.WorkspaceNoneObject1.Class.IdAsString, NewWorkspaceId = "-1" }, },
            };

            var api = new Api(this.Transaction, "Y");
            var pushResponse = api.Push(pushRequest);

            this.Transaction.Rollback();

            var none1 = (WorkspaceNoneObject1)this.Transaction.Instantiate(pushResponse.NewObjects[0].DatabaseId);

            Assert.Null(none1);
        }

        [Fact]
        public void WorkspaceNoneObjectInWorkspaceNone()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                NewObjects = new[] { new PushRequestNewObject { ObjectType = this.M.WorkspaceNoneObject1.Class.IdAsString, NewWorkspaceId = "-1" }, },
            };

            var api = new Api(this.Transaction, "None");
            var pushResponse = api.Push(pushRequest);

            this.Transaction.Rollback();

            var none1 = (WorkspaceNoneObject1)this.Transaction.Instantiate(pushResponse.NewObjects[0].DatabaseId);

            Assert.Null(none1);
        }
    }
}
