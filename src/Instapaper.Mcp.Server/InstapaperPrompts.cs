using System.ComponentModel;
using ModelContextProtocol.Server;

/// <summary>
/// MCP prompts for common AI agent tasks related to managing and organizing bookmarks.
/// </summary>
[McpServerPromptType]
public class InstapaperPrompts
{
    /// <summary>
    /// Prompt for organizing unread bookmarks into folders.
    /// </summary>
    [McpServerPrompt(Name = "organize_reading_list")]
    [Description("Organize unread bookmarks.")]
    public static string OrganizeReadingList() => "Analyze and organize unread bookmarks into folders.";

    /// <summary>
    /// Prompt for generating a weekly digest of bookmarks.
    /// </summary>
    [McpServerPrompt(Name = "weekly_digest")]
    [Description("Generate a weekly digest.")]
    public static string GetWeeklyDigest() => "Generate summary of unread bookmarks from last 7 days.";

    /// <summary>
    /// Prompt for recommending the next bookmark to read.
    /// </summary>
    [McpServerPrompt(Name = "recommend_next")]
    [Description("Suggest bookmark.")]
    public static string RecommendNextBookmark() => "Suggest what to read next based on context.";

    /// <summary>
    /// Prompt for analyzing bookmarks on a specific topic.
    /// </summary>
    [McpServerPrompt(Name = "research_mode")]
    [Description("Analyze bookmarks on a specific topic.")]
    public static string Research(
        [Description("The topic to research in your bookmarks.")]
        string topic) => $"Analyze bookmarks on a specific topic. The topic is {topic}.";

    /// <summary>
    /// Prompt for identifying old or irrelevant bookmarks for cleanup.
    /// </summary>
    [McpServerPrompt(Name = "clean_up_suggestions")]
    [Description("Identify old bookmarks to archive.")]
    public static string Cleanup() => $"Identify old or irrelevant bookmarks for archiving.";
}