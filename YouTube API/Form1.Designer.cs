﻿
namespace YouTube_API
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
            this.btnGetChannelVideoList = new System.Windows.Forms.Button();
            this.textBoxChannelName = new System.Windows.Forms.TextBox();
            this.textBoxChannelId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeaderId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnSaveList = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbDescending = new System.Windows.Forms.RadioButton();
            this.rbAscending = new System.Windows.Forms.RadioButton();
            this.rbPopularity = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGetChannelVideoList
            // 
            this.btnGetChannelVideoList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetChannelVideoList.Location = new System.Drawing.Point(382, 80);
            this.btnGetChannelVideoList.Name = "btnGetChannelVideoList";
            this.btnGetChannelVideoList.Size = new System.Drawing.Size(190, 23);
            this.btnGetChannelVideoList.TabIndex = 0;
            this.btnGetChannelVideoList.Text = "Получить список видео канала";
            this.btnGetChannelVideoList.UseVisualStyleBackColor = true;
            this.btnGetChannelVideoList.Click += new System.EventHandler(this.btnGetChannelVideoList_Click);
            // 
            // textBoxChannelName
            // 
            this.textBoxChannelName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxChannelName.Location = new System.Drawing.Point(160, 15);
            this.textBoxChannelName.Name = "textBoxChannelName";
            this.textBoxChannelName.Size = new System.Drawing.Size(412, 20);
            this.textBoxChannelName.TabIndex = 1;
            this.textBoxChannelName.Text = "Бурухтания";
            // 
            // textBoxChannelId
            // 
            this.textBoxChannelId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxChannelId.Location = new System.Drawing.Point(160, 41);
            this.textBoxChannelId.Name = "textBoxChannelId";
            this.textBoxChannelId.Size = new System.Drawing.Size(412, 20);
            this.textBoxChannelId.TabIndex = 2;
            this.textBoxChannelId.Text = "UCeod7jWDtg1gz5HokbBdEbQ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Введите название канала:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 44);
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
            this.listView1.Location = new System.Drawing.Point(12, 119);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(560, 255);
            this.listView1.TabIndex = 5;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
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
            // btnSaveList
            // 
            this.btnSaveList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveList.Location = new System.Drawing.Point(497, 383);
            this.btnSaveList.Name = "btnSaveList";
            this.btnSaveList.Size = new System.Drawing.Size(75, 23);
            this.btnSaveList.TabIndex = 6;
            this.btnSaveList.Text = "Сохранить";
            this.btnSaveList.UseVisualStyleBackColor = true;
            this.btnSaveList.Click += new System.EventHandler(this.btnSaveList_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbPopularity);
            this.groupBox1.Controls.Add(this.rbAscending);
            this.groupBox1.Controls.Add(this.rbDescending);
            this.groupBox1.Location = new System.Drawing.Point(15, 69);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(342, 44);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Сортировка";
            // 
            // rbDescending
            // 
            this.rbDescending.AutoSize = true;
            this.rbDescending.Checked = true;
            this.rbDescending.Location = new System.Drawing.Point(12, 17);
            this.rbDescending.Name = "rbDescending";
            this.rbDescending.Size = new System.Drawing.Size(96, 17);
            this.rbDescending.TabIndex = 0;
            this.rbDescending.TabStop = true;
            this.rbDescending.Text = "Новые сверху";
            this.rbDescending.UseVisualStyleBackColor = true;
            // 
            // rbAscending
            // 
            this.rbAscending.AutoSize = true;
            this.rbAscending.Location = new System.Drawing.Point(114, 17);
            this.rbAscending.Name = "rbAscending";
            this.rbAscending.Size = new System.Drawing.Size(100, 17);
            this.rbAscending.TabIndex = 1;
            this.rbAscending.Text = "Старые сверху";
            this.rbAscending.UseVisualStyleBackColor = true;
            // 
            // rbPopularity
            // 
            this.rbPopularity.AutoSize = true;
            this.rbPopularity.Location = new System.Drawing.Point(219, 17);
            this.rbPopularity.Name = "rbPopularity";
            this.rbPopularity.Size = new System.Drawing.Size(112, 17);
            this.rbPopularity.TabIndex = 2;
            this.rbPopularity.Text = "По популярности";
            this.rbPopularity.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 418);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnSaveList);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxChannelId);
            this.Controls.Add(this.textBoxChannelName);
            this.Controls.Add(this.btnGetChannelVideoList);
            this.MinimumSize = new System.Drawing.Size(600, 300);
            this.Name = "Form1";
            this.Text = "YouTube API";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnGetChannelVideoList;
        private System.Windows.Forms.TextBox textBoxChannelName;
        private System.Windows.Forms.TextBox textBoxChannelId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeaderId;
        private System.Windows.Forms.ColumnHeader columnHeaderTitle;
        private System.Windows.Forms.Button btnSaveList;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbPopularity;
        private System.Windows.Forms.RadioButton rbAscending;
        private System.Windows.Forms.RadioButton rbDescending;
    }
}

