using System;
using VocabularyCoach.Models;
using VocabularyCoach.ViewModels.Extensions;

namespace VocabularyCoach.ViewModels
{
	public sealed class LanguageTextViewModel
	{
		public LanguageText LanguageText { get; }

		public string Text => LanguageText.Text;

		public string Note => LanguageText.Note;

		public string TextWithNote => LanguageText.GetTextWithNote();

		public LanguageTextViewModel(LanguageText languageText)
		{
			LanguageText = languageText ?? throw new ArgumentNullException(nameof(languageText));
		}

		public override string ToString()
		{
			return TextWithNote;
		}
	}
}
