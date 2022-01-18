using Markdig;

namespace Website.Components.Helpers
{
    public class MarkdownHelper
    {
        public static string ParseToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
                    .UseEmojiAndSmiley()
                    .UseAdvancedExtensions()
                    .Build();

            return Markdown.ToHtml(markdown, pipeline);
        }
    }
}
