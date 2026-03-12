using System;
using System.Text;
using System.Windows.Forms;
using Monthly_Excel.Processors;

namespace Monthly_Excel.Handlers
{
    public class KeywordEventHandler
    {
        private readonly TextBox _inputKeywordBox;
        private readonly ListBox _leftListBox;
        private readonly ListBox _rightListBox;

        public KeywordEventHandler(TextBox inputKeywordBox, ListBox leftListBox, ListBox rightListBox)
        {
            _inputKeywordBox = inputKeywordBox;
            _leftListBox = leftListBox;
            _rightListBox = rightListBox;
        }

        public void OnConvertClicked(object? sender, EventArgs e)
        {
            _leftListBox.Items.Clear();
            _rightListBox.Items.Clear();

            var (leftKeywords, rightKeywords) = KeywordProcessor.ProcessKeywords(_inputKeywordBox.Text);

            foreach (var keyword in leftKeywords)
            {
                _leftListBox.Items.Add(keyword);
            }

            foreach (var keyword in rightKeywords)
            {
                _rightListBox.Items.Add(keyword);
            }
        }

        public void OnCopyLeftClicked(object? sender, EventArgs e)
        {
            CopyItems(_leftListBox, "왼쪽 키워드가 복사되었습니다.");
        }

        public void OnCopyRightClicked(object? sender, EventArgs e)
        {
            CopyItems(_rightListBox, "오른쪽 키워드가 복사되었습니다.");
        }

        private void CopyItems(ListBox listBox, string successMessage)
        {
            if (listBox.Items.Count == 0)
            {
                MessageBox.Show("복사할 항목이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Clipboard.SetText(BuildExcelSafeString(listBox));
            MessageBox.Show(successMessage, "복사 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string BuildExcelSafeString(ListBox listBox)
        {
            var builder = new StringBuilder();

            foreach (var item in listBox.Items)
            {
                builder.AppendLine(item?.ToString());
            }

            return $"\"{builder.ToString().TrimEnd('\r', '\n')}\"";
        }
    }
}
