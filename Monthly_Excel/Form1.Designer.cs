namespace Monthly_Excel
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // 공통 UI
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

        // 키워드 탭 요소
        private System.Windows.Forms.TextBox inputKeywordBox;
        private System.Windows.Forms.Button convertButton;
        private System.Windows.Forms.ListBox leftListBox;
        private System.Windows.Forms.ListBox rightListBox;
        private System.Windows.Forms.Button copyLeftButton;
        private System.Windows.Forms.Button copyRightButton;
        private System.Windows.Forms.Label labelLeft;
        private System.Windows.Forms.Label labelRight;

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
            tabControl.SuspendLayout();
            tabPageCrawling.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            tabPageKeyword.SuspendLayout();
            keywordLayout.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageCrawling);
            tabControl.Controls.Add(tabPageKeyword);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(500, 280);
            tabControl.TabIndex = 0;
            // 
            // tabPageCrawling
            // 
            tabPageCrawling.Controls.Add(tableLayoutPanel);
            tabPageCrawling.Location = new Point(4, 24);
            tabPageCrawling.Name = "tabPageCrawling";
            tabPageCrawling.Padding = new Padding(10);
            tabPageCrawling.Size = new Size(492, 252);
            tabPageCrawling.TabIndex = 0;
            tabPageCrawling.Text = "크롤링";
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
            tableLayoutPanel.Size = new Size(472, 232);
            tableLayoutPanel.TabIndex = 0;
            // 
            // labelUpload
            // 
            labelUpload.Dock = DockStyle.Fill;
            labelUpload.Location = new Point(23, 20);
            labelUpload.Name = "labelUpload";
            labelUpload.Size = new Size(166, 40);
            labelUpload.TabIndex = 0;
            labelUpload.Text = "📅 엑셀 업로드";
            labelUpload.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonUpload
            // 
            buttonUpload.Dock = DockStyle.Fill;
            buttonUpload.Location = new Point(195, 23);
            buttonUpload.Name = "buttonUpload";
            buttonUpload.Size = new Size(254, 34);
            buttonUpload.TabIndex = 1;
            buttonUpload.Text = "파일 선택";
            // 
            // labelDownload
            // 
            labelDownload.Dock = DockStyle.Fill;
            labelDownload.Location = new Point(23, 60);
            labelDownload.Name = "labelDownload";
            labelDownload.Size = new Size(166, 40);
            labelDownload.TabIndex = 2;
            labelDownload.Text = "📄 엑셀 다운로드";
            labelDownload.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonDownload
            // 
            buttonDownload.Dock = DockStyle.Fill;
            buttonDownload.Location = new Point(195, 63);
            buttonDownload.Name = "buttonDownload";
            buttonDownload.Size = new Size(254, 34);
            buttonDownload.TabIndex = 3;
            buttonDownload.Text = "다운로드";
            // 
            // labelTemplate
            // 
            labelTemplate.Dock = DockStyle.Fill;
            labelTemplate.Location = new Point(23, 100);
            labelTemplate.Name = "labelTemplate";
            labelTemplate.Size = new Size(166, 40);
            labelTemplate.TabIndex = 4;
            labelTemplate.Text = "📘 양식 다운로드";
            labelTemplate.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonTemplateDownload
            // 
            buttonTemplateDownload.Dock = DockStyle.Fill;
            buttonTemplateDownload.Location = new Point(195, 103);
            buttonTemplateDownload.Name = "buttonTemplateDownload";
            buttonTemplateDownload.Size = new Size(254, 34);
            buttonTemplateDownload.TabIndex = 5;
            buttonTemplateDownload.Text = "양식 받기";
            // 
            // labelStatus
            // 
            tableLayoutPanel.SetColumnSpan(labelStatus, 2);
            labelStatus.Dock = DockStyle.Fill;
            labelStatus.Location = new Point(23, 140);
            labelStatus.Name = "labelStatus";
            labelStatus.Padding = new Padding(5, 0, 0, 0);
            labelStatus.Size = new Size(426, 25);
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
            progressBar.Size = new Size(426, 29);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 7;
            // 
            // tabPageKeyword
            // 
            tabPageKeyword.Controls.Add(keywordLayout);
            tabPageKeyword.Location = new Point(4, 24);
            tabPageKeyword.Name = "tabPageKeyword";
            tabPageKeyword.Padding = new Padding(10);
            tabPageKeyword.Size = new Size(192, 72);
            tabPageKeyword.TabIndex = 1;
            tabPageKeyword.Text = "키워드";
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
            keywordLayout.Size = new Size(172, 52);
            keywordLayout.TabIndex = 0;
            // 
            // inputKeywordBox
            // 
            inputKeywordBox.Dock = DockStyle.Fill;
            inputKeywordBox.Location = new Point(13, 13);
            inputKeywordBox.Multiline = true;
            inputKeywordBox.Name = "inputKeywordBox";
            inputKeywordBox.ScrollBars = ScrollBars.Vertical;
            inputKeywordBox.Size = new Size(70, 54);
            inputKeywordBox.TabIndex = 0;
            // 
            // convertButton
            // 
            convertButton.Dock = DockStyle.Fill;
            convertButton.Location = new Point(89, 13);
            convertButton.Name = "convertButton";
            convertButton.Size = new Size(70, 54);
            convertButton.TabIndex = 1;
            convertButton.Text = "Convert";
            // 
            // labelLeft
            // 
            labelLeft.Dock = DockStyle.Fill;
            labelLeft.Location = new Point(13, 70);
            labelLeft.Name = "labelLeft";
            labelLeft.Size = new Size(70, 25);
            labelLeft.TabIndex = 2;
            labelLeft.Text = "키워드 1";
            labelLeft.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelRight
            // 
            labelRight.Dock = DockStyle.Fill;
            labelRight.Location = new Point(89, 70);
            labelRight.Name = "labelRight";
            labelRight.Size = new Size(70, 25);
            labelRight.TabIndex = 3;
            labelRight.Text = "키워드 2";
            labelRight.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // leftListBox
            // 
            leftListBox.Dock = DockStyle.Fill;
            leftListBox.ItemHeight = 15;
            leftListBox.Location = new Point(13, 98);
            leftListBox.Name = "leftListBox";
            leftListBox.Size = new Size(70, 1);
            leftListBox.TabIndex = 4;
            // 
            // rightListBox
            // 
            rightListBox.Dock = DockStyle.Fill;
            rightListBox.ItemHeight = 15;
            rightListBox.Location = new Point(89, 98);
            rightListBox.Name = "rightListBox";
            rightListBox.Size = new Size(70, 1);
            rightListBox.TabIndex = 5;
            // 
            // copyLeftButton
            // 
            copyLeftButton.Dock = DockStyle.Right;
            copyLeftButton.Location = new Point(13, -15);
            copyLeftButton.Name = "copyLeftButton";
            copyLeftButton.Size = new Size(70, 34);
            copyLeftButton.TabIndex = 6;
            copyLeftButton.Text = "복사";
            // 
            // copyRightButton
            // 
            copyRightButton.Dock = DockStyle.Right;
            copyRightButton.Location = new Point(89, -15);
            copyRightButton.Name = "copyRightButton";
            copyRightButton.Size = new Size(70, 34);
            copyRightButton.TabIndex = 7;
            copyRightButton.Text = "복사";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 280);
            Controls.Add(tabControl);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Monthly Excel Manager by.silee";
            tabControl.ResumeLayout(false);
            tabPageCrawling.ResumeLayout(false);
            tableLayoutPanel.ResumeLayout(false);
            tabPageKeyword.ResumeLayout(false);
            keywordLayout.ResumeLayout(false);
            keywordLayout.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel keywordLayout;
    }
}
