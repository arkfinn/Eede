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

## Phase 1: New UseCase Implementation (TDD)
**Goal:** Implement new atomic UseCases using strict TDD, keeping the old code untouched for now.

- [ ] Task: Create `TransformImageUseCase` (Rotate/Flip)
    - [ ] Write failing tests using `PictureEditingUseCase`'s test cases as a reference.
    - [ ] Implement logic (copying from old UseCase or Domain).
- [ ] Task: Create `TransferImageToCanvasUseCase` (Pull)
    - [ ] Write failing tests.
    - [ ] Implement logic.
- [ ] Task: Create `TransferImageFromCanvasUseCase` (Push)
    - [ ] Write failing tests.
    - [ ] Implement logic.
- [ ] Task: Conductor - User Manual Verification 'Phase 1: New UseCases' (Protocol in workflow.md)

## Phase 2: Parallel Integration & Strangler Fig Pattern
**Goal:** Introduce new UseCases into `MainViewModel` alongside the old one, verifying behavior incrementally.

- [ ] Task: Register new UseCases in `App.axaml.cs` (DI).
- [ ] Task: Inject new UseCases into `MainViewModel` (keep `PictureEditingUseCase` for now).
- [ ] Task: Switch `MainViewModel.ExecutePictureAction` to use `TransformImageUseCase`.
    - [ ] Verify functionality (Manual & Auto).
- [ ] Task: Switch `MainViewModel.OnPushToDrawArea` to use `TransferImageFromCanvasUseCase`.
    - [ ] Verify functionality.
- [ ] Task: Switch `MainViewModel.OnPullFromDrawArea` to use `TransferImageToCanvasUseCase`.
    - [ ] Verify functionality.
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Integration' (Protocol in workflow.md)

## Phase 3: Cleanup
**Goal:** Remove the dead code once the migration is fully verified.

- [ ] Task: Remove `PictureEditingUseCase` dependency from `MainViewModel`.
- [ ] Task: Delete `PictureEditingUseCase.cs` and `IPictureEditingUseCase.cs`.
- [ ] Task: Delete the Characterization Tests (or migrate valuable scenarios to new UseCase tests).
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Cleanup' (Protocol in workflow.md)
