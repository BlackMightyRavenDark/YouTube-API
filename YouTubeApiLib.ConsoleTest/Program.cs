using System;
using static YouTubeApiLib.Utils;

namespace YouTubeApiLib.ConsoleTest
{
	internal class Program
	{
		static void Main(string[] args)
		{
			// 11 HOURS of 4K Underwater Wonders + Relaxing Music - Coral Reefs & Colorful Sea Life in UHD
			// 2160p | 30 fps | 78583477005 bytes
			string videoUrl = "https://www.youtube.com/watch?v=843Rpqza_6o";

			// Magical Underwater World 4K
			// 2160p | 24 fps | 7304448660 bytes
			//string videoUrl = "https://www.youtube.com/watch?v=7szcXCT-Oqw";

			// Ускорь разработку с помощью Chrome DevTools | Фишки Chrome DevTools о которых ТЫ НЕ ЗНАЛ | FRONTEND
			// 2160p | HDR | 60 fps | DRC audio | 1253582837 bytes
			//string videoUrl = "https://www.youtube.com/watch?v=H3_M9KASsHY";

			// Moving to Phoenix Arizona?
			// 1080p | 24 fps | Multilingual | 232686490 bytes
			//string videoUrl = "https://www.youtube.com/watch?v=pXyRINEG044";

			// date night what's in my bag, removing trees, and celebrating mothers everywhere ep. 147
			// 18+ | 1080p | 60 fps | 951661536 bytes
			//string videoUrl = "https://www.youtube.com/watch?v=EUKa5G7TNI0";

			Console.WriteLine($"Video URL: {videoUrl}");

			YouTubeVideoId videoId = ExtractVideoIdFromUrl(videoUrl);

			if (videoId != null)
			{
				YouTubeVideo video = videoId.GetVideo();
				if (video != null)
				{
					// Выводимая информация может быть неточной, потому что API ютуба иногда выдаёт неверные данные!

					if (video.IsInfoAvailable)
					{
						Console.WriteLine($"Title: {video.Title}");
						Console.WriteLine($"ID: {video.Id}");
						Console.WriteLine($"URL: {video.Url}");
						if (video.SimplifiedInfo.IsMicroformatInfoAvailable)
						{
							Console.WriteLine($"Uploaded: {DateTimeToString(video.DateUploaded)}");
							Console.WriteLine($"Published: {DateTimeToString(video.DatePublished)}");
						}
						if (video.Length > TimeSpan.Zero)
						{
							Console.WriteLine($"Length: {video.Length}");
						}
						Console.WriteLine($"Channel title: {video.OwnerChannelTitle}");
						Console.WriteLine($"Channel ID: {video.OwnerChannelId}");
						Console.WriteLine($"Description: {video.Description}");
						Console.WriteLine($"View count: {video.ViewCount}");
						Console.WriteLine($"Category: {video.Category}");
						if (!video.IsLiveContent)
						{
							Console.WriteLine($"Short (aka Reel): {video.IsShortFormat}");
						}
						Console.WriteLine($"Private: {video.IsPrivate}");
						Console.WriteLine($"Unlisted: {video.IsUnlisted}");
						Console.WriteLine($"Family safe: {video.IsFamilySafe}");
						Console.WriteLine($"Live content: {video.IsLiveContent}");
						if (video.IsLiveContent)
						{
							Console.WriteLine($"Is broadcasting now: {video.IsLiveNow}");
							if (video.IsLiveNow)
							{
								Console.WriteLine($"HLS manifest URL: {video.HlsManifestUrl}");
							}
						}
						Console.WriteLine($"Is DASH manifest present: {video.IsDashed}");
						if (video.IsDashed)
						{
							Console.WriteLine($"DASH manifest URL: {video.DashManifestUrl}");
						}
						Console.Write("Thumbnails: ");
						if (video.Thumbnails != null && video.Thumbnails.Count > 0)
						{
							Console.WriteLine("");
							foreach (YouTubeVideoThumbnail videoThumbnail in video.Thumbnails)
							{
								Console.WriteLine(videoThumbnail);
							}
						}
						else
						{
							Console.WriteLine("null");
						}

						if (video.IsPlayable)
						{
							Console.Write("Receiving download URLs...");
							IYouTubeClient client = new YouTubeClientAndroidSdkless();
							video.UpdateMediaFormats(client);
							if (video.MediaTracks.Count > 0)
							{
								foreach (var dictItem in video.MediaTracks)
								{
									Console.WriteLine("");
									if (dictItem.Value.Client is YouTubeClientWebPage)
									{
										Console.WriteLine("Warning! Download URLs from the video web page are broken!");
									}

									string clientName = dictItem.Value.Client?.DisplayName ?? "unknown";
									Console.WriteLine($"Track list for client [{clientName}]:");
									foreach (YouTubeMediaTrack track in dictItem.Value.Tracks)
									{
										string info;
										if (track.GetType() == typeof(YouTubeMediaTrackVideo))
										{
											YouTubeMediaTrackVideo videoTrack = track as YouTubeMediaTrackVideo;
											string trackType = videoTrack.IsDashManifestPresent ? "DASH VIDEO" : "VIDEO";
											int bitrate = videoTrack.AverageBitrate > 0 ? videoTrack.AverageBitrate : videoTrack.Bitrate;
											info = $"{trackType} | ID {videoTrack.FormatId} | {videoTrack.VideoWidth}x{videoTrack.VideoHeight} | " +
												$"{videoTrack.FrameRate} fps | {videoTrack.FileExtension}";
											if (bitrate > 0)
											{
												info += $" | ~{bitrate / 1024} kb/s";
											}
											if (videoTrack.ContentLength > 0L)
											{
												info += $" | {videoTrack.ContentLength} bytes";
											}
										}
										else if (track.GetType() == typeof(YouTubeMediaTrackHlsStream))
										{
											YouTubeMediaTrackHlsStream hls = track as YouTubeMediaTrackHlsStream;
											info = $"HLS | ID {hls.FormatId} | {hls.VideoWidth}x{hls.VideoHeight} | " +
												$"{hls.FrameRate} fps | {hls.FileExtension}\nPlaylist URL: {hls.FileUrl}";
										}
										else if (track.GetType() == typeof(YouTubeMediaTrackAudio))
										{
											YouTubeMediaTrackAudio audioTrack = track as YouTubeMediaTrackAudio;
											string trackType = audioTrack.IsDashManifestPresent ? "DASH AUDIO" : "AUDIO";
											string formatIdString = audioTrack.IsDynamicRangeCompression ? $"{audioTrack.FormatId}-DRC" : audioTrack.FormatId.ToString();
											int bitrate = audioTrack.AverageBitrate > 0 ? audioTrack.AverageBitrate : audioTrack.Bitrate;
											info = $"{trackType} | ID {formatIdString} | {audioTrack.SampleRate} Hz | " +
												$"{audioTrack.ChannelCount} ch | {audioTrack.AudioQuality} | {audioTrack.FileExtension}";
											if (bitrate > 0)
											{
												info += $" | ~{bitrate / 1024} kb/s";
											}
											if (audioTrack.ContentLength > 0L)
											{
												info += $" | {audioTrack.ContentLength} bytes";
											}
											if (audioTrack.Language != null)
											{
												string languageString = $"{audioTrack.Language.Id} | {audioTrack.Language.DisplayName}";
												if (audioTrack.Language.IsDefault) { languageString += " | DEFAULT"; }
												info += $"{Environment.NewLine}Language: {languageString}";
											}
										}
										else if (track.GetType() == typeof(YouTubeMediaTrackContainer))
										{
											YouTubeMediaTrackContainer container = track as YouTubeMediaTrackContainer;
											info = $"CONTAINER | ID {container.FormatId} | {container.VideoWidth}x{container.VideoHeight} | " +
												$"{container.VideoFrameRate} fps | {container.FileExtension}";
										}
										else
										{
											Console.WriteLine("ERROR! Unknown track type!");
											continue;
										}
										Console.WriteLine(info);
										if (track.IsDashManifestPresent)
										{
											string dashChunkCountString = track.DashUrls != null ? track.DashUrls.Count.ToString() : "null";
											Console.WriteLine($"DASH chunk URL count: {dashChunkCountString}");
										}
										else if (track.GetType() != typeof(YouTubeMediaTrackHlsStream))
										{
											string url = track.FileUrl.ToString();
											if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url)) { url = "null"; }
											Console.WriteLine($"URL: {url}");

											if (track.FileUrl.SplitUrl() && track.FileUrl.QueryUrl.ContainsKey("n"))
											{
												Console.WriteLine($"'n'-param: {track.FileUrl.QueryUrl["n"]}");
											}
										}
									}
									YouTubeConfig youTubeConfig = dictItem.Value.UrlDecryptionData?.VideoWebPage.ExtractYouTubeConfig();
									string playerUrl = youTubeConfig != null ? youTubeConfig.PlayerUrl : "<Not available>";
									Console.WriteLine($"Player URL: {playerUrl}");
								}
							}
							else
							{
								Console.WriteLine("ERROR! No download URLs found!");
							}
						}
						else
						{
							Console.WriteLine("The video is unplayable!");
							Console.WriteLine($"Is private: {video.Status.IsPrivate}");
							Console.WriteLine($"Is adult: {video.Status.IsAdult}");
							Console.WriteLine($"Is login required: {video.Status.IsLoginRequired}");
							Console.WriteLine($"Is \"you're bot\" warning: {video.Status.IsBotWarning}");
							Console.WriteLine($"Status: {video.Status.Status}");
							Console.WriteLine($"Reason: {video.Status.Reason}");
							if (!string.IsNullOrEmpty(video.Status.ReasonDetails))
							{
								Console.WriteLine($"Reason details: {video.Status.ReasonDetails}");
							}

							string thumbnailUrl =
								string.IsNullOrEmpty(video.Status.ThumbnailUrl) ||
								string.IsNullOrWhiteSpace(video.Status.ThumbnailUrl) ?
								"null or empty" : video.Status.ThumbnailUrl;
							Console.WriteLine($"Thumbnail URL: {thumbnailUrl}");
						}
					}
					else
					{
						Console.WriteLine("Video info is unavailable!");
					}

					Console.Write("Type 'Y' to print full raw data or just press ENTER to exit...");
					string userAnswer = Console.ReadLine();
					if (userAnswer == "y" || userAnswer == "Y")
					{
						Console.Write("Raw info: ");
						Console.WriteLine(video.RawInfo != null ? $"\n{video.RawInfo}" : "null");
						Console.ReadLine();
					}
				}
				else
				{
					Console.WriteLine("ERROR! Video is null!");
					Console.ReadLine();
				}
			}
			else
			{
				Console.WriteLine("Video ID: <ERROR>");
				Console.ReadLine();
			}
		}

		private static string DateTimeToString(DateTime dateTime)
		{
			string s = dateTime.ToString("yyyy.MM.dd HH:mm:ss");
			return dateTime.Kind != DateTimeKind.Utc ? s : $"{s} GMT";
		}
	}
}
