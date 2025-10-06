using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	public class YouTubeVideoPlayabilityStatus
	{
		public string Status { get; }
		public string Reason { get; }
		public string ReasonDetails { get; }
		public string ThumbnailUrl { get; }
		public bool IsPlayable { get; }
		public bool IsPrivate { get; }
		public bool IsAdult { get; }
		public bool IsLoginRequired { get; }
		public int ErrorCode { get; }
		public string RawInfo { get; }

		public YouTubeVideoPlayabilityStatus(string status, string reason, string reasonDetails,
			string thumbnailUrl, int errorCode, string rawInfo)
		{
			Status = status;
			Reason = reason;
			ReasonDetails = reasonDetails;
			ThumbnailUrl = thumbnailUrl;
			ErrorCode = errorCode;
			RawInfo = rawInfo;
			IsPlayable = status == "OK";
			IsPrivate = GetIsPrivate();
			IsAdult = GetIsAdult();
			IsLoginRequired = GetIsLoginRequired();
		}

		public YouTubeVideoPlayabilityStatus(int errorCode)
			: this(null, null, null, null, errorCode, null) { }

		public static YouTubeVideoPlayabilityStatus Parse(JObject jPlayabilityStatus)
		{
			string status = jPlayabilityStatus.Value<string>("status");
			string reason = null;
			string reasonDetails = null;

			string thumbnailUrl = null;
			JObject jErrorScreen = jPlayabilityStatus.Value<JObject>("errorScreen");
			if (jErrorScreen != null)
			{
				JObject jPlayerErrorMessageRenderer = jErrorScreen.Value<JObject>("playerErrorMessageRenderer");
				if (jPlayerErrorMessageRenderer != null)
				{
					reason = jPlayerErrorMessageRenderer.Value<JObject>("reason")?.Value<string>("simpleText");
					reasonDetails = jPlayerErrorMessageRenderer.Value<JObject>("subreason")?.Value<string>("simpleText");

					JArray jaThumbnails = jPlayerErrorMessageRenderer.Value<JObject>("thumbnail")?.Value<JArray>("thumbnails");
					if (jaThumbnails != null && jaThumbnails.Count > 0)
					{
						string urlPartial = (jaThumbnails[0] as JObject)?.Value<string>("url");
						if (!string.IsNullOrEmpty(urlPartial) && !string.IsNullOrWhiteSpace(urlPartial))
						{
							thumbnailUrl = $"https:{urlPartial}";
						}
					}
				}
			}

			if (string.IsNullOrEmpty(reason) || string.IsNullOrWhiteSpace(reason))
			{
				reason = jPlayabilityStatus.Value<string>("reason");
			}
			if (string.IsNullOrEmpty(reasonDetails) || string.IsNullOrWhiteSpace(reasonDetails))
			{
				reasonDetails = jPlayabilityStatus.Value<string>("reasonTitle");
			}

			int errorCode = status == "OK" ? 200 : 403;
			return new YouTubeVideoPlayabilityStatus(status, reason, reasonDetails,
				thumbnailUrl, errorCode, jPlayabilityStatus.ToString());
		}

		private bool GetIsPrivate()
		{
			return !string.IsNullOrEmpty(Reason) && !string.IsNullOrWhiteSpace(Reason) &&
				Reason.ToLower().Contains("private");
		}

		private bool GetIsAdult()
		{
			return !string.IsNullOrEmpty(Reason) && !string.IsNullOrWhiteSpace(Reason) &&
				Reason.ToLower().Contains("age");
		}

		private bool GetIsLoginRequired()
		{
			return Status == "LOGIN_REQUIRED";
		}
	}
}
