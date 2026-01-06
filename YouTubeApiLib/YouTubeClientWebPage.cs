using System.Net;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public class YouTubeClientWebPage : IYouTubeClient
	{
		public string DisplayName => "Web page";
		public YouTubeVideoWebPage WebPage { get; private set; }
		public FileDownloader Downloader { get; set; }

		public JObject GenerateRequestBody(string videoId, YouTubeConfig youTubeConfig)
		{
			return null;
		}

		public WebHeaderCollection GenerateRequestHeaders(string videoId, YouTubeConfig youTubeConfig)
		{
			return null;
		}

		public YouTubeRawVideoInfoResult GetRawVideoInfo(YouTubeVideoId videoId, out string errorMessage)
		{
			int errorCode = GetRawVideoInfo(videoId.Id, out YouTubeRawVideoInfo rawVideoInfo, out errorMessage);
			return new YouTubeRawVideoInfoResult(rawVideoInfo, errorCode);
		}

		public int GetRawVideoInfo(string videoId, out YouTubeRawVideoInfo rawVideoInfo, out string errorMessage)
		{
			errorMessage = null;
			YouTubeVideoWebPageResult webPageResult = GetWebPage(videoId);
			if (webPageResult.ErrorCode == 200)
			{
				SetWebPage(webPageResult.VideoWebPage);
				YouTubeMediaTrackUrlDecryptionData urlDecryptionData = new YouTubeMediaTrackUrlDecryptionData(webPageResult.VideoWebPage);
				string raw = Utils.ExtractRawVideoInfoFromWebPageCode(webPageResult.VideoWebPage.WebPageCode);
				rawVideoInfo = new YouTubeRawVideoInfo(raw, this, urlDecryptionData, System.DateTime.UtcNow);
				return webPageResult.ErrorCode;
			}
			else
			{
				SetWebPage(null);
			}

			rawVideoInfo = null;
			return webPageResult.ErrorCode;
		}

		public YouTubeVideoWebPageResult GetWebPage(YouTubeVideoId videoId)
		{
			return GetWebPage(videoId.Id);
		}

		public YouTubeVideoWebPageResult GetWebPage(string videoId)
		{
			return YouTubeVideoWebPage.Get(videoId, Downloader);
		}

		public void SetWebPage(YouTubeVideoWebPage webPage)
		{
			WebPage = webPage;
		}

		public override string ToString()
		{
			return DisplayName;
		}
	}
}
