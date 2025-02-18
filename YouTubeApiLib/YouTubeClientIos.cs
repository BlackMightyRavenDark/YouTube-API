using System;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public class YouTubeClientIos : IYouTubeClient
	{
		public string DisplayName => "IOS";
		public YouTubeVideoWebPage WebPage { get; private set; }
		public FileDownloader Downloader { get; set; }
		public IosDevice Device { get; }

		public YouTubeClientIos() : this(null) { }

		public YouTubeClientIos(IosDevice device)
		{
			Device = device ?? new IosDevice("Apple", "iPhone16,2", "IOS", 18, 1, 0, "22B83", "IOS", 19, 45, 4);
		}

		public JObject GenerateRequestBody(string videoId, YouTubeConfig youTubeConfig = null)
		{
			if (youTubeConfig == null)
			{
				youTubeConfig = WebPage?.ExtractYouTubeConfig();
				if (youTubeConfig == null) { return null; }
			}

			int sts = youTubeConfig.SignatureTimestamp;
			string visitorData = youTubeConfig.VisitorData;
			string clientVersionString = $"{Device.ClientVersionMajor}.{Device.ClientVersionMinor}.{Device.ClientVersionPatch}";
			string osVersionString = $"{Device.OsVersionMajor}.{Device.OsVersionMinor}.{Device.OsVersionPatch}.{Device.OsVersionBuild}";

			JObject jClient = new JObject()
			{
				["clientName"] = Device.ClientName,
				["clientVersion"] = clientVersionString,
				["deviceMake"] = Device.DeviceMaker,
				["deviceModel"] = Device.DeviceModel,
				["platform"] = "MOBILE",
				["osName"] = Device.OsName,
				["osVersion"] = osVersionString,
				["hl"] = "en",
				["gl"] = "US",
				["timeZone"] = "UTC",
				["utcOffsetMinutes"] = 0
			};
			if (!string.IsNullOrEmpty(visitorData))
			{
				jClient["visitorData"] = visitorData;
			}

			JObject jContentPlaybackContext = new JObject()
			{
				["html5Preference"] = "HTML5_PREF_WANTS",
				["signatureTimestamp"] = sts
			};
			JObject jPlaybackContext = new JObject() { ["contentPlaybackContext"] = jContentPlaybackContext };
			JObject jContext = new JObject() { ["client"] = jClient };
			JObject jBody = new JObject()
			{
				["context"] = jContext,
				["videoId"] = videoId,
				["playbackContext"] = jPlaybackContext,
				["contentCheckOk"] = true,
				["racyCheckOk"] = true
			};

			return jBody;
		}

		public NameValueCollection GenerateRequestHeaders(string videoId, YouTubeConfig youTubeConfig = null)
		{
			if (youTubeConfig == null)
			{
				youTubeConfig = WebPage?.ExtractYouTubeConfig();
				if (youTubeConfig == null) { return null; }
			}
			
			string visitorData = youTubeConfig.VisitorData;
			string clientVersionString = $"{Device.ClientVersionMajor}.{Device.ClientVersionMinor}.{Device.ClientVersionPatch}";
			NameValueCollection headers = new NameValueCollection()
			{
				{ "Origin", Utils.YOUTUBE_URL },
				{ "X-Goog-Visitor-Id", visitorData },
				{ "X-YouTube-Client-Name", "5" },
				{ "X-YouTube-Client-Version", clientVersionString },
				{ "User-Agent", Device.UserAgent }
			};

			return headers;
		}

		public YouTubeRawVideoInfoResult GetRawVideoInfo(YouTubeVideoId videoId, out string errorMessage)
		{
			int errorCode = GetRawVideoInfo(videoId.Id, out YouTubeRawVideoInfo rawVideoInfo, out errorMessage);
			return new YouTubeRawVideoInfoResult(rawVideoInfo, errorCode);
		}

		public int GetRawVideoInfo(string videoId, out YouTubeRawVideoInfo rawVideoInfo, out string errorMessage)
		{
			try
			{
				string webPageCode = WebPage?.WebPageCode;
				YouTubeVideoWebPageResult webPageResult = !string.IsNullOrEmpty(webPageCode) && !string.IsNullOrWhiteSpace(webPageCode) ?
					new YouTubeVideoWebPageResult(WebPage, 200) :
					YouTubeVideoWebPage.Get(videoId, Downloader);
				if (webPageResult.ErrorCode == 200)
				{
					YouTubeConfig config = webPageResult.VideoWebPage.ExtractYouTubeConfig();
					if (config == null)
					{
						rawVideoInfo = null;
						errorMessage = "YouTubeConfig is not found!";
						return 404;
					}

					NameValueCollection headers = GenerateRequestHeaders(videoId, config);
					JObject body = GenerateRequestBody(videoId, config);
					int errorCode = YouTubeApiV1.CallPlayerApi(headers, body.ToString(), out string response);
					if (errorCode == 200)
					{
						rawVideoInfo = new YouTubeRawVideoInfo(response, this,
							new YouTubeMediaTrackUrlDecryptionData(webPageResult.VideoWebPage));
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
				return webPageResult.ErrorCode;
			} catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				rawVideoInfo = null;
				errorMessage = ex.Message;
				return ex.HResult;
			}
		}

		public void SetWebPage(YouTubeVideoWebPage webPage)
		{
			WebPage = webPage;
		}
	}
}
