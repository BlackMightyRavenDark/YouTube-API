
namespace YouTubeApiLib
{
	public class YouTubeApiV1SearchResults
	{
		public string RawData { get; }
		public YouTubeApiV1SearchResultFilter UsedFilter { get; }
		public string SearchQuery { get; }
		public bool IsContinuationItem { get; }
		public int ErrorCode { get; }

		public YouTubeApiV1SearchResults(
			string rawData,
			YouTubeApiV1SearchResultFilter usedFilter,
			string searchQuery,
			bool isContinuationItem,
			int errorCode)
		{
			RawData = rawData;
			UsedFilter = usedFilter;
			SearchQuery = searchQuery;
			IsContinuationItem = isContinuationItem;
			ErrorCode = errorCode;
		}
	}
}
