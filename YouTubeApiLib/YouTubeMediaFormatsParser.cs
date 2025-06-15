using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib
{
	public static class YouTubeMediaFormatsParser
	{
		public static YouTubeMediaFormatList Parse(YouTubeStreamingData streamingData, FileDownloader downloader = null)
		{
			if (streamingData == null || streamingData.RawData == null)
			{
				return null;
			}

			LinkedList<YouTubeMediaTrack> mediaTracks = new LinkedList<YouTubeMediaTrack>();

			string hlsManifestUrl = streamingData.GetHlsManifestUrl();
			if (!string.IsNullOrEmpty(hlsManifestUrl) && !string.IsNullOrWhiteSpace(hlsManifestUrl) &&
				Utils.DownloadString(hlsManifestUrl, out string hlsManifest, downloader) == 200)
			{
				YouTubeHlsManifestParser parser = new YouTubeHlsManifestParser(hlsManifest);
				LinkedList<YouTubeBroadcast> broadcasts = parser.Parse();
				if (broadcasts != null)
				{
					foreach (YouTubeBroadcast broadcast in broadcasts)
					{
						YouTubeMediaTrack hlsStream = new YouTubeMediaTrackHlsStream(broadcast, hlsManifestUrl);
						mediaTracks.AddLast(hlsStream);
					}
				}
			}

			string dashManifestUrl = streamingData.GetDashManifestUrl();
			if (!string.IsNullOrEmpty(dashManifestUrl) && !string.IsNullOrWhiteSpace(dashManifestUrl) &&
				Utils.DownloadString(dashManifestUrl, out string dashManifest, downloader) == 200)
			{
				YouTubeDashManifestParser parser = new YouTubeDashManifestParser(dashManifest, dashManifestUrl);
				LinkedList<YouTubeMediaTrack> dashList = parser.Parse();
				if (dashList != null)
				{
					foreach (YouTubeMediaTrack track in dashList)
					{
						mediaTracks.AddLast(track);
					}
				}
			}

			JArray jaAdaptiveFormats = streamingData.GetAdaptiveFormats();
			if (jaAdaptiveFormats != null)
			{
				foreach (JObject jFormat in jaAdaptiveFormats.Cast<JObject>())
				{
					string mimeType = jFormat.Value<string>("mimeType");
					if (string.IsNullOrEmpty(mimeType) || string.IsNullOrWhiteSpace(mimeType))
					{
						System.Diagnostics.Debug.WriteLine("The \"mimeType\" field read error!");
						continue;
					}

					if (mimeType.Contains("video"))
					{
						YouTubeMediaTrackVideo video = ParseVideoTrackItem(jFormat, mimeType);
						mediaTracks.AddLast(video);
					}
					else if (mimeType.Contains("audio"))
					{
						JToken jt = jFormat.Value<JToken>("isDrc");
						bool isDrc = jt != null && jt.Value<bool>();
						if (!isDrc) // Временно игнорируем дорожки с пометкой "DRC (Dynamic range compression)".
						{
							YouTubeMediaTrackAudio audio = ParseAudioTrackItem(jFormat, mimeType);
							mediaTracks.AddLast(audio);
						}
					}
					else
					{
						System.Diagnostics.Debug.WriteLine($"Warning! Unknown MIME type!");
					}
				}
			}

			JArray jaFormats = streamingData.GetFormats();
			if (jaFormats != null)
			{
				foreach (JObject jFormat in jaFormats.Cast<JObject>())
				{
					YouTubeMediaTrackContainer container = ParseContainerTrackItem(jFormat);
					mediaTracks.AddLast(container);
				}
			}

			return new YouTubeMediaFormatList(mediaTracks, streamingData.Client, streamingData.UrlDecryptionData);
		}

		private static YouTubeMediaTrackVideo ParseVideoTrackItem(JObject jFormatItem, string mimeTypeRaw)
		{
			int formatId = jFormatItem.Value<int>("itag");
			ParseMime(mimeTypeRaw, out string mimeCodecs, out string mimeExt);
			string fileExtension = !string.IsNullOrEmpty(mimeExt) && !string.IsNullOrWhiteSpace(mimeExt) ?
				(mimeExt.ToLower() == "mp4" ? "m4v" : "webm") : "dat";
			int bitrate = jFormatItem.Value<int>("bitrate");
			int averageBitrate = jFormatItem.Value<int>("averageBitrate");
			int videoWidth = jFormatItem.Value<int>("width");
			int videoHeight = jFormatItem.Value<int>("height");
			string quality = jFormatItem.Value<string>("quality");
			string qualityLabel = jFormatItem.Value<string>("qualityLabel");
			int videoFrameRate = jFormatItem.Value<int>("fps");
			string projectionType = jFormatItem.Value<string>("projectionType");
			string lastModified = jFormatItem.Value<string>("lastModified");
			long contentLength = -1L;
			JToken jt = jFormatItem.Value<JToken>("contentLength");
			if (jt != null)
			{
				string contentLengthString = jt.Value<string>();
				if (!long.TryParse(contentLengthString, out contentLength))
				{
					contentLength = -1;
				}
			}
			int approxDurationMs = -1;
			bool isCiphered = false;
			string signatureCipherString = null;
			jt = jFormatItem.Value<JToken>("signatureCipher");
			if (jt != null)
			{
				signatureCipherString = jt.Value<string>();
				isCiphered = true;
			}
			string url = jFormatItem.Value<string>("url");

			YouTubeMediaTrackUrl trackUrl = new YouTubeMediaTrackUrl(url, signatureCipherString);

			YouTubeMediaTrackVideo video = new YouTubeMediaTrackVideo(
				formatId, videoWidth, videoHeight, videoFrameRate, bitrate, averageBitrate,
				lastModified, contentLength, quality, qualityLabel, approxDurationMs,
				projectionType, trackUrl,
				mimeTypeRaw, mimeExt, mimeCodecs, fileExtension, isCiphered);
			return video;
		}

		private static YouTubeMediaTrackAudio ParseAudioTrackItem(JObject jFormatItem, string mimeTypeRaw)
		{
			int formatId = jFormatItem.Value<int>("itag");
			ParseMime(mimeTypeRaw, out string mimeCodecs, out string mimeExt);
			string fileExtension = !string.IsNullOrEmpty(mimeExt) && !string.IsNullOrWhiteSpace(mimeExt) ?
				(mimeExt.ToLower() == "mp4" ? "m4a" : "weba") : "dat";
			int bitrate = jFormatItem.Value<int>("bitrate");
			int averageBitrate = jFormatItem.Value<int>("averageBitrate");
			string quality = jFormatItem.Value<string>("quality");
			string qualityLabel = jFormatItem.Value<string>("qualityLabel");
			string lastModified = jFormatItem.Value<string>("lastModified");
			long contentLength = -1L;
			JToken jt = jFormatItem.Value<JToken>("contentLength");
			if (jt != null)
			{
				string contentLengthString = jt.Value<string>();
				if (!long.TryParse(contentLengthString, out contentLength))
				{
					contentLength = -1;
				}
			}
			int approxDurationMs = -1;
			bool isCiphered = false;
			string signatureCipherString = null;
			jt = jFormatItem.Value<JToken>("signatureCipher");
			if (jt != null)
			{
				signatureCipherString = jt.Value<string>();
				isCiphered = true;
			}
			string url = jFormatItem.Value<string>("url");

			YouTubeMediaTrackUrl trackUrl = new YouTubeMediaTrackUrl(url, signatureCipherString);

			string audioQuality = jFormatItem.Value<string>("audioQuality");
			if (!int.TryParse(jFormatItem.Value<string>("audioSampleRate"), out int audioSampleRate))
			{
				audioSampleRate = -1;
			}
			int audioChannelCount = jFormatItem.Value<int>("audioChannels");
			double loudnessDb = jFormatItem.Value<double>("loudnessDb");

			YouTubeMediaTrackAudio audio = new YouTubeMediaTrackAudio(
				formatId, bitrate, averageBitrate, lastModified, contentLength,
				quality, qualityLabel, audioQuality, audioSampleRate,
				audioChannelCount, loudnessDb, approxDurationMs, trackUrl,
				mimeTypeRaw, mimeExt, mimeCodecs, fileExtension, isCiphered);
			return audio;
		}

		private static YouTubeMediaTrackContainer ParseContainerTrackItem(JObject jFormatItem)
		{
			string mimeType = jFormatItem.Value<string>("mimeType");
			ParseMime(mimeType, out string mimeCodecs, out string mimeExt);
			string fileExtension = !string.IsNullOrEmpty(mimeExt) && !string.IsNullOrWhiteSpace(mimeExt) ?
				mimeExt.ToLower() : "mp4"; // Не исключено, что для некоторых видео это может быть не правильно.
			int formatId = jFormatItem.Value<int>("itag");
			int bitrate = jFormatItem.Value<int>("bitrate");
			int averageBitrate = jFormatItem.Value<int>("averageBitrate");
			int videoWidth = jFormatItem.Value<int>("width");
			int videoHeight = jFormatItem.Value<int>("height");
			string quality = jFormatItem.Value<string>("quality");
			string qualityLabel = jFormatItem.Value<string>("qualityLabel");
			int videoFrameRate = jFormatItem.Value<int>("fps");
			string projectionType = jFormatItem.Value<string>("projectionType");
			string lastModified = jFormatItem.Value<string>("lastModified");
			long contentLength = -1L;
			JToken jt = jFormatItem.Value<JToken>("contentLength");
			if (jt != null)
			{
				string contentLengthString = jt.Value<string>();
				if (!long.TryParse(contentLengthString, out contentLength))
				{
					contentLength = -1;
				}
			}
			string audioQuality = jFormatItem.Value<string>("audioQuality");
			jt = jFormatItem.Value<JToken>("audioSampleRate");
			int audioSampleRate = jt != null ? int.Parse(jt.Value<string>()) : -1;
			jt = jFormatItem.Value<JToken>("audioChannels");
			int audioChannelCount = jt != null ? int.Parse(jt.Value<string>()) : -1;
			jt = jFormatItem.Value<JToken>("approxDurationMs");
			int approxDurationMs = jt != null ? int.Parse(jt.Value<string>()) : -1;
			bool isCiphered = false;
			string signatureCipherString = null;
			jt = jFormatItem.Value<JToken>("signatureCipher");
			if (jt != null)
			{
				signatureCipherString = jt.Value<string>();
				isCiphered = true;
			}
			string url = jFormatItem.Value<string>("url");

			YouTubeMediaTrackUrl trackUrl = new YouTubeMediaTrackUrl(url, signatureCipherString);

			YouTubeMediaTrackContainer container = new YouTubeMediaTrackContainer(
				formatId, videoWidth, videoHeight, videoFrameRate, bitrate, averageBitrate,
				lastModified, contentLength, quality, qualityLabel,
				audioQuality, audioSampleRate, audioChannelCount, approxDurationMs,
				projectionType, trackUrl,
				mimeType, mimeExt, mimeCodecs, fileExtension, isCiphered);
			return container;
		}

		private static void ParseMime(string mime, out string codecs, out string mimeExt)
		{
			string[] t = mime.Split(';', '/', '=');
			codecs = t.Length > 3 ?
				(!string.IsNullOrEmpty(t[3]) && !string.IsNullOrWhiteSpace(t[3]) ?
					t[3].Replace("\"", string.Empty) : null) :
				null;
			mimeExt = t.Length > 1 ? t[1] : null;
		}
	}
}
