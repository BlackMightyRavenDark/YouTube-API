using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;
using static YouTubeApiLib.Utils;

namespace YouTubeApiLib
{
	public class YouTubeRawVideoInfo
	{
		public string RawData { get; private set; }

		/// <summary>
		/// Клиент YouTube, которым была получена информация.
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

		public static YouTubeRawVideoInfoResult Get(string videoId, IYouTubeClient client = null)
		{
			if (client == null) {
				client = YouTubeApi.GetYouTubeClient(YouTubeApi.GetDefaultYouTubeClientId());
				if (client == null) { return new YouTubeRawVideoInfoResult(null, 400); }
			}

			int errorCode = client.GetRawVideoInfo(videoId, out YouTubeRawVideoInfo rawVideoInfo, out _);
			return new YouTubeRawVideoInfoResult(rawVideoInfo, errorCode);
		}

		public static YouTubeRawVideoInfoResult Get(YouTubeVideoId videoId, IYouTubeClient client)
		{
			return Get(videoId.Id, client);
		}

		public static YouTubeRawVideoInfoResult Get(string videoId, FileDownloader downloader)
		{
			IYouTubeClient client = YouTubeApi.GetYouTubeClient(YouTubeApi.GetDefaultYouTubeClientId());
			if (client == null) { return new YouTubeRawVideoInfoResult(null, 400); }
			client.Downloader = downloader;
			return Get(videoId, client);
		}

		public static YouTubeRawVideoInfoResult Get(YouTubeVideoId videoId, FileDownloader downloader)
		{
			return Get(videoId.Id, downloader);
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

		public YouTubeSimplifiedVideoInfoResult Simplify(JObject customMicroformat,
			YouTubeStreamingData customStreamingData)
		{
			return SimplifyRawVideoInfo(VideoDetails, customMicroformat ?? Microformat, customStreamingData ?? StreamingData?.Data);
		}

		public YouTubeSimplifiedVideoInfoResult Simplify(JObject customMicroformat)
		{
			return Simplify(customMicroformat, null);
		}

		public YouTubeSimplifiedVideoInfoResult Simplify(YouTubeStreamingData customStreamingData)
		{
			return Simplify(null, customStreamingData);
		}

		public YouTubeSimplifiedVideoInfoResult Simplify()
		{
			return Simplify(null, null);
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

		/// <summary>
		/// Создаёт объект класса "YouTubeVideo" из переданных аргументов.
		/// </summary>
		/// <param name="customMicroformat">
		/// Если не 'null', эти данные будут использованы вместо текущих.
		/// </param>
		/// <param name="downloader">
		/// Объект скачивателя, который будет использован для получения дополнительных данных (если это необходимо).
		/// Если передать 'null', будет автоматически создан новый объект скачивателя с настройками по-умолчанию.
		/// </param>
		public YouTubeVideo ToVideo(JObject customMicroformat, FileDownloader downloader = null)
		{
			return MakeYouTubeVideo(this, customMicroformat, downloader);
		}

		/// <summary>
		/// Создаёт объект класса "YouTubeVideo" из переданных аргументов.
		/// </summary>
		/// <param name="downloader">
		/// Объект скачивателя, который будет использован для получения дополнительных данных (если это необходимо).
		/// Если передать 'null', будет автоматически создан новый объект скачивателя с настройками по-умолчанию.
		/// </param>
		public YouTubeVideo ToVideo(FileDownloader downloader = null)
		{
			return ToVideo(null, downloader);
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
