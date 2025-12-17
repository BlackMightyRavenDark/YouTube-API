using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public class YouTubeVideosTabPage
	{
		public List<string> IdList { get; }
		public List<YouTubeVideo> VideoList { get; private set; }
		public string NextPageToken { get; }

		public YouTubeVideosTabPage(IEnumerable<string> idList, string nextPageToken)
		{
			IdList = idList.ToList();
			NextPageToken = nextPageToken;
		}

		public bool UpdateVideos()
		{
			if (IdList != null && IdList.Count > 0)
			{
				VideoList = new List<YouTubeVideo>();
				foreach (string videoId in IdList)
				{
					YouTubeVideo video = YouTubeVideo.GetById(videoId);
					if (video != null)
					{
						VideoList.Add(video);
					}
				}
			}

			return VideoList != null;
		}

		public bool UpdateVideosMultiThreaded(byte simultaneousThreads,
			CancellationToken cancellationToken, FileDownloader downloader = null)
		{
			if (IdList != null && IdList.Count > 0)
			{
				if (simultaneousThreads < 1) { simultaneousThreads = 2; }
				ConcurrentBag<YouTubeVideo> bag = new ConcurrentBag<YouTubeVideo>();

				for (int i = 0; i < IdList.Count && !cancellationToken.IsCancellationRequested; i += simultaneousThreads)
				{
					int remaining = IdList.Count - i;
					int count = remaining > simultaneousThreads ? simultaneousThreads : remaining;
					if (count > 0)
					{
						List<string> idGroup = IdList.GetRange(i, count);
						if (idGroup.Count > 0)
						{
							var tasks = idGroup.Select(videoId => Task.Run(() =>
							{
								YouTubeVideo video = YouTubeVideo.GetById(videoId, downloader);
								if (video != null) { bag.Add(video); }
							}));
							Task.WhenAll(tasks).Wait();
						}
					}
				}

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

		public bool UpdateVideosMultiThreaded(byte simultaneousThreads)
		{
			return UpdateVideosMultiThreaded(simultaneousThreads, default);
		}

		public bool UpdateVideosMultiThreaded()
		{
			return UpdateVideosMultiThreaded(2);
		}
	}
}
