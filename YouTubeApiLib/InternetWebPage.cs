using System.Net;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public class InternetWebPage
	{
		public static int DownloadWebPageCode(string url, out string response, FileDownloader downloader = null)
		{
			if (downloader == null)
			{
				WebHeaderCollection headers = GetDefaultHttpHeaders();
				downloader = new FileDownloader() { Headers = headers };
			}

			return Utils.DownloadString(url, out response, downloader);
		}

		public static WebHeaderCollection GetDefaultHttpHeaders()
		{
			return new WebHeaderCollection()
			{
				{ "Host", "www.youtube.com" },
				{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:151.0) Gecko/20100101 Firefox/151.0" },
				{ "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
				{ "Accept-Language", "en-US,en;q=0.9" },
				{ "Accept-Encoding", "gzip, deflate, br, zstd" },
				{ "Upgrade-Insecure-Requests", "1" },
				{ "Sec-Fetch-Dest", "document" },
				{ "Sec-Fetch-Mode", "navigate" },
				{ "Sec-Fetch-Site", "none" },
				{ "Sec-Fetch-User", "?1" },
				{ "Connection", "keep-alive" }
			};
		}
	}
}
