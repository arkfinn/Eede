# Implementation Plan: Animation Panel & Sequencer

## Phase 1: Foundation & Domain Models (TDD)
アニメーションの定義とグリッド設定を支えるドメインモデルを構築します。

- [x] 7477a08 Task: Create `AnimationPattern` and `AnimationFrame` Domain Models (Eede.Domain)
- [x] 57e4412 Task: Implement Serialization for Animation Patterns (JSON)
- [x] 7246b9d Task: Add GridSettings to AnimationPattern model
- [ ] Task: Write Unit Tests for Sequence Manipulation (Add, Move, Delete frames)
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Foundation' (Protocol in workflow.md)

## Phase 2: Application Services & State Pattern
アニメーション編集モードのロジックと、アプリケーション全体で共有するサービスを実装します。

- [ ] Task: Define `IAnimationService` for global pattern management
- [ ] Task: Implement `AnimationEditingState` in `Eede.Application` (State Pattern)
- [ ] Task: Write Tests for `AnimationEditingState` (Coordinate to CellIndex conversion)
- [ ] Task: Implement Canvas overlay for Grid display when in Animation Mode
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Logic' (Protocol in workflow.md)

## Phase 3: ViewModels & Data Binding
UIとロジックを繋ぐViewModelを実装し、プレビューエンジンの基礎を作ります。

- [ ] Task: Create `AnimationViewModel` for the Sequencer and Preview
- [ ] Task: Implement Preview Timer logic (Frame switching based on Duration)
- [ ] Task: Create `AnimationDockViewModel` for Dock.Avalonia integration
- [ ] Task: Write Unit Tests for Preview frame calculation logic
- [ ] Task: Conductor - User Manual Verification 'Phase 3: ViewModels' (Protocol in workflow.md)

## Phase 4: UI Implementation (Avalonia & Dock)
実際のUIコンポーネントを作成し、レイアウトに組み込みます。

- [ ] Task: Create `AnimationPreviewView` (Right Sidebar)
- [ ] Task: Create `AnimationSequencerView` (Bottom Footer) with Drag & Drop support
- [ ] Task: Update Default Layout in `Eede.Presentation` to include new panels
- [ ] Task: Add "Animation Mode" toggle button to the main toolbar
- [ ] Task: Implement Grid Overlay rendering on the main canvas
- [ ] Task: Conductor - User Manual Verification 'Phase 4: UI' (Protocol in workflow.md)

## Phase 5: Integration & Polish
インポート/エクスポート機能の完成と、全体的なUXの調整を行います。

- [ ] Task: Implement Export/Import UI (File Dialogs)
- [ ] Task: Ensure Tab Switching updates Preview image but maintains Animation state
- [ ] Task: Final UI Polish (Icons, Tooltips, Spacing)
- [ ] Task: Comprehensive Integration Testing
- [ ] Task: Conductor - User Manual Verification 'Phase 5: Integration' (Protocol in workflow.md)