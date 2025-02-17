using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public class YouTubeClientVideoInfo : IYouTubeClient
	{
		public string DisplayName => "Video info client";
		public FileDownloader Downloader { get; set; }

		public JObject GenerateRequestBody(string videoId, YouTubeConfig youTubeConfig = null)
		{
			const string CLIENT_NAME = "WEB";
			const string CLIENT_VERSION = "2.20201021.03.00";

			JObject jClient = YouTubeApiV1.GenerateYouTubeClientBody(CLIENT_NAME, CLIENT_VERSION);
			string visitorData = youTubeConfig != null ? youTubeConfig.VisitorData : Utils.GetYouTubeVisitorData();
			if (!string.IsNullOrEmpty(visitorData))
			{
				jClient["visitorData"] = visitorData;
			}

			JObject jContext = new JObject
			{
				["client"] = jClient
			};

			JObject json = new JObject
			{
				["context"] = jContext
			};
			json["videoId"] = videoId;

			return json;
		}

		public NameValueCollection GenerateRequestHeaders(string videoId, YouTubeConfig youTubeConfig = null)
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
			JObject body = GenerateRequestBody(videoId);
			string url = YouTubeApiV1.GetPlayerRequestUrl();
			int errorCode = Utils.YouTubeHttpPost(url, body.ToString(), out string response);
			rawVideoInfo = errorCode == 200 ? new YouTubeRawVideoInfo(response, this, null) : null;
			errorMessage = null;
			return errorCode;
		}
	}
}
