// <copyright file="StateConverter.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class StateConverter
    {
        public const string XlfV12UserStatePrefix = "x-";

        public static string ToXlfV12(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentException();
            }

            if (XlfV12States.Enumerate().Contains(state))
            {
                return state;
            }

            switch (state)
            {
                case MuiDbStates.New:
                case XlfV20States.Initial:
                    return XlfV12States.New;

                case MuiDbStates.Translated: // XlfV20States.Translated
                    return XlfV12States.Translated;

                case MuiDbStates.Reviewed: // XlfV20States.Reviewed
                    return XlfV12States.SignedOff;

                case MuiDbStates.Final: // XlfV20States.Final
                    return XlfV12States.Final;
            }

            if (state.StartsWith(XlfV12UserStatePrefix))
            {
                return state;
            }

            throw new ArgumentOutOfRangeException(nameof(state), state, $"The state '{state}' is unknown and thus can not be converted to a valid XLIFF v1.2 state");
        }

        public static string ToXlfV20(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentException();
            }

            if (XlfV20States.Enumerate().Contains(state))
            {
                return state;
            }

            switch (state)
            {
                case XlfV12States.New:
                case XlfV12States.NeedsAdaptation:
                case XlfV12States.NeedsReviewAdaptation:
                case XlfV12States.NeedsL10n:
                case XlfV12States.NeedsTranslation:
                case XlfV20States.Initial:
                    return XlfV20States.Initial;

                case XlfV12States.Translated: // case KnownXlfStatesV20.Translated:
                case XlfV12States.NeedsReviewL10n:
                case XlfV12States.NeedsReviewTranslation:
                    return XlfV20States.Translated;

                case XlfV12States.SignedOff:
                case XlfV20States.Reviewed:
                    return XlfV20States.Reviewed;

                case XlfV12States.Final: // case KnownXlfStatesV20.Final:
                    return XlfV20States.Final;
            }

            if (state.StartsWith(XlfV12UserStatePrefix))
            {
                return XlfV20States.Initial;
            }

            throw new ArgumentOutOfRangeException(nameof(state), state, $"The state '{state}' is unknown and thus can not be converted to a valid XLIFF v2.0 state");
        }

        public static string ToMuiDB(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentException();
            }

            if (MuiDbStates.Enumerate().Contains(state))
            {
                return state;
            }

            switch (state)
            {
                case XlfV12States.New:
                case XlfV12States.NeedsAdaptation:
                case XlfV12States.NeedsReviewAdaptation:
                case XlfV12States.NeedsL10n:
                case XlfV12States.NeedsTranslation:
                case XlfV20States.Initial:
                    return MuiDbStates.New;

                case XlfV12States.Translated: // case KnownXlfStatesV20.Translated:
                case XlfV12States.NeedsReviewL10n:
                case XlfV12States.NeedsReviewTranslation:
                    return MuiDbStates.Translated;

                case XlfV12States.SignedOff:
                case XlfV20States.Reviewed:
                    return MuiDbStates.Reviewed;

                case XlfV12States.Final: // case KnownXlfStatesV20.Final:
                    return MuiDbStates.Final;
            }

            if (state.StartsWith(XlfV12UserStatePrefix))
            {
                return MuiDbStates.New;
            }

            throw new ArgumentOutOfRangeException(nameof(state), state, $"The state '{state}' is unknown and thus can not be converted to a valid MuiDB state");
        }

        public static class MuiDbStates
        {
            public const string New = "new";

            public const string Translated = "translated";

            public const string Reviewed = "reviewed";

            public const string Final = "final";

            public static IEnumerable<string> Enumerate()
            {
                return typeof(MuiDbStates).GetFields().Select(f => f.GetValue(null) as string);
            }
        }

        // XLIFF v1.2 states
        // TODO: In addition, user-defined values can be used with this attribute. A user-defined value must start with an "x-" prefix.
        public static class XlfV12States
        {
            // Indicates the terminating state.
            public const string Final = "final";

            // Indicates only non-textual information needs adaptation.
            public const string NeedsAdaptation = "needs-adaptation";

            // Indicates both text and non-textual information needs adaptation.
            public const string NeedsL10n = "needs-l10n";

            // Indicates only non-textual information needs review.
            public const string NeedsReviewAdaptation = "needs-review-adaptation";

            // Indicates both text and non-textual information needs review.
            public const string NeedsReviewL10n = "needs-review-l10n";

            // Indicates that only the text of the item needs to be reviewed.
            public const string NeedsReviewTranslation = "needs-review-translation";

            // Indicates that the item needs to be translated.
            public const string NeedsTranslation = "needs-translation";

            // Indicates that the item is new. For example, translation units that were not in a previous version of the document.
            public const string New = "new";

            // Indicates that changes are reviewed and approved.
            public const string SignedOff = "signed-off";

            // Indicates that the item has been translated.
            public const string Translated = "translated";

            public static IEnumerable<string> Enumerate()
            {
                return typeof(XlfV12States).GetFields().Select(f => f.GetValue(null) as string);
            }
        }

        // XLIFF 2.0 states
        // One can further specify the state of the Translation using the subState attribute.
        public static class XlfV20States
        {
            // Indicates the segment is in its initial state.
            public const string Initial = "initial";

            // Indicates the segment has been translated.
            public const string Translated = "translated";

            // Indicates the segment has been reviewed.
            public const string Reviewed = "reviewed";

            // Indicates the segment is finalized and ready to be used.
            public const string Final = "final";

            public static IEnumerable<string> Enumerate()
            {
                return typeof(XlfV20States).GetFields().Select(f => f.GetValue(null) as string);
            }
        }
    }
}
