using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace YouTubeApiLib
{
	internal interface IYouTubeChannelTabPageRequestParser
	{
		JArray FindGridItems();
		IEnumerable<YouTubeVideoThumbnail> ExtractThumbnails(JObject jVideoItem);
		List<YouTubeVideoLite> Parse(out string continuationToken);
	}
}
