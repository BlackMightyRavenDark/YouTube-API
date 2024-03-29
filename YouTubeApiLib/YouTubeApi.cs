﻿using static YouTubeApiLib.Utils;

namespace YouTubeApiLib
{
    public sealed class YouTubeApi
    {
        public static bool getMediaTracksInfoImmediately = false;
        internal static VideoInfoGettingMethod defaultVideoInfoGettingMethod = VideoInfoGettingMethod.HiddenApiEncryptedUrls;
        public static bool decryptMediaTrackUrlsAutomaticallyIfPossible = true;

        public YouTubeVideo GetVideo(VideoId videoId)
        {
            RawVideoInfoResult rawVideoInfoResult = GetRawVideoInfo(videoId);
            if (rawVideoInfoResult.ErrorCode == 200)
            {
                return MakeYouTubeVideo(rawVideoInfoResult.RawVideoInfo);
            }
            return YouTubeVideo.CreateEmpty(new YouTubeVideoPlayabilityStatus(
                null, null, null, rawVideoInfoResult.ErrorCode, null));
        }

        public YouTubeVideo GetVideo(YouTubeVideoWebPage videoWebPage)
        {
            return GetVideoFromWebPage(videoWebPage);
        }

        public YouTubeVideo GetVideo(string webPageCode)
        {
            YouTubeVideoWebPageResult webPageResult = YouTubeVideoWebPage.FromCode(webPageCode);
            if (webPageResult.ErrorCode == 200)
            {
                return GetVideo(webPageResult.VideoWebPage);
            }
            return null;
        }

        public RawVideoInfoResult GetRawVideoInfo(VideoId videoId,
            VideoInfoGettingMethod method)
        {
            return Utils.GetRawVideoInfo(videoId.Id, method);
        }

        public RawVideoInfoResult GetRawVideoInfo(VideoId videoId)
        {
            return Utils.GetRawVideoInfo(videoId.Id, defaultVideoInfoGettingMethod);
        }

        public SimplifiedVideoInfoResult GetSimplifiedVideoInfo(string videoId)
        {
            return Utils.GetSimplifiedVideoInfo(videoId);
        }

        public VideoListResult GetChannelVideoList(YouTubeChannel channel)
        {
            return YouTubeApiV1.GetChannelVideoList(channel.Id);
        }

        public VideoIdPageResult GetVideoIdPage(YouTubeChannel youTubeChannel, string continuationToken)
        {
            return GetVideoIdPage(youTubeChannel, null, continuationToken);
        }

        public VideoIdPageResult GetVideoIdPage(
            YouTubeChannel youTubeChannel, YouTubeChannelTabPage channelTabPage, string continuationToken)
        {
            return YouTubeApiV1.GetVideoIdPage(youTubeChannel?.Id, channelTabPage, continuationToken);
        }

        public VideoPageResult GetVideoPage(YouTubeChannel youTubeChannel, string continuationToken)
        {
            return GetVideoPage(youTubeChannel, null, continuationToken);
        }

        public VideoPageResult GetVideoPage(
            YouTubeChannel youTubeChannel, YouTubeChannelTabPage channelTabPage, string continuationToken)
        {
            return YouTubeApiV1.GetVideoPage(youTubeChannel?.Id, channelTabPage, continuationToken);
        }

        public YouTubeChannelTabResult GetChannelTab(
            YouTubeChannel youTubeChannel, YouTubeChannelTabPage youTubeChannelTabPage)
        {
            return YouTubeApiV1.GetChannelTab(youTubeChannel.Id, youTubeChannelTabPage);
        }

        public YouTubeApiV1SearchResults Search(string searchQuery, string continuationToken,
            YouTubeApiV1SearchResultFilter searchResultFilter)
        {
            return YouTubeApiV1.SearchYouTube(searchQuery, continuationToken, searchResultFilter);
        }
    }
}
