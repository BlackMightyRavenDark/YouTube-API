
namespace YouTubeApiLib
{
	public class YouTubeAudioTrackLanguage
	{
		public string DisplayName { get; }

		/// <summary>
		/// Код языка.
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// Дорожка по-умолчанию.
		/// </summary>
		public bool IsDefault { get; }

		/// <summary>
		/// Язык оригинала.
		/// </summary>
		public bool IsOriginal { get; }

		/// <summary>
		/// Звуковая дорожка создана автоматически с использованием машинного перевода и озвучки.
		/// Перевёл с оригинала и озвучил, скорее всего, робот сам, без помощи кожаных мешков.
		/// </summary>
		public bool IsAutoDubbed { get; }

		public YouTubeAudioTrackLanguage(string displayName, string id, bool isDefault, bool isAutoDubbed)
		{
			DisplayName = displayName;
			Id = id;
			IsDefault = isDefault;
			IsOriginal = !isAutoDubbed && !string.IsNullOrEmpty(displayName) && !string.IsNullOrWhiteSpace(displayName) &&
				(displayName.Contains("original") || displayName.Contains("оригинал"));
			IsAutoDubbed = isAutoDubbed;
		}
	}
}
