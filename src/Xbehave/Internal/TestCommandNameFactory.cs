﻿// <copyright file="TestCommandNameFactory.cs" company="Adam Ralph">
//  Copyright (c) Adam Ralph. All rights reserved.
// </copyright>

namespace Xbehave.Internal
{
    using System.Collections.Generic;
    using System.Linq;

    internal class TestCommandNameFactory : ITestCommandNameFactory
    {
        public string CreateSharedContext(IEnumerable<Step> givens, IEnumerable<Step> whens)
        {
            return string.Concat(Create(givens.Concat(whens)), " { (shared context)");
        }

        public string CreateSharedStep(IEnumerable<Step> givens, IEnumerable<Step> whens, Step then)
        {
            return string.Concat(Create(givens.Concat(whens)), " | ", Create(then));
        }

        public string CreateDisposal(IEnumerable<Step> givens, IEnumerable<Step> whens)
        {
            return string.Concat(Create(givens.Concat(whens)), " } (disposal)");
        }

        public string CreateIsolatedStep(IEnumerable<Step> givens, IEnumerable<Step> whens, Step then)
        {
            return string.Concat(Create(givens.Concat(whens)), ", ", Create(then));
        }

        public string CreateSkippedStep(IEnumerable<Step> givens, IEnumerable<Step> whens, Step then)
        {
            return this.CreateIsolatedStep(givens, whens, then);
        }

        private static string Create(Step step)
        {
            return Create(new[] { step });
        }

        private static string Create(IEnumerable<Step> steps)
        {
            var tokens = steps
                .Where(step => step != null)
                .Select(step => step.Message)
                .Where(token => !string.IsNullOrEmpty(token))
                .Select(token => token.Trim(' ', ','))
                .Where(token => token.Length > 0);

            return string.Join(", ", tokens.ToArray());
        }
    }
}
