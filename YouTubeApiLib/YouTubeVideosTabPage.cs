﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YouTubeApiLib
{
	public class YouTubeVideosTabPage
	{
		public List<string> IdList { get; }
		public List<YouTubeVideo> VideoList { get; private set; }
		public string NextPageToken { get; }

		public YouTubeVideosTabPage(List<string> idList, string nextPageToken)
		{
			IdList = idList;
			NextPageToken = nextPageToken;
		}

		public bool UpdateVideos()
		{
			if (IdList != null && IdList.Count > 0)
			{
				VideoList = new List<YouTubeVideo>();
				YouTubeApi api = new YouTubeApi();
				foreach (string videoId in IdList)
				{
					YouTubeVideoId youTubeVideoId = new YouTubeVideoId(videoId);
					YouTubeVideo video = api.GetVideo(youTubeVideoId);
					if (video != null)
					{
						VideoList.Add(video);
					}
				}
			}

			return VideoList != null;
		}

		public bool UpdateVideosMultiThreaded()
		{
			if (IdList != null && IdList.Count > 0)
			{
				ConcurrentBag<YouTubeVideo> bag = new ConcurrentBag<YouTubeVideo>();
				YouTubeApi api = new YouTubeApi();
				var tasks = IdList.Select(videoId => Task.Run(() =>
				{
					YouTubeVideoId youTubeVideoId = new YouTubeVideoId(videoId);
					YouTubeVideo video = api.GetVideo(youTubeVideoId);
					if (video != null)
					{
						bag.Add(video);
					}
				}));

				Task.WhenAll(tasks).Wait();

				if (bag.Count > 0)
				{
					VideoList = new List<YouTubeVideo>();
					foreach (YouTubeVideo item in bag)
					{
						VideoList.Add(item);
					}
				}
			}

			return VideoList != null;
		}
	}
}
