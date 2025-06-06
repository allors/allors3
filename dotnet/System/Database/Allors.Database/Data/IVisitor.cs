// <copyright file="TreeNode.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    public interface IVisitor
    {
        void VisitAnd(And visited);

        void VisitBetween(Between visited);

        void VisitContainedIn(ContainedIn visited);

        void VisitContains(Contains visited);

        void VisitEquals(Equals visited);

        void VisitExcept(Except visited);

        void VisitExists(Exists visited);

        void VisitExtent(Extent visited);

        void VisitSelect(Select visited);

        void VisitGreaterThan(GreaterThan visited);

        void VisitInstanceOf(Instanceof visited);

        void VisitIntersect(Intersect visited);

        void VisitLessThan(LessThan visited);

        void VisitLike(Like visited);

        void VisitNode(Node visited);

        void VisitNot(Not visited);

        void VisitOr(Or visited);

        void VisitPull(Pull visited);

        void VisitResult(Result visited);

        void VisitSort(Sort visited);

        void VisitUnion(Union visited);

        void VisitProcedure(Procedure procedure);
    }
}
