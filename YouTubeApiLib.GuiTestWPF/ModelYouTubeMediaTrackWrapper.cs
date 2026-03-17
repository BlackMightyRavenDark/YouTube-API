
namespace YouTubeApiLib.GuiTestWPF
{
	public class ModelYouTubeMediaTrackWrapper
	{
		public YouTubeMediaTrack BaseTrack { get; }
		public string Type { get; }
		public string FormatId { get; }
		public string Resolution { get; }
		public string FrameRate { get; }
		public long FileSize => BaseTrack.ContentLength;
		public string RowBackgroundColor { get; }
		public string Language { get; }

		public string FormattedFileSize => FileSize >= 0L ? FileSize.ToString() : string.Empty;

		public ModelYouTubeMediaTrackWrapper(YouTubeMediaTrack baseTrack)
		{
			BaseTrack = baseTrack;
			Type = FormatItemType();
			FormatId = FormatFormatId();
			Resolution = FormatResolution();
			FrameRate = FormatFrameRate();
			Language = FormatLanguage();
			RowBackgroundColor = GetRowBackgroundColor();
		}

		private string FormatItemType()
		{
			if (BaseTrack is YouTubeMediaTrackContainer)
			{
				return "Container";
			}
			else if (BaseTrack is YouTubeMediaTrackAudio)
			{
				return "Audio";
			}
			else if (BaseTrack is YouTubeMediaTrackVideo)
			{
				return "Video";
			}
			else
			{
				return "Unknown";
			}
		}

		private string FormatFormatId()
		{
			string t = BaseTrack.FormatId.ToString();
			return (BaseTrack is YouTubeMediaTrackAudio audio) && audio.IsDynamicRangeCompression ? $"{t}-DRC" : t;
		}

		private string FormatResolution()
		{
			if (BaseTrack is YouTubeMediaTrackVideo video)
			{
				return $"{video.VideoWidth}x{video.VideoHeight}";
			}
			else if (BaseTrack is YouTubeMediaTrackContainer container)
			{
				return $"{container.VideoWidth}x{container.VideoHeight}";
			}

			return null;
		}

		private string FormatFrameRate()
		{
			if (BaseTrack is YouTubeMediaTrackVideo video)
			{
				return $"{video.FrameRate} FPS";
			}
			else if (BaseTrack is YouTubeMediaTrackContainer container)
			{
				return $"{container.VideoFrameRate} FPS";
			}

			return null;
		}

		private string GetRowBackgroundColor()
		{
			if (BaseTrack is YouTubeMediaTrackContainer)
			{
				return "Orange";
			}
			else if (BaseTrack is YouTubeMediaTrackAudio)
			{
				return "LightSkyBlue";
			}
			else if (BaseTrack is YouTubeMediaTrackVideo)
			{
				return "LimeGreen";
			}
			else
			{
				return "LightGray";
			}
		}

		private string FormatLanguage()
		{
			return (BaseTrack is YouTubeMediaTrackAudio audio) ? audio.Language?.DisplayName : null;
		}
	}
}
