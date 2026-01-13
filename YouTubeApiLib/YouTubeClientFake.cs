using System.Net;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	internal class YouTubeClientFake : IYouTubeClient
	{
		public string DisplayName { get; }
		public YouTubeVideoWebPage WebPage { get; }
		public FileDownloader Downloader { get; set; }

		internal YouTubeClientFake(string displayName)
		{
			DisplayName = displayName; 
		}

		public JObject GenerateRequestBody(string videoId, YouTubeConfig youTubeConfig = null)
		{
			return null;
		}

		public WebHeaderCollection GenerateRequestHeaders(string videoId, YouTubeConfig youTubeConfig = null)
		{
			return null;
		}

		public YouTubeRawVideoInfoResult GetRawVideoInfo(YouTubeVideoId videoId, out string errorMessage)
		{
			errorMessage = null;
			return null;
		}

		public int GetRawVideoInfo(string videoId, out YouTubeRawVideoInfo rawVideoInfo, out string errorMessage)
		{
			errorMessage = null;
			rawVideoInfo = null;
			return 400;
		}

		public void SetWebPage(YouTubeVideoWebPage webPage) { }
	}
}
