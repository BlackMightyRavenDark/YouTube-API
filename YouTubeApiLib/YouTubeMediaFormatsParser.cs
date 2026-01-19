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
			if (streamingData?.RawData == null) { return null; }

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
				var tracks = ParseFormatList(jaAdaptiveFormats, true);
				foreach (YouTubeMediaTrack track in tracks)
				{
					mediaTracks.AddLast(track);
				}
			}

			JArray jaFormats = streamingData.GetFormats();
			if (jaFormats != null)
			{
				var tracks = ParseFormatList(jaFormats, false);
				if (tracks != null)
				{
					foreach (YouTubeMediaTrack track in tracks)
					{
						mediaTracks.AddLast(track);
					}
				}
			}

			return new YouTubeMediaFormatList(mediaTracks, streamingData.Client, streamingData.DateReceived,
				streamingData.UrlDecryptionData, streamingData.RawData);
		}

		private static IEnumerable<YouTubeMediaTrack> ParseFormatList(JArray jaFormats, bool isAdaptive)
		{
			foreach (JObject jFormat in jaFormats.Cast<JObject>())
			{
				string mimeType = jFormat.Value<string>("mimeType");
				if (string.IsNullOrEmpty(mimeType) || string.IsNullOrWhiteSpace(mimeType))
				{
#if DEBUG
					System.Diagnostics.Debug.WriteLine("The \"mimeType\" field read error!");
#endif
					continue;
				}

				if (mimeType.Contains("video"))
				{
					YouTubeMediaTrack track = ParseMediaTrackItem(jFormat, mimeType, isAdaptive ? "video" : "container");
					if (track != null) { yield return track; }
				}
				else if (mimeType.Contains("audio"))
				{
					YouTubeMediaTrack track = ParseMediaTrackItem(jFormat, mimeType, "audio");
					if (track != null) { yield return track; }
				}
#if DEBUG
				else
				{
					System.Diagnostics.Debug.WriteLine("Warning! Unknown MIME type!");
				}
#endif
			}
		}

		private static YouTubeMediaTrack ParseMediaTrackItem(JObject jFormatItem, string mimeType, string trackType)
		{
			ParseMime(mimeType, out string mimeCodecs, out string mimeExt);

			int formatId = jFormatItem.Value<int>("itag");
			int bitrate = jFormatItem.Value<int>("bitrate");
			int averageBitrate = jFormatItem.Value<int>("averageBitrate");
			string quality = jFormatItem.Value<string>("quality");
			string qualityLabel = jFormatItem.Value<string>("qualityLabel");
			string lastModified = jFormatItem.Value<string>("lastModified");
			long contentLength = -1L;
			JToken jtLength = jFormatItem.Value<JToken>("contentLength");
			if (jtLength != null)
			{
				string contentLengthString = jtLength.Value<string>();
				if (!long.TryParse(contentLengthString, out contentLength))
				{
					contentLength = -1;
				}
			}
			JToken jtApproxDurationMs = jFormatItem.Value<JToken>("approxDurationMs");
			int approxDurationMs = jtApproxDurationMs != null ? int.Parse(jtApproxDurationMs.Value<string>()) : -1;
			bool isCiphered = false;
			string signatureCipherString = null;
			JToken jtCipher = jFormatItem.Value<JToken>("signatureCipher");
			if (jtCipher != null)
			{
				signatureCipherString = jtCipher.Value<string>();
				isCiphered = true;
			}
			string url = jFormatItem.Value<string>("url");

			YouTubeMediaTrackUrl trackUrl = new YouTubeMediaTrackUrl(url, signatureCipherString);

			string audioQuality = trackType == "audio" || trackType == "container" ? jFormatItem.Value<string>("audioQuality") : null;
			int audioChannelCount = trackType == "audio" || trackType == "container" ? jFormatItem.Value<int>("audioChannels") : -1;
			int audioSampleRate = -1;
			if ((trackType == "audio" || trackType == "container") &&
				!int.TryParse(jFormatItem.Value<string>("audioSampleRate"), out audioSampleRate))
			{
				audioSampleRate = -1;
			}

			switch (trackType)
			{
				case "video":
				case "container":
					{
						string fileExtension = !string.IsNullOrEmpty(mimeExt) && !string.IsNullOrWhiteSpace(mimeExt) ?
							(trackType == "video" ? (mimeExt.ToLower() == "mp4" ? "m4v" : "webm") : mimeExt) : "dat";
						int videoWidth = jFormatItem.Value<int>("width");
						int videoHeight = jFormatItem.Value<int>("height");
						int videoFrameRate = jFormatItem.Value<int>("fps");
						string projectionType = jFormatItem.Value<string>("projectionType");

						if (trackType == "video")
						{
							return new YouTubeMediaTrackVideo(
								formatId, videoWidth, videoHeight, videoFrameRate, bitrate, averageBitrate,
								lastModified, contentLength, quality, qualityLabel, approxDurationMs,
								projectionType, trackUrl,
								mimeType, mimeExt, mimeCodecs, fileExtension, isCiphered);
						}

						return new YouTubeMediaTrackContainer(
							formatId, videoWidth, videoHeight, videoFrameRate, bitrate, averageBitrate,
							lastModified, contentLength, quality, qualityLabel, audioQuality, audioSampleRate,
							audioChannelCount, approxDurationMs, projectionType, trackUrl,
							mimeType, mimeExt, mimeCodecs, fileExtension, isCiphered);
					}

				case "audio":
					{
						string fileExtension = !string.IsNullOrEmpty(mimeExt) && !string.IsNullOrWhiteSpace(mimeExt) ?
							(mimeExt.ToLower() == "mp4" ? "m4a" : "weba") : "dat";
						bool isDrc = jFormatItem.Value<bool>("isDrc");
						bool isVoiceBoosted = jFormatItem.Value<bool>("isVb");
						double loudnessDb = jFormatItem.Value<double>("loudnessDb");
						YouTubeAudioTrackLanguage language = null;
						if (jFormatItem.ContainsKey("audioTrack"))
						{
							JObject j = jFormatItem.Value<JObject>("audioTrack");
							language = new YouTubeAudioTrackLanguage(
								j.Value<string>("displayName"),
								j.Value<string>("id"),
								j.Value<bool>("audioIsDefault"),
								j.Value<bool>("isAutoDubbed"));
						}

						return new YouTubeMediaTrackAudio(
							formatId, bitrate, averageBitrate, lastModified, contentLength,
							quality, qualityLabel, audioQuality, audioSampleRate, audioChannelCount,
							isDrc, isVoiceBoosted, language, loudnessDb, approxDurationMs, trackUrl,
							mimeType, mimeExt, mimeCodecs, fileExtension, isCiphered);
					}
			}

			return null;
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
