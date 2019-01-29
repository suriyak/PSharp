﻿//-----------------------------------------------------------------------
// <copyright file="SendAndExecuteTest7.cs">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//      EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//      MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//      IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//      CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//      TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//      SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.PSharp.TestingServices.Tests.Unit
{
    public class SendAndExecuteTest7 : BaseTest
    {
        public SendAndExecuteTest7(ITestOutputHelper output)
            : base(output)
        { }

        class E : Event { }

        class Harness : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            async Task InitOnEntry()
            {
                var runtime = this.Id.GetRuntimeProxy();
                var m = await runtime.CreateMachineAndExecuteAsync(typeof(M));
                var handled = await runtime.SendEventAndExecuteAsync(m, new E());
                this.Assert(handled);
            }

        }

        class M : Machine
        {

            [Start]
            class Init : MachineState { }
        }

        [Fact]
        public void TestUnhandledEventOnSendExec()
        {
            var test = new Action<IMachineRuntime>((r) => {
                r.CreateMachine(typeof(Harness));
            });

            base.AssertFailed(test, "Machine 'M()' received event 'E' that cannot be handled.", true);
        }
    }
}
