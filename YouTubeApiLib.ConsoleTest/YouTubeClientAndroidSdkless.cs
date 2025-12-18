using System.Net;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib.ConsoleTest
{
	internal class YouTubeClientAndroidSdkless : IYouTubeClient
	{
		public string DisplayName => "android sdkless";
		public YouTubeVideoWebPage WebPage { get; private set; }
		public FileDownloader Downloader { get; set; }

		public const string CLIENT_VERSION = "20.10.38";

		public JObject GenerateRequestBody(string videoId, YouTubeConfig youTubeConfig = null)
		{
			if (youTubeConfig == null) { return null; }

			JObject jContentPlaybackContext = new JObject()
			{
				["html5Preference"] = "HTML5_PREF_WANTS",
				["signatureTimestamp"] = youTubeConfig.SignatureTimestamp
			};
			JObject jPlaybackContext = new JObject()
			{
				["contentPlaybackContext"] = jContentPlaybackContext
			};

			JObject jClient = new JObject()
			{
				["clientName"] = "ANDROID",
				["clientVersion"] = CLIENT_VERSION,
				["userAgent"] = $"com.google.android.youtube/{CLIENT_VERSION} (Linux; U; Android 11) gzip",
				["osName"] = "Android",
				["osVersion"] = "11"
			};
			JObject jContext = new JObject()
			{
				["client"] = jClient
			};

			return new JObject()
			{
				["context"] = jContext,
				["playbackContext"] = jPlaybackContext,
				["videoId"] = videoId,
				["contentCheckOk"] = true,
				["racyCheckOk"] = true
			};
		}

		public WebHeaderCollection GenerateRequestHeaders(string videoId, YouTubeConfig youTubeConfig = null)
		{
			return new WebHeaderCollection()
			{
				{ "Origin", Utils.YOUTUBE_URL },
				{ "X-Goog-Visitor-Id", youTubeConfig.VisitorData },
				{ "X-YouTube-Client-Name", "3" },
				{ "X-YouTube-Client-Version", CLIENT_VERSION }
			};
		}

		public YouTubeRawVideoInfoResult GetRawVideoInfo(YouTubeVideoId videoId, out string errorMessage)
		{
			int errorCode = GetRawVideoInfo(videoId.Id, out YouTubeRawVideoInfo rawVideoInfo, out errorMessage);
			return new YouTubeRawVideoInfoResult(rawVideoInfo, errorCode);
		}

		public int GetRawVideoInfo(string videoId, out YouTubeRawVideoInfo rawVideoInfo, out string errorMessage)
		{
			if (WebPage == null)
			{
				YouTubeVideoWebPageResult webPageResult = YouTubeVideoWebPage.Get(new YouTubeVideoId(videoId), Downloader);
				if (webPageResult.ErrorCode == 200) { WebPage = webPageResult.VideoWebPage; }
			}
			if (WebPage != null)
			{
				YouTubeConfig youTubeConfig = WebPage.ExtractYouTubeConfig();
				if (youTubeConfig == null)
				{
					rawVideoInfo = null;
					errorMessage = "YouTubeConfig is not found";
					return 404;
				}

				JObject body = GenerateRequestBody(videoId, youTubeConfig);
				WebHeaderCollection headers = GenerateRequestHeaders(videoId, youTubeConfig);
				int errorCode = Utils.YouTubeHttpPost(YouTubeApiV1.API_V1_PLAYER_URL, body.ToString(), headers, out string response);
				if (errorCode == 200)
				{
					rawVideoInfo = YouTubeRawVideoInfo.MakeFromRaw(response, this);
					errorMessage = null;
					return 200;
				}
				else
				{
					rawVideoInfo = null;
					errorMessage = response;
					return errorCode;
				}
			}

			rawVideoInfo = null;
			errorMessage = null;
			return 404;
		}

		public void SetWebPage(YouTubeVideoWebPage webPage)
		{
			WebPage = webPage;
		}
	}
}
