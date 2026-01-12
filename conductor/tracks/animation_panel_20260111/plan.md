# Implementation Plan: Animation Panel & Sequencer

## Phase 1: Foundation & Domain Models (TDD) [checkpoint: 73d4af3]
アニメーションの定義とグリッド設定を支えるドメインモデルを構築します。

- [x] 7477a08 Task: Create `AnimationPattern` and `AnimationFrame` Domain Models (Eede.Domain)
- [x] 57e4412 Task: Implement Serialization for Animation Patterns (JSON)
- [x] 7246b9d Task: Add GridSettings to AnimationPattern model
- [x] b8f8cee Task: Write Unit Tests for Sequence Manipulation (Add, Move, Delete frames)
- [x] 73d4af3 Task: Conductor - User Manual Verification 'Phase 1: Foundation' (Protocol in workflow.md)

## Phase 2: Application Services & State Pattern [checkpoint: bd007a0]
アニメーション編集モードのロジックと、アプリケーション全体で共有するサービスを実装します。

- [x] f99d37b Task: Define `IAnimationService` for global pattern management
- [x] 9fb4bb9 Task: Implement `AnimationEditingState` in `Eede.Application` (State Pattern)
- [x] 9fb4bb9 Task: Write Tests for `AnimationEditingState` (Coordinate to CellIndex conversion)
- [x] a439e77 Task: Implement Canvas overlay for Grid display when in Animation Mode
- [x] bd007a0 Task: Conductor - User Manual Verification 'Phase 2: Logic' (Protocol in workflow.md)

## Phase 3: ViewModels & Data Binding [checkpoint: 9c676ed]
UIとロジックを繋ぐViewModelを実装し、プレビューエンジンの基礎を作ります。

- [x] 2d54e4c Task: Create `AnimationViewModel` for the Sequencer and Preview
- [x] 625fbdb Task: Implement Preview Timer logic (Frame switching based on Duration)
- [x] eef9027 Task: Create `AnimationDockViewModel` for Dock.Avalonia integration
- [x] 111ea05 Task: Write Unit Tests for Preview frame calculation logic
- [x] 9c676ed Task: Conductor - User Manual Verification 'Phase 3: ViewModels' (Protocol in workflow.md)

## Phase 4: UI Implementation (Avalonia & Dock) [checkpoint: cef7495]
実際のUIコンポーネントを作成し、レイアウトに組み込みます。

- [x] cef7495 Task: Create `AnimationPreviewView` (Right Sidebar)
- [x] 4c1dad5 Task: Create `AnimationSequencerView` (Right Sidebar / Vertical)
- [x] cef7495 Task: Update Layout in `Eede.Presentation` to include new panels
- [x] cef7495 Task: Add "Animation Mode" toggle button to the Animation Panel
- [x] cef7495 Task: Implement Grid Overlay rendering on the dock pictures
- [x] cef7495 Task: Conductor - User Manual Verification 'Phase 4: UI' (Protocol in workflow.md)

## Phase 5: Integration & Polish
インポート/エクスポート機能の完成と、全体的なUXの調整を行います。

- [x] 8465b7c Task: Implement Export/Import UI (File Dialogs)
- [x] 28eb7dc Task: Ensure Tab Switching updates Preview image but maintains Animation state
- [x] a3e3278 Task: Final UI Polish (Icons, Tooltips, Spacing)
- [ ] Task: Comprehensive Integration Testing
- [ ] Task: Conductor - User Manual Verification 'Phase 5: Integration' (Protocol in workflow.md)