using System.Collections.Concurrent;
using System.Collections.Generic;

namespace YouTubeApiLib
{
	public sealed class YouTubeApi
	{
		private static string _defaultYouTubeClientId = "web_page";
		private static ConcurrentDictionary<string, IYouTubeClient> _clients = new ConcurrentDictionary<string, IYouTubeClient>()
		{
			["web_page"] = new YouTubeClientWebPage()
		};

		public YouTubeVideo GetVideo(YouTubeVideoId youTubeVideoId, IYouTubeClient client)
		{
			return YouTubeVideo.GetById(youTubeVideoId, client);
		}

		public YouTubeVideo GetVideo(YouTubeVideoId youTubeVideoId)
		{
			IYouTubeClient client = GetYouTubeClient(GetDefaultYouTubeClientId());
			return client != null ? GetVideo(youTubeVideoId, client) : null;
		}

		public YouTubeVideo GetVideo(YouTubeVideoWebPage videoWebPage)
		{
			return YouTubeVideo.GetByWebPage(videoWebPage);
		}

		public YouTubeVideo GetVideo(string webPageCode)
		{
			return YouTubeVideo.GetByWebPage(webPageCode);
		}

		public YouTubeSimplifiedVideoInfoResult GetSimplifiedVideoInfo(string videoId, IYouTubeClient client)
		{
			return Utils.GetSimplifiedVideoInfo(videoId, client);
		}

		public YouTubeVideoListResult GetChannelVideoList(YouTubeChannel channel)
		{
			return YouTubeApiV1.GetChannelVideoList(channel.Id, null);
		}

		public YouTubeVideoIdPageResult GetVideoIdPage(
			YouTubeChannel youTubeChannel, YouTubeChannelTabPage channelTabPage, string continuationToken)
		{
			return youTubeChannel.GetVideoIdPage(channelTabPage, continuationToken);
		}

		public YouTubeVideoIdPageResult GetVideoIdPage(YouTubeChannel youTubeChannel, YouTubeChannelTabPage channelTabPage)
		{
			return GetVideoIdPage(youTubeChannel, channelTabPage, null);
		}

		public YouTubeVideoIdPageResult GetVideoIdPage(YouTubeChannel youTubeChannel, string continuationToken)
		{
			return GetVideoIdPage(youTubeChannel, null, continuationToken);
		}

		public YouTubeVideoIdPageResult GetVideoIdPage(YouTubeChannel youTubeChannel)
		{
			return GetVideoIdPage(youTubeChannel, string.Empty);
		}

		public YouTubeVideoPageResult GetVideoPage(YouTubeChannel youTubeChannel, string continuationToken)
		{
			return GetVideoPage(youTubeChannel, null, continuationToken);
		}

		public YouTubeVideoPageResult GetVideoPage(
			YouTubeChannel youTubeChannel, YouTubeChannelTabPage channelTabPage, string continuationToken)
		{
			return YouTubeApiV1.GetVideoPage(youTubeChannel?.Id, channelTabPage, continuationToken);
		}

		public YouTubeChannelTabResult GetChannelTab(
			YouTubeChannel youTubeChannel, YouTubeChannelTabPage youTubeChannelTabPage)
		{
			return YouTubeApiV1.GetChannelTab(youTubeChannel.Id, youTubeChannelTabPage);
		}

		public YouTubeApiV1SearchResults Search(string searchQuery, string continuationToken,
			YouTubeApiV1SearchResultFilter searchResultFilter)
		{
			return YouTubeApiV1.SearchYouTube(searchQuery, continuationToken, searchResultFilter);
		}

		public static string GetDefaultYouTubeClientId()
		{
			lock (_defaultYouTubeClientId)
			{
				return _defaultYouTubeClientId;
			}
		}

		public static void SetDefaultYouTubeClientId(string clientId)
		{
			lock (_defaultYouTubeClientId)
			{
				_defaultYouTubeClientId = clientId;
			}
		}

		public static IYouTubeClient GetYouTubeClient(string clientId)
		{
			return _clients.ContainsKey(clientId) ? _clients[clientId] : null;
		}

		/// <summary>
		/// Добавляет новый элемент или заменяет существующий элемент в списке.
		/// </summary>
		/// <returns>'true', если успешно; 'false', если неудачно.</returns>
		public static bool AddYouTubeClient(string clientId, IYouTubeClient client)
		{
			try
			{
				_clients[clientId] = client;
				return true;
			} catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				return false;
			}
		}

		public static bool RemoveYouTubeClient(string clientId)
		{
			return _clients.ContainsKey(clientId) ? _clients.TryRemove(clientId, out _) : false;
		}

		public static IEnumerable<string> GetYouTubeClientNames()
		{
			var keys = _clients.Keys;
			foreach (string name in keys)
			{
				yield return name;
			}
		}
	}
}
