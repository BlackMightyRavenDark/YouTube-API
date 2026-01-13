using System;
using System.Collections.Generic;
using System.Linq;

namespace YouTubeApiLib
{
	public class YouTubeMediaFormatList
	{
		public List<YouTubeMediaTrack> Tracks { get; }
		public IYouTubeClient Client { get; }

		/// <summary>
		/// Дата и время получение информации.
		/// </summary>
		public DateTime DateReceived { get; }

		public YouTubeMediaTrackUrlDecryptionData UrlDecryptionData { get; }
		public string RawData { get; }

		public YouTubeMediaFormatList(IEnumerable<YouTubeMediaTrack> tracks,
			IYouTubeClient client, DateTime dateReceived,
			YouTubeMediaTrackUrlDecryptionData urlDecryptionData,
			string rawData)
		{
			Tracks = tracks.ToList();
			Client = client;
			DateReceived = dateReceived;
			UrlDecryptionData = urlDecryptionData;
			RawData = rawData;
		}
	}
}
