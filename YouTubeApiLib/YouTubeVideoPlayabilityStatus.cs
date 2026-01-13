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
		public bool IsPlayableInEmbed { get; }
		public bool IsPrivate { get; }
		public bool IsAdult { get; }
		public bool IsLoginRequired { get; }
		public bool IsBotWarning { get; }
		public int ErrorCode { get; }
		public string RawInfo { get; }

		public YouTubeVideoPlayabilityStatus(string status, string reason, string reasonDetails,
			bool isPlayableInEmbed,
			string thumbnailUrl, int errorCode, string rawInfo)
		{
			Status = status;
			Reason = reason;
			ReasonDetails = reasonDetails;
			IsPlayableInEmbed = isPlayableInEmbed;
			ThumbnailUrl = thumbnailUrl;
			ErrorCode = errorCode;
			RawInfo = rawInfo;
			IsPlayable = status == "OK";
			IsPrivate = GetIsPrivate();
			IsAdult = GetIsAdult();
			IsLoginRequired = GetIsLoginRequired();
			IsBotWarning = GetIsBotWarning();
		}

		public YouTubeVideoPlayabilityStatus(string status, string reason, string reasonDetails,
			bool isPlayable, bool isPlayableInEmbed, bool isPrivate, bool isAdult,
			bool isLoginRequired, bool isBotWarning, string thumbnailUrl, string rawInfo = null)
		{
			Status = status;
			Reason = reason;
			ReasonDetails = reasonDetails;
			IsPlayable = isPlayable;
			IsPlayableInEmbed = isPlayableInEmbed;
			IsPrivate = isPrivate;
			IsAdult = isAdult;
			IsLoginRequired = isLoginRequired;
			IsBotWarning = isBotWarning;
			ThumbnailUrl = thumbnailUrl;
			RawInfo = rawInfo;
			ErrorCode = string.Compare(status, "OK", true) == 0 ? 200 : 403;
		}

		public YouTubeVideoPlayabilityStatus(int errorCode)
			: this(null, null, null, false, null, errorCode, null) { }

		public static YouTubeVideoPlayabilityStatus Parse(JObject jPlayabilityStatus)
		{
			string status = jPlayabilityStatus.Value<string>("status");
			string reason = null;
			string reasonDetails = null;
			string thumbnailUrl = null;
			bool isPlayableInEmbed = jPlayabilityStatus.Value<bool>("playableInEmbed");

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
			return new YouTubeVideoPlayabilityStatus(status, reason, reasonDetails, isPlayableInEmbed,
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

		private bool GetIsBotWarning()
		{
			if (!string.IsNullOrEmpty(Reason) && !string.IsNullOrWhiteSpace(Reason))
			{
				string lowercased = Reason.ToLower();
				return lowercased.Contains("not a bot") || lowercased.Contains("вы не бот");
			}

			return false;
		}

		public JObject ToJson()
		{
			JObject json = new JObject()
			{
				["status"] = string.IsNullOrWhiteSpace(Status) ? string.Empty : Status,
				["reason"] = string.IsNullOrWhiteSpace(Reason) ? string.Empty : Reason,
				["reason_details"] = string.IsNullOrWhiteSpace(ReasonDetails) ? string.Empty : ReasonDetails,
				["is_playable"] = IsPlayable,
				["is_playable_in_embed"] = IsPlayableInEmbed,
				["is_private"] = IsPrivate,
				["is_adult"] = IsAdult,
				["is_login_required"] = IsLoginRequired,
				["is_bot_warning"] = IsBotWarning
			};
			if (!string.IsNullOrEmpty(ThumbnailUrl))
			{
				json["thumbnail_url"] = ThumbnailUrl;
			}
			return json;
		}

		public static YouTubeVideoPlayabilityStatus FromJson(JObject json)
		{
			return new YouTubeVideoPlayabilityStatus(
				json.Value<string>("status"),
				json.Value<string>("reason"),
				json.Value<string>("reason_details"),
				json.Value<bool>("is_playable"),
				json.Value<bool>("is_playable_in_embed"),
				json.Value<bool>("is_private"),
				json.Value<bool>("is_adult"),
				json.Value<bool>("is_login_required"),
				json.Value<bool>("is_bot_warning"),
				json.Value<string>("thumbnail_url"));
		}
	}
}
