using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	public class YouTubeChannel
	{
		public string DisplayName { get; }
		public string Id { get; }

		public YouTubeChannel(string id, string displayName)
		{
			Id = id;
			DisplayName = displayName;
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
		/// (до 30 последних видео из указанной вкладки канала).
		/// </param>
		internal static YouTubeVideoLitePageResult GetVideoLitePage(YouTubeChannel channel,
			YouTubeChannelTabPage channelTabPage, string continuationToken)
		{
			return YouTubeApiV1.GetChannelVideoLitePage(channel, channelTabPage, continuationToken);
		}

		/// <summary>
		/// Получить упрощённый список видео из вкладки со страницы канала, предварительно скачав эту страницу.
		/// </summary>
		/// <param name="channel">Канал на YouTube</param>
		/// <param name="channelTabPage">Запрашиваемая вкладка со страницы канала</param>
		/// <returns>Список из 30 последних видео из указанной вкладки со страницы канала</returns>
		internal static YouTubeVideoLitePageResult GetVideoLitePage(YouTubeChannel channel, YouTubeChannelTabPage channelTabPage)
		{
			return YouTubeApiV1.GetChannelVideoLitePage(channel, channelTabPage);
		}

		/// <summary>
		/// Получить упрощённый список видео из вкладки со страницы канала, предварительно скачав эту страницу.
		/// </summary>
		/// <param name="channelTabPage">Запрашиваемая вкладка со страницы текущего канала</param>
		/// <returns>Список из 30 последних видео из указанной вкладки со страницы канала</returns>
		internal YouTubeVideoLitePageResult GetVideoLitePage(YouTubeChannelTabPage channelTabPage)
		{
			return GetVideoLitePage(this, channelTabPage);
		}

		public string GetTabPageUrl(YouTubeChannelTabPage channelTabPage)
		{
			return GetTabPageUrl(Id, channelTabPage);
		}

		public static string GetTabPageUrl(string channelId, YouTubeChannelTabPage channelTabPage)
		{
			return channelTabPage.GetWebPageUrl(channelId);
		}

		public static IEnumerable<YouTubeChannelTab> FindTabList(YouTubeChannel channel, JObject channelTabResponse)
		{
			JObject j = channelTabResponse.Value<JObject>("contents");
			if (j == null)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine("\"contents\" not found!");
#endif
				yield break;
			}
			j = j.Value<JObject>("twoColumnBrowseResultsRenderer");
			if (j == null)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine("\"twoColumnBrowseResultsRenderer\" not found!");
#endif
				yield break;
			}
			JArray jaTabs = j.Value<JArray>("tabs");
			if (jaTabs == null || jaTabs.Count == 0)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine("Tabs is not found!");
#endif
				yield break;
			}

			foreach (JObject jTab in jaTabs.Cast<JObject>())
			{
				JObject jo = jTab.Value<JObject>("tabRenderer");
				if (jo != null) { yield return new YouTubeChannelTab(channel, jo); }
			}
		}

		public static YouTubeChannelTab FindSelectedTab(string response, YouTubeChannel channel = null)
		{
			JObject j = Utils.TryParseJson(response);

			return FindSelectedTab(j, channel);
		}
		internal static YouTubeChannelTab FindSelectedTab(JObject responseJson, YouTubeChannel channel = null)
		{
			IEnumerable<YouTubeChannelTab> tabs = FindTabList(channel, responseJson);
			return tabs.FirstOrDefault(tab => tab.IsSelected);
		}

		internal static YouTubeChannelTab FindSelectedTab(JArray jaTabs, YouTubeChannel channel)
		{
			JObject json = (JObject)jaTabs.FirstOrDefault(j =>
			{
				JObject jo = j.Value<JObject>("tabRenderer");
				return jo != null && jo.Value<bool>("selected");
			});

			return json != null ? new YouTubeChannelTab(channel, json) : null;
		}

		public override string ToString()
		{
			return $"{DisplayName} ({Id})";
		}
	}
}
