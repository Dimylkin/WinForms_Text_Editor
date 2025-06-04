using Common.Enums;

namespace test_project
{
    public class FormatAnalyzer
    {
        MarkdownEditor logic;
        public FormatAnalyzer()
        {
            logic = new MarkdownEditor();
        }

        private const int DefaultFontSize = 15;
        private static readonly Dictionary<FormattingStyle, List<int[]>> _formattingCache = new();

        public List<MarkdownEditor.FormattedLine> GetFormattedLines(RichTextBox richTextBox)
        {
            var formatting = AnalyzeFormatting(richTextBox);
            string[] lines = richTextBox.Lines;
            var result = new List<MarkdownEditor.FormattedLine>();

            for (int lineNum = 0; lineNum < lines.Length; lineNum++)
            {
                result.Add(ProcessLine(lines[lineNum], lineNum, formatting));
            }

            return result;
        }

        private Dictionary<FormattingStyle, List<int[]>> AnalyzeFormatting(RichTextBox richTextBox)
        {
            ClearFormattingCache();
            string[] lines = richTextBox.Text.Split(new[] { '\n', '\v' }, StringSplitOptions.None);
            int lineStartIndex = 0;

            for (int lineNum = 0; lineNum < lines.Length; lineNum++)
            {
                string line = lines[lineNum];
                if (string.IsNullOrEmpty(line))
                {
                    lineStartIndex += line.Length + 1;
                    continue;
                }

                ProcessLineFormatting(richTextBox, line, lineNum, ref lineStartIndex);
            }

            return _formattingCache;
        }

        private void ProcessLineFormatting(RichTextBox richTextBox, string line, int lineNum, ref int lineStartIndex)
        {
            int lineLength = line.Length;
            int currentPosition = lineStartIndex;
            Font currentFont = GetCharFormat(richTextBox, currentPosition);
            FontStyle currentStyle = currentFont?.Style ?? FontStyle.Regular;
            int currentSize = (int)(currentFont?.Size ?? DefaultFontSize);
            int formatStart = 0;

            for (int j = 1; j <= lineLength; j++)
            {
                Font charFont = GetCharFormat(richTextBox, currentPosition + j);
                FontStyle charStyle = charFont?.Style ?? FontStyle.Regular;
                int charSize = (int)(charFont?.Size ?? DefaultFontSize);

                if (charStyle != currentStyle || charSize != currentSize || j == lineLength)
                {
                    int segmentLength = j - formatStart;
                    if (segmentLength > 0)
                    {
                        SaveStyleSegment(currentStyle, currentSize, lineNum, formatStart, segmentLength);
                    }

                    currentStyle = charStyle;
                    currentSize = charSize;
                    formatStart = j;
                }
            }

            lineStartIndex += line.Length + 1;
        }

        private MarkdownEditor.FormattedLine ProcessLine(string line, int lineNum, Dictionary<FormattingStyle, List<int[]>> formatting)
        {
            var formattedLine = new MarkdownEditor.FormattedLine();
            int pos = 0;

            while (pos < line.Length)
            {
                var styles = GetStylesAtPosition(formatting, lineNum, pos);
                int runLength = CalculateRunLength(line, formatting, lineNum, pos, styles);

                formattedLine.Runs.Add(new MarkdownEditor.TextRun
                {
                    Text = line.Substring(pos, runLength),
                    Styles = styles.Select(ConvertToMarkdownStyle).ToList()
                });

                pos += runLength;
            }

            return formattedLine;
        }

        private List<FormattingStyle> GetStylesAtPosition(Dictionary<FormattingStyle, List<int[]>> formatting, int lineNum, int pos)
        {
            var styles = new List<FormattingStyle>();
        
            foreach (var kvp in formatting)
            {
                if (kvp.Value.Any(range => 
                    range[0] == lineNum && 
                    pos >= range[1] && 
                    pos < range[1] + range[2]))
                {
                    styles.Add(kvp.Key);
                }
            }
        
            return styles;
        }

        private int CalculateRunLength(string line, Dictionary<FormattingStyle, List<int[]>> formatting, 
                                     int lineNum, int startPos, List<FormattingStyle> initialStyles)
        {
            int length = 1;
        
            while (startPos + length < line.Length)
            {
                var currentStyles = GetStylesAtPosition(formatting, lineNum, startPos + length);
                if (!currentStyles.SequenceEqual(initialStyles))
                    break;
                
                length++;
            }
        
            return length;
        }

        private Common.Enums.FormattingStyle ConvertToMarkdownStyle(FormattingStyle style)
        {
            return style;
        }

        private void SaveStyleSegment(FontStyle style, int fontSize, int lineNumber, int start, int length)
        {
            if (length <= 0) return;

            int[] info = { lineNumber, start, length };

            if ((style & FontStyle.Bold) == FontStyle.Bold)
            {
                if (fontSize == 18)
                    _formattingCache[FormattingStyle.Heading1].Add(info);
                else if (fontSize == 21)
                    _formattingCache[FormattingStyle.Heading2].Add(info);
                else if (fontSize == 24)
                    _formattingCache[FormattingStyle.Heading3].Add(info);
                else
                    _formattingCache[FormattingStyle.Bold].Add(info);
            }

            if ((style & FontStyle.Italic) == FontStyle.Italic)
                _formattingCache[FormattingStyle.Italic].Add(info);

            if ((style & FontStyle.Underline) == FontStyle.Underline)
                _formattingCache[FormattingStyle.Underline].Add(info);

            if ((style & FontStyle.Strikeout) == FontStyle.Strikeout)
                _formattingCache[FormattingStyle.Strikethrough].Add(info);
        }

        private void ClearFormattingCache()
        {
            foreach (var style in Enum.GetValues(typeof(FormattingStyle)).Cast<FormattingStyle>())
            {
                if (!_formattingCache.ContainsKey(style))
                    _formattingCache[style] = new List<int[]>();
                else
                    _formattingCache[style].Clear();
            }
        }

        public static Font GetCharFormat(RichTextBox richTextBox, int position)
        {
            if (position >= richTextBox.Text.Length)
            {
                return new Font(richTextBox.Font, FontStyle.Regular);
            }
            richTextBox.Select(position, 1);
            return richTextBox.SelectionFont;
        }
    }
}