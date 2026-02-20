# Agent Instructions

Instructions for GitHub Copilot and other AI coding agents working with the Instapaper MCP Server repository.

## Repository Overview

A minimal, agent-first MCP server for interacting with Instapaper.

This project is intentionally opinionated.
It is designed for **AI agents**, not for recreating the Instapaper UI.

### Key Components

- **Instapaper.Mcp.Server**: Instapaper MCP Server

### Technology Stack

- .NET 10.0
- C# 13 preview features
- xUnit SDK v3 with Microsoft.Testing.Platform for testing
- Microsoft.DotNet.Arcade.Sdk for build infrastructure
- ModelContextProtocol package for MCP implementation
- Multi-platform support (Windows, Linux, macOS, containers)

## General

- Make only high-confidence suggestions when reviewing code changes.
- Always use the latest version C#, currently C# 13 features.
- Never change global.json unless explicitly asked to.
- Never change package.json or package-lock.json files unless explicitly asked to.
- Never change NuGet.config files unless explicitly asked to.
- Don't update generated files (e.g., API definitions or auto-generated code) as they are generated.

## Code Review Instructions

When reviewing pull requests:

- New public APIs should be reviewed for design, naming, and functionality.
- Only flag API concerns if there are breaking changes to existing APIs without proper justification.

## Formatting

- Apply code-formatting style defined in `.editorconfig`.
- Prefer file-scoped namespace declarations and single-line using directives.
- Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`, `using`, `try`, etc.).
- Ensure that the final return statement of a method is on its own line.
- Use pattern matching and switch expressions wherever possible.
- Use `nameof` instead of string literals when referring to member names.
- Place private class declarations at the bottom of the file.

### Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.

### Building

Use standard .NET CLI commands for building the project. Ensure you have the correct .NET SDK installed as specified in global.json.

#### Prerequisites

1. **Install .NET SDK**: Download and install the required .NET SDK version from the official Microsoft website if not already present.

#### Build Commands

- **Restore Packages**: `dotnet restore`
- **Build**: `dotnet build`
- **Build with No Restore**: `dotnet build --no-restore` (assumes restore already done)
- **Clean Build**: `dotnet clean` followed by `dotnet build`
- **Package Generation**: `dotnet pack` to create NuGet packages

#### Build Troubleshooting

- If build fails with SDK errors, verify the .NET SDK version and run `dotnet restore` again.
- Treat warnings as errors; address all warnings before committing.
- Build artifacts go to `bin/` and `obj/` directories by default.

#### Visual Studio / VS Code Setup

- **VS Code**: Open the project folder and use the .NET CLI commands or integrated terminal.
- **Visual Studio**: Open the solution file (.slnx) and build from the IDE.

### Testing

- We use xUnit SDK v3 with Microsoft.Testing.Platform (https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro)
- Do not emit "Act", "Arrange" or "Assert" comments.
- We do not use any mocking framework at the moment.
- Copy existing style in nearby files for test method names and capitalization.
- Do not leave newly-added tests commented out. All added tests should be building and passing.
- Do not use Directory.SetCurrentDirectory in tests as it can cause side effects when tests execute concurrently.

## Running tests

(1) Restore and build the project: `dotnet restore` then `dotnet build`.
(2) If there are errors, fix them and rebuild.
(3) To run tests for a specific project: `dotnet test tests/Instapaper.Mcp.Server.Tests/Instapaper.Mcp.Server.Tests.csproj --no-build`
(4) To run specific tests, use filters: `dotnet test --filter "FullyQualifiedName~TestNamespace.TestClass.TestMethod"`

**Important**: In automation, use appropriate filters to exclude flaky or long-running tests.

### Test Verification Commands

- **Single Test Project**: Typical runtime ~10-60 seconds per test project
- **Full Test Suite**: Can take 30+ minutes, use targeted testing instead

## Project Layout and Architecture

### Directory Structure

- **`/src`**: Main source code for all packages
  - **`/src/Instapaper.Mcp.Server`**: Main MCP server implementation with tools, resources, and configuration
- **`/tests`**: Comprehensive test suites mirroring src structure
- **`/docs`**: Documentation including contributing guides and area ownership
- **`/.github`**: CI/CD workflows, issue templates, and GitHub automation

### Key Configuration Files

- **`global.json`**: Pins .NET SDK version - never modify without explicit request
- **`.editorconfig`**: Code formatting rules, null annotations, diagnostic configurations
- **`Directory.Build.props`**: Shared MSBuild properties across all projects
- **`Directory.Packages.props`**: Centralized package version management
- **`Instapaper.Mcp.slnx`**: Main solution file

### Dependencies and Hidden Requirements

- **Local .NET SDK**: Use the version specified in global.json
- **Package References**: Centrally managed via Directory.Packages.props
- **ModelContextProtocol**: Core dependency for MCP implementation
- **API Surface**: Public APIs should be stable; avoid breaking changes

### MCP Server Specifics

The server implements the Model Context Protocol (MCP) for integration with AI agents like Claude or Copilot. Key components include:

- **Tools**: Various bookmark and folder management tools
- **Resources**: Read-only access to Instapaper data
- **Prompts**: Built-in prompts for common tasks

### Common Validation Steps

1. **Build Verification**: `dotnet build` should complete without errors
2. **Package Generation**: `dotnet pack` verifies all packages can be created
3. **Specific Tests**: Target individual test projects related to your changes
4. **Integration Testing**: Test the MCP server with actual AI clients after changes

## Quarantined tests

- Tests that are flaky and don't fail deterministically can be marked with a custom attribute or skipped.
- Such tests should be isolated and run separately.

## Disabled tests

- Tests that consistently fail due to known issues can be skipped with attributes.
- Use for blocked tests, not flaky ones.

## Outerloop tests

- Long-running or resource-intensive tests should be marked and run separately.

## Snapshot Testing with Verify

- If using Verify for snapshot testing:
- Snapshot files in `Snapshots` directories.
- Accept changes with appropriate tools after updates.

## Editing resources

The `*.Designer.cs` files match `*.resx` files. Update both when changing resources.

## Markdown files

- No multiple consecutive blank lines.
- Code blocks with triple backticks and language identifier.
- Proper indentation for JSON.

## Localization files

- Do not manually translate localization files; use dedicated workflows.

## Trust These Instructions

These instructions are comprehensive. Only search for more if outdated or errors occur.
