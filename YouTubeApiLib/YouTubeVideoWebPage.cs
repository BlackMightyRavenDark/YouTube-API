using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public class YouTubeVideoWebPage
	{
		public YouTubeVideoId VideoId { get; }
		public string WebPageCode { get; }
		public bool IsProvidedManually { get; }

		private YouTubeVideoWebPage(YouTubeVideoId videoId,
			string webPageCode, bool isProvidedManually)
		{
			VideoId = videoId;
			WebPageCode = webPageCode;
			IsProvidedManually = isProvidedManually;
		}

		internal static YouTubeVideoWebPageResult Get(string videoId, FileDownloader downloader)
		{
			string url = Utils.GetYouTubeVideoUrl(videoId);
			int errorCode = Utils.DownloadString(url, out string responseWebPageCode, downloader);
			YouTubeVideoWebPage webPage = errorCode == 200 ?
				new YouTubeVideoWebPage(new YouTubeVideoId(videoId), responseWebPageCode, false) : null;
			return new YouTubeVideoWebPageResult(webPage, errorCode);
		}

		public static YouTubeVideoWebPageResult Get(YouTubeVideoId youTubeVideoId, FileDownloader downloader)
		{
			return youTubeVideoId != null ? Get(youTubeVideoId.Id, downloader) : new YouTubeVideoWebPageResult(null, 400);
		}

		public static YouTubeVideoWebPageResult FromCode(string videoId, string webPageCode)
		{
			if (!string.IsNullOrEmpty(webPageCode) && !string.IsNullOrWhiteSpace(webPageCode))
			{
				YouTubeVideoWebPage videoWebPage = MakeFromCode(videoId, webPageCode);
				return new YouTubeVideoWebPageResult(videoWebPage, 200);
			}
			return new YouTubeVideoWebPageResult(null, 404);
		}

		public static YouTubeVideoWebPageResult FromCode(string webPageCode)
		{
			return FromCode(null, webPageCode);
		}

		public static YouTubeVideoWebPage MakeFromCode(YouTubeVideoId videoId, string webPageCode)
		{
			return new YouTubeVideoWebPage(videoId, webPageCode, true);
		}

		public static YouTubeVideoWebPage MakeFromCode(string videoId, string webPageCode)
		{
			YouTubeVideoId youTubeVideoId =
				string.IsNullOrEmpty(videoId) || string.IsNullOrWhiteSpace(videoId) ?
				null : new YouTubeVideoId(videoId);
			return MakeFromCode(youTubeVideoId, webPageCode);
		}

		public static YouTubeVideoWebPage MakeFromCode(string webPageCode)
		{
			return MakeFromCode((YouTubeVideoId)null, webPageCode);
		}

		public YouTubeConfig ExtractYouTubeConfig(string pattern)
		{
			return !string.IsNullOrEmpty(WebPageCode) ?
				Utils.ExtractYouTubeConfigFromWebPageCode(WebPageCode, VideoId?.Id, pattern) : null;
		}

		public YouTubeConfig ExtractYouTubeConfig()
		{
			return !string.IsNullOrEmpty(WebPageCode) ?
				Utils.ExtractYouTubeConfigFromWebPageCode(WebPageCode, VideoId?.Id) : null;
		}
	}
}
