﻿// <copyright file="ScenarioTimeoutFeature.cs" company="xBehave.net contributors">
//  Copyright (c) xBehave.net contributors. All rights reserved.
// </copyright>

#if !V2
namespace Xbehave.Test.Acceptance
{
    using System;
    using System.Linq;
#if NET40 || NET45
    using System.Threading;
#endif
    using FluentAssertions;
    using Xbehave.Test.Acceptance.Infrastructure;
    using Xunit.Abstractions;

    // In order to prevent very long running tests <-- improve!
    // As a developer
    // I want a feature to fail if a given scenario takes to long to run
    public class ScenarioTimeoutFeature : Feature
    {
#if NET40 || NET45
        private static readonly ManualResetEventSlim @event = new ManualResetEventSlim();
#endif

        [Scenario]
        public void ScenarioExecutesFastEnough()
        {
            var feature = default(Type);
            var results = default(ITestResultMessage[]);

            "Given a feature with a scenario which does not exceed it's timeout"
                .Given(() => feature = typeof(ScenarioFastEnough));

            "When I run the scenarios"
                .When(() => results = this.Run<ITestResultMessage>(feature));

            "Then there should be one result"
                .Then(() => results.Count().Should().Be(1));

            "And the result should be a pass"
                .And(() => results.Should().ContainItemsAssignableTo<ITestPassed>());
        }

#if NET40 || NET45
        [Scenario(Skip = "See https://github.com/xbehave/xbehave.net/issues/93/")]
        public void ScenarioExecutesTooSlowly()
        {
            var feature = default(Type);
            var results = default(ITestResultMessage[]);

            "Given a feature with a scenario which exceeds it's 1ms timeout"
                .Given(() => feature = typeof(ScenarioTooSlow));

            "When I run the scenarios"
                .When(() =>
                {
                    @event.Reset();
                    results = this.Run<ITestResultMessage>(feature);
                })
                .Teardown(() => @event.Set());

            "Then there should be one result"
                .Then(() => results.Count().Should().Be(1));

            "And the result should be a failure"
                .And(() => results.Should().ContainItemsAssignableTo<ITestFailed>());

            "And the result message should be \"Test execution time exceeded: 1ms\""
                .And(() => results.Cast<ITestFailed>().Should().OnlyContain(result =>
                    result.Messages.Single() == "Test execution time exceeded: 1ms"));
        }

        [Scenario(Skip = "See https://github.com/xbehave/xbehave.net/issues/93/")]
        public void ScenarioExecutesTooSlowlyInOneStepAndHasASubsequentPassingStep()
        {
            var feature = default(Type);
            var results = default(ITestResultMessage[]);

            "Given a feature with a scenario which exceeds it's 1ms timeout"
                .Given(() => feature = typeof(ScenarioTooSlowInOneStepAndHasASubsequentPassingStep));

            "When I run the scenarios"
                .When(() =>
                {
                    @event.Reset();
                    results = this.Run<ITestResultMessage>(feature);
                })
                .Teardown(() => @event.Set());

            "Then there should be two result"
                .Then(() => results.Count().Should().Be(2));

            "And the results should be failures"
                .And(() => results.Should().ContainItemsAssignableTo<ITestFailed>());

            "And the first result message should be \"Test execution time exceeded: 1ms\""
                .And(() => results.Cast<ITestFailed>().Should().OnlyContain(result =>
                    result.Messages.Single() == "Test execution time exceeded: 1ms"));
        }
#endif

        private static class ScenarioFastEnough
        {
#pragma warning disable 618
            [Scenario(Timeout = int.MaxValue)]
#pragma warning restore 618
            public static void Scenario()
            {
                "Given something"
                    .Given(() => { });
            }
        }

#if NET40 || NET45
        private static class ScenarioTooSlow
        {
#pragma warning disable 618
            [Scenario(Timeout = 1)]
#pragma warning restore 618
            public static void Scenario()
            {
                "Given something"
                    .Given(() => @event.Wait());
            }
        }

        private static class ScenarioTooSlowInOneStepAndHasASubsequentPassingStep
        {
#pragma warning disable 618
            [Scenario(Timeout = 1)]
#pragma warning restore 618
            public static void Scenario()
            {
                "Given something"
                    .Given(() => @event.Wait());

                "Then true is true"
                    .Then(() => true.Should().BeTrue());
            }
        }
#endif
    }
}
#endif
