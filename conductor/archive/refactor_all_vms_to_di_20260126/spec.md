# Track Specification: 全 ViewModel の DI 移行と DrawingSession 管理の洗練

## 1. 概要
MainViewModel に残っているサブ ViewModel 群の生成ロジックを排除し、全ての ViewModel を DI 管理下に移行します。また、アプリの核となる「描画セッション状態」を IDrawingSessionProvider として独立させ、状態と操作の分離を徹底します。

## 2. 背景・目的
- MainViewModel のコンストラクタ内でサブ ViewModel が 
ew されており、依然として「初期化の手順」という知識を持ってしまっている。
- DrawingSession の初期化が MainViewModel にハードコードされており、柔軟性に欠ける。
- DDD の原則に基づき、「状態（Provider/Service）」と「操作（UseCase）」を明確に分離し、テスト容易性を最大化する。

## 3. ゴール (Acceptance Criteria)
- [ ] 全てのサブ ViewModel (DrawableCanvasViewModel, AnimationViewModel, DrawingSessionViewModel, PaletteContainerViewModel) が MainViewModel にコンストラクタ注入されている。
- [ ] MainViewModel 内の ViewModel 生成における 
ew が完全に排除されている。
- [ ] IDrawingSessionProvider が導入され、現在の DrawingSession（Undo/Redo 履歴含む）がサービス層で管理されている。
- [ ] App.axaml.cs で全ての依存関係が自動的に解決されるように設定されている。
- [ ] 既存の機能（描画、Undo/Redo、アニメーション、パレット）がリファクタリング前と変わらず動作することをテストで確認。

## 4. 機能要件
### 4.1. DrawingSession 管理の改善
- IDrawingSessionProvider を Application 層に作成し、現在のセッション状態を保持・提供する。
- 描画アクションによるセッション更新は既存の UseCase を通じて Provider を更新する流れにする。

### 4.2. 各 ViewModel の DI 対応
- 各サブ ViewModel のコンストラクタを、インターフェース注入を前提とした形に整理。
- MainViewModel のコンストラクタ引数をサブ ViewModel 群を受け取る形に変更。

### 4.3. DI コンテナの設定 (App.axaml.cs)
- IServiceCollection への全 ViewModel および Provider の登録を追加。

## 5. 非機能要件
- **保守性:** 依存関係がコンストラクタで一目瞭然であること。
- **堅牢性:** ReactiveUI のバインドや TestScheduler を用いたテストが引き続き機能すること。

## 6. アウトオブスコープ
- 各 ViewModel 内の表示ロジック（UI部品の並び替え等）の変更。
- 新しい編集機能の追加。
