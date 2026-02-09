
namespace YouTubeApiLib
{
	public class YouTubeMediaTrackContainer : YouTubeMediaTrack
	{
		public int VideoWidth { get; }
		public int VideoHeight { get; }
		public int VideoFrameRate { get; }
		public string AudioQuality { get; }
		public int AudioSampleRate { get; }
		public int AudioChannelCount { get; }

		public YouTubeMediaTrackContainer(
			int formatId,
			int videoWidth, int videoHeight,
			int videoFrameRate,
			int bitrate,
			int averageBitrate,
			string lastModified,
			long contentLength,
			string quality,
			string qualityLabel,
			string audioQuality,
			int audioSampleRate,
			int audioChannelCount,
			int approxDurationMs,
			string projectionType,
			YouTubeMediaTrackUrl fileUrl,
			string mimeType,
			string mimeExt,
			string mimeCodecs,
			string fileExtension,
			bool isCiphered,
			string rawInfo)
			: base(formatId, bitrate, averageBitrate, lastModified, contentLength,
				quality, qualityLabel, approxDurationMs, projectionType, fileUrl,
				mimeType, mimeExt, mimeCodecs, fileExtension,
				false, isCiphered, null, null, rawInfo)
		{
			VideoWidth = videoWidth;
			VideoHeight = videoHeight;
			VideoFrameRate = videoFrameRate;
			AudioQuality = audioQuality;
			AudioSampleRate = audioSampleRate;
			AudioChannelCount = audioChannelCount;
		}
	}
}
