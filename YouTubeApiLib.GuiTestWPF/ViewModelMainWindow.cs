using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MultiThreadedDownloaderLib;
using static YouTubeApiLib.GuiTestWPF.Utils;

namespace YouTubeApiLib.GuiTestWPF
{
	internal class ViewModelMainWindow : Notifier
	{
		public ObservableCollection<ModelYouTubeMediaTrackWrapper> MediaTracks { get; }
		public string VideoId { get => _videoId; set => SetProperty(ref _videoId, value); }
		public BitmapImage VideoThumbnail { get => _thumbnail; set => SetProperty(ref _thumbnail, value); }
		public string VideoTitle { get => _videoTitle; set => SetProperty(ref _videoTitle, value); }
		public string VideoChannelOwnerTitle { get => _videoChannelOwnerTitle; set => SetProperty(ref _videoChannelOwnerTitle, value); }
		public string VideoPublishDateFormatted { get => _videoPublishDateFormatted; set => SetProperty(ref _videoPublishDateFormatted, value); }
		public string ApiClientId { get => _apiClientId; set { SetProperty(ref _apiClientId, value); RaisePropertyChanged(nameof(FormattedApiClientId)); } }
		public bool IsVideoSearching { get => _isVideoSearching; set { SetProperty(ref _isVideoSearching, value);
			ResultVisibility = IsVideoSearching || _video == null ? Visibility.Hidden : Visibility.Visible; } }
		public bool IsFormatListSearching { get => _isFormatListSearching; set => SetProperty(ref _isFormatListSearching, value); }
		public Visibility ResultVisibility { get => _resultVisibility; set => SetProperty(ref _resultVisibility, value); }
		public bool UseProxyServer { get => _useProxyServer; set => SetProperty(ref _useProxyServer, value); }
		public string ProxyServerAddress { get => _proxyServerAddress; set => SetProperty(ref _proxyServerAddress, value); }
		public int ProxyServerPort { get => _proxyServerPort; set => SetProperty(ref _proxyServerPort, value); }
		public string CookieInput { get => _cookieInput; set => SetProperty(ref _cookieInput, value); }
		public string FormattedApiClientId => $"Client ID: {(!string.IsNullOrEmpty(ApiClientId) ? ApiClientId : "<undefined>")}";

		private string _videoId;
		private BitmapImage _thumbnail;
		private string _videoTitle;
		private string _videoChannelOwnerTitle;
		private string _videoPublishDateFormatted;
		private string _apiClientId;
		private bool _isVideoSearching;
		private bool _isFormatListSearching;
		private Visibility _resultVisibility = Visibility.Hidden;
		private bool _useProxyServer;
		private string _proxyServerAddress = "127.0.0.1";
		private int _proxyServerPort = 12345;
		private string _cookieInput;
		private CookieContainer _cookies;
		private YouTubeVideo _video;

		public ICommand CommandFindVideo { get; }
		public ICommand CommandApplyCookies { get; }
		public ICommand CommandCopyThumbnailUrl { get; }
		public ICommand CommandOpenThumbnailInBrowser { get; }
		public ICommand CommandOpenVideoInBrowser { get; }
		public ICommand CommandCopyVideoTitle { get; }
		public ICommand CommandMediaTrackListLeftDoubleClick { get; }
		public ICommand CommandUpdateFormatList { get; }

		public ViewModelMainWindow()
		{
			MediaTracks = new ObservableCollection<ModelYouTubeMediaTrackWrapper>();
			CommandApplyCookies = new LambdaCommand(obj => _cookies = GetCookieContainer(CookieInput));
			CommandFindVideo = new LambdaCommand(async obj =>
			{
				if (!IsVideoSearching)
				{
					if (string.IsNullOrEmpty(VideoId) || string.IsNullOrWhiteSpace(VideoId))
					{
						MessageBox.Show("Введите ссылку на видео или его ID!", "Ошибка!",
							MessageBoxButton.OK, MessageBoxImage.Warning);
						return;
					}

					YouTubeVideoId videoId = YouTubeApiLib.Utils.ExtractVideoIdFromUrl(VideoId);
					if (videoId == null)
					{
						MessageBox.Show("Не удалось извлечь ID видео!", "Ошибка!",
							MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					if (UseProxyServer && (string.IsNullOrEmpty(ProxyServerAddress) || string.IsNullOrWhiteSpace(ProxyServerAddress)))
					{
						MessageBox.Show("Не указан адрес прокси-сервера!", "Ошибка!",
							MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					IsVideoSearching = true;
					MediaTracks.Clear();
					_video = await Task.Run(() =>
					{
						FileDownloader d = new FileDownloader() { Cookies = _cookies };
						if (UseProxyServer)
						{
							d.Proxy = new WebProxy(ProxyServerAddress, ProxyServerPort);
						}
						return YouTubeVideo.GetById(videoId.Id, d);
					});
					if (_video != null)
					{
						if (_video.Status.ErrorCode == 200)
						{
							VideoTitle = _video.Title;
							VideoChannelOwnerTitle = $"Канал: {_video.OwnerChannelTitle}";
							VideoPublishDateFormatted = FormatDateTime(_video.DatePublished);
							VideoThumbnail = _video.Thumbnails != null && _video.Thumbnails.Count > 0 ?
								await Task.Run(() =>
								{
									WebProxy proxy = UseProxyServer ? new WebProxy(ProxyServerAddress, ProxyServerPort) : null;
									return DownloadImage(_video.Thumbnails[0].Url, proxy);
								}) : null;

							if (_video.MediaTracks.Count > 0)
							{
								string[] keys = _video.MediaTracks.Keys.ToArray();
								ApiClientId = keys[0];
								foreach (YouTubeMediaTrack track in _video.MediaTracks[ApiClientId].Tracks)
								{
									MediaTracks.Add(new ModelYouTubeMediaTrackWrapper(track));
								}
							}
						}
						else
						{
							VideoTitle = $"{_video.Status.Status}, {_video.Status.Reason}";
							VideoChannelOwnerTitle = "Канал: <Недоступно>";
							VideoPublishDateFormatted = "Дата публикации: <Недоступно>";
							ApiClientId = null;
							VideoThumbnail = await Task.Run(() => DownloadImage(_video.Status.ThumbnailUrl));
						}
					}
					else
					{
						MessageBox.Show("Ошибка поиска!", "Ошибка!",
							MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}

				IsVideoSearching = false;
			}, obj => !IsVideoSearching);
			CommandCopyThumbnailUrl = new LambdaCommand(obj =>
			{
				try
				{
					if (_video != null)
					{
						string url = _video.IsInfoAvailable ? _video.Thumbnails[0].Url : _video.Status.ThumbnailUrl;
						Clipboard.SetText(url);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			});
			CommandOpenThumbnailInBrowser = new LambdaCommand(obj =>
			{
				try
				{
					string url = _video.IsInfoAvailable ? _video.Thumbnails[0].Url : _video.Status.ThumbnailUrl;
					OpenUrl(url);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			});
			CommandOpenVideoInBrowser = new LambdaCommand(obj =>
			{
				try
				{
					if (_video != null) { OpenUrl(_video.Url); }
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			});
			CommandCopyVideoTitle = new LambdaCommand(obj =>
			{
				try
				{
					if (!string.IsNullOrEmpty(_video?.Title)) { Clipboard.SetText(_video.Title); }
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			});
			CommandUpdateFormatList = new LambdaCommand(async obj =>
			{
				if (UseProxyServer && (string.IsNullOrEmpty(ProxyServerAddress) || string.IsNullOrWhiteSpace(ProxyServerAddress)))
				{
					MessageBox.Show("Не указан адрес прокси-сервера!", "Ошибка!",
						MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				IsFormatListSearching = true;
				MediaTracks.Clear();
				ApiClientId = null;
				YouTubeMediaFormatList formatList = await Task.Run(() =>
				{
					IYouTubeClient client = new YouTubeClientAndroidVr()
					{
						Downloader = new FileDownloader() { Cookies = _cookies }
					};
					if (UseProxyServer)
					{
						client.Downloader.Proxy = new WebProxy(ProxyServerAddress, ProxyServerPort);
					}
					YouTubeStreamingDataResult streamingDataResult = YouTubeStreamingData.Get(_video.Id, client);
					client.Downloader.Dispose();
					return streamingDataResult.ErrorCode == 200 ? streamingDataResult.Data.Parse() : null;
				});
				if (formatList != null)
				{
					foreach (YouTubeMediaTrack track in formatList.Tracks)
					{
						MediaTracks.Add(new ModelYouTubeMediaTrackWrapper(track));
					}
				}
				ApiClientId = formatList?.Client.DisplayName;
				IsFormatListSearching = false;
			}, obj => !IsFormatListSearching && !IsVideoSearching);
			CommandMediaTrackListLeftDoubleClick = new LambdaCommand(obj =>
			{
				try
				{
					ModelYouTubeMediaTrackWrapper trackWrapper = obj as ModelYouTubeMediaTrackWrapper;
					OpenUrl(trackWrapper.BaseTrack.FileUrl.Url);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}, obj => MediaTracks.Count > 0);
		}
	}
}
