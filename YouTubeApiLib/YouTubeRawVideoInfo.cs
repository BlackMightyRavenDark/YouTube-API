using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;
using static YouTubeApiLib.Utils;

namespace YouTubeApiLib
{
	public class YouTubeRawVideoInfo
	{
		public string RawData { get; private set; }

		/// <summary>
		/// The YouTube client which used for getting info.
		/// </summary>
		public IYouTubeClient Client { get; }

		public YouTubeMediaTrackUrlDecryptionData UrlDecryptionData { get; }

		public YouTubeVideoPlayabilityStatus PlayabilityStatus => ExtractPlayabilityStatus();
		public YouTubeStreamingDataResult StreamingData => ExtractStreamingData();
		public YouTubeVideoDetails VideoDetails => ExtractVideoDetails();
		public JObject Microformat => ExtractMicroformat();

		private JObject _parsedData = null;

		public YouTubeRawVideoInfo(string rawData, IYouTubeClient client, YouTubeMediaTrackUrlDecryptionData urlDecryptionData)
		{
			RawData = rawData;
			Client = client;
			UrlDecryptionData = urlDecryptionData;
		}

		public static YouTubeRawVideoInfoResult Get(YouTubeVideoId videoId, IYouTubeClient client)
		{
			return videoId != null && client != null ? GetRawVideoInfo(videoId.Id, client) :
				new YouTubeRawVideoInfoResult(null, 400);
		}

		public static YouTubeRawVideoInfoResult Get(string videoId, IYouTubeClient client)
		{
			int errorCode = client.GetRawVideoInfo(videoId, out YouTubeRawVideoInfo rawVideoInfo, out _);
			return new YouTubeRawVideoInfoResult(rawVideoInfo, errorCode);
		}

		public static YouTubeRawVideoInfoResult Get(YouTubeVideoId videoId, FileDownloader downloader = null)
		{
			return Get(videoId.Id, downloader);
		}

		public static YouTubeRawVideoInfoResult Get(string videoId, FileDownloader downloader = null)
		{
			IYouTubeClient client = YouTubeApi.GetYouTubeClient(YouTubeApi.GetDefaultYouTubeClientId());
			if (client == null) { return new YouTubeRawVideoInfoResult(null, 400); }
			client.Downloader = downloader;
			return Get(videoId, client);
		}

		public static YouTubeRawVideoInfo MakeFromRaw(string rawData, IYouTubeClient client,
			YouTubeMediaTrackUrlDecryptionData urlDecryptionData = null)
		{
			return new YouTubeRawVideoInfo(rawData, client, urlDecryptionData);
		}

		public static YouTubeRawVideoInfo MakeFromRaw(string rawData)
		{
			return MakeFromRaw(rawData, null);
		}

		public YouTubeSimplifiedVideoInfoResult Simplify(YouTubeStreamingData customStreamingData = null)
		{
			return SimplifyRawVideoInfo(this, customStreamingData);
		}

		private YouTubeVideoPlayabilityStatus ExtractPlayabilityStatus()
		{
			if (PreParse())
			{
				JObject jPlayabilityStatus = _parsedData.Value<JObject>("playabilityStatus");
				return jPlayabilityStatus != null ? YouTubeVideoPlayabilityStatus.Parse(jPlayabilityStatus) : null;
			}

			return null;
		}

		private YouTubeStreamingDataResult ExtractStreamingData()
		{
			if (PreParse())
			{
				JObject jStreamingData = _parsedData.Value<JObject>("streamingData");
				if (jStreamingData != null)
				{
					YouTubeStreamingData streamingData = new YouTubeStreamingData(
						jStreamingData.ToString(), Client, UrlDecryptionData);
					return new YouTubeStreamingDataResult(streamingData, 200);
				}
			}

			return new YouTubeStreamingDataResult(null, 404);
		}

		private YouTubeVideoDetails ExtractVideoDetails()
		{
			if (PreParse())
			{
				JObject j = _parsedData.Value<JObject>("videoDetails");
				return j != null ? new YouTubeVideoDetails(j.ToString(), Client) : null;
			}

			return null;
		}

		private JObject ExtractMicroformat()
		{
			return PreParse() ? _parsedData.Value<JObject>("microformat") : null;
		}

		public YouTubeVideo ToVideo()
		{
			return MakeYouTubeVideo(this);
		}

		public bool PreParse()
		{
			if (_parsedData != null) { return true; }
			_parsedData = TryParseJson(RawData);
			return _parsedData != null;
		}

		public void FormatRawData()
		{
			if (PreParse())
			{
				RawData = _parsedData.ToString();
			}
		}

		public override string ToString()
		{
			return RawData ?? "null";
		}
	}
}
