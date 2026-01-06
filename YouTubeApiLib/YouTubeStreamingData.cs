using System;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;
using static YouTubeApiLib.Utils;

namespace YouTubeApiLib
{
	public class YouTubeStreamingData
	{
		public string RawData { get; private set; }

		/// <summary>
		/// Клиент YouTube, которым были получены данные.
		/// </summary>
		public IYouTubeClient Client { get; }

		/// <summary>
		/// Дата и время получения данных.
		/// </summary>
		public DateTime DateReceived { get; }

		public YouTubeMediaTrackUrlDecryptionData UrlDecryptionData { get; }

		private JObject _parsedData = null;

		public YouTubeStreamingData(string rawData, IYouTubeClient client,
			YouTubeMediaTrackUrlDecryptionData urlDecryptionData, DateTime dateReceived)
		{
			RawData = rawData;
			Client = client;
			UrlDecryptionData = urlDecryptionData;
			DateReceived = dateReceived;
		}

		public static YouTubeStreamingDataResult Get(YouTubeVideoId videoId, IYouTubeClient client)
		{
			return Get(videoId.Id, client);
		}

		public static YouTubeStreamingDataResult Get(string videoId, IYouTubeClient client)
		{
			int errorCode = client.GetRawVideoInfo(videoId, out YouTubeRawVideoInfo rawVideoInfo, out _);
			return errorCode == 200 ? rawVideoInfo.StreamingData :
				new YouTubeStreamingDataResult(null, errorCode);
		}

		public static YouTubeStreamingDataResult Get(YouTubeVideoId videoId, FileDownloader downloader = null)
		{
			return Get(videoId.Id, downloader);
		}

		public static YouTubeStreamingDataResult Get(string videoId, FileDownloader downloader = null)
		{
			IYouTubeClient client = YouTubeApi.GetYouTubeClient(YouTubeApi.GetDefaultYouTubeClientId());
			if (client == null) { return new YouTubeStreamingDataResult(null, 400); }
			client.Downloader = downloader;
			return Get(videoId, client);
		}

		public static YouTubeStreamingData MakeFromRaw(string rawData, IYouTubeClient client,
			YouTubeMediaTrackUrlDecryptionData urlDecryptionData = null)
		{
			return new YouTubeStreamingData(rawData, client, urlDecryptionData, DateTime.UtcNow);
		}

		public static YouTubeStreamingData MakeFromRaw(string rawData)
		{
			return MakeFromRaw(rawData, null);
		}

		public YouTubeMediaFormatList Parse(FileDownloader downloader = null)
		{
			return YouTubeMediaFormatsParser.Parse(this, downloader);
		}

		public JArray GetFormats()
		{
			return PreParse() ? _parsedData.Value<JArray>("formats") : null;
		}

		public JArray GetAdaptiveFormats()
		{
			return PreParse() ? _parsedData.Value<JArray>("adaptiveFormats") : null;
		}

		public string GetDashManifestUrl()
		{
			return PreParse() ? _parsedData.Value<string>("dashManifestUrl") : null;
		}

		public string GetHlsManifestUrl()
		{
			return PreParse() ? _parsedData.Value<string>("hlsManifestUrl") : null;
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
