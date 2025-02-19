using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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
		/// It's the short (aka "reel") video or not.
		/// </summary>
		public bool IsShortFormat { get; }

		public bool IsPrivate { get; }
		public bool IsUnlisted { get; }
		public bool IsFamilySafe { get; }
		public bool IsLiveContent { get; }

		/// <summary>
		/// Warning! This value can be always TRUE for some videos!
		/// E.G. when the broadcast is finished, but not yet fully processed by YouTube.
		/// This is a YouTube API bug.
		/// It's better to use 'UpdateIsLiveNow()' method.
		/// </summary>
		public bool IsLiveNow => GetIsLiveNow();

		public bool IsDashed { get; }
		public string DashManifestUrl { get; }
		public string HlsManifestUrl { get; }
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

			if (MediaTracks != null)
			{
				foreach (var item in MediaTracks)
				{
					foreach (YouTubeMediaTrack track in item.Value.Tracks)
					{
						if (!IsDashed)
						{
							IsDashed = track.IsDashManifestPresent;
							DashManifestUrl = track.DashManifestUrl;
						}
						if (IsLiveNow && track.GetType() == typeof(YouTubeMediaTrackHlsStream))
						{
							HlsManifestUrl = (track as YouTubeMediaTrackHlsStream).HlsManifestUrl;
						}
						if (IsDashed && !string.IsNullOrEmpty(HlsManifestUrl))
						{
							return;
						}
					}
				}
			}
			else
			{
				YouTubeStreamingData streamingData = rawInfo?.StreamingData.Data;
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

		public static YouTubeVideo GetById(YouTubeVideoId videoId, IYouTubeClient client)
		{
			bool automaticClientSelection = client == null;
			if (automaticClientSelection) { client = YouTubeApi.GetYouTubeClient("web_page"); }
			if (client == null) { return null; }
			YouTubeRawVideoInfoResult rawVideoInfoResult = client.GetRawVideoInfo(videoId, out _);
			if (rawVideoInfoResult.ErrorCode == 200)
			{
				YouTubeVideo video = rawVideoInfoResult.RawVideoInfo.ToVideo();
				if (video != null)
				{
					if (YouTubeApi.getMediaTracksInfoImmediately && !(client is YouTubeClientIos))
					{
						IYouTubeClient streamingDataClient = automaticClientSelection ?
							YouTubeApi.GetYouTubeClient("ios") : client;
						if (streamingDataClient != null)
						{
							video.UpdateMediaFormats(streamingDataClient);
						}
					}

					return video;
				}
			}

			return CreateEmpty(new YouTubeVideoPlayabilityStatus(400));
		}

		public static YouTubeVideo GetById(YouTubeVideoId videoId)
		{
			return GetById(videoId, null);
		}

		public static YouTubeVideo GetById(string videoId, IYouTubeClient client)
		{
			YouTubeVideoId youTubeVideoId = new YouTubeVideoId(videoId);
			return GetById(youTubeVideoId, client);
		}

		public static YouTubeVideo GetById(string videoId)
		{
			return GetById(videoId, null);
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
		/// Reparse the downloadable formats info.
		/// Warming!!! You may lost some data in the current media track list!
		/// </summary>
		public void UpdateMediaFormats(YouTubeRawVideoInfo rawVideoInfo)
		{
			YouTubeStreamingDataResult streamingDataResult = rawVideoInfo.StreamingData;
			if (streamingDataResult.ErrorCode == 200)
			{
				YouTubeMediaFormatList list = rawVideoInfo.StreamingData.Data.Parse();
				string clientName = list.Client.DisplayName;
				if (list.Tracks.Count > 0)
				{
					MediaTracks[clientName] = list;
				}
				else if (MediaTracks.ContainsKey(clientName))
				{
					MediaTracks.Remove(clientName);
				}
			}
		}

		/// <summary>
		/// Redownload and reparse the downloadable formats info.
		/// Warming!!! You will lost the data previously obtained by the specified client!
		/// </summary>
		/// <returns>HTTP error code.</returns>
		public int UpdateMediaFormats(IYouTubeClient client)
		{
			if (MediaTracks.ContainsKey(client.DisplayName))
			{
				MediaTracks.Remove(client.DisplayName);
			}
			YouTubeRawVideoInfoResult rawVideoInfoResult = YouTubeRawVideoInfo.Get(Id, client);
			if (rawVideoInfoResult != null)
			{
				RawInfo = rawVideoInfoResult.RawVideoInfo;
				if (rawVideoInfoResult.ErrorCode == 200)
				{
					UpdateMediaFormats(rawVideoInfoResult.RawVideoInfo);
					return MediaTracks.ContainsKey(client.DisplayName) ? 200 : 204;
				}
				return rawVideoInfoResult.ErrorCode;
			}
			return 404;
		}

		/// <summary>
		/// Redownload and reparse the downloadable formats info using the default YouTube client.
		/// Warming!!! You may lost some data in the current media track list!
		/// </summary>
		/// <returns>HTTP error code.</returns>
		public int UpdateMediaFormats()
		{
			IYouTubeClient client = YouTubeApi.GetYouTubeClient(YouTubeApi.GetDefaultYouTubeClientId());
			return client != null ? UpdateMediaFormats(client) : 400;
		}

		public void ClearMediaFormatList()
		{
			MediaTracks?.Clear();
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
	}
}
