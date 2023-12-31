using System;
using System.Threading;
using System.Threading.Tasks;
using VocabularyCoach.Models;
using VocabularyCoach.Services.Interfaces;
using VocabularyCoach.Services.Internal;
using VocabularyCoach.Services.LanguageTraits;

namespace VocabularyCoach.Services
{
	internal class SpellCheckService : ISpellCheckService
	{
		private readonly ISupportedLanguageTraits supportedLanguageTraits;

		private readonly IWebBrowser webBrowser;

		public SpellCheckService(ISupportedLanguageTraits supportedLanguageTraits, IWebBrowser webBrowser)
		{
			this.supportedLanguageTraits = supportedLanguageTraits ?? throw new ArgumentNullException(nameof(supportedLanguageTraits));
			this.webBrowser = webBrowser ?? throw new ArgumentNullException(nameof(webBrowser));
		}

		// This is a naive implementation of spell check.
		// It just opens corresponding page in wiktionary.org for a user check.
		public Task<bool> PerformSpellCheck(LanguageText languageText, CancellationToken cancellationToken)
		{
			var urlForSpellCheck = supportedLanguageTraits.GetLanguageTraits(languageText.Language)
				.GetUrlForSpellCheck(languageText.Text);

			webBrowser.OpenPage(urlForSpellCheck);

			return Task.FromResult(true);
		}
	}
}
