using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public class YouTubeChannelWebPage : InternetWebPage
	{
		public static int GetChannelTabWebPageCode(string channelId, YouTubeChannelTabPage channelTabPage,
			out string response, FileDownloader downloader = null)
		{
			string url = channelTabPage.GetWebPageUrl(channelId);
			return DownloadWebPageCode(url, out response, downloader);
		}

		public static int GetChannelTabWebPageCode(YouTubeChannel channel, YouTubeChannelTabPage channelTabPage,
			out string response, FileDownloader downloader = null)
		{
			return GetChannelTabWebPageCode(channel.Id, channelTabPage, out response, downloader);
		}
	}
}
