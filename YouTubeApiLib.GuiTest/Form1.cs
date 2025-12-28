using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace YouTubeApiLib.GuiTest
{
	public partial class Form1 : Form
	{
		private YouTubeChannel _channel;
		private YouTubeChannelTabPage _channelTabPage;
		private string _nextPageToken = null;

		public Form1()
		{
			InitializeComponent();
		}

		private void listView1_Resize(object sender, EventArgs e)
		{
			columnHeaderTitle.Width = listView1.Width - columnHeaderId.Width - 30;
		}

		private void btnOpenChannel_Click(object sender, EventArgs e)
		{
			btnOpenChannel.Enabled =
			btnNextPage.Enabled = false;
			listView1.Items.Clear();
			_nextPageToken = null;

			string channelName = textBoxChannelName.Text;
			if (string.IsNullOrEmpty(channelName) || string.IsNullOrWhiteSpace(channelName))
			{
				MessageBox.Show("Не введено название канала!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				btnOpenChannel.Enabled = true;
				return;
			}
			string channelId = textBoxChannelId.Text;
			if (string.IsNullOrEmpty(channelId) || string.IsNullOrWhiteSpace(channelId))
			{
				MessageBox.Show("Не введён ID канала!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				btnOpenChannel.Enabled = true;
				return;
			}

			_channel = new YouTubeChannel(channelId, channelName);
			_channelTabPage = GetChannelTabPage();
			YouTubeApi api = new YouTubeApi();
			YouTubeVideoLitePageResult youTubeVideoLitePageResult = api.GetChannelVideoPage(_channel, _channelTabPage, null);
			if (youTubeVideoLitePageResult.ErrorCode == 200 && youTubeVideoLitePageResult.VideoLitePage.Count > 0)
			{
				foreach (YouTubeVideoLite videoLite in youTubeVideoLitePageResult.VideoLitePage.Videos)
				{
					ListViewItem item = new ListViewItem(videoLite.Id);
					item.SubItems.Add(videoLite.Title);
					item.Tag = videoLite;
					listView1.Items.Add(item);
				}

				_nextPageToken = youTubeVideoLitePageResult.VideoLitePage.ContinuationToken;
				if (!string.IsNullOrEmpty(_nextPageToken) && !string.IsNullOrWhiteSpace(_nextPageToken))
				{
					btnNextPage.Enabled = true;
				}
			}
			else
			{
				MessageBox.Show("Ничего не найдено!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			btnOpenChannel.Enabled = true;
		}

		private void btnNextPage_Click(object sender, EventArgs e)
		{
			btnNextPage.Enabled = false;
			btnOpenChannel.Enabled = false;
			if (string.IsNullOrEmpty(_nextPageToken) || string.IsNullOrWhiteSpace(_nextPageToken))
			{
				MessageBox.Show("Дальше ничего нет! Дальше только мрак и пустота!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				btnOpenChannel.Enabled = true;
				return;
			}

			YouTubeApi api = new YouTubeApi();
			YouTubeVideoLitePageResult youTubeVideoLitePageResult = api.GetChannelVideoPage(_channel, _channelTabPage, _nextPageToken);
			if (youTubeVideoLitePageResult.ErrorCode == 200 && youTubeVideoLitePageResult.VideoLitePage.Count > 0)
			{
				int count = listView1.Items.Count;
				foreach (YouTubeVideoLite videoLite in youTubeVideoLitePageResult.VideoLitePage.Videos)
				{
					ListViewItem item = new ListViewItem(videoLite.Id);
					item.SubItems.Add(videoLite.Title);
					item.Tag = videoLite;
					listView1.Items.Add(item);
				}
				listView1.SelectedIndices.Clear();
				listView1.Items[count].Selected = true;
				listView1.EnsureVisible(count);

				_nextPageToken = youTubeVideoLitePageResult.VideoLitePage.ContinuationToken;
				if (!string.IsNullOrEmpty(_nextPageToken) && !string.IsNullOrWhiteSpace(_nextPageToken))
				{
					btnNextPage.Enabled = true;
				}
			}
			else
			{
				_channel = null;
				_channelTabPage = null;
				_nextPageToken = null;
				MessageBox.Show("Дальше ничего нет! Там только мрак и пустота!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			btnOpenChannel.Enabled = true;
		}

		private async void btnGetChannelPages_Click(object sender, EventArgs e)
		{
			btnGetChannelPages.Enabled = false;
			textBoxChannelPages.Clear();

			string channelName = textBoxChannelName.Text;
			if (string.IsNullOrEmpty(channelName) || string.IsNullOrWhiteSpace(channelName))
			{
				MessageBox.Show("Не введено название канала!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				btnGetChannelPages.Enabled = true;
				return;
			}
			string channelId = textBoxChannelId.Text;
			if (string.IsNullOrEmpty(channelId) || string.IsNullOrWhiteSpace(channelId))
			{
				MessageBox.Show("Не введён ID канала!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				btnGetChannelPages.Enabled = true;
				return;
			}

			YouTubeChannel youTubeChannel = new YouTubeChannel(channelId, channelName);

			JObject jResult = new JObject();

			await Task.Run(() =>
			{
				List<YouTubeChannelTabPage> pages = new List<YouTubeChannelTabPage>()
				{
					YouTubeChannelTabPages.Home,
					YouTubeChannelTabPages.Videos,
					YouTubeChannelTabPages.Shorts,
					YouTubeChannelTabPages.Live,
					YouTubeChannelTabPages.Playlists,
					YouTubeChannelTabPages.Posts
				};
				YouTubeApi api = new YouTubeApi();
				foreach (YouTubeChannelTabPage channelTabPage in pages)
				{
					YouTubeChannelTabResult channelTabResult = api.GetChannelTab(youTubeChannel, channelTabPage);
					jResult[channelTabPage.Title] = channelTabResult.ChannelTab?.Data;
				}
			});

			textBoxChannelPages.Text = jResult.ToString();

			btnGetChannelPages.Enabled = true;
		}

		private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && listView1.SelectedIndices != null &&
				listView1.SelectedIndices.Count > 0)
			{
				int id = listView1.SelectedIndices[0];
				if (id >= 0 && id < listView1.Items.Count)
				{
					if (listView1.Items[id].Tag is YouTubeVideo video)
					{
						FormVideoInfo formVideoInfo = new FormVideoInfo(video);
						formVideoInfo.ShowDialog();
					}
				}
			}
		}

		private YouTubeChannelTabPage GetChannelTabPage()
		{
			if (radioButtonShorts.Checked)
			{
				return YouTubeChannelTabPages.Shorts;
			}
			else if (radioButtonStreams.Checked)
			{
				return YouTubeChannelTabPages.Live;
			}
			else
			{
				radioButtonVideos.Checked = true;
				return YouTubeChannelTabPages.Videos;
			}
		}
	}
}
