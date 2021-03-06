﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.PSharp.LanguageServices.Parsing
{
    /// <summary>
    /// Interface for a lexer.
    /// </summary>
    public interface ILexer
    {
        /// <summary>
        /// Tokenizes the given text.
        /// </summary>
        /// <param name="text">Text to tokenize</param>
        /// <returns>List of tokens</returns>
        List<Token> Tokenize(string text);
    }
}
