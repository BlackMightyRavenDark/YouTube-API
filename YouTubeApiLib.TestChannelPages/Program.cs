using System;

namespace YouTubeApiLib.TestChannelPages
{
	internal class Program
	{
		static void Main(string[] args)
		{
			YouTubeApi api = new YouTubeApi();
			YouTubeChannel channel = new YouTubeChannel("UCSCHk4GbzMlKtxwpXPyYeMA", "Frozzen Fro");
			YouTubeChannelTabPage[] pages = new YouTubeChannelTabPage[]
			{
				YouTubeChannelTabPages.Videos,
				YouTubeChannelTabPages.Shorts,
				YouTubeChannelTabPages.Live
			};

			foreach (YouTubeChannelTabPage page in pages)
			{
				YouTubeVideoLitePageResult videoLitePageResult = api.GetChannelVideoPage(channel, page, null);
				if (videoLitePageResult.ErrorCode == 200)
				{
					Console.WriteLine($"{channel} {page.Title} tab page:");
					foreach (YouTubeVideoLite videoLight in videoLitePageResult.VideoLitePage.Videos)
					{
						Console.WriteLine($"{videoLight.Id} > {videoLight.Title}");
					}

					string token = videoLitePageResult.VideoLitePage.HasNextPage ? videoLitePageResult.VideoLitePage.ContinuationToken : "null";
					Console.WriteLine($"Continuation token: {token}");
				}
				else
				{
					Console.WriteLine($"{channel} {page.Title} tab page: null");
				}
			}

			Console.ReadLine();
		}
	}
}
