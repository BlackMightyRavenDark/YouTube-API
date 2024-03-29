﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
    public static class Utils
    {
        public const string YOUTUBE_URL = "https://www.youtube.com";

        public enum VideoInfoGettingMethod
        {
            HiddenApiEncryptedUrls,
            HiddenApiDecryptedUrls,

            /// <summary>
            /// This way will download and parse the video web page.
            /// Warning!!! YouTube may ban your IP address if you make requests too often!
            /// You will get the "HTTP error 429" for some days or weeks (maybe forever!).
            /// You should not abuse this method!
            /// </summary>
            WebPage,

            /// <summary>
            /// Used when user provide the web page code or video info data manually.
            /// </summary>
            Manual
        }

        public static string GetVideoUrl(string videoId)
        {
            return $"{YOUTUBE_URL}/watch?v={videoId}";
        }

        internal static YouTubeVideo GetVideoFromWebPage(YouTubeVideoWebPage webPage)
        {
            if (webPage != null)
            {
                RawVideoInfoResult rawVideoInfoResult = ExtractRawVideoInfoFromWebPage(webPage);
                if (rawVideoInfoResult.ErrorCode == 200)
                {
                    SimplifiedVideoInfoResult simplifiedVideoInfoResult = ParseRawVideoInfo(rawVideoInfoResult.RawVideoInfo);
                    if (simplifiedVideoInfoResult.ErrorCode == 200)
                    {
                        return MakeYouTubeVideo(rawVideoInfoResult.RawVideoInfo, simplifiedVideoInfoResult.SimplifiedVideoInfo);
                    }
                }
            }
            return null;
        }

        public static RawVideoInfoResult GetRawVideoInfo(
            string videoId, VideoInfoGettingMethod method)
        {
            switch (method)
            {
                case VideoInfoGettingMethod.HiddenApiEncryptedUrls:
                case VideoInfoGettingMethod.HiddenApiDecryptedUrls:
                    return YouTubeApiV1.GetRawVideoInfo(videoId, method);

                case VideoInfoGettingMethod.WebPage:
                    return GetRawVideoInfoViaWebPage(videoId);
            }
            return new RawVideoInfoResult(null, 400);
        }

        internal static RawVideoInfoResult GetRawVideoInfoViaWebPage(string videoId)
        {
            YouTubeVideoWebPageResult videoWebPageResult = YouTubeVideoWebPage.Get(videoId);
            if (videoWebPageResult.ErrorCode == 200)
            {
                return ExtractRawVideoInfoFromWebPage(videoWebPageResult.VideoWebPage);
            }
            return new RawVideoInfoResult(null, videoWebPageResult.ErrorCode);
        }

        internal static SimplifiedVideoInfoResult GetSimplifiedVideoInfo(string videoId)
        {
            RawVideoInfoResult rawVideoInfoResult = GetRawVideoInfo(videoId, YouTubeApi.defaultVideoInfoGettingMethod);
            if (rawVideoInfoResult.ErrorCode == 200)
            {
                return ParseRawVideoInfo(rawVideoInfoResult.RawVideoInfo);
            }
            return new SimplifiedVideoInfoResult(null, rawVideoInfoResult.ErrorCode);
        }

        public static SimplifiedVideoInfoResult ParseRawVideoInfo(RawVideoInfo rawVideoInfo)
        {
            JObject jVideoDetails = rawVideoInfo.VideoDetails;
            JObject jMicroformat = rawVideoInfo.Microformat;
            JObject jMicroformatRenderer = jMicroformat?.Value<JObject>("playerMicroformatRenderer");

            JObject jSimplifiedVideoInfo = new JObject();
            string videoId = null;
            bool isUnlisted = false;
            bool isFamilySafe = true;
            if (jVideoDetails != null)
            {
                jSimplifiedVideoInfo["title"] = jVideoDetails.Value<string>("title");
                videoId = jVideoDetails.Value<string>("videoId");
                jSimplifiedVideoInfo["id"] = videoId;
                jSimplifiedVideoInfo["url"] = GetVideoUrl(videoId);
                if (int.TryParse(jVideoDetails.Value<string>("lengthSeconds"), out int lengthSeconds))
                {
                    jSimplifiedVideoInfo["lengthSeconds"] = lengthSeconds;
                }
                jSimplifiedVideoInfo["ownerChannelTitle"] = jVideoDetails.Value<string>("author");
                jSimplifiedVideoInfo["ownerChannelId"] = jVideoDetails.Value<string>("channelId");
                jSimplifiedVideoInfo["viewCount"] = int.Parse(jVideoDetails.Value<string>("viewCount"));
                jSimplifiedVideoInfo["isPrivate"] = jVideoDetails.Value<bool>("isPrivate");
                jSimplifiedVideoInfo["isLiveContent"] = jVideoDetails.Value<bool>("isLiveContent");
                jSimplifiedVideoInfo["shortDescription"] = jVideoDetails.Value<string>("shortDescription");
            }
            if (jMicroformatRenderer != null)
            {
                JObject jDescription = jMicroformatRenderer.Value<JObject>("description");
                jSimplifiedVideoInfo["description"] = jDescription?.Value<string>("simpleText");
                isFamilySafe = jMicroformatRenderer.Value<bool>("isFamilySafe");
                jSimplifiedVideoInfo["isFamilySafe"] = isFamilySafe;
                isUnlisted = jMicroformatRenderer.Value<bool>("isUnlisted");
                jSimplifiedVideoInfo["isUnlisted"] = isUnlisted;
                jSimplifiedVideoInfo["category"] = jMicroformatRenderer.Value<string>("category");
                jSimplifiedVideoInfo["datePublished"] = jMicroformatRenderer.Value<string>("publishDate");
                jSimplifiedVideoInfo["dateUploaded"] = jMicroformatRenderer.Value<string>("uploadDate");

                JObject jLiveBroadcastDetails = jMicroformatRenderer.Value<JObject>("liveBroadcastDetails");
                if (jLiveBroadcastDetails != null)
                {
                    jSimplifiedVideoInfo["startTimestamp"] = jLiveBroadcastDetails.Value<string>("startTimestamp");
                    jSimplifiedVideoInfo["endTimestamp"] = jLiveBroadcastDetails.Value<string>("endTimestamp");
                }
            }

            List<YouTubeVideoThumbnail> videoThumbnails = GetThumbnailUrls(jMicroformat, videoId);
            jSimplifiedVideoInfo["thumbnails"] = ThumbnailsToJson(videoThumbnails);

            StreamingData streamingData = null;
            if (YouTubeApi.getMediaTracksInfoImmediately && isFamilySafe &&
                rawVideoInfo.DataGettingMethod != VideoInfoGettingMethod.HiddenApiDecryptedUrls)
            {
                VideoInfoGettingMethod method =
                    YouTubeApi.decryptMediaTrackUrlsAutomaticallyIfPossible && !isUnlisted ?
                    VideoInfoGettingMethod.HiddenApiDecryptedUrls :
                    VideoInfoGettingMethod.HiddenApiEncryptedUrls;
                streamingData = GetStreamingData(videoId, method);
            }
            jSimplifiedVideoInfo["streamingData"] =
                streamingData != null ? streamingData.RawData : rawVideoInfo.StreamingData?.RawData;

            SimplifiedVideoInfo simplifiedVideoInfo = new SimplifiedVideoInfo(
                jSimplifiedVideoInfo, jVideoDetails != null, jMicroformatRenderer != null, streamingData);
            return new SimplifiedVideoInfoResult(simplifiedVideoInfo, 200);
        }

        public static YouTubeVideo MakeYouTubeVideo(RawVideoInfo rawVideoInfo)
        {
            SimplifiedVideoInfoResult simplifiedVideoInfoResult = ParseRawVideoInfo(rawVideoInfo);
            if (simplifiedVideoInfoResult.ErrorCode != 200)
            {
                return YouTubeVideo.CreateEmpty(rawVideoInfo.PlayabilityStatus);
            }

            return MakeYouTubeVideo(rawVideoInfo, simplifiedVideoInfoResult.SimplifiedVideoInfo);
        }

        public static YouTubeVideo MakeYouTubeVideo(RawVideoInfo rawVideoInfo, SimplifiedVideoInfo simplifiedVideoInfo)
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
                isFamilySafe = simplifiedVideoInfo.Info.Value<bool>("isFamilySafe");
                isUnlisted = simplifiedVideoInfo.Info.Value<bool>("isUnlisted");
                category = simplifiedVideoInfo.Info.Value<string>("category");
                ExtractDatesFromMicroformat(simplifiedVideoInfo.Info, out dateUploaded, out datePublished);
            }

            JArray jaThumbnails = simplifiedVideoInfo.Info.Value<JArray>("thumbnails");
            if (jaThumbnails != null && jaThumbnails.Count > 0)
            {
                videoThumbnails = new List<YouTubeVideoThumbnail>();
                foreach (JObject jThumbnail in jaThumbnails)
                {
                    string id = jThumbnail.Value<string>("id");
                    string url = jThumbnail.Value<string>("url");
                    videoThumbnails.Add(new YouTubeVideoThumbnail(id, url));
                }
            }

            LinkedList<YouTubeMediaTrack> mediaTracks = null;
            if (YouTubeApi.getMediaTracksInfoImmediately)
            {
                mediaTracks = simplifiedVideoInfo.StreamingData != null ?
                    ParseMediaTracks(simplifiedVideoInfo.StreamingData) :
                    ParseMediaTracks(rawVideoInfo);
            }

            YouTubeVideoPlayabilityStatus videoStatus = rawVideoInfo.PlayabilityStatus;

            string descr = !string.IsNullOrEmpty(description) ? description : shortDescription;

            YouTubeVideo youTubeVideo = new YouTubeVideo(
                videoTitle, videoId, videoLength, dateUploaded, datePublished, ownerChannelTitle,
                ownerChannelId, descr, viewCount, category, isPrivate, isUnlisted,
                isFamilySafe, isLiveContent, videoThumbnails, mediaTracks,
                rawVideoInfo, simplifiedVideoInfo, videoStatus);
            return youTubeVideo;
        }

        public static StreamingData GetStreamingData(string videoId, VideoInfoGettingMethod method)
        {
            RawVideoInfoResult rawVideoInfoResult = GetRawVideoInfo(videoId, method);
            if (rawVideoInfoResult.ErrorCode == 200)
            {
                return rawVideoInfoResult.RawVideoInfo.StreamingData;
            }
            return null;
        }

        internal static string ExtractVideoIDsFromGridRendererItem(JObject gridVideoRendererItem)
        {
            JObject j = gridVideoRendererItem.Value<JObject>("gridVideoRenderer");
            if (j != null)
            {
                return j.Value<string>("videoId");
            }
            else
            {
                j = gridVideoRendererItem.Value<JObject>("richItemRenderer");
                if (j != null)
                {
                    j = j.Value<JObject>("content");
                    if (j != null)
                    {
                        JObject jRenderer = j.Value<JObject>("videoRenderer");
                        if (jRenderer == null)
                        {
                            jRenderer = j.Value<JObject>("reelItemRenderer");
                        }
                        if (jRenderer != null)
                        {
                            return jRenderer.Value<string>("videoId");
                        }
                    }
                }
            }
            return null;
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
            foreach (JObject jItem in gridVideoRendererItems)
            {
                string videoId = ExtractVideoIDsFromGridRendererItem(jItem);
                if (!string.IsNullOrEmpty(videoId) && !string.IsNullOrWhiteSpace(videoId))
                {
                    idList.Add(videoId);
                }
                else
                {
                    JObject jContinuationItemRenderer = jItem.Value<JObject>("continuationItemRenderer");
                    if (jContinuationItemRenderer != null)
                    {
                        JObject j = jContinuationItemRenderer.Value<JObject>("continuationEndpoint");
                        if (j != null)
                        {
                            j = j.Value<JObject>("continuationCommand");
                            if (j != null)
                            {
                                continuationToken = j.Value<string>("token");
                            }
                        }
                    }
                }
            }
            return idList;
        }

        public static JArray FindTabItems(JObject megaRoot)
        {
            JObject j = megaRoot.Value<JObject>("contents");
            if (j == null)
            {
                return null;
            }
            j = j.Value<JObject>("twoColumnBrowseResultsRenderer");
            if (j == null)
            {
                return null;
            }
            return j.Value<JArray>("tabs");
        }

        public static YouTubeChannelTab FindSelectedChannelTab(JObject megaRoot)
        {
            JArray jaTabs = FindTabItems(megaRoot);
            if (jaTabs == null || jaTabs.Count == 0)
            {
                return null;
            }
            return FindSelectedChannelTab(jaTabs);
        }
        
        public static YouTubeChannelTab FindSelectedChannelTab(JArray jaTabs)
        {
            foreach (JObject jObject in jaTabs)
            {
                JObject j = jObject.Value<JObject>("tabRenderer");
                if (j == null)
                {
                    j = jObject.Value<JObject>("expandableTabRenderer");
                }

                if (j != null)
                {
                    bool selected = j.Value<bool>("selected");
                    if (selected)
                    {
                        string tabTitle = j.Value<string>("title");
                        return new YouTubeChannelTab(tabTitle, jObject);
                    }
                }
            }
            return null;
        }

        internal static JArray FindItemsArray(JObject json, bool token)
        {
            try
            {
                if (token)
                {
                    List<ITabPageParser> continuationParsers = new List<ITabPageParser>()
                    {
                        new TabPageVideoContinuationParser1(),
                        new TabPageVideoContinuationParser2()
                    };
                    foreach (ITabPageParser continuationParser in continuationParsers)
                    {
                        JArray items = continuationParser.FindGridItems(json);
                        if (items != null && items.Count > 0)
                        {
                            return items;
                        }
                    }
                }
                else
                {
                    YouTubeChannelTab selectedTab = FindSelectedChannelTab(json);
                    if (selectedTab != null)
                    {
                        List<ITabPageParser> parsers = new List<ITabPageParser>() {
                            new TabPageParserVideo1(), new TabPageParserVideo2()
                        };
                        foreach (ITabPageParser parser in parsers)
                        {
                            JArray items = parser.FindGridItems(selectedTab.Json);
                            if (items != null && items.Count > 0)
                            {
                                return items;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return null;
            }

            return null;
        }

        internal static List<YouTubeVideoThumbnail> GetThumbnailUrls(JObject jMicroformat, string videoId)
        {
            if (string.IsNullOrEmpty(videoId) || string.IsNullOrWhiteSpace(videoId))
            {
                return null;
            }

            List<YouTubeVideoThumbnail> possibleThumbnails = new List<YouTubeVideoThumbnail>()
            {
                new YouTubeVideoThumbnail("maxresdefault", $"https://i.ytimg.com/vi/{videoId}/maxresdefault.jpg"),
                new YouTubeVideoThumbnail("hqdefault", $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg"),
                new YouTubeVideoThumbnail("mqdefault", $"https://i.ytimg.com/vi/{videoId}/mqdefault.jpg"),
                new YouTubeVideoThumbnail("sddefault", $"https://i.ytimg.com/vi/{videoId}/sddefault.jpg"),
                new YouTubeVideoThumbnail("default", $"https://i.ytimg.com/vi/{videoId}/default.jpg")
            };

            if (jMicroformat != null)
            {
                List<YouTubeVideoThumbnail> thumbnails = ExtractThumbnailsFromMicroformat(jMicroformat);
                foreach (YouTubeVideoThumbnail thumbnail in thumbnails)
                {
                    bool found = false;
                    foreach (YouTubeVideoThumbnail thumbnail2 in possibleThumbnails)
                    {
                        if (thumbnail.Url == thumbnail2.Url)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        possibleThumbnails.Add(thumbnail);
                    }
                }
            }

            List<YouTubeVideoThumbnail> resList = new List<YouTubeVideoThumbnail>();
            foreach (YouTubeVideoThumbnail thumbnail in possibleThumbnails)
            {
                //TODO: Check the URL availability
                //But it's extremely slow operation :(
                resList.Add(thumbnail);
            }

            return resList;
        }

        private static List<YouTubeVideoThumbnail> ExtractThumbnailsFromMicroformat(JObject jMicroformat)
        {
            if (jMicroformat != null)
            {
                JObject jMicroformatRenderer = jMicroformat.Value<JObject>("playerMicroformatRenderer");
                return ExtractThumbnailsFromMicroformatRenderer(jMicroformatRenderer);
            }
            return null;
        }

        private static List<YouTubeVideoThumbnail> ExtractThumbnailsFromMicroformatRenderer(JObject jMicroformatRenderer)
        {
            List<YouTubeVideoThumbnail> resList = new List<YouTubeVideoThumbnail>();
            if (jMicroformatRenderer != null)
            {
                JObject jThumbnail = jMicroformatRenderer.Value<JObject>("thumbnail");
                if (jThumbnail != null)
                {
                    JArray jaThumbnails = jThumbnail.Value<JArray>("thumbnails");
                    if (jaThumbnails != null && jaThumbnails.Count > 0)
                    {
                        foreach (JObject j in jaThumbnails)
                        {
                            string url = j.Value<string>("url");
                            if (!string.IsNullOrEmpty(url) && !string.IsNullOrWhiteSpace(url))
                            {
                                if (url.Contains("?"))
                                {
                                    url = url.Substring(0, url.IndexOf("?"));
                                }
                                if (url.Contains("vi_webp"))
                                {
                                    url = url.Replace("vi_webp", "vi").Replace(".webp", ".jpg");
                                }

                                bool found = false;
                                foreach (YouTubeVideoThumbnail thumbnail in resList)
                                {
                                    if (thumbnail.Url == url)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    resList.Add(new YouTubeVideoThumbnail("Unnamed", url));
                                }
                            }
                        }
                    }
                }
            }
            return resList;
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

        internal static LinkedList<YouTubeMediaTrack> ParseMediaTracks(StreamingData streamingData)
        {
            return streamingData?.Parse();
        }

        internal static LinkedList<YouTubeMediaTrack> ParseMediaTracks(RawVideoInfo rawVideoInfo)
        {
            return rawVideoInfo != null ? ParseMediaTracks(rawVideoInfo.StreamingData) : null;
        }

        public static int HttpsPost(string url, string body, out string responseString)
        {
            responseString = "Client error";
            int res = 400;
            try
            {
                const string userAgent = "com.google.android.youtube/17.10.35 (Linux; U; Android 12; GB) gzip";
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentLength = body.Length;
                httpWebRequest.Host = "www.youtube.com";
                httpWebRequest.UserAgent = userAgent;
                httpWebRequest.Method = "POST";
                using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(body);
                    streamWriter.Dispose();
                }
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    responseString = streamReader.ReadToEnd();
                    streamReader.Dispose();
                    res = (int)httpResponse.StatusCode;
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                responseString = ex.Message;
                res = ex.HResult;
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                    res = (int)httpWebResponse.StatusCode;
                }
            }
            return res;
        }

        public static RawVideoInfoResult ExtractRawVideoInfoFromWebPage(YouTubeVideoWebPage webPage)
        {
            if (webPage != null)
            {
                string videoInfo = ExtractRawVideoInfoFromWebPageCode(webPage.WebPageCode);
                if (!string.IsNullOrEmpty(videoInfo) && !string.IsNullOrWhiteSpace(videoInfo))
                {
                    JObject j = JObject.Parse(videoInfo);
                    if (j != null)
                    {
                        VideoInfoGettingMethod method = webPage.IsProvidedManually ? VideoInfoGettingMethod.Manual : VideoInfoGettingMethod.WebPage;
                        return new RawVideoInfoResult(new RawVideoInfo(j, method), 200);
                    }
                    else
                    {
                        return new RawVideoInfoResult(null, 400);
                    }
                }
            }
            return new RawVideoInfoResult(null, 404);
        }

        internal static string ExtractRawVideoInfoFromWebPageCode(string webPageCode)
        {
            //TODO: Replace this shit with a cool web page parser!
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

        public static VideoId ExtractVideoIdFromUrl(string url)
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
                return new VideoId(url);
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

                    return new VideoId(videoId);
                }
                return null;
            }

            Dictionary<string, string> dict = SplitUrlQueryToDictionary(uri.Query);
            if (dict == null || !dict.ContainsKey("v"))
            {
                return null;
            }

            return new VideoId(dict["v"]);
        }

        public static int DownloadString(string url, out string response)
        {
            FileDownloader d = new FileDownloader() { Url = url };
            return d.DownloadString(out response);
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
                    string[] t = keyValues[i].Split(valueSeparator);
                    dict.Add(t[0], t[1]);
                }
            }
            return dict;
        }

        public static bool ParseMicroformatDate(string dateString, out DateTime dateTime)
        {
            if (DateTime.TryParseExact(dateString, "MM/dd/yyyy HH:mm:ss",
                null, DateTimeStyles.None, out dateTime))
            {
                return true;
            }

            if (DateTime.TryParseExact(dateString, "yyyy-MM-dd",
                null, DateTimeStyles.None, out dateTime))
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
            if (!DateTime.TryParseExact(published, "MM/dd/yyyy HH:mm:ss",
                null, DateTimeStyles.None, out publishDate))
            {
                string startTimestamp = jSimplifiedVideoInfo.Value<string>("startTimestamp");
                if (!DateTime.TryParseExact(startTimestamp, "MM/dd/yyyy HH:mm:ss",
                    null, DateTimeStyles.None, out publishDate))
                {
                    if (!DateTime.TryParseExact(published, "yyyy-MM-dd",
                        null, DateTimeStyles.None, out publishDate))
                    {
                        publishDate = DateTime.MaxValue;
                    }
                }
            }

            string uploaded = jSimplifiedVideoInfo.Value<string>("dateUploaded");
            ParseMicroformatDate(uploaded, out uploadDate);
        }
    }
}
