
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
		/// Получить список ID видео из вкладки со страницы канала, используя API YouTube V1.
		/// </summary>
		/// <param name="channelTabPage">
		/// Запрашиваемая вкладка со страницы канала. Игнорируется, если указан continuation token.
		/// </param>
		/// <param name="continuationToken">
		/// Токен, указывающий, какая часть списка должна быть получена.
		/// Если передать 'null' или пустую строку, будут получены первые 30 элементов списка 
		/// (30 ID последних видео из указанной вкладки канала).
		/// </param>
		public YouTubeVideoIdPageResult GetVideoIdPage(YouTubeChannelTabPage channelTabPage, string continuationToken)
		{
			return GetVideoIdPage(Id, channelTabPage, continuationToken);
		}

		/// <summary>
		/// Получить список ID видео из вкладки со страницы канала, используя API YouTube V1.
		/// </summary>
		/// <param name="channelId">ID канала YouTube</param>
		/// <param name="channelTabPage">
		/// Запрашиваемая вкладка со страницы канала. Игнорируется, если указан continuation token.
		/// </param>
		/// <param name="continuationToken">
		/// Токен, указывающий, какая часть списка должна быть получена.
		/// Если передать 'null' или пустую строку, будут получены первые 30 элементов списка 
		/// (30 ID последних видео из указанной вкладки канала).
		/// </param>
		public static YouTubeVideoIdPageResult GetVideoIdPage(string channelId,
			YouTubeChannelTabPage channelTabPage, string continuationToken)
		{
			return YouTubeApiV1.GetVideoIdPage(channelId, channelTabPage, continuationToken);
		}

		/// <summary>
		/// Получить список ID видео из вкладки со страницы канала, предварительно скачав эту страницу.
		/// </summary>
		/// <param name="channelId">ID канала YouTube</param>
		/// <param name="channelTabPage">Запрашиваемая вкладка со страницы канала</param>
		/// <returns>Список из 30 ID последних видео из указанной вкладки со страницы канала</returns>
		public static YouTubeVideoIdPageResult GetVideoIdPage(string channelId, YouTubeChannelTabPage channelTabPage)
		{
			return YouTubeApiV1.GetVideoIdPage(channelId, channelTabPage);
		}

		/// <summary>
		/// Получить список ID видео из вкладки со страницы канала, предварительно скачав эту страницу.
		/// </summary>
		/// <param name="channelTabPage">Запрашиваемая вкладка со страницы канала</param>
		/// <returns>Список из 30 ID последних видео из указанной вкладки со страницы канала</returns>
		public YouTubeVideoIdPageResult GetVideoIdPage(YouTubeChannelTabPage channelTabPage)
		{
			return GetVideoIdPage(Id, channelTabPage);
		}

		public string GetTabPageUrl(YouTubeChannelTabPage channelTabPage)
		{
			return GetTabPageUrl(Id, channelTabPage);
		}

		public static string GetTabPageUrl(string channelId, YouTubeChannelTabPage channelTabPage)
		{
			return channelTabPage.GetWebPageUrl(channelId);
		}

		public override string ToString()
		{
			return $"{DisplayName} ({Id})";
		}
	}
}
