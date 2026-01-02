using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	internal class YouTubeChannelTabPageParserVideo : IYouTubeChannelTabPageRequestParser
	{
		internal YouTubeChannel Channel { get; }
		internal YouTubeChannelTabPage ChannelTabPage { get; }
		internal JObject ChannelTabPageResponse { get; }

		internal YouTubeChannelTabPageParserVideo(YouTubeChannel channel, YouTubeChannelTabPage channelTabPage,
			JObject channelTabPageResponse)
		{
			Channel = channel;
			ChannelTabPage = channelTabPage;
			ChannelTabPageResponse = channelTabPageResponse;
		}

		public IEnumerable<YouTubeVideoThumbnail> ExtractThumbnails(JObject jVideoItem)
		{
			JArray jaThumbnails = jVideoItem.Value<JObject>("richItemRenderer")?.Value<JObject>("content")?.Value<JObject>("videoRenderer")?.Value<JObject>("thumbnail")?.Value<JArray>("thumbnails");
			if (jaThumbnails != null && jaThumbnails.Count > 0)
			{
				var thumbnails = YouTubeChannelTab.ParseThumbnails(new JArray[] { jaThumbnails });
				if (thumbnails != null)
				{
					foreach (YouTubeVideoThumbnail thumbnail in thumbnails)
					{
						yield return thumbnail;
					}
				}
			}
		}

		public JArray FindGridItems()
		{
			JObject j = ChannelTabPageResponse?.Value<JObject>("contents");
			if (j == null)
			{
				JArray jaOnResponseReceivedActions = ChannelTabPageResponse?.Value<JArray>("onResponseReceivedActions");
				if (jaOnResponseReceivedActions == null || jaOnResponseReceivedActions.Count == 0)
				{
#if DEBUG
					System.Diagnostics.Debug.WriteLine($"{this}: \"contents\" or \"onResponseReceivedActions\" not found!");
#endif
					return null;
				}
				return (jaOnResponseReceivedActions[0] as JObject).Value<JObject>("appendContinuationItemsAction")?.Value<JArray>("continuationItems");
			}
			j = j.Value<JObject>("twoColumnBrowseResultsRenderer");
			if (j == null)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"{this}: \"twoColumnBrowseResultsRenderer\" not found!");
#endif
				return null;
			}
			JArray jaTabs = j.Value<JArray>("tabs");
			if (jaTabs == null || jaTabs.Count == 0)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"{this}: Tabs not found!");
#endif
				return null;
			}
			YouTubeChannelTab selectedTab = YouTubeChannel.FindSelectedTab(jaTabs, Channel);
			if (selectedTab == null)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"{this}: No selected tab found!");
#endif
				return null;
			}
			j = selectedTab.Data.Value<JObject>("tabRenderer");
			if (j == null)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"{this}: \"tabRenderer\" not found!");
#endif
				return null;
			}
			j = j.Value<JObject>("content");
			if (j == null)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"{this}: \"content\" not found!");
#endif
				return null;
			}
			j = j.Value<JObject>("richGridRenderer");
			if (j == null)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"{this}: \"richGridRenderer\" not found!");
#endif
				return null;
			}
			JArray ja = j.Value<JArray>("contents");
			if (ja == null || ja.Count == 0)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"{this}: \"contents\" not found or empty!");
#endif
				return null;
			}
			return ja;
		}

		public List<YouTubeVideoLite> Parse(out string continuationToken)
		{
			continuationToken = null;
			JArray jaItems = FindGridItems();
			if (jaItems != null && jaItems.Count > 0)
			{
				List<YouTubeVideoLite> videos = new List<YouTubeVideoLite>();
				foreach (JObject j in jaItems.Cast<JObject>())
				{
					JObject jVideoRenderer = j.Value<JObject>("richItemRenderer")?.Value<JObject>("content")?.Value<JObject>("videoRenderer");
					if (jVideoRenderer != null)
					{
						string videoId = jVideoRenderer.Value<string>("videoId");
						if (!string.IsNullOrEmpty(videoId))
						{
							JArray jRuns = jVideoRenderer.Value<JObject>("title")?.Value<JArray>("runs");
							string title = jRuns != null && jRuns.Count > 0 ? (jRuns[0] as JObject).Value<string>("text") : "<untitled>";
							string length = jVideoRenderer.Value<JObject>("lengthText").Value<string>("simpleText");
							List<YouTubeVideoThumbnail> thumbnails = ExtractThumbnails(j).ToList();
							if (thumbnails.Count > 1)
							{
								thumbnails.Sort((x, y) => x.Height > y.Height ? -1 : 1);
							}
							videos.Add(new YouTubeVideoLite(title, videoId, length, thumbnails, Channel, ChannelTabPage));
						}
					}
					else
					{
						continuationToken = YouTubeChannelTab.ExtractContinuationToken(j);
					}
				}

				return videos;
			}

			return null;
		}

		public override string ToString()
		{
			return "YouTubeChannelTabPageParserVideo";
		}
	}
}
