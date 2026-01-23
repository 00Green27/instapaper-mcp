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

### search_bookmarks
Primary discovery tool.

- Acts as both list and search
- Query is optional
- Folder defaults to `unread`
- Always limited

---

### get_article_content
Fetches article text for one or more bookmarks.

- Supports bulk by default
- Designed for analysis, summarization, and highlighting

---

### add_bookmark
Stores content for later use.

- Supports URLs and plain text notes
- Optional metadata (title, description)
- Folder assignment optional

---

### manage_bookmarks
Universal state transition tool.

Actions:
- archive
- unarchive
- delete
- star
- unstar

Bulk-first by design.

---

### move_bookmarks
Moves one or more bookmarks to a different folder.

---

### manage_folders
Folder lifecycle management.

Actions:
- list
- create
- delete

No ordering concepts.

---

### manage_highlights
Highlight management for articles.

Actions:
- list
- add
- delete

Designed to support “second brain” workflows.

---

## Resources

Resources expose Instapaper data in a read-only manner.

Important rules:
- All resources are limited by default
- Large datasets must be explicitly requested
- Context safety is a first-class concern

Example:
- `instapaper://highlights/all` enables cross-article knowledge analysis

---

## Prompts

This server is designed to work with higher-level prompts such as:
- clean_up_suggestions
- daily_briefing
- research_mode

Prompts should guide agents toward:
- decision-making
- synthesis
- extraction of meaning

---

## Design Constraints
These constraints are intentional:
- No progress tracking
- No UI ordering
- No redundant tools
- No silent magic

If you feel something is missing, ask:
“Does an AI agent actually need this?”

---

## How to Run

### 1. Configuration
Set your Instapaper API credentials using **User Secrets** (recommended for dev) or Environment Variables.

#### Option A: Automatic Authentication (xAuth)
If you provide your Instapaper credentials, the server will automatically exchange them for access tokens on the first request.

```bash
cd src/InstapaperMcp.Api
dotnet user-secrets set "Instapaper:ConsumerKey" "your_app_key"
dotnet user-secrets set "Instapaper:ConsumerSecret" "your_app_secret"
dotnet user-secrets set "Instapaper:Username" "your_email"
dotnet user-secrets set "Instapaper:Password" "your_password"
```

#### Option B: Manual Tokens (OAuth 1.0)
If you already have your access tokens, you can set them directly:

```bash
cd src/InstapaperMcp.Api
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
The server communicates via `stdin` and `stdout` using JSON-RPC.
```bash
dotnet run --project src/InstapaperMcp.Api
```

### 4. Integration with Claude Desktop
Add the following to your `claude_desktop_config.json`:
```json
{
  "mcpServers": {
    "instapaper": {
      "command": "dotnet",
      "args": ["run", "--project", "D:/Dev/personal/instapaper-mcp/src/InstapaperMcp.Api", "--no-build"],
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

---

## Status
This project is evolving.
Expect iteration, tightening of rules, and removal of features that add noise.

Clarity beats completeness.
