namespace Tests.Workspace
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Workspace;
    using Xunit;

    public static class AssertExtensions
    {
        #region ShouldEqual
        public static void ShouldEqual(this object actual, object expected, Context context, DatabaseMode mode1, DatabaseMode mode2)
            => Assert.True(Equals(actual, expected), $"Expected Equals: [{actual}, {expected}] on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldEqual(this object actual, object expected, Context context, WorkspaceMode mode1, WorkspaceMode mode2)
        => Assert.True(Equals(actual, expected), $"Expected Equals: [{actual}, {expected}] on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldEqual(this object actual, object expected, Context context, WorkspaceMode mode1, DatabaseMode mode2)
        => Assert.True(Equals(actual, expected), $"Expected Equals: [{actual}, {expected}] on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldEqual(this object actual, object expected, Context context, DatabaseMode mode)
            => Assert.True(Equals(actual, expected), $"Expected Equals: [{actual}, {expected}] on context {context} with mode1 {mode}");

        public static void ShouldEqual(this object actual, object expected, Context context, WorkspaceMode mode)
          => Assert.True(Equals(actual, expected), $"Expected Equals: [{actual}, {expected}] on context {context} with mode1 {mode}");

        #endregion

        #region ShouldNotEqual
        public static void ShouldNotEqual(this object actual, object expected, Context context, DatabaseMode mode1, DatabaseMode mode2)
            => Assert.True(!Equals(actual, expected), $"Expected Equals: [{actual}, {expected}] on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldNotEqual(this object actual, object expected, Context context, WorkspaceMode mode1, WorkspaceMode mode2)
        => Assert.True(!Equals(actual, expected), $"Expected Equals: [{actual}, {expected}] on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldNotEqual(this object actual, object expected, Context context, WorkspaceMode mode1, DatabaseMode mode2)
        => Assert.True(!Equals(actual, expected), $"Expected Equals: [{actual}, {expected}] on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldNotEqual(this object actual, object expected, Context context, DatabaseMode mode)
            => Assert.True(!Equals(actual, expected), $"Expected Equals: [{actual}, {expected}] on context {context} with mode1 {mode}");

        public static void ShouldNotEqual(this object actual, object expected, Context context, WorkspaceMode mode)
          => Assert.True(!Equals(actual, expected), $"Expected Equals: [{actual}, {expected}] on context {context} with mode1 {mode}");

        #endregion

        #region ShouldNotBeNull

        public static void ShouldNotBeNull(this object actual, Context context, DatabaseMode mode1, DatabaseMode mode2)
            => Assert.True(!(actual is null), $"Expected Not Null: [{actual}] on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldNotBeNull(this object actual, Context context, WorkspaceMode mode1, DatabaseMode mode2)
            => Assert.True(!(actual is null), $"Expected Not Null: [{actual}] on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldNotBeNull(this object actual, Context context, WorkspaceMode mode1, WorkspaceMode mode2)
             => Assert.True(!(actual is null), $"Expected Not Null: [{actual}] on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldNotBeNull(this object actual, Context context, DatabaseMode mode)
            => Assert.True(!(actual is null), $"Expected Not Null: [{actual}] on context {context} with mode1 {mode}");

        public static void ShouldNotBeNull(this object actual, Context context, WorkspaceMode mode)
       => Assert.True(!(actual is null), $"Expected Not Null: [{actual}] on context {context} with mode1 {mode}");

        #endregion

        #region ShouldContains

        // TODO: object => T
        public static void ShouldContains(this IEnumerable<IObject> collection, IObject expected, Context context, DatabaseMode mode1, DatabaseMode mode2)
            => Assert.True(collection.Contains(expected), $"Expected Not Null: [{collection}, {expected}] on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldContains(this IEnumerable<IObject> collection, IObject expected, Context context, WorkspaceMode mode1, WorkspaceMode mode2)
            => Assert.True(collection.Contains(expected), $"Expected Not Null: [{collection}, {expected}] on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldContains(this IEnumerable<IObject> collection, IObject expected, Context context, WorkspaceMode mode1, DatabaseMode mode2)
            => Assert.True(collection.Contains(expected), $"Expected Not Null: [{collection}, {expected}] on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldContains(this IEnumerable<IObject> collection, IObject expected, Context context, DatabaseMode mode)
            => Assert.True(collection.Contains(expected), $"Expected Not Null: [{collection}, {expected}] on context {context} with mode1 {mode}");

        public static void ShouldContains(this IEnumerable<IObject> collection, IObject expected, Context context, WorkspaceMode mode)
        => Assert.True(collection.Contains(expected), $"Expected Not Null: [{collection}, {expected}] on context {context} with mode1 {mode}");

        #endregion

        #region ShouldNotContains
        public static void ShouldNotContains(this IEnumerable<IObject> collection, IObject expected, Context context, DatabaseMode mode)
            => Assert.True(!collection.Contains(expected), $"Expected Not Contains: [{collection}, {expected}] on context {context} with mode {mode}");

        public static void ShouldNotContains(this IEnumerable<IObject> collection, IObject expected, Context context, WorkspaceMode mode)
            => Assert.True(!collection.Contains(expected), $"Expected Not Contains: [{collection}, {expected}] on context {context} with mode {mode}");

        public static void ShouldNotContains(this IEnumerable<IObject> collection, IObject expected, Context context, DatabaseMode mode1, DatabaseMode mode2)
            => Assert.True(!collection.Contains(expected), $"Expected Not Contains: [{collection}, {expected}] on context {context} with mode& {mode1} and mode2 {mode2}");

        public static void ShouldNotContains(this IEnumerable<IObject> collection, IObject expected, Context context, WorkspaceMode mode1, DatabaseMode mode2)
        => Assert.True(!collection.Contains(expected), $"Expected Not Contains: [{collection}, {expected}] on context {context} with mode& {mode1} and mode2 {mode2}");

        public static void ShouldNotContains(this IEnumerable<IObject> collection, IObject expected, Context context, WorkspaceMode mode1, WorkspaceMode mode2)
            => Assert.True(!collection.Contains(expected), $"Expected Not Contains: [{collection}, {expected}] on context {context} with mode& {mode1} and mode2 {mode2}");

        #endregion
    }
}
