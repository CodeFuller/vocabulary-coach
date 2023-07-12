using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VocabularyCoach.Models;
using VocabularyCoach.Services.Extensions;
using VocabularyCoach.Services.Interfaces;
using VocabularyCoach.Services.Interfaces.Repositories;
using VocabularyCoach.Services.Internal;

namespace VocabularyCoach.Services
{
	internal sealed class VocabularyService : IVocabularyService
	{
		private static readonly IReadOnlyList<int> CheckIntervals = new List<int>
		{
			// If the last (^1) check was failed, we add 1 day to the last check date.
			+1,

			// If ^1 check was successful, but ^2 check was failed, we add 2 days to the last check date.
			+2,

			// ...
			+3,
			+7,
			+14,

			// The last interval is added for texts with all successful checks.
			+30,
		};

		private readonly ILanguageRepository languageRepository;

		private readonly ILanguageTextRepository languageTextRepository;

		private readonly IPronunciationRecordRepository pronunciationRecordRepository;

		private readonly ICheckResultRepository checkResultRepository;

		private readonly ISystemClock systemClock;

		public VocabularyService(ILanguageRepository languageRepository, ILanguageTextRepository languageTextRepository,
			IPronunciationRecordRepository pronunciationRecordRepository, ICheckResultRepository checkResultRepository, ISystemClock systemClock)
		{
			this.languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
			this.languageTextRepository = languageTextRepository ?? throw new ArgumentNullException(nameof(languageTextRepository));
			this.pronunciationRecordRepository = pronunciationRecordRepository ?? throw new ArgumentNullException(nameof(pronunciationRecordRepository));
			this.checkResultRepository = checkResultRepository ?? throw new ArgumentNullException(nameof(checkResultRepository));
			this.systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		}

		public async Task<IReadOnlyCollection<Language>> GetLanguages(CancellationToken cancellationToken)
		{
			var languages = await languageRepository.GetLanguages(cancellationToken);

			return languages.OrderBy(x => x.Name).ToList();
		}

		public async Task<IReadOnlyCollection<StudiedText>> GetTextsForCheck(User user, Language studiedLanguage, Language knownLanguage, CancellationToken cancellationToken)
		{
			var studiedTexts = await languageTextRepository.GetStudiedTexts(user.Id, studiedLanguage.Id, knownLanguage.Id, cancellationToken);

			return studiedTexts
				.Select(x => new
				{
					StudiedText = x,
					NextCheckDateTime = GetNextCheckDateTimeForStudiedText(x.CheckResults).Date,
				})
				.Where(x => x.NextCheckDateTime <= systemClock.Now.Date)
				.GroupBy(x => x.NextCheckDateTime, x => x.StudiedText)
				.OrderBy(x => x.Key)
				.SelectMany(x => x.Randomize())
				.ToList();
		}

		private static DateTimeOffset GetNextCheckDateTimeForStudiedText(IReadOnlyList<CheckResult> checkResults)
		{
			if (!checkResults.Any())
			{
				// If text was not yet checked, this is the highest priority for practice.
				return DateTimeOffset.MinValue;
			}

			var lastCheckDate = checkResults[0].DateTime;

			for (var i = 0; i < Math.Max(checkResults.Count, CheckIntervals.Count); ++i)
			{
				// If all text checks are successful, however they are not enough - we add interval for first missing check.
				if (i >= checkResults.Count)
				{
					return lastCheckDate.AddDays(CheckIntervals[i]);
				}

				// If some check in the past is failed, we add interval for latest failed check.
				if (checkResults[i].CheckResultType != CheckResultType.Ok)
				{
					return lastCheckDate.AddDays(CheckIntervals[i]);
				}
			}

			// If all checks are successful, we add the last interval.
			return lastCheckDate.DateTime.AddDays(CheckIntervals[^1]);
		}

		public Task<PronunciationRecord> GetPronunciationRecord(ItemId textId, CancellationToken cancellationToken)
		{
			return pronunciationRecordRepository.GetPronunciationRecord(textId, cancellationToken);
		}

		public async Task<CheckResultType> CheckTypedText(User user, StudiedText studiedText, string typedText, CancellationToken cancellationToken)
		{
			var checkResult = new CheckResult
			{
				DateTime = systemClock.Now,
				CheckResultType = GetCheckResultType(studiedText.TextInStudiedLanguage, typedText),
			};

			await checkResultRepository.AddCheckResult(user.Id, studiedText.TextInStudiedLanguage.Id, checkResult, cancellationToken);

			studiedText.AddCheckResult(checkResult);

			return checkResult.CheckResultType;
		}

		private static CheckResultType GetCheckResultType(LanguageText languageText, string typedText)
		{
			if (String.IsNullOrEmpty(typedText))
			{
				return CheckResultType.Skipped;
			}

			return String.Equals(languageText.Text, typedText, StringComparison.OrdinalIgnoreCase)
				? CheckResultType.Ok
				: CheckResultType.Misspelled;
		}
	}
}
