using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	public class YouTubeSimplifiedVideoInfo
	{
		/// <summary>
		/// Упрощённая информация о видео.
		/// </summary>
		public JObject SimplifiedVideoInfo { get; }

		public bool IsVideoInfoAvailable { get; }
		public bool IsMicroformatInfoAvailable { get; }

		/// <summary>
		/// Сырые данные, на основе которых был создан этот объект.
		/// Если значение равно 'null', это означает, что упрощённые данные
		/// были загружены из заранее сохранённого файла, без использования сырой информации.
		/// </summary>
		public YouTubeRawVideoInfo RawVideoInfo { get; }

		public YouTubeSimplifiedVideoInfo(JObject simplifiedVideoInfo,
			bool isVideoInfoAvailable, bool isMicroformatInfoAvailable,
			YouTubeRawVideoInfo rawVideoInfo)
		{
			SimplifiedVideoInfo = simplifiedVideoInfo;
			IsVideoInfoAvailable = isVideoInfoAvailable;
			IsMicroformatInfoAvailable = isMicroformatInfoAvailable;
			RawVideoInfo = rawVideoInfo;
		}

		public static YouTubeSimplifiedVideoInfo MakeFromRaw(string rawJsonData,
			YouTubeRawVideoInfo rawVideoInfo, out string errorMessage)
		{
			// Искренне надеемся на то, что все необходимые данные в переданном JSON'е находятся на своих местах!
			JObject json = Utils.TryParseJson(rawJsonData, out errorMessage);
			return json != null ? new YouTubeSimplifiedVideoInfo(json, true, true, rawVideoInfo) : null;
		}

		public static YouTubeSimplifiedVideoInfo MakeFromRaw(string rawJsonData, YouTubeRawVideoInfo rawVideoInfo = null)
		{
			return MakeFromRaw(rawJsonData, rawVideoInfo, out _);
		}

		/// <summary>
		/// Создаёт объект класса "YouTubeVideo".
		/// </summary>
		public YouTubeVideo ToVideo()
		{
			string videoTitle = null;
			string videoId = null;
			TimeSpan videoDuration = TimeSpan.Zero;
			string ownerChannelTitle = null;
			string ownerChannelId = null;
			long viewCount = 0L;
			bool isPrivate = false;
			bool isLiveContent = false;
			string shortDescription = null;

			string description = null;
			bool isShort = false;
			bool isFamilySafe = true;
			bool isUnlisted = false;
			string category = null;
			DateTime datePublished = DateTime.MaxValue;
			DateTime dateUploaded = DateTime.MaxValue;

			List<YouTubeVideoThumbnail> videoThumbnails = null;

			if (IsVideoInfoAvailable)
			{
				videoTitle = SimplifiedVideoInfo.Value<string>("title");
				videoId = SimplifiedVideoInfo.Value<string>("id");
				if (int.TryParse(SimplifiedVideoInfo.Value<string>("length_seconds"), out int lengthSeconds))
				{
					videoDuration = TimeSpan.FromSeconds(lengthSeconds);
				}
				JObject jOwnerChannel = SimplifiedVideoInfo.Value<JObject>("owner_channel");
				if (jOwnerChannel != null)
				{
					ownerChannelTitle = jOwnerChannel.Value<string>("title");
					ownerChannelId = jOwnerChannel.Value<string>("id");
				}
				if (!long.TryParse(SimplifiedVideoInfo.Value<string>("view_count"), out viewCount))
				{
					viewCount = 0L;
				}
				isPrivate = SimplifiedVideoInfo.Value<bool>("is_private");
				isLiveContent = SimplifiedVideoInfo.Value<bool>("is_live_content");
				shortDescription = SimplifiedVideoInfo.Value<string>("short_description");
			}
			if (IsMicroformatInfoAvailable)
			{
				description = SimplifiedVideoInfo.Value<string>("description");
				isShort = SimplifiedVideoInfo.Value<bool>("is_short_format");
				isFamilySafe = SimplifiedVideoInfo.Value<bool>("is_family_safe");
				isUnlisted = SimplifiedVideoInfo.Value<bool>("is_unlisted");
				category = SimplifiedVideoInfo.Value<string>("category");
				Utils.ExtractDatesFromMicroformat(SimplifiedVideoInfo, out dateUploaded, out datePublished);
			}

			JArray jaThumbnails = SimplifiedVideoInfo.Value<JArray>("thumbnails");
			if (jaThumbnails != null && jaThumbnails.Count > 0)
			{
				videoThumbnails = new List<YouTubeVideoThumbnail>();
				foreach (JObject jThumbnail in jaThumbnails.Cast<JObject>())
				{
					ushort width = jThumbnail.Value<ushort>("width");
					ushort height = jThumbnail.Value<ushort>("height");
					string fileName = jThumbnail.Value<string>("file_name");
					string url = jThumbnail.Value<string>("url");
					if (string.IsNullOrEmpty(fileName) || string.IsNullOrWhiteSpace(fileName))
					{
						fileName = Utils.ExtractFileNameFromThumbnailUrl(url);
					}
					videoThumbnails.Add(new YouTubeVideoThumbnail(width, height, fileName, url));
				}
			}

			YouTubeVideoPlayabilityStatus videoStatus = YouTubeVideoPlayabilityStatus.FromJson(SimplifiedVideoInfo.Value<JObject>("playability_status"));
			string descr = !string.IsNullOrEmpty(description) ? description : shortDescription;

			YouTubeVideo youTubeVideo = new YouTubeVideo(
				videoTitle, videoId, videoDuration, dateUploaded, datePublished, ownerChannelTitle,
				ownerChannelId, descr, viewCount, category, isShort, isPrivate, isUnlisted,
				isFamilySafe, isLiveContent, videoThumbnails, this, videoStatus);

			JArray jaDownloadUrls = SimplifiedVideoInfo.Value<JArray>("download_urls");
			if (jaDownloadUrls != null && jaDownloadUrls.Count > 0)
			{
				foreach (JObject jItem in jaDownloadUrls.Cast<JObject>())
				{
					JObject jStreamingData = jItem.Value<JObject>("streaming_data");
					if (jStreamingData != null)
					{
						string clientName = jItem.Value<string>("client_id");
						if (string.IsNullOrEmpty(clientName) || string.IsNullOrWhiteSpace(clientName))
						{
							clientName = "unknown";
						}

						DateTime apiCallingDate = ExtractApiCallingDate(jItem);
						IYouTubeClient client = new YouTubeClientFake(clientName);
						YouTubeStreamingData streamingData = YouTubeStreamingData.MakeFromRaw(jStreamingData.ToString(), client, apiCallingDate);
						YouTubeMediaFormatList mediaFormats = streamingData.Parse();
						if (mediaFormats != null)
						{
							youTubeVideo.MediaTracks[clientName] = mediaFormats;
						}
					}
				}

				youTubeVideo.UpdateIsMultilingual();
			}

			return youTubeVideo;
		}

		internal static JObject FormatApiClientJson(string streamingDataRaw, DateTime dateReceived, string apiClientId)
		{
			JObject jStreamingData = Utils.TryParseJson(streamingDataRaw);
			if (jStreamingData == null) { return null; }

			string actualClientId = string.IsNullOrEmpty(apiClientId) || string.IsNullOrWhiteSpace(apiClientId) ? "unknown" : apiClientId;
			JObject jClient = new JObject()
			{
				["client_id"] = actualClientId,
				["streaming_data"] = jStreamingData
			};

			if (dateReceived < DateTime.MaxValue)
			{
				jClient["api_calling_date"] = dateReceived;
				jClient["api_calling_date_unix_ticks"] = dateReceived.ToUnixTimeTicks();
			}

			return jClient;
		}

		internal static JObject FormatApiClientJson(YouTubeStreamingData streamingData)
		{
			return FormatApiClientJson(streamingData.RawData, streamingData.DateReceived, streamingData.Client?.DisplayName);
		}

		private static DateTime ExtractApiCallingDate(JObject json)
		{
			long unixTicks = json.Value<long>("api_calling_date_unix_ticks");
			if (unixTicks > 0L)
			{
				return Utils.UnixTimeTicksToDateTime(unixTicks);
			}

			string date = json.Value<string>("api_calling_date");
			if (DateTime.TryParseExact(date, "yyyy-MM-ddTHH:mm:ssZ",
				null, DateTimeStyles.AdjustToUniversal, out DateTime dateTime))
			{
				return dateTime;
			}

			return DateTime.MaxValue;
		}
	}
}
