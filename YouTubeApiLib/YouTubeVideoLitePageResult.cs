
namespace YouTubeApiLib
{
	public class YouTubeVideoLitePageResult
	{
		public YouTubeVideoLitePage VideoLitePage { get; }
		public int ErrorCode { get; }

		public YouTubeVideoLitePageResult(YouTubeVideoLitePage videoLitePage, int errorCode)
		{
			VideoLitePage = videoLitePage;
			ErrorCode = errorCode;
		}
	}
}
