using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public static class Utils
	{
		public const string YOUTUBE_URL = "https://www.youtube.com";
		private static readonly DateTime unixMinDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static string GetYouTubeVideoUrl(string videoId, int seekToSecond = 0)
		{
			string url = $"{YOUTUBE_URL}/watch?v={videoId}";
			if (seekToSecond > 0) { url += $"&t={seekToSecond}"; }
			return url;
		}

		public static string GetYouTubeVideoUrl(string videoId, TimeSpan seekTo)
		{
			int seconds = seekTo != null && seekTo > TimeSpan.Zero ? (int)seekTo.TotalSeconds : 0;
			return GetYouTubeVideoUrl(videoId, seconds);
		}

		public static YouTubeVideoDetails GetVideoDetails(string videoId, IYouTubeClient client)
		{
			YouTubeRawVideoInfoResult rawVideoInfoResult = client.GetRawVideoInfo(new YouTubeVideoId(videoId), out _);
			if (rawVideoInfoResult.ErrorCode == 200)
			{
				YouTubeVideoDetails details = rawVideoInfoResult.RawVideoInfo.VideoDetails;
				JObject jDetails = details?.Parse();
				if (jDetails != null)
				{
					return new YouTubeVideoDetails(jDetails.ToString(), client);
				}
			}

			return null;
		}

		public static YouTubeVideoDetails GetVideoDetails(string videoId)
		{
			IYouTubeClient client = YouTubeApi.GetYouTubeClient(YouTubeApi.GetDefaultYouTubeClientId());
			if (client == null) { return null; }
			YouTubeRawVideoInfoResult rawVideoInfoResult = client.GetRawVideoInfo(new YouTubeVideoId(videoId), out _);
			return rawVideoInfoResult.ErrorCode == 200 ? rawVideoInfoResult.RawVideoInfo.VideoDetails : null;
		}

		public static string FindRegexp(string inputString, string pattern)
		{
			if (!string.IsNullOrEmpty(pattern))
			{
				Regex regex = new Regex(pattern);
				MatchCollection matches = regex.Matches(inputString);
				if (matches != null && matches.Count > 0 && matches[0].Groups.Count > 1)
				{
					string value = matches[0].Groups[1].Value;
					return value;
				}
			}

			return null;
		}

		public static string FindRegexp(string inputString, IEnumerable<string> patterns)
		{
			foreach (string pattern in patterns)
			{
				string value = FindRegexp(inputString, pattern);
				if (!string.IsNullOrEmpty(value)) { return value; }
			}

			return null;
		}

		internal static List<YouTubeVideoThumbnail> GetThumbnails(
			YouTubeVideoDetails videoDetails, JObject jMicroformat, string videoId = null)
		{
			var microformatThumbnails = ExtractThumbnailsFromMicroformat(jMicroformat);
			var videoDetailsThumbnails = videoDetails != null ? ExtractThumbnails(videoDetails.Parse().Value<JObject>("thumbnail")?.Value<JArray>("thumbnails")) : null;

			if ((microformatThumbnails == null && videoDetailsThumbnails == null) ||
				(microformatThumbnails != null && microformatThumbnails.Count() <= 0 &&
				videoDetailsThumbnails != null && videoDetailsThumbnails.Count() <= 0))
			{
				return null;
			}

			List<YouTubeVideoThumbnail> thumbnails = new List<YouTubeVideoThumbnail>();

			if (microformatThumbnails != null) { thumbnails.AddRange(microformatThumbnails); }
			if (videoDetailsThumbnails != null)
			{
				foreach (YouTubeVideoThumbnail thumbnail in videoDetailsThumbnails)
				{
					if (thumbnails.All(item => !item.Url.Equals(thumbnail.Url)))
					{
						// В массиве 'videoDetailsThumbnails' содержится картинка с ошибочным размером 1920x1080.
						// Картинка с правильным размером - 1280x720 (если таковая есть) - содержится в массиве 'microformatThumbnails'.

						bool isValid = true;
						if (thumbnail.Height == 1080 && thumbnail.Url.Contains("maxresdefault") &&
							thumbnails.Any(item => item.FileName.Contains("maxresdefault")))
						{
							if (thumbnail.Url.Contains("webp"))
							{
								thumbnails.Add(new YouTubeVideoThumbnail(1280, 720, thumbnail.FileName, thumbnail.Url));
								break;
							}

							isValid = false;
						}

						if (isValid) { thumbnails.Add(thumbnail); }
					}
				}
			}

			if (thumbnails.Count > 0)
			{
				thumbnails.Sort((x, y) => x.Height > y.Height ? -1 : 1);
				if (!string.IsNullOrEmpty(thumbnails[0].FileName) && thumbnails[0].FileName.EndsWith(".webp"))
				{
					string url = thumbnails[0].Url.Replace("vi_webp", "vi").Replace(".webp", ".jpg");
					string fileName = ExtractFileNameFromThumbnailUrl(url, @"vi/.{11}/([^\&\?]*)");
					thumbnails.Insert(0, new YouTubeVideoThumbnail(
						thumbnails[0].Width, thumbnails[0].Height, fileName, url));
				}
			}

			if (!string.IsNullOrEmpty(videoId) && !string.IsNullOrWhiteSpace(videoId))
			{
				// Добавляем в список стандартные ссылки (на всякий случай).
				// Однако, для некоторых видео они могут не работать!

				YouTubeVideoThumbnail[] standardThumbnails = new YouTubeVideoThumbnail[]
				{
					new YouTubeVideoThumbnail(1280, 720, "maxresdefault.jpg", $"https://i.ytimg.com/vi/{videoId}/maxresdefault.jpg"),
					new YouTubeVideoThumbnail(640, 480, "sddefault.jpg", $"https://i.ytimg.com/vi/{videoId}/sddefault.jpg"),
					new YouTubeVideoThumbnail(480, 360, "hqdefault.jpg", $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg"),
					new YouTubeVideoThumbnail(320, 180, "mqdefault.jpg", $"https://i.ytimg.com/vi/{videoId}/mqdefault.jpg"),
					new YouTubeVideoThumbnail(120, 90, "default.jpg", $"https://i.ytimg.com/vi/{videoId}/default.jpg")
				};

				foreach (YouTubeVideoThumbnail thumbnail in standardThumbnails)
				{
					if (thumbnails.All(item => !item.Url.Contains(thumbnail.Url)))
					{
						thumbnails.Add(thumbnail);
					}
				}
			}

			if (thumbnails.Count > 1 && thumbnails[0].Url.Contains("?"))
			{
				for (int i = 0; i < thumbnails.Count; ++i)
				{
					// Избавляемся от пост-обработки картинки максимального качества.
					if ((thumbnails[i].Height == 720 || thumbnails[i].Height == 1080) && thumbnails[i].Url.Contains("?"))
					{
						string url = thumbnails[i].Url.Split('?')[0];
						thumbnails.Insert(0, new YouTubeVideoThumbnail(1280, 720, thumbnails[i].FileName, url));
						break;
					}
				}
			}

			return thumbnails;
		}

		private static IEnumerable<YouTubeVideoThumbnail> ExtractThumbnailsFromMicroformat(JObject jMicroformat)
		{
			if (jMicroformat != null)
			{
				JObject jMicroformatRenderer = jMicroformat.Value<JObject>("playerMicroformatRenderer");
				return ExtractThumbnailsFromMicroformatRenderer(jMicroformatRenderer);
			}
			return null;
		}

		private static IEnumerable<YouTubeVideoThumbnail> ExtractThumbnailsFromMicroformatRenderer(JObject jMicroformatRenderer)
		{
			JArray jaThumbnails = jMicroformatRenderer?.Value<JObject>("thumbnail")?.Value<JArray>("thumbnails");
			return ExtractThumbnails(jaThumbnails);
		}

		private static IEnumerable<YouTubeVideoThumbnail> ExtractThumbnails(JArray jsonArray)
		{
			if (jsonArray != null)
			{
				foreach (JObject jThumbnail in jsonArray.Cast<JObject>())
				{
					string url = jThumbnail.Value<string>("url");
					if (!string.IsNullOrEmpty(url) && !string.IsNullOrWhiteSpace(url))
					{
						ushort width = jThumbnail.Value<ushort>("width");
						ushort height = jThumbnail.Value<ushort>("height");
						string fileName = jThumbnail.Value<string>("file_name");
						if (string.IsNullOrEmpty(fileName) || string.IsNullOrWhiteSpace(fileName))
						{
							fileName = ExtractFileNameFromThumbnailUrl(url);
						}
						yield return new YouTubeVideoThumbnail(width, height, fileName, url);
					}
				}
			}
		}

		internal static string ExtractFileNameFromThumbnailUrl(string url, string expression = @"\w/.{11}/([^\&\?]*)")
		{
			string fileName = FindRegexp(url, expression);
			return !string.IsNullOrEmpty(fileName) && !string.IsNullOrWhiteSpace(fileName) ? fileName : "unnamed.jpg";
		}

		internal static JArray ThumbnailsToJson(IEnumerable<YouTubeVideoThumbnail> videoThumbnails)
		{
			if (videoThumbnails == null)
			{
				return null;
			}

			JArray jsonArr = new JArray();
			foreach (YouTubeVideoThumbnail thumbnail in videoThumbnails)
			{
				jsonArr.Add(thumbnail.ToJson());
			}

			return jsonArr;
		}

		public static int YouTubeHttpPost(string url, byte[] body,
			WebHeaderCollection headers, CookieContainer cookies, IWebProxy proxy,
			int timeout, out string responseString)
		{
			if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url) || body == null || body.Length == 0)
			{
				responseString = null;
				return 400;
			}

			try
			{
				if (headers == null) { headers = new WebHeaderCollection(); }

				headers["Content-Type"] = "application/json";
				headers["Content-Length"] = body.Length.ToString();

				using (HttpRequestResult requestResult = HttpRequestSender.Send("POST", url, body, headers, cookies, proxy, timeout))
				{
					responseString = requestResult.HasErrorMessage ? requestResult.ErrorMessage : null;
					int errorCode = requestResult.ErrorCode == 200 ? requestResult.GetContent(out responseString) : requestResult.ErrorCode;
					return errorCode == 200 ? requestResult.WebContent.ContentToString(out responseString) : errorCode;
				}
			}
			catch (Exception ex)
			{
				responseString = ex.Message;
				return ex.HResult;
			}
		}

		public static int YouTubeHttpPost(string url, byte[] body,
			WebHeaderCollection headers, CookieContainer cookies, IWebProxy proxy,
			out string responseString)
		{
			return YouTubeHttpPost(url, body, headers, cookies, proxy, 10000, out responseString);
		}

		public static int YouTubeHttpPost(string url, string body,
			WebHeaderCollection headers, CookieContainer cookies, IWebProxy proxy,
			int timeout, out string responseString)
		{
			byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
			return YouTubeHttpPost(url, bodyBytes, headers, cookies, proxy, timeout, out responseString);
		}

		public static int YouTubeHttpPost(string url, string body,
			WebHeaderCollection headers, CookieContainer cookies, IWebProxy proxy,
			out string responseString)
		{
			return YouTubeHttpPost(url, body, headers, cookies, proxy, 10000, out responseString);
		}

		public static int YouTubeHttpPost(string url, string body, Encoding bodyEncoding,
			WebHeaderCollection headers, out string responseString)
		{
			byte[] bodyBytes = bodyEncoding.GetBytes(body);
			return YouTubeHttpPost(url, bodyBytes, headers, null, null, out responseString);
		}

		public static int YouTubeHttpPost(string url, string body,
			WebHeaderCollection headers, out string responseString)
		{
			return YouTubeHttpPost(url, body, Encoding.UTF8, headers, out responseString);
		}

		public static int YouTubeHttpPost(string url, string body, string userAgent, out string responseString)
		{
			WebHeaderCollection headers = new WebHeaderCollection()
			{
				{ "Host", "www.youtube.com" },
				{ "User-Agent", userAgent },
				{ "Accept", "*/*" },
				{ "Accept-Encoding", "gzip" }
			};
			byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
			return YouTubeHttpPost(url, bodyBytes, headers, null, null, out responseString);
		}

		public static int YouTubeHttpPost(string url, string body, out string responseString)
		{
			const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:132.0) Gecko/20100101 Firefox/132.0";
			return YouTubeHttpPost(url, body, userAgent, out responseString);
		}

		public static YouTubeRawVideoInfoResult ExtractRawVideoInfoFromWebPage(YouTubeVideoWebPage webPage)
		{
			if (webPage != null)
			{
				string rawVideoInfo = ExtractRawVideoInfoFromWebPageCode(webPage.WebPageCode);
				if (!string.IsNullOrEmpty(rawVideoInfo) && !string.IsNullOrWhiteSpace(rawVideoInfo))
				{
					IYouTubeClient client = new YouTubeClientWebPage();
					YouTubeMediaTrackUrlDecryptionData urlDecryptionData = new YouTubeMediaTrackUrlDecryptionData(webPage);
					YouTubeRawVideoInfo youTubeRawVideoInfo = new YouTubeRawVideoInfo(rawVideoInfo, client, urlDecryptionData, DateTime.UtcNow);
					return new YouTubeRawVideoInfoResult(youTubeRawVideoInfo, 200);
				}
				else
				{
					return new YouTubeRawVideoInfoResult(null, 400);
				}
			}
			return new YouTubeRawVideoInfoResult(null, 404);
		}

		internal static string ExtractRawVideoInfoFromWebPageCode(string webPageCode)
		{
			//TODO: Заменить этот говнокод на что-то более получше!
			try
			{
				int n = webPageCode.IndexOf("var ytInitialPlayerResponse");
				if (n > 0)
				{
					int n2 = webPageCode.IndexOf("}};var meta =");
					if (n2 > 0)
					{
						return webPageCode.Substring(n + 30, n2 - n - 28);
					}

					n2 = webPageCode.IndexOf("};\nvar meta =");
					if (n2 > 0)
					{
						return webPageCode.Substring(n + 29, n2 - n - 28);
					}

					n2 = webPageCode.IndexOf("}};var head =");
					if (n2 > 0)
					{
						return webPageCode.Substring(n + 30, n2 - n - 28);
					}

					n2 = webPageCode.IndexOf("};\nvar head =");
					if (n2 > 0)
					{
						return webPageCode.Substring(n + 29, n2 - n - 28);
					}

					n2 = webPageCode.IndexOf(";</script><div");
					if (n2 > 0)
					{
						return webPageCode.Substring(n + 30, n2 - n - 30);
					}
				}
			}
#if DEBUG
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
#else
			catch { }
#endif
			return null;
		}

		public static YouTubeConfig ExtractYouTubeConfigFromWebPageCode(
			string webPageCode, string videoId, string pattern = @"ytcfg\.set\(({\s*"".*""}+)\);.*window\.ytcfg")
		{
			Regex regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled);
			MatchCollection matches = regex.Matches(webPageCode);
			if (matches.Count > 0 && matches[0].Groups.Count > 1)
			{
				string t = matches[0].Groups[1].Value;
				return new YouTubeConfig(videoId, t);
			}

			return null;
		}

		public static YouTubeInitialData ExtractYouTubeInitialDataFromWebPageCode(string webPageCode,
			string pattern = @"var ytInitialData =\s*(.*}}});</script")
		{
			Regex regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled);
			MatchCollection matches = regex.Matches(webPageCode);
			if (matches.Count > 0 && matches[0].Groups.Count > 1)
			{
				return new YouTubeInitialData(matches[0].Groups[1].Value);
			}

			return null;
		}

		public static YouTubeVideoId ExtractVideoIdFromUrl(string url)
		{
			if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
			{
				return null;
			}

			Uri uri;
			try
			{
				uri = new Uri(url);
			}
#if DEBUG
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
#else
			catch
			{
#endif
				//подразумевается, что юзер ввёл ID видео, а не ссылку.
				return new YouTubeVideoId(url);
			}

			if (!uri.Host.EndsWith("youtube.com", StringComparison.OrdinalIgnoreCase) &&
				!uri.Host.EndsWith("youtu.be", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			if (string.IsNullOrEmpty(uri.Query))
			{
				if (!string.IsNullOrEmpty(uri.AbsolutePath) && !string.IsNullOrWhiteSpace(uri.AbsolutePath))
				{
					string videoId = uri.AbsolutePath;
					if (videoId.StartsWith("/shorts/", StringComparison.OrdinalIgnoreCase))
					{
						videoId = videoId.Substring(8);
					}
					else if (videoId.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase))
					{
						videoId = videoId.Substring(7);
					}

					if (videoId.StartsWith("/"))
					{
						videoId = videoId.Remove(0, 1);
					}

					if (!string.IsNullOrEmpty(videoId) && videoId.Length > 11)
					{
						videoId = videoId.Substring(0, 11);
					}

					return new YouTubeVideoId(videoId);
				}
				return null;
			}

			Dictionary<string, string> dict = SplitUrlQueryToDictionary(uri.Query);
			if (dict == null || !dict.ContainsKey("v"))
			{
				return null;
			}

			return new YouTubeVideoId(dict["v"]);
		}

		public static string GetYouTubeVisitorData(WebHeaderCollection requestHeaders = null)
		{
			string rawData = YouTubeVisitorData.GetRawData(requestHeaders);
			return string.IsNullOrEmpty(rawData) ? null : YouTubeVisitorData.ExtractVisitorDataValue(rawData);
		}

		public static string GetYouTubeVisitorData(string userAgent)
		{
			WebHeaderCollection headers = null;
			if (!string.IsNullOrEmpty(userAgent))
			{
				headers = new WebHeaderCollection()
				{
					{ "User-Agent", userAgent }
				};
			}

			return GetYouTubeVisitorData(headers);
		}

		public static int DownloadString(string url, out string response, FileDownloader downloader = null)
		{
			if (downloader == null) { downloader = new FileDownloader(); }
			downloader.Url = url;
			return downloader.DownloadString(out response);
		}

		public static Dictionary<string, string> SplitUrlQueryToDictionary(string urlQuery)
		{
			if (string.IsNullOrEmpty(urlQuery) || string.IsNullOrWhiteSpace(urlQuery))
			{
				return null;
			}
			if (urlQuery[0] == '?')
			{
				urlQuery = urlQuery.Remove(0, 1);
			}
			return SplitStringToKeyValues(urlQuery, '&', '=');
		}

		public static Dictionary<string, string> SplitStringToKeyValues(
			string inputString, char keySeparator, char valueSeparator)
		{
			if (string.IsNullOrEmpty(inputString) || string.IsNullOrWhiteSpace(inputString))
			{
				return null;
			}
			string[] keyValues = inputString.Split(keySeparator);
			Dictionary<string, string> dict = new Dictionary<string, string>();
			for (int i = 0; i < keyValues.Length; i++)
			{
				if (!string.IsNullOrEmpty(keyValues[i]) && !string.IsNullOrWhiteSpace(keyValues[i]))
				{
					string[] t = keyValues[i].Split(new char[] { valueSeparator }, 2, StringSplitOptions.RemoveEmptyEntries);
					if (t.Length > 1) { dict.Add(t[0], t[1]); }
				}
			}
			return dict;
		}

		public static bool ParseMicroformatDate(string dateString, out DateTime dateTime)
		{
			if (DateTime.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ssZ",
				null, DateTimeStyles.AdjustToUniversal, out dateTime))
			{
				return true;
			}

			if (DateTime.TryParseExact(dateString, "yyyy-MM-dd",
				null, DateTimeStyles.AssumeLocal, out dateTime))
			{
				return true;
			}

			dateTime = DateTime.MaxValue;
			return false;
		}

		internal static TimeSpan DurationFromString(string lengthTime, out string errorMessage)
		{
			try
			{
				errorMessage = null;

				// The lengthTime must be in 'H:MM:SS' or 'M:SS' or 'SS' format, with or without leading zeros.
				string[] splitted = lengthTime?.Split(':');
				if (splitted != null)
				{
					switch (splitted.Length)
					{
						case 1:
							return TimeSpan.FromSeconds(int.Parse(splitted[0]));

						case 2:
							return TimeSpan.FromSeconds(int.Parse(splitted[0]) * 60 + int.Parse(splitted[1]));

						case 3:
							return TimeSpan.FromSeconds(int.Parse(splitted[0]) * 3600 + int.Parse(splitted[1]) * 60 + int.Parse(splitted[2]));
					}
				}
			}
			catch (Exception ex)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine(ex.Message);
#endif
				errorMessage = ex.Message;
			}

			return TimeSpan.Zero;
		}

		public static void ExtractDatesFromMicroformat(
			JObject jSimplifiedVideoInfo, out DateTime uploadDate, out DateTime publishDate)
		{
			if (jSimplifiedVideoInfo.ContainsKey("date_publish_unix"))
			{
				long unixMilliseconds = jSimplifiedVideoInfo.Value<long>("date_publish_unix");
				publishDate = UnixTimeMillisecondsToDateTime(unixMilliseconds);
			}
			else
			{
				string published = jSimplifiedVideoInfo.Value<string>("date_publish");
				if (!DateTime.TryParseExact(published, "yyyy-MM-ddTHH:mm:ssZ",
					null, DateTimeStyles.AdjustToUniversal, out publishDate))
				{
					string startTimestamp = jSimplifiedVideoInfo.Value<string>("start_timestamp");
					if (!DateTime.TryParseExact(startTimestamp, "yyyy-MM-ddTHH:mm:ssZ",
						null, DateTimeStyles.AdjustToUniversal, out publishDate))
					{
						if (!DateTime.TryParseExact(published, "yyyy-MM-dd",
							null, DateTimeStyles.AssumeLocal, out publishDate))
						{
							publishDate = DateTime.MaxValue;
						}
					}
				}
			}

			if (jSimplifiedVideoInfo.ContainsKey("date_upload_unix"))
			{
				long unixMilliseconds = jSimplifiedVideoInfo.Value<long>("date_upload_unix");
				uploadDate = UnixTimeMillisecondsToDateTime(unixMilliseconds);
			}
			else
			{
				string uploaded = jSimplifiedVideoInfo.Value<string>("date_upload");
				ParseMicroformatDate(uploaded, out uploadDate);
			}
		}

		public static string ToUtcString(this DateTime dateTime)
		{
			DateTime dt = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
			return $"{dt:yyyy-MM-dd\"T\"HH:mm:ss}Z";
		}

		internal static string DateTimeStringToUtcString(string s, out DateTime dateTime)
		{
			if (!DateTime.TryParseExact(s, "MM/dd/yyyy HH:mm:ss",
				null, DateTimeStyles.AssumeLocal, out dateTime))
			{
				return s;
			}

			return dateTime.ToUtcString();
		}

		internal static long ToUnixTimeMilliseconds(this DateTime dateTime)
		{
			DateTime gmt = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
			DateTimeOffset offset = new DateTimeOffset(gmt);
			return offset.ToUnixTimeMilliseconds();
		}

		internal static long ToUnixTimeTicks(this DateTime dateTime)
		{
			DateTime gmt = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
			return (gmt - unixMinDateTime).Ticks;
		}

		internal static DateTime UnixTimeMillisecondsToDateTime(long unixMillliseconds)
		{
			return new DateTime(unixMinDateTime.Ticks + unixMillliseconds * 10000, DateTimeKind.Utc);
		}

		internal static DateTime UnixTimeTicksToDateTime(long unixTicks)
		{
			return new DateTime(unixMinDateTime.Ticks + unixTicks, DateTimeKind.Utc);
		}

		internal static string FormatVideoDuration(TimeSpan duration)
		{
			if (duration >= TimeSpan.FromHours(1))
			{
				return duration.ToString("h':'mm':'ss");
			}
			else if (duration >= TimeSpan.FromMinutes(1))
			{
				return duration.ToString("m':'ss");
			}
			else if (duration > TimeSpan.Zero)
			{
				return duration.ToString("\"0:\"ss");
			}
			else
			{
				return "0:00:00";
			}
		}

		public static string UrlDecode(string inputString)
		{
			return HttpUtility.UrlDecode(inputString);
		}

		public static string UrlEncode(string inputString)
		{
			return HttpUtility.UrlEncode(inputString);
		}

		internal static JObject TryParseJson(string jsonString, out string errorText)
		{
			try
			{
				errorText = null;
				return JObject.Parse(jsonString);
			}
			catch (Exception ex)
			{
				errorText = ex.Message;
				return null;
			}
		}

		internal static JObject TryParseJson(string jsonString)
		{
			return TryParseJson(jsonString, out _);
		}

		internal static JArray TryParseJsonArray(string jsonArrayString, out string errorText)
		{
			try
			{
				errorText = null;
				return JArray.Parse(jsonArrayString);
			}
			catch (Exception ex)
			{
				errorText = ex.Message;
				return null;
			}
		}

		internal static JArray TryParseJsonArray(string jsonArrayString)
		{
			return TryParseJsonArray(jsonArrayString, out _);
		}
	}
}
