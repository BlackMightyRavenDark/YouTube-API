
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
		/// Язык оригинала.
		/// </summary>
		public bool IsDefault { get; }

		public YouTubeAudioTrackLanguage(string displayName, string id, bool isDefault)
		{
			DisplayName = displayName;
			Id = id;
			IsDefault = isDefault;
		}
	}
}
