# Specification: Copy, Cut, and Paste Selection

## User Story
As a user, I want to copy or cut a selected area of an image and paste it, so that I can duplicate or move parts of the drawing.

## Requirements

### 1. Copy
- **Precondition:** An area may or may not be selected.
- **Action:** User executes "Copy" (via menu or shortcut Ctrl+C).
- **Behavior:**
    - If an area is selected: The image data within the selected area is copied to the clipboard.
    - **If no area is selected:** The entire working area (canvas) is copied to the clipboard.
- **Postcondition:** The original image and selection remain unchanged.

### 2. Cut
- **Precondition:** An area may or may not be selected.
- **Action:** User executes "Cut" (via menu or shortcut Ctrl+X).
- **Behavior:**
    - If an area is selected:
        - The image data within the selected area is copied to the clipboard.
        - The selected area on the canvas is erased.
    - **If no area is selected:**
        - The entire working area is copied to the clipboard.
        - The entire working area is erased.
- **Postcondition:** The selection remains active (if it existed) or the canvas is cleared.

### 3. Paste
- **Precondition:** Image data exists in the clipboard.
- **Action:** User executes "Paste" (via menu or shortcut Ctrl+V).
- **Behavior:**
    - The image data from the clipboard is pasted onto the canvas.
    - **Crucial:** The pasted image becomes the new selected area. This implies a "Floating Selection" or "Move Tool" state where the pasted content can be moved immediately.
- **Postcondition:** The application enters a state where the pasted content is selected.

## Technical Constraints
- Must utilize existing `Selection` and `Picture` domain logic.
- Must integrate with `Eede.Presentation` for input handling and rendering.
- Ensure strict separation of concerns (ViewModel handles command logic, Domain handles image manipulation).
