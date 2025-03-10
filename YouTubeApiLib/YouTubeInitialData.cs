using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	public class YouTubeInitialData
	{
		public JObject Data { get; }
		public string RawData { get; }
		public bool DataWasReceivedWithContinuationToken { get; }

		public YouTubeInitialData(string rawData, bool dataWasReceivedWithContinuationToken = false)
		{
			RawData = rawData;
			Data = Utils.TryParseJson(rawData);
			DataWasReceivedWithContinuationToken = dataWasReceivedWithContinuationToken;
		}

		public static YouTubeInitialData ExtractFromWebPageCode(string webPageCode,
			string pattern = @"var ytInitialData =\s*(.*}}});</script")
		{
			return Utils.ExtractYouTubeInitialDataFromWebPageCode(webPageCode, pattern);
		}

		public YouTubeVideoIdPageResult ToVideoIdPage()
		{
			YouTubeVideoIdPage videoIdPage = new YouTubeVideoIdPage(RawData, DataWasReceivedWithContinuationToken);
			int errorCode = videoIdPage.Parse() > 0 ? 200 : 404;
			return new YouTubeVideoIdPageResult(videoIdPage, errorCode);
		}
	}
}
