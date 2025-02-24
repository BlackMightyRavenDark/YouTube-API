
namespace YouTubeApiLib
{
	public abstract class YouTubeChannelTabPage
	{
		public string Title { get; protected set; }
		public string ParamsId { get; protected set; }
		public string UrlSuffix { get; protected set; }

		public virtual string GetWebPageUrl(string channelId)
		{
			return $"{Utils.YOUTUBE_URL}/channel/{channelId}/{GetWebPageUrlSuffix()}";
		}

		public virtual string GetWebPageUrlSuffix()
		{
			return !string.IsNullOrEmpty(UrlSuffix) ? UrlSuffix.ToLower() : Title.ToLower();
		}
	}
}
