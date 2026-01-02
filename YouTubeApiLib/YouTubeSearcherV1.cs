using Newtonsoft.Json.Linq;
using static YouTubeApiLib.Utils;
using static YouTubeApiLib.YouTubeApiV1;

namespace YouTubeApiLib
{
	internal class YouTubeSearcherV1 : IYouTubeSearcher
	{
		public string SearchQuery { get; }
		public string ContinuationToken { get; }
		public YouTubeApiV1SearchResultFilter SearchFilter { get; }

		public YouTubeSearcherV1(string searchQuery, string continuationToken,
			YouTubeApiV1SearchResultFilter searchFilter)
		{
			SearchQuery = searchQuery;
			ContinuationToken = continuationToken;
			SearchFilter = searchFilter;
		}

		public object Search()
		{
			string url = GetSearchRequestUrl();
			JObject body = GenerateSearchQueryRequestBody(SearchQuery, ContinuationToken, SearchFilter);
			int errorCode = YouTubeHttpPost(url, body.ToString(), out string response);
			if (errorCode == 200)
			{
				bool isContinuationTokenUsed = !string.IsNullOrEmpty(ContinuationToken) && !string.IsNullOrWhiteSpace(ContinuationToken);
				return new YouTubeApiV1SearchResult(response, SearchFilter, SearchQuery, isContinuationTokenUsed, errorCode);
			}

			return new YouTubeApiV1SearchResult(null, SearchFilter, SearchQuery, false, errorCode);
		}
	}
}
