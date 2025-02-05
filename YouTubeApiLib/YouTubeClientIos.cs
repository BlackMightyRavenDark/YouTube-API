using System;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	public class YouTubeClientIos : IYouTubeClient
	{
		public string DisplayName => "IOS";

		private const string CLIENT_VERSION = "19.29.1";
		private readonly string USER_AGENT;

		private YouTubeVideoWebPage _videoWebPage;

		public YouTubeClientIos() : this(null) { }

		public YouTubeClientIos(YouTubeVideoWebPage videoWebPage)
		{
			USER_AGENT = $"com.google.ios.youtube/{CLIENT_VERSION} (iPhone16,2; U; CPU iOS 17_5_1 like Mac OS X;)";
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
			string visitorData = Utils.GetVisitorData(USER_AGENT);

			JObject jClient = new JObject()
			{
				["clientName"] = "IOS",
				["clientVersion"] = CLIENT_VERSION,
				["deviceMake"] = "Apple",
				["deviceModel"] = "iPhone16,2",
				["userAgent"] = USER_AGENT,
				["osName"] = "iPhone",
				["osVersion"] = "17.5.1.21F90",
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
			NameValueCollection headers = new NameValueCollection()
			{
				{ "Origin", Utils.YOUTUBE_URL },
				{ "X-Goog-Visitor-Id", visitorData },
				{ "X-YouTube-Client-Name", "5" },
				{ "X-YouTube-Client-Version", CLIENT_VERSION },
				{ "User-Agent", USER_AGENT }
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
