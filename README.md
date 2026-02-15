# instapaper-mcp

A minimal, agent-first MCP server for interacting with Instapaper.

This project is intentionally opinionated.
It is designed for **AI agents**, not for recreating the Instapaper UI.

---

## What This Is

- A Model Context Protocol (MCP) server
- A clean abstraction over Instapaper data
- A toolset optimized for LLM reasoning
- A foundation for reading, research, and knowledge extraction workflows

---

## What This Is NOT

- A full Instapaper client
- A UI-focused API
- A mirror of the official Instapaper feature set
- A CRUD playground

---

## Core Philosophy

### Agent-First Design

This project assumes:

- Agents reason in intents, not clicks
- Fewer tools lead to better decisions
- Bulk operations are the norm
- Context size is a scarce resource

Every design decision must reduce:

- Tool selection ambiguity
- Context pollution
- Redundant concepts

---

## Tools Overview

### Bookmark Management Tools

#### list_bookmarks

List or search bookmarks. Defaults to unread folder.

- Optional search query
- Optional folder ID to search in (unread, starred, archive, or a folder ID)
- Maximum number of items to return (default 100)

#### add_bookmark

Add a new bookmark or note.

- URL of the bookmark or note
- Optional title and description
- Optional folder ID to add to
- Full HTML content of the page
- Options for resolving redirects and archiving on add

#### archive_bookmark / unarchive_bookmark

Move bookmark to/from archive.

- Single bookmark ID or list of bookmark IDs

#### mark_bookmark / unmark_bookmark

Mark/unmark bookmark as important.

- Single bookmark ID

#### move_bookmarks / move_bookmark

Move bookmarks to a different folder.

- List of bookmark IDs to move and target folder ID

### Folder Management Tools

#### list_folders

List all user folders.

#### create_folder

Create an organizational folder.

- Title of the folder

#### delete_folder

Delete a folder.

- Folder ID

#### reorder_folders

Re-order a user's folders.

- Array of folder ID and position tuples

---

## Resources

Resources expose Instapaper data in a read-only manner.

Important rules:

- All resources are limited by default
- Large datasets must be explicitly requested
- Context safety is a first-class concern

Available resources:

- `instapaper://bookmarks/unread` - List of unread bookmarks
- `instapaper://bookmarks/archive` - List of archived bookmarks
- `instapaper://bookmarks/starred` - List of starred bookmarks
- `instapaper://folders` - List of all user folders
- `instapaper://bookmark/{id}` - Full text and metadata for specific bookmark

---

## Prompts

The server provides several built-in prompts for common tasks:

- `organize_reading_list` - Analyze and organize unread bookmarks into folders
- `weekly_digest` - Generate summary of unread bookmarks from last 7 days
- `recommend_next` - Suggest what to read next based on context
- `research_mode` - Analyze bookmarks on a specific topic
- `clean_up_suggestions` - Identify old or irrelevant bookmarks for archiving

---

## Design Constraints

These constraints are intentional:

- No progress tracking
- No UI ordering
- No redundant tools
- No silent magic

If you feel something is missing, ask:
"Does an AI agent actually need this?"

---

## How to Run

### 1. Configuration

Set your Instapaper API credentials using **User Secrets** (recommended for dev) or Environment Variables.

#### Option A: Automatic Authentication (xAuth)

If you provide your Instapaper credentials, the server will automatically exchange them for access tokens on the first request.

```bash
cd src/Instapaper.Mcp.Server
dotnet user-secrets set "Instapaper:ConsumerKey" "your_app_key"
dotnet user-secrets set "Instapaper:ConsumerSecret" "your_app_secret"
dotnet user-secrets set "Instapaper:Username" "your_email"
dotnet user-secrets set "Instapaper:Password" "your_password"
```

#### Option B: Manual Tokens (OAuth 1.0)

If you already have your access tokens, you can set them directly:

```bash
cd src/Instapaper.Mcp.Server
dotnet user-secrets set "Instapaper:ConsumerKey" "your_app_key"
dotnet user-secrets set "Instapaper:ConsumerSecret" "your_app_secret"
dotnet user-secrets set "Instapaper:AccessToken" "your_token"
dotnet user-secrets set "Instapaper:AccessTokenSecret" "your_token_secret"
```

#### Environment Variables:

The following environment variables are supported (compatible with Docker/CI):

- `Instapaper__ConsumerKey`
- `Instapaper__ConsumerSecret`
- `Instapaper__Username`
- `Instapaper__Password`
- `Instapaper__AccessToken`
- `Instapaper__AccessTokenSecret`

### 2. Build

```bash
dotnet build
```

### 3. Run

The server communicates via `stdin` and `stdout` using JSON-RPC. For local development:

```bash
dotnet run --project src/Instapaper.Mcp.Server --no-build -v quiet
```

### 4. Integration with MCP Clients (Claude, Copilot, etc.)

Add the following to your configuration file (e.g., `claude_desktop_config.json` or `config.json` for Copilot).
**Note:** Use absolute paths to ensure the project can be located from any working directory.

#### Using `dotnet run` (Development)

```json
{
  "mcpServers": {
    "instapaper": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "D:/Dev/Instapaper.Mcp.Server",
        "--no-build",
        "-v",
        "quiet"
      ],
      "env": {
        "Instapaper__ConsumerKey": "...",
        "Instapaper__ConsumerSecret": "...",
        "Instapaper__AccessToken": "...",
        "Instapaper__AccessTokenSecret": "..."
      }
    }
  }
}
```

#### Using published DLL (Production - Recommended)

1. Publish the project: `dotnet publish src/Instapaper.Mcp.Server -c Release -o ./publish`
2. Use the DLL directly:

```json
{
  "mcpServers": {
    "instapaper": {
      "command": "dotnet",
      "args": ["D:/McpServers/Instapaper.Mcp.Server.dll"],
      "env": { ... }
    }
  }
}
```
