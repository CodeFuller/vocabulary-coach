using Microsoft.EntityFrameworkCore;
using VocabularyCoach.Infrastructure.Sqlite.Entities;

namespace VocabularyCoach.Infrastructure.Sqlite.Internal
{
	internal sealed class VocabularyCoachDbContext : DbContext
	{
		public DbSet<LanguageEntity> Languages { get; set; }

		public DbSet<TextEntity> Texts { get; set; }

		public DbSet<TranslationEntity> Translations { get; set; }

		public DbSet<PronunciationRecordEntity> PronunciationRecords { get; set; }

		public DbSet<CheckResultEntity> CheckResults { get; set; }

		public DbSet<UserStatisticsEntity> UserStatistics { get; set; }

		public DbSet<UserSettingsEntity> UserSettings { get; set; }

		public VocabularyCoachDbContext(DbContextOptions options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<TranslationEntity>(builder =>
			{
				builder.HasKey(x => new { x.Text1Id, x.Text2Id });
			});

			modelBuilder.Entity<UserStatisticsEntity>(builder =>
			{
				builder.HasKey(x => new { x.UserId, x.StudiedLanguageId, x.KnownLanguageId, x.Date });
			});

			modelBuilder.Entity<UserSettingsEntity>(builder =>
			{
				builder.HasKey(x => x.UserId);
			});
		}
	}
}
