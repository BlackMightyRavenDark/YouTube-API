﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;

namespace YouTubeApiLib
{
    public static class Utils
    {
        public static SimplifiedVideoInfoResult ParseRawVideoInfo(RawVideoInfo rawVideoInfo)
        {
            JObject jVideoDetails = rawVideoInfo.RawData.Value<JObject>("videoDetails");
            if (jVideoDetails == null)
            {
                return new SimplifiedVideoInfoResult(null, 404);
            }
            string videoTitle = jVideoDetails.Value<string>("title");
            string videoId = jVideoDetails.Value<string>("videoId");
            string shortDescription = jVideoDetails.Value<string>("shortDescription");
            int lengthSeconds = int.Parse(jVideoDetails.Value<string>("lengthSeconds"));
            string ownerChannelTitle = jVideoDetails.Value<string>("author");
            string ownerChannelId = jVideoDetails.Value<string>("channelId");
            int viewCount = int.Parse(jVideoDetails.Value<string>("viewCount"));
            bool isPrivate = jVideoDetails.Value<bool>("isPrivate");
            bool isLiveContent = jVideoDetails.Value<bool>("isLiveContent");
            JObject jMicroformatRenderer = rawVideoInfo.RawData.Value<JObject>("microformat").Value<JObject>("playerMicroformatRenderer");
            string description = null;
            JToken jt = jMicroformatRenderer.Value<JToken>("description");
            if (jt != null)
            {
                jt = jt.Value<JToken>("simpleText");
                if (jt != null)
                {
                    description = jt.Value<string>();
                }
            }
            bool isFamilySafe = jMicroformatRenderer.Value<bool>("isFamilySafe");
            bool isUnlisted = jMicroformatRenderer.Value<bool>("isUnlisted");
            string category = jMicroformatRenderer.Value<string>("category");
            string datePublished = jMicroformatRenderer.Value<string>("publishDate");
            string dateUploaded = jMicroformatRenderer.Value<string>("uploadDate");

            JObject simplifiedVideoInfo = new JObject();
            simplifiedVideoInfo["title"] = videoTitle;
            simplifiedVideoInfo["id"] = videoId;
            simplifiedVideoInfo["url"] = $"{YouTubeApi.YOUTUBE_URL}/watch?v={videoId}";
            simplifiedVideoInfo["lengthSeconds"] = lengthSeconds;
            simplifiedVideoInfo["ownerChannelTitle"] = ownerChannelTitle;
            simplifiedVideoInfo["ownerChannelId"] = ownerChannelId;
            simplifiedVideoInfo["viewCount"] = viewCount;
            simplifiedVideoInfo["category"] = category;
            simplifiedVideoInfo["isPrivate"] = isPrivate;
            simplifiedVideoInfo["isUnlisted"] = isUnlisted;
            simplifiedVideoInfo["isFamilySafe"] = isFamilySafe;
            simplifiedVideoInfo["isLiveContent"] = isLiveContent;
            simplifiedVideoInfo["datePublished"] = datePublished;
            simplifiedVideoInfo["dateUploaded"] = dateUploaded;
            simplifiedVideoInfo["description"] = description;
            simplifiedVideoInfo["shortDescription"] = shortDescription;

            JObject jThumbnail = jVideoDetails.Value<JObject>("thumbnail");
            JArray jaThumbnails = jThumbnail?.Value<JArray>("thumbnails");
            simplifiedVideoInfo["thumbnails"] = jaThumbnails;

            JObject jStreamingData = rawVideoInfo.RawData.Value<JObject>("streamingData");
            simplifiedVideoInfo["streamingData"] = jStreamingData;

            return new SimplifiedVideoInfoResult(new SimplifiedVideoInfo(simplifiedVideoInfo), 200);
        }

        public static int HttpsPost(string url, string body, out string responseString)
        {
            responseString = "Client error";
            int res = 400;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentLength = body.Length;
            httpWebRequest.Host = "www.youtube.com";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3591.2 Safari/537.36";
            httpWebRequest.Method = "POST";
            StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
            try
            {
                streamWriter.Write(body);
                streamWriter.Dispose();
            }
            catch
            {
                if (streamWriter != null)
                {
                    streamWriter.Dispose();
                }
                return res;
            }
            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream());
                try
                {
                    responseString = streamReader.ReadToEnd();
                    streamReader.Dispose();
                    res = (int)httpResponse.StatusCode;
                }
                catch
                {
                    if (streamReader != null)
                    {
                        streamReader.Dispose();
                    }
                    return 400;
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                    responseString = ex.Message;
                    res = (int)httpWebResponse.StatusCode;
                }
            }
            return res;
        }

        public static string ExtractVideoIdFromUrl(string url)
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
                return url;
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

                    return videoId;
                }
                return null;
            }

            Dictionary<string, string> dict = SplitUrlQueryToDictionary(uri.Query);
            if (dict == null || !dict.ContainsKey("v"))
            {
                return null;
            }

            return dict["v"];
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
            string inputString, char keySeparaor, char valueSeparator)
        {
            if (string.IsNullOrEmpty(inputString) || string.IsNullOrWhiteSpace(inputString))
            {
                return null;
            }
            string[] keyValues = inputString.Split(keySeparaor);
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

        public static bool StringToDateTime(string inputString, out DateTime resDateTime, string format = "yyyy-MM-dd")
        {
            bool res = DateTime.TryParseExact(inputString, format,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out resDateTime);
            if (!res)
            {
                resDateTime = DateTime.MaxValue;
            }
            return res;
        }

    }
}
