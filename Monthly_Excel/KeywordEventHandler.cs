using System;
using System.Text;
using System.Windows.Forms;

namespace Monthly_Excel
{
    public class KeywordEventHandler
    {
        private readonly TextBox inputKeywordBox;
        private readonly ListBox leftListBox;
        private readonly ListBox rightListBox;

        public KeywordEventHandler(TextBox input, ListBox left, ListBox right)
        {
            inputKeywordBox = input;
            leftListBox = left;
            rightListBox = right;
        }

        public void OnConvertClicked(object sender, EventArgs e)
        {
            if (inputKeywordBox == null || leftListBox == null || rightListBox == null)
                return;

            leftListBox.Items.Clear();
            rightListBox.Items.Clear();

            var (left, right) = KeywordProcessor.ProcessKeywords(inputKeywordBox.Text);

            foreach (var item in left) leftListBox.Items.Add(item);
            foreach (var item in right) rightListBox.Items.Add(item);
        }

        public void OnCopyLeftClicked(object sender, EventArgs e)
        {
            if (leftListBox.Items.Count == 0)
            {
                MessageBox.Show("복사할 항목이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string result = BuildExcelSafeString(leftListBox);
            Clipboard.SetText(result);
            MessageBox.Show("왼쪽 키워드가 복사되었습니다.", "복사 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void OnCopyRightClicked(object sender, EventArgs e)
        {
            if (rightListBox.Items.Count == 0)
            {
                MessageBox.Show("복사할 항목이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string result = BuildExcelSafeString(rightListBox);
            Clipboard.SetText(result);
            MessageBox.Show("오른쪽 키워드가 복사되었습니다.", "복사 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string BuildExcelSafeString(ListBox listBox)
        {
            var sb = new StringBuilder();

            foreach (var item in listBox.Items)
            {
                sb.AppendLine(item?.ToString());
            }

            return $"\"{sb.ToString().TrimEnd('\r', '\n')}\"";
        }
    }
}
