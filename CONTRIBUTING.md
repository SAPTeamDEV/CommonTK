# Contributing to CommonTK Project

Thank you for considering contributing to CommonTK! We welcome contributions from the community and are excited to work with you. This guide will help you get started with contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How to Contribute](#how-to-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Features](#suggesting-features)
  - [Submitting Pull Requests](#submitting-pull-requests)
- [Development Setup](#development-setup)
- [Native AOT Compilation](#native-aot-compilation)
- [Style Guide](#style-guide)
- [License](#license)

## Code of Conduct

Please read and follow our [Code of Conduct](https://dotnetfoundation.org/code-of-conduct) to ensure a welcoming and inclusive environment for everyone.

## How to Contribute

### Reporting Bugs

If you find a bug in the project, please create an issue on GitHub with the following information:
- A clear and descriptive title.
- A detailed description of the problem.
- Steps to reproduce the issue.
- Any relevant logs, screenshots, or error messages.

If the bug is related to security, **DO NOT REPORT IT ON GITHUB ISSUES**, please read our [Security Guidelines](SECURITY.md).

### Suggesting Features

We welcome feature suggestions! To suggest a new feature, please create an issue on GitHub with the following information:
- A clear and descriptive title.
- A detailed description of the proposed feature.
- Any relevant use cases or examples.

### Submitting Pull Requests

We appreciate your contributions! To submit a pull request, follow these steps:
1. Fork the repository and clone your fork.
2. Create a new branch for your changes.
3. Make your changes, ensuring that your code adheres to the project's style guide.
4. Commit your changes with a clear and descriptive commit message.
5. Push your changes to your fork.
6. Create a pull request on GitHub, providing a detailed description of your changes.

## Development Setup

To set up your development environment, follow these steps:
1. Ensure you have [.NET SDK](https://dotnet.microsoft.com/download) installed. You must have .NET SDK 9 installed.
2. Clone the repository:
   
```
   git clone https://github.com/SAPTeamDEV/CommonTK.git
   cd CommonTK
```

3. Restore the project dependencies:
   
```
   dotnet restore
```

4. Build the project:
   
```
   dotnet build
```

You could find output files in artifacts folder.

## Style Guide

Please follow these guidelines to ensure consistency in the codebase:
- Use [C# coding conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).
- Write clear and descriptive commit messages.
- Ensure your code is well-documented with XML comments where appropriate.
- Run tests before submitting your pull request to ensure your changes do not break existing functionality.

## License

By contributing to CommonTK, you agree that your contributions will be licensed under the [MIT License](LICENSE.md).

Thank you for your contributions!
