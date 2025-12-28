namespace YouTubeApiLib.GuiTest
{
	partial class Form1
	{
		/// <summary>
		/// Обязательная переменная конструктора.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Освободить все используемые ресурсы.
		/// </summary>
		/// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Код, автоматически созданный конструктором форм Windows

		/// <summary>
		/// Требуемый метод для поддержки конструктора — не изменяйте 
		/// содержимое этого метода с помощью редактора кода.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnOpenChannel = new System.Windows.Forms.Button();
			this.textBoxChannelName = new System.Windows.Forms.TextBox();
			this.textBoxChannelId = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeaderId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPageChannelVideos = new System.Windows.Forms.TabPage();
			this.groupBoxChannelTab = new System.Windows.Forms.GroupBox();
			this.radioButtonStreams = new System.Windows.Forms.RadioButton();
			this.radioButtonShorts = new System.Windows.Forms.RadioButton();
			this.radioButtonVideos = new System.Windows.Forms.RadioButton();
			this.btnNextPage = new System.Windows.Forms.Button();
			this.tabPageChannelPages = new System.Windows.Forms.TabPage();
			this.textBoxChannelPages = new System.Windows.Forms.TextBox();
			this.btnGetChannelPages = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.tabPageChannelVideos.SuspendLayout();
			this.groupBoxChannelTab.SuspendLayout();
			this.tabPageChannelPages.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOpenChannel
			// 
			this.btnOpenChannel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnOpenChannel.Location = new System.Drawing.Point(6, 287);
			this.btnOpenChannel.Name = "btnOpenChannel";
			this.btnOpenChannel.Size = new System.Drawing.Size(115, 23);
			this.btnOpenChannel.TabIndex = 0;
			this.btnOpenChannel.Text = "Перейти на канал";
			this.btnOpenChannel.UseVisualStyleBackColor = true;
			this.btnOpenChannel.Click += new System.EventHandler(this.btnOpenChannel_Click);
			// 
			// textBoxChannelName
			// 
			this.textBoxChannelName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxChannelName.Location = new System.Drawing.Point(160, 5);
			this.textBoxChannelName.Name = "textBoxChannelName";
			this.textBoxChannelName.Size = new System.Drawing.Size(412, 20);
			this.textBoxChannelName.TabIndex = 1;
			this.textBoxChannelName.Text = "Бурухтания";
			// 
			// textBoxChannelId
			// 
			this.textBoxChannelId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxChannelId.Location = new System.Drawing.Point(160, 31);
			this.textBoxChannelId.Name = "textBoxChannelId";
			this.textBoxChannelId.Size = new System.Drawing.Size(412, 20);
			this.textBoxChannelId.TabIndex = 2;
			this.textBoxChannelId.Text = "UCeod7jWDtg1gz5HokbBdEbQ";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(142, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Введите название канала:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 34);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(105, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Введите ID канала:";
			// 
			// listView1
			// 
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.BackColor = System.Drawing.Color.Black;
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.columnHeaderId,
			this.columnHeaderTitle});
			this.listView1.Font = new System.Drawing.Font("Lucida Console", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.listView1.ForeColor = System.Drawing.Color.Lime;
			this.listView1.FullRowSelect = true;
			this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(6, 6);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(537, 223);
			this.listView1.TabIndex = 5;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
			this.listView1.Resize += new System.EventHandler(this.listView1_Resize);
			// 
			// columnHeaderId
			// 
			this.columnHeaderId.Text = "ID видео";
			this.columnHeaderId.Width = 150;
			// 
			// columnHeaderTitle
			// 
			this.columnHeaderTitle.Text = "Название видео";
			this.columnHeaderTitle.Width = 300;
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabPageChannelVideos);
			this.tabControl1.Controls.Add(this.tabPageChannelPages);
			this.tabControl1.Location = new System.Drawing.Point(15, 57);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(557, 342);
			this.tabControl1.TabIndex = 8;
			// 
			// tabPageChannelVideos
			// 
			this.tabPageChannelVideos.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageChannelVideos.Controls.Add(this.groupBoxChannelTab);
			this.tabPageChannelVideos.Controls.Add(this.btnNextPage);
			this.tabPageChannelVideos.Controls.Add(this.listView1);
			this.tabPageChannelVideos.Controls.Add(this.btnOpenChannel);
			this.tabPageChannelVideos.Location = new System.Drawing.Point(4, 22);
			this.tabPageChannelVideos.Name = "tabPageChannelVideos";
			this.tabPageChannelVideos.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageChannelVideos.Size = new System.Drawing.Size(549, 316);
			this.tabPageChannelVideos.TabIndex = 0;
			this.tabPageChannelVideos.Text = "Спиок видео канала";
			// 
			// groupBoxChannelTab
			// 
			this.groupBoxChannelTab.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxChannelTab.Controls.Add(this.radioButtonStreams);
			this.groupBoxChannelTab.Controls.Add(this.radioButtonShorts);
			this.groupBoxChannelTab.Controls.Add(this.radioButtonVideos);
			this.groupBoxChannelTab.Location = new System.Drawing.Point(6, 235);
			this.groupBoxChannelTab.Name = "groupBoxChannelTab";
			this.groupBoxChannelTab.Size = new System.Drawing.Size(537, 46);
			this.groupBoxChannelTab.TabIndex = 6;
			this.groupBoxChannelTab.TabStop = false;
			this.groupBoxChannelTab.Text = "Channel tab";
			// 
			// radioButtonStreams
			// 
			this.radioButtonStreams.AutoSize = true;
			this.radioButtonStreams.Location = new System.Drawing.Point(139, 19);
			this.radioButtonStreams.Name = "radioButtonStreams";
			this.radioButtonStreams.Size = new System.Drawing.Size(63, 17);
			this.radioButtonStreams.TabIndex = 2;
			this.radioButtonStreams.Text = "Streams";
			this.radioButtonStreams.UseVisualStyleBackColor = true;
			// 
			// radioButtonShorts
			// 
			this.radioButtonShorts.AutoSize = true;
			this.radioButtonShorts.Location = new System.Drawing.Point(78, 19);
			this.radioButtonShorts.Name = "radioButtonShorts";
			this.radioButtonShorts.Size = new System.Drawing.Size(55, 17);
			this.radioButtonShorts.TabIndex = 1;
			this.radioButtonShorts.Text = "Shorts";
			this.radioButtonShorts.UseVisualStyleBackColor = true;
			// 
			// radioButtonVideos
			// 
			this.radioButtonVideos.AutoSize = true;
			this.radioButtonVideos.Checked = true;
			this.radioButtonVideos.Location = new System.Drawing.Point(15, 19);
			this.radioButtonVideos.Name = "radioButtonVideos";
			this.radioButtonVideos.Size = new System.Drawing.Size(57, 17);
			this.radioButtonVideos.TabIndex = 0;
			this.radioButtonVideos.TabStop = true;
			this.radioButtonVideos.Text = "Videos";
			this.radioButtonVideos.UseVisualStyleBackColor = true;
			// 
			// btnNextPage
			// 
			this.btnNextPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnNextPage.Enabled = false;
			this.btnNextPage.Location = new System.Drawing.Point(473, 290);
			this.btnNextPage.Name = "btnNextPage";
			this.btnNextPage.Size = new System.Drawing.Size(70, 20);
			this.btnNextPage.TabIndex = 0;
			this.btnNextPage.Text = "Дальше";
			this.btnNextPage.UseVisualStyleBackColor = true;
			this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
			// 
			// tabPageChannelPages
			// 
			this.tabPageChannelPages.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageChannelPages.Controls.Add(this.textBoxChannelPages);
			this.tabPageChannelPages.Controls.Add(this.btnGetChannelPages);
			this.tabPageChannelPages.Location = new System.Drawing.Point(4, 22);
			this.tabPageChannelPages.Name = "tabPageChannelPages";
			this.tabPageChannelPages.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageChannelPages.Size = new System.Drawing.Size(549, 316);
			this.tabPageChannelPages.TabIndex = 1;
			this.tabPageChannelPages.Text = "Страницы канала";
			// 
			// textBoxChannelPages
			// 
			this.textBoxChannelPages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxChannelPages.BackColor = System.Drawing.Color.Black;
			this.textBoxChannelPages.ForeColor = System.Drawing.Color.Lime;
			this.textBoxChannelPages.Location = new System.Drawing.Point(3, 6);
			this.textBoxChannelPages.Multiline = true;
			this.textBoxChannelPages.Name = "textBoxChannelPages";
			this.textBoxChannelPages.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxChannelPages.Size = new System.Drawing.Size(534, 275);
			this.textBoxChannelPages.TabIndex = 1;
			// 
			// btnGetChannelPages
			// 
			this.btnGetChannelPages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnGetChannelPages.Location = new System.Drawing.Point(462, 287);
			this.btnGetChannelPages.Name = "btnGetChannelPages";
			this.btnGetChannelPages.Size = new System.Drawing.Size(75, 23);
			this.btnGetChannelPages.TabIndex = 0;
			this.btnGetChannelPages.Text = "Получить";
			this.btnGetChannelPages.UseVisualStyleBackColor = true;
			this.btnGetChannelPages.Click += new System.EventHandler(this.btnGetChannelPages_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(584, 411);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBoxChannelId);
			this.Controls.Add(this.textBoxChannelName);
			this.MinimumSize = new System.Drawing.Size(600, 400);
			this.Name = "Form1";
			this.Text = "YouTube API";
			this.tabControl1.ResumeLayout(false);
			this.tabPageChannelVideos.ResumeLayout(false);
			this.groupBoxChannelTab.ResumeLayout(false);
			this.groupBoxChannelTab.PerformLayout();
			this.tabPageChannelPages.ResumeLayout(false);
			this.tabPageChannelPages.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnOpenChannel;
		private System.Windows.Forms.TextBox textBoxChannelName;
		private System.Windows.Forms.TextBox textBoxChannelId;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeaderId;
		private System.Windows.Forms.ColumnHeader columnHeaderTitle;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPageChannelVideos;
		private System.Windows.Forms.TabPage tabPageChannelPages;
		private System.Windows.Forms.TextBox textBoxChannelPages;
		private System.Windows.Forms.Button btnGetChannelPages;
		private System.Windows.Forms.Button btnNextPage;
		private System.Windows.Forms.GroupBox groupBoxChannelTab;
		private System.Windows.Forms.RadioButton radioButtonStreams;
		private System.Windows.Forms.RadioButton radioButtonShorts;
		private System.Windows.Forms.RadioButton radioButtonVideos;
	}
}
