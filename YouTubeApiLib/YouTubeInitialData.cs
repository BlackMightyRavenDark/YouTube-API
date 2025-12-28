using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	public class YouTubeInitialData
	{
		public JObject Data { get; }
		public string RawData { get; }
		public bool DataWasReceivedWithContinuationToken { get; }

		public YouTubeInitialData(string rawData, bool dataWasReceivedWithContinuationToken = false)
		{
			RawData = rawData;
			Data = Utils.TryParseJson(rawData);
			DataWasReceivedWithContinuationToken = dataWasReceivedWithContinuationToken;
		}

		public static YouTubeInitialData ExtractFromWebPageCode(string webPageCode,
			string pattern = @"var ytInitialData =\s*(.*}}});</script")
		{
			return Utils.ExtractYouTubeInitialDataFromWebPageCode(webPageCode, pattern);
		}

		/// <summary>
		/// Извлечь упрощённый список видео из объекта.
		/// </summary>
		/// <param name="channel">Канал на YouTube, с которого были получены данные. Используется только как идентификатор.
		/// </param>
		/// <param name="channelTabPage">
		/// Вкладка со страницы канала, из которой были получены данные. Используется только как идентификатор.
		/// Если передать 'null', будет установлена вкладка "Videos".
		/// </param>
		public YouTubeVideoLitePageResult ToVideoLitePage(
			YouTubeChannel channel, YouTubeChannelTabPage channelTabPage)
		{
			YouTubeVideoLitePage videoLitePage = new YouTubeVideoLitePage(channel,
				channelTabPage ?? YouTubeChannelTabPages.Videos,
				RawData, DataWasReceivedWithContinuationToken);
			int errorCode = videoLitePage.Parse() > 0 ? 200 : 404;
			return new YouTubeVideoLitePageResult(videoLitePage, errorCode);
		}
	}
}
