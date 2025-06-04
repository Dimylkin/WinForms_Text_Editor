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
            Strikethrough,
            Heading1,
            Heading2,
            Heading3
        }

        public static List<int[]> boldList = [];
        public static List<int[]> italicList = [];
        public static List<int[]> underlineList = [];
        public static List<int[]> strikeoutList = [];
        public static List<int[]> heading1List = [];
        public static List<int[]> heading2List = [];
        public static List<int[]> heading3List = [];

        Dictionary<FormattingStyle, List<int[]>> _formatting = new()
        {
            { FormattingStyle.Bold, boldList },
            { FormattingStyle.Italic, italicList },
            { FormattingStyle.Underline, underlineList },
            { FormattingStyle.Strikethrough, strikeoutList },
            { FormattingStyle.Heading1, heading1List },
            { FormattingStyle.Heading2, heading2List },
            { FormattingStyle.Heading3, heading3List }
        };


        public static RichTextBox richTextBox;
        public static FontStyle lastFont;
        public Dictionary<test_project.NormalTextFormatAnalyzer.FormattingStyle, List<int[]>> SaveFormatting(RichTextBox richTextBox)
        {
            boldList.Clear();
            italicList.Clear();
            underlineList.Clear();
            strikeoutList.Clear();
            heading1List.Clear();
            heading2List.Clear();
            heading3List.Clear();

            int lineNumber = 0;
            int lineStartIndex = 0;
            const int defaultFontSize = 15;

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
                int currentSize = (int)(currentFont?.Size ?? defaultFontSize);
                int formatStart = 0;

                for (int j = 1; j <= lineLength; j++)
                {
                    Font charFont = GetCharFormat(richTextBox, currentPosition + j);
                    FontStyle charStyle = charFont?.Style ?? FontStyle.Regular;
                    int charSize = (int)(charFont?.Size ?? defaultFontSize);

                    if (charStyle != currentStyle || charSize != currentSize || j == lineLength)
                    {
                        int segmentLength = j - formatStart;
                        if (segmentLength > 0)
                        {
                            SaveStyleInfo(currentStyle, currentSize, i, formatStart, segmentLength, boldList, italicList, underlineList, strikeoutList, heading1List, heading2List, heading3List);

                        }

                        currentStyle = charStyle;
                        currentSize = charSize;
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
            if (position >= richTextBox.Text.Length)
            {
                return new Font(richTextBox.Font, FontStyle.Regular);
            }
            richTextBox.Select(position, 1);
            return richTextBox.SelectionFont;
        }
        private static void SaveStyleInfo(FontStyle style, int fontSize, int lineNumber, int start, int length,
                                List<int[]> boldList, List<int[]> italicList,
                                List<int[]> underlineList, List<int[]> strikeoutList,
                                List<int[]> heading1List, List<int[]> heading2List, List<int[]> heading3List)
        {
            if (length <= 0)
                return;

            int[] info = { lineNumber, start, length };

            if ((style & FontStyle.Bold) == FontStyle.Bold)
            {
                if (fontSize == 18)
                    heading1List.Add(info);
                else if (fontSize == 21)
                    heading2List.Add(info);
                else if (fontSize == 24)
                    heading3List.Add(info);
                else
                    boldList.Add(info);
            }

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
            Strikethrough,
            Heading1,
            Heading2,
            Heading3
        }

        public static List<int[]> boldList = [];
        public static List<int[]> italicList = [];
        public static List<int[]> underlineList = [];
        public static List<int[]> strikeoutList = [];
        public static List<int[]> heading1List = [];
        public static List<int[]> heading2List = [];
        public static List<int[]> heading3List = [];

        Dictionary<FormattingStyle, List<int[]>> _formatting = new()
        {
            { FormattingStyle.Bold, boldList },
            { FormattingStyle.Italic, italicList },
            { FormattingStyle.Underline, underlineList },
            { FormattingStyle.Strikethrough, strikeoutList },
            { FormattingStyle.Heading1, heading1List },
            { FormattingStyle.Heading2, heading2List },
            { FormattingStyle.Heading3, heading3List }
        };

        public Dictionary<FormattingStyle, List<int[]>> SaveFormatting(string filePath)
        {
            boldList.Clear();
            italicList.Clear();
            underlineList.Clear();
            strikeoutList.Clear();
            heading1List.Clear();
            heading2List.Clear();
            heading3List.Clear();
            string[] lines = File.ReadAllLines(filePath);

            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                string line = lines[lineNumber];
                if (string.IsNullOrEmpty(line)) continue;

                AnalyzeMarkdownFormat(line, lineNumber, "**", "**", boldList);

                AnalyzeMarkdownFormat(line, lineNumber, "*", "*", italicList);

                AnalyzeMarkdownFormat(line, lineNumber, "~~", "~~", strikeoutList);

                AnalyzeMarkdownFormat(line, lineNumber, "__", "__", underlineList);

                AnalyzeMarkdownFormat(line, lineNumber, "#", "#", heading1List);

                AnalyzeMarkdownFormat(line, lineNumber, "##", "##", heading2List);

                AnalyzeMarkdownFormat(line, lineNumber, "###", "###", heading3List);
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