using System;
using System.Linq;
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

		/// <summary>
		/// Дата и время получения информации.
		/// </summary>
		public DateTime DateReceived { get; }

		public YouTubeMediaTrackUrlDecryptionData UrlDecryptionData { get; }

		public YouTubeVideoPlayabilityStatus PlayabilityStatus => ExtractPlayabilityStatus();
		public YouTubeStreamingDataResult StreamingData => ExtractStreamingData();
		public YouTubeVideoDetails VideoDetails => ExtractVideoDetails();
		public JObject Microformat => ExtractMicroformat();

		private JObject _parsedData = null;

		public YouTubeRawVideoInfo(string rawData, IYouTubeClient client,
			YouTubeMediaTrackUrlDecryptionData urlDecryptionData, DateTime dateReceived)
		{
			RawData = rawData;
			Client = client;
			UrlDecryptionData = urlDecryptionData;
			DateReceived = dateReceived;
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
			DateTime apiCallingDate,
			YouTubeMediaTrackUrlDecryptionData urlDecryptionData = null)
		{
			return new YouTubeRawVideoInfo(rawData, client, urlDecryptionData, apiCallingDate);
		}

		public static YouTubeRawVideoInfo MakeFromRaw(string rawData, IYouTubeClient client,
			YouTubeMediaTrackUrlDecryptionData urlDecryptionData = null)
		{
			return MakeFromRaw(rawData, client, DateTime.MaxValue, urlDecryptionData);
		}

		public static YouTubeRawVideoInfo MakeFromRaw(string rawData)
		{
			return MakeFromRaw(rawData, null);
		}

		public YouTubeSimplifiedVideoInfoResult Simplify(bool storeStreamingData = true)
		{
			YouTubeVideoPlayabilityStatus playabilityStatus = PlayabilityStatus;
			YouTubeVideoDetails videoDetails = VideoDetails;
			JObject jVideoDetails = VideoDetails?.Parse();
			JObject jMicroformatRenderer = Microformat?.Value<JObject>("playerMicroformatRenderer");

			JObject jSimplifiedVideoInfo = new JObject();
			if (playabilityStatus != null)
			{
				jSimplifiedVideoInfo["playability_status"] = playabilityStatus.ToJson();
			}

			string videoId;
			if (jVideoDetails != null)
			{
				videoId = jVideoDetails.Value<string>("videoId");
				jSimplifiedVideoInfo["id"] = videoId;
				jSimplifiedVideoInfo["title"] = jVideoDetails.Value<string>("title");
				JObject jOwnerChannel = new JObject()
				{
					["title"] = jVideoDetails.Value<string>("author"),
					["id"] = jVideoDetails.Value<string>("channelId")
				};
				jSimplifiedVideoInfo["owner_channel"] = jOwnerChannel;
				jSimplifiedVideoInfo["url"] = GetYouTubeVideoUrl(videoId);
				if (int.TryParse(jVideoDetails.Value<string>("lengthSeconds"), out int lengthSeconds))
				{
					jSimplifiedVideoInfo["length_seconds"] = lengthSeconds;
					jSimplifiedVideoInfo["length"] = FormatVideoDuration(TimeSpan.FromSeconds(lengthSeconds));
				}
				if (!long.TryParse(jVideoDetails.Value<string>("viewCount"), out long viewCount))
				{
					viewCount = -1L;
				}
				jSimplifiedVideoInfo["view_count"] = viewCount;
				jSimplifiedVideoInfo["is_private"] = jVideoDetails.Value<bool>("isPrivate");
				jSimplifiedVideoInfo["is_live_content"] = jVideoDetails.Value<bool>("isLiveContent");
				jSimplifiedVideoInfo["is_crawlable"] = jVideoDetails.Value<bool>("isCrawlable");
				jSimplifiedVideoInfo["description"] = jVideoDetails.Value<string>("shortDescription");
			}
			else
			{
				videoId = null;
			}

			if (jMicroformatRenderer != null)
			{
				if (string.IsNullOrEmpty(jSimplifiedVideoInfo.Value<string>("description")))
				{
					JObject jDescription = jMicroformatRenderer.Value<JObject>("description");
					if (jDescription != null)
					{
						jSimplifiedVideoInfo["description"] = jDescription.Value<string>("simpleText");
					}
				}
				jSimplifiedVideoInfo["category"] = jMicroformatRenderer.Value<string>("category");
				jSimplifiedVideoInfo["is_short_format"] = jMicroformatRenderer.Value<bool>("isShortsEligible");
				jSimplifiedVideoInfo["like_count"] = jMicroformatRenderer.Value<long>("likeCount");
				jSimplifiedVideoInfo["is_family_safe"] = jMicroformatRenderer.Value<bool>("isFamilySafe");
				jSimplifiedVideoInfo["is_unlisted"] = jMicroformatRenderer.Value<bool>("isUnlisted");

				{
					string date = jMicroformatRenderer.Value<string>("publishDate");
					jSimplifiedVideoInfo["date_publish"] = DateTimeStringToUtcString(date, out DateTime dateTime);
					jSimplifiedVideoInfo["date_publish_unix"] = dateTime.ToUnixTimeMilliseconds();
				}
				{
					string date = jMicroformatRenderer.Value<string>("uploadDate");
					jSimplifiedVideoInfo["date_upload"] = DateTimeStringToUtcString(date, out DateTime dateTime);
					jSimplifiedVideoInfo["date_upload_unix"] = dateTime.ToUnixTimeMilliseconds();
				}

				JObject jLiveBroadcastDetails = jMicroformatRenderer.Value<JObject>("liveBroadcastDetails");
				if (jLiveBroadcastDetails != null)
				{
					bool isLiveNow = jLiveBroadcastDetails.Value<bool>("isLiveNow");
					JObject jLive = new JObject()
					{
						["is_live_now"] = isLiveNow
					};

					{
						string date = jLiveBroadcastDetails.Value<string>("startTimestamp");
						jLive["start_timestamp"] = DateTimeStringToUtcString(date, out DateTime dateTime);
						jLive["start_timestamp_unix"] = dateTime.ToUnixTimeMilliseconds();
					}
					if (!isLiveNow)
					{
						string date = jLiveBroadcastDetails.Value<string>("endTimestamp");
						if (!string.IsNullOrEmpty(date) && !string.IsNullOrWhiteSpace(date))
						{
							jLive["end_timestamp"] = DateTimeStringToUtcString(date, out DateTime dateTime);
							jLive["end_timestamp_unix"] = dateTime.ToUnixTimeMilliseconds();
						}
					}

					jSimplifiedVideoInfo["live_stream_info"] = jLive;
				}
			}

			var videoThumbnails = GetThumbnails(videoDetails, Microformat, videoId).ToList();
			if (videoThumbnails.Count > 0)
			{
				jSimplifiedVideoInfo["thumbnails"] = ThumbnailsToJson(videoThumbnails);
			}

			if (storeStreamingData)
			{
				YouTubeStreamingData streamingData = StreamingData.Data;
				if (streamingData != null)
				{
					JObject jClient = YouTubeSimplifiedVideoInfo.FormatApiClientJson(streamingData);
					if (jClient != null)
					{
						JArray jaDownloadUrls = new JArray() { jClient };
						jSimplifiedVideoInfo["download_urls"] = jaDownloadUrls;
					}
				}
			}

			YouTubeSimplifiedVideoInfo simplifiedVideoInfo = new YouTubeSimplifiedVideoInfo(
				jSimplifiedVideoInfo, jVideoDetails != null, jMicroformatRenderer != null, this);
			return new YouTubeSimplifiedVideoInfoResult(simplifiedVideoInfo, 200);
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
						jStreamingData.ToString(), Client, UrlDecryptionData, DateReceived);
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
		/// Создаёт объект класса "YouTubeVideo".
		/// </summary>
		public YouTubeVideo ToVideo()
		{
			YouTubeSimplifiedVideoInfoResult simplified = Simplify();
			return simplified.ErrorCode == 200 ? simplified.SimplifiedVideoInfo.ToVideo() : null;
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
