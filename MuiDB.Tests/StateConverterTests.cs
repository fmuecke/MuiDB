// <copyright file="StateConverterTests.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using fmdev.MuiDB;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MuiDbStates = StateConverter.MuiDbStates;
    using Xlf12States = StateConverter.XlfV12States;
    using Xlf20States = StateConverter.XlfV20States;

    [TestClass]
    public class StateConverterTests
    {
        [TestMethod]
        public void EnumerateKnownStatesTest()
        {
            foreach (var state in MuiDbStates.Enumerate())
            {
                switch (state)
                {
                    case MuiDbStates.New:
                    case MuiDbStates.Translated:
                    case MuiDbStates.Reviewed:
                    case MuiDbStates.Final:
                        break;

                    default:
                        Assert.Fail($"state {state} is unknown! Please adjust other tests as well!");
                        break;
                }
            }

            foreach (var state in Xlf12States.Enumerate())
            {
                switch (state)
                {
                    case Xlf12States.Final:
                    case Xlf12States.NeedsAdaptation:
                    case Xlf12States.NeedsL10n:
                    case Xlf12States.NeedsReviewAdaptation:
                    case Xlf12States.NeedsReviewL10n:
                    case Xlf12States.NeedsReviewTranslation:
                    case Xlf12States.NeedsTranslation:
                    case Xlf12States.New:
                    case Xlf12States.SignedOff:
                    case Xlf12States.Translated:
                        break;

                    default:
                        Assert.Fail($"state {state} is unknown! Please adjust other tests as well!");
                        break;
                }
            }

            foreach (var state in Xlf20States.Enumerate())
            {
                switch (state)
                {
                    case Xlf20States.Initial:
                    case Xlf20States.Translated:
                    case Xlf20States.Reviewed:
                    case Xlf20States.Final:
                        break;

                    default:
                        Assert.Fail($"state {state} is unknown! Please adjust other tests as well!");
                        break;
                }
            }
        }

        [TestMethod]
        public void ToXlf12Test()
        {
            Assert.ThrowsException<ArgumentException>(() => StateConverter.ToXlfV12(null));
            Assert.ThrowsException<ArgumentException>(() => StateConverter.ToXlfV12(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => StateConverter.ToXlfV12(" "));

            // XLIFF 1.2 states
            foreach (var state in StateConverter.XlfV12States.Enumerate())
            {
                StateConverter.ToXlfV12(state).Should().Be(state);
            }

            // XLIFF 1.2 user states
            foreach (var state in new string[] { StateConverter.XlfV12UserStatePrefix, StateConverter.XlfV12UserStatePrefix + "asasdsadj kljkl" })
            {
                StateConverter.ToXlfV12(state).Should().Be(state, "All user specific states are passed through 1:1.");
            }

            // MuiDB states
            StateConverter.ToXlfV12(MuiDbStates.New).Should().Be(Xlf12States.New);
            StateConverter.ToXlfV12(MuiDbStates.Translated).Should().Be(Xlf12States.Translated);
            StateConverter.ToXlfV12(MuiDbStates.Reviewed).Should().Be(Xlf12States.SignedOff);
            StateConverter.ToXlfV12(MuiDbStates.Final).Should().Be(Xlf12States.Final);

            // XLIFF v2.0 states
            StateConverter.ToXlfV12(Xlf20States.Initial).Should().Be(Xlf12States.New);
            StateConverter.ToXlfV12(Xlf20States.Translated).Should().Be(Xlf12States.Translated);
            StateConverter.ToXlfV12(Xlf20States.Reviewed).Should().Be(Xlf12States.SignedOff);
            StateConverter.ToXlfV12(Xlf20States.Final).Should().Be(Xlf12States.Final);
        }

        [TestMethod]
        public void ToXlf20Test()
        {
            Assert.ThrowsException<ArgumentException>(() => StateConverter.ToXlfV20(null));
            Assert.ThrowsException<ArgumentException>(() => StateConverter.ToXlfV20(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => StateConverter.ToXlfV20(" "));

            // XLIFF v2.0 states
            foreach (var state in StateConverter.XlfV20States.Enumerate())
            {
                StateConverter.ToXlfV20(state).Should().Be(state);
            }

            // MuiDB states
            StateConverter.ToXlfV20(MuiDbStates.New).Should().Be(Xlf20States.Initial);
            StateConverter.ToXlfV20(MuiDbStates.Translated).Should().Be(Xlf20States.Translated);
            StateConverter.ToXlfV20(MuiDbStates.Reviewed).Should().Be(Xlf20States.Reviewed);
            StateConverter.ToXlfV20(MuiDbStates.Final).Should().Be(Xlf20States.Final);

            // XLIFF 1.2
            StateConverter.ToXlfV20(Xlf12States.New).Should().Be(Xlf20States.Initial);
            StateConverter.ToXlfV20(Xlf12States.NeedsAdaptation).Should().Be(Xlf20States.Initial);
            StateConverter.ToXlfV20(Xlf12States.NeedsReviewAdaptation).Should().Be(Xlf20States.Initial);
            StateConverter.ToXlfV20(Xlf12States.NeedsL10n).Should().Be(Xlf20States.Initial);
            StateConverter.ToXlfV20(Xlf12States.NeedsTranslation).Should().Be(Xlf20States.Initial);
            StateConverter.ToXlfV20(Xlf12States.Translated).Should().Be(Xlf20States.Translated);
            StateConverter.ToXlfV20(Xlf12States.NeedsReviewL10n).Should().Be(Xlf20States.Translated);
            StateConverter.ToXlfV20(Xlf12States.NeedsReviewTranslation).Should().Be(Xlf20States.Translated);
            StateConverter.ToXlfV20(Xlf12States.SignedOff).Should().Be(Xlf20States.Reviewed);
            StateConverter.ToXlfV20(Xlf12States.Final).Should().Be(Xlf20States.Final);

            // XLIFF 1.2 user states
            StateConverter.ToXlfV20(StateConverter.XlfV12UserStatePrefix).Should().Be(Xlf20States.Initial, "All user specific states are mapped to the New state!");
            StateConverter.ToXlfV20(StateConverter.XlfV12UserStatePrefix + "hkjfhskfahsldf").Should().Be(Xlf20States.Initial, "All user specific states are mapped to the New state!");
        }

        [TestMethod]
        public void ToMuiDBTest()
        {
            Assert.ThrowsException<ArgumentException>(() => StateConverter.ToMuiDB(null));
            Assert.ThrowsException<ArgumentException>(() => StateConverter.ToMuiDB(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => StateConverter.ToMuiDB(" "));

            // MuiDB states
            foreach (var state in StateConverter.MuiDbStates.Enumerate())
            {
                StateConverter.ToMuiDB(state).Should().Be(state);
            }

            // XLIFF 1.2
            StateConverter.ToMuiDB(Xlf12States.New).Should().Be(MuiDbStates.New);
            StateConverter.ToMuiDB(Xlf12States.NeedsAdaptation).Should().Be(MuiDbStates.New);
            StateConverter.ToMuiDB(Xlf12States.NeedsReviewAdaptation).Should().Be(MuiDbStates.New);
            StateConverter.ToMuiDB(Xlf12States.NeedsL10n).Should().Be(MuiDbStates.New);
            StateConverter.ToMuiDB(Xlf12States.NeedsTranslation).Should().Be(MuiDbStates.New);
            StateConverter.ToMuiDB(Xlf12States.Translated).Should().Be(MuiDbStates.Translated);
            StateConverter.ToMuiDB(Xlf12States.NeedsReviewL10n).Should().Be(MuiDbStates.Translated);
            StateConverter.ToMuiDB(Xlf12States.NeedsReviewTranslation).Should().Be(MuiDbStates.Translated);
            StateConverter.ToMuiDB(Xlf12States.SignedOff).Should().Be(MuiDbStates.Reviewed);
            StateConverter.ToMuiDB(Xlf12States.Final).Should().Be(MuiDbStates.Final);

            // XLIFF 1.2 user states
            StateConverter.ToMuiDB(StateConverter.XlfV12UserStatePrefix).Should().Be(MuiDbStates.New, "All user specific states are mapped to the New state!");
            StateConverter.ToMuiDB(StateConverter.XlfV12UserStatePrefix + "hkjfhskfahsldf").Should().Be(MuiDbStates.New, "All user specific states are mapped to the New state!");

            // XLIFF 2.0
            StateConverter.ToMuiDB(Xlf20States.Initial).Should().Be(MuiDbStates.New);
            StateConverter.ToMuiDB(Xlf20States.Translated).Should().Be(MuiDbStates.Translated);
            StateConverter.ToMuiDB(Xlf20States.Reviewed).Should().Be(MuiDbStates.Reviewed);
            StateConverter.ToMuiDB(Xlf20States.Final).Should().Be(MuiDbStates.Final);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => StateConverter.ToMuiDB("asdsadsad"), "States that can not be mapped result in an ArgumentException");
        }
    }
}