# Implementation Plan: PictureEditingUseCase Refactoring (Legacy Safe Mode)

## Phase 0: Safety Net Construction (Legacy Code Protection) [checkpoint: 9faa6b5]
**Goal:** Lock down the current behavior of `PictureEditingUseCase` before touching any code.

- [x] Task: Analyze existing test coverage for `PictureEditingUseCase`. [No existing tests found]
- [x] Task: Create "Characterization Tests" (Golden Master) for `PictureEditingUseCase`. [9faa6b5]
    - [x] Capture inputs/outputs for `ExecuteAction` (Rotate/Flip). [9faa6b5]
    - [x] Capture inputs/outputs for `PushToCanvas`. [9faa6b5]
    - [x] Capture inputs/outputs for `PullFromCanvas`. [9faa6b5]
    - [x] Ensure tests pass and cover edge cases (null areas, boundary sizes). [9faa6b5]
- [x] Task: Conductor - User Manual Verification 'Phase 0: Safety Net' (Protocol in workflow.md) [9faa6b5]

## Phase 1: New UseCase Implementation (TDD) [checkpoint: db99ce1]
**Goal:** Implement new atomic UseCases using strict TDD, keeping the old code untouched for now.

- [x] Task: Create `TransformImageUseCase` (Rotate/Flip) [46ede10]
    - [x] Write failing tests using `PictureEditingUseCase`'s test cases as a reference. [46ede10]
    - [x] Implement logic (copying from old UseCase or Domain). [46ede10]
- [x] Task: Create `TransferImageToCanvasUseCase` (Push) [c9b5094]
    - [x] Write failing tests. [c9b5094]
    - [x] Implement logic. [c9b5094]
- [x] Task: Create `TransferImageFromCanvasUseCase` (Pull) [66ce09f]
    - [x] Write failing tests. [66ce09f]
    - [x] Implement logic. [66ce09f]
- [x] Task: Conductor - User Manual Verification 'Phase 1: New UseCases' (Protocol in workflow.md) [db99ce1]

## Phase 2: Parallel Integration & Strangler Fig Pattern [checkpoint: ddcce9b]
**Goal:** Introduce new UseCases into `MainViewModel` alongside the old one, verifying behavior incrementally.

- [x] Task: Register new UseCases in `App.axaml.cs` (DI). [3a186dc]
- [x] Task: Inject new UseCases into `MainViewModel` (keep `PictureEditingUseCase` for now). [c43bdb5]
- [x] Task: Switch `MainViewModel.ExecutePictureAction` to use `TransformImageUseCase`. [a4ba43a]
    - [x] Verify functionality (Manual & Auto). [a4ba43a]
- [x] Task: Switch `MainViewModel.OnPushToDrawArea` to use `TransferImageToCanvasUseCase`. [e153f3d]
    - [x] Verify functionality. [e153f3d]
- [x] Task: Switch `MainViewModel.OnPullFromDrawArea` to use `TransferImageFromCanvasUseCase`. [b21449d]
    - [x] Verify functionality. [b21449d]
- [x] Task: Conductor - User Manual Verification 'Phase 2: Integration' (Protocol in workflow.md) [ddcce9b]

## Phase 3: Cleanup
**Goal:** Remove the dead code once the migration is fully verified.

- [ ] Task: Remove `PictureEditingUseCase` dependency from `MainViewModel`.
- [ ] Task: Delete `PictureEditingUseCase.cs` and `IPictureEditingUseCase.cs`.
- [ ] Task: Delete the Characterization Tests (or migrate valuable scenarios to new UseCase tests).
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Cleanup' (Protocol in workflow.md)
