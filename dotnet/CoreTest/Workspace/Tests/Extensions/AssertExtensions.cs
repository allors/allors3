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
            => Assert.True(Equals(actual, expected), $"{actual} should equal {expected} on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldEqual(this object actual, object expected, Context context, DatabaseMode mode)
            => Assert.True(Equals(actual, expected), $"{actual} should equal {expected} on context {context} with mode1 {mode}");

        public static void ShouldEqual(this object actual, object expected, Context context)
          => Assert.True(Equals(actual, expected), $"{actual} should equal {expected} on context {context}");

        #endregion

        #region ShouldNotEqual
        public static void ShouldNotEqual(this object actual, object expected, Context context, DatabaseMode mode1, DatabaseMode mode2)
            => Assert.True(!Equals(actual, expected), $"{actual} should not equal: {expected} on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldNotEqual(this object actual, object expected, Context context, DatabaseMode mode)
            => Assert.True(!Equals(actual, expected), $"{actual} should not equal: {expected} on context {context} with mode1 {mode}");

        public static void ShouldNotEqual(this object actual, object expected, Context context)
          => Assert.True(!Equals(actual, expected), $"{actual} should not equal: {expected} on context {context}");

        #endregion

        #region ShouldNotBeNull

        public static void ShouldNotBeNull(this object actual, Context context, DatabaseMode mode1, DatabaseMode mode2)
            => Assert.True(!(actual is null), $"{actual} should not be null on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldNotBeNull(this object actual, Context context, DatabaseMode mode)
            => Assert.True(!(actual is null), $"{actual} should not be null on context {context} with mode1 {mode}");

        public static void ShouldNotBeNull(this object actual, Context context)
            => Assert.True(!(actual is null), $"{actual} should not be null on context {context}");

        #endregion

        #region ShouldContain

        public static void ShouldContain(this IEnumerable<IObject> collection, IObject expected, Context context, DatabaseMode mode1, DatabaseMode mode2)
            => Assert.True(collection.Contains(expected), $"{collection.Dump()} should contain {expected} on context {context} with mode1 {mode1} and mode2 {mode2}");

        public static void ShouldContain(this IEnumerable<IObject> collection, IObject expected, Context context, DatabaseMode mode)
            => Assert.True(collection.Contains(expected), $"{collection.Dump()} should contain {expected} on context {context} with mode1 {mode}");

        public static void ShouldContain(this IEnumerable<IObject> collection, IObject expected, Context context)
        => Assert.True(collection.Contains(expected), $"{collection.Dump()} should contain {expected} on context {context}");

        #endregion

        #region ShouldNotContain
        public static void ShouldNotContain(this IEnumerable<IObject> collection, IObject expected, Context context)
            => Assert.True(!collection.Contains(expected), $"{collection.Dump()} should not Contain {expected} on context {context}");

        public static void ShouldNotContain(this IEnumerable<IObject> collection, IObject expected, Context context, DatabaseMode mode)
            => Assert.True(!collection.Contains(expected), $"{collection.Dump()} should not contain {expected} on context {context} with mode {mode}");

        public static void ShouldNotContain(this IEnumerable<IObject> collection, IObject expected, Context context, DatabaseMode mode1, DatabaseMode mode2)
                => Assert.True(!collection.Contains(expected), $"{collection.Dump()} should not contain {expected} on context {context} with mode& {mode1} and mode2 {mode2}");

        #endregion
    }
}
