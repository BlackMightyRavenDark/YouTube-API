using System.Collections.Generic;
using System.Collections.Specialized;
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

		public static int CallHiddenApi(string url, NameValueCollection headers, string body, out string response)
		{
			try
			{
				using (HttpRequestResult requestResult = HttpRequestSender.Send("POST", url, body, headers))
				{
					if (requestResult.ErrorCode == 200)
					{
						return requestResult.WebContent.ContentToString(out response);
					}
					else
					{
						response = requestResult.ErrorMessage;
						return requestResult.ErrorCode;
					}
				}
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				response = ex.Message;
				return ex.HResult;
			}
		}

		public static int CallBrowseApi(NameValueCollection headers, string body, out string response)
		{
			string url = GetBrowseRequestUrl();
			return CallHiddenApi(url, headers, body, out response);
		}

		public static int CallPlayerApi(NameValueCollection headers, string body, out string response)
		{
			string url = GetPlayerRequestUrl();
			return CallHiddenApi(url, headers, body, out response);
		}

		public static int CallSearchApi(NameValueCollection headers, string body, out string response)
		{
			string url = GetSearchRequestUrl();
			return CallHiddenApi(url, headers, body, out response);
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
				["context"] = jContext,
				["browseId"] = channelId
			};

			bool tokenExists = !string.IsNullOrEmpty(continuationToken) && !string.IsNullOrWhiteSpace(continuationToken);
			if (tokenExists)
			{
				json["continuation"] = continuationToken;
			}
			else if (youTubeChannelTabPage != null)
			{
				json["params"] = youTubeChannelTabPage.ParamsId;
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

		internal static YouTubeVideoPageResult GetVideoPage(string channelId, YouTubeChannelTabPage tabPage, string continuationToken)
		{
			YouTubeVideoIdPageResult videoIdPageResult = YouTubeChannel.GetVideoIdPage(channelId, tabPage, continuationToken);
			if (videoIdPageResult.ErrorCode == 200)
			{
				LinkedList<YouTubeVideo> videos = new LinkedList<YouTubeVideo>();
				foreach (string videoId in videoIdPageResult.VideoIdPage.VideoIds)
				{
					YouTubeVideo video = YouTubeVideo.GetById(videoId);
					if (video != null && video.Status != null)
					{
						videos.AddLast(video);
					}
				}

				return new YouTubeVideoPageResult(new YouTubeVideoPage(videos, videoIdPageResult.VideoIdPage.ContinuationToken), 200);
			}

			return new YouTubeVideoPageResult(null, videoIdPageResult.ErrorCode);
		}

		internal static YouTubeVideoIdPageResult GetVideoIdPage(string channelId, YouTubeChannelTabPage tabPage, string continuationToken)
		{
			JObject body = GenerateChannelTabRequestBody(channelId, tabPage, continuationToken);
			bool tokenExists = !string.IsNullOrEmpty(continuationToken) && !string.IsNullOrWhiteSpace(continuationToken);
			return GetVideoIdPage(body, tokenExists);
		}

		internal static YouTubeVideoIdPageResult GetVideoIdPage(JObject requestBody, bool continuationTokenExists)
		{
			string url = GetBrowseRequestUrl();
			string body = requestBody != null ? requestBody.ToString() : string.Empty;
			int errorCode = YouTubeHttpPost(url, body, out string response);
			if (errorCode == 200)
			{
				YouTubeVideoIdPage videoIdPage = new YouTubeVideoIdPage(response, continuationTokenExists);
				int count = videoIdPage.Parse();
				return new YouTubeVideoIdPageResult(videoIdPage, count > 0 ? 200 : 400);
			}
			return new YouTubeVideoIdPageResult(null, errorCode);
		}

		internal static YouTubeVideoIdPageResult GetVideoIdPage(string channelId, YouTubeChannelTabPage channelTabPage)
		{
			string url = channelTabPage.GetWebPageUrl(channelId);
			int errorCode = InternetWebPage.DownloadWebPageCode(url, out string response);
			if (errorCode == 200)
			{
				YouTubeInitialData initialData = YouTubeInitialData.ExtractFromWebPageCode(response);
				if (initialData != null)
				{
					YouTubeVideoIdPage videoIdPage = new YouTubeVideoIdPage(initialData.RawData, false);
					int count = videoIdPage.Parse();
					return new YouTubeVideoIdPageResult(videoIdPage, count > 0 ? 200 : 400);
				}
			}

			return new YouTubeVideoIdPageResult(null, errorCode);
		}

		internal static YouTubeVideoListResult GetChannelVideoList(string channelId, IYouTubeClient client)
		{
			JArray resList = new JArray();
			string continuationToken = null;
			int errorCode;
			while (true)
			{
				YouTubeVideoIdPageResult videoIdPageResult = YouTubeChannel.GetVideoIdPage(channelId, YouTubeChannelTabPages.Videos, continuationToken);

				errorCode = videoIdPageResult.ErrorCode;
				if (errorCode != 200)
				{
					break;
				}

				foreach (string videoId in videoIdPageResult.VideoIdPage.VideoIds)
				{
					YouTubeSimplifiedVideoInfoResult simplifiedVideoInfoResult = GetSimplifiedVideoInfo(videoId, client);
					if (simplifiedVideoInfoResult.ErrorCode == 200)
					{
						resList.Add(simplifiedVideoInfoResult.SimplifiedVideoInfo.Info);
					}
				}

				continuationToken = videoIdPageResult.VideoIdPage.ContinuationToken;
				bool continuationTokenExists = !string.IsNullOrEmpty(continuationToken) && !string.IsNullOrEmpty(continuationToken);
				if (!continuationTokenExists)
				{
					break;
				}

				System.Diagnostics.Debug.WriteLine(continuationToken);
			}

			return new YouTubeVideoListResult(resList, resList.Count > 0 ? 200 : errorCode);
		}

		internal static YouTubeChannelTabResult GetChannelTab(string channelId, YouTubeChannelTabPage channelTabPage)
		{
			string url = GetBrowseRequestUrl();
			JObject body = GenerateChannelTabRequestBody(channelId, channelTabPage, null);
			int errorCode = YouTubeHttpPost(url, body.ToString(), out string response);
			if (errorCode == 200)
			{
				JObject json = TryParseJson(response);
				if (json == null)
				{
					return new YouTubeChannelTabResult(null, 404);
				}

				YouTubeChannelTab selectedTab = YouTubeChannelTab.FindSelectedTab(json);
				if (selectedTab == null)
				{
					return new YouTubeChannelTabResult(null, 404);
				}

				if (selectedTab.Title != channelTabPage.Title)
				{
					// If the requested tab is not exists,
					// API returns the "Home" tab content.
					return new YouTubeChannelTabResult(null, 404);
				}

				return new YouTubeChannelTabResult(selectedTab, errorCode);
			}

			return new YouTubeChannelTabResult(null, errorCode);
		}

		public static YouTubeChannelTabPageContentResult GetChannelTabContentRawData(
			string channelId, YouTubeChannelTabPage channelTabPage, string continuationToken)
		{
			string url = GetBrowseRequestUrl();
			JObject body = GenerateChannelTabRequestBody(channelId, channelTabPage, continuationToken);
			int errorCode = YouTubeHttpPost(url, body.ToString(), out string response);
			return new YouTubeChannelTabPageContentResult(
				new YouTubeChannelTabPageContent(channelTabPage, response), errorCode);
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
