using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock;
using VocabularyCoach.Models;
using VocabularyCoach.Services.Internal;
using VocabularyCoach.Services.UnitTests.Helpers;

namespace VocabularyCoach.Services.UnitTests.Internal
{
	[TestClass]
	public class TextsForPracticeSelectorTests
	{
		[TestMethod]
		public void GetTextsForPractice_ForMissingStudiedTexts_ReturnsEmptyCollection()
		{
			// Arrange

			var studiedTexts = Array.Empty<StudiedText>();

			var mocker = new AutoMocker();
			var target = mocker.CreateInstance<TextsForPracticeSelector>();

			// Act

			var result = target.GetTextsForPractice(new DateOnly(2023, 07, 11), studiedTexts);

			// Assert

			result.Should().BeEmpty();
		}

		[TestMethod]
		public void GetTextsForPractice_ForTextsWithNoChecks_ReturnsSuchTexts()
		{
			// Arrange

			var studiedTexts = new[]
			{
				Array.Empty<CheckResult>().ToStudiedText("Text with no checks"),
			};

			var mocker = new AutoMocker();
			var target = mocker.CreateInstance<TextsForPracticeSelector>();

			// Act

			var result = target.GetTextsForPractice(new DateOnly(2023, 07, 11), studiedTexts);

			// Assert

			result.Should().BeEquivalentTo(studiedTexts, x => x.WithoutStrictOrdering());
		}

		[TestMethod]
		public void GetTextsForPractice_ForTextsWithSmallNumberOfAllSuccessfulChecks_TreatsFirstMissingCheckAsFailed()
		{
			// Arrange

			var studiedTexts = new[]
			{
				new[]
				{
					// Next check: +2 days - 2023.07.21
					GetSuccessfulCheck(new DateTime(2023, 07, 19)),
				}.ToStudiedText("Text with 1 successful check - returned"),

				new[]
				{
					// Next check: +2 days - 2023.07.22
					GetSuccessfulCheck(new DateTime(2023, 07, 20)),
				}.ToStudiedText("Text with 1 successful check - not returned"),

				new[]
				{
					// Next check: +3 days - 2023.07.21
					GetSuccessfulCheck(new DateTime(2023, 07, 17)),
					GetSuccessfulCheck(new DateTime(2023, 07, 18)),
				}.ToStudiedText("Text with 2 successful checks - returned"),

				new[]
				{
					// Next check: +3 days - 2023.07.22
					GetSuccessfulCheck(new DateTime(2023, 07, 18)),
					GetSuccessfulCheck(new DateTime(2023, 07, 19)),
				}.ToStudiedText("Text with 2 successful checks - not returned"),

				new[]
				{
					// Next check: +7 days - 2023.07.21
					GetSuccessfulCheck(new DateTime(2023, 07, 12)),
					GetSuccessfulCheck(new DateTime(2023, 07, 13)),
					GetSuccessfulCheck(new DateTime(2023, 07, 14)),
				}.ToStudiedText("Text with 3 successful checks - returned"),

				new[]
				{
					// Next check: +7 days - 2023.07.22
					GetSuccessfulCheck(new DateTime(2023, 07, 13)),
					GetSuccessfulCheck(new DateTime(2023, 07, 14)),
					GetSuccessfulCheck(new DateTime(2023, 07, 15)),
				}.ToStudiedText("Text with 3 successful checks - not returned"),

				new[]
				{
					// Next check: +14 days - 2023.07.21
					GetSuccessfulCheck(new DateTime(2023, 07, 04)),
					GetSuccessfulCheck(new DateTime(2023, 07, 05)),
					GetSuccessfulCheck(new DateTime(2023, 07, 06)),
					GetSuccessfulCheck(new DateTime(2023, 07, 07)),
				}.ToStudiedText("Text with 4 successful checks - returned"),

				new[]
				{
					// Next check: +14 days - 2023.07.22
					GetSuccessfulCheck(new DateTime(2023, 07, 05)),
					GetSuccessfulCheck(new DateTime(2023, 07, 06)),
					GetSuccessfulCheck(new DateTime(2023, 07, 07)),
					GetSuccessfulCheck(new DateTime(2023, 07, 08)),
				}.ToStudiedText("Text with 4 successful checks - not returned"),
			};

			var mocker = new AutoMocker();
			var target = mocker.CreateInstance<TextsForPracticeSelector>();

			// Act

			var result = target.GetTextsForPractice(new DateOnly(2023, 07, 21), studiedTexts);

			// Assert

			var expectedTextsForCheck = new[]
			{
				studiedTexts[0],
				studiedTexts[2],
				studiedTexts[4],
				studiedTexts[6],
			};

			result.Should().BeEquivalentTo(expectedTextsForCheck, x => x.WithoutStrictOrdering());
		}

		[TestMethod]
		public void GetTextsForPractice_ForTextsWithAllSuccessfulChecks_AppliesCorrectCheckInterval()
		{
			// Arrange

			var studiedTexts = new[]
			{
				new[]
				{
					// Next check: +30 days - 2023.07.11
					GetSuccessfulCheck(new DateTime(2023, 06, 07)),
					GetSuccessfulCheck(new DateTime(2023, 06, 08)),
					GetSuccessfulCheck(new DateTime(2023, 06, 09)),
					GetSuccessfulCheck(new DateTime(2023, 06, 10)),
					GetSuccessfulCheck(new DateTime(2023, 06, 11)),
				}.ToStudiedText("Text with 5 successful checks - returned"),

				new[]
				{
					// Next check: +30 days - 2023.07.12
					GetSuccessfulCheck(new DateTime(2023, 06, 08)),
					GetSuccessfulCheck(new DateTime(2023, 06, 09)),
					GetSuccessfulCheck(new DateTime(2023, 06, 10)),
					GetSuccessfulCheck(new DateTime(2023, 06, 11)),
					GetSuccessfulCheck(new DateTime(2023, 06, 12)),
				}.ToStudiedText("Text with 5 successful checks - not returned"),

				new[]
				{
					// Next check: +30 days - 2023.07.11
					GetSuccessfulCheck(new DateTime(2023, 06, 06)),
					GetSuccessfulCheck(new DateTime(2023, 06, 07)),
					GetSuccessfulCheck(new DateTime(2023, 06, 08)),
					GetSuccessfulCheck(new DateTime(2023, 06, 09)),
					GetSuccessfulCheck(new DateTime(2023, 06, 10)),
					GetSuccessfulCheck(new DateTime(2023, 06, 11)),
				}.ToStudiedText("Text with 6 successful checks - returned"),

				new[]
				{
					// Next check: +30 days - 2023.07.11
					GetFailedCheck(new DateTime(2023, 06, 06)),
					GetSuccessfulCheck(new DateTime(2023, 06, 07)),
					GetSuccessfulCheck(new DateTime(2023, 06, 08)),
					GetSuccessfulCheck(new DateTime(2023, 06, 09)),
					GetSuccessfulCheck(new DateTime(2023, 06, 10)),
					GetSuccessfulCheck(new DateTime(2023, 06, 11)),
				}.ToStudiedText("Text with 5 successful checks and old failed check - returned"),

				// This case was added to verify a fix for ArgumentOutOfRangeException when there are 7 or more check results for a text.
				new[]
				{
					// Next check: +30 days - 2023.07.11
					GetFailedCheck(new DateTime(2023, 06, 05)),
					GetSuccessfulCheck(new DateTime(2023, 06, 06)),
					GetSuccessfulCheck(new DateTime(2023, 06, 07)),
					GetSuccessfulCheck(new DateTime(2023, 06, 08)),
					GetSuccessfulCheck(new DateTime(2023, 06, 09)),
					GetSuccessfulCheck(new DateTime(2023, 06, 10)),
					GetSuccessfulCheck(new DateTime(2023, 06, 11)),
				}.ToStudiedText("Text with 7 successful checks - returned"),
			};

			var mocker = new AutoMocker();
			var target = mocker.CreateInstance<TextsForPracticeSelector>();

			// Act

			var result = target.GetTextsForPractice(new DateOnly(2023, 07, 11), studiedTexts);

			// Assert

			var expectedTextsForCheck = new[]
			{
				studiedTexts[0],
				studiedTexts[2],
				studiedTexts[3],
				studiedTexts[4],
			};

			result.Should().BeEquivalentTo(expectedTextsForCheck, x => x.WithoutStrictOrdering());
		}

		[TestMethod]
		public void GetTextsForPractice_ForTextsWithFailedCheck_AppliesCorrectCheckInterval()
		{
			// Arrange

			var studiedTexts = new[]
			{
				new[]
				{
					// Next check: +1 day - 2023.07.21
					GetFailedCheck(new DateTime(2023, 07, 20)),
				}.ToStudiedText("Text with 1st failed check - returned"),

				new[]
				{
					// Next check: +1 day - 2023.07.22
					GetFailedCheck(new DateTime(2023, 07, 21)),
				}.ToStudiedText("Text with 1st failed check - not returned"),

				new[]
				{
					// Next check: +2 days - 2023.07.21
					GetFailedCheck(new DateTime(2023, 07, 18)),
					GetSuccessfulCheck(new DateTime(2023, 07, 19)),
				}.ToStudiedText("Text with 2nd failed check - returned"),

				new[]
				{
					// Next check: +2 days - 2023.07.22
					GetFailedCheck(new DateTime(2023, 07, 19)),
					GetSuccessfulCheck(new DateTime(2023, 07, 20)),
				}.ToStudiedText("Text with 2nd failed check - not returned"),

				new[]
				{
					// Next check: +3 days - 2023.07.21
					GetFailedCheck(new DateTime(2023, 07, 16)),
					GetSuccessfulCheck(new DateTime(2023, 07, 17)),
					GetSuccessfulCheck(new DateTime(2023, 07, 18)),
				}.ToStudiedText("Text with 3rd failed check - returned"),

				new[]
				{
					// Next check: +3 days - 2023.07.22
					GetFailedCheck(new DateTime(2023, 07, 17)),
					GetSuccessfulCheck(new DateTime(2023, 07, 18)),
					GetSuccessfulCheck(new DateTime(2023, 07, 19)),
				}.ToStudiedText("Text with 3rd failed check - not returned"),

				new[]
				{
					// Next check: +7 days - 2023.07.21
					GetFailedCheck(new DateTime(2023, 07, 11)),
					GetSuccessfulCheck(new DateTime(2023, 07, 12)),
					GetSuccessfulCheck(new DateTime(2023, 07, 13)),
					GetSuccessfulCheck(new DateTime(2023, 07, 14)),
				}.ToStudiedText("Text with 4th failed check - returned"),

				new[]
				{
					// Next check: +7 days - 2023.07.22
					GetFailedCheck(new DateTime(2023, 07, 12)),
					GetSuccessfulCheck(new DateTime(2023, 07, 13)),
					GetSuccessfulCheck(new DateTime(2023, 07, 14)),
					GetSuccessfulCheck(new DateTime(2023, 07, 15)),
				}.ToStudiedText("Text with 4th failed check - not returned"),

				new[]
				{
					// Next check: +14 days - 2023.07.21
					GetFailedCheck(new DateTime(2023, 07, 03)),
					GetSuccessfulCheck(new DateTime(2023, 07, 04)),
					GetSuccessfulCheck(new DateTime(2023, 07, 05)),
					GetSuccessfulCheck(new DateTime(2023, 07, 06)),
					GetSuccessfulCheck(new DateTime(2023, 07, 07)),
				}.ToStudiedText("Text with 5th failed check - returned"),

				new[]
				{
					// Next check: +14 days - 2023.07.22
					GetFailedCheck(new DateTime(2023, 07, 04)),
					GetSuccessfulCheck(new DateTime(2023, 07, 05)),
					GetSuccessfulCheck(new DateTime(2023, 07, 06)),
					GetSuccessfulCheck(new DateTime(2023, 07, 07)),
					GetSuccessfulCheck(new DateTime(2023, 07, 08)),
				}.ToStudiedText("Text with 5th failed check - not returned"),
			};

			var mocker = new AutoMocker();
			var target = mocker.CreateInstance<TextsForPracticeSelector>();

			// Act

			var result = target.GetTextsForPractice(new DateOnly(2023, 07, 21), studiedTexts);

			// Assert

			var expectedTextsForCheck = new[]
			{
				studiedTexts[0],
				studiedTexts[2],
				studiedTexts[4],
				studiedTexts[6],
				studiedTexts[8],
			};

			result.Should().BeEquivalentTo(expectedTextsForCheck, x => x.WithoutStrictOrdering());
		}

		[TestMethod]
		public void GetTextsForPractice_ForTextsWithDifferentNextCheckDate_OrdersChecksByNextCheckDate()
		{
			// Arrange

			var studiedTexts = new[]
			{
				new[]
				{
					// Next check: +1 day - 2023.07.11
					GetFailedCheck(new DateTime(2023, 07, 10)),
				}.ToStudiedText("Text with next check on 2023.07.11 - #1"),

				// Next check: MinDate
				Array.Empty<CheckResult>().ToStudiedText("Text with no checks - #1"),

				new[]
				{
					// Next check: +1 day - 2023.07.10
					GetFailedCheck(new DateTime(2023, 07, 09)),
				}.ToStudiedText("Text with next check on 2023.07.10 - #1"),

				new[]
				{
					// Next check: +1 day - 2023.07.11
					GetFailedCheck(new DateTime(2023, 07, 10)),
				}.ToStudiedText("Text with next check on 2023.07.11 - #2"),

				// Next check: MinDate
				Array.Empty<CheckResult>().ToStudiedText("Text with no checks - #2"),

				new[]
				{
					// Next check: +1 day - 2023.07.10
					GetFailedCheck(new DateTime(2023, 07, 09)),
				}.ToStudiedText("Text with next check on 2023.07.10 - #2"),
			};

			var mocker = new AutoMocker();
			var target = mocker.CreateInstance<TextsForPracticeSelector>();

			// Act

			var result = target.GetTextsForPractice(new DateOnly(2023, 07, 11), studiedTexts);

			// Assert

			result.Count.Should().Be(6);

			result.Skip(0).Take(2).Should().BeEquivalentTo(new[] { studiedTexts[1], studiedTexts[4] }, x => x.WithoutStrictOrdering());
			result.Skip(2).Take(2).Should().BeEquivalentTo(new[] { studiedTexts[2], studiedTexts[5] }, x => x.WithoutStrictOrdering());
			result.Skip(4).Take(2).Should().BeEquivalentTo(new[] { studiedTexts[0], studiedTexts[3] }, x => x.WithoutStrictOrdering());
		}

		private static CheckResult GetFailedCheck(DateTimeOffset dateTime)
		{
			return new CheckResult
			{
				DateTime = dateTime,
				CheckResultType = CheckResultType.Misspelled,
			};
		}

		private static CheckResult GetSuccessfulCheck(DateTimeOffset dateTime)
		{
			return new CheckResult
			{
				DateTime = dateTime,
				CheckResultType = CheckResultType.Ok,
			};
		}
	}
}
