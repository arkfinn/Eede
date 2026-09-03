---
activation: model_decision
description: "Eede Session Recovery architecture, atomic swap, IndexedDB fallback, and unedited document rehydration"
---
# 🔄 Eede セッション復元 (Session Recovery) アーキテクチャ規約

## 1. ストレージ永続化とフェイルセーフ (Storage Persistence)
- **デスクトップ: アトミックスワップ手順とフォールバック (LocalFileSessionStorage)**:
  - 1. `temp/` へメタデータ（`session.json`）および画像ペイロードを一括書き込み ➔ 2. `staging/` へアトミックリネーム ➔ 3. 既存 `current/` を `backup/` へ退避 ➔ 4. `staging/` を `current/` へリネーム ➔ 5. `backup/` を削除。
  - ステップ4失敗時は `catch` で `backup/` を `current/` へ自動ロールバックし、健全な直前セッションの永続性を物理保証すること。
  - `payloadRef` 外部キーはパストラバーサル文字（`..`, `/`, `\`）をホワイトリスト検証で水際防御すること。
- **Web版 (WASM): IndexedDB 大容量非同期永続化と「優雅な縮退」規約 (BrowserIndexedDbSessionStorage)**:
  - ブラウザの **IndexedDB** をバックエンドに採用し、非同期かつバイナリ（`byte[]`）を直接保存することで、容量無制限かつ描画カクつき 0ms を実現する。
  - 万が一容量超過（`QuotaExceededError`）が発生した場合でも、画像ペイロードを間引いてメタデータ（タブ構成・名前・サイズ等）だけを死守する「優雅な縮退（Graceful Degradation）」を配備し、アプリのクラッシュや描画中断を結して起こさないこと。

## 2. セッション復旧・再開 UX (Session Resume & Rehydration)
- **未編集既存ファイルのディスク再ロード規約 (Unedited Document Rehydration)**:
  - セッションスナップショット保存時、デスクトップ環境で実在する物理ローカルファイル（`Edited == false` かつ `OriginalFilePath != null` かつローカルファイル）のみ容量節約のため画像ペイロード（`ImagePayloadRef`）をスキップする。
  - 復元側（`RestoreDocumentsAsync`）において、`doc.Snapshot.ImagePayloadRef is null` かつ `!filePath.IsEmpty()` の場合、空白画像にするのではなく、必ず `_pictureFileIO.LoadAsync(filePath)` を呼び出してディスク上の実画像をリロードすること。
- **Web版 (WASM) および仮想URIにおける画像ペイロード完全永続化規律 (Web/Virtual Payload Defense)**:
  - Web版（`OperatingSystem.IsBrowser()`）またはブラウザ仮想URI（`blob:` 等）や新規未保存画像（`FilePath.IsEmpty()`）では、物理ディスク上の永続ファイルが存在しないため、未編集（`Edited == false`）であっても必ず全ドキュメントの画像ペイロード（`ImagePayloadRef`）を IndexedDB / スナップショットへ保存すること。
  - セッション保存時、仮想URI（`blob:` 等）から元ファイル名（`AvaloniaFileStorage.TryGetOriginalFileName`）を復元して `OriginalFilePath` に記録し、復元時のタブ表示名やダウンロード保存名の劣化を防ぐこと。
  - 復元時（`RestoreDocumentsAsync`）、復元された画像を静的キャッシュ（`AvaloniaFileStorage.RegisterCacheData`）へ再登録し、復元直後のパレット抽出やストリーム再利用を即座に可能にすること。
- **複合タブコンテナの階層スナップショット設計 (PaletteTabSnapshot)**:
  - 「一時パレット」と「ファイルパレット」が共存する複合コンテナにおいて、パレットのセッション保存は全タブのファイルパス、Dirty状態、CustomTitle、IsClosable、SourceIdentity、256色配列を `PaletteTabSnapshot` として完全記録し、復元時に構造ごと同期復元すること。
- **正常終了と異常終了の直交分離とセッション再開 UX (WelcomeView Session Resume)**:
  - クリーン終了マーカー（`clean_exit.marker` / `clean_exit` キー）は「セッションの物理破棄」ではなく「前回の終了が異常終了だったか否かのメタ情報」としてのみ機能させる。
  - セッションデータ自体はクリーン終了時も温存し、ウェルカム画面（`WelcomeView`）の「開始」メニュー先頭に「前回の作業を再開」としてシームレスに提示する。

## 3. コーディネーション & 状態同期 (Coordination & Sync)
- **2フェーズ・アイドル保存と直列先行キャンセル (SessionRecoveryCoordinator)**:
  - Phase 1 (スナップショット抽出): UI スレッド上で不変な `SessionSnapshot` を瞬時に抽出（< 1ms）。
  - Phase 2 (Taskpool オフロード): 重い画像エンコードと保存はワーカースレッド上で非同期実行。
  - `SemaphoreSlim(1, 1)` による直列排他制御と先行 `CancellationTokenSource` キャンセルを協調させ、常に最新状態のみをコミットする。
- **Pull/Push およびキャンバス操作契機における Dirty 通知の網羅的伝播原則**:
  - ドックからキャンバスへの領域転送（Pull）や書き戻し（Push）、Undo/Redo、画像変形など、キャンバス状態が変化するすべての契機で即座に `SessionRecoveryCoordinator.NotifyDirty()` を発火させること。
  - 復元側（`RestorePullState`）でも、`DrawingSessionViewModel.Sync` ➔ `DrawableCanvasViewModel.SyncWithSession` ➔ `SetPictureToDrawArea` の順序で強制同期を適用すること。
