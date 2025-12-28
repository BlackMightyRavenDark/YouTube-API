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
					System.Diagnostics.Debug.WriteLine($"{this}: \"contents\" or \"onResponseReceivedActions\" not found!");
					return null;
				}
				return (jaOnResponseReceivedActions[0] as JObject).Value<JObject>("appendContinuationItemsAction")?.Value<JArray>("continuationItems");
			}
			j = j.Value<JObject>("twoColumnBrowseResultsRenderer");
			if (j == null)
			{
				System.Diagnostics.Debug.WriteLine($"{this}: \"twoColumnBrowseResultsRenderer\" not found!");
				return null;
			}
			JArray jaTabs = j.Value<JArray>("tabs");
			if (jaTabs == null || jaTabs.Count == 0)
			{
				System.Diagnostics.Debug.WriteLine($"{this}: Tabs not found!");
				return null;
			}
			YouTubeChannelTab selectedTab = YouTubeChannel.FindSelectedTab(jaTabs, Channel);
			if (selectedTab == null)
			{
				System.Diagnostics.Debug.WriteLine($"{this}: No selected tab found!");
				return null;
			}
			j = selectedTab.Data.Value<JObject>("tabRenderer");
			if (j == null)
			{
				System.Diagnostics.Debug.WriteLine($"{this}: \"tabRenderer\" not found!");
				return null;
			}
			j = j.Value<JObject>("content");
			if (j == null)
			{
				System.Diagnostics.Debug.WriteLine($"{this}: \"content\" not found!");
				return null;
			}
			j = j.Value<JObject>("richGridRenderer");
			if (j == null)
			{
				System.Diagnostics.Debug.WriteLine($"{this}: \"richGridRenderer\" not found!");
				return null;
			}
			JArray ja = j.Value<JArray>("contents");
			if (ja == null || ja.Count == 0)
			{
				System.Diagnostics.Debug.WriteLine($"{this}: \"contents\" not found or empty!");
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
							videos.Add(new YouTubeVideoLite(title, videoId, length, thumbnails, Channel, YouTubeChannelTabPages.Videos));
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
