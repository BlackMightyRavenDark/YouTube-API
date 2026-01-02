#if DEBUG
using System;
#endif
using System.Collections.Generic;

namespace YouTubeApiLib
{
	public class YouTubeHlsManifestParser
	{
		public string HlsManifest { get; }

		public YouTubeHlsManifestParser(string hlsManifest)
		{
			HlsManifest = hlsManifest;
		}

		public LinkedList<YouTubeBroadcast> Parse()
		{
			if (string.IsNullOrEmpty(HlsManifest) || string.IsNullOrWhiteSpace(HlsManifest))
			{
				return null;
			}

			string[] strings = HlsManifest.Split('\n');
			if (strings == null || strings.Length == 0)
			{
				return null;
			}

			LinkedList<YouTubeBroadcast> resList = new LinkedList<YouTubeBroadcast>();

			int startIndex;

			for (startIndex = 0; startIndex < strings.Length; ++startIndex)
			{
				if (strings[startIndex].StartsWith("#EXT-X-STREAM-INF"))
				{
					break;
				}
			}

			for (; startIndex < strings.Length - 1; startIndex += 2)
			{
				string t = GetParameterValue(strings[startIndex], "RESOLUTION");
				if (string.IsNullOrEmpty(t) || string.IsNullOrWhiteSpace(t))
				{
					continue;
				}
				string[] widthHeight = t.Split('x');
				if (widthHeight == null || widthHeight.Length < 2)
				{
					continue;
				}

				if (!int.TryParse(widthHeight[0], out int width))
				{
					width = 0;
				}
				if (!int.TryParse(widthHeight[1], out int height))
				{
					height = 0;
				}
				if (!int.TryParse(GetParameterValue(strings[startIndex], "BANDWIDTH"), out int bandwidth))
				{
					bandwidth = 0;
				}
				string codecs = GetParameterValue(strings[startIndex], "CODECS");
				if (!int.TryParse(GetParameterValue(strings[startIndex], "FRAME-RATE"), out int frameRate))
				{
					frameRate = 0;
				}
				string url = strings[startIndex + 1];
				int formatId = ExtractFormatIdFromUrl(url);
				YouTubeMediaTrackUrl trackUrl = new YouTubeMediaTrackUrl(url, null);

				YouTubeBroadcast broadcast = new YouTubeBroadcast(formatId, width, height, frameRate, bandwidth, codecs, trackUrl);
				resList.AddLast(broadcast);
			}

			return resList;
		}

		private static string GetParameterValue(string info, string parameterName)
		{
			try
			{
				if (parameterName.Equals("CODECS"))
				{
					int j = info.IndexOf(parameterName) + 8;
					string s = info.Substring(j);
					string[] s2 = s.Split('"');
					return s2[0];
				}
				int n = info.IndexOf(parameterName);
				if (n <= 0)
				{
					return null;
				}
				string t = info.Substring(n);
				n = t.IndexOf("=");
				t = t.Substring(n + 1);
				string[] t2 = t.Split(',');
				return t2[0];
			}
#if DEBUG
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
#else
			catch { }
#endif
			return null;
		}

		private static int ExtractFormatIdFromUrl(string playlistUrl)
		{
			try
			{
				int n = playlistUrl.IndexOf("/itag/");
				if (n <= 0)
				{
					return 0;
				}
				string t = playlistUrl.Substring(n + 6);
				n = t.IndexOf("/");
				t = t.Substring(0, n);
				if (!int.TryParse(t, out int res)) { res = 0; }
				return res;
			}
#if DEBUG
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
#else
			catch { }
#endif
			return 0;
		}
	}
}
