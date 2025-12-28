using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	internal class YouTubeChannelTabPageParserShorts : IYouTubeChannelTabPageRequestParser
	{
		internal YouTubeChannel Channel { get; }
		internal YouTubeChannelTabPage ChannelTabPage { get; }
		internal JObject ChannelTabPageResponse { get; }

		internal YouTubeChannelTabPageParserShorts(YouTubeChannel channel, YouTubeChannelTabPage channelTabPage,
			JObject channelTabPageResponse)
		{
			Channel = channel;
			ChannelTabPage = channelTabPage;
			ChannelTabPageResponse = channelTabPageResponse;
		}

		public JArray FindGridItems()
		{
			IYouTubeChannelTabPageRequestParser parser = new YouTubeChannelTabPageParserVideo(Channel, ChannelTabPage, ChannelTabPageResponse);
			return parser.FindGridItems();
		}

		public IEnumerable<YouTubeVideoThumbnail> ExtractThumbnails(JObject jVideoItem)
		{
			JObject jShortsLockupViewModel = jVideoItem.Value<JObject>("richItemRenderer")?.Value<JObject>("content").Value<JObject>("shortsLockupViewModel");
			if (jShortsLockupViewModel != null)
			{
				JArray jaThumbnails1 = jShortsLockupViewModel.Value<JObject>("onTap")?.Value<JObject>("innertubeCommand")?.Value<JObject>("reelWatchEndpoint")?.Value<JObject>("thumbnail")?.Value<JArray>("thumbnails");
				JArray jaThumbnails2 = jShortsLockupViewModel.Value<JObject>("thumbnailViewModel")?.Value<JObject>("thumbnailViewModel").Value<JObject>("image").Value<JArray>("sources");
				var thumbnails = YouTubeChannelTab.ParseThumbnails(new JArray[] { jaThumbnails1, jaThumbnails2 });
				if (thumbnails != null)
				{
					foreach (YouTubeVideoThumbnail thumbnail in thumbnails)
					{
						yield return thumbnail;
					}
				}
			}
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
					JObject jShortsLockupViewModel = j.Value<JObject>("richItemRenderer")?.Value<JObject>("content").Value<JObject>("shortsLockupViewModel");
					if (jShortsLockupViewModel != null)
					{
						string entityId = jShortsLockupViewModel.Value<string>("entityId");
						string videoId = entityId.Length > 10 ? entityId.Substring(entityId.Length - 11) : entityId;
						if (!string.IsNullOrEmpty(videoId))
						{
							string title = jShortsLockupViewModel.Value<JObject>("overlayMetadata")?.Value<JObject>("primaryText")?.Value<string>("content") ?? "<untitled>";
							List<YouTubeVideoThumbnail> thumbnails = ExtractThumbnails(j).ToList();
							if (thumbnails.Count > 1)
							{
								thumbnails.Sort((x, y) => x.Height > y.Height ? -1 : 1);
							}
							videos.Add(new YouTubeVideoLite(title, videoId, null, thumbnails, Channel, YouTubeChannelTabPages.Shorts));
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
			return "YouTubeChannelTabPageParserShorts";
		}
	}
}
