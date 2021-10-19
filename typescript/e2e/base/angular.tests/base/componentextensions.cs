// <copyright file="ComponentExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using Allors.Database.Meta;
    using OpenQA.Selenium;

    public static partial class ComponentExtensions
    {
        public static Anchor<T> Anchor<T>(this T @this, By selector) where T : Component => new Anchor<T>(@this, @this.M, selector);

        public static Button<T> Button<T>(this T @this, By selector) where T : Component => new Button<T>(@this, @this.M, selector);

        public static Element<T> Element<T>(this T @this, By selector) where T : Component => new Element<T>(@this, @this.M, selector);

        public static Input<T> Input<T>(this T @this, params By[] selectors) where T : Component => new Input<T>(@this, @this.M, selectors);

        // TODO: Remove
        public static Input<T> Input<T>(this T @this, MetaPopulation m, string formControlName) where T : Component => new Input<T>(@this, m, "formControlName", formControlName);
    }
}