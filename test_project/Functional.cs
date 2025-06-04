using System.Text;
using static MarkdownEditor;
using Common.Enums;

namespace test_project
{
    class Functional
    {
        MarkdownEditor logic;
        FormatAnalyzer analyzer;
        public Functional()
        {
            logic = new MarkdownEditor();
            analyzer = new FormatAnalyzer();
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

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return null;
                }

                string filePath = openFileDialog.FileName;

                logic.OpenFile(filePath);

                var formattedContent = logic.ReturnContent();

                var formattingInfo = logic.ReturnFormating();

                richTextBox.Clear();
                richTextBox.Text = GetPlainText(formattedContent);

                ApplyFormatting(richTextBox, formattingInfo);

                return filePath;
            }
        }

        private string GetPlainText(List<FormattedLine> content)
        {
            var sb = new StringBuilder();
            foreach (var line in content)
            {
                foreach (var run in line.Runs)
                {
                    sb.Append(run.Text);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private void ApplyFormatting(RichTextBox richTextBox, Dictionary<FormattingStyle, List<int[]>> formattingInfo)
        {
            foreach (var styleEntry in formattingInfo)
            {
                foreach (var format in styleEntry.Value)
                {
                    int lineNumber = format[0];
                    int start = format[1];
                    int length = format[2];
                    int globalPos = format[3];

                    richTextBox.Select(globalPos, length);

                    FontStyle newStyle = richTextBox.SelectionFont.Style;

                    switch (styleEntry.Key)
                    {
                        case FormattingStyle.Bold:
                            newStyle |= FontStyle.Bold;
                            break;
                        case FormattingStyle.Italic:
                            newStyle |= FontStyle.Italic;
                            break;
                        case FormattingStyle.Underline:
                            newStyle |= FontStyle.Underline;
                            break;
                        case FormattingStyle.Strikethrough:
                            newStyle |= FontStyle.Strikeout;
                            break;
                    }

                    richTextBox.SelectionFont = new Font(richTextBox.Font, newStyle);
                }
            }
        }

        public void SaveFileForInterface(RichTextBox richTextBox, string filePath)
        {
            try
            {
                var formattedContent = analyzer.GetFormattedLines(richTextBox);

                logic.SaveContent(formattedContent);
                SavingStatus status = logic.SaveFile(filePath);

                if (status == SavingStatus.Saved)
                {
                    MessageBox.Show("Файл успешно сохранён", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                else ShowSaveError(status, filePath);
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string? SaveAsFileForInterface(object sender, EventArgs e, RichTextBox richTextBox)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Сохранить файл как";
                saveFileDialog.Filter = "Markdown файлы (*.md)|*.md|Текстовые файлы (*.txt)|*.txt";
                saveFileDialog.FileName = "markdownText.md";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var formattedContent = analyzer.GetFormattedLines(richTextBox);

                        logic.SaveContent(formattedContent);
                        SavingStatus status = logic.SaveFile(saveFileDialog.FileName);

                        if (status == SavingStatus.Saved)
                        {
                            MessageBox.Show("Файл успешно сохранён", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return saveFileDialog.FileName;
                        }
                        else ShowSaveError(status, saveFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            return null;
        }

        public void ShowSaveError(SavingStatus status, string filePath)
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
                case SavingStatus.OSError:
                    errorMessage = $"Системная ошибка при сохранении файла {filePath}";
                    break;
                case SavingStatus.DiskFullError:
                    errorMessage = $"Ошибка: На диске недостаточно места для сохранения {filePath}";
                    break;
                case SavingStatus.InvalidFileNameError:
                    errorMessage = $"Ошибка: Некорректное имя файла {filePath}";
                    break;
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
