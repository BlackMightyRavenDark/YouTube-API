using System;
using System.Collections.Generic;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public class YouTubeVideoLite
	{
		public string Title { get; }
		public string Id { get; }
		public string Length { get; }
		public TimeSpan Duration{ get; }
		public List<YouTubeVideoThumbnail> Thumbnails { get; }
		public YouTubeChannel OwnedChannel { get; }
		public YouTubeChannelTabPage ChannelTabPage { get; }
		public YouTubeVideo FullInfo { get; private set; }

		public YouTubeVideoLite(string title, string id, string length,
			IEnumerable<YouTubeVideoThumbnail> thumbnails, YouTubeChannel ownedChannel, YouTubeChannelTabPage channelTabPage)
		{
			Title = title;
			Id = id;
			Length = length;
			bool isLiveNow = string.Equals(length, "live", StringComparison.OrdinalIgnoreCase);
			Duration = !isLiveNow ? Utils.DurationFromString(length) : TimeSpan.Zero;
			if (thumbnails != null)
			{
				Thumbnails = new List<YouTubeVideoThumbnail>(thumbnails);
			}
			OwnedChannel = ownedChannel;
			ChannelTabPage = channelTabPage;
		}

		public int UpdateFullInformation(IYouTubeClient client, FileDownloader downloader)
		{
			FullInfo = YouTubeVideo.GetById(Id, client, downloader);
			return FullInfo != null && FullInfo.IsInfoAvailable ? 200 : 404;
		}

		public int UpdateFullInformation(IYouTubeClient client = null)
		{
			return UpdateFullInformation(client, null);
		}
	}
}
