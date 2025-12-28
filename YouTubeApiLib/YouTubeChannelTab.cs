using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	public class YouTubeChannelTab
	{
		public string Title { get; }
		public bool IsSelected { get; }
		public YouTubeChannel Channel { get; }
		public JObject Data { get; }

		public YouTubeChannelTab(YouTubeChannel channel, JObject tabContent, string title = null)
		{
			if (string.IsNullOrEmpty(title) || string.IsNullOrWhiteSpace(title))
			{
				string t = tabContent?.Value<string>("title");
				Title = string.IsNullOrEmpty(t) || string.IsNullOrWhiteSpace(t) ? title : t;
			}
			else
			{
				Title = title;
			}

			IsSelected = tabContent != null && tabContent.Value<bool>("selected");
			Channel = channel;
			Data = tabContent;
		}

		internal static IEnumerable<YouTubeVideoThumbnail> ParseThumbnails(JArray[] thumbnails)
		{
			foreach (JArray ja in thumbnails)
			{
				if (ja != null)
				{
					foreach (JObject j in ja.Cast<JObject>())
					{
						string url = j.Value<string>("url");
						ushort width = j.Value<ushort>("width");
						ushort height = j.Value<ushort>("height");
						string fileName = ExtractFileNameFromThumbnailUrl(url);
						yield return new YouTubeVideoThumbnail(width, height, fileName, url);
					}
				}
			}
		}

		private static string ExtractFileNameFromThumbnailUrl(string url)
		{
			string fileName = Utils.FindRegexp(url, @"/vi(?:_webp)?/.{11}/(.*)\?");
			if (string.IsNullOrEmpty(fileName) || string.IsNullOrWhiteSpace(fileName))
			{
				int n = url.LastIndexOf("/");
				if (n > 0)
				{
					return url.Substring(n + 1);
				}

				return "unnamed.jpg";
			}

			return fileName;
		}

		internal static string ExtractContinuationToken(JObject jTokenRoot)
		{
			JObject jContinuationItemRenderer = jTokenRoot.Value<JObject>("continuationItemRenderer");
			return jContinuationItemRenderer?.Value<JObject>("continuationEndpoint")?.Value<JObject>("continuationCommand")?.Value<string>("token");
		}
	}
}
