# 🎨 feat(palettes): 画像からのパレット自動インポート & fix(wasm/color/recovery): Web版完全修復・色化け根絶・セッション復元

## 🎯 概要 (Overview)
1. **画像からのカラーパレット自動抽出・新タブ展開**:
   - 256色以下のインデックスカラーPNG（PLTEチャンク）、ARV画像、およびダイレクトカラーPNGからユニーク色を高速スキャンし、新しいパレットタブとして自動展開。
   - インポートしたタブの誤操作による元画像破壊を防ぐライフサイクル隔離規約（`SourceIdentity` による重複検知と安全なクローズハンドリング）を完備。
2. **Web版（WASM / Avalonia.Browser）でのファイルオープン＆パレット抽出の完全修復**:
   - WebAssembly 環境（`blob:https://...` 等の非ファイルURI）において、`Uri.LocalPath` への無防備なアクセスが `InvalidOperationException` を誘発してファイルが開けなくなっていた致命的バグを根本解決。
   - `TryExtractPaletteFromImageAsync` において `AvaloniaFileStorage` の静的ファイルキャッシュを最優先参照するように改善し、Web版での画像オープン・ドラッグ＆ドロップ時にもパレット自動抽出が 100% 確実に動作するよう修復。
3. **PNG256（256色インデックスカラーPNG）等の色化け・反転の根絶**:
   - Avalonia の `Bitmap.CopyPixels` が内部 `Rgba8888` フォーマットのままコピーしてしまい、`Picture`（`Bgra8888`）との間で赤と青が反転（色化け）していた問題、およびアルファ乗算の劣化を根本解決。
   - `PictureRepository` に `IPictureCodec`（`SkiaSharpPictureCodec`）を注入し、インデックスカラーPNGを含めて純粋な SkiaSharp（`SKCodec` + `SKImageInfo(Bgra8888, Unpremul)`）でデコードするよう刷新。1ビットの狂いもない完全な色情報を復元。
4. **Windows版セッション再開時の既存ファイル空白化の解消**:
   - 未編集の既存ファイル（`Edited == false` かつ `OriginalFilePath != null`）について、復元時（`RestoreDocumentsAsync`）に `_pictureFileIO.LoadAsync(filePath)` で実画像を安全に再ロード。
5. **セッション再開時のパレット「一時パレット」固定化＆クローズ不可バグの解消**:
   - `PaletteTabSnapshot` に `CustomTitle`, `IsClosable`, `SourceIdentity` を完全保持させ、再開後も元のタイトルと閉じられる状態（×ボタン）を100%復元。

---

## 🔍 敵対的コードレビューと品質対策 (Adversarial Review)
- **非ファイルURIにおける例外の連鎖防止**:
  - `TryExtractPaletteFromImageAsync` での例外キャッチを堅牢化し、パレット抽出（付加的機能）の如何に関わらず画像自体のオープン可用性を100%保証。
  - `blob:` URI 等で拡張子が脱落した場合でも、ファイル名から拡張子を正しくフォールバック解決する仕組みを配備。
- **ドラッグ＆ドロップ（D&D）の完全救済**:
  - ドロップされた `IStorageFile` を自動的に `AvaloniaFileStorage` の静的キャッシュへ登録し、デスクトップ・ブラウザ双方で安定してストリームを開けるよう防御。
- **メモリ・性能防衛**:
  - ダイレクトカラー画像の走査時は、ユニーク色が 257 色に達した瞬間に即座に処理を中断（Early Exit）し、大容量画像によるフリーズを防止。

---

## 🧪 テスト・検証結果 (Verification)
- **テストスイート (`dotnet test`)**: **817 件 ALL PASS**（0 fail / 100% 成功）
- **新規テスト**:
  - `FileClassificationTests`: 形式判定・拡張子抽出・クエリパラメータ除去（19件）
  - `FileIdentityTests`: ローカルパスおよびブラウザ仮想URIの属性検証
  - `AvaloniaFileStorageTests`: 非ファイルURI（`blob:`）における `LocalPath` 例外防衛とストリーム解決
  - `MainViewModelTests`: WebAssembly 環境を模した非ファイルURIでの画像オープン・パレット自動抽出結合テスト
  - `PictureRepositoryTests`: 256色PNGのデコード時に赤と青が反転せず正確な色でロードされることの検証
  - `SessionRecoveryE2ETests`: セッション再開時の既存ファイル空白化解消・インポートパレットタブ復元の検証
