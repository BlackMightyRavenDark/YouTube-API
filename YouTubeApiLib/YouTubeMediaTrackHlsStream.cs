
namespace YouTubeApiLib
{
	public class YouTubeMediaTrackHlsStream : YouTubeMediaTrackVideo
	{
		public string HlsManifestUrl { get; }
		public YouTubeBroadcast Broadcast { get; }

		public YouTubeMediaTrackHlsStream(YouTubeBroadcast broadcast, string hlsManifestUrl)
			: base(broadcast.FormatId, broadcast.VideoWidth, broadcast.VideoHeight, broadcast.FrameRate,
				broadcast.Bandwidth, broadcast.Bandwidth, null, -1L,
				null, null, -1, null, broadcast.PlaylistUrl,
				"video/ts", "ts", broadcast.Codecs, "ts",
				false, false, null, null)
		{
			VideoWidth = broadcast.VideoWidth;
			VideoHeight = broadcast.VideoHeight;
			FrameRate = broadcast.FrameRate;
			HlsManifestUrl = hlsManifestUrl;
			Broadcast = broadcast;
		}
	}
}
