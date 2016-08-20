// <copyright file="MissingTranslationsException.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.MuiDB
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    [Serializable]
    public class MissingTranslationsException : Exception
    {
        private List<string> items = new List<string>();

        public MissingTranslationsException()
        {
        }

        ////public MissingTranslationsException(string message)
        ////    : base(message)
        ////{
        ////}

        public MissingTranslationsException(List<string> items)
        {
            this.items = items;
        }

        ////public MissingTranslationsException(string message, Exception innerException)
        ////    : base(message, innerException)
        ////{
        ////}

        ////protected MissingTranslationsException(SerializationInfo info, StreamingContext context)
        ////    : base(info, context)
        ////{
        ////}

        public IEnumerable<string> Items
        {
            get
            {
                return items;
            }
        }

        public override string Message
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var i in Items)
                {
                    var tokens = i.Split(':');
                    var id = tokens[0];
                    var lang = tokens.Length == 1 ? "unknown" : tokens[1];
                    sb.AppendLine($"'{id}' misses a translation in language '{lang}'.");
                }

                return sb.ToString();
            }
        }
    }
}