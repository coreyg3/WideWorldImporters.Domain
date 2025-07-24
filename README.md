# WideWorldImporters.Domain

Domain-driven design models and value objects for the Wide World Importers business domain. This repository provides the core entities, value objects, and supporting types for building robust, maintainable business applications.

---

## Table of Contents
- [Overview](#overview)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Building the Solution](#building-the-solution)
- [Running Tests](#running-tests)
- [Using the NuGet Package](#using-the-nuget-package)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

This solution implements the core domain model for Wide World Importers, including:
- **Entities** for Sales, Purchasing, and Warehouse management
- **Value Objects** for financials, addresses, and business rules
- **Shared primitives** for common business concepts

The goal is to provide a reusable, extensible foundation for business applications following domain-driven design (DDD) principles.

---

## Project Structure

```
WideWorldImporters.Domain.sln
WideWorldImporters.Domain/           # Domain models and value objects
WideWorldImporters.Domain.Tests/     # Unit tests for domain logic
scripts/                            # Build and packaging scripts
CHANGELOG.md                        # Semantic versioning changelog
LICENSE                             # Open source license (MIT)
README.md                           # Solution overview (this file)
```

---

## Getting Started

1. **Clone the repository:**
   ```sh
   git clone https://github.com/coreyg3/WideWorldImporters.Domain.git
   cd WideWorldImporters.Domain
   ```
2. **Restore dependencies:**
   ```sh
   dotnet restore
   ```
3. **Build the solution:**
   ```sh
   dotnet build
   ```

---

## Building the Solution

To build all projects:
```sh
dotnet build WideWorldImporters.Domain.sln
```

To build and create a NuGet package:
```sh
pwsh ./scripts/package.ps1 -Strategy Manual
```

---

## Running Tests

To run all unit tests:
```sh
dotnet test WideWorldImporters.Domain.sln
```

---

## Using the NuGet Package

The domain models are published as a NuGet package:

```sh
dotnet add package WideWorldImporters.Domain
```

Or reference in your `.csproj`:
```xml
<PackageReference Include="WideWorldImporters.Domain" Version="1.0.0" />
```

See the [NuGet Gallery](https://www.nuget.org/packages/WideWorldImporters.Domain) for the latest version.

---

## Contributing

Contributions are welcome! Please open issues or submit pull requests for improvements, bug fixes, or new features.

- Follow the DDD and clean architecture principles.
- See project-level READMEs for details on each subproject.

---

## License

This project is licensed under the [MIT License](LICENSE).

---

## Links
- [Changelog](CHANGELOG.md)
- [NuGet Package](https://www.nuget.org/packages/WideWorldImporters.Domain)
- [GitHub Repository](https://github.com/<yourusername>/WideWorldImporters.Domain) 