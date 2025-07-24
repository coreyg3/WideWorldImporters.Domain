# NuGet Package Creation and Versioning Guide

## Table of Contents
1. [Conceptual Framework](#conceptual-framework)
2. [Versioning Strategies](#versioning-strategies)
3. [Implementation Approaches](#implementation-approaches)
4. [Automation and CI/CD](#automation-and-cicd)
5. [Advanced Scenarios](#advanced-scenarios)
6. [Best Practices](#best-practices)

## Conceptual Framework

### NuGet Package Architecture

A NuGet package is fundamentally a structured archive containing:

```
package.nupkg
├── lib/                    # Compiled assemblies
│   └── net8.0/
│       └── WideWorldImporters.Domain.dll
├── build/                  # MSBuild targets (optional)
├── content/                # Content files (optional)
├── tools/                  # PowerShell scripts (optional)
└── package.nuspec          # Package metadata
```

### Versioning Dimensions

.NET packages operate with multiple version identifiers serving distinct purposes:

1. **PackageVersion**: Consumer-facing version for dependency resolution
2. **AssemblyVersion**: Runtime identity used by the CLR for binding
3. **FileVersion**: Build-specific identifier for diagnostics
4. **InformationalVersion**: Human-readable version with metadata

### Semantic Versioning Foundation

The semantic versioning specification provides a logical framework:

```
MAJOR.MINOR.PATCH[-PRERELEASE][+BUILDMETADATA]
```

- **MAJOR**: Incompatible API changes
- **MINOR**: Backward-compatible functionality additions
- **PATCH**: Backward-compatible bug fixes
- **PRERELEASE**: Development versions (alpha, beta, rc)
- **BUILDMETADATA**: Build-specific information

## Versioning Strategies

### 1. Manual Semantic Versioning

**Philosophy**: Human-controlled version management with explicit decision-making.

**Implementation**:
```xml
<PropertyGroup>
  <VersionPrefix>1.0.0</VersionPrefix>
  <VersionSuffix></VersionSuffix>
</PropertyGroup>
```

**Advantages**:
- Complete control over version semantics
- Clear correlation between changes and version increments
- Simple to understand and implement

**Considerations**:
- Requires discipline and coordination in teams
- Prone to human error in version bumping
- Manual process can become bottleneck

### 2. Git-Based Automated Versioning

**Philosophy**: Derive versions from Git repository state and commit metadata.

**Implementation** (using GitVersion):
```json
{
  "version": "1.0",
  "assemblyVersion": { "precision": "major.minor" },
  "publicReleaseRefSpec": ["^refs/heads/main$"],
  "release": {
    "versionIncrement": "minor",
    "firstUnstableTag": "alpha"
  }
}
```

**Advantages**:
- Automatic version calculation based on branching strategy
- Consistent versioning across distributed teams
- Integration with Git workflow

**Considerations**:
- Requires understanding of branching model
- Less explicit control over version semantics
- Dependency on specific tooling

### 3. Build-Number Based Versioning

**Philosophy**: Incorporate build system metadata for unique identification.

**Implementation**:
```xml
<PropertyGroup Condition="'$(BuildNumber)' != ''">
  <FileVersion>$(VersionPrefix).$(BuildNumber)</FileVersion>
  <VersionSuffix>build.$(BuildNumber)</VersionSuffix>
</PropertyGroup>
```

**Advantages**:
- Unique identifier for every build
- Traceability to build system
- Suitable for continuous integration

**Considerations**:
- Version numbers can become large
- Not semantically meaningful to consumers
- Requires CI/CD infrastructure

### 4. Hybrid Approaches

**Philosophy**: Combine multiple strategies for different scenarios.

**Example Configuration**:
```xml
<!-- Production: Manual semantic versions -->
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <Version>$(VersionPrefix)</Version>
</PropertyGroup>

<!-- Development: Timestamp-based versions -->
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <VersionSuffix>dev.$([System.DateTime]::Now.ToString('yyyyMMddHHmm'))</VersionSuffix>
</PropertyGroup>
```

## Implementation Approaches

### Project-Level Configuration

The most direct approach involves configuring the `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- Core Package Identity -->
    <PackageId>WideWorldImporters.Domain</PackageId>
    <Title>Wide World Importers Domain Models</Title>
    <Description>Domain models and business logic</Description>
    
    <!-- Versioning -->
    <VersionPrefix>1.0.0</VersionPrefix>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    
    <!-- Package Metadata -->
    <Authors>Your Name</Authors>
    <Company>Your Company</Company>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    
    <!-- Quality Gates -->
    <EnablePackageValidation>true</EnablePackageValidation>
    <GeneratePackageOnBuild>false</GeneratePackageOnBuild>
  </PropertyGroup>
</Project>
```

### Centralized Configuration with Directory.Build.props

For multi-project solutions, centralize common properties:

```xml
<Project>
  <!-- Shared versioning logic -->
  <PropertyGroup>
    <MajorVersion>1</MajorVersion>
    <MinorVersion>0</MinorVersion>
    <PatchVersion>0</PatchVersion>
    <VersionPrefix>$(MajorVersion).$(MinorVersion).$(PatchVersion)</VersionPrefix>
  </PropertyGroup>
  
  <!-- Environment-specific overrides -->
  <PropertyGroup Condition="'$(Configuration)' == 'Debug'">
    <VersionSuffix>dev</VersionSuffix>
  </PropertyGroup>
</Project>
```

## Automation and CI/CD

### Command-Line Package Creation

Basic package creation:
```bash
dotnet pack --configuration Release --output ./packages
```

Advanced package creation with version override:
```bash
dotnet pack \
  --configuration Release \
  --output ./packages \
  -p:Version=1.2.3-beta.1 \
  -p:PackageVersion=1.2.3-beta.1
```

### CI/CD Pipeline Integration

**Azure DevOps Example**:
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Create NuGet Package'
  inputs:
    command: 'pack'
    packagesToPack: '**/*.csproj'
    configuration: 'Release'
    versioningScheme: 'byBuildNumber'
    versionEnvVar: 'BUILD_BUILDNUMBER'
```

**GitHub Actions Example**:
```yaml
- name: Create NuGet Package
  run: |
    dotnet pack \
      --configuration Release \
      --output ./packages \
      -p:Version=${{ steps.version.outputs.version }}
```

## Advanced Scenarios

### Multi-Targeting Packages

Support multiple .NET versions:
```xml
<PropertyGroup>
  <TargetFrameworks>net8.0;net6.0;netstandard2.1</TargetFrameworks>
</PropertyGroup>
```

### Conditional Dependencies

Include dependencies based on target framework:
```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <PackageReference Include="System.Text.Json" Version="8.0.0" />
</ItemGroup>
```

### Symbol Packages

Enable debugging support:
```xml
<PropertyGroup>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
</PropertyGroup>
```

### Package Validation

Ensure package quality:
```xml
<PropertyGroup>
  <EnablePackageValidation>true</EnablePackageValidation>
  <PackageValidationBaselineVersion>1.0.0</PackageValidationBaselineVersion>
</PropertyGroup>
```

## Best Practices

### Version Number Management

1. **Conservative AssemblyVersion**: Only increment for breaking changes
2. **Semantic PackageVersion**: Follow SemVer strictly for consumer clarity
3. **Descriptive FileVersion**: Include build metadata for diagnostics

### Package Metadata

1. **Comprehensive Description**: Clearly explain package purpose and scope
2. **Appropriate Tags**: Enable discoverability
3. **License Information**: Specify licensing terms explicitly
4. **Repository Links**: Provide source code access

### Quality Assurance

1. **Package Validation**: Enable automatic validation
2. **Symbol Packages**: Include debugging information
3. **Source Link**: Enable source code debugging
4. **Dependency Minimization**: Avoid unnecessary dependencies

### Release Management

1. **Changelog Maintenance**: Document all changes systematically
2. **Pre-release Testing**: Use alpha/beta versions for validation
3. **Backward Compatibility**: Maintain API contracts across minor versions
4. **Deprecation Strategy**: Communicate breaking changes in advance

## Command Reference

### Basic Commands

Create package:
```bash
dotnet pack
```

Create with specific version:
```bash
dotnet pack -p:Version=1.2.3
```

Create with custom output:
```bash
dotnet pack --output ./dist
```

### Advanced Commands

Create with multiple properties:
```bash
dotnet pack \
  -p:Version=1.2.3-alpha.1 \
  -p:AssemblyVersion=1.2.0.0 \
  -p:FileVersion=1.2.3.123
```

Validate package:
```bash
dotnet pack --verbosity normal
```

### Publishing Commands

Publish to NuGet.org:
```bash
dotnet nuget push package.1.0.0.nupkg --api-key <key> --source https://api.nuget.org/v3/index.json
```

Publish to private feed:
```bash
dotnet nuget push package.1.0.0.nupkg --source https://your-feed.com/v3/index.json
```

## Troubleshooting

### Common Issues

1. **Version Conflicts**: Ensure consistent versioning across projects
2. **Missing Metadata**: Verify all required package properties are set
3. **Build Failures**: Check for compilation errors before packaging
4. **Invalid Versions**: Validate version format compliance

### Debugging Tools

1. **Package Explorer**: Visual inspection of package contents
2. **dotnet pack --verbosity diagnostic**: Detailed build information
3. **NuGet Package Validator**: Automated package validation
4. **MSBuild Binary Log**: Complete build analysis

This guide provides a comprehensive framework for understanding and implementing NuGet packaging and versioning strategies, enabling you to choose the approach that best fits your project's requirements and constraints. 