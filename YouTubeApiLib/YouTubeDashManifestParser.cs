using System.Collections.Generic;
using System.Xml;

namespace YouTubeApiLib
{
	public class YouTubeDashManifestParser
	{
		public string DashManifest { get; }
		public string DashManifestUrl { get; }

		public YouTubeDashManifestParser(string dashManifest, string dashManifestUrl)
		{
			DashManifest = dashManifest;
			DashManifestUrl = dashManifestUrl;
		}

		public LinkedList<YouTubeMediaTrack> Parse()
		{
			if (string.IsNullOrEmpty(DashManifest) || string.IsNullOrWhiteSpace(DashManifest))
			{
				return null;
			}

			XmlDocument xml = new XmlDocument();
			xml.LoadXml(DashManifest);
			XmlNode nodePeriod = xml.DocumentElement.SelectSingleNode("//*[local-name()='Period']");
			if (nodePeriod == null)
			{
				return null;
			}

			LinkedList<YouTubeMediaTrack> resList = new LinkedList<YouTubeMediaTrack>();

			foreach (XmlNode nodeAdaptationSet in nodePeriod)
			{
				if (nodeAdaptationSet == null || nodeAdaptationSet.Name != "AdaptationSet")
				{
					continue;
				}
				XmlAttribute attrMimeType = nodeAdaptationSet.Attributes["mimeType"];
				if (attrMimeType == null)
				{
					continue;
				}

				string mimeType = attrMimeType.Value;
				string[] mimeTypeSplitted = mimeType.Split('/');
				if (mimeTypeSplitted == null || mimeTypeSplitted.Length < 2)
				{
					continue;
				}
				string splittedMimeTypeLowecased = mimeTypeSplitted[0].ToLower();
				if (splittedMimeTypeLowecased == "video")
				{
					foreach (XmlNode node in nodeAdaptationSet)
					{
						if (node.Name == "Representation")
						{
							string formatIdString = node.Attributes["id"]?.Value;
							if (!int.TryParse(formatIdString, out int formatId))
							{
								formatId = 0;
							}
							string videoWidthString = node.Attributes["width"]?.Value;
							if (!int.TryParse(videoWidthString, out int videoWidth))
							{
								videoWidth = 0;
							}
							string videoHeightString = node.Attributes["height"]?.Value;
							if (!int.TryParse(videoHeightString, out int videoHeight))
							{
								videoHeight = 0;
							}
							string videoFrameRateString = node.Attributes["frameRate"]?.Value;
							if (!int.TryParse(videoFrameRateString, out int videoFrameRate))
							{
								videoFrameRate = 0;
							}
							string videoBitrateString = node.Attributes["bandwidth"]?.Value;
							if (!int.TryParse(videoBitrateString, out int videoBitrate))
							{
								videoBitrate = 0;
							}
							string videoCodecs = node.Attributes["codecs"]?.Value;

							string mimeExtLowerCased = mimeTypeSplitted[1].ToLower();
							string fileExtension = mimeExtLowerCased == "mp4" ? "m4v" : mimeExtLowerCased;

							XmlNode nodeBaseUrl = node.SelectSingleNode("./*[local-name()='BaseURL']");
							if (nodeBaseUrl == null)
							{
#if DEBUG
								System.Diagnostics.Debug.WriteLine("ERROR! The \"nodeBaseUrl\" is NULL!");
#endif
								continue;
							}
							string baseUrl = nodeBaseUrl.InnerText;

							XmlNode nodeSegmentList = node.SelectSingleNode("./*[local-name()='SegmentList']");
							if (nodeSegmentList == null)
							{
#if DEBUG
								System.Diagnostics.Debug.WriteLine("ERROR! The \"nodeSegmentList\" is NULL!");
#endif
								continue;
							}

							List<string> dashChunkUrls = new List<string>();

							XmlNode nodeInitialization = nodeSegmentList.SelectSingleNode("./*[local-name()='Initialization']");
							if (nodeInitialization != null)
							{
								XmlAttribute attrSourceUrl = nodeInitialization.Attributes["sourceURL"];
								if (attrSourceUrl != null)
								{
									dashChunkUrls.Add(attrSourceUrl.Value);
								}
							}
#if DEBUG
							else
							{
								System.Diagnostics.Debug.WriteLine("Warning! The \"nodeInitialization\" is NULL! The first track chunk may be lost!");
							}
#endif
							foreach (XmlNode nodeSegment in nodeSegmentList)
							{
								if (nodeSegment.Name == "SegmentURL")
								{
									string segmentUrlPartial = nodeSegment.Attributes["media"]?.Value;
									dashChunkUrls.Add(segmentUrlPartial);
								}
							}
							YouTubeDashUrlList dashUrlList = new YouTubeDashUrlList(baseUrl, dashChunkUrls);

							YouTubeMediaTrack video = new YouTubeMediaTrackVideo(
								formatId, videoWidth, videoHeight, videoFrameRate, videoBitrate,
								mimeType, mimeExtLowerCased, videoCodecs, fileExtension,
								DashManifestUrl, dashUrlList);
							resList.AddLast(video);
						}
					}
				}
				else if (splittedMimeTypeLowecased == "audio")
				{
					foreach (XmlNode node in nodeAdaptationSet)
					{
						if (node.Name == "Representation")
						{
							string formatIdString = node.Attributes["id"]?.Value;
							if (!int.TryParse(formatIdString, out int formatId))
							{
								formatId = 0;
							}
							string audioSamplingRateString = node.Attributes["audioSamplingRate"]?.Value;
							if (!int.TryParse(audioSamplingRateString, out int audioSampleRate))
							{
								audioSampleRate = 0;
							}
							string audioBitrateString = node.Attributes["bandwidth"]?.Value;
							if (!int.TryParse(audioBitrateString, out int audioBitrate))
							{
								audioBitrate = 0;
							}
							string audioCodecs = node.Attributes["codecs"]?.Value;

							int audioChannelCount;
							XmlNode nodeAudio = FindChildNodeByName(node, "AudioChannelConfiguration");
							if (nodeAudio != null)
							{
								string val = nodeAudio.Attributes["value"]?.Value;
								if (!int.TryParse(val, out audioChannelCount))
								{
									audioChannelCount = -1;
								}
							}
							else
							{
								audioChannelCount = -1;
							}

							string mimeExtLowerCased = mimeTypeSplitted[1].ToLower();
							string fileExtension;
							if (mimeExtLowerCased == "mp4")
							{
								fileExtension = "m4a";
							}
							else if (mimeExtLowerCased == "webm")
							{
								fileExtension = "weba";
							}
							else
							{
								fileExtension = mimeExtLowerCased;
							}

							XmlNode nodeBaseUrl = node.SelectSingleNode("./*[local-name()='BaseURL']");
							if (nodeBaseUrl == null)
							{
#if DEBUG
								System.Diagnostics.Debug.WriteLine("ERROR! The \"nodeBaseUrl\" is NULL!");
#endif
								continue;
							}
							string baseUrl = nodeBaseUrl.InnerText;

							XmlNode nodeSegmentList = node.SelectSingleNode("./*[local-name()='SegmentList']");
							if (nodeSegmentList == null)
							{
#if DEBUG
								System.Diagnostics.Debug.WriteLine("ERROR! The \"nodeSegmentList\" is NULL!");
#endif
								continue;
							}

							List<string> dashChunkUrls = new List<string>();

							XmlNode nodeInitialization = nodeSegmentList.SelectSingleNode("./*[local-name()='Initialization']");
							if (nodeInitialization != null)
							{
								XmlAttribute attrSourceUrl = nodeInitialization.Attributes["sourceURL"];
								if (attrSourceUrl != null)
								{
									dashChunkUrls.Add(attrSourceUrl.Value);
								}
							}
#if DEBUG
							else
							{
								System.Diagnostics.Debug.WriteLine("Warning! The \"nodeInitialization\" is NULL! The first track chunk may be lost!");
							}
#endif
							foreach (XmlNode nodeSegment in nodeSegmentList)
							{
								if (nodeSegment.Name == "SegmentURL")
								{
									string segmentUrl = nodeSegment.Attributes["media"]?.Value;
									dashChunkUrls.Add(segmentUrl);
								}
							}
							YouTubeDashUrlList dashUrlList = new YouTubeDashUrlList(baseUrl, dashChunkUrls);

							YouTubeMediaTrack audio = new YouTubeMediaTrackAudio(
								formatId, audioBitrate, audioSampleRate, audioChannelCount,
								mimeType, mimeExtLowerCased, audioCodecs, fileExtension,
								DashManifestUrl, dashUrlList);
							resList.AddLast(audio);
						}
					}
				}
			}

			return resList;
		}

		private static XmlNode FindChildNodeByName(XmlNode parentNode, string nodeName)
		{
			foreach (XmlNode node in parentNode.ChildNodes)
			{
				if (node.Name == nodeName)
				{
					return node;
				}
			}
			return null;
		}
	}
}
