
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

		public YouTubeVideoIdPageResult GetVideoIdPage(YouTubeChannelTabPage channelTabPage, string continuationToken = null)
		{
			return GetVideoIdPage(Id, channelTabPage, continuationToken);
		}

		public static YouTubeVideoIdPageResult GetVideoIdPage(string channelId,
			YouTubeChannelTabPage channelTabPage, string continuationToken = null)
		{
			return YouTubeApiV1.GetVideoIdPage(channelId, channelTabPage, continuationToken);
		}

		public override string ToString()
		{
			return $"{DisplayName} ({Id})";
		}
	}
}
