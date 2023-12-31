using System;

namespace VocabularyCoach.Services.Internal
{
	internal class SystemClock : ISystemClock
	{
		public DateTimeOffset Now => DateTimeOffset.Now;

		public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
	}
}
