using System.ComponentModel;
using ModelContextProtocol.Server;

[McpServerPromptType]
public class InstapaperPrompts
{
    [McpServerPrompt(Name = "organize_reading_list")]
    [Description("Organize unread bookmarks.")]
    public static string OrganizeReadingList() => "Analyze and organize unread bookmarks into folders.";

    [McpServerPrompt(Name = "weekly_digest")]
    [Description("Generate a weekly digest.")]
    public static string GetWeeklyDigest() => "Generate summary of unread bookmarks from last 7 days.";

    [McpServerPrompt(Name = "recommend_next")]
    [Description("Suggest bookmark.")]
    public static string RecommendNextBookmark() => "Suggest what to read next based on context.";

    [McpServerPrompt(Name = "research_mode")]
    [Description("Analyze bookmarks on a specific topic.")]
    public static string Research(
        [Description("The topic to research in your bookmarks.")] int topic) => $"Analyze bookmarks on a specific topic. The topic is {topic}.";

    [McpServerPrompt(Name = "clean_up_suggestions")]
    [Description("Identify old bookmarks to archive.")]
    public static string Cleanup() => $"Identify old or irrelevant bookmarks for archiving.";
}