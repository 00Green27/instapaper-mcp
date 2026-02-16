using System.ComponentModel;
using Instapaper.Mcp.Server;
using ModelContextProtocol.Server;

/// <summary>
/// MCP tools for managing folders in Instapaper.
/// </summary>
[McpServerToolType]
public sealed class InstapaperFolderTools
{
    private readonly IInstapaperClient _instapaperClient;

    /// <summary>
    /// Initializes a new instance of the InstapaperFolderTools class.
    /// </summary>
    /// <param name="instapaperClient">The Instapaper API client.</param>
    public InstapaperFolderTools(IInstapaperClient instapaperClient)
    {
        _instapaperClient = instapaperClient;
    }

    /// <summary>
    /// Creates an organizational folder. Returns existing folder if it already exists.
    /// </summary>
    [McpServerTool(Name = "create_folder")]
    [Description("Creates an organizational folder.")]
    public async Task<Folder> CreateFolderAsync(
        [Description("The title of the folder.")]
        string title,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _instapaperClient.CreateFolderAsync(title, cancellationToken);
        }
        catch (InstapaperApiException ex) when (ex.ErrorCode == InstapaperErrorCode.FolderAlreadyExists)
        {
            var folder = await _instapaperClient.SearchFolderAsync(title, cancellationToken);

            if (folder is null)
            {
                throw new InvalidOperationException("Something went wrong.");
            }
            return folder;
        }
    }

    /// <summary>
    /// Deletes a folder.
    /// </summary>
    [McpServerTool(Name = "delete_folder")]
    [Description("Deletes a folder.")]
    public async Task<bool> DeleteFolderAsync(long folderId, CancellationToken cancellationToken) =>
      await _instapaperClient.DeleteFolderAsync(folderId, cancellationToken);

    /// <summary>
    /// Lists all folders.
    /// </summary>
    [McpServerTool(Name = "list_folders")]
    [Description("Lists all folders.")]
    public async Task<IReadOnlyCollection<Folder>> ListFolderAsync(CancellationToken cancellationToken) =>
      await _instapaperClient.ListFoldersAsync(cancellationToken);

    /// <summary>
    /// Reorders folders by specifying new positions.
    /// </summary>
    [McpServerTool(Name = "reorder_folders")]
    [Description("Re-orders a user's folders.")]
    public async Task<IReadOnlyCollection<Folder>> ReorderFoldersAsync(
        [Description("List of the folder ID and position.")]
        (long FolderId, int Position)[] folderOrders,
        CancellationToken cancellationToken) =>
      await _instapaperClient.ReorderFoldersAsync(folderOrders, cancellationToken);
}