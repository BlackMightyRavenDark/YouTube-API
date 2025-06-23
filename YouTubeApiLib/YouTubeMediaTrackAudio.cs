
namespace YouTubeApiLib
{
	public class YouTubeMediaTrackAudio : YouTubeMediaTrack
	{
		public string AudioQuality { get; }
		public int SampleRate { get; }
		public int ChannelCount { get; }
		public bool IsDynamicRangeCompression { get; }
		public double LoudnessDb { get; }
		public YouTubeAudioTrackLanguage Language { get; }

		public YouTubeMediaTrackAudio(
			int formatId,
			int bitrate,
			int averageBitrate,
			string lastModified,
			long contentLength,
			string quality,
			string qualityLabel,
			string audioQuality,
			int sampleRate,
			int channelCount,
			bool isDynamicRangeCompression,
			YouTubeAudioTrackLanguage language,
			double loudnessDb,
			int approxDurationMs,
			YouTubeMediaTrackUrl fileUrl,
			string mimeType,
			string mimeExt,
			string mimeCodecs,
			string fileExtension,
			bool isDash,
			bool isCiphered,
			string dashManifestUrl,
			YouTubeDashUrlList dashUrls)
			: base(formatId, bitrate, averageBitrate, lastModified, contentLength,
				quality, qualityLabel, approxDurationMs, null, fileUrl,
				mimeType, mimeExt, mimeCodecs, fileExtension,
				isDash, isCiphered, dashManifestUrl, dashUrls)
		{
			AudioQuality = audioQuality;
			SampleRate = sampleRate;
			ChannelCount = channelCount;
			IsDynamicRangeCompression = isDynamicRangeCompression;
			Language = language;
			LoudnessDb = loudnessDb;
		}

		// Упрощенный конструктор для аудио-дорожек DASH
		public YouTubeMediaTrackAudio(
			int formatId,
			int bitrate,
			int sampleRate,
			int channelCount,
			string mimeType,
			string mimeExt,
			string mimeCodecs,
			string fileExtension,
			string dashManifestUrl,
			YouTubeDashUrlList dashUrls)
			: this(formatId, bitrate, bitrate, null, -1L, null, null, null, sampleRate, channelCount,
				false, null, 0.0, -1, null, mimeType, mimeExt, mimeCodecs, fileExtension,
				true, false, dashManifestUrl, dashUrls) { }

		// Упрощенный конструктор для аудио-дорожек не-DASH
		public YouTubeMediaTrackAudio(
			int formatId,
			int bitrate,
			int averageBitrate,
			string lastModified,
			long contentLength,
			string quality,
			string qualityLabel,
			string audioQuality,
			int sampleRate,
			int channelCount,
			bool isDynamicRangeCompression,
			YouTubeAudioTrackLanguage language,
			double loudnessDb,
			int approxDurationMs,
			YouTubeMediaTrackUrl fileUrl,
			string mimeType,
			string mimeExt,
			string mimeCodecs,
			string fileExtension,
			bool isCiphered)
			: this(formatId, bitrate, averageBitrate, lastModified, contentLength, quality, qualityLabel,
				audioQuality, sampleRate, channelCount, isDynamicRangeCompression, language, loudnessDb,
				approxDurationMs, fileUrl, mimeType, mimeExt, mimeCodecs, fileExtension,
				false, isCiphered, null, null) { }
	}
}
