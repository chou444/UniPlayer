using System.Windows.Forms;

namespace UniPlayer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.contextMenuPlaylist = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnPrev = new System.Windows.Forms.Button();
            this.btnPlayPause = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnSavePlaylist = new System.Windows.Forms.Button();
            this.btnSleepTimer = new System.Windows.Forms.Button();
            this.btnHistory = new System.Windows.Forms.Button();
            this.btnPlayMode = new System.Windows.Forms.Button();
            this.lblPlayMode = new System.Windows.Forms.Label();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.lblSleepTimer = new System.Windows.Forms.Label();
            this.lblSongName = new System.Windows.Forms.Label();
            this.lblSongCount = new System.Windows.Forms.Label();
            this.lblFileInfo = new System.Windows.Forms.Label();
            this.lblTimeInfo = new System.Windows.Forms.Label();
            this.lblVolume = new System.Windows.Forms.Label();
            this.seekBar = new System.Windows.Forms.TrackBar();
            this.trackVolume = new System.Windows.Forms.TrackBar();
            this.axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();
            this.videoView1 = new LibVLCSharp.WinForms.VideoView();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timerProgress = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.trayMenuShow = new System.Windows.Forms.ToolStripMenuItem();
            this.trayMenuPlayPause = new System.Windows.Forms.ToolStripMenuItem();
            this.trayMenuNext = new System.Windows.Forms.ToolStripMenuItem();
            this.trayMenuExit = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.seekBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoView1)).BeginInit();
            this.trayMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.AllowDrop = true;
            this.listBox1.ContextMenuStrip = this.contextMenuPlaylist;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 18;
            this.listBox1.Location = new System.Drawing.Point(15, 175);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(600, 220);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // contextMenuPlaylist
            // 
            this.contextMenuPlaylist.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuPlaylist.Name = "contextMenuPlaylist";
            this.contextMenuPlaylist.Size = new System.Drawing.Size(61, 4);
            // 
            // btnPrev
            // 
            this.btnPrev.Location = new System.Drawing.Point(14, 128);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(50, 35);
            this.btnPrev.TabIndex = 1;
            this.btnPrev.Text = "⏮";
            this.btnPrev.UseVisualStyleBackColor = true;
            this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
            // 
            // btnPlayPause
            // 
            this.btnPlayPause.Location = new System.Drawing.Point(70, 128);
            this.btnPlayPause.Name = "btnPlayPause";
            this.btnPlayPause.Size = new System.Drawing.Size(70, 35);
            this.btnPlayPause.TabIndex = 2;
            this.btnPlayPause.Text = "播放";
            this.btnPlayPause.UseVisualStyleBackColor = true;
            this.btnPlayPause.Click += new System.EventHandler(this.btnPlayPause_Click);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(144, 128);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(50, 35);
            this.btnNext.TabIndex = 3;
            this.btnNext.Text = "⏭";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(200, 128);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(50, 35);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "⏹";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(15, 425);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(95, 35);
            this.btnOpen.TabIndex = 14;
            this.btnOpen.Text = "选择歌曲";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Location = new System.Drawing.Point(120, 425);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(100, 35);
            this.btnOpenFolder.TabIndex = 15;
            this.btnOpenFolder.Text = "扫描文件夹";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // btnSavePlaylist
            // 
            this.btnSavePlaylist.Location = new System.Drawing.Point(230, 425);
            this.btnSavePlaylist.Name = "btnSavePlaylist";
            this.btnSavePlaylist.Size = new System.Drawing.Size(95, 35);
            this.btnSavePlaylist.TabIndex = 16;
            this.btnSavePlaylist.Text = "保存歌单";
            this.btnSavePlaylist.UseVisualStyleBackColor = true;
            this.btnSavePlaylist.Click += new System.EventHandler(this.btnSavePlaylist_Click);
            // 
            // btnSleepTimer
            // 
            this.btnSleepTimer.Location = new System.Drawing.Point(340, 425);
            this.btnSleepTimer.Name = "btnSleepTimer";
            this.btnSleepTimer.Size = new System.Drawing.Size(90, 35);
            this.btnSleepTimer.TabIndex = 20;
            this.btnSleepTimer.Text = "定时关闭";
            this.btnSleepTimer.UseVisualStyleBackColor = true;
            this.btnSleepTimer.Click += new System.EventHandler(this.btnSleepTimer_Click);
            // 
            // btnHistory
            // 
            this.btnHistory.Location = new System.Drawing.Point(445, 425);
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Size = new System.Drawing.Size(95, 35);
            this.btnHistory.TabIndex = 22;
            this.btnHistory.Text = "播放历史";
            this.btnHistory.UseVisualStyleBackColor = true;
            this.btnHistory.Click += new System.EventHandler(this.btnHistory_Click);
            // 
            // btnPlayMode
            // 
            this.btnPlayMode.Location = new System.Drawing.Point(269, 128);
            this.btnPlayMode.Name = "btnPlayMode";
            this.btnPlayMode.Size = new System.Drawing.Size(40, 35);
            this.btnPlayMode.TabIndex = 5;
            this.btnPlayMode.Text = "🔁";
            this.btnPlayMode.UseVisualStyleBackColor = true;
            this.btnPlayMode.Click += new System.EventHandler(this.btnPlayMode_Click);
            // 
            // lblPlayMode
            // 
            this.lblPlayMode.AutoSize = true;
            this.lblPlayMode.Location = new System.Drawing.Point(315, 136);
            this.lblPlayMode.Name = "lblPlayMode";
            this.lblPlayMode.Size = new System.Drawing.Size(80, 18);
            this.lblPlayMode.TabIndex = 6;
            this.lblPlayMode.Text = "顺序播放";
            // 
            // lblSpeed
            // 
            this.lblSpeed.AutoSize = true;
            this.lblSpeed.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Underline);
            this.lblSpeed.ForeColor = System.Drawing.Color.DodgerBlue;
            this.lblSpeed.Location = new System.Drawing.Point(394, 134);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(45, 24);
            this.lblSpeed.TabIndex = 19;
            this.lblSpeed.Text = "1.0x";
            this.lblSpeed.Click += new System.EventHandler(this.lblSpeed_Click);
            // 
            // lblSleepTimer
            // 
            this.lblSleepTimer.AutoSize = true;
            this.lblSleepTimer.Location = new System.Drawing.Point(340, 465);
            this.lblSleepTimer.Name = "lblSleepTimer";
            this.lblSleepTimer.Size = new System.Drawing.Size(62, 18);
            this.lblSleepTimer.TabIndex = 21;
            this.lblSleepTimer.Text = "未设置";
            // 
            // lblSongName
            // 
            this.lblSongName.AutoSize = true;
            this.lblSongName.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblSongName.Location = new System.Drawing.Point(15, 12);
            this.lblSongName.Name = "lblSongName";
            this.lblSongName.Size = new System.Drawing.Size(79, 30);
            this.lblSongName.TabIndex = 9;
            this.lblSongName.Text = "未播放";
            // 
            // lblSongCount
            // 
            this.lblSongCount.AutoSize = true;
            this.lblSongCount.Location = new System.Drawing.Point(530, 15);
            this.lblSongCount.Name = "lblSongCount";
            this.lblSongCount.Size = new System.Drawing.Size(71, 18);
            this.lblSongCount.TabIndex = 10;
            this.lblSongCount.Text = "共 0 首";
            // 
            // lblFileInfo
            // 
            this.lblFileInfo.AutoSize = true;
            this.lblFileInfo.Location = new System.Drawing.Point(15, 38);
            this.lblFileInfo.Name = "lblFileInfo";
            this.lblFileInfo.Size = new System.Drawing.Size(62, 18);
            this.lblFileInfo.TabIndex = 11;
            this.lblFileInfo.Text = "未选择";
            // 
            // lblTimeInfo
            // 
            this.lblTimeInfo.AutoSize = true;
            this.lblTimeInfo.Location = new System.Drawing.Point(515, 69);
            this.lblTimeInfo.Name = "lblTimeInfo";
            this.lblTimeInfo.Size = new System.Drawing.Size(125, 18);
            this.lblTimeInfo.TabIndex = 8;
            this.lblTimeInfo.Text = "00:00 / 00:00";
            // 
            // lblVolume
            // 
            this.lblVolume.AutoSize = true;
            this.lblVolume.Location = new System.Drawing.Point(581, 113);
            this.lblVolume.Name = "lblVolume";
            this.lblVolume.Size = new System.Drawing.Size(26, 18);
            this.lblVolume.TabIndex = 13;
            this.lblVolume.Text = "50";
            // 
            // seekBar
            // 
            this.seekBar.Location = new System.Drawing.Point(15, 62);
            this.seekBar.Name = "seekBar";
            this.seekBar.Size = new System.Drawing.Size(490, 69);
            this.seekBar.TabIndex = 7;
            this.seekBar.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // trackVolume
            // 
            this.trackVolume.Location = new System.Drawing.Point(445, 100);
            this.trackVolume.Maximum = 100;
            this.trackVolume.Name = "trackVolume";
            this.trackVolume.Size = new System.Drawing.Size(130, 69);
            this.trackVolume.TabIndex = 12;
            this.trackVolume.TickFrequency = 10;
            this.trackVolume.Value = 50;
            this.trackVolume.Scroll += new System.EventHandler(this.trackVolume_Scroll);
            // 
            // axWindowsMediaPlayer1
            // 
            this.axWindowsMediaPlayer1.Enabled = true;
            this.axWindowsMediaPlayer1.Location = new System.Drawing.Point(0, 0);
            this.axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
            this.axWindowsMediaPlayer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWindowsMediaPlayer1.OcxState")));
            this.axWindowsMediaPlayer1.Size = new System.Drawing.Size(1, 1);
            this.axWindowsMediaPlayer1.TabIndex = 17;
            this.axWindowsMediaPlayer1.Visible = false;
            this.axWindowsMediaPlayer1.Enter += new System.EventHandler(this.videoView1_Click);
            // 
            // videoView1
            // 
            this.videoView1.BackColor = System.Drawing.Color.Black;
            this.videoView1.Location = new System.Drawing.Point(0, 0);
            this.videoView1.MediaPlayer = null;
            this.videoView1.Name = "videoView1";
            this.videoView1.Size = new System.Drawing.Size(1, 1);
            this.videoView1.TabIndex = 18;
            this.videoView1.Visible = false;
            this.videoView1.Click += new System.EventHandler(this.videoView1_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // timerProgress
            // 
            this.timerProgress.Interval = 500;
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.trayMenu;
            this.notifyIcon.Text = "UniPlayer";
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // trayMenu
            // 
            this.trayMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trayMenuShow,
            this.trayMenuPlayPause,
            this.trayMenuNext,
            this.trayMenuExit});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(171, 124);
            // 
            // trayMenuShow
            // 
            this.trayMenuShow.Name = "trayMenuShow";
            this.trayMenuShow.Size = new System.Drawing.Size(170, 30);
            this.trayMenuShow.Text = "显示主窗口";
            this.trayMenuShow.Click += new System.EventHandler(this.trayMenuShow_Click);
            // 
            // trayMenuPlayPause
            // 
            this.trayMenuPlayPause.Name = "trayMenuPlayPause";
            this.trayMenuPlayPause.Size = new System.Drawing.Size(170, 30);
            this.trayMenuPlayPause.Text = "播放/暂停";
            this.trayMenuPlayPause.Click += new System.EventHandler(this.trayMenuPlayPause_Click);
            // 
            // trayMenuNext
            // 
            this.trayMenuNext.Name = "trayMenuNext";
            this.trayMenuNext.Size = new System.Drawing.Size(170, 30);
            this.trayMenuNext.Text = "下一曲";
            this.trayMenuNext.Click += new System.EventHandler(this.trayMenuNext_Click);
            // 
            // trayMenuExit
            // 
            this.trayMenuExit.Name = "trayMenuExit";
            this.trayMenuExit.Size = new System.Drawing.Size(170, 30);
            this.trayMenuExit.Text = "退出";
            this.trayMenuExit.Click += new System.EventHandler(this.trayMenuExit_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(751, 558);
            this.Controls.Add(this.lblSpeed);
            this.Controls.Add(this.btnHistory);
            this.Controls.Add(this.lblSleepTimer);
            this.Controls.Add(this.btnSleepTimer);
            this.Controls.Add(this.btnSavePlaylist);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.lblVolume);
            this.Controls.Add(this.trackVolume);
            this.Controls.Add(this.lblFileInfo);
            this.Controls.Add(this.lblSongCount);
            this.Controls.Add(this.lblSongName);
            this.Controls.Add(this.lblTimeInfo);
            this.Controls.Add(this.seekBar);
            this.Controls.Add(this.lblPlayMode);
            this.Controls.Add(this.btnPlayMode);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnPlayPause);
            this.Controls.Add(this.btnPrev);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.axWindowsMediaPlayer1);
            this.Controls.Add(this.videoView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UniPlayer - 音乐播放器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.seekBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoView1)).EndInit();
            this.trayMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        // ========================================================================
        //  控件字段声明
        // ========================================================================

        // — 原始控件 —
        private System.Windows.Forms.ListBox listBox1;
        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
        private LibVLCSharp.WinForms.VideoView videoView1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;

        // — 播放控制 —
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Button btnPlayPause;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnPlayMode;
        private System.Windows.Forms.Label lblPlayMode;
        private System.Windows.Forms.Label lblSpeed;

        // — 信息标签 —
        private System.Windows.Forms.Label lblSongName;
        private System.Windows.Forms.Label lblSongCount;
        private System.Windows.Forms.Label lblFileInfo;
        private System.Windows.Forms.Label lblTimeInfo;
        private System.Windows.Forms.Label lblVolume;

        // — 进度 / 音量 —
        private System.Windows.Forms.TrackBar seekBar;
        private System.Windows.Forms.TrackBar trackVolume;

        // — 歌单操作 —
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnSavePlaylist;
        private System.Windows.Forms.ContextMenuStrip contextMenuPlaylist;

        // — 高级功能 —
        private System.Windows.Forms.Button btnSleepTimer;
        private System.Windows.Forms.Label lblSleepTimer;
        private System.Windows.Forms.Button btnHistory;

        // — 系统 —
        private System.Windows.Forms.Timer timerProgress;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.ToolStripMenuItem trayMenuShow;
        private System.Windows.Forms.ToolStripMenuItem trayMenuPlayPause;
        private System.Windows.Forms.ToolStripMenuItem trayMenuNext;
        private System.Windows.Forms.ToolStripMenuItem trayMenuExit;

        // — LibVLC —
        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer;
    }
}
