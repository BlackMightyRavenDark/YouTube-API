using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	internal static class YouTubeVisitorData
	{
		public const string FILE_URL = "https://www.youtube.com/sw.js_data";

		public static string GetRawData(NameValueCollection requestHeaders)
		{
			FileDownloader d = new FileDownloader() { Url = FILE_URL, Headers = requestHeaders };
			int errorCode = d.DownloadString(out string response);
			return errorCode == 200 ? response : null;
		}

		public static string ExtractVisitorDataValue(
			string rawData, string pattern = @",""([a-zA-Z0-9]*%3D%3D)""")
		{
			string value = Utils.FindRegexp(rawData, pattern);
			if (string.IsNullOrEmpty(value) || !value.Contains("%3D"))
			{
				try
				{
					int n = rawData.IndexOf("[");
					if (n > 0) { rawData = rawData.Substring(n); }
					JArray jsonArr = JArray.Parse(rawData);
					value = jsonArr[0][2][0][0][13].Value<string>();
				} catch (System.Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.Message);
					return null;
				}
			}

			return value;
		}
	}
}
