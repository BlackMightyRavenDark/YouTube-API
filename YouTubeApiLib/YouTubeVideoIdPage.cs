using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	public class YouTubeVideoIdPage : IYouTubeVideoPageParser
	{
		public string RawData { get; }
		public List<string> VideoIds { get; private set; }
		public string ContinuationToken { get; private set; }
		private bool _isContinuationToken;

		public YouTubeVideoIdPage(string rawData, bool isContinuationToken)
		{
			RawData = rawData;
			_isContinuationToken = isContinuationToken;
		}

		/// <summary>
		/// Проанализировать данные, создать из них список ID видео и поместить его в свойство 'VideoIds'.
		/// А так же, поместить в свойство 'ContinuationToken' токен для получения следующей страницы данных, если он есть.
		/// </summary>
		/// <returns>Количество ID видео.</returns>
		public int Parse()
		{
			JObject json = Utils.TryParseJson(RawData);
			if (json == null) { return 0; }
			JArray jaItems = Utils.FindItemsArray(json, _isContinuationToken);
			if (jaItems == null)
			{
				return 0;
			}
			VideoIds = Utils.ExtractVideoIDsFromGridRendererItems(jaItems, out string token);
			ContinuationToken = token;
			return VideoIds != null ? VideoIds.Count : 0;
		}
	}
}
