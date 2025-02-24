using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	public class YouTubeInitialData
	{
		public JObject Data { get; }
		public string RawData { get; }

		public YouTubeInitialData(string rawData)
		{
			RawData = rawData;
			Data = Utils.TryParseJson(rawData);
		}
	}
}
