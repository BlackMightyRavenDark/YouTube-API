#if DEBUG
using System;
#endif
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	public class YouTubeVideoLitePage
	{
		public string RawData { get; }
		public YouTubeChannel Channel { get; }
		public JObject RequestBody { get; }
		public List<YouTubeVideoLite> Videos { get; private set; }
		public YouTubeChannelTabPage ChannelTabPage { get; private set; }
		public string ContinuationToken { get; private set; }
		public bool IsContinuationTokenUsedInRequest { get; private set; }
		public bool HasNextPage { get; private set; }
		public int Count => Videos != null ? Videos.Count : 0;

		public YouTubeVideoLitePage(YouTubeChannel channel, string rawData, JObject requestBody)
		{
			Channel = channel;
			RawData = rawData;
			RequestBody = requestBody;
			if (!ParseRequestBody())
			{
				ChannelTabPage = YouTubeChannelTabPages.Home;
				IsContinuationTokenUsedInRequest = false;
			}
		}

		public YouTubeVideoLitePage(YouTubeChannel channel, YouTubeChannelTabPage channelTabPage,
			string rawData, JObject requestBody)
		{
			Channel = channel;
			ChannelTabPage = channelTabPage;
			RawData = rawData;
			RequestBody = requestBody;
			string token = RequestBody?.Value<string>("continuation");
			IsContinuationTokenUsedInRequest = !string.IsNullOrEmpty(token) && !string.IsNullOrWhiteSpace(token);
		}

		public YouTubeVideoLitePage(YouTubeChannel channel, YouTubeChannelTabPage channelTabPage,
			string rawData, bool isContinuationTokenUsedInRequest)
		{
			Channel = channel;
			ChannelTabPage = channelTabPage;
			RawData = rawData;
			IsContinuationTokenUsedInRequest = isContinuationTokenUsedInRequest;
		}

		/// <summary>
		/// Проанализировать данные, создать из них список с упрощённой информацией о видео и поместить его в свойство 'Videos'.
		/// А так же, поместить в свойство 'ContinuationToken' токен для получения следующей страницы данных, если он есть.
		/// Внимание! Текущий список видео и токен будут потеряны!
		/// </summary>
		/// <returns>Количество видео в новом списке.</returns>
		public int Parse()
		{
			Videos = null;
			ContinuationToken = null;
			HasNextPage = false;
			JObject json = Utils.TryParseJson(RawData);
			if (json == null) { return 0; }

			string token;
			if (ChannelTabPage == YouTubeChannelTabPages.Videos || ChannelTabPage == YouTubeChannelTabPages.Live)
			{
				IYouTubeChannelTabPageRequestParser parser = new YouTubeChannelTabPageParserVideo(Channel, ChannelTabPage, json);
				Videos = parser.Parse(out token);
			}
			else if (ChannelTabPage == YouTubeChannelTabPages.Shorts)
			{
				IYouTubeChannelTabPageRequestParser parser = new YouTubeChannelTabPageParserShorts(Channel, ChannelTabPage, json);
				Videos = parser.Parse(out token);
			}
			else
			{
				return 0;
			}

			ContinuationToken = token;
			HasNextPage = !string.IsNullOrEmpty(token) && !string.IsNullOrWhiteSpace(token);
			
			return Count;
		}

		private bool ParseRequestBody()
		{
			try
			{
				if (RequestBody != null)
				{
					string token = RequestBody.Value<string>("continuation");
					IsContinuationTokenUsedInRequest = !string.IsNullOrEmpty(token) && !string.IsNullOrWhiteSpace(token);

					string pageParams = RequestBody.Value<string>("params");
					ChannelTabPage = YouTubeChannelTabPages.PageFromParams(pageParams);

					return true;
				}
			}
#if DEBUG
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
#else
			catch { }
#endif
			return false;
		}
	}
}
