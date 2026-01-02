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

		/// <summary>
		/// Получить упрощённый список видео из вкладки со страницы канала, используя API YouTube V1.
		/// </summary>
		/// <param name="channel">Канал на YouTube. Если указан continuation token, используется только как идентификатор.
		/// </param>
		/// <param name="channelTabPage">
		/// Запрашиваемая вкладка со страницы канала. Если указан continuation token, используется только как идентификатор.
		/// </param>
		/// <param name="continuationToken">
		/// Токен, указывающий, какая часть списка должна быть получена.
		/// Если передать 'null' или пустую строку, будут получены первые 30 элементов списка 
		/// (до 30 последних видео со вкладки канала).
		/// </param>
		public YouTubeVideoLitePageResult GetChannelVideoPage(YouTubeChannel channel,
			YouTubeChannelTabPage channelTabPage, string continuationToken)
		{
			return YouTubeChannel.GetVideoLitePage(channel, channelTabPage, continuationToken);
		}

		/// <summary>
		/// Получить упрощённый список видео из вкладки со страницы канала, предварительно скачав эту страницу.
		/// </summary>
		/// <param name="channel">Канал на YouTube</param>
		/// <param name="channelTabPage">Запрашиваемая вкладка со страницы канала</param>
		/// <returns>Список из 30 последних видео из указанной вкладки со страницы канала</returns>
		public YouTubeVideoLitePageResult GetChannelVideoPage(YouTubeChannel channel,
			YouTubeChannelTabPage channelTabPage)
		{
			return YouTubeChannel.GetVideoLitePage(channel, channelTabPage);
		}

		public YouTubeChannelTabResult GetChannelTab(
			YouTubeChannel youTubeChannel, YouTubeChannelTabPage youTubeChannelTabPage)
		{
			return YouTubeApiV1.GetChannelTab(youTubeChannel, youTubeChannelTabPage);
		}

		public YouTubeApiV1SearchResult Search(string searchQuery, string continuationToken,
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
			}
#if DEBUG
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
#else
			catch { }
#endif
			return false;
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
