using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public class YouTubeVideoId
	{
		public string Id { get; }

		public YouTubeVideoId(string id)
		{
			Id = id;
		}

		public YouTubeVideo GetVideo(IYouTubeClient client = null)
		{
			return YouTubeVideo.GetById(this, client);
		}

		public YouTubeVideoWebPageResult GetWebPage(FileDownloader downloader = null)
		{
			return YouTubeVideoWebPage.Get(Id, downloader);
		}

		public override string ToString()
		{
			return Id;
		}
	}
}
