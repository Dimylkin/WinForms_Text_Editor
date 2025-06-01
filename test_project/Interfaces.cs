namespace test_project
{
    public class MainForm : Form
    {
        private MenuStrip mainMenu;
        private Panel separator_top;
        private Panel separator_bottom;
        private Panel mainPanel;
        private Panel labelPanel;
        private Label rightLabel;
        private Label leftLabel;
        private RichTextBox richTextBox; 
        private RichTextBox richTextBox2;

        private Button btnCreate;
        private Button btnOpen;
        private Button btnSave;
        private Button btnSaveAs;

        private Button btnBold;
        private Button btnItalic;
        private Button btnUnderline;
        private Button btnStrikeout;

        private Button btnCopy;
        private Button btnPaste;

        public string create_filePath;
        public string open_filePath;
        public bool lastActionWasCreate = false;

        private Functional functional;
        private TextInputMonitor monitor;
        SavingStatus status;

        private ToolTip toolTip1;
        private readonly Dictionary<Keys, Action> _keyActions = new Dictionary<Keys, Action>();
        private readonly Dictionary<Keys, Button> _keyButtons = new Dictionary<Keys, Button>();


        public MainForm()
        {
            Text = "Текстовый редактор";
            WindowState = FormWindowState.Maximized;

            Interfaces();
            Theme();

            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
            InitializeHotKeys();

            functional = new Functional();
            monitor = new TextInputMonitor(richTextBox, this);
            monitor.TextInputMonitorNotVisible(richTextBox2);
        }

        private void Interfaces()
        {
            mainMenu = new MenuStrip()
            {
                AutoSize = false,
                Size = new Size(1920, 35),
                Dock = DockStyle.Top
            };

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var fileMenuItem = new ToolStripMenuItem("📄 Файл")
            {
                Font = new Font("Times New Roman", 10),
                AutoSize = false,
                Size = new Size(90, 35),
                BackColor = Color.White
            };

            var formatMenuItem = new ToolStripMenuItem("</> Формат")
            {
                Font = new Font("Times New Roman", 10),
                AutoSize = false,
                Size = new Size(90, 35)
            };

            var insertMenuItem = new ToolStripMenuItem("🔗 Вставка")
            {
                Font = new Font("Times New Roman", 10),
                AutoSize = false,
                Size = new Size(90, 35)
            };

            mainMenu.Items.AddRange([
                fileMenuItem,
                formatMenuItem,
                insertMenuItem
            ]);

            separator_top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1
            };

            separator_bottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1
            };

            Panel file_panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Visible = true
            };

            Panel format_panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Visible = false
            };

            Panel insert_panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Visible = false
            };

            labelPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 35,
                AutoScroll = true
            };

            richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Times New Roman", 15),
                BorderStyle = BorderStyle.None
            };

            richTextBox2 = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Times New Roman", 15),
                BorderStyle = BorderStyle.None,
                Visible = false,
            };

            TableLayoutPanel tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(5)
            };

            leftLabel = new Label
            {
                Text = " ⚠️ Есть несохраненные изменения",
                Font = new Font("Times New Roman", 10),
                Dock = DockStyle.Left,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            rightLabel = new Label
            {
                Text = " ⚠️ Файл не найден",
                Font = new Font("Times New Roman", 10),
                Dock = DockStyle.Right,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            toolTip1 = new ToolTip()
            {
                AutoPopDelay = 5000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                ShowAlways = true
            };

            fileMenuItem.Click += (sender, e) =>
            {
                fileMenuItem.BackColor = Color.White;
                formatMenuItem.BackColor = SystemColors.Control;
                formatMenuItem.ForeColor = Color.Black;
                insertMenuItem.BackColor = SystemColors.Control;
                insertMenuItem.ForeColor = Color.Black;
                file_panel.Visible = true;
                format_panel.Visible = false;
                insert_panel.Visible = false;
            };

            formatMenuItem.Click += (sender, e) =>
            {
                formatMenuItem.BackColor = Color.White;
                fileMenuItem.BackColor = SystemColors.Control;
                fileMenuItem.ForeColor = Color.Black;
                insertMenuItem.BackColor = SystemColors.Control;
                insertMenuItem.ForeColor = Color.Black;
                format_panel.Visible = true;
                file_panel.Visible = false;
                insert_panel.Visible = false;
            };

            insertMenuItem.Click += (sender, e) =>
            {
                insertMenuItem.BackColor = Color.White;
                fileMenuItem.BackColor = SystemColors.Control;
                fileMenuItem.ForeColor = Color.Black;
                formatMenuItem.BackColor = SystemColors.Control;
                formatMenuItem.ForeColor = Color.Black;
                insert_panel.Visible = true;
                file_panel.Visible = false;
                format_panel.Visible = false;
            };

            file_panel.Controls.AddRange(
            [
                CreateButton("✚ Создать", FontStyle.Regular, new Point(5, 5), 80, 30, ref btnCreate, clickHandler: CreateFileInterface),
                CreateButton("✎ Открыть", FontStyle.Regular, new Point(95, 5), 80, 30, ref btnOpen, clickHandler: OpenFileInterface),
                CreateButton("💾 Сохранить", FontStyle.Regular, new Point(185, 5), 100, 30, ref btnSave, clickHandler: SaveFileInterface),
                CreateButton("💾 Сохранить как", FontStyle.Regular, new Point(295, 5), 120, 30, ref btnSaveAs, clickHandler: SaveAsFileInterface)
            ]);

            format_panel.Controls.AddRange(
            [
                CreateButton("Жирный", FontStyle.Bold, new Point(5, 5), 80, 30, ref btnBold, clickHandler: Bold),
                CreateButton("Курсив", FontStyle.Italic, new Point(90, 5), 80, 30, ref btnItalic, clickHandler: Italic),
                CreateButton("Подчеркнутый", FontStyle.Underline, new Point(175, 5), 110, 30, ref btnUnderline, clickHandler: UnderLine),
                CreateButton("Зачеркнутый", FontStyle.Strikeout, new Point(290, 5), 110, 30, ref btnStrikeout, clickHandler: CrossedLine),
            ]);

            insert_panel.Controls.AddRange(
            [
                CreateButton("🗐 Скопировать", FontStyle.Regular, new Point(5, 5), 120, 30, ref btnCopy, clickHandler: Copy),
                CreateButton("📋 Вставить", FontStyle.Regular, new Point(135, 5), 100, 30, ref btnPaste, clickHandler: Paste),
            ]);

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            tableLayout.Controls.Add(leftLabel, 0, 0);
            tableLayout.Controls.Add(rightLabel, 1, 0);

            labelPanel.Controls.Add(tableLayout);

            mainPanel.Controls.Add(richTextBox);

            Controls.Add(separator_bottom);
            Controls.Add(labelPanel);
            Controls.Add(mainPanel);
            Controls.Add(separator_top);
            Controls.Add(file_panel);
            Controls.Add(format_panel);
            Controls.Add(insert_panel);
            Controls.Add(mainMenu);
        }

        private void Theme()
        {
            BackColor = Color.White;
            ForeColor = Color.Black;

            mainMenu.BackColor = SystemColors.Control;
            mainMenu.ForeColor = Color.Black;

            separator_top.BackColor = SystemColors.ControlDark;

            separator_bottom.BackColor = SystemColors.ControlDark;

            mainPanel.BackColor = Color.White;

            labelPanel.BackColor = SystemColors.Control;
            labelPanel.ForeColor = Color.Black;

            foreach (Control control in mainPanel.Controls)
            {
                if (control is Label label)
                {
                    label.ForeColor = Color.Black;
                }
            }
        }

        private void InitializeHotKeys()
        {
            RegisterHotKey(Keys.Control | Keys.N, btnCreate, CreateFileInterface);
            RegisterHotKey(Keys.Control | Keys.O, btnOpen, OpenFileInterface);
            RegisterHotKey(Keys.Control | Keys.S, btnSave, SaveFileInterface);
            RegisterHotKey(Keys.Control | Keys.Shift | Keys.S, btnSaveAs, SaveAsFileInterface);

            RegisterHotKey(Keys.Control | Keys.B, btnBold, Bold);
            RegisterHotKey(Keys.Control | Keys.I, btnItalic, Italic);
            RegisterHotKey(Keys.Control | Keys.U, btnUnderline, UnderLine);
            RegisterHotKey(Keys.Control | Keys.K, btnStrikeout, CrossedLine);

            RegisterHotKey(Keys.Control | Keys.C, btnCopy, Copy);
            RegisterHotKey(Keys.Control | Keys.V, btnPaste, Paste);
        }

        private void RegisterHotKey(Keys keyCombination, Button button, EventHandler handler)
        {
            _keyActions[keyCombination] = () => handler(button, EventArgs.Empty);
            _keyButtons[keyCombination] = button;

            string keyText = keyCombination.ToString()
                .Replace("Control", "Ctrl")
                .Replace(", ", "+");
            toolTip1.SetToolTip(button, $"{button.Text} ({keyText})");
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            Keys keyData = e.KeyData;

            if (keyData == Keys.ControlKey || keyData == Keys.ShiftKey || keyData == Keys.Menu)
                return;

            if (_keyActions.TryGetValue(keyData, out Action action))
            {
                action.Invoke();
                e.Handled = true;
                e.SuppressKeyPress = true;

                if (_keyButtons.TryGetValue(keyData, out Button btn))
                {
                    FlashButton(btn);
                }
            }
        }

        private async void FlashButton(Button btn)
        {
            Color originalColor = btn.BackColor;
            btn.BackColor = Color.LightSkyBlue;
            await Task.Delay(100);
            btn.BackColor = originalColor;
        }

        private Button CreateButton(string text, FontStyle style, Point location, int width, int height, ref Button field, EventHandler clickHandler = null)
        {
            var button = new Button
            {
                Width = width,
                Height = height,
                Text = text,
                Font = new Font("Times New Roman", 10, style),
                Location = location
            };

            if (clickHandler != null)
                button.Click += clickHandler;

            field = button;
            return button;
        }

        public void CreateFileInterface(object sender, EventArgs e)
        {
            create_filePath = functional.CreateFileForInterface(sender, e);
            if (create_filePath != null)
            {
                string fileName = Path.GetFileName(create_filePath);
                rightLabel.Text = $" ✅ Выбранный файл: {fileName}";
                MessageBox.Show($"{fileName} успешно создан", "✅ Успешно! ✅");
                lastActionWasCreate = true;
                monitor.StartMonitoring();
            }
            else
            {
                rightLabel.Text = " ❌ Файл не найден";
                MessageBox.Show("При создании файла произошла ошибка. \n Поробуйте ещё раз.", "❌ Ошибка! ❌");
            }
        }

        public void OpenFileInterface(object sender, EventArgs e)
        {
            open_filePath = functional.OpenFileForInterface(sender, e, richTextBox);
            if (open_filePath != null)
            {
                string fileName = Path.GetFileName(open_filePath);
                rightLabel.Text = $" ✅ Выбранный файл: {fileName}";
                lastActionWasCreate = false;
                monitor.StopMonitoring();
                monitor.StartMonitoring();
                leftLabel.Text = " ❌ Есть несохраненные изменения";
            }
            else
            {
                rightLabel.Text = "❌ Выбранный файл: Файл не выбран";
                MessageBox.Show("При открытии файла произошла ошибка. \n Попробуйте ещё раз.", "❌ Ошибка! ❌");
            }
        }

        public void SaveFileInterface(object sender, EventArgs e)
        {
            string pathToUse = lastActionWasCreate ? create_filePath : open_filePath;

            if (string.IsNullOrEmpty(pathToUse))
            {
                DialogResult result = MessageBox.Show(
                    "Файл для сохранения не выбран. Хотите создать его?",
                    "Сохранение",
                    MessageBoxButtons.YesNo
                );

                if (result == DialogResult.Yes)
                {
                    CreateFileInterface(sender, e);
                    if (create_filePath != null)
                    {
                        pathToUse = create_filePath;
                        monitor.StopMonitoring();
                        functional.SaveFileForInterface(richTextBox, pathToUse);
                        leftLabel.Text = functional.GetStatusMessage(status, pathToUse);
                    }
                }
                return;
            }

            monitor.StopMonitoring();
            functional.SaveFileForInterface(richTextBox, pathToUse);
            leftLabel.Text = functional.GetStatusMessage(status, pathToUse);
        }

        public void SaveAsFileInterface(object sender, EventArgs e)
        {
            monitor.StopMonitoring();
            string pathToUse = functional.SaveAsFileForInterface(sender, e, richTextBox);
            leftLabel.Text = functional.GetStatusMessage(status, pathToUse);
        }

        private void Bold(object sender, EventArgs e) => ToggleFontStyleExclusive(FontStyle.Bold);
        private void Italic(object sender, EventArgs e) => ToggleFontStyleExclusive(FontStyle.Italic);
        private void UnderLine(object sender, EventArgs e) => ToggleFontStyleExclusive(FontStyle.Underline);
        private void CrossedLine(object sender, EventArgs e) => ToggleFontStyleExclusive(FontStyle.Strikeout);

        private void ToggleFontStyleExclusive(FontStyle style)
        {
            Font currentFont = richTextBox.SelectionFont;
            FontStyle newStyle;

            if (currentFont == null)
            {
                richTextBox.SelectionFont = new Font("Times New Roman", 15, style);
                return;
            }

            if (currentFont.Style == style)
            {
                newStyle = FontStyle.Regular;
            }

            else
            {
                newStyle = style;
            }

            richTextBox.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle);
        }

        private void Copy(object sender, EventArgs e)
        {
            richTextBox.Copy();
        }

        private void Paste(object sender, EventArgs e)
        {
            richTextBox.Paste();
        }
    }
}