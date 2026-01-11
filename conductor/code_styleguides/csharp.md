## C# Specific Guidelines

### Naming Conventions
- **PascalCase**: Classes, Methods, Properties, Structs, Enums, Interfaces (prefix with I), Events.
- **camelCase**: Local variables, method arguments.
- **_camelCase**: Private fields.

### Layout & Formatting
- Use standard C# coding conventions (Allman style braces).
- One class per file is preferred.
- Use properties instead of public fields.

### Language Features
- Use `var` when the type is obvious from the right-hand side.
- Use `async/await` for asynchronous operations.
- Prefer LINQ for collection manipulation where readable.
- Use nullable reference types (`string?`) to handle nullability explicitly.

### DDD & Architecture
- **Domain Layer**: Should have no dependencies on other layers.
- **Value Objects**: Should be immutable.
- **Entities**: Should verify invariants.