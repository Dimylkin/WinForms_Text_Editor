namespace test_project
{
    class Functional
    {
        MarkdownEditor logic;
        MarkDownTextFormatAnalyzer md_parse;

        public SavingStatus status;
        public static Dictionary<FormattingStyle, List<int[]>> formating;
        private Dictionary<MarkDownTextFormatAnalyzer.FormattingStyle, List<int[]>> formating_md;

        public Functional()
        {
            logic = new MarkdownEditor();
            md_parse = new MarkDownTextFormatAnalyzer();
        }

        public string? CreateFileForInterface(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new())
            {
                saveFileDialog.Title = "Создать файл";
                saveFileDialog.Filter = "Markdown файлы (*.md)|*.md|Текстовые файлы (*.txt)|*.txt";
                saveFileDialog.FileName = "markdownText.md";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string create_filePath = saveFileDialog.FileName;
                    if (create_filePath != null)
                    {
                        logic.CreateFile(create_filePath);
                        return create_filePath;
                    }
                    else { return null; }
                }
                return null;
            }
        }

        public string OpenFileForInterface(object sender, EventArgs e, RichTextBox richTextBox)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите файл";
                openFileDialog.Filter = "Markdown файлы (*.md)|*.md|Текстовые файлы (*.txt)|*.txt";
                openFileDialog.FileName = "markdownText.md";

                if (openFileDialog.ShowDialog() != DialogResult.OK) { return null; }

                string filePath = openFileDialog.FileName;
                logic.OpenFile(filePath);

                string[] content = logic.ReturnContent();
                //formating_md = md_parse.SaveFormatting(filePath);
                formating = logic.ReturnFormating();

                richTextBox.Clear();
                ApplyFormatting(richTextBox, content, formating);
                return filePath;
            }
        }

        public void ApplyFormatting(RichTextBox richTextBox, string[] content, Dictionary<FormattingStyle, List<int[]>> formatting)
        {
            richTextBox.Clear();

            richTextBox.Text = string.Join(Environment.NewLine, content);

            foreach (var styleEntry in formatting)
            {
                FormattingStyle style = styleEntry.Key;
                List<int[]> formatInfoList = styleEntry.Value;

                foreach (int[] formatInfo in formatInfoList)
                {
                    int lineNumber = formatInfo[0];
                    int startPosition = formatInfo[1];
                    int length = formatInfo[2];

                    int lineStartIndex = richTextBox.GetFirstCharIndexFromLine(lineNumber);

                    int rtbStart = lineStartIndex + startPosition;

                    if (rtbStart >= 0 && rtbStart + length <= richTextBox.Text.Length)
                    {
                        richTextBox.Select(rtbStart, length);

                        switch (style)
                        {
                            case FormattingStyle.Bold:
                                richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Bold);
                                break;
                            case FormattingStyle.Italic:
                                richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Italic);
                                break;
                            case FormattingStyle.Underline:
                                richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Underline);
                                break;
                            case FormattingStyle.Strikethrough:
                                richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Strikeout);
                                break;
                        }
                    }
                }
            }
            richTextBox.Select(0, 0);
        }
    
        public void SaveFileForInterface(RichTextBox richTextBox, string filePath)
        {

            var analyzer_md = new NormalTextFormatAnalyzer();
            Dictionary<NormalTextFormatAnalyzer.FormattingStyle, List<int[]>> formating = analyzer_md.SaveFormatting(richTextBox);

            string[] stringArray = richTextBox.Lines;

            logic.SaveContent(stringArray);
            logic.SaveFormating(formating.ToDictionary(kvp => (FormattingStyle)kvp.Key, kvp => kvp.Value));

            SavingStatus status = logic.SaveFile(filePath);

            if (status == SavingStatus.Saved)
            {
                MessageBox.Show($"{Path.GetFileName(filePath)} успешно сохранён", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ShowSaveError(status, filePath);
            }
        }

        public string? SaveAsFileForInterface(object sender, EventArgs e, RichTextBox richTextBox)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Сохранить файл как";
                saveFileDialog.Filter = "Markdown файлы (*.md)|*.md|Текстовые файлы (*.txt)|*.txt";
                saveFileDialog.FileName = "markdownText.md";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    var analyzer_md = new NormalTextFormatAnalyzer();
                    Dictionary<NormalTextFormatAnalyzer.FormattingStyle, List<int[]>> formating = analyzer_md.SaveFormatting(richTextBox);

                    string[] stringArray = richTextBox.Lines;

                    logic.SaveContent(stringArray);
                    logic.SaveFormating(formating.ToDictionary(kvp => (FormattingStyle)kvp.Key, kvp => kvp.Value));

                    SavingStatus status = logic.SaveFile(filePath);

                    if (status == SavingStatus.Saved)
                    {
                        MessageBox.Show($"{Path.GetFileName(filePath)} успешно сохранён", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return filePath;
                    }
                    else
                    {
                        ShowSaveError(status, filePath);
                        return filePath;
                    }
                }
                return null;
            }
        }

        private void ShowSaveError(SavingStatus status, string filePath)
        {
            string errorMessage;

            switch (status)
            {
                case SavingStatus.PermissionError:
                    errorMessage = $"Ошибка доступа: Нет прав для записи в файл {filePath}";
                    break;
                case SavingStatus.FileNotFoundError:
                    errorMessage = $"Ошибка: Директория не найдена для файла {filePath}";
                    break;
                case SavingStatus.FileExistsError:
                    errorMessage = $"Ошибка: Файл {filePath} уже существует и не может быть перезаписан";
                    break;
                //case SavingStatus.OSError:
                //    errorMessage = $"Системная ошибка при сохранении файла {filePath}";
                //    break;
                case SavingStatus.DiskFullError:
                    errorMessage = $"Ошибка: На диске недостаточно места для сохранения {filePath}";
                    break;
                //case SavingStatus.InvalidFileNameError:
                //    errorMessage = $"Ошибка: Некорректное имя файла {filePath}";
                //    break;
                case SavingStatus.UnsupportedFormatError:
                    errorMessage = $"Ошибка: Формат файла {filePath} не поддерживается";
                    break;
                case SavingStatus.BufferOverflowError:
                    errorMessage = $"Ошибка: Недостаточно памяти для сохранения {filePath}";
                    break;
                case SavingStatus.PathTooLongError:
                    errorMessage = $"Ошибка: Слишком длинный путь к файлу {filePath}";
                    break;
                default:
                    errorMessage = $"Неизвестная ошибка при сохранении файла {filePath}";
                    break;
            }

            MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public string GetStatusMessage(SavingStatus status, string filePath = "")
        {
            switch (status)
            {
                case SavingStatus.Saved:
                    return "Файл успешно сохранён";
                case SavingStatus.PermissionError:
                    return $"Ошибка: Нет прав для записи в файл {filePath}";
                case SavingStatus.FileNotFoundError:
                    return $"Ошибка: Директория не найдена ({filePath})";
                case SavingStatus.FileExistsError:
                    return $"Ошибка: Файл уже существует ({filePath})";
                case SavingStatus.OSError:
                    return $"Системная ошибка при сохранении ({filePath})";
                case SavingStatus.DiskFullError:
                    return $"Ошибка: Недостаточно места на диске ({filePath})";
                case SavingStatus.InvalidFileNameError:
                    return $"Ошибка: Недопустимое имя файла ({filePath})";
                case SavingStatus.UnsupportedFormatError:
                    return $"Ошибка: Формат не поддерживается ({filePath})";
                case SavingStatus.BufferOverflowError:
                    return $"Ошибка: Недостаточно памяти ({filePath})";
                case SavingStatus.PathTooLongError:
                    return $"Ошибка: Слишком длинный путь ({filePath})";
                default:
                    return $"Неизвестная ошибка ({filePath})";
            }
        }
    }
}
