﻿// <copyright file="ScenarioFeature.cs" company="xBehave.net contributors">
//  Copyright (c) xBehave.net contributors. All rights reserved.
// </copyright>

namespace Xbehave.Test.Acceptance
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using FluentAssertions;
    using Xbehave.Test.Acceptance.Infrastructure;
    using Xunit;
    using Xunit.Abstractions;

    // In order to prevent bugs due to incorrect code
    // As a developer
    // I want to run automated acceptance tests describing each feature of my product using scenarios
    public class ScenarioFeature : Feature
    {
        // NOTE (adamralph): a plain xunit fact to prove that plain scenarios work in 2.x
        [Fact]
        public void ScenarioWithTwoPassingStepsAndOneFailingStepYieldsTwoPassesAndOneFail()
        {
            // arrange
            var feature = typeof(FeatureWithAScenarioWithTwoPassingStepsAndOneFailingStep);

            // act
            var results = this.Run<ITestResultMessage>(feature);

            // assert
            results.Length.Should().Be(3);
            results.Take(2).Should().ContainItemsAssignableTo<ITestPassed>();
            results.Skip(2).Should().ContainItemsAssignableTo<ITestFailed>();
        }

        [Scenario]
        public void ScenarioWithThreeSteps()
        {
            var feature = default(Type);
            var results = default(ITestResultMessage[]);

            "Given a feature with a scenario with three steps"
                .f(() => feature = typeof(FeatureWithAScenarioWithThreeSteps));

            "When I run the scenarios"
                .f(() => results = this.Run<ITestResultMessage>(feature));

            "Then there should be three results"
                .f(() => results.Length.Should().Be(3));

            "And the first result should have a display name ending with 'Step 1'"
                .f(() => results[0].Test.DisplayName.Should().EndWith("Step 1"));

            "And the second result should have a display name ending with 'Step 2'"
                .f(() => results[1].Test.DisplayName.Should().EndWith("Step 2"));

            "And the third result should have a display name ending with 'Step 3'"
                .f(() => results[2].Test.DisplayName.Should().EndWith("Step 3"));
        }

        [Scenario]
        public void OrderingStepsByDisplayName()
        {
            var feature = default(Type);
            var results = default(ITestResultMessage[]);

            "Given ten steps named alphabetically backwards starting with 'z'"
                .f(() => feature = typeof(TenStepsNamedAlphabeticallyBackwardsStartingWithZ));

            "When I run the scenarios"
                .f(() => results = this.Run<ITestResultMessage>(feature));

            "And I sort the results by their display name"
                .f(() => results = results.OrderBy(result => result.Test.DisplayName).ToArray());

            "Then a concatenation of the last character of each result display names should be 'zyxwvutsrq'"
                .f(() => new string(results.Select(result => result.Test.DisplayName.Last()).ToArray())
                    .Should().Be("zyxwvutsrq"));
        }

        [Scenario]
        public void ScenarioWithTwoPassingStepsAndOneFailingStep()
        {
            var feature = default(Type);
            var results = default(ITestResultMessage[]);

            "Given a feature with a scenario with two passing steps and one failing step"
                .f(() => feature = typeof(FeatureWithAScenarioWithTwoPassingStepsAndOneFailingStep));

            "When I run the scenarios"
                .f(() => results = this.Run<ITestResultMessage>(feature));

            "Then there should be three results"
                .f(() => results.Length.Should().Be(3));

            "And the first two results should be passes"
                .f(() => results.Take(2).Should().ContainItemsAssignableTo<ITestPassed>());

            "And the third result should be a fail"
                .f(() => results.Skip(2).Should().ContainItemsAssignableTo<ITestFailed>());
        }

        [Scenario]
        public void ScenarioBodyThrowsAnException()
        {
            var feature = default(Type);
            var exception = default(Exception);
            var results = default(ITestResultMessage[]);

            "Given a feature with a scenario body which throws an exception"
                .f(() => feature = typeof(FeatureWithAScenarioBodyWhichThrowsAnException));

            "When I run the scenarios"
                .f(() => exception = Record.Exception(() =>
                    results = this.Run<ITestResultMessage>(feature)));

            "Then no exception should be thrown"
                .f(() => exception.Should().BeNull());

            "And the results should not be empty"
                .f(() => results.Should().NotBeEmpty());

            "And each result should be a failure"
                .f(() => results.Should().ContainItemsAssignableTo<ITestFailed>());
        }

        [Scenario]
        public void FeatureCannotBeConstructed()
        {
            var feature = default(Type);
            var exception = default(Exception);
            var results = default(ITestResultMessage[]);

            "Given a feature with a non-static scenario but no default constructor"
                .f(() => feature = typeof(FeatureWithANonStaticScenarioButNoDefaultConstructor));

            "When I run the scenarios"
                .f(() => exception = Record.Exception(() =>
                    results = this.Run<ITestResultMessage>(feature)));

            "Then no exception should be thrown"
                .f(() => exception.Should().BeNull());

            "And the results should not be empty"
                .f(() => results.Should().NotBeEmpty());

            "And each result should be a failure"
                .f(() => results.Should().ContainItemsAssignableTo<ITestFailed>());
        }

        [Scenario]
        public void FailingStepFollowedByPassingSteps()
        {
            var feature = default(Type);
            var results = default(ITestResultMessage[]);

            "Given a failing step and two passing steps named alphabetically backwards"
                .f(() => feature = typeof(AFailingStepAndTwoPassingStepsNamedAlphabeticallyBackwards));

            "When I run the scenario"
                .f(() => results = this.Run<ITestResultMessage>(feature));

            "And I sort the results by their display name"
                .f(() => results = results.OrderBy(result => result.Test.DisplayName).ToArray());

            "Then each result should be a failure"
                .f(() => results.Should().ContainItemsAssignableTo<ITestFailed>());

            "And the second result should refer to the second step"
                .f(() => results[1].Test.DisplayName.Should().ContainEquivalentOf("Step y"));

            "And the third result should refer to the third step"
                .f(() => results[2].Test.DisplayName.Should().ContainEquivalentOf("Step x"));

            "And each subsequent result message should indicate that the first step failed"
                .f(() =>
                {
                    foreach (var result in results.Cast<ITestFailed>().Skip(1))
                    {
                        result.Messages.Single().Should().ContainEquivalentOf("Failed to execute preceding step");
                        result.Messages.Single().Should().ContainEquivalentOf("Step z");
                    }
                });
        }

        private static class FeatureWithAScenarioWithThreeSteps
        {
            [Scenario]
            public static void Scenario()
            {
                "Step 1"
                    .f(() => { });

                "Step 2"
                    .f(() => { });

                "Step 3"
                    .f(() => { });
            }
        }

        private static class TenStepsNamedAlphabeticallyBackwardsStartingWithZ
        {
            [Scenario]
            public static void Scenario()
            {
                "z"
                    .f(() => { });

                "y"
                    .f(() => { });

                "x"
                    .f(() => { });

                "w"
                    .f(() => { });

                "v"
                    .f(() => { });

                "u"
                    .f(() => { });

                "t"
                    .f(() => { });

                "s"
                    .f(() => { });

                "r"
                    .f(() => { });

                "q"
                    .f(() => { });
            }
        }

        private static class FeatureWithAScenarioWithTwoPassingStepsAndOneFailingStep
        {
            [Scenario]
            public static void Scenario()
            {
                var i = 0;

                "Given 1"
                    .f(() => i = 1);

                "When I add 1"
                    .f(() => i += 1);

                "Then I have 3"
                    .f(() => i.Should().Be(3));
            }
        }

        private static class FeatureWithAScenarioBodyWhichThrowsAnException
        {
            [Scenario]
            public static void Scenario()
            {
                throw new InvalidOperationException();
            }
        }

        private static class AFailingStepAndTwoPassingStepsNamedAlphabeticallyBackwards
        {
            [Scenario]
            public static void Scenario()
            {
                "Step z"
                    .f(() =>
                    {
                        throw new NotImplementedException();
                    });

                "Step y"
                    .f(() => { });

                "Step x"
                    .f(() => { });
            }
        }

        private class FeatureWithANonStaticScenarioButNoDefaultConstructor
        {
            public FeatureWithANonStaticScenarioButNoDefaultConstructor(int ignored)
            {
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for testing.")]
            [Scenario]
            public void Scenario()
            {
                "Given something"
                    .f(() => { });
            }
        }
    }
}
