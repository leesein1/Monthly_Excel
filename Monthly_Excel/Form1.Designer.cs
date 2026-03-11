namespace Monthly_Excel
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label labelUpload;
        private System.Windows.Forms.Button buttonUpload;
        private System.Windows.Forms.Label labelDownload;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.Label labelTemplate;
        private System.Windows.Forms.Button buttonTemplateDownload;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageCrawling;
        private System.Windows.Forms.TabPage tabPageKeyword;
        private System.Windows.Forms.TabPage tabPageBlogCleaner;

        private System.Windows.Forms.TextBox inputKeywordBox;
        private System.Windows.Forms.Button convertButton;
        private System.Windows.Forms.ListBox leftListBox;
        private System.Windows.Forms.ListBox rightListBox;
        private System.Windows.Forms.Button copyLeftButton;
        private System.Windows.Forms.Button copyRightButton;
        private System.Windows.Forms.Label labelLeft;
        private System.Windows.Forms.Label labelRight;
        private System.Windows.Forms.TableLayoutPanel keywordLayout;

        private System.Windows.Forms.TableLayoutPanel blogLayout;
        private System.Windows.Forms.Panel blogTopPanel;
        private System.Windows.Forms.TextBox blogUrlTextBox;
        private System.Windows.Forms.Button buttonBlogOpen;
        private System.Windows.Forms.Button buttonBlogClean;
        private System.Windows.Forms.Button buttonBlogRefresh;
        private System.Windows.Forms.Button buttonBlogDownloadImages;
        private System.Windows.Forms.Label labelBlogStatus;
        private Microsoft.Web.WebView2.WinForms.WebView2 blogWebView;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            tabControl = new TabControl();
            tabPageCrawling = new TabPage();
            tableLayoutPanel = new TableLayoutPanel();
            labelUpload = new Label();
            buttonUpload = new Button();
            labelDownload = new Label();
            buttonDownload = new Button();
            labelTemplate = new Label();
            buttonTemplateDownload = new Button();
            labelStatus = new Label();
            progressBar = new ProgressBar();
            tabPageKeyword = new TabPage();
            keywordLayout = new TableLayoutPanel();
            inputKeywordBox = new TextBox();
            convertButton = new Button();
            labelLeft = new Label();
            labelRight = new Label();
            leftListBox = new ListBox();
            rightListBox = new ListBox();
            copyLeftButton = new Button();
            copyRightButton = new Button();
            tabPageBlogCleaner = new TabPage();
            blogLayout = new TableLayoutPanel();
            blogTopPanel = new Panel();
            blogUrlTextBox = new TextBox();
            buttonBlogOpen = new Button();
            buttonBlogClean = new Button();
            buttonBlogRefresh = new Button();
            buttonBlogDownloadImages = new Button();
            labelBlogStatus = new Label();
            blogWebView = new Microsoft.Web.WebView2.WinForms.WebView2();
            tabControl.SuspendLayout();
            tabPageCrawling.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            tabPageKeyword.SuspendLayout();
            keywordLayout.SuspendLayout();
            tabPageBlogCleaner.SuspendLayout();
            blogLayout.SuspendLayout();
            blogTopPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)blogWebView).BeginInit();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageCrawling);
            tabControl.Controls.Add(tabPageKeyword);
            tabControl.Controls.Add(tabPageBlogCleaner);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(516, 289);
            tabControl.TabIndex = 0;
            // 
            // tabPageCrawling
            // 
            tabPageCrawling.Controls.Add(tableLayoutPanel);
            tabPageCrawling.Location = new Point(4, 24);
            tabPageCrawling.Name = "tabPageCrawling";
            tabPageCrawling.Padding = new Padding(10);
            tabPageCrawling.Size = new Size(508, 261);
            tabPageCrawling.TabIndex = 0;
            tabPageCrawling.Text = "크롤링";
            tabPageCrawling.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableLayoutPanel.Controls.Add(labelUpload, 0, 0);
            tableLayoutPanel.Controls.Add(buttonUpload, 1, 0);
            tableLayoutPanel.Controls.Add(labelDownload, 0, 1);
            tableLayoutPanel.Controls.Add(buttonDownload, 1, 1);
            tableLayoutPanel.Controls.Add(labelTemplate, 0, 2);
            tableLayoutPanel.Controls.Add(buttonTemplateDownload, 1, 2);
            tableLayoutPanel.Controls.Add(labelStatus, 0, 3);
            tableLayoutPanel.Controls.Add(progressBar, 0, 4);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(10, 10);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.Padding = new Padding(20);
            tableLayoutPanel.RowCount = 6;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Size = new Size(488, 241);
            tableLayoutPanel.TabIndex = 0;
            // 
            // labelUpload
            // 
            labelUpload.Dock = DockStyle.Fill;
            labelUpload.Location = new Point(23, 20);
            labelUpload.Name = "labelUpload";
            labelUpload.Size = new Size(173, 40);
            labelUpload.TabIndex = 0;
            labelUpload.Text = "📅 엑셀 업로드";
            labelUpload.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonUpload
            // 
            buttonUpload.Dock = DockStyle.Fill;
            buttonUpload.Location = new Point(202, 23);
            buttonUpload.Name = "buttonUpload";
            buttonUpload.Size = new Size(263, 34);
            buttonUpload.TabIndex = 1;
            buttonUpload.Text = "파일 선택";
            buttonUpload.UseVisualStyleBackColor = true;
            // 
            // labelDownload
            // 
            labelDownload.Dock = DockStyle.Fill;
            labelDownload.Location = new Point(23, 60);
            labelDownload.Name = "labelDownload";
            labelDownload.Size = new Size(173, 40);
            labelDownload.TabIndex = 2;
            labelDownload.Text = "📄 엑셀 다운로드";
            labelDownload.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonDownload
            // 
            buttonDownload.Dock = DockStyle.Fill;
            buttonDownload.Location = new Point(202, 63);
            buttonDownload.Name = "buttonDownload";
            buttonDownload.Size = new Size(263, 34);
            buttonDownload.TabIndex = 3;
            buttonDownload.Text = "다운로드";
            buttonDownload.UseVisualStyleBackColor = true;
            // 
            // labelTemplate
            // 
            labelTemplate.Dock = DockStyle.Fill;
            labelTemplate.Location = new Point(23, 100);
            labelTemplate.Name = "labelTemplate";
            labelTemplate.Size = new Size(173, 40);
            labelTemplate.TabIndex = 4;
            labelTemplate.Text = "📘 양식 다운로드";
            labelTemplate.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonTemplateDownload
            // 
            buttonTemplateDownload.Dock = DockStyle.Fill;
            buttonTemplateDownload.Location = new Point(202, 103);
            buttonTemplateDownload.Name = "buttonTemplateDownload";
            buttonTemplateDownload.Size = new Size(263, 34);
            buttonTemplateDownload.TabIndex = 5;
            buttonTemplateDownload.Text = "양식 받기";
            buttonTemplateDownload.UseVisualStyleBackColor = true;
            // 
            // labelStatus
            // 
            tableLayoutPanel.SetColumnSpan(labelStatus, 2);
            labelStatus.Dock = DockStyle.Fill;
            labelStatus.Location = new Point(23, 140);
            labelStatus.Name = "labelStatus";
            labelStatus.Padding = new Padding(5, 0, 0, 0);
            labelStatus.Size = new Size(442, 25);
            labelStatus.TabIndex = 6;
            labelStatus.Text = "상태: 대기 중";
            labelStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            tableLayoutPanel.SetColumnSpan(progressBar, 2);
            progressBar.Dock = DockStyle.Fill;
            progressBar.Location = new Point(23, 168);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(442, 29);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 7;
            // 
            // tabPageKeyword
            // 
            tabPageKeyword.Controls.Add(keywordLayout);
            tabPageKeyword.Location = new Point(4, 24);
            tabPageKeyword.Name = "tabPageKeyword";
            tabPageKeyword.Padding = new Padding(10);
            tabPageKeyword.Size = new Size(508, 261);
            tabPageKeyword.TabIndex = 1;
            tabPageKeyword.Text = "키워드";
            tabPageKeyword.UseVisualStyleBackColor = true;
            // 
            // keywordLayout
            // 
            keywordLayout.ColumnCount = 2;
            keywordLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            keywordLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            keywordLayout.Controls.Add(inputKeywordBox, 0, 0);
            keywordLayout.Controls.Add(convertButton, 1, 0);
            keywordLayout.Controls.Add(labelLeft, 0, 1);
            keywordLayout.Controls.Add(labelRight, 1, 1);
            keywordLayout.Controls.Add(leftListBox, 0, 2);
            keywordLayout.Controls.Add(rightListBox, 1, 2);
            keywordLayout.Controls.Add(copyLeftButton, 0, 3);
            keywordLayout.Controls.Add(copyRightButton, 1, 3);
            keywordLayout.Dock = DockStyle.Fill;
            keywordLayout.Location = new Point(10, 10);
            keywordLayout.Name = "keywordLayout";
            keywordLayout.Padding = new Padding(10);
            keywordLayout.RowCount = 5;
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            keywordLayout.Size = new Size(488, 241);
            keywordLayout.TabIndex = 0;
            // 
            // inputKeywordBox
            // 
            inputKeywordBox.Dock = DockStyle.Fill;
            inputKeywordBox.Location = new Point(13, 13);
            inputKeywordBox.Multiline = true;
            inputKeywordBox.Name = "inputKeywordBox";
            inputKeywordBox.ScrollBars = ScrollBars.Vertical;
            inputKeywordBox.Size = new Size(228, 54);
            inputKeywordBox.TabIndex = 0;
            // 
            // convertButton
            // 
            convertButton.Dock = DockStyle.Fill;
            convertButton.Location = new Point(247, 13);
            convertButton.Name = "convertButton";
            convertButton.Size = new Size(228, 54);
            convertButton.TabIndex = 1;
            convertButton.Text = "Convert";
            convertButton.UseVisualStyleBackColor = true;
            // 
            // labelLeft
            // 
            labelLeft.Dock = DockStyle.Fill;
            labelLeft.Location = new Point(13, 70);
            labelLeft.Name = "labelLeft";
            labelLeft.Size = new Size(228, 25);
            labelLeft.TabIndex = 2;
            labelLeft.Text = "키워드 1";
            labelLeft.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelRight
            // 
            labelRight.Dock = DockStyle.Fill;
            labelRight.Location = new Point(247, 70);
            labelRight.Name = "labelRight";
            labelRight.Size = new Size(228, 25);
            labelRight.TabIndex = 3;
            labelRight.Text = "키워드 2";
            labelRight.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // leftListBox
            // 
            leftListBox.Dock = DockStyle.Fill;
            leftListBox.FormattingEnabled = true;
            leftListBox.ItemHeight = 15;
            leftListBox.Location = new Point(13, 98);
            leftListBox.Name = "leftListBox";
            leftListBox.Size = new Size(228, 70);
            leftListBox.TabIndex = 4;
            // 
            // rightListBox
            // 
            rightListBox.Dock = DockStyle.Fill;
            rightListBox.FormattingEnabled = true;
            rightListBox.ItemHeight = 15;
            rightListBox.Location = new Point(247, 98);
            rightListBox.Name = "rightListBox";
            rightListBox.Size = new Size(228, 70);
            rightListBox.TabIndex = 5;
            // 
            // copyLeftButton
            // 
            copyLeftButton.Dock = DockStyle.Right;
            copyLeftButton.Location = new Point(166, 174);
            copyLeftButton.Name = "copyLeftButton";
            copyLeftButton.Size = new Size(75, 34);
            copyLeftButton.TabIndex = 6;
            copyLeftButton.Text = "복사";
            copyLeftButton.UseVisualStyleBackColor = true;
            // 
            // copyRightButton
            // 
            copyRightButton.Dock = DockStyle.Right;
            copyRightButton.Location = new Point(400, 174);
            copyRightButton.Name = "copyRightButton";
            copyRightButton.Size = new Size(75, 34);
            copyRightButton.TabIndex = 7;
            copyRightButton.Text = "복사";
            copyRightButton.UseVisualStyleBackColor = true;
            // 
            // tabPageBlogCleaner
            // 
            tabPageBlogCleaner.Controls.Add(blogLayout);
            tabPageBlogCleaner.Location = new Point(4, 24);
            tabPageBlogCleaner.Name = "tabPageBlogCleaner";
            tabPageBlogCleaner.Padding = new Padding(10);
            tabPageBlogCleaner.Size = new Size(508, 261);
            tabPageBlogCleaner.TabIndex = 2;
            tabPageBlogCleaner.Text = "블로그 정리";
            tabPageBlogCleaner.UseVisualStyleBackColor = true;
            // 
            // blogLayout
            // 
            blogLayout.ColumnCount = 1;
            blogLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            blogLayout.Controls.Add(blogTopPanel, 0, 0);
            blogLayout.Controls.Add(labelBlogStatus, 0, 1);
            blogLayout.Controls.Add(blogWebView, 0, 2);
            blogLayout.Dock = DockStyle.Fill;
            blogLayout.Location = new Point(10, 10);
            blogLayout.Name = "blogLayout";
            blogLayout.RowCount = 3;
            blogLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            blogLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            blogLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            blogLayout.Size = new Size(488, 241);
            blogLayout.TabIndex = 0;
            // 
            // blogTopPanel
            // 
            blogTopPanel.Controls.Add(blogUrlTextBox);
            blogTopPanel.Controls.Add(buttonBlogOpen);
            blogTopPanel.Controls.Add(buttonBlogClean);
            blogTopPanel.Controls.Add(buttonBlogRefresh);
            blogTopPanel.Controls.Add(buttonBlogDownloadImages);
            blogTopPanel.Dock = DockStyle.Fill;
            blogTopPanel.Location = new Point(3, 3);
            blogTopPanel.Name = "blogTopPanel";
            blogTopPanel.Size = new Size(482, 42);
            blogTopPanel.TabIndex = 0;
            // 
            // blogUrlTextBox
            //
            blogUrlTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            blogUrlTextBox.Location = new Point(3, 9);
            blogUrlTextBox.Name = "blogUrlTextBox";
            blogUrlTextBox.PlaceholderText = "네이버 블로그 URL 입력";
            blogUrlTextBox.Size = new Size(168, 23);
            blogUrlTextBox.TabIndex = 0;
            // 
            // buttonBlogOpen
            //
            buttonBlogOpen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonBlogOpen.Location = new Point(176, 8);
            buttonBlogOpen.Name = "buttonBlogOpen";
            buttonBlogOpen.Size = new Size(48, 25);
            buttonBlogOpen.TabIndex = 1;
            buttonBlogOpen.Text = "열기";
            buttonBlogOpen.UseVisualStyleBackColor = true;
            // 
            // buttonBlogClean
            //
            buttonBlogClean.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonBlogClean.Location = new Point(229, 8);
            buttonBlogClean.Name = "buttonBlogClean";
            buttonBlogClean.Size = new Size(48, 25);
            buttonBlogClean.TabIndex = 2;
            buttonBlogClean.Text = "정리";
            buttonBlogClean.UseVisualStyleBackColor = true;
            // 
            // buttonBlogRefresh
            //
            buttonBlogRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonBlogRefresh.Location = new Point(282, 8);
            buttonBlogRefresh.Name = "buttonBlogRefresh";
            buttonBlogRefresh.Size = new Size(90, 25);
            buttonBlogRefresh.TabIndex = 3;
            buttonBlogRefresh.Text = "새로고침";
            buttonBlogRefresh.UseVisualStyleBackColor = true;
            // 
            // buttonBlogDownloadImages
            //
            buttonBlogDownloadImages.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonBlogDownloadImages.Location = new Point(380, 8);
            buttonBlogDownloadImages.Name = "buttonBlogDownloadImages";
            buttonBlogDownloadImages.Size = new Size(90, 25);
            buttonBlogDownloadImages.TabIndex = 4;
            buttonBlogDownloadImages.Text = "이미지 다운";
            buttonBlogDownloadImages.UseVisualStyleBackColor = true;
            // 
            // labelBlogStatus
            // 
            labelBlogStatus.Dock = DockStyle.Fill;
            labelBlogStatus.Location = new Point(3, 48);
            labelBlogStatus.Name = "labelBlogStatus";
            labelBlogStatus.Padding = new Padding(4, 0, 0, 0);
            labelBlogStatus.Size = new Size(482, 28);
            labelBlogStatus.TabIndex = 1;
            labelBlogStatus.Text = "상태: 대기 중";
            labelBlogStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // blogWebView
            // 
            blogWebView.AllowExternalDrop = true;
            blogWebView.CreationProperties = null;
            blogWebView.DefaultBackgroundColor = Color.White;
            blogWebView.Dock = DockStyle.Fill;
            blogWebView.Location = new Point(3, 79);
            blogWebView.Name = "blogWebView";
            blogWebView.Size = new Size(482, 159);
            blogWebView.Source = new Uri("https://blog.naver.com", UriKind.Absolute);
            blogWebView.TabIndex = 2;
            blogWebView.ZoomFactor = 1D;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(516, 289);
            Controls.Add(tabControl);
            MinimumSize = new Size(516, 289);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Monthly Excel Manager by.silee";
            tabControl.ResumeLayout(false);
            tabPageCrawling.ResumeLayout(false);
            tableLayoutPanel.ResumeLayout(false);
            tabPageKeyword.ResumeLayout(false);
            keywordLayout.ResumeLayout(false);
            keywordLayout.PerformLayout();
            tabPageBlogCleaner.ResumeLayout(false);
            blogLayout.ResumeLayout(false);
            blogTopPanel.ResumeLayout(false);
            blogTopPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)blogWebView).EndInit();
            ResumeLayout(false);
        }

        #endregion
    }
}
