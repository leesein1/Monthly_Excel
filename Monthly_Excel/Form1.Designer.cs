namespace Monthly_Excel
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageCrawling;
        private System.Windows.Forms.TabPage tabPageKeyword;
        private System.Windows.Forms.TabPage tabPageBlogCleaner;
        private System.Windows.Forms.TabPage tabPageInspector;
        private System.Windows.Forms.TabPage tabPageImageConverter;
        private System.Windows.Forms.TabPage tabPageSettings;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            tabControl = new System.Windows.Forms.TabControl();
            tabPageCrawling = new System.Windows.Forms.TabPage();
            tabPageKeyword = new System.Windows.Forms.TabPage();
            tabPageBlogCleaner = new System.Windows.Forms.TabPage();
            tabPageInspector = new System.Windows.Forms.TabPage();
            tabPageImageConverter = new System.Windows.Forms.TabPage();
            tabPageSettings = new System.Windows.Forms.TabPage();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageCrawling);
            tabControl.Controls.Add(tabPageKeyword);
            tabControl.Controls.Add(tabPageBlogCleaner);
            tabControl.Controls.Add(tabPageInspector);
            tabControl.Controls.Add(tabPageImageConverter);
            tabControl.Controls.Add(tabPageSettings);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Location = new System.Drawing.Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(516, 289);
            tabControl.TabIndex = 0;
            // 
            // tabPageCrawling
            // 
            tabPageCrawling.Location = new System.Drawing.Point(4, 24);
            tabPageCrawling.Name = "tabPageCrawling";
            tabPageCrawling.Padding = new System.Windows.Forms.Padding(10);
            tabPageCrawling.Size = new System.Drawing.Size(508, 261);
            tabPageCrawling.TabIndex = 0;
            tabPageCrawling.Text = "크롤링";
            tabPageCrawling.UseVisualStyleBackColor = true;
            // 
            // tabPageKeyword
            // 
            tabPageKeyword.Location = new System.Drawing.Point(4, 24);
            tabPageKeyword.Name = "tabPageKeyword";
            tabPageKeyword.Padding = new System.Windows.Forms.Padding(10);
            tabPageKeyword.Size = new System.Drawing.Size(508, 261);
            tabPageKeyword.TabIndex = 1;
            tabPageKeyword.Text = "키워드";
            tabPageKeyword.UseVisualStyleBackColor = true;
            // 
            // tabPageBlogCleaner
            // 
            tabPageBlogCleaner.Location = new System.Drawing.Point(4, 24);
            tabPageBlogCleaner.Name = "tabPageBlogCleaner";
            tabPageBlogCleaner.Padding = new System.Windows.Forms.Padding(10);
            tabPageBlogCleaner.Size = new System.Drawing.Size(508, 261);
            tabPageBlogCleaner.TabIndex = 2;
            tabPageBlogCleaner.Text = "블로그 정리";
            tabPageBlogCleaner.UseVisualStyleBackColor = true;
            // 
            // tabPageInspector
            // 
            tabPageInspector.Location = new System.Drawing.Point(4, 24);
            tabPageInspector.Name = "tabPageInspector";
            tabPageInspector.Padding = new System.Windows.Forms.Padding(10);
            tabPageInspector.Size = new System.Drawing.Size(508, 261);
            tabPageInspector.TabIndex = 3;
            tabPageInspector.Text = "검사기";
            tabPageInspector.UseVisualStyleBackColor = true;
            // 
            // tabPageImageConverter
            // 
            tabPageImageConverter.Location = new System.Drawing.Point(4, 24);
            tabPageImageConverter.Name = "tabPageImageConverter";
            tabPageImageConverter.Padding = new System.Windows.Forms.Padding(10);
            tabPageImageConverter.Size = new System.Drawing.Size(508, 261);
            tabPageImageConverter.TabIndex = 4;
            tabPageImageConverter.Text = "이미지 변환";
            tabPageImageConverter.UseVisualStyleBackColor = true;
            // 
            // tabPageSettings
            // 
            tabPageSettings.Location = new System.Drawing.Point(4, 24);
            tabPageSettings.Name = "tabPageSettings";
            tabPageSettings.Padding = new System.Windows.Forms.Padding(10);
            tabPageSettings.Size = new System.Drawing.Size(508, 261);
            tabPageSettings.TabIndex = 5;
            tabPageSettings.Text = "설정";
            tabPageSettings.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(516, 289);
            Controls.Add(tabControl);
            Name = "Form1";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Monthly Excel Manager by.silee";
            ResumeLayout(false);
        }

        #endregion
    }
}
