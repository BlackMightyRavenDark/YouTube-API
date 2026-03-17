using System.Net;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib.GuiTestWPF
{
	internal class YouTubeClientAndroidVr : IYouTubeClient
	{
		public string DisplayName => "android vr";
		public YouTubeVideoWebPage WebPage { get; private set; }
		public FileDownloader Downloader { get; set; }

		public const string CLIENT_VERSION = "1.71.26";

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
				["clientName"] = "ANDROID_VR",
				["clientVersion"] = CLIENT_VERSION,
				["deviceMake"] = "Oculus",
				["deviceModel"] = "Quest 3",
				["androidSdkVersion"] = 32,
				["userAgent"] = $"com.google.android.apps.youtube.vr.oculus/{CLIENT_VERSION} (Linux; U; Android 12L; eureka-user Build/SQ3A.220605.009.A1) gzip",
				["osName"] = "Android",
				["osVersion"] = "12L"
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
				{ "Origin", YouTubeApiLib.Utils.YOUTUBE_URL },
				{ "X-Goog-Visitor-Id", youTubeConfig.VisitorData },
				{ "X-YouTube-Client-Name", "28" },
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
				int errorCode = YouTubeApiLib.Utils.YouTubeHttpPost(YouTubeApiV1.API_V1_PLAYER_URL, body.ToString(), headers, out string response);
				if (errorCode == 200)
				{
					rawVideoInfo = YouTubeRawVideoInfo.MakeFromRaw(response, this, System.DateTime.UtcNow);
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
