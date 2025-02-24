using System.Collections.Specialized;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public class InternetWebPage
	{
		public static int DownloadWebPageCode(string url, out string response, FileDownloader downloader = null)
		{
			if (downloader == null)
			{
				NameValueCollection headers = new NameValueCollection()
				{
					{ "Host", "www.youtube.com" },
					{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:135.0) Gecko/20100101 Firefox/135.0" },
					{ "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
					{ "Accept-Language", "en-US" },
					{ "Accept-Encoding", "gzip" },
					{ "DNT", "1" },
					{ "Sec-GPC", "1" },
					{ "Upgrade-Insecure-Requests", "1" },
					{ "Sec-Fetch-Dest", "document" },
					{ "Sec-Fetch-Mode", "navigate" },
					{ "Sec-Fetch-Site", "none" },
					{ "Sec-Fetch-User", "?1" },
					{ "Priority", "u=0, i" }
				};

				downloader = new FileDownloader() { Headers = headers };
			}

			return Utils.DownloadString(url, out response, downloader);
		}
	}
}
