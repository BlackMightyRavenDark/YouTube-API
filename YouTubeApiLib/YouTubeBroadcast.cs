
namespace YouTubeApiLib
{
	public class YouTubeBroadcast
	{
		public int FormatId { get; }
		public int VideoWidth { get; }
		public int VideoHeight { get; }
		public int FrameRate { get; }
		public int Bandwidth { get; }
		public string Codecs { get; }
		public YouTubeMediaTrackUrl PlaylistUrl { get; }
		public string RawInfo { get; }

		public YouTubeBroadcast(int formatId, int videoWidth, int videoHeight,
			int frameRate, int bandwidth, string codecs, YouTubeMediaTrackUrl playlistUrl, string rawInfo)
		{
			FormatId = formatId;
			VideoWidth = videoWidth;
			VideoHeight = videoHeight;
			FrameRate = frameRate;
			Bandwidth = bandwidth;
			Codecs = codecs;
			PlaylistUrl = playlistUrl;
			RawInfo = rawInfo;
		}
	}
}
