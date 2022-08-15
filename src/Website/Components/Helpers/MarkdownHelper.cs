using Markdig;

namespace Website.Components.Helpers
{
    public class MarkdownHelper
    {
        public static string ParseToHtml(string markdown, bool disableHtml = true)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            MarkdownPipelineBuilder builder = new MarkdownPipelineBuilder()
                    .UseEmojiAndSmiley()
                    .UseSoftlineBreakAsHardlineBreak()
                    .UseAdvancedExtensions()
                    .UseAutoLinks();

            if (disableHtml)
                builder.DisableHtml();

            return Markdown.ToHtml(markdown, builder.Build());
        }
    }
}
