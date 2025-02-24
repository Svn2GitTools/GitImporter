# GitImporter
[![NuGet Package](https://img.shields.io/nuget/v/GitImporter?style=flat-square)](https://www.nuget.org/packages/GitImporter)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

# Overview
A .NET library for programmatically writing Git history to a Git repository.  It simplifies the process of creating commits, branches, tags, and managing Git repository structure from code.  While useful for tasks like converting repositories from other version control systems (e.g., Subversion), it can be used in any scenario where you need to generate Git history programmatically.

# Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Usage](#usage)
- [Authors Mapping](#authors-mapping)
- [License](#license)
- [Support](#support)

## Features

- **Programmatic Git History Creation:**  Provides an  API to create and manage Git commits, branches, and tags directly from your .NET code.
- **Abstracts Git Complexity:** Simplifies interaction with Git repositories, allowing developers to focus on the logic of history generation rather than low-level Git commands.
- **Step-by-Step Commit Building:** Supports building Git history revision by revision, suitable for processing data from external sources in chunks.
- **Author Mapping:**  Allows you to map author names to Git author identities (name and email), crucial for accurate history representation.
- **Flexible Change Handling:**  Provides mechanisms to define file and directory changes (add, modify, delete, replace) within commits, including content for both text and binary files.
- **Extensible Architecture:** Designed with interfaces and factories for customization and extension of conversion processes.
- **.NET 9.0 Compatibility:** Built using modern .NET features for performance and maintainability.

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later.
- A compatible IDE for .NET development (e.g., Visual Studio, Visual Studio Code with C# extension, Rider).
- Git command-line tools must be installed and accessible in your environment, as the library interacts with Git repositories on disk.

### Installation

As `GitImporter` is a library, you would typically include it in your .NET project using NuGet.

1. **Add NuGet Package:**
   You can add the `GitImporter` NuGet package to your project using the .NET CLI or your IDE's NuGet package manager.

   **Using .NET CLI:**

   ```bash
   dotnet add package GitImporter
   ```

   **Using Visual Studio NuGet Package Manager:**
   * Right-click on your project in Solution Explorer.
   * Select "Manage NuGet Packages...".
   * Go to the "Browse" tab and search for "GitImporter".
   * Select the `GitImporter` package and click "Install".

### Usage

To use `GitImporter`, you will typically:

1. **Create a `CompositionRoot`:** This is the entry point for accessing the library's services and managing the Git repository context.
2. **Get a `IGitRevisionConverter`:** This service is responsible for converting and writing individual revisions (commits) to the Git repository.
3. **Prepare `GitRevision` objects:**  Create instances of `GitRevision` and `GitNodeChange` (from `GitImporter.Models`) to represent the history you want to write to Git.  This is where you would process data from your source (e.g., SVN dump, another system, or generate history programmatically).
4. **Use the `IGitRevisionConverter` to convert and write each `GitRevision`**.
5. **Call `Checkout()` on the `CompositionRoot`** after all revisions are processed to finalize the Git repository (e.g., checkout the main branch).

**Example Code Snippet (Conceptual - You'll need to adapt this to your specific data source):**

```csharp
using GitImporter;
using GitImporter.Models;
using GitImporter.Interfaces; // Make sure to include this for IAuthorsMap if you're using it

public class ExampleImporter
{
    public static void Main(string[] args)
    {
        string gitRepoPath = "path/to/your/new/git/repo"; // Specify where to create/use the Git repo
        // You might need to initialize an empty Git repository at gitRepoPath beforehand

        // Example Author Mapping (if needed) - Implement IAuthorsMap
        IAuthorsMap authorsMap = new MyAuthorsMap(); // Create your own implementation

        using (var compositionRoot = new CompositionRoot(gitRepoPath, authorsMap))
        {
            var revisionConverter = compositionRoot.GetRevisionConverter();

            // Simulate processing some data source and creating GitRevisions
            List<GitRevision> revisionsToImport = GenerateRevisionsFromDataSource();

            foreach (var gitRevision in revisionsToImport)
            {
                revisionConverter.ConvertRevision(gitRevision);
            }

            compositionRoot.Checkout(); // Finalize Git repository
        }

        Console.WriteLine("Git history import complete!");
    }

    // Example:  You would replace this with your actual data processing logic
    static List<GitRevision> GenerateRevisionsFromDataSource()
    {
        var revisions = new List<GitRevision>();

        // Example Revision 1
        var revision1 = new GitRevision
        {
            Number = 1,
            Author = "user1", // SVN username or similar identifier
            Date = DateTime.Now.AddDays(-2),
            LogMessage = "Initial commit",
        };
        revision1.AddNode(new GitNodeChange { Action = EChangeAction.Add, Kind = ENodeKind.File, Path = "file1.txt", TextContent = "Content of file1" });
        revisions.Add(revision1);

        // Example Revision 2
        var revision2 = new GitRevision
        {
            Number = 2,
            Author = "user2", // SVN username or similar identifier
            Date = DateTime.Now.AddDays(-1),
            LogMessage = "Modify file1 and add file2",
        };
        revision2.AddNode(new GitNodeChange { Action = EChangeAction.Modify, Kind = ENodeKind.File, Path = "file1.txt", TextContent = "Updated content of file1" });
        revision2.AddNode(new GitNodeChange { Action = EChangeAction.Add, Kind = ENodeKind.File, Path = "file2.txt", TextContent = "Content of file2" });
        revisions.Add(revision2);

        return revisions;
    }

    // Example IAuthorsMap implementation (replace with your actual mapping logic)
    class MyAuthorsMap : IAuthorsMap
    {
        public string GetAuthorEmail(string authorName)
        {
            // Implement your author mapping logic here.
            // For example, based on authorName, return the corresponding Git email.
            if (authorName == "user1") return "user1@example.com";
            if (authorName == "user2") return "user2@example.com";
            return "default@example.com"; // Default email if no mapping found
        }
    }
}
```

**Important Notes:**

* **Repository Initialization:**  `GitImporter` assumes you have a Git repository ready at the specified `gitRepoPath`. You might need to initialize an empty repository using `git init` command or through code before using `GitImporter`. The library does not create the repository itself.
* **Error Handling:** The example code is simplified.  In a real application, you would need to add proper error handling, logging, and potentially progress reporting.
* **Author Mapping Implementation:** You'll need to implement the `IAuthorsMap` interface according to your author mapping requirements.  The example `MyAuthorsMap` is just a placeholder.
* **Data Source Integration:** The `GenerateRevisionsFromDataSource()` function is a placeholder. You will replace this with code that reads your data source (SVN dump, database, other VCS, etc.) and transforms it into `GitRevision` objects.

## Authors Mapping

The `GitImporter` library supports author mapping through the `IAuthorsMap` interface. This is essential for correctly attributing commits in Git when importing history from systems that use different author identifiers (like SVN usernames).

To use author mapping:

1. **Implement the `IAuthorsMap` Interface:** Create a class that implements the `GitImporter.Interfaces.IAuthorsMap` interface.  The interface has a single method:

   ```csharp
   string GetAuthorEmail(string authorName);
   ```

   Your implementation of `GetAuthorEmail` should take an author name (e.g., an SVN username) as input and return the corresponding Git author's email address. You can use any logic you need within this method (e.g., lookups in a dictionary, reading from a file, etc.).

2. **Pass your `IAuthorsMap` implementation to the `CompositionRoot` constructor** when creating an instance of `CompositionRoot`. If you don't provide an `IAuthorsMap`, a default implementation will be used (which might not perform any mapping).

**Example `IAuthorsMap` Implementation (using a Dictionary):**

```csharp
using GitImporter.Interfaces;
using System.Collections.Generic;

public class DictionaryAuthorsMap : IAuthorsMap
{
    private readonly Dictionary<string, string> _authorMap;

    public DictionaryAuthorsMap(Dictionary<string, string> authorMap)
    {
        _authorMap = authorMap;
    }

    public string GetAuthorEmail(string authorName)
    {
        if (_authorMap.TryGetValue(authorName, out string? email))
        {
            return email;
        }
        return "default@example.com"; // Default email if no mapping found
    }
}
```

**Usage with `DictionaryAuthorsMap`:**

```csharp
// ... (rest of your code)

Dictionary<string, string> myAuthorMappings = new Dictionary<string, string>()
{
    {"svn_user1", "Git User One <gituser1@example.com>"},
    {"svn_user2", "Git User Two <gituser2@example.com>"}
};
IAuthorsMap authorsMap = new DictionaryAuthorsMap(myAuthorMappings);

using (var compositionRoot = new CompositionRoot(gitRepoPath, authorsMap))
{
    // ... (rest of your import logic)
}
```

## License

This project is licensed under the [MIT License](LICENSE). See the `LICENSE` file for details.

## Support

For questions, bug reports, or feature requests, please open an issue on the [GitHub repository]([Your GitHub Repository Link Here]).

