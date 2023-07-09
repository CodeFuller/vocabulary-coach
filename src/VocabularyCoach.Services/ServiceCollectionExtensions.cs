using Microsoft.Extensions.DependencyInjection;
using VocabularyCoach.Services.Interfaces;
using VocabularyCoach.Services.LanguageTraits;

namespace VocabularyCoach.Services
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddVocabularyCoachServices(this IServiceCollection services)
		{
			services.AddSingleton<IVocabularyService, VocabularyService>();
			services.AddSingleton<IEditVocabularyService, EditVocabularyService>();

			services.AddSingleton<ILanguageTraits, PolishLanguageTraits>();

			return services;
		}
	}
}