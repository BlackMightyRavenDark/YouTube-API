using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
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

		internal static YouTubeVideo GetVideoFromWebPage(YouTubeVideoWebPage webPage)
		{
			YouTubeRawVideoInfoResult rawVideoInfoResult = ExtractRawVideoInfoFromWebPage(webPage);
			if (rawVideoInfoResult.ErrorCode == 200)
			{
				YouTubeSimplifiedVideoInfoResult simplifiedVideoInfoResult = rawVideoInfoResult.RawVideoInfo.Simplify();
				if (simplifiedVideoInfoResult.ErrorCode == 200)
				{
					return MakeYouTubeVideo(rawVideoInfoResult.RawVideoInfo, simplifiedVideoInfoResult.SimplifiedVideoInfo);
				}
			}
			return null;
		}

		internal static YouTubeRawVideoInfoResult GetRawVideoInfo(
			string videoId, IYouTubeClient client)
		{
			int errorCode = client.GetRawVideoInfo(videoId, out YouTubeRawVideoInfo rawVideoInfo, out _);
			return new YouTubeRawVideoInfoResult(rawVideoInfo, errorCode);
		}

		internal static YouTubeSimplifiedVideoInfoResult GetSimplifiedVideoInfo(string videoId, IYouTubeClient client)
		{
			YouTubeRawVideoInfoResult rawVideoInfoResult = client.GetRawVideoInfo(new YouTubeVideoId(videoId), out _);
			if (rawVideoInfoResult.ErrorCode == 200)
			{
				return rawVideoInfoResult.RawVideoInfo.Simplify();
			}
			return new YouTubeSimplifiedVideoInfoResult(null, rawVideoInfoResult.ErrorCode);
		}

		internal static YouTubeSimplifiedVideoInfoResult SimplifyRawVideoInfo(
			YouTubeVideoDetails videoDetails, JObject microformat,
			YouTubeStreamingData streamingData = null)
		{
			JObject jVideoDetails = videoDetails?.Parse();
			JObject jMicroformatRenderer = microformat?.Value<JObject>("playerMicroformatRenderer");
			JObject jSimplifiedVideoInfo = new JObject();

			string videoId;
			if (jVideoDetails != null)
			{
				jSimplifiedVideoInfo["title"] = jVideoDetails.Value<string>("title");
				videoId = jVideoDetails.Value<string>("videoId");
				jSimplifiedVideoInfo["id"] = videoId;
				jSimplifiedVideoInfo["url"] = GetYouTubeVideoUrl(videoId);
				if (int.TryParse(jVideoDetails.Value<string>("lengthSeconds"), out int lengthSeconds))
				{
					jSimplifiedVideoInfo["lengthSeconds"] = lengthSeconds;
				}
				jSimplifiedVideoInfo["ownerChannelTitle"] = jVideoDetails.Value<string>("author");
				jSimplifiedVideoInfo["ownerChannelId"] = jVideoDetails.Value<string>("channelId");
				if (!long.TryParse(jVideoDetails.Value<string>("viewCount"), out long viewCount))
				{
					viewCount = -1L;
				}
				jSimplifiedVideoInfo["viewCount"] = viewCount;
				jSimplifiedVideoInfo["isPrivate"] = jVideoDetails.Value<bool>("isPrivate");
				jSimplifiedVideoInfo["isLiveContent"] = jVideoDetails.Value<bool>("isLiveContent");
				jSimplifiedVideoInfo["shortDescription"] = jVideoDetails.Value<string>("shortDescription");
			}
			else
			{
				videoId = null;
			}

			if (jMicroformatRenderer != null)
			{
				JObject jDescription = jMicroformatRenderer.Value<JObject>("description");
				if (jDescription != null)
				{
					jSimplifiedVideoInfo["description"] = jDescription.Value<string>("simpleText");
				}
				bool isFamilySafe = jMicroformatRenderer.Value<bool>("isFamilySafe");
				jSimplifiedVideoInfo["isFamilySafe"] = isFamilySafe;
				bool isUnlisted = jMicroformatRenderer.Value<bool>("isUnlisted");
				jSimplifiedVideoInfo["isUnlisted"] = isUnlisted;
				bool isShort = jMicroformatRenderer.Value<bool>("isShortsEligible");
				jSimplifiedVideoInfo["isShort"] = isShort;
				jSimplifiedVideoInfo["category"] = jMicroformatRenderer.Value<string>("category");
				{
					string date = jMicroformatRenderer.Value<string>("publishDate");
					jSimplifiedVideoInfo["datePublished"] = DateTimeStringToUtcString(date);
				}
				{
					string date = jMicroformatRenderer.Value<string>("uploadDate");
					jSimplifiedVideoInfo["dateUploaded"] = DateTimeStringToUtcString(date);
				}

				JObject jLiveBroadcastDetails = jMicroformatRenderer.Value<JObject>("liveBroadcastDetails");
				if (jLiveBroadcastDetails != null)
				{
					{
						string date = jLiveBroadcastDetails.Value<string>("startTimestamp");
						jSimplifiedVideoInfo["startTimestamp"] = DateTimeStringToUtcString(date);
					}
					{
						string date = jLiveBroadcastDetails.Value<string>("endTimestamp");
						jSimplifiedVideoInfo["endTimestamp"] = DateTimeStringToUtcString(date);
					}
				}
			}

			var videoThumbnails = GetThumbnails(videoDetails, microformat, videoId).ToList();
			if (videoThumbnails.Count > 0)
			{
				jSimplifiedVideoInfo["thumbnails"] = ThumbnailsToJson(videoThumbnails);
			}

			if (streamingData != null)
			{
				JObject jStreamingData = TryParseJson(streamingData.RawData);
				if (jStreamingData != null)
				{
					jSimplifiedVideoInfo["streamingData"] = jStreamingData;
				}
			}

			YouTubeSimplifiedVideoInfo simplifiedVideoInfo = new YouTubeSimplifiedVideoInfo(
				jSimplifiedVideoInfo, jVideoDetails != null, jMicroformatRenderer != null);
			return new YouTubeSimplifiedVideoInfoResult(simplifiedVideoInfo, 200);
		}

		internal static YouTubeSimplifiedVideoInfoResult SimplifyRawVideoInfo(YouTubeRawVideoInfo rawVideoInfo,
			JObject customMicroformat)
		{
			YouTubeVideoDetails videoDetails = rawVideoInfo.VideoDetails;
			JObject jMicroformat = customMicroformat ?? rawVideoInfo.Microformat;
			YouTubeStreamingData streamingData = rawVideoInfo.StreamingData.Data;

			return SimplifyRawVideoInfo(videoDetails, jMicroformat, streamingData);
		}

		internal static YouTubeSimplifiedVideoInfoResult SimplifyRawVideoInfo(YouTubeRawVideoInfo rawVideoInfo)
		{
			return SimplifyRawVideoInfo(rawVideoInfo, null);
		}

		/// <summary>
		/// Создаёт объект класса "YouTubeVideo" из переданных аргументов.
		/// </summary>
		/// <param name="customStreamingData">
		/// Если не 'null', эти данные будут использованы вместо "rawVideoInfo.StreamingData.Data".
		/// </param>
		/// <param name="downloader">
		/// Объект скачивателя, который будет использован для получения дополнительных данных (если это необходимо).
		/// Если передать 'null', будет автоматически создан новый объект скачивателя с настройками по-умолчанию.
		/// </param>
		public static YouTubeVideo MakeYouTubeVideo(YouTubeRawVideoInfo rawVideoInfo,
			YouTubeSimplifiedVideoInfo simplifiedVideoInfo, YouTubeStreamingData customStreamingData,
			FileDownloader downloader = null)
		{
			string videoTitle = null;
			string videoId = null;
			TimeSpan videoLength = TimeSpan.Zero;
			string ownerChannelTitle = null;
			string ownerChannelId = null;
			int viewCount = 0;
			bool isPrivate = false;
			bool isLiveContent = false;
			string shortDescription = null;

			string description = null;
			bool isShort = false;
			bool isFamilySafe = true;
			bool isUnlisted = false;
			string category = null;
			DateTime datePublished = DateTime.MaxValue;
			DateTime dateUploaded = DateTime.MaxValue;

			List<YouTubeVideoThumbnail> videoThumbnails = null;

			if (simplifiedVideoInfo.IsVideoInfoAvailable)
			{
				videoTitle = simplifiedVideoInfo.Info.Value<string>("title");
				videoId = simplifiedVideoInfo.Info.Value<string>("id");
				if (int.TryParse(simplifiedVideoInfo.Info.Value<string>("lengthSeconds"), out int lengthSeconds))
				{
					videoLength = TimeSpan.FromSeconds(lengthSeconds);
				}
				ownerChannelTitle = simplifiedVideoInfo.Info.Value<string>("ownerChannelTitle");
				ownerChannelId = simplifiedVideoInfo.Info.Value<string>("ownerChannelId");
				if (!int.TryParse(simplifiedVideoInfo.Info.Value<string>("viewCount"), out viewCount))
				{
					viewCount = 0;
				}
				isPrivate = simplifiedVideoInfo.Info.Value<bool>("isPrivate");
				isLiveContent = simplifiedVideoInfo.Info.Value<bool>("isLiveContent");
				shortDescription = simplifiedVideoInfo.Info.Value<string>("shortDescription");
			}
			if (simplifiedVideoInfo.IsMicroformatInfoAvailable)
			{
				description = simplifiedVideoInfo.Info.Value<string>("description");
				isShort = simplifiedVideoInfo.Info.Value<bool>("isShort");
				isFamilySafe = simplifiedVideoInfo.Info.Value<bool>("isFamilySafe");
				isUnlisted = simplifiedVideoInfo.Info.Value<bool>("isUnlisted");
				category = simplifiedVideoInfo.Info.Value<string>("category");
				ExtractDatesFromMicroformat(simplifiedVideoInfo.Info, out dateUploaded, out datePublished);
			}

			JArray jaThumbnails = simplifiedVideoInfo.Info.Value<JArray>("thumbnails");
			if (jaThumbnails != null && jaThumbnails.Count > 0)
			{
				videoThumbnails = new List<YouTubeVideoThumbnail>();
				foreach (JObject jThumbnail in jaThumbnails.Cast<JObject>())
				{
					ushort width = jThumbnail.Value<ushort>("width");
					ushort height = jThumbnail.Value<ushort>("height");
					string fileName = jThumbnail.Value<string>("fileName");
					if (string.IsNullOrEmpty(fileName) || string.IsNullOrWhiteSpace(fileName)) { fileName = "unnamed.dat"; }
					string url = jThumbnail.Value<string>("url");
					videoThumbnails.Add(new YouTubeVideoThumbnail(width, height, fileName, url));
				}
			}

			YouTubeVideoDetails videoDetails = rawVideoInfo.VideoDetails;
			YouTubeVideoPlayabilityStatus videoStatus = rawVideoInfo.PlayabilityStatus;

			string descr = !string.IsNullOrEmpty(description) ? description : shortDescription;

			YouTubeVideo youTubeVideo = new YouTubeVideo(
				videoTitle, videoId, videoLength, dateUploaded, datePublished, ownerChannelTitle,
				ownerChannelId, descr, viewCount, category, isShort, isPrivate, isUnlisted,
				isFamilySafe, isLiveContent, videoDetails, videoThumbnails,
				rawVideoInfo, simplifiedVideoInfo, videoStatus);

			YouTubeStreamingData actualStreamingData = customStreamingData ?? rawVideoInfo.StreamingData?.Data;
			YouTubeMediaFormatList mediaFormats = actualStreamingData?.Parse(downloader);
			if (mediaFormats != null)
			{
				string clientName = mediaFormats.Client?.DisplayName ?? "unknown";
				youTubeVideo.MediaTracks[clientName] = mediaFormats;
			}

			return youTubeVideo;
		}

		/// <summary>
		/// Создаёт объект класса "YouTubeVideo" из переданных аргументов.
		/// </summary>
		/// <param name="customMicroformat">
		/// Если не 'null', эти данные будут использованы вместо "rawVideoInfo.Microformat".
		/// </param>
		/// <param name="downloader">
		/// Объект скачивателя, который будет использован для получения дополнительных данных (если это необходимо).
		/// Если передать 'null', будет автоматически создан новый объект скачивателя с настройками по-умолчанию.
		/// </param>
		public static YouTubeVideo MakeYouTubeVideo(YouTubeRawVideoInfo rawVideoInfo, JObject customMicroformat,
			FileDownloader downloader = null)
		{
			if (rawVideoInfo.PlayabilityStatus.IsLoginRequired || rawVideoInfo.PlayabilityStatus.IsBotWarning)
			{
				return YouTubeVideo.CreateEmpty(rawVideoInfo.PlayabilityStatus);
			}

			JObject actualMicroformat = customMicroformat ?? rawVideoInfo.Microformat;
			YouTubeSimplifiedVideoInfoResult simplifiedVideoInfoResult = rawVideoInfo.Simplify(actualMicroformat);
			if (simplifiedVideoInfoResult.ErrorCode != 200)
			{
				return YouTubeVideo.CreateEmpty(rawVideoInfo.PlayabilityStatus);
			}

			return MakeYouTubeVideo(rawVideoInfo, simplifiedVideoInfoResult.SimplifiedVideoInfo, downloader);
		}

		/// <summary>
		/// Создаёт объект класса "YouTubeVideo" из переданных аргументов.
		/// </summary>
		/// <param name="downloader">
		/// Объект скачивателя, который будет использован для получения дополнительных данных (если это необходимо).
		/// Если передать 'null', будет автоматически создан новый объект скачивателя с настройками по-умолчанию.
		/// </param>
		public static YouTubeVideo MakeYouTubeVideo(YouTubeRawVideoInfo rawVideoInfo,
			YouTubeSimplifiedVideoInfo simplifiedVideoInfo, FileDownloader downloader = null)
		{
			return MakeYouTubeVideo(rawVideoInfo, simplifiedVideoInfo, null, downloader);
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

		public static string ExtractVideoIdFromGridRendererItem(string gridVideoRendererItemString)
		{
			string[] patterns = new string[]
			{
				@"""videoId"":\s*""(.*)""",
				@"\/shorts/(.*)""",
				@"""shorts-shelf-item-(.*)"""
			};
			return FindRegexp(gridVideoRendererItemString, patterns);
		}

		internal static string ExtractVideoIdFromGridRendererItem(JObject jGridVideoRendererItem)
		{
			return ExtractVideoIdFromGridRendererItem(jGridVideoRendererItem.ToString());
		}

		internal static List<string> ExtractVideoIDsFromGridRendererItems(
			JArray gridVideoRendererItems, out string continuationToken)
		{
			continuationToken = null;
			if (gridVideoRendererItems == null || gridVideoRendererItems.Count == 0)
			{
				return null;
			}

			List<string> idList = new List<string>();
			foreach (JObject jItem in gridVideoRendererItems.Cast<JObject>())
			{
				string videoId = ExtractVideoIdFromGridRendererItem(jItem);
				if (!string.IsNullOrEmpty(videoId) && !string.IsNullOrWhiteSpace(videoId))
				{
					idList.Add(videoId);
				}
				else
				{
					JObject jContinuationItemRenderer = jItem.Value<JObject>("continuationItemRenderer");
					if (jContinuationItemRenderer != null)
					{
						JObject jContinuationCommand = jContinuationItemRenderer.Value<JObject>("continuationEndpoint")?.Value<JObject>("continuationCommand");
						continuationToken = jContinuationCommand?.Value<string>("token");
					}
				}
			}
			return idList;
		}

		internal static JArray FindItemsArray(JObject json, bool dataWasRecievedUsingContinuationToken)
		{
			try
			{
				if (dataWasRecievedUsingContinuationToken)
				{
					IYouTubeChannelTabPageParser[] continuationParsers = new IYouTubeChannelTabPageParser[]
					{
						new YouTubeChannelTabPageVideoContinuationParser1(),
						new YouTubeChannelTabPageVideoContinuationParser2()
					};

					foreach (IYouTubeChannelTabPageParser continuationParser in continuationParsers)
					{
						JArray items = continuationParser.FindGridItems(json);
						if (items != null && items.Count > 0) { return items; }
					}
				}
				else
				{
					YouTubeChannelTab selectedTab = YouTubeChannelTab.FindSelectedTab(json);
					if (selectedTab != null)
					{
						IYouTubeChannelTabPageParser[] parsers = new IYouTubeChannelTabPageParser[]
						{
							new YouTubeChannelTabPageParserVideo1(),
							new YouTubeChannelTabPageParserVideo2()
						};

						foreach (IYouTubeChannelTabPageParser parser in parsers)
						{
							JArray items = parser.FindGridItems(selectedTab.Json);
							if (items != null && items.Count > 0) { return items; }
						}
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.StackTrace);
			}

			return null;
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

		internal static IEnumerable<YouTubeVideoThumbnail> GetThumbnails(
			YouTubeVideoDetails videoDetails, JObject jMicroformat, string videoId = null)
		{
			var microformatThumbnails = ExtractThumbnailsFromMicroformat(jMicroformat);
			var videoDetailsThumbnails = ExtractThumbnails(videoDetails.Parse().Value<JObject>("thumbnail")?.Value<JArray>("thumbnails"));
			List<YouTubeVideoThumbnail> thumbnails = new List<YouTubeVideoThumbnail>();
			if (microformatThumbnails == null && videoDetailsThumbnails == null &&
				microformatThumbnails.Count() <= 0 && videoDetailsThumbnails.Count() <= 0)
			{
				return thumbnails;
			}

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
					string fileName = FindRegexp(url, @"vi/.{11}/([^\&\?]*)");
					if (string.IsNullOrEmpty(fileName)) { fileName = "unnamed.jpg"; }
					thumbnails.Insert(0, new YouTubeVideoThumbnail(
						thumbnails[0].Width, thumbnails[0].Height, fileName, url));
				}
			}

			if (!string.IsNullOrEmpty(videoId))
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
						string fileName = FindRegexp(url, @"\w/.{11}/([^\&\?]*)");
						if (string.IsNullOrEmpty(fileName)) { fileName = "unnamed.dat"; }

						yield return new YouTubeVideoThumbnail(width, height, fileName, url);
					}
				}
			}
		}

		private static JArray ThumbnailsToJson(IEnumerable<YouTubeVideoThumbnail> videoThumbnails)
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

		public static int YouTubeHttpPost(string url, byte[] body, NameValueCollection headers, out string responseString)
		{
			if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url) || body == null || body.Length == 0)
			{
				responseString = null;
				return 400;
			}

			try
			{
				if (headers == null) { headers = new NameValueCollection(); }

				headers["Content-Type"] = "application/json";
				headers["Content-Length"] = body.Length.ToString();

				using (HttpRequestResult requestResult = HttpRequestSender.Send("POST", url, body, headers))
				{
					if (requestResult.ErrorCode == 200)
					{
						bool isZipped = requestResult.IsZippedContent();
						return requestResult.WebContent.ContentToString(out responseString, 4096, isZipped, null, default);
					}
					else
					{
						responseString = requestResult.ErrorMessage;
						return requestResult.ErrorCode;
					}
				}
			}
			catch (Exception ex)
			{
				responseString = ex.Message;
				return ex.HResult;
			}
		}

		public static int YouTubeHttpPost(string url, string body, Encoding bodyEncoding,
			NameValueCollection headers, out string responseString)
		{
			byte[] bodyBytes = bodyEncoding.GetBytes(body);
			return YouTubeHttpPost(url, bodyBytes, headers, out responseString);
		}

		public static int YouTubeHttpPost(string url, string body,
			NameValueCollection headers, out string responseString)
		{
			return YouTubeHttpPost(url, body, Encoding.UTF8, headers, out responseString);
		}

		public static int YouTubeHttpPost(string url, string body, string userAgent, out string responseString)
		{
			NameValueCollection headers = new NameValueCollection()
			{
				{ "Host", "www.youtube.com" },
				{ "User-Agent", userAgent },
				{ "Accept", "*/*" },
				{ "Accept-Encoding", "gzip" }
			};
			byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
			return YouTubeHttpPost(url, bodyBytes, headers, out responseString);
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
					YouTubeRawVideoInfo youTubeRawVideoInfo = new YouTubeRawVideoInfo(rawVideoInfo, client, urlDecryptionData);
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
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}

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
			catch (Exception ex)
			{
				//подразумевается, что юзер ввёл ID видео, а не ссылку.
				System.Diagnostics.Debug.WriteLine(ex.Message);
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

		public static string GetYouTubeVisitorData(NameValueCollection requestHeaders = null)
		{
			string rawData = YouTubeVisitorData.GetRawData(requestHeaders);
			return string.IsNullOrEmpty(rawData) ? null : YouTubeVisitorData.ExtractVisitorDataValue(rawData);
		}

		public static string GetYouTubeVisitorData(string userAgent)
		{
			NameValueCollection headers = null;
			if (!string.IsNullOrEmpty(userAgent))
			{
				headers = new NameValueCollection()
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
				null, DateTimeStyles.AssumeLocal, out dateTime))
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

		public static void ExtractDatesFromMicroformat(
			JObject jSimplifiedVideoInfo, out DateTime uploadDate, out DateTime publishDate)
		{
			string published = jSimplifiedVideoInfo.Value<string>("datePublished");
			if (!DateTime.TryParseExact(published, "yyyy-MM-ddTHH:mm:ssZ",
				null, DateTimeStyles.AssumeLocal, out publishDate))
			{
				string startTimestamp = jSimplifiedVideoInfo.Value<string>("startTimestamp");
				if (!DateTime.TryParseExact(startTimestamp, "yyyy-MM-ddTHH:mm:ssZ",
					null, DateTimeStyles.AssumeLocal, out publishDate))
				{
					if (!DateTime.TryParseExact(published, "yyyy-MM-dd",
						null, DateTimeStyles.AssumeLocal, out publishDate))
					{
						publishDate = DateTime.MaxValue;
					}
				}
			}

			string uploaded = jSimplifiedVideoInfo.Value<string>("dateUploaded");
			ParseMicroformatDate(uploaded, out uploadDate);

			if (publishDate < DateTime.MaxValue) { publishDate = publishDate.ToUniversalTime(); }
			if (uploadDate < DateTime.MaxValue) { uploadDate = uploadDate.ToUniversalTime(); }
		}

		public static string ToUtcString(this DateTime dateTime)
		{
			DateTime dt = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
			return $"{dt:yyyy-MM-dd\"T\"HH:mm:ss}Z";
		}

		internal static string DateTimeStringToUtcString(string s)
		{
			if (!DateTime.TryParseExact(s, "MM/dd/yyyy HH:mm:ss",
				null, DateTimeStyles.AssumeLocal, out DateTime dateTime))
			{
				return s;
			}

			return dateTime.ToUtcString();
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
