using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VocabularyCoach.Models;

namespace VocabularyCoach.ViewModels.Interfaces
{
	public interface IEditVocabularyViewModel : IPageViewModel
	{
		ObservableCollection<LanguageText> TextsInStudiedLanguage { get; }

		Language StudiedLanguage { get; }

		Language KnownLanguage { get; }

		string TextInStudiedLanguage { get; set; }

		bool TextInStudiedLanguageIsFilled { get; }

#pragma warning disable CA1056 // URI-like properties should not be strings
		string PronunciationRecordUrl { get; set; }
#pragma warning restore CA1056 // URI-like properties should not be strings

		bool PronunciationRecordUrlIsFilled { get; }

		string TextInKnownLanguage { get; set; }

		string NoteInKnownLanguage { get; set; }

		ICommand CheckTextCommand { get; }

		ICommand PlayPronunciationRecordCommand { get; }

		ICommand SaveChangesCommand { get; }

		ICommand ClearChangesCommand { get; }

		ICommand GoToStartPageCommand { get; }

		Task Load(Language studiedLanguage, Language knownLanguage, CancellationToken cancellationToken);
	}
}
