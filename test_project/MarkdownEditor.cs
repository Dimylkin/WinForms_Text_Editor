using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Common.Enums;

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

    public class TextRun
    {
        public string Text { get; set; }
        public List<FormattingStyle> Styles { get; set; } = new();
    }

    public class FormattedLine
    {
        public List<TextRun> Runs { get; set; } = new();
    }

    private List<FormattedLine> _content = new();

    private readonly Dictionary<Common.Enums.FormattingStyle, Regex> _patterns = new()
    {
        { FormattingStyle.Bold, new Regex(@"\*\*(.+?)\*\*", RegexOptions.Compiled) },
        { FormattingStyle.Italic, new Regex(@"(?<!\*)\*(?!\*)(.+?)\*(?!\*)", RegexOptions.Compiled) },
        { FormattingStyle.Underline, new Regex(@"__(.+?)__", RegexOptions.Compiled) },
        { FormattingStyle.Strikethrough, new Regex(@"~~(.+?)~~", RegexOptions.Compiled) },
        { FormattingStyle.Heading1, new Regex(@"#(.+?)#", RegexOptions.Compiled) },
        { FormattingStyle.Heading2, new Regex(@"##(.+?)##", RegexOptions.Compiled) },
        { FormattingStyle.Heading3, new Regex(@"###(.+?)###", RegexOptions.Compiled) }
    };

    public void CreateFile(string filePath)
    {
        using (FileStream fs = File.Create(filePath)) { }
    }

    public void OpenFile(string path)
    {
        _content.Clear();

        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            _content.Add(ParseLine(line));
        }
    }

    private FormattedLine ParseLine(string line)
    {
        var result = new FormattedLine();
        string combinedPattern = @"(\*\*.+?\*\*|(?<!\*)\*(?!\*).+?(?<!\*)\*(?!\*)|__.+?__|~~.+?~~|#.+?#|##.+?##|###.+?###)";
        var regex = new Regex(combinedPattern);

        int pos = 0;
        foreach (Match match in regex.Matches(line))
        {
            if (match.Index > pos)
            {
                result.Runs.Add(new TextRun { Text = line[pos..match.Index] });
            }

            string token = match.Value;
            string inner = token;
            var styles = new List<FormattingStyle>();

            foreach (var kvp in _patterns)
            {
                var m = kvp.Value.Match(token);
                if (m.Success && m.Groups.Count > 1)
                {
                    inner = m.Groups[1].Value;
                    styles.Add(kvp.Key);
                    break;
                }
            }

            result.Runs.Add(new TextRun { Text = inner, Styles = styles });
            pos = match.Index + match.Length;
        }

        if (pos < line.Length)
        {
            result.Runs.Add(new TextRun { Text = line[pos..] });
        }

        return result;
    }

    public SavingStatus SaveFile(string path)
    {
        try
        {
            var lines = new List<string>();

            foreach (var line in _content)
            {
                var result = "";
                foreach (var run in line.Runs)
                {
                    var text = run.Text;
                    foreach (var style in run.Styles)
                    {
                        var tag = GetTag(style);
                        text = tag + text + tag;
                    }
                    result += text;
                }
                lines.Add(result);
            }

            File.WriteAllLines(path, lines);
            savingStatus = true;
            return SavingStatus.Saved;
        }
        catch (PathTooLongException) { return SavingStatus.PathTooLongError; }
        catch (UnauthorizedAccessException) { return SavingStatus.PermissionError; }
        catch (DirectoryNotFoundException) { return SavingStatus.FileNotFoundError; }
        catch (IOException ex)
        {
            if (File.Exists(path) && ex.Message.Contains("already exists"))
                return SavingStatus.FileExistsError;
            if (ex.Message.Contains("There is not enough space"))
                return SavingStatus.DiskFullError;
            return SavingStatus.OSError;
        }
        catch (NotSupportedException) { return SavingStatus.UnsupportedFormatError; }
        catch (OutOfMemoryException) { return SavingStatus.BufferOverflowError; }
    }

    public List<FormattedLine> ReturnContent() => _content;

    public Dictionary<FormattingStyle, List<int[]>> ReturnFormating()
    {
        var formattingInfo = new Dictionary<FormattingStyle, List<int[]>>();

        // Для каждого типа форматирования создаем список позиций
        formattingInfo.Add(FormattingStyle.Bold, new List<int[]>());
        formattingInfo.Add(FormattingStyle.Italic, new List<int[]>());
        formattingInfo.Add(FormattingStyle.Underline, new List<int[]>());
        formattingInfo.Add(FormattingStyle.Strikethrough, new List<int[]>());
        formattingInfo.Add(FormattingStyle.Heading1, new List<int[]>());
        formattingInfo.Add(FormattingStyle.Heading2, new List<int[]>());
        formattingInfo.Add(FormattingStyle.Heading3, new List<int[]>());

        int lineNumber = 0;
        int globalPosition = 0;

        foreach (var line in _content)
        {
            int linePosition = 0;

            foreach (var run in line.Runs)
            {
                if (run.Styles.Count > 0)
                {
                    foreach (var style in run.Styles)
                    {
                        // Добавляем информацию о позиции форматирования
                        // [номер строки, начальная позиция, длина, глобальная позиция]
                        formattingInfo[style].Add(new int[] {
                        lineNumber,
                        linePosition,
                        run.Text.Length,
                        globalPosition
                    });
                    }
                }

                linePosition += run.Text.Length;
                globalPosition += run.Text.Length;
            }

            // Учитываем символ перевода строки
            globalPosition += Environment.NewLine.Length;
            lineNumber++;
        }

        return formattingInfo;
    }

    public void SaveContent(List<FormattedLine> content)
    {
        _content = content;
        savingStatus = false;
    }

    public bool ReturnSavingStatus() => savingStatus;

    public string ReturnExtension(string filePath) => Path.GetExtension(filePath);

    private string GetTag(FormattingStyle style) => style switch
    {
        FormattingStyle.Bold => "**",
        FormattingStyle.Italic => "*",
        FormattingStyle.Underline => "__",
        FormattingStyle.Strikethrough => "~~",
        FormattingStyle.Heading1 => "#",
        FormattingStyle.Heading2 => "##",
        FormattingStyle.Heading3=> "###",
        _ => ""
    };
}
