using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	internal class YouTubeClientWebPage : IYouTubeClient
	{
		public string DisplayName => "Web page";
		public FileDownloader Downloader { get; set; }

		public JObject GenerateRequestBody(string videoId, YouTubeConfig youTubeConfig)
		{
			return null;
		}

		public NameValueCollection GenerateRequestHeaders(string videoId, YouTubeConfig youTubeConfig)
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
				YouTubeMediaTrackUrlDecryptionData urlDecryptionData = new YouTubeMediaTrackUrlDecryptionData(webPageResult.VideoWebPage);
				string raw = Utils.ExtractRawVideoInfoFromWebPageCode(webPageResult.VideoWebPage.WebPageCode);
				rawVideoInfo = new YouTubeRawVideoInfo(raw, this, urlDecryptionData);
				return webPageResult.ErrorCode;
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

		public override string ToString()
		{
			return DisplayName;
		}
	}
}
