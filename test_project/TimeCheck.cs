namespace test_project
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public class TextInputMonitor
    {
        public List<string> list = new ();
        private RichTextBox _richTextBox;
        private RichTextBox _richTextBox2;
        private Timer _cursorTimer;
        private DateTime _lastCursorMoveTime;
        private Point _lastCursorPosition;
        private MarkdownEditor logic;
        private MainForm _mainForm;

        public TextInputMonitor()
        {
            logic = new MarkdownEditor();
        }

        public TextInputMonitor(RichTextBox richTextBox, MainForm mainForm)
        {
            _richTextBox = richTextBox;
            _mainForm = mainForm;
            logic = new MarkdownEditor();
            _lastCursorPosition = Cursor.Position;

            _cursorTimer = new Timer();
            _cursorTimer.Interval = 1000;
            _cursorTimer.Tick += CheckCursorActivity;

            _richTextBox.MouseMove += OnCursorMoved;
            _richTextBox.GotFocus += OnControlFocused;
        }

        public void TextInputMonitorNotVisible(RichTextBox richTextBox)
        {
            _richTextBox2 = richTextBox;
        }

        public void StartMonitoring()
        {
            _cursorTimer.Start();
        }

        public void StopMonitoring()
        {
            _cursorTimer.Stop();
        }

        private void OnCursorMoved(object sender, MouseEventArgs e)
        {
            UpdateCursorActivity();
        }

        private void OnControlFocused(object sender, EventArgs e)
        {
            UpdateCursorActivity();
        }

        private void UpdateCursorActivity()
        {
            if (Cursor.Position != _lastCursorPosition)
            {
                _lastCursorPosition = Cursor.Position;
                _lastCursorMoveTime = DateTime.Now;
            }
        }

        private void CheckCursorActivity(object sender, EventArgs e)
        {
            if ((DateTime.Now - _lastCursorMoveTime).TotalSeconds >= 50)
            {
                PerformActions();
                _lastCursorMoveTime = DateTime.Now;
            }
        }

        private void PerformActions()
        {
            _richTextBox2.Rtf = _richTextBox.Rtf;

            var analyzer_md = new NormalTextFormatAnalyzer();
            Dictionary<NormalTextFormatAnalyzer.FormattingStyle, List<int[]>> formating = analyzer_md.SaveFormatting(_richTextBox2);

            string[] stringArray = _richTextBox2.Lines;

            logic.SaveContent(stringArray);
            logic.SaveFormating(formating.ToDictionary(kvp => (FormattingStyle)kvp.Key, kvp => kvp.Value));

            string filePath = _mainForm.lastActionWasCreate ? _mainForm.create_filePath : _mainForm.open_filePath;
            SavingStatus status = logic.SaveFile(filePath);
        }
    }
}
