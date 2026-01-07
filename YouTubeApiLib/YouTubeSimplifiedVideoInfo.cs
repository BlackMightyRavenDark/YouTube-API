using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	public class YouTubeSimplifiedVideoInfo
	{
		public JObject Info { get; }
		public bool IsVideoInfoAvailable { get; }
		public bool IsMicroformatInfoAvailable { get; }

		public YouTubeSimplifiedVideoInfo(JObject simplifiedVideoInfo,
			bool isVideoInfoAvailable, bool isMicroformatInfoAvailable)
		{
			Info = simplifiedVideoInfo;
			IsVideoInfoAvailable = isVideoInfoAvailable;
			IsMicroformatInfoAvailable = isMicroformatInfoAvailable;
		}

		public static YouTubeSimplifiedVideoInfo MakeFromRaw(string rawJsonData, out string errorMessage)
		{
			// Искренне надеемся на то, что все необходимые данные в переданном JSON'е находятся на своих местах!
			JObject json = Utils.TryParseJson(rawJsonData, out errorMessage);
			return json != null ? new YouTubeSimplifiedVideoInfo(json, true, true) : null;
		}

		public static YouTubeSimplifiedVideoInfo MakeFromRaw(string rawJsonData)
		{
			return MakeFromRaw(rawJsonData, out _);
		}
	}
}
