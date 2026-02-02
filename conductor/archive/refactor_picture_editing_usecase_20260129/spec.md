# Specification: PictureEditingUseCase Refactoring

## 1. Overview
Current implementation of `PictureEditingUseCase` acts as a "Service Class" (God Class) handling multiple disparate responsibilities: geometric transformations (Flip/Rotate) and image transfer operations (Push/Pull) between canvas and dock. This refactoring aims to dismantle this service and redistribute its responsibilities into atomic, domain-focused UseCases, adhering to the Single Responsibility Principle and strict Onion Architecture.

## 2. Goals
- **Eliminate Service Anti-Pattern:** Remove `PictureEditingUseCase` and `IPictureEditingUseCase`.
- **Atomic UseCases:** Create focused UseCases for distinct domain operations.
- **Domain Alignment:** Ensure UseCase names and structures reflect domain concepts (Transform vs Transfer).
- **Testability:** Increase unit test coverage for individual operations.

## 3. Scope of Refactoring

### 3.1. Target Class
- `Eede.Application.UseCase.Pictures.PictureEditingUseCase`

### 3.2. New UseCase Candidates (Domain-Centric)
The functionality will be split into the following UseCases:

1.  **Image Transfer (Canvas <-> Dock)**
    -   `TransferImageToCanvasUseCase` (Push): Handles extracting a region from a dock picture to be placed on the canvas.
    -   `TransferImageFromCanvasUseCase` (Pull): Handles blending the canvas content into a dock picture.

2.  **Geometric Transformation (Flip/Rotate)**
    -   `TransformImageUseCase`: Handles `PictureActions` (Rotate, Flip) on a `Picture` or a specific region.

### 3.3. Affected Consumers
- `Eede.Presentation.ViewModels.Pages.MainViewModel`

## 4. Functional Requirements
- The refactoring MUST NOT change the external behavior of the application (Refactoring only).
- `MainViewModel` must be updated to depend on the new individual interfaces instead of `IPictureEditingUseCase`.
- Existing logic for "Push," "Pull," and "Action (Rotate/Flip)" must be preserved exactly.

## 5. Non-Functional Requirements
- **Naming Convention:** UseCase names should end with `UseCase`.
- **Dependency Injection:** Register all new UseCases in `App.axaml.cs` via DI.
- **Testing:** Each new UseCase must have its own dedicated test suite.

## 6. Acceptance Criteria
- [ ] `PictureEditingUseCase.cs` and `IPictureEditingUseCase.cs` are deleted.
- [ ] New UseCase classes exist and follow the single responsibility principle.
- [ ] `MainViewModel` constructor injects the specific UseCases it needs.
- [ ] All existing tests pass (or are migrated to new test classes).
- [ ] Manual verification confirms Push/Pull and Rotate/Flip features work as before.
