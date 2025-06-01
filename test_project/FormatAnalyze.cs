using System.Collections.Generic;

namespace test_project
{
    public class NormalTextFormatAnalyzer
    {
        public enum FormattingStyle
        {
            Bold,
            Italic,
            Underline,
            Strikethrough
        }

        public static List<int[]> boldList = [];
        public static List<int[]> italicList = [];
        public static List<int[]> underlineList = [];
        public static List<int[]> strikeoutList = [];

        Dictionary<FormattingStyle, List<int[]>> _formatting = new()
        {
            { FormattingStyle.Bold, boldList },
            { FormattingStyle.Italic, italicList },
            { FormattingStyle.Underline, underlineList },
            { FormattingStyle.Strikethrough, strikeoutList }
        };


        public static RichTextBox richTextBox;
        public static FontStyle lastFont;
        public Dictionary<test_project.NormalTextFormatAnalyzer.FormattingStyle, List<int[]>> SaveFormatting(RichTextBox richTextBox)
        {
            boldList.Clear();
            italicList.Clear();
            underlineList.Clear();
            strikeoutList.Clear();
            int lineNumber = 0;
            int lineStartIndex = 0;

            string[] lines = richTextBox.Text.Split(new[] { '\n', '\v' }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line))
                {
                    lineStartIndex += line.Length + 1;
                    continue;
                }

                int lineLength = line.Length;
                int currentPosition = lineStartIndex;
                Font currentFont = GetCharFormat(richTextBox, currentPosition);
                FontStyle currentStyle = currentFont?.Style ?? FontStyle.Regular;
                int formatStart = 0;

                for (int j = 1; j <= lineLength; j++)
                {
                    Font charFont = GetCharFormat(richTextBox, currentPosition + j);
                    FontStyle charStyle = charFont?.Style ?? FontStyle.Regular;

                    if (charStyle != currentStyle || j == lineLength)
                    {
                        int segmentLength = j - formatStart;
                        if (segmentLength > 0)
                        {
                            SaveStyleInfo(currentStyle, i, formatStart, segmentLength, boldList, italicList, underlineList, strikeoutList);
                        }

                        currentStyle = charStyle;
                        formatStart = j;
                    }
                }
                lineStartIndex += line.Length + 1;

            }
            lastFont = FontStyle.Regular;
            return _formatting;
        }



        public static Font GetCharFormat(RichTextBox richTextBox, int position)
        {
            richTextBox.Select(position, 1);
            return richTextBox.SelectionFont;
        }
        private static void SaveStyleInfo(FontStyle style, int lineNumber, int start, int length,
                                List<int[]> boldList, List<int[]> italicList,
                                List<int[]> underlineList, List<int[]> strikeoutList)
        {
            if (length <= 0)
                return;

            int[] info = { lineNumber, start, length };

            if ((style & FontStyle.Bold) == FontStyle.Bold)
                boldList.Add(info);

            if ((style & FontStyle.Italic) == FontStyle.Italic)
                italicList.Add(info);

            if ((style & FontStyle.Underline) == FontStyle.Underline)
                underlineList.Add(info);

            if ((style & FontStyle.Strikeout) == FontStyle.Strikeout)
                strikeoutList.Add(info);
        }
    }

    public class MarkDownTextFormatAnalyzer
    {
        public enum FormattingStyle
        {
            Bold,
            Italic,
            Underline,
            Strikethrough
        }

        public static List<int[]> boldList = [];
        public static List<int[]> italicList = [];
        public static List<int[]> underlineList = [];
        public static List<int[]> strikeoutList = [];

        Dictionary<FormattingStyle, List<int[]>> _formatting = new()
        {
            { FormattingStyle.Bold, boldList },
            { FormattingStyle.Italic, italicList },
            { FormattingStyle.Underline, underlineList },
            { FormattingStyle.Strikethrough, strikeoutList }
        };

        public Dictionary<FormattingStyle, List<int[]>> SaveFormatting(string filePath)
        {
            boldList.Clear();
            italicList.Clear();
            underlineList.Clear();
            strikeoutList.Clear();
            string[] lines = File.ReadAllLines(filePath);

            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                string line = lines[lineNumber];
                if (string.IsNullOrEmpty(line)) continue;

                AnalyzeMarkdownFormat(line, lineNumber, "**", "**", boldList);

                AnalyzeMarkdownFormat(line, lineNumber, "*", "*", italicList);

                AnalyzeMarkdownFormat(line, lineNumber, "~~", "~~", strikeoutList);

                AnalyzeMarkdownFormat(line, lineNumber, "__", "__", underlineList);
            }
            return _formatting;
        }

        private void AnalyzeMarkdownFormat(string line, int lineNumber, string openTag, string closeTag, List<int[]> resultList)
        {
            int lastIndex = 0;
            while (true)
            {
                int startTagPos = line.IndexOf(openTag, lastIndex);
                if (startTagPos == -1) break;

                int contentStart = startTagPos + openTag.Length;
                int endTagPos = line.IndexOf(closeTag, contentStart);
                if (endTagPos == -1) break;

                int contentLength = endTagPos - contentStart;
                if (contentLength > 0)
                {
                    int[] info = { lineNumber, startTagPos, contentLength };
                    resultList.Add(info);
                }

                lastIndex = endTagPos + closeTag.Length;
            }
        }
    }
}