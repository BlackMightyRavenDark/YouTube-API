using System;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	public class YouTubeClientIos : IYouTubeClient
	{
		public string DisplayName => "IOS";

		private readonly IosDevice DEVICE; 

		private YouTubeVideoWebPage _videoWebPage;

		public YouTubeClientIos() : this(null) { }

		public YouTubeClientIos(YouTubeVideoWebPage videoWebPage)
		{
			DEVICE = new IosDevice("Apple", "iPhone16,2", "IOS", 18, 1, 0, "22B83", "IOS", 19, 45, 4);
			_videoWebPage = videoWebPage;
		}

		public JObject GenerateRequestBody(string videoId, YouTubeConfig youTubeConfig = null)
		{
			if (youTubeConfig == null)
			{
				youTubeConfig = _videoWebPage?.ExtractYouTubeConfig();
				if (youTubeConfig == null) { return null; }
			}

			int sts = youTubeConfig.SignatureTimestamp;
			string visitorData = Utils.GetVisitorData(DEVICE.UserAgent);
			string clientVersionString = $"{DEVICE.ClientVersionMajor}.{DEVICE.ClientVersionMinor}.{DEVICE.ClientVersionPatch}";
			string osVersionString = $"{DEVICE.OsVersionMajor}.{DEVICE.OsVersionMinor}.{DEVICE.OsVersionPatch}.{DEVICE.OsVersionBuild}";

			JObject jClient = new JObject()
			{
				["clientName"] = DEVICE.ClientName,
				["clientVersion"] = clientVersionString,
				["deviceMake"] = DEVICE.DeviceMaker,
				["deviceModel"] = DEVICE.DeviceModel,
				["userAgent"] = DEVICE.UserAgent,
				["osName"] = DEVICE.OsName,
				["osVersion"] = osVersionString,
				["hl"] = "en",
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
				youTubeConfig = _videoWebPage?.ExtractYouTubeConfig();
				if (youTubeConfig == null) { return null; }
			}
			
			string visitorData = youTubeConfig.VisitorData;
			string clientVersionString = $"{DEVICE.ClientVersionMajor}.{DEVICE.ClientVersionMinor}.{DEVICE.ClientVersionPatch}";
			NameValueCollection headers = new NameValueCollection()
			{
				{ "Origin", Utils.YOUTUBE_URL },
				{ "X-Goog-Visitor-Id", visitorData },
				{ "X-YouTube-Client-Name", "5" },
				{ "X-YouTube-Client-Version", clientVersionString },
				{ "User-Agent", DEVICE.UserAgent }
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
				YouTubeVideoWebPageResult webPageResult = _videoWebPage != null ?
					new YouTubeVideoWebPageResult(_videoWebPage, 200) :
					YouTubeVideoWebPage.Get(videoId);
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
	}
}
