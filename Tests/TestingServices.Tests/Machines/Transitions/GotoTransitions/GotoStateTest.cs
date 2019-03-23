﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.PSharp.TestingServices.Tests
{
    public class GotoStateTest : BaseTest
    {
        public GotoStateTest(ITestOutputHelper output)
            : base(output)
        { }

        class Program1 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                this.Goto<Done>();
            }

            class Done : MachineState { }
        }

        class Program2 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
#pragma warning disable 618
                this.Goto(typeof(Done));
#pragma warning restore 618
            }

            class Done : MachineState { }
        }

        internal static int MonitorValue;

        class M1 : Monitor
        {
            [Start]
            [OnEntry(nameof(Init))]
            class S1 : MonitorState { }

            [OnEntry(nameof(IncrementValue))]
            class S2 : MonitorState { }

            void Init() { this.Goto<S2>(); }

            void IncrementValue() { MonitorValue = 101; }
        }

        class M2 : Monitor
        {
            [Start]
            [OnEntry(nameof(Init))]
            class S1 : MonitorState { }

            [OnEntry(nameof(IncrementValue))]
            class S2 : MonitorState { }

            void Init()
            {
#pragma warning disable 618
                this.Goto(typeof(S2));
#pragma warning restore 618
            }

            void IncrementValue() { MonitorValue = 202; }
        }

        [Fact]
        public void TestGotoStateGenericMethod()
        {
            var test = new Action<IMachineRuntime>((r) => {
                r.CreateMachine(typeof(Program1));
            });

            base.AssertSucceeded(test);
        }

        [Fact]
        public void TestGotoStateTypeof()
        {
            var test = new Action<IMachineRuntime>((r) => {
                r.CreateMachine(typeof(Program2));
            });

            base.AssertSucceeded(test);
        }

        [Fact]
        public void TestGotoStateMonitorGenericMethod()
        {
            var test = new Action<IMachineRuntime>((r) => {
                r.RegisterMonitor(typeof(M1));
            });

            base.AssertSucceeded(test);
            Assert.Equal(101, MonitorValue);
        }

        [Fact]
        public void TestGotoStateMonitorTypeof()
        {
            var test = new Action<IMachineRuntime>((r) => {
                r.RegisterMonitor(typeof(M2));
            });

            base.AssertSucceeded(test);
            Assert.Equal(202, MonitorValue);
        }
    }
}
