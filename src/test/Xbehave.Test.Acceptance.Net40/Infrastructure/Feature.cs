﻿// <copyright file="Feature.cs" company="xBehave.net contributors">
//  Copyright (c) xBehave.net contributors. All rights reserved.
// </copyright>

#if V2
namespace Xbehave.Test.Acceptance.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using LiteGuard;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class Feature : IDisposable
    {
        private readonly IList<Xunit2> runners = new List<Xunit2>();

        ~Feature()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IMessageSinkMessage[] Run<TMessage1, TMessage2>(Type feature)
            where TMessage1 : IMessageSinkMessage
            where TMessage2 : IMessageSinkMessage
        {
            var messages = this.Run(feature);
            return messages.OfType<TMessage1>().Cast<IMessageSinkMessage>()
                .Concat(messages.OfType<TMessage2>().Cast<IMessageSinkMessage>())
                .ToArray();
        }

        public TMessage[] Run<TMessage>(Assembly assembly, string collectionName)
            where TMessage : IMessageSinkMessage
        {
            return this.Run(assembly, collectionName).OfType<TMessage>().ToArray();
        }

        public TMessage[] Run<TMessage>(Type feature)
            where TMessage : IMessageSinkMessage
        {
            return this.Run(feature).OfType<TMessage>().ToArray();
        }

        public IMessageSinkMessage[] Run(Assembly assembly, string collectionName)
        {
            this.runners.Add(new Xunit2(new NullSourceInformationProvider(), assembly.GetLocalCodeBase()));
            var runner = this.runners.Last();
            return runner.Run(runner.Find(collectionName)).ToArray();
        }

        public IMessageSinkMessage[] Run(Type feature)
        {
            Guard.AgainstNullArgument("feature", feature);

            this.runners.Add(new Xunit2(new NullSourceInformationProvider(), feature.Assembly.GetLocalCodeBase()));
            var runner = this.runners.Last();
            return runner.Run(runner.Find(feature)).ToArray();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Exception exception = null;
                foreach (var runner in this.runners.Reverse())
                {
                    try
                    {
                        runner.Dispose();
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                }

                if (exception != null)
                {
                    ExceptionDispatchInfo.Capture(exception).Throw();
                }
            }
        }
    }
}
#else
namespace Xbehave.Test.Acceptance.Infrastructure
{
    using System;
    using System.Linq;
    using System.Threading;
    using FakeItEasy;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public abstract class Feature : IDisposable
    {
        ~Feature()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IMessageSinkMessage[] Run<TMessage1, TMessage2>(Type feature)
            where TMessage1 : IMessageSinkMessage
            where TMessage2 : IMessageSinkMessage
        {
            var messages = this.Run(feature);
            return messages.OfType<TMessage1>().Cast<IMessageSinkMessage>()
                .Concat(messages.OfType<TMessage2>().Cast<IMessageSinkMessage>())
                .ToArray();
        }

        public TMessage[] Run<TMessage>(Type feature)
            where TMessage : IMessageSinkMessage
        {
            return this.Run(feature).OfType<TMessage>().ToArray();
        }

        public IMessageSinkMessage[] Run(Type feature)
        {
            var command = new TestClassCommand(feature);
            IMessageSinkMessage[] results = null;
            var thread = new Thread(() => results = TestClassCommandRunner
                .Execute(command, command.EnumerateTestMethods().ToList(), null, null)
                .Results
                .Select(Map)
                .Where(message => message != null)
                .ToArray());

            thread.Start();
            thread.Join();

            return results;
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        private static IMessageSinkMessage Map(ITestResult result)
        {
            return Map(result as PassedResult) ?? Map(result as SkipResult) ?? Map(result as FailedResult);
        }

        private static IMessageSinkMessage Map(MethodResult result)
        {
            if (result == null)
            {
                return null;
            }

            var message = A.Fake<ITestPassed>();
            A.CallTo(() => message.Test.DisplayName).Returns(result.DisplayName);
            return message;
        }

        private static IMessageSinkMessage Map(SkipResult result)
        {
            if (result == null)
            {
                return null;
            }

            var message = A.Fake<ITestSkipped>();
            A.CallTo(() => message.Test.DisplayName).Returns(result.DisplayName);
            A.CallTo(() => message.Reason).Returns(result.Reason);
            return message;
        }

        private static IMessageSinkMessage Map(FailedResult result)
        {
            if (result == null)
            {
                return null;
            }

            var message = A.Fake<ITestFailed>();
            A.CallTo(() => message.Test.DisplayName).Returns(result.DisplayName);
            A.CallTo(() => message.Messages).Returns(new[] { result.Message });
            A.CallTo(() => message.ExceptionTypes).Returns(new[] { result.ExceptionType });
            A.CallTo(() => message.StackTraces).Returns(new[] { result.StackTrace });
            return message;
        }
    }
}
#endif
