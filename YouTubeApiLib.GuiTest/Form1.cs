using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace YouTubeApiLib.GuiTest
{
	public partial class Form1 : Form
	{
		private List<YouTubeVideo> foundVideos;
		private string nextPageToken = null;

		public Form1()
		{
			InitializeComponent();
		}

		private void listView1_Resize(object sender, EventArgs e)
		{
			columnHeaderTitle.Width = listView1.Width - columnHeaderId.Width - 30;
		}

		private void panel1_Resize(object sender, EventArgs e)
		{
			btnNextPage.Left = panel1.Width / 2 - btnNextPage.Width / 2;
		}

		private void btnSaveList_Click(object sender, EventArgs e)
		{
			btnSaveList.Enabled = false;
			if (foundVideos == null || foundVideos.Count == 0)
			{
				MessageBox.Show("Список пуст!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				btnSaveList.Enabled = true;
				return;
			}

			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = "Save ass...";
			sfd.Filter = "JSON|*.json";
			sfd.FileName = textBoxChannelName.Text;
			sfd.DefaultExt = ".json";
			if (sfd.ShowDialog() == DialogResult.OK)
			{
				JArray jArray = new JArray();
				foreach (YouTubeVideo video in foundVideos)
				{
					jArray.Add(video.SimplifiedInfo.Info);
				}
				JObject json = new JObject();
				json.Add(new JProperty("videos", jArray));
				System.IO.File.WriteAllText(sfd.FileName, json.ToString());
			}
			sfd.Dispose();

			btnSaveList.Enabled = true;
		}

		private void btnOpenChannel_Click(object sender, EventArgs e)
		{
			btnOpenChannel.Enabled = false;
			btnNextPage.Enabled = false;
			btnSaveList.Enabled = false;
			listView1.Items.Clear();
			nextPageToken = null;
			foundVideos = new List<YouTubeVideo>();

			string channelName = textBoxChannelName.Text;
			if (string.IsNullOrEmpty(channelName) || string.IsNullOrWhiteSpace(channelName))
			{
				MessageBox.Show("Не введено название канала!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				btnOpenChannel.Enabled = true;
				btnSaveList.Enabled = true;
				return;
			}
			string channelId = textBoxChannelId.Text;
			if (string.IsNullOrEmpty(channelId) || string.IsNullOrWhiteSpace(channelId))
			{
				MessageBox.Show("Не введён ID канала!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				btnOpenChannel.Enabled = true;
				btnSaveList.Enabled = true;
				return;
			}

			YouTubeChannelTabPageContentResult pageContentResult =
				YouTubeChannelTabPageContent.Get(channelId, YouTubeChannelTabPages.Videos, null);
			if (pageContentResult.ErrorCode == 200)
			{
				YouTubeVideosTabPage videosTabPage = pageContentResult.Content.ParseAsVideosOrShortsOrLiveTabPage();
				if (videosTabPage != null)
				{
					if (videosTabPage.UpdateVideosMultiThreaded())
					{
						videosTabPage.VideoList.Sort((x, y) => x.DatePublished > y.DatePublished ? -1 : 1);

						foreach (YouTubeVideo video in videosTabPage.VideoList)
						{
							foundVideos.Add(video);

							ListViewItem item = new ListViewItem(video.Id);
							item.SubItems.Add(video.Title);
							item.Tag = video;
							listView1.Items.Add(item);
						}
					}

					nextPageToken = videosTabPage.NextPageToken;
					if (!string.IsNullOrEmpty(nextPageToken) && !string.IsNullOrWhiteSpace(nextPageToken))
					{
						btnNextPage.Enabled = true;
					}
				}
				else
				{
					MessageBox.Show("Ничего не найдено!", "Ошибка!",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			else
			{
				MessageBox.Show("Ничего не найдено!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			btnOpenChannel.Enabled = true;
			btnSaveList.Enabled = true;
		}

		private void btnNextPage_Click(object sender, EventArgs e)
		{
			btnNextPage.Enabled = false;
			btnOpenChannel.Enabled = false;
			if (string.IsNullOrEmpty(nextPageToken) || string.IsNullOrWhiteSpace(nextPageToken))
			{
				MessageBox.Show("Дальше ничего нет! Дальше только мрак и пустота!", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				btnOpenChannel.Enabled = true;
				return;
			}

			YouTubeChannelTabPageContentResult pageContentResult = YouTubeChannelTabPageContent.Get(nextPageToken);
			if (pageContentResult != null)
			{
				YouTubeVideosTabPage videosTabPage = pageContentResult.Content.ParseAsVideosOrShortsOrLiveTabPage();
				if (videosTabPage != null)
				{
					if (videosTabPage.UpdateVideosMultiThreaded())
					{
						videosTabPage.VideoList.Sort((x, y) => x.DatePublished > y.DatePublished ? -1 : 1);

						foreach (YouTubeVideo video in videosTabPage.VideoList)
						{
							foundVideos.Add(video);

							ListViewItem item = new ListViewItem(video.Id);
							item.SubItems.Add(video.Title);
							item.Tag = video;
							listView1.Items.Add(item);
						}
					}

					nextPageToken = videosTabPage.NextPageToken;
					if (!string.IsNullOrEmpty(nextPageToken) && !string.IsNullOrWhiteSpace(nextPageToken))
					{
						btnNextPage.Enabled = true;
					}
				}
				else
				{
					MessageBox.Show("Ничего не найдено!", "Ошибка!",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			else
			{
				MessageBox.Show("Ничего не найдено!", "Ошибка!",
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
					YouTubeChannelTabPages.Community
				};
				YouTubeApi api = new YouTubeApi();
				foreach (YouTubeChannelTabPage channelTabPage in pages)
				{
					YouTubeChannelTabResult channelTabResult = api.GetChannelTab(youTubeChannel, channelTabPage);
					jResult.Add(new JProperty(channelTabPage.Title, channelTabResult.ChannelTab?.Json));
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
	}
}
