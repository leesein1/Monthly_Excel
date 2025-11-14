using System;
using System.Reflection; // ← 버전 정보를 위해 추가
using System.Windows.Forms;

namespace Monthly_Excel
{
    public partial class Form1 : Form
    {
        private KeywordEventHandler keywordHandler;
        private CrawlingEventHandler crawlingHandler;

        public Form1()
        {
            InitializeComponent();

            // 1 폼 제목에 버전 자동 표시
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text = $"Monthly Excel Manager by.silee  v{version}";

            // 2 핸들러 인스턴스 생성
            keywordHandler = new KeywordEventHandler(inputKeywordBox, leftListBox, rightListBox);
            crawlingHandler = new CrawlingEventHandler(labelStatus, progressBar);

            // 3 키워드 관련 버튼 이벤트
            convertButton.Click += keywordHandler.OnConvertClicked;
            copyLeftButton.Click += keywordHandler.OnCopyLeftClicked;
            copyRightButton.Click += keywordHandler.OnCopyRightClicked;

            // 4 크롤링 관련 버튼 이벤트
            buttonUpload.Click += crawlingHandler.OnUploadClicked;
            buttonDownload.Click += crawlingHandler.OnDownloadClicked;
            buttonTemplateDownload.Click += crawlingHandler.OnTemplateDownloadClicked;
        }
    }
}
