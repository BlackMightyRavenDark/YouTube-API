using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public class YouTubeVideo
	{
		public string Title { get; }
		public string Id { get; }
		public string Url { get; }
		public DateTime DateUploaded { get; }
		public DateTime DatePublished { get; }
		public TimeSpan Length { get; }
		public string OwnerChannelTitle { get; }
		public string OwnerChannelId { get; }
		public string Description { get; }
		public long ViewCount { get; }
		public string Category { get; }

		/// <summary>
		/// Является ли данное видео коротким (short aka "reel").
		/// </summary>
		public bool IsShortFormat { get; }

		/// <summary>
		/// Присутствуют ли аудио-дорожки с переводами на другие языки.
		/// </summary>
		public bool IsMultilingual { get; private set; }

		public bool IsPrivate { get; }
		public bool IsUnlisted { get; }
		public bool IsFamilySafe { get; }
		public bool IsLiveContent { get; }

		/// <summary>
		/// Является ли данное видео прямой трансляцией (стримом) и находится ли она сейчас в эфире.
		/// Внимание! Это значение может быть всегда положительным, даже если стрим был завершён какое-то время назад! Это глюк ютуба.
		/// Лучше использовать метод 'UpdateIsLiveNow()'.
		/// </summary>
		public bool IsLiveNow => GetIsLiveNow();

		public bool IsDashed { get; private set; }
		public string DashManifestUrl { get; private set; }
		public string HlsManifestUrl { get; private set; }
		public YouTubeVideoDetails Details { get; private set; }
		public List<YouTubeVideoThumbnail> Thumbnails { get; }
		public Dictionary<string, YouTubeMediaFormatList> MediaTracks { get; private set; }
		public YouTubeRawVideoInfo RawInfo { get; private set; }
		public YouTubeSimplifiedVideoInfo SimplifiedInfo { get; }
		public YouTubeVideoPlayabilityStatus Status { get; }
		public bool IsInfoAvailable => GetIsInfoAvailable();
		public bool IsPlayable => GetIsPlayable();

		public YouTubeVideo(
			string title,
			string id,
			TimeSpan length,
			DateTime dateUploaded,
			DateTime datePublished,
			string ownerChannelTitle,
			string ownerChannelId,
			string description,
			long viewCount,
			string category,
			bool isShortFormat,
			bool isPrivate,
			bool isUnlisted,
			bool isFamilySafe,
			bool isLiveContent,
			YouTubeVideoDetails videoDetails,
			List<YouTubeVideoThumbnail> thumbnails,
			YouTubeRawVideoInfo rawInfo,
			YouTubeSimplifiedVideoInfo simplifiedInfo,
			YouTubeVideoPlayabilityStatus status)
		{
			Title = title;
			Id = id;
			Url = !string.IsNullOrEmpty(id) && !string.IsNullOrWhiteSpace(id) ?
				Utils.GetYouTubeVideoUrl(id) : null;
			Length = length;
			DateUploaded = dateUploaded;
			DatePublished = datePublished;
			OwnerChannelTitle = ownerChannelTitle;
			OwnerChannelId = ownerChannelId;
			Description = description;
			ViewCount = viewCount;
			Category = category;
			IsShortFormat = isShortFormat;
			IsPrivate = isPrivate;
			IsUnlisted = isUnlisted;
			IsFamilySafe = isFamilySafe;
			IsLiveContent = isLiveContent;
			Details = videoDetails;
			Thumbnails = thumbnails;
			MediaTracks = new Dictionary<string, YouTubeMediaFormatList>();
			RawInfo = rawInfo;
			SimplifiedInfo = simplifiedInfo;
			Status = status;

			UpdateStates();
		}

		public static YouTubeVideo CreateEmpty(YouTubeVideoPlayabilityStatus status)
		{
			return new YouTubeVideo(null, null, TimeSpan.Zero, DateTime.MaxValue, DateTime.MaxValue,
				null, null, null, 0L, null, false, false, false, false, false, null, null, null, null, status);
		}

		public static YouTubeVideo CreateEmpty()
		{
			return CreateEmpty(null);
		}

		/// <summary>
		/// Получить информацию о видео на YouTube.
		/// </summary>
		/// <param name="videoId">
		/// ID запрашиваемого видео на YouTube.
		/// </param>
		/// <param name="client">
		/// Клиент YouTube для получения информации о видео.
		/// Если передать 'null', будут использованы методы по-умолчанию.
		/// </param>
		/// <param name="downloader">
		/// Объект скачивателя, который будет использован для получения данных.
		/// Если передать 'null', будет автоматически создан новый объект скачивателя с настройками по-умолчанию.
		/// </param>
		public static YouTubeVideo GetById(YouTubeVideoId videoId, IYouTubeClient client, FileDownloader downloader = null)
		{
			if (client == null)
			{
				client = YouTubeApi.GetYouTubeClient(YouTubeApi.GetDefaultYouTubeClientId());
				if (client == null) { return CreateEmpty(new YouTubeVideoPlayabilityStatus(400)); }
			}

			client.Downloader = downloader;
			YouTubeRawVideoInfoResult rawVideoInfoResult = client.GetRawVideoInfo(videoId, out _);
			if (rawVideoInfoResult.ErrorCode == 200)
			{
				return rawVideoInfoResult.RawVideoInfo.ToVideo(downloader);
			}

			return CreateEmpty(new YouTubeVideoPlayabilityStatus(404));
		}

		/// <summary>
		/// Получить информацию о видео на YouTube, используя методы по-умолчанию.
		/// </summary>
		/// <param name="videoId">
		/// ID запрашиваемого видео на YouTube.
		/// <param name="downloader">
		/// Объект скачивателя, который будет использован для получения данных.
		/// Если передать 'null', будет автоматически создан новый объект скачивателя с настройками по-умолчанию.
		/// </param>
		public static YouTubeVideo GetById(YouTubeVideoId videoId, FileDownloader downloader = null)
		{
			return GetById(videoId, null, downloader);
		}

		/// <summary>
		/// Получить информацию о видео на YouTube.
		/// </summary>
		/// <param name="videoId">
		/// ID запрашиваемого видео на YouTube.
		/// </param>
		/// <param name="client">
		/// Клиент YouTube для получения информации о видео.
		/// Если передать 'null', будут использованы методы по-умолчанию.
		/// </param>
		/// <param name="downloader">
		/// Объект скачивателя, который будет использован для получения данных.
		/// Если передать 'null', будет автоматически создан новый объект скачивателя с настройками по-умолчанию.
		/// </param>
		public static YouTubeVideo GetById(string videoId, IYouTubeClient client, FileDownloader downloader = null)
		{
			YouTubeVideoId youTubeVideoId = new YouTubeVideoId(videoId);
			return GetById(youTubeVideoId, client, downloader);
		}

		/// <summary>
		/// Получить информацию о видео на YouTube, используя методы по-умолчанию.
		/// </summary>
		/// <param name="videoId">
		/// ID запрашиваемого видео на YouTube.
		/// <param name="downloader">
		/// Объект скачивателя, который будет использован для получения данных.
		/// Если передать 'null', будет автоматически создан новый объект скачивателя с настройками по-умолчанию.
		/// </param>
		public static YouTubeVideo GetById(string videoId, FileDownloader downloader = null)
		{
			return GetById(videoId, null, downloader);
		}

		public static YouTubeVideo GetByWebPage(YouTubeVideoWebPage videoWebPage)
		{
			return Utils.GetVideoFromWebPage(videoWebPage);
		}

		public static YouTubeVideo GetByWebPage(string videoWebPageCode)
		{
			YouTubeVideoWebPageResult videoWebPageResult = YouTubeVideoWebPage.FromCode(videoWebPageCode);
			return videoWebPageResult.ErrorCode == 200 ? Utils.GetVideoFromWebPage(videoWebPageResult.VideoWebPage) : null;
		}

		public string GetUrl(int seekToSecond = 0)
		{
			return Utils.GetYouTubeVideoUrl(Id, seekToSecond);
		}

		public string GetUrl(TimeSpan seekTo)
		{
			return Utils.GetYouTubeVideoUrl(Id, seekTo);
		}

		/// <summary>
		/// Обновить список медиа-форматов и ссылок для скачивания.
		/// Внимание! Текущий список и ссылки будут утеряны!
		/// </summary>
		/// <param name="rawVideoInfo">
		/// Объект, из которого будут взяты новые данные.
		/// </param>
		public void UpdateMediaFormats(YouTubeRawVideoInfo rawVideoInfo)
		{
			YouTubeStreamingDataResult streamingDataResult = rawVideoInfo.StreamingData;
			if (streamingDataResult.ErrorCode == 200)
			{
				YouTubeMediaFormatList list = rawVideoInfo.StreamingData?.Data.Parse();
				if (list != null)
				{
					string clientName = list.Client?.DisplayName ?? "unknown";
					if (list.Tracks.Count > 0)
					{
						MediaTracks[clientName] = list;
					}
					else if (MediaTracks.ContainsKey(clientName))
					{
						MediaTracks.Remove(clientName);
					}

					IsMultilingual = IsTranslatedAudioTrackPresent();
				}
			}
		}

		/// <summary>
		/// Скачать заново и обновить список медиа-форматов и ссылок для скачивания.
		/// Внимание! Текущий список и ссылки будут утеряны!
		/// Текущая сырая информация (raw info) о видео будет обновлена в случае успешного вызова,
		/// либо утеряна в случае неудачного вызова!
		/// </summary>
		/// <param name="client">
		/// Клиент YouTube для получения информации о видео.
		/// </param>
		/// <returns>Код ошибки HTTP.</returns>
		public int UpdateMediaFormats(IYouTubeClient client)
		{
			if (MediaTracks.ContainsKey(client.DisplayName))
			{
				MediaTracks.Remove(client.DisplayName);
			}
			YouTubeRawVideoInfoResult rawVideoInfoResult = YouTubeRawVideoInfo.Get(Id, client);
			RawInfo = rawVideoInfoResult.RawVideoInfo;
			UpdateStates();
			if (rawVideoInfoResult.ErrorCode == 200)
			{
				UpdateMediaFormats(rawVideoInfoResult.RawVideoInfo);
				return MediaTracks.ContainsKey(client.DisplayName) ? 200 : 204;
			}
			return rawVideoInfoResult.ErrorCode;
		}

		/// <summary>
		/// Скачать заново и обновить список медиа-форматов и ссылок для скачивания, используя методы по-умолчанию.
		/// Внимание! Текущий список и ссылки будут утеряны!
		/// </summary>
		/// <returns>Код ошибки HTTP.</returns>
		public int UpdateMediaFormats()
		{
			IYouTubeClient client = YouTubeApi.GetYouTubeClient(YouTubeApi.GetDefaultYouTubeClientId());
			return client != null ? UpdateMediaFormats(client) : 400;
		}

		public void ClearMediaFormatList()
		{
			MediaTracks?.Clear();
			IsMultilingual = false;
		}

		private bool GetIsInfoAvailable()
		{
			return RawInfo?.VideoDetails != null &&
				(SimplifiedInfo.IsVideoInfoAvailable || SimplifiedInfo.IsMicroformatInfoAvailable);
		}

		private bool GetIsPlayable()
		{
			return Status != null && Status.IsPlayable;
		}

		public bool UpdateVideoDetails()
		{
			Details = Utils.GetVideoDetails(Id);
			return Details != null;
		}

		private bool GetIsLiveNow()
		{
			JObject jDetails = Details?.Parse();
			if (jDetails != null)
			{
				JToken jt = jDetails.Value<JToken>("isLive");
				if (jt != null) { return jt.Value<bool>(); }
			}

			return !string.IsNullOrEmpty(HlsManifestUrl);
		}

		public bool UpdateIsLiveNow(IYouTubeClient client = null)
		{
			if (client == null)
			{
				client = YouTubeApi.GetYouTubeClient(YouTubeApi.GetDefaultYouTubeClientId());
			}

			if (client != null)
			{
				YouTubeVideoDetails details = Utils.GetVideoDetails(Id, client);
				JObject jDetails = details?.Parse();
				if (jDetails != null)
				{
					JToken jt = jDetails.Value<JToken>("isLive");
					return jt != null ? jt.Value<bool>() :
						jDetails.Value<JToken>("viewCount") != null;
				}
			}

			return false;
		}

		public YouTubeSimplifiedVideoInfo GetSimplifiedInfo()
		{
			YouTubeSimplifiedVideoInfoResult infoResult = RawInfo.Simplify();
			return infoResult.ErrorCode == 200 ? infoResult.SimplifiedVideoInfo : null;
		}

		private void UpdateStates()
		{
			YouTubeStreamingData streamingData = RawInfo?.StreamingData.Data;
			if (streamingData != null)
			{
				DashManifestUrl = streamingData.GetDashManifestUrl();
				IsDashed = !string.IsNullOrEmpty(DashManifestUrl) && !string.IsNullOrWhiteSpace(DashManifestUrl);
				HlsManifestUrl = streamingData.GetHlsManifestUrl();
			}
			else
			{
				DashManifestUrl = null;
				IsDashed = false;
				HlsManifestUrl = null;
			}
		}

		public IEnumerable<YouTubeMediaTrack> GetAllMediaTracks()
		{
			foreach (var item in MediaTracks)
			{
				foreach (YouTubeMediaTrack track in item.Value.Tracks)
				{
					yield return track;
				}
			}
		}

		private bool IsTranslatedAudioTrackPresent()
		{
			var tracks = GetAllMediaTracks();
			return tracks.Any(track =>
			{
				YouTubeAudioTrackLanguage language = track is YouTubeMediaTrackAudio ?
					(track as YouTubeMediaTrackAudio).Language : null;
				return language != null && !language.IsDefault;
			});
		}
	}
}
