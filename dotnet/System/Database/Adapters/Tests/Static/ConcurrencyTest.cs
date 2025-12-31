// <copyright file="ConcurrencyTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using Xunit;

    /// <summary>
    /// Regression tests for thread safety during concurrent transaction operations.
    /// These tests would have caught race conditions in adapter implementations
    /// where concurrent reads and writes caused exceptions or data corruption.
    /// </summary>
    public abstract class ConcurrencyTest : IDisposable
    {
        protected abstract IProfile Profile { get; }

        protected IDatabase Database => this.Profile.Database;

        protected Action[] Inits => this.Profile.Inits;

        public abstract void Dispose();

        [Fact]
        public void ConcurrentCommitsAndQueriesShouldNotThrow()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Database.Context().M;

                const int writerCount = 4;
                const int readerCount = 4;
                const int iterationsPerThread = 50;
                var exceptions = new ConcurrentBag<Exception>();
                var barrier = new Barrier(writerCount + readerCount);
                var running = true;

                // Start concurrent writers
                var writers = Enumerable.Range(0, writerCount).Select(writerId => Task.Run(() =>
                {
                    barrier.SignalAndWait();
                    for (var i = 0; i < iterationsPerThread; i++)
                    {
                        try
                        {
                            using var tx = this.Database.CreateTransaction();
                            var c1 = (C1)tx.Create(m.C1);
                            c1.C1AllorsString = $"Writer{writerId}_Iter{i}";
                            c1.C1AllorsInteger = writerId * 1000 + i;
                            tx.Commit();
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                })).ToArray();

                // Start concurrent readers
                var readers = Enumerable.Range(0, readerCount).Select(readerId => Task.Run(() =>
                {
                    barrier.SignalAndWait();
                    for (var i = 0; i < iterationsPerThread && running; i++)
                    {
                        try
                        {
                            using var tx = this.Database.CreateTransaction();

                            // Query by string
                            var extent1 = tx.Extent(m.C1);
                            extent1.Filter.AddEquals(m.C1.C1AllorsString, $"Writer0_Iter{i % 25}");
                            _ = extent1.ToArray();

                            // Query by integer
                            var extent2 = tx.Extent(m.C1);
                            extent2.Filter.AddEquals(m.C1.C1AllorsInteger, i % 50);
                            _ = extent2.ToArray();
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                })).ToArray();

                Task.WaitAll(writers);
                running = false;
                Task.WaitAll(readers);

                Assert.Empty(exceptions);
            }
        }

        [Fact]
        public void ConcurrentReadsAndWritesShouldNotCorruptData()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Database.Context().M;

                // Seed initial data
                using (var tx = this.Database.CreateTransaction())
                {
                    for (var i = 0; i < 50; i++)
                    {
                        var c1 = (C1)tx.Create(m.C1);
                        c1.C1AllorsString = $"Initial{i}";
                        c1.C1AllorsInteger = i;
                    }

                    tx.Commit();
                }

                const int threadCount = 8;
                const int iterations = 25;
                var exceptions = new ConcurrentBag<Exception>();
                var barrier = new Barrier(threadCount);

                var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
                {
                    barrier.SignalAndWait();
                    for (var i = 0; i < iterations; i++)
                    {
                        try
                        {
                            if (threadId % 2 == 0)
                            {
                                // Writer
                                using var tx = this.Database.CreateTransaction();
                                var c1 = (C1)tx.Create(m.C1);
                                c1.C1AllorsString = $"Thread{threadId}_Iter{i}";
                                c1.C1AllorsInteger = threadId * 1000 + i;
                                tx.Commit();
                            }
                            else
                            {
                                // Reader
                                using var tx = this.Database.CreateTransaction();
                                var extent = tx.Extent(m.C1);
                                extent.Filter.AddEquals(m.C1.C1AllorsInteger, i % 50);
                                var results = extent.ToArray();

                                foreach (var obj in results)
                                {
                                    var c1 = (C1)obj;
                                    _ = c1.C1AllorsString;
                                    _ = c1.C1AllorsInteger;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                })).ToArray();

                Task.WaitAll(tasks);

                Assert.Empty(exceptions);
            }
        }

        [Fact]
        public void HighContentionCommitsShouldNotDeadlock()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Database.Context().M;

                const int threadCount = 10;
                const int iterations = 50;
                var completed = 0;

                var tasks = Enumerable.Range(0, threadCount).Select(_ => Task.Run(() =>
                {
                    for (var i = 0; i < iterations; i++)
                    {
                        using var tx = this.Database.CreateTransaction();
                        var c1 = (C1)tx.Create(m.C1);
                        c1.C1AllorsString = "SameValue"; // High contention
                        c1.C1AllorsInteger = 42;
                        tx.Commit();
                    }

                    Interlocked.Increment(ref completed);
                })).ToArray();

                var allCompleted = Task.WaitAll(tasks, TimeSpan.FromSeconds(60));

                Assert.True(allCompleted, "Tasks did not complete - possible deadlock");
                Assert.Equal(threadCount, completed);
            }
        }
    }
}
