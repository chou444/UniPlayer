using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace UniPlayer.Forms
{
    public partial class HistoryForm : Form
    {
        private readonly List<string> _history;
        private readonly Action<string> _onReplay;

        public HistoryForm(List<string> history, Action<string> onReplay)
        {
            InitializeComponent();
            _history = history;
            _onReplay = onReplay;
            RefreshList();
        }

        public void RefreshList()
        {
            listBoxHistory.Items.Clear();
            foreach (string path in _history)
            {
                listBoxHistory.Items.Add(System.IO.Path.GetFileNameWithoutExtension(path));
            }
        }

        private void listBoxHistory_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxHistory.SelectedIndex >= 0 && listBoxHistory.SelectedIndex < _history.Count)
            {
                _onReplay?.Invoke(_history[listBoxHistory.SelectedIndex]);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _history.Clear();
            RefreshList();
        }
    }
}
