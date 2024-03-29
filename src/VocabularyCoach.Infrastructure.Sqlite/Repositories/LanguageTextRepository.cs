using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VocabularyCoach.Infrastructure.Sqlite.Entities;
using VocabularyCoach.Infrastructure.Sqlite.Extensions;
using VocabularyCoach.Infrastructure.Sqlite.Internal;
using VocabularyCoach.Models;
using VocabularyCoach.Services.Data;
using VocabularyCoach.Services.Interfaces.Repositories;

namespace VocabularyCoach.Infrastructure.Sqlite.Repositories
{
	internal sealed class LanguageTextRepository : ILanguageTextRepository
	{
		private readonly IDbContextFactory<VocabularyCoachDbContext> contextFactory;

		public LanguageTextRepository(IDbContextFactory<VocabularyCoachDbContext> contextFactory)
		{
			this.contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
		}

		public async Task<IReadOnlyCollection<LanguageText>> GetLanguageTexts(ItemId languageId, CancellationToken cancellationToken)
		{
			await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);

			var texts = await dbContext.Texts
				.Include(x => x.Language)
				.Where(x => x.LanguageId == languageId.ToInt32())
				.ToListAsync(cancellationToken);

			return texts.Select(x => x.ToModel()).ToList();
		}

		public async Task<IReadOnlyCollection<Translation>> GetTranslations(ItemId language1Id, ItemId language2Id, CancellationToken cancellationToken)
		{
			await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);

			var language1DbId = language1Id.ToInt32();
			var language2DbId = language2Id.ToInt32();

			var translations = await GetTranslationsQueryable(dbContext, language1DbId, language2DbId)
				.Concat(GetTranslationsQueryable(dbContext, language2DbId, language1DbId))
				.ToListAsync(cancellationToken);

			return translations
				.Select(x => x.ToModel(language1DbId, language2DbId))
				.ToList();
		}

		public async Task<IReadOnlyCollection<StudiedTranslationData>> GetStudiedTranslations(ItemId userId, ItemId studiedLanguageId, ItemId knownLanguageId, CancellationToken cancellationToken)
		{
			await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);

			var studiedLanguageDbId = studiedLanguageId.ToInt32();
			var knownLanguageDbId = knownLanguageId.ToInt32();

			var translations1 = await GetTranslationsQueryable(dbContext, studiedLanguageDbId, knownLanguageDbId)
				.Include(x => x.Text1)
				.ToListAsync(cancellationToken);

			var translations2 = await GetTranslationsQueryable(dbContext, knownLanguageDbId, studiedLanguageDbId)
				.Include(x => x.Text2)
				.ToListAsync(cancellationToken);

			// We used to load CheckResults as JOIN with Texts, i.e. dbContext.Translations.Include(x => x.Text1).ThenInclude(x => x.CheckResults.Where(y => y.UserId == userId.ToInt32())).
			// However this approach works very slow.
			// That is why we replaced it with a trick: CheckResults are loaded with a separate query.
			// This load will populate CheckResults property in TextEntity unless it has no check results yet.
			await dbContext.CheckResults.Where(x => x.UserId == userId.ToInt32()).ToListAsync(cancellationToken);

			return translations1.Concat(translations2)
				.Select(x => x.ToStudiedTranslationData(studiedLanguageDbId, knownLanguageDbId))
				.ToList();
		}

		private static IQueryable<TranslationEntity> GetTranslationsQueryable(VocabularyCoachDbContext dbContext, int language1Id, int language2Id)
		{
			return dbContext.Translations
				.Include(x => x.Text1).ThenInclude(x => x.Language)
				.Include(x => x.Text2).ThenInclude(x => x.Language)
				.Where(x => x.Text1.LanguageId == language1Id)
				.Where(x => x.Text2.LanguageId == language2Id);
		}

		public async Task AddLanguageText(LanguageText languageText, CancellationToken cancellationToken)
		{
			var textEntity = languageText.ToEntity();

			await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);

			await dbContext.Texts.AddAsync(textEntity, cancellationToken);
			await dbContext.SaveChangesAsync(cancellationToken);

			languageText.Id = textEntity.Id.ToItemId();
		}

		public async Task AddTranslation(Translation translation, CancellationToken cancellationToken)
		{
			await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);

			var translationEntity = translation.ToEntity();

			await dbContext.Translations.AddAsync(translationEntity, cancellationToken);
			await dbContext.SaveChangesAsync(cancellationToken);
		}

		public async Task UpdateLanguageText(LanguageText languageText, CancellationToken cancellationToken)
		{
			await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);

			var textEntity = await dbContext.Texts.SingleAsync(x => x.Id == languageText.Id.ToInt32(), cancellationToken);

			textEntity.Text = languageText.Text;
			textEntity.Note = languageText.Note;
			await dbContext.SaveChangesAsync(cancellationToken);
		}

		public async Task DeleteLanguageText(LanguageText languageText, CancellationToken cancellationToken)
		{
			await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);

			var textEntity = await dbContext.Texts.SingleAsync(x => x.Id == languageText.Id.ToInt32(), cancellationToken);

			dbContext.Texts.Remove(textEntity);
			await dbContext.SaveChangesAsync(cancellationToken);
		}

		public async Task DeleteTranslation(Translation translation, CancellationToken cancellationToken)
		{
			await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);

			var translationEntity = await dbContext.Translations.SingleAsync(x => x.Text1Id == translation.Text1.Id.ToInt32() && x.Text2Id == translation.Text2.Id.ToInt32(), cancellationToken);

			dbContext.Translations.Remove(translationEntity);
			await dbContext.SaveChangesAsync(cancellationToken);
		}
	}
}
