using System.ComponentModel;
using Instapaper.Mcp.Server;
using ModelContextProtocol.Server;

[McpServerToolType]
public sealed class InstapaperFolderTools
{
    private readonly IInstapaperClient _instapaperClient;

    public InstapaperFolderTools(IInstapaperClient instapaperClient)
    {
        _instapaperClient = instapaperClient;
    }
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

    [McpServerTool(Name = "delete_folder")]
    [Description("Delete folder.")]
    public async Task<bool> DeleteFolderAsync(long folderId, CancellationToken cancellationToken) =>
      await _instapaperClient.DeleteFolderAsync(folderId, cancellationToken);

    [McpServerTool(Name = "list_folders")]
    [Description("List folders.")]
    public async Task<IReadOnlyCollection<Folder>> ListFolderAsync(CancellationToken cancellationToken) =>
      await _instapaperClient.ListFoldersAsync(cancellationToken);
}