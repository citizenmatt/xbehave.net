﻿// <copyright file="Step.cs" company="xBehave.net contributors">
//  Copyright (c) xBehave.net contributors. All rights reserved.
// </copyright>

namespace Xbehave.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides the implementation to execute each step.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Step", Justification = "By design.")]
    public class Step
    {
        private readonly string name;
        private readonly Func<object> body;
        private readonly List<IDisposable> disposables = new List<IDisposable>();
        private readonly List<Action> teardowns = new List<Action>();

        public Step(string name, Action body)
            : this(name)
        {
            this.body = () =>
            {
                body();
                return null;
            };
        }

        public Step(string name, Func<Task> body)
            : this(name)
        {
            this.body = body;
        }

        private Step(string name)
        {
            this.name = name;
        }

        public virtual string Name
        {
            get { return this.name; }
        }

        public virtual Func<object> Body
        {
            get { return this.body; }
        }

        public IEnumerable<IDisposable> Disposables
        {
            get { return this.disposables.ToArray(); }
        }

        public IEnumerable<Action> Teardowns
        {
            get { return this.teardowns.ToArray(); }
        }

        public string SkipReason { get; set; }

        public void AddDisposable(IDisposable disposable)
        {
            if (disposable != null)
            {
                this.disposables.Add(disposable);
            }
        }

        public void AddTeardown(Action teardown)
        {
            if (teardown != null)
            {
                this.teardowns.Add(teardown);
            }
        }
    }
}
