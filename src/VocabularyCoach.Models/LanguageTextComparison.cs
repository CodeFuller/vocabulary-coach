using System;
using System.Collections.Generic;

namespace VocabularyCoach.Models
{
	public static class LanguageTextComparison
	{
		// Ordinal string comparison could not be used due to local language characters (e.g. ł or ó).
		public static StringComparison IgnoreCase => StringComparison.InvariantCultureIgnoreCase;

		public static StringComparer IgnoreCaseEqualityComparer => StringComparer.InvariantCultureIgnoreCase;
	}
}
