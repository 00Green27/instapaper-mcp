# AGENTS.md — instapaper-mcp

## Purpose
This document defines strict rules for AI agents and contributors working on the
instapaper-mcp project.

The primary goal of this repository is to provide a **clean, minimal, agent-first MCP server**
for interacting with Instapaper data in a way that is reliable, predictable, and context-safe
for large language models.

---

## Agent Role
You are acting as a senior .NET engineer with experience in:

- C# and modern .NET
- ASP.NET Core
- Model Context Protocol (MCP)
- Agent-oriented API design
- Distributed and integration-heavy systems

You must think in terms of **agent intent**, not UI workflows.

---

## Core Design Principles

### Agent-First API
This project is designed for AI agents, not human users.

Avoid:
- UI-driven abstractions
- Visual concepts (ordering, progress bars, pagination as UX)
- Redundant tools for similar intents

Prefer:
- Intent-based operations
- State transitions (unread → archived)
- Bulk operations by default

---

### Tool Minimalism (Critical Rule)
Tool explosion is considered a bug.

Rules:
- One intent = one tool
- Bulk operations are the default
- Single-item operations are represented as arrays of size 1
- No separate “bulk” tools

If a new tool is proposed, it must justify:
1. Why it cannot be expressed as an action
2. Why it cannot reuse an existing tool

---

## Tool Philosophy

### add_bookmark
Represents a single concept: “store something to read or remember”.

- If `url` is provided → external article
- If `content` is provided → private note
- No separate “private bookmark” concept

---

### State Management
Agents do not “read with eyes”.

- Reading progress is irrelevant
- “Read” is modeled as `archive`
- No progress tracking tools are allowed

---

### Folder Management
Folders are logical containers, not visual lists.

- Ordering is irrelevant
- Reordering tools are forbidden
- Move operations must be explicit and bulk-capable

---

## Resources and Context Safety

Resources must be safe for LLM context windows.

Rules:
- No unbounded lists
- Default limits must be enforced
- Lazy or paged access is preferred
- Large resources must be explicitly requested

Example:
- `instapaper://bookmarks/unread` must return a limited subset by default

---

## Testing Expectations
- Core logic must be unit-testable
- Tool behavior must be deterministic
- No hidden state or side effects
- Errors must be explicit and explainable to agents

---

## Forbidden Changes
AI agents and contributors must not:
- Add UI-only concepts
- Introduce redundant tools
- Add silent breaking changes
- Return unbounded data from resources

---

## Guiding Principle
If an AI agent can misunderstand an API, it eventually will.

Design for clarity, not completeness.
