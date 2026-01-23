# Implementation Plan: Copy, Cut, and Paste Selection

## Phase 1: Core Logic & States
- [ ] **Define Clipboard Interface:**
    - Create `IClipboardService` in `Eede.Application/Services`.
    - Define methods: `Copy(Picture picture)`, `GetPicture()`, `HasPicture()`.
- [ ] **Update DraggingState:**
    - Modify `Eede.Application/Common/SelectionStates/DraggingState.cs`.
    - Add `isFloating` flag (or similar) to constructor.
    - If `isFloating` is true, skip clearing the original area in `HandlePointerLeftButtonReleased`.
- [ ] **Create FloatingSelectionState:**
    - Create `Eede.Application/Common/SelectionStates/FloatingSelectionState.cs`.
    - Represents a pasted image that hasn't been dropped yet.
    - Transitions to `DraggingState` (with `isFloating=true`) on pointer down within the image area.
    - Transitions to `NormalCursorState` (and merges image) on pointer down outside? Or just drops it? Usually, clicking outside confirms the paste.

## Phase 2: Infrastructure & Integration
- [ ] **Implement Clipboard Service:**
    - Create `AvaloniaClipboardService` in `Eede.Presentation/Services`.
    - Use `Avalonia.Application.Current.Clipboard`.
    - Handle conversion between `Picture` and `Avalonia.Media.Imaging.Bitmap` (using `PictureBitmapAdapter`).
- [ ] **Update ViewModel:**
    - In `DrawableCanvasViewModel`:
        - Inject `IClipboardService`.
        - Implement `CopyCommand`:
            - If `SelectingArea` is active: Extract picture from area.
            - Else: Extract entire picture.
            - Send to clipboard.
        - Implement `CutCommand`:
            - If `SelectingArea` is active: Extract, send to clipboard, clear area.
            - Else: Extract entire, send to clipboard, clear entire canvas.
        - Implement `PasteCommand`: Get picture from clipboard, enter `FloatingSelectionState`.
    - Wire up commands to `MainViewModel` or key bindings.

## Phase 3: Verification
- [ ] **Unit Tests:**
    - Test `DraggingState` with and without `isFloating`.
    - Test `FloatingSelectionState` transitions.
- [ ] **Manual Verification:**
    - **Copy/Paste:** Select -> Copy -> Paste -> Move pasted image -> Click outside to confirm.
    - **Cut/Paste:** Select -> Cut (area becomes transparent) -> Paste -> Move.
    - **Verify Selection:** Pasted image should be "selected" (moveable).