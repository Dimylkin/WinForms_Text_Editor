using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public enum FormattingStyle
{
    Bold,
    Italic,
    Underline,
    Strikethrough,
    Heading1,
    Heading2,
    Heading3
}

public enum SavingStatus
{
    Saved = 0,
    PermissionError = 1,
    FileNotFoundError = 2,
    FileExistsError = 3,
    OSError = 4,
    DiskFullError = 5,
    InvalidFileNameError = 6,
    UnsupportedFormatError = 7,
    BufferOverflowError = 8,
    PathTooLongError = 9
}

public class MarkdownEditor
{
    public bool savingStatus = true;
    public List<string> _content = new();
    private Dictionary<FormattingStyle, List<int[]>> _formatting = new();

    public MarkdownEditor()
    {
        foreach (FormattingStyle style in Enum.GetValues(typeof(FormattingStyle)))
            _formatting[style] = new List<int[]>();
    }

    public void CreateFile(string filePath)
    {
        using (FileStream fs = File.Create(filePath)) { }
    }

    // Метод открытия файла, считывания, сохранения разметки и очистки
    public void OpenFile(string path)
    {
        _content.Clear();
        foreach (var style in _formatting.Keys)
            _formatting[style].Clear();

        var patterns = new Dictionary<FormattingStyle, string>
        {
            { FormattingStyle.Bold, @"\*\*(.+?)\*\*" },
            { FormattingStyle.Italic, @"(?<!\*)\*(?!\*)(.+?)\*(?!\*)" },
            { FormattingStyle.Underline, @"__(.+?)__" },
            { FormattingStyle.Strikethrough, @"~~(.+?)~~" },
            { FormattingStyle.Heading1, @"#(.+?)#" },
            { FormattingStyle.Heading2, @"##(.+?)##" },
            { FormattingStyle.Heading3, @"###(.+?)###" }
        };

        var lines = File.ReadAllLines(path);
        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            string currentLine = lines[lineIndex];
            string workingLine = currentLine;

            foreach (var kvp in patterns)
            {
                var style = kvp.Key;
                var regex = new Regex(kvp.Value);

                Match match;
                while ((match = regex.Match(workingLine)).Success)
                {
                    int tagLen = GetTagLength(style);
                    int cleanStart = match.Index + tagLen;
                    int cleanLength = match.Groups[1].Length;

                    _formatting[style].Add(new int[] { lineIndex, cleanStart, cleanLength });

                    // Удаляем теги
                    string unwrapped = match.Groups[1].Value;
                    workingLine = workingLine.Remove(match.Index, match.Length)
                                             .Insert(match.Index, unwrapped);

                }
            }

            _content.Add(workingLine);
        }
    }

    // Метод сохранения с повторным применением форматирования
    public SavingStatus SaveFile(string path)
    {
    try
    {
        var lines = new List<string>(_content);
        var formatting = _formatting
            .SelectMany(kvp => kvp.Value.Select(v => new { Style = kvp.Key, Data = v }))
            .OrderByDescending(x => x.Data[0])  // Строки с конца
            .ThenByDescending(x => x.Data[1])    // Позиции с конца
            .ToList();

        foreach (var item in formatting)
        {
            int line = item.Data[0];
            int start = item.Data[1];
            int length = item.Data[2];
            string tag = GetTag(item.Style);

            if (line >= 0 && line < lines.Count && 
                start >= 0 && start + length <= lines[line].Length)
            {
                lines[line] = lines[line].Insert(start + length, tag)
                                         .Insert(start, tag);
            }
        }


            File.WriteAllLines(path, lines);
            savingStatus = true;
            return SavingStatus.Saved;
        }
        catch (PathTooLongException)
        {
            return SavingStatus.PathTooLongError;
        }
        catch (UnauthorizedAccessException)
        {
            return SavingStatus.PermissionError;
        }
        catch (DirectoryNotFoundException)
        {
            return SavingStatus.FileNotFoundError;
        }
        catch (IOException ex)
        {
            // Проверка: файл уже существует (в случае использования StreamWriter без overwrite — здесь не применимо)
            if (File.Exists(path) && ex.Message.Contains("already exists"))
            {
                return SavingStatus.FileExistsError;
            }

            // Проверка: диск переполнен (нет точного способа отличить, но можно ориентироваться по сообщению)
            if (ex.Message.Contains("There is not enough space"))
            {
                return SavingStatus.DiskFullError;
            }

            return SavingStatus.OSError;
        }
        //catch (ArgumentException)
        //{
        //    return SavingStatus.InvalidFileNameError;
        //}
        catch (NotSupportedException)
        {
            return SavingStatus.UnsupportedFormatError;
        }
        catch (OutOfMemoryException)
        {
            return SavingStatus.BufferOverflowError;
        }
        //catch (Exception)
        //{
        //    return SavingStatus.OSError;
        //}
    }

    public string[] ReturnContent()
    {
        return _content.ToArray();
    }

    public Dictionary<FormattingStyle, List<int[]>> ReturnFormating()
    {
        return _formatting;
    }

    public void SaveContent(string[] content)
    {
        _content = content.ToList<string>();
        savingStatus = false;
    }

    public void SaveFormating(Dictionary<FormattingStyle, List<int[]>> dictionary)
    {
        _formatting = dictionary;
        savingStatus = false;
    }

    public bool ReturnSavingStatus()
    {
        return savingStatus;
    }

    public string ReturnExtension(string filePath)
    {
        return Path.GetExtension(filePath);
    }

    private int GetTagLength(FormattingStyle style) => style switch
    {
        FormattingStyle.Bold => 2,
        FormattingStyle.Italic => 1,
        FormattingStyle.Underline => 2,
        FormattingStyle.Strikethrough => 2,
        FormattingStyle.Heading1 => 1,
        FormattingStyle.Heading2 => 2,
        FormattingStyle.Heading3 => 3,
        _ => 0
    };

    private string GetTag(FormattingStyle style) => style switch
    {
        FormattingStyle.Bold => "**",
        FormattingStyle.Italic => "*",
        FormattingStyle.Underline => "__",
        FormattingStyle.Strikethrough => "~~",
        FormattingStyle.Heading1 => "#",
        FormattingStyle.Heading2 => "##",
        FormattingStyle.Heading3 => "###",
        _ => ""
    };
}
