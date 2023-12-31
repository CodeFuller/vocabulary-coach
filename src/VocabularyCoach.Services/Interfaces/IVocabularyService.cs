using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VocabularyCoach.Models;
using VocabularyCoach.Services.Data;

namespace VocabularyCoach.Services.Interfaces
{
	public interface IVocabularyService
	{
		Task<IReadOnlyCollection<Language>> GetLanguages(CancellationToken cancellationToken);

		Task<IReadOnlyCollection<StudiedText>> GetTextsForPractice(User user, Language studiedLanguage, Language knownLanguage, CancellationToken cancellationToken);

		Task<IReadOnlyCollection<StudiedText>> GetProblematicTexts(User user, Language studiedLanguage, Language knownLanguage, CancellationToken cancellationToken);

		Task<PronunciationRecord> GetPronunciationRecord(ItemId textId, CancellationToken cancellationToken);

		Task<CheckResultType> CheckTypedText(User user, StudiedText studiedText, string typedText, CancellationToken cancellationToken);

		Task<UserStatisticsData> GetTodayUserStatistics(User user, Language studiedLanguage, Language knownLanguage, CancellationToken cancellationToken);

		Task UpdateTodayUserStatistics(User user, Language studiedLanguage, Language knownLanguage, CancellationToken cancellationToken);

		Task<IReadOnlyCollection<UserStatisticsData>> GetUserStatisticsHistory(User user, Language studiedLanguage, Language knownLanguage, CancellationToken cancellationToken);
	}
}
