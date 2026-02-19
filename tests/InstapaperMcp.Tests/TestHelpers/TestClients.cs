using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Instapaper.Mcp.Server.Tests.TestHelpers;

public static class TestClients
{
    public sealed class TestHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
        public TestHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_responder(request));
    }

    public sealed class FakeSignatureGenerator : IOAuth1SignatureGenerator
    {
        public string CreateAuthorizationHeader(HttpMethod method, Uri uri, string consumerKey, string consumerSecret, string? token, string? tokenSecret, Dictionary<string, string>? parameters)
        {
            return "OAuth oauth_consumer_key=\"ck\"";
        }
    }

    public sealed class SpyClient : IInstapaperClient
    {
        public string? LastQuery;
        public string? LastFolderId;
        public int? LastLimit;
        public bool ListFoldersCalled;
        public IReadOnlyCollection<Folder>? FoldersToReturn;

        public Task<IReadOnlyCollection<Bookmark>> ListBookmarksAsync(string? query, string? folderId, int? limit, CancellationToken ct = default)
        {
            LastQuery = query;
            LastFolderId = folderId;
            LastLimit = limit ?? 100; // Default if null

            var items = new List<Bookmark>
            {
                new Bookmark { BookmarkId = 1, Url = "http://a", Title = "A", FolderId = 0 }
            };

            return Task.FromResult((IReadOnlyCollection<Bookmark>)items);
        }

        public Task<IReadOnlyCollection<Folder>> ListFoldersAsync(CancellationToken ct = default)
        {
            ListFoldersCalled = true;
            if (FoldersToReturn is not null)
                return Task.FromResult(FoldersToReturn);

            var folders = new List<Folder>
            {
                new Folder { FolderId = 1, Title = "Inbox" }
            };

            return Task.FromResult((IReadOnlyCollection<Folder>)folders);
        }

        public Task<Bookmark> AddBookmarkAsync(string? url = null, string? title = null, string? description = null, long? folderId = null, string? content = null, bool resolveFinalUrl = true, bool archiveOnAdd = false, List<string>? tags = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(url) && string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("URL or content must be specified");
            return Task.FromResult(new Bookmark { BookmarkId = 1, Url = url ?? "http://test", Title = title ?? "Test" });
        }

        public Task<string> GetBookmarkContentAsync(long bookmarkId, CancellationToken ct = default)
        {
            if (bookmarkId <= 0)
                throw new ArgumentOutOfRangeException(nameof(bookmarkId));
            return Task.FromResult("Test content");
        }

        #region NotImplemented
        public Task<IReadOnlyDictionary<long, string>> GetBookmarkContentsAsync(IEnumerable<long> bookmarkIds, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Bookmark> ManageBookmarksAsync(long bookmarkId, BookmarkAction action, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<Bookmark>> ManageBookmarksAsync(IEnumerable<long> bookmarkIds, BookmarkAction action, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Bookmark> MoveBookmarkAsync(long bookmarkId, long folderId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<Bookmark>> MoveBookmarksAsync(IEnumerable<long> bookmarkIds, long folderId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Folder?> SearchFolderAsync(string title, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Folder> CreateFolderAsync(string title, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<bool> DeleteFolderAsync(long folderId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<Folder>> ReorderFoldersAsync((long FolderId, int Position)[] folderOrders, CancellationToken cancellationToken) => throw new NotImplementedException();
        #endregion
    }

    /// <summary>
    /// Fake client for testing tools. Returns mock data for all operations.
    /// </summary>
    public sealed class FakeClient : IInstapaperClient
    {
        public bool AddBookmarkCalled;
        public string? LastAddedBookmarkUrl;
        public bool MoveBookmarkCalled;
        public int MoveBookmarkCallCount;
        public bool ManageBookmarkCalled;
        public int ManageBookmarkCallCount;
        public BookmarkAction LastBookmarkAction;
        public bool GetBookmarkContentCalled;
        public string ContentToReturn = "Default content";

        public Task<IReadOnlyCollection<Bookmark>> ListBookmarksAsync(string? query, string? folderId, int? limit, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyCollection<Bookmark>>([new Bookmark { BookmarkId = 1, Url = "http://test", Title = "Test" }]);

        public Task<Bookmark> AddBookmarkAsync(string? url = null, string? title = null, string? description = null, long? folderId = null, string? content = null, bool resolveFinalUrl = true, bool archiveOnAdd = false, List<string>? tags = null, CancellationToken ct = default)
        {
            AddBookmarkCalled = true;
            LastAddedBookmarkUrl = url;
            return Task.FromResult(new Bookmark { BookmarkId = 1, Url = url ?? "http://test", Title = title ?? "Test" });
        }

        public Task<string> GetBookmarkContentAsync(long bookmarkId, CancellationToken ct = default)
        {
            GetBookmarkContentCalled = true;
            if (bookmarkId <= 0)
                throw new ArgumentOutOfRangeException(nameof(bookmarkId));
            return Task.FromResult(ContentToReturn);
        }

        public Task<IReadOnlyDictionary<long, string>> GetBookmarkContentsAsync(IEnumerable<long> bookmarkIds, CancellationToken ct = default)
        {
            var result = bookmarkIds.ToDictionary(id => id, id => ContentToReturn);
            return Task.FromResult<IReadOnlyDictionary<long, string>>(result);
        }

        public Task<Bookmark> ManageBookmarksAsync(long bookmarkId, BookmarkAction action, CancellationToken ct = default)
        {
            ManageBookmarkCalled = true;
            ManageBookmarkCallCount++;
            LastBookmarkAction = action;
            return Task.FromResult(new Bookmark { BookmarkId = bookmarkId, Url = "http://test", Title = "Test" });
        }

        public Task<IReadOnlyCollection<Bookmark>> ManageBookmarksAsync(IEnumerable<long> bookmarkIds, BookmarkAction action, CancellationToken ct = default)
        {
            var results = bookmarkIds.Select(id => new Bookmark { BookmarkId = id, Url = "http://test", Title = "Test" }).ToList();
            ManageBookmarkCallCount += results.Count;
            return Task.FromResult<IReadOnlyCollection<Bookmark>>(results);
        }

        public Task<Bookmark> MoveBookmarkAsync(long bookmarkId, long folderId, CancellationToken ct = default)
        {
            MoveBookmarkCalled = true;
            MoveBookmarkCallCount++;
            return Task.FromResult(new Bookmark { BookmarkId = bookmarkId, Url = "http://test", Title = "Test", FolderId = folderId });
        }

        public Task<IReadOnlyCollection<Bookmark>> MoveBookmarksAsync(IEnumerable<long> bookmarkIds, long folderId, CancellationToken ct = default)
        {
            var results = bookmarkIds.Select(id => new Bookmark { BookmarkId = id, Url = "http://test", Title = "Test", FolderId = folderId }).ToList();
            MoveBookmarkCallCount += results.Count;
            return Task.FromResult<IReadOnlyCollection<Bookmark>>(results);
        }

        public Task<IReadOnlyCollection<Folder>> ListFoldersAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyCollection<Folder>>([new Folder { FolderId = 1, Title = "Test" }]);

        public Task<Folder?> SearchFolderAsync(string title, CancellationToken ct = default) =>
            Task.FromResult<Folder?>(new Folder { FolderId = 1, Title = title });

        public Task<Folder> CreateFolderAsync(string title, CancellationToken ct = default) =>
            Task.FromResult(new Folder { FolderId = 1, Title = title });

        public Task<bool> DeleteFolderAsync(long folderId, CancellationToken ct = default) =>
            Task.FromResult(true);

        public Task<IReadOnlyCollection<Folder>> ReorderFoldersAsync((long FolderId, int Position)[] folderOrders, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<Folder>>(folderOrders.Select(o => new Folder { FolderId = o.FolderId, Title = "Test" }).ToList());
    }

    public static InstapaperClient CreateClientUnderTest(HttpClient httpClient)
    {
        var options = Options.Create(new Configuration.InstapaperOptions
        {
            ConsumerKey = "ck",
            ConsumerSecret = "cs",
            AccessToken = "access",
            AccessTokenSecret = "secret",
            Username = "u",
            Password = "p"
        });

        var logger = LoggerFactory.Create(_ => { }).CreateLogger<InstapaperClient>();
        var timeProvider = TimeProvider.System;

        return new InstapaperClient(httpClient, new FakeSignatureGenerator(), options, logger, timeProvider);
    }
}