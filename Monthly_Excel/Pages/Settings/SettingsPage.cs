using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Monthly_Excel.UI;

namespace Monthly_Excel.Pages.Settings
{
    public class SettingsPage : UserControl
    {
        private readonly CheckedListBox _tabList;
        private readonly Button _moveUpButton;
        private readonly Button _moveDownButton;
        private readonly Button _resetButton;
        private readonly Button _applyButton;
        private int _dragIndex = -1;

        public event EventHandler? ApplyRequested;
        public event EventHandler? ResetRequested;

        public SettingsPage()
        {
            AppTheme.ApplyPage(this);

            var surface = AppTheme.CreateSurfacePanel();
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = AppTheme.SurfaceBackground
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var titleLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "탭 표시/순서 설정",
                TextAlign = ContentAlignment.MiddleLeft
            };
            AppTheme.StyleSectionLabel(titleLabel);

            var guideLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "체크 해제: 숨김 / 드래그 또는 위아래 버튼: 순서 이동",
                TextAlign = ContentAlignment.MiddleLeft
            };
            AppTheme.StyleStatusLabel(guideLabel);

            _tabList = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppTheme.SurfaceBackground,
                ForeColor = AppTheme.TextPrimary,
                Font = AppTheme.BodyFont,
                ItemHeight = 22,
                AllowDrop = true
            };

            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Margin = new Padding(8, 0, 0, 0)
            };
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

            _moveUpButton = new Button
            {
                Dock = DockStyle.Fill,
                Text = "위로",
                Margin = new Padding(0)
            };

            _moveDownButton = new Button
            {
                Dock = DockStyle.Fill,
                Text = "아래로",
                Margin = new Padding(0)
            };

            _resetButton = new Button
            {
                Dock = DockStyle.Fill,
                Text = "초기화",
                Margin = new Padding(0)
            };

            _applyButton = new Button
            {
                Dock = DockStyle.Fill,
                Text = "적용",
                Margin = new Padding(0)
            };

            AppTheme.StyleSecondaryButton(_moveUpButton);
            AppTheme.StyleSecondaryButton(_moveDownButton);
            AppTheme.StyleSecondaryButton(_resetButton);
            AppTheme.StylePrimaryButton(_applyButton);

            buttonPanel.Controls.Add(_moveUpButton, 0, 0);
            buttonPanel.Controls.Add(_moveDownButton, 0, 1);
            buttonPanel.Controls.Add(_resetButton, 0, 2);
            buttonPanel.Controls.Add(_applyButton, 0, 3);

            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(guideLabel, 0, 1);
            layout.Controls.Add(_tabList, 0, 2);
            layout.Controls.Add(buttonPanel, 1, 2);

            surface.Controls.Add(layout);
            Controls.Add(surface);

            _moveUpButton.Click += (_, _) => MoveSelectedItem(-1);
            _moveDownButton.Click += (_, _) => MoveSelectedItem(1);
            _resetButton.Click += (_, _) => ResetRequested?.Invoke(this, EventArgs.Empty);
            _applyButton.Click += (_, _) => ApplyRequested?.Invoke(this, EventArgs.Empty);

            _tabList.MouseDown += TabList_MouseDown;
            _tabList.MouseMove += TabList_MouseMove;
            _tabList.DragOver += TabList_DragOver;
            _tabList.DragDrop += TabList_DragDrop;
            _tabList.KeyDown += TabList_KeyDown;
        }

        public void SetTabs(IReadOnlyList<SettingsTabState> tabs)
        {
            _tabList.Items.Clear();

            foreach (var tab in tabs)
            {
                int index = _tabList.Items.Add(new SettingsTabState(tab.Key, tab.Title, tab.Visible));
                _tabList.SetItemChecked(index, tab.Visible);
            }

            if (_tabList.Items.Count > 0)
            {
                _tabList.SelectedIndex = 0;
            }
        }

        public IReadOnlyList<SettingsTabState> GetTabs()
        {
            return Enumerable
                .Range(0, _tabList.Items.Count)
                .Select(index =>
                {
                    var item = (SettingsTabState)_tabList.Items[index];
                    return new SettingsTabState(item.Key, item.Title, _tabList.GetItemChecked(index));
                })
                .ToList();
        }

        private void MoveSelectedItem(int direction)
        {
            int currentIndex = _tabList.SelectedIndex;
            if (currentIndex < 0)
            {
                return;
            }

            int targetIndex = currentIndex + direction;
            if (targetIndex < 0 || targetIndex >= _tabList.Items.Count)
            {
                return;
            }

            MoveItem(currentIndex, targetIndex);
        }

        private void MoveItem(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex || fromIndex < 0 || toIndex < 0 || fromIndex >= _tabList.Items.Count || toIndex >= _tabList.Items.Count)
            {
                return;
            }

            var fromItem = _tabList.Items[fromIndex];
            bool fromChecked = _tabList.GetItemChecked(fromIndex);
            bool toChecked = _tabList.GetItemChecked(toIndex);
            var toItem = _tabList.Items[toIndex];

            _tabList.Items[fromIndex] = toItem;
            _tabList.Items[toIndex] = fromItem;
            _tabList.SetItemChecked(fromIndex, toChecked);
            _tabList.SetItemChecked(toIndex, fromChecked);
            _tabList.SelectedIndex = toIndex;
        }

        private void TabList_MouseDown(object? sender, MouseEventArgs e)
        {
            _dragIndex = _tabList.IndexFromPoint(e.Location);
        }

        private void TabList_MouseMove(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || _dragIndex < 0 || _dragIndex >= _tabList.Items.Count)
            {
                return;
            }

            _tabList.DoDragDrop(_tabList.Items[_dragIndex], DragDropEffects.Move);
        }

        private void TabList_DragOver(object? sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void TabList_DragDrop(object? sender, DragEventArgs e)
        {
            if (_dragIndex < 0 || _dragIndex >= _tabList.Items.Count)
            {
                return;
            }

            var point = _tabList.PointToClient(new Point(e.X, e.Y));
            int targetIndex = _tabList.IndexFromPoint(point);
            if (targetIndex < 0)
            {
                targetIndex = _tabList.Items.Count - 1;
            }

            MoveItem(_dragIndex, targetIndex);
            _dragIndex = -1;
        }

        private void TabList_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_tabList.Items.Count == 0)
            {
                return;
            }

            if (e.Control && e.KeyCode == Keys.Up)
            {
                MoveSelectedItem(-1);
                e.Handled = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.Down)
            {
                MoveSelectedItem(1);
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Up && _tabList.SelectedIndex > 0)
            {
                _tabList.SelectedIndex -= 1;
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Down && _tabList.SelectedIndex < _tabList.Items.Count - 1)
            {
                _tabList.SelectedIndex += 1;
                e.Handled = true;
            }
        }
    }

    public sealed class SettingsTabState
    {
        public SettingsTabState(string key, string title, bool visible)
        {
            Key = key;
            Title = title;
            Visible = visible;
        }

        public string Key { get; }
        public string Title { get; }
        public bool Visible { get; }

        public override string ToString() => Title;
    }
}
