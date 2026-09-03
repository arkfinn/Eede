# 🌐 fix(wasm): Web版セッション復元時にドックエリアの画像が空になる不具合の修正

## 🎯 概要 (Overview)
本PRは、WebAssembly（ブラウザ版）において「前回の作業を復元」「前回の作業を再開」を実行した際、ドックエリアに配置された画像が透明な空画像（`Picture.CreateEmpty`）になってしまう不具合を根本解決します。

### 🔍 真因と解決内容
1. **未編集ドキュメントの画像ペイロード永続化規律の配備**:
   - デスクトップ版の「未編集ファイル（`Edited == false`）はディスク上の実ファイルから再ロードできるため画像ペイロードをスキップする」規律がWeb版にも適用されていたため、ブラウザ再読込後にローカルファイル不在でドックエリアが空画像になっていた問題を特定。
   - Web環境（`OperatingSystem.IsBrowser()`）、ブラウザ仮想URI（`blob:` 等）、および未保存画像（`FilePath.IsEmpty()`）においては、未編集であっても必ず全ドキュメントの画像ペイロード（`ImagePayloadRef`）を生成して IndexedDB / スナップショットへ保存するよう条件分岐（`ShouldSaveDocumentPayload`）を整備。
2. **ブラウザ仮想URIからの元ファイル名復元と劣化防止**:
   - スナップショット作成時、ブラウザの仮想URI（`blob:http://...`）から `AvaloniaFileStorage.TryGetOriginalFileName` を用いて元ファイル名（`hero.png` 等）を抽出し、`OriginalFilePath` に優先保存。
   - セッション復元時のタブ表示名やダウンロード保存名が無意味な GUID blob 文字列に劣化することを防止。
3. **復元画像の静的キャッシュ再登録**:
   - `RestoreDocumentsAsync` において、復元されたドキュメントの画像バイト列を `AvaloniaFileStorage.RegisterCacheData` により静的キャッシュに再登録。復元直後であってもパレット抽出やストリーム再利用が即座に動作するよう保証。

---

## 🧪 テスト・検証結果 (Verification)
- **テストスイート (`dotnet test`)**: **824 件 ALL PASS**（0 fail / 100% 成功）
- **WebAssembly プロジェクトビルド (`Eede.Presentation.Browser`)**: **0 警告・0 エラー**
- **新規結合テスト**:
  - `SessionRecoveryE2ETests.CaptureSession_WhenDocumentIsVirtualUriOrUneditedWebFile_SavesImagePayloadAndRestoresNonEmptyPicture`:
    - 仮想URI（`blob:`）の未編集画像をスナップショット保存・再起動復元した際に、画像ペイロードが正しく保持され、ドックエリアに元の実画像（青色ピクセル）が完全に復元されることを実証。
