using CodeFuller.Library.Bootstrap;
using CodeFuller.Library.Logging;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VocabularyCoach.Infrastructure.Sqlite;
using VocabularyCoach.Interfaces;
using VocabularyCoach.Internal;
using VocabularyCoach.Services;
using VocabularyCoach.ViewModels;
using VocabularyCoach.ViewModels.Interfaces;

namespace VocabularyCoach
{
	internal sealed class ApplicationBootstrapper : BasicApplicationBootstrapper<ApplicationViewModel>
	{
		protected override void RegisterServices(IServiceCollection services, IConfiguration configuration)
		{
			RegisterViewModels(services);

			services.AddVocabularyCoachServices();
			services.AddVocabularyCoachSqliteDal(settings => configuration.Bind("database", settings));

			services.AddSingleton<IPronunciationRecordPlayer, PronunciationRecordPlayer>();
			services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

			services.AddSingleton<IPronunciationRecordLoader, PronunciationRecordLoader>();
			services.AddSingleton<IWebBrowser, DefaultSystemWebBrowser>();
			services.AddSingleton<IContentDownloader, HttpContentDownloader>();
		}

		private static void RegisterViewModels(IServiceCollection services)
		{
			services.AddSingleton<ApplicationViewModel>();

			services.AddSingleton<IStartPageViewModel, StartPageViewModel>();
			services.AddSingleton<IPracticeVocabularyViewModel, PracticeVocabularyViewModel>();
			services.AddSingleton<ICheckResultsViewModel, CheckResultsViewModel>();
			services.AddSingleton<IEditVocabularyViewModel, EditVocabularyViewModel>();
		}

		protected override void BootstrapLogging(ILoggerFactory loggerFactory, IConfiguration configuration)
		{
			loggerFactory.AddLogging(settings => configuration.Bind("logging", settings));
		}
	}
}
