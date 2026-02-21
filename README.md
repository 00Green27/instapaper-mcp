# instapaper-mcp

[![CI](https://github.com/00Green27/instapaper-mcp/actions/workflows/ci.yml/badge.svg)](https://github.com/00Green27/instapaper-mcp/actions/workflows/ci.yml)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

MCP server for interacting with Instapaper. Optimized for AI agents — manage bookmarks, organize content, and analyze articles through tools and resources.

---

## Quick Start

### 1. Configure Credentials

```bash
cd src/Instapaper.Mcp.Server
dotnet user-secrets set "Instapaper:ConsumerKey" "your_app_key"
dotnet user-secrets set "Instapaper:ConsumerSecret" "your_app_secret"
dotnet user-secrets set "Instapaper:Username" "your_email"
dotnet user-secrets set "Instapaper:Password" "your_password"
```

Or via environment variables:

- `Instapaper__ConsumerKey`
- `Instapaper__ConsumerSecret`
- `Instapaper__Username`
- `Instapaper__Password`

### 2. Build

```bash
dotnet build
```

### 3. Run

```bash
dotnet run --project src/Instapaper.Mcp.Server --no-build -v quiet
```

### 4. MCP Client Integration

**Development** — Add to your MCP client configuration (e.g., `claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "instapaper": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/instapaper-mcp/src/Instapaper.Mcp.Server",
        "--no-build",
        "-v",
        "quiet"
      ],
      "env": {
        "Instapaper__ConsumerKey": "your_key",
        "Instapaper__ConsumerSecret": "your_secret",
        "Instapaper__Username": "your_email",
        "Instapaper__Password": "your_password"
      }
    }
  }
}
```

**Production** — Use the published DLL directly:

```bash
dotnet publish src/Instapaper.Mcp.Server -c Release -o ./publish
```

```json
{
  "mcpServers": {
    "instapaper": {
      "command": "dotnet",
      "args": ["/absolute/path/to/Instapaper.Mcp.Server.dll"],
      "env": {
        "Instapaper__ConsumerKey": "your_key",
        "Instapaper__ConsumerSecret": "your_secret",
        "Instapaper__Username": "your_email",
        "Instapaper__Password": "your_password"
      }
    }
  }
}
```

---

## Configuration

| Variable                        | Description                               |
| ------------------------------- | ----------------------------------------- |
| `Instapaper__ConsumerKey`       | Application API key                       |
| `Instapaper__ConsumerSecret`    | Application API secret                    |
| `Instapaper__Username`          | Instapaper account email                  |
| `Instapaper__Password`          | Instapaper account password               |
| `Instapaper__AccessToken`       | Access token (optional, for OAuth)        |
| `Instapaper__AccessTokenSecret` | Access token secret (optional, for OAuth) |

---

## Tools

| Tool                 | Description                                  |
| -------------------- | -------------------------------------------- |
| `list_bookmarks`     | List/search bookmarks (folder, limit, query) |
| `add_bookmark`       | Add a new bookmark or note                   |
| `archive_bookmark`   | Move bookmark to archive                     |
| `unarchive_bookmark` | Restore bookmark from archive                |
| `mark_bookmark`      | Mark bookmark as important                   |
| `unmark_bookmark`    | Remove important mark                        |
| `move_bookmarks`     | Move bookmarks to another folder             |
| `list_folders`       | List all folders                             |
| `create_folder`      | Create a new folder                          |
| `delete_folder`      | Delete a folder                              |
| `reorder_folders`    | Reorder folders                              |

---

## Resources

Resources provide read-only access to Instapaper data:

- `instapaper://bookmarks/unread` — Unread bookmarks
- `instapaper://bookmarks/archive` — Archived bookmarks
- `instapaper://bookmarks/starred` — Starred bookmarks
- `instapaper://folders` — List of folders
- `instapaper://bookmark/{id}` — Full text and metadata for a bookmark

---

## Example Workflows

### Analyze Unread Articles

> "Show me unread bookmarks from the last week and highlight the most interesting ones by title"

### Organize by Topic

> "Create folders for 'AI', 'DevOps', 'Security' and organize unread bookmarks into them"

### Weekly Digest

> "Generate a brief summary of unread articles from the past 7 days with key points"

### Archive Cleanup

> "Find old archived bookmarks (older than 6 months) and suggest which to delete"

---

## Docker

### Build Image (.NET SDK Container Support)

This project uses .NET SDK Container Support for simplified container builds.

```bash
# Build and publish container image
dotnet publish /t:PublishContainer -c Release
```

This creates the `00green27/instapaper-mcp-server:latest` image (Alpine-based) for linux-x64.

### Run Container

```bash
docker run -d \
  --name instapaper-mcp \
  -e Instapaper__ConsumerKey="your_key" \
  -e Instapaper__ConsumerSecret="your_secret" \
  -e Instapaper__Username="your_email" \
  -e Instapaper__Password="your_password" \
  00green27/instapaper-mcp-server:latest
```

### Docker Compose

```bash
# Create .env file with your credentials first
docker-compose up -d
```

---

## Troubleshooting

| Issue              | Resolution                                          |
| ------------------ | --------------------------------------------------- |
| 401 Unauthorized   | Verify credentials; tokens refresh automatically    |
| Rate limiting      | Instapaper API limits requests — wait a few minutes |
| Bookmark not found | Check bookmark ID (use `list_bookmarks`)            |
