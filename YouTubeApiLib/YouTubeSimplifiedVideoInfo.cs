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
	}
}
