
namespace YouTubeApiLib
{
	public class YouTubeChannel
	{
		public string DisplayName { get; }
		public string Id { get; }

		public YouTubeChannel(string id, string displayName)
		{
			Id = id;
			DisplayName = displayName;
		}

		public YouTubeVideoIdPageResult GetVideoIdPage(YouTubeChannelTabPage channelTabPage, string continuationToken)
		{
			return GetVideoIdPage(Id, channelTabPage, continuationToken);
		}

		public static YouTubeVideoIdPageResult GetVideoIdPage(string channelId,
			YouTubeChannelTabPage channelTabPage, string continuationToken)
		{
			return YouTubeApiV1.GetVideoIdPage(channelId, channelTabPage, continuationToken);
		}

		public static YouTubeVideoIdPageResult GetVideoIdPage(string channelId, YouTubeChannelTabPage channelTabPage)
		{
			return YouTubeApiV1.GetVideoIdPage(channelId, channelTabPage);
		}

		public YouTubeVideoIdPageResult GetVideoIdPage(YouTubeChannelTabPage channelTabPage)
		{
			return GetVideoIdPage(Id, channelTabPage);
		}

		public string GetTabPageUrl(YouTubeChannelTabPage channelTabPage)
		{
			return GetTabPageUrl(Id, channelTabPage);
		}

		public static string GetTabPageUrl(string channelId, YouTubeChannelTabPage channelTabPage)
		{
			return channelTabPage.GetWebPageUrl(channelId);
		}

		public override string ToString()
		{
			return $"{DisplayName} ({Id})";
		}
	}
}
