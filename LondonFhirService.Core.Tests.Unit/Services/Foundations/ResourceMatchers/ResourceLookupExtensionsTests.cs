// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers
{
    public class ResourceLookupExtensionsTests
    {
        [Fact]
        public void ShouldBuildLookupForEveryResourceWhenMatchKeysAreDistinct()
        {
            // given
            MatchCandidate firstCandidate = CreateRandomMatchCandidate(keySuffix: 1);
            MatchCandidate secondCandidate = CreateRandomMatchCandidate(keySuffix: 2);
            MatchCandidate thirdCandidate = CreateRandomMatchCandidate(keySuffix: 3);

            var randomCandidates = new List<MatchCandidate>
            {
                firstCandidate,
                secondCandidate,
                thirdCandidate
            };

            var expectedLookup = new Dictionary<string, string>
            {
                { firstCandidate.MatchKey, firstCandidate.Payload },
                { secondCandidate.MatchKey, secondCandidate.Payload },
                { thirdCandidate.MatchKey, thirdCandidate.Payload }
            };

            // when
            Dictionary<string, string> actualLookup =
                randomCandidates.ToDictionaryFirstWins(
                    candidate => candidate.MatchKey,
                    candidate => candidate.Payload);

            // then
            actualLookup.Should().BeEquivalentTo(expectedLookup);
        }

        [Fact]
        public void ShouldMatchToDictionaryBehaviourWhenMatchKeysAreDistinct()
        {
            // given
            MatchCandidate firstCandidate = CreateRandomMatchCandidate(keySuffix: 1);
            MatchCandidate secondCandidate = CreateRandomMatchCandidate(keySuffix: 2);

            var randomCandidates = new List<MatchCandidate>
            {
                firstCandidate,
                secondCandidate
            };

            Dictionary<string, string> expectedLookup =
                randomCandidates.ToDictionary(
                    candidate => candidate.MatchKey,
                    candidate => candidate.Payload);

            // when
            Dictionary<string, string> actualLookup =
                randomCandidates.ToDictionaryFirstWins(
                    candidate => candidate.MatchKey,
                    candidate => candidate.Payload);

            // then
            actualLookup.Should().BeEquivalentTo(expectedLookup);
        }

        [Fact]
        public void ShouldKeepFirstResourceAndDropLaterOnesWhenMatchKeyIsDuplicated()
        {
            // given
            string duplicatedMatchKey = CreateRandomMatchKey(keySuffix: 1);
            string distinctMatchKey = CreateRandomMatchKey(keySuffix: 2);

            var firstCandidateForDuplicatedKey =
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: $"{GetRandomString()}-first");

            var laterCandidateForDuplicatedKey =
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: $"{GetRandomString()}-later");

            var candidateForDistinctKey =
                new MatchCandidate(MatchKey: distinctMatchKey, Payload: GetRandomString());

            var randomCandidates = new List<MatchCandidate>
            {
                firstCandidateForDuplicatedKey,
                laterCandidateForDuplicatedKey,
                candidateForDistinctKey
            };

            var expectedLookup = new Dictionary<string, string>
            {
                { duplicatedMatchKey, firstCandidateForDuplicatedKey.Payload },
                { distinctMatchKey, candidateForDistinctKey.Payload }
            };

            // when
            Dictionary<string, string> actualLookup =
                randomCandidates.ToDictionaryFirstWins(
                    candidate => candidate.MatchKey,
                    candidate => candidate.Payload);

            // then
            actualLookup.Should().BeEquivalentTo(expectedLookup);
            actualLookup[duplicatedMatchKey].Should().Be(firstCandidateForDuplicatedKey.Payload);
            actualLookup[duplicatedMatchKey].Should().NotBe(laterCandidateForDuplicatedKey.Payload);
        }

        [Fact]
        public void ShouldTolerateDuplicateMatchKeysInsteadOfThrowingLikeToDictionaryDoes()
        {
            // given
            string duplicatedMatchKey = CreateRandomMatchKey(keySuffix: 1);

            var randomCandidates = new List<MatchCandidate>
            {
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: GetRandomString()),
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: GetRandomString())
            };

            Action plainToDictionaryAction = () =>
                randomCandidates.ToDictionary(
                    candidate => candidate.MatchKey,
                    candidate => candidate.Payload);

            Action toDictionaryFirstWinsAction = () =>
                randomCandidates.ToDictionaryFirstWins(
                    candidate => candidate.MatchKey,
                    candidate => candidate.Payload);

            // when
            // then
            plainToDictionaryAction.Should().Throw<ArgumentException>();
            toDictionaryFirstWinsAction.Should().NotThrow();
        }

        [Fact]
        public void ShouldCollapseToSingleEntryWhenEveryMatchKeyIsTheSame()
        {
            // given
            string duplicatedMatchKey = CreateRandomMatchKey(keySuffix: 1);

            var firstCandidate =
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: GetRandomString());

            var randomCandidates = new List<MatchCandidate>
            {
                firstCandidate,
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: GetRandomString()),
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: GetRandomString())
            };

            // when
            Dictionary<string, string> actualLookup =
                randomCandidates.ToDictionaryFirstWins(
                    candidate => candidate.MatchKey,
                    candidate => candidate.Payload);

            // then
            actualLookup.Should().HaveCount(1);
            actualLookup.Should().ContainKey(duplicatedMatchKey);
            actualLookup[duplicatedMatchKey].Should().Be(firstCandidate.Payload);
        }

        [Fact]
        public void ShouldReturnEmptyLookupWhenSourceIsEmpty()
        {
            // given
            var emptyCandidates = new List<MatchCandidate>();

            // when
            Dictionary<string, string> actualLookup =
                emptyCandidates.ToDictionaryFirstWins(
                    candidate => candidate.MatchKey,
                    candidate => candidate.Payload);

            // then
            actualLookup.Should().NotBeNull();
            actualLookup.Should().BeEmpty();
        }

        [Fact]
        public void ShouldApplyValueSelectorRatherThanStoringTheSourceItem()
        {
            // given
            MatchCandidate randomCandidate = CreateRandomMatchCandidate(keySuffix: 1);
            string expectedValue = randomCandidate.Payload;

            var randomCandidates = new List<MatchCandidate>
            {
                randomCandidate
            };

            // when
            Dictionary<string, string> actualLookup =
                randomCandidates.ToDictionaryFirstWins(
                    candidate => candidate.MatchKey,
                    candidate => candidate.Payload);

            // then
            actualLookup[randomCandidate.MatchKey].Should().Be(expectedValue);
            actualLookup[randomCandidate.MatchKey].Should().NotBe(randomCandidate.MatchKey);
        }

        [Fact]
        public void ShouldHandBackTheDuplicateValueWhenMatchKeyIsDuplicated()
        {
            // given
            string duplicatedMatchKey = CreateRandomMatchKey(keySuffix: 1);
            int valueSelectorCallCount = 0;

            var randomCandidates = new List<MatchCandidate>
            {
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: GetRandomString()),
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: GetRandomString())
            };

            Func<MatchCandidate, string> countingValueSelector = candidate =>
            {
                valueSelectorCallCount++;

                return candidate.Payload;
            };

            // when
            Dictionary<string, string> actualLookup =
                randomCandidates.ToDictionaryFirstWins(
                    candidate => candidate.MatchKey,
                    countingValueSelector);

            // then
            // The value selector runs for the duplicate too, because the dropped value is handed
            // to onDuplicate so callers can surface it rather than lose it. Only the first
            // occurrence reaches the lookup.
            actualLookup.Should().HaveCount(1);
            valueSelectorCallCount.Should().Be(2);
        }

        [Fact]
        public void ShouldHandEveryDroppedDuplicateToTheCallbackWithItsKeyAndValue()
        {
            // given
            // This callback is what stops a dropped resource disappearing from the comparison
            // report. Without it the lookup is lossy and silent.
            string duplicatedMatchKey = CreateRandomMatchKey(keySuffix: 1);
            string firstPayload = GetRandomString();
            string secondPayload = GetRandomString();
            string thirdPayload = GetRandomString();
            MatchCandidate distinctCandidate = CreateRandomMatchCandidate(keySuffix: 2);

            var randomCandidates = new List<MatchCandidate>
            {
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: firstPayload),
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: secondPayload),
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: thirdPayload),
                distinctCandidate
            };

            var actualDuplicates = new List<(string Key, string Value)>();

            // when
            Dictionary<string, string> actualLookup =
                randomCandidates.ToDictionaryFirstWins(
                    candidate => candidate.MatchKey,
                    candidate => candidate.Payload,
                    onDuplicate: (key, value) => actualDuplicates.Add((key, value)));

            // then
            // The first occurrence wins the lookup; every later one is reported, in order, and a
            // key seen only once never reaches the callback.
            actualLookup.Should().HaveCount(2);
            actualLookup[duplicatedMatchKey].Should().Be(firstPayload);

            actualDuplicates.Should().Equal(
                (duplicatedMatchKey, secondPayload),
                (duplicatedMatchKey, thirdPayload));
        }

        [Fact]
        public void ShouldNotRequireACallbackWhenDuplicatesAreNotOfInterest()
        {
            // given
            string duplicatedMatchKey = CreateRandomMatchKey(keySuffix: 1);

            var randomCandidates = new List<MatchCandidate>
            {
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: GetRandomString()),
                new MatchCandidate(MatchKey: duplicatedMatchKey, Payload: GetRandomString())
            };

            // when
            Action buildLookup = () =>
                randomCandidates.ToDictionaryFirstWins(
                    candidate => candidate.MatchKey,
                    candidate => candidate.Payload);

            // then
            // The callback is optional - omitting it must not throw a null reference.
            buildLookup.Should().NotThrow();
        }

        private static MatchCandidate CreateRandomMatchCandidate(int keySuffix) =>
            new MatchCandidate(
                MatchKey: CreateRandomMatchKey(keySuffix),
                Payload: GetRandomString());

        private static string CreateRandomMatchKey(int keySuffix) =>
            $"{GetRandomString()}-{keySuffix}";

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private record MatchCandidate(string MatchKey, string Payload);
    }
}
