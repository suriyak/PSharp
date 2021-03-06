﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.PSharp.SharedObjects.Tests
{
    public class MockSharedDictionaryTest : BaseTest
    {
        public MockSharedDictionaryTest(ITestOutputHelper output)
            : base(output)
        { }

        class E1 : Event
        {
            public ISharedDictionary<int, string> Counter;

            public E1(ISharedDictionary<int, string> counter)
            {
                this.Counter = counter;
            }
        }

        class E2 : Event
        {
            public ISharedDictionary<int, string> Counter;
            public bool Flag;

            public E2(ISharedDictionary<int, string> counter, bool flag)
            {
                this.Counter = counter;
                this.Flag = flag;
            }
        }

        class M1 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                var counter = SharedDictionary.Create<int, string>(this.Id.Runtime);
                this.CreateMachine(typeof(N1), new E1(counter));

                counter.TryAdd(1, "M");

                var v = counter[1];

                this.Assert(v == "M");
            }
        }

        class N1 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                var counter = (this.ReceivedEvent as E1).Counter;
                counter.TryUpdate(1, "N", "M");
            }
        }

        class M2 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                var counter = SharedDictionary.Create<int, string>(this.Id.Runtime);
                counter.TryAdd(1, "M");

                // Key not present; will throw an exception.
                var v = counter[2];
            }
        }

        class M3 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                var counter = SharedDictionary.Create<int, string>(this.Id.Runtime);
                this.CreateMachine(typeof(N3), new E1(counter));

                counter.TryAdd(1, "M");

                var v = counter[1];
                var c = counter.Count;

                this.Assert(c == 1);
            }
        }

        class N3 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                var counter = (this.ReceivedEvent as E1).Counter;
                counter.TryUpdate(1, "N", "M");
            }
        }

        class M4 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                var counter = SharedDictionary.Create<int, string>(this.Id.Runtime);
                this.CreateMachine(typeof(N4), new E1(counter));

                counter.TryAdd(1, "M");

                var b = counter.TryRemove(1, out string v);

                this.Assert(b == false || v == "M");
                this.Assert(counter.Count == 0);
            }
        }

        class N4 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                var counter = (this.ReceivedEvent as E1).Counter;
                var b = counter.TryRemove(1, out string v);

                this.Assert(b == false || v == "M");
            }
        }

        class M5 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                var counter = (this.ReceivedEvent as E2).Counter;
                var flag = (this.ReceivedEvent as E2).Flag;

                counter.TryAdd(1, "M");

                if (flag)
                {
                    this.CreateMachine(typeof(N5), new E2(counter, false));
                }

                var b = counter.TryGetValue(2, out string v);

                if (!flag)
                {
                    this.Assert(!b);
                }

                if (b)
                {
                    this.Assert(v == "N");
                }
            }
        }

        class N5 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                var counter = (this.ReceivedEvent as E2).Counter;
                bool b = counter.TryGetValue(1, out string v);

                this.Assert(b);
                this.Assert(v == "M");

                counter.TryAdd(2, "N");
            }
        }

        class M6 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                var counter = (this.ReceivedEvent as E1).Counter;

                this.CreateMachine(typeof(N6), new E1(counter));
                counter.TryAdd(1, "M");

                var b = counter.TryGetValue(2, out string v);
                this.Assert(!b);
            }
        }

        class N6 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                var counter = (this.ReceivedEvent as E1).Counter;
                counter.TryAdd(2, "N");
            }
        }

        [Fact]
        public void TestMockSharedDictionary1()
        {
            var config = Configuration.Create().WithNumberOfIterations(50);
            var test = new Action<PSharpRuntime>((r) => {
                r.CreateMachine(typeof(M1));
            });

            base.AssertFailed(config, test, "Detected an assertion failure.");
        }

        [Fact]
        public void TestMockSharedDictionary2()
        {
            var config = Configuration.Create().WithNumberOfIterations(50);
            var test = new Action<PSharpRuntime>((r) => {
                r.CreateMachine(typeof(M2));
            });

            base.AssertFailed(config, test, 1);
        }

        [Fact]
        public void TestMockSharedDictionary3()
        {
            var config = Configuration.Create().WithNumberOfIterations(50);
            var test = new Action<PSharpRuntime>((r) => {
                r.CreateMachine(typeof(M3));
            });

            base.AssertSucceeded(config, test);
        }

        [Fact]
        public void TestMockSharedDictionary4()
        {
            var config = Configuration.Create().WithNumberOfIterations(50);
            var test = new Action<PSharpRuntime>((r) => {
                r.CreateMachine(typeof(M4));
            });

            base.AssertSucceeded(config, test);
        }

        [Fact]
        public void TestMockSharedDictionary5()
        {
            var config = Configuration.Create().WithNumberOfIterations(50);
            var test = new Action<PSharpRuntime>((r) => {
                var counter = SharedDictionary.Create<int, string>(r);
                r.CreateMachine(typeof(M5), new E2(counter, true));
            });

            base.AssertSucceeded(config, test);
        }

        [Fact]
        public void TestMockSharedDictionary6()
        {
            var config = Configuration.Create().WithNumberOfIterations(50);
            var test = new Action<PSharpRuntime>((r) => {
                var counter = SharedDictionary.Create<int, string>(r);
                r.CreateMachine(typeof(M5), new E2(counter, false));
            });

            base.AssertSucceeded(config, test);
        }

        [Fact]
        public void TestMockSharedDictionary7()
        {
            var config = Configuration.Create().WithNumberOfIterations(50);
            var test = new Action<PSharpRuntime>((r) => {
                var counter = SharedDictionary.Create<int, string>(r);
                r.CreateMachine(typeof(M6), new E1(counter));
            });

            base.AssertFailed(config, test, "Detected an assertion failure.");
        }
    }
}
