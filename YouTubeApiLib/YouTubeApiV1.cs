using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;
using static YouTubeApiLib.Utils;

namespace YouTubeApiLib
{
	public static class YouTubeApiV1
	{
		public const string API_V1_KEY = "AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8";
		public const string API_V1_BROWSE_URL = "https://www.youtube.com/youtubei/v1/browse";
		public const string API_V1_PLAYER_URL = "https://www.youtube.com/youtubei/v1/player";
		public const string API_V1_SEARCH_URL = "https://www.youtube.com/youtubei/v1/search";

		public static int CallHiddenApi(string url, WebHeaderCollection headers, CookieContainer cookies, IWebProxy proxy,
			string body, int timeout, out string response)
		{
			try
			{
				byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
				using (HttpRequestResult requestResult = HttpRequestSender.Send("POST", url, bodyBytes, headers, cookies, proxy, timeout))
				{
					response = requestResult.HasErrorMessage ? requestResult.ErrorMessage : null;
					int errorCode = requestResult.ErrorCode == 200 ? requestResult.GetContent(out response) : requestResult.ErrorCode;
					return errorCode == 200 ? requestResult.WebContent.ContentToString(out response) : errorCode;
				}
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				response = ex.Message;
				return ex.HResult;
			}
		}

		public static int CallBrowseApi(WebHeaderCollection headers, CookieContainer cookies, IWebProxy proxy,
			int timeout, string body, out string response)
		{
			string url = GetBrowseRequestUrl();
			return CallHiddenApi(url, headers, null, null, body, timeout, out response);
		}

		public static int CallPlayerApi(WebHeaderCollection headers, CookieContainer cookies, IWebProxy proxy,
			int timeout, string body, out string response)
		{
			string url = GetPlayerRequestUrl();
			return CallHiddenApi(url, headers, cookies, proxy, body, timeout, out response);
		}

		public static int CallSearchApi(WebHeaderCollection headers, CookieContainer cookies, IWebProxy proxy,
			int timeout, string body, out string response)
		{
			string url = GetSearchRequestUrl();
			return CallHiddenApi(url, headers, cookies, proxy, body, timeout, out response);
		}

		public static JObject GenerateSearchQueryRequestBody(
			string searchQuery, string continuationToken, YouTubeApiV1SearchResultFilter filter)
		{
			const string CLIENT_NAME = "WEB";
			const string CLIENT_VERSION = "2.20210408.08.00";

			JObject jClient = GenerateYouTubeClientBody(CLIENT_NAME, CLIENT_VERSION);
			jClient["utcOffsetMinutes"] = 0;

			JObject jContext = new JObject()
			{
				["client"] = jClient
			};

			JObject json = new JObject()
			{
				["context"] = jContext
			};

			if (!string.IsNullOrEmpty(searchQuery) && !string.IsNullOrWhiteSpace(searchQuery))
			{
				json["query"] = searchQuery;
				if (filter != YouTubeApiV1SearchResultFilters.None)
				{
					json["params"] = filter.ParamsId;
				}
			}
			else if (!string.IsNullOrEmpty(continuationToken) && !string.IsNullOrWhiteSpace(continuationToken))
			{
				json["continuation"] = continuationToken;
			}

			return json;
		}

		public static JObject GenerateChannelTabRequestBody(string channelId,
			YouTubeChannelTabPage youTubeChannelTabPage, string continuationToken)
		{
			const string CLIENT_NAME = "WEB";
			const string CLIENT_VERSION = "2.20241029.07.00";

			JObject jClient = GenerateYouTubeClientBody(CLIENT_NAME, CLIENT_VERSION);
			JObject jContext = new JObject()
			{
				["client"] = jClient
			};

			JObject json = new JObject()
			{
				["context"] = jContext
			};

			bool tokenExists = !string.IsNullOrEmpty(continuationToken) && !string.IsNullOrWhiteSpace(continuationToken);
			if (tokenExists)
			{
				json["continuation"] = continuationToken;
			}
			else
			{
				if (!string.IsNullOrEmpty(channelId) && !string.IsNullOrWhiteSpace(channelId))
				{
					json["browseId"] = channelId;
				}
				if (youTubeChannelTabPage != null)
				{
					json["params"] = youTubeChannelTabPage.ParamsId;
				}
			}

			return json;
		}

		public static JObject GenerateYouTubeClientBody(string clientName, string clientVersion, string hl, string gl)
		{
			return new JObject()
				{
					["clientName"] = clientName,
					["clientVersion"] = clientVersion,
					["hl"] = hl,
					["gl"] = gl
				};
		}

		public static JObject GenerateYouTubeClientBody(string clientName, string clientVersion)
		{
			return GenerateYouTubeClientBody(clientName, clientVersion, "en", "US");
		}

		public static JObject GenerateChannelVideoListRequestBody(string channelId, string continuationToken)
		{
			return GenerateChannelTabRequestBody(channelId, YouTubeChannelTabPages.Videos, continuationToken);
		}

		public static string GetBrowseRequestUrl()
		{
			return $"{API_V1_BROWSE_URL}?key={API_V1_KEY}";
		}

		public static string GetPlayerRequestUrl()
		{
			return $"{API_V1_PLAYER_URL}?key={API_V1_KEY}";
		}

		public static string GetSearchRequestUrl()
		{
			return $"{API_V1_SEARCH_URL}?key={API_V1_KEY}";
		}

		internal static YouTubeRawVideoInfoResult GetRawVideoInfo(YouTubeVideoId videoId)
		{
			IYouTubeClient client = YouTubeApi.GetYouTubeClient(YouTubeApi.GetDefaultYouTubeClientId());
			return client != null ? client.GetRawVideoInfo(videoId, out _) : new YouTubeRawVideoInfoResult(null, 400);
		}

		internal static YouTubeRawVideoInfoResult GetRawVideoInfo(string videoId)
		{
			return GetRawVideoInfo(new YouTubeVideoId(videoId));
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
		internal static YouTubeVideoLitePageResult GetChannelVideoLitePage(
			YouTubeChannel channel, YouTubeChannelTabPage channelTabPage, string continuationToken)
		{
			JObject body = GenerateChannelTabRequestBody(channel.Id, channelTabPage, continuationToken);
			return GetChannelVideoLitePage(body, channel, channelTabPage);
		}

		/// <summary>
		/// Получить список ID видео из вкладки со страницы канала, используя API YouTube V1.
		/// </summary>
		/// <param name="requestBody">Тело запроса</param>
		/// <param name="channel">Канал на YouTube. Будет привязан к результату запроса.</param>
		/// <param name="channelTabPage">Используется для идентификации запрошенной страницы</param>
		internal static YouTubeVideoLitePageResult GetChannelVideoLitePage(JObject requestBody,
			YouTubeChannel channel, YouTubeChannelTabPage channelTabPage)
		{
			string url = GetBrowseRequestUrl();
			string body = requestBody != null ? requestBody.ToString() : string.Empty;
			int errorCode = YouTubeHttpPost(url, body, out string response);
			if (errorCode == 200)
			{
				YouTubeVideoLitePage videoLitePage = new YouTubeVideoLitePage(channel, channelTabPage, response, requestBody);
				int count = videoLitePage.Parse();
				return new YouTubeVideoLitePageResult(videoLitePage, count > 0 ? 200 : 400);
			}
			return new YouTubeVideoLitePageResult(null, errorCode);
		}

		/// <summary>
		/// Получить упрощённый список видео из вкладки со страницы канала, предварительно скачав эту страницу.
		/// </summary>
		/// <param name="channel">Канал на YouTube</param>
		/// <param name="channelTabPage">Запрашиваемая вкладка со страницы канала</param>
		/// <returns>Список из 30 последних видео из указанной вкладки со страницы канала</returns>
		internal static YouTubeVideoLitePageResult GetChannelVideoLitePage(YouTubeChannel channel, YouTubeChannelTabPage channelTabPage)
		{
			string url = channelTabPage.GetWebPageUrl(channel.Id);
			int errorCode = InternetWebPage.DownloadWebPageCode(url, out string response);
			if (errorCode == 200)
			{
				YouTubeInitialData initialData = YouTubeInitialData.ExtractFromWebPageCode(response);
				if (initialData != null)
				{
					return initialData.ToVideoLitePage(channel, channelTabPage);
				}

				return new YouTubeVideoLitePageResult(null, 404);
			}

			return new YouTubeVideoLitePageResult(null, errorCode);
		}

		internal static YouTubeChannelTabResult GetChannelTab(YouTubeChannel channel, YouTubeChannelTabPage channelTabPage)
		{
			string url = GetBrowseRequestUrl();
			JObject body = GenerateChannelTabRequestBody(channel.Id, channelTabPage, null);
			int errorCode = YouTubeHttpPost(url, body.ToString(), out string response);
			if (errorCode == 200)
			{
				JObject json = TryParseJson(response);
				if (json == null)
				{
					return new YouTubeChannelTabResult(null, 404);
				}

				YouTubeChannelTab selectedTab = YouTubeChannel.FindSelectedTab(json, channel);
				if (selectedTab == null)
				{
					return new YouTubeChannelTabResult(null, 404);
				}

				if (selectedTab.Title != channelTabPage.Title)
				{
					// Если запрашиваемая вкладка не существует, API ютуба выдаёт данные вкладки "Home".
					return new YouTubeChannelTabResult(null, 404);
				}

				return new YouTubeChannelTabResult(selectedTab, errorCode);
			}

			return new YouTubeChannelTabResult(null, errorCode);
		}

		internal static YouTubeApiV1SearchResults SearchYouTube(
			string searchQuery, string continuationToken,
			YouTubeApiV1SearchResultFilter searchResultFilter)
		{
			IYouTubeSearcher searcher = new YouTubeSearcherV1(searchQuery, continuationToken, searchResultFilter);
			return (YouTubeApiV1SearchResults)searcher.Search();
		}
	}
}
