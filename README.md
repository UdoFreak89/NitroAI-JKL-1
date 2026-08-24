# NitroAI-JKL-1

NitroAI-JKL-1 is a small .NET 10 console assistant designed to remain local and lightweight. It understands basic requests in Czech and English and can help with simple questions, planning, brainstorming, text processing, calculations, games, and starter code generation.

## Features

- Czech and English conversation detection
- English console interface
- A short conversation context window
- English response for unsupported languages
- Local-first design with no required cloud service or API key
- Beginner-friendly explanations after generated code
- Clear `Code` / `Kód` sections and an end-of-code marker
- Basic code generation for:
  - C
  - C#
  - C++
  - Python
  - Batch
  - HTML
  - WPF/XAML
- Starter examples such as:
  - calculators
	- number-guessing games
  - Rock Paper Scissors
  - TODO lists
  - console applications
  - HTML pages
  - WPF windows
  - DTOs and service classes

## Requirements

- .NET 10 SDK
- Windows, macOS, or Linux terminal

## Run the project

From the repository root, run:

```bash
dotnet run --project NitroAI-jkl-1/NitroAI-jkl-1.csproj
```

The application starts with the `agent>` prompt. You can write naturally, for example:

```text
Can you make a C++ game?
Create a Python TODO app.
Naprogramuj kalkulačku v C#.
Vytvoř webovou stránku v HTML.
Explain how a loop works.
Plan a personal website.
```

Type `exit` to close the application. The goodbye message remains visible briefly before the console closes.

## Important note

This is currently a local rule-based assistant, not a full large language model. It recognizes common words and patterns and returns prepared starter templates. It does not require a cloud service, API key, or internet connection to run. It is intentionally designed to remain a local model/assistant and does not upload conversations to a remote AI service. It does not yet freely understand every topic or modify project files autonomously.

Future versions can keep this local-only approach by connecting NitroAI-JKL-1 to an optional locally hosted model and adding project file analysis, patch generation, builds, tests, and code repair.

## Project structure

```text
NitroAI-jkl-1.slnx
NitroAI-jkl-1/
  NitroAI-jkl-1.csproj
  Program.cs
```

## License

No license has been specified yet.
