using System.Net;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public interface IYouTubeClient
	{
		string DisplayName { get; }
		YouTubeVideoWebPage WebPage { get; }
		FileDownloader Downloader { get; set; }
		JObject GenerateRequestBody(string videoId, YouTubeConfig youTubeConfig = null);
		WebHeaderCollection GenerateRequestHeaders(string videoId, YouTubeConfig youTubeConfig = null);
		YouTubeRawVideoInfoResult GetRawVideoInfo(YouTubeVideoId videoId, out string errorMessage);
		int GetRawVideoInfo(string videoId, out YouTubeRawVideoInfo rawVideoInfo, out string errorMessage);
		void SetWebPage(YouTubeVideoWebPage webPage);
	}
}
