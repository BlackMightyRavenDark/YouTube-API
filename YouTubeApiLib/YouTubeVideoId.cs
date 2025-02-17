
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

		public override string ToString()
		{
			return Id;
		}
	}
}
