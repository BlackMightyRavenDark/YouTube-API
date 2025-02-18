using MultiThreadedDownloaderLib;
using System.Collections.Specialized;

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

		internal static YouTubeVideoWebPageResult Get(string videoId, FileDownloader downloader = null)
		{
			string url = Utils.GetYouTubeVideoUrl(videoId);
			if (downloader == null)
			{
				NameValueCollection headers = new NameValueCollection()
				{
					{ "Host", "www.youtube.com" },
					{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:135.0) Gecko/20100101 Firefox/135.0" },
					{ "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
					{ "Accept-Language", "en-US" },
					{ "Accept-Encoding", "gzip" },
					{ "DNT", "1" },
					{ "Sec-GPC", "1" },
					{ "Upgrade-Insecure-Requests", "1" },
					{ "Sec-Fetch-Dest", "document" },
					{ "Sec-Fetch-Mode", "navigate" },
					{ "Sec-Fetch-Site", "none" },
					{ "Sec-Fetch-User", "?1" },
					{ "Priority", "u=0, i" }
				};

				downloader = new FileDownloader() { Headers = headers };
			};

			int errorCode = Utils.DownloadString(url, out string responseWebPageCode, downloader);
			YouTubeVideoWebPage webPage = errorCode == 200 ?
				new YouTubeVideoWebPage(new YouTubeVideoId(videoId), responseWebPageCode, false) : null;
			return new YouTubeVideoWebPageResult(webPage, errorCode);
		}

		public static YouTubeVideoWebPageResult Get(YouTubeVideoId youTubeVideoId, FileDownloader downloader = null)
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

		public YouTubeRawVideoInfoResult ExtractRawVideoInfo()
		{
			return Utils.ExtractRawVideoInfoFromWebPage(this);
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
