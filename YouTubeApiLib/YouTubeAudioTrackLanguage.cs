
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

		public YouTubeAudioTrackLanguage(string displayName, string id, bool isDefault, bool isOriginal)
		{
			DisplayName = displayName;
			Id = id;
			IsDefault = isDefault;
			IsOriginal = isOriginal;
		}
	}
}
