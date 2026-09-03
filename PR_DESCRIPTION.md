# 🎨 feat(palettes): 画像からのパレット自動インポート & fix(wasm/color/recovery): Web版完全修復・色化け根絶・セッション復元

## 🎯 概要 (Overview)
1. **画像からのカラーパレット自動抽出・新タブ展開**:
   - 256色以下のインデックスカラーPNG（PLTEチャンク）、ARV画像、およびダイレクトカラーPNGからユニーク色を高速スキャンし、新しいパレットタブとして自動展開。
   - インポートしたタブの誤操作による元画像破壊を防ぐライフサイクル隔離規約（`SourceIdentity` による重複検知と安全なクローズハンドリング）を完備。
2. **Web版（WASM / Avalonia.Browser）でのファイルオープン＆パレット抽出の完全修復**:
   - **ブラウザでのストリーム再オープン不可の完全克服**: 画像読込（`PictureRepository`）とパレット抽出（`TryExtractPaletteFromImageAsync`）で2回ストリームを開こうとした際、ブラウザの JS Interop 制限により2回目が失敗していた問題を `AvaloniaFileStorage.StaticDataCache`（バイト配列インメモリキャッシュ）により完全解決。
   - **GUID blob URI による拡張子・ファイル名欠落の解決**: ブラウザ環境で URI が `blob:.../guid` となりファイル名や `.png` 拡張子が消滅していた問題を、`StaticNameCache` による元ファイル名復元およびストリーム先頭マジックナンバー（`0x89 0x50 0x4E 0x47`）自動検知により解決。
   - **チャンク化ストリームの完全読み切り**: WebAssembly の JSStream による部分読み込みで PNG チャンク検証が不正終了していた問題を `PngPaletteReader` の `TryReadExactly` 化により解決。
3. **PNG256（256色インデックスカラーPNG）等の色化け・反転の根絶**:
   - Avalonia の `Bitmap.CopyPixels` が内部 `Rgba8888` フォーマットのままコピーしてしまい、`Picture`（`Bgra8888`）との間で赤と青が反転（色化け）していた問題、およびアルファ乗算の劣化を根本解決。
   - `PictureRepository` に `IPictureCodec`（`SkiaSharpPictureCodec`）を注入し、インデックスカラーPNGを含めて純粋な SkiaSharp（`SKCodec` + `SKImageInfo(Bgra8888, Unpremul)`）でデコードするよう刷新。1ビットの狂いもない完全な色情報を復元。
4. **Windows版セッション再開時の既存ファイル空白化の解消**:
   - 未編集の既存ファイル（`Edited == false` かつ `OriginalFilePath != null`）について、復元時（`RestoreDocumentsAsync`）に `_pictureFileIO.LoadAsync(filePath)` で実画像を安全に再ロード。
5. **セッション再開時のパレット「一時パレット」固定化＆クローズ不可バグの解消**:
   - `PaletteTabSnapshot` に `CustomTitle`, `IsClosable`, `SourceIdentity` を完全保持させ、再開後も元のタイトルと閉じられる状態（×ボタン）を100%復元。

---

## 🧪 テスト・検証結果 (Verification)
- **テストスイート (`dotnet test`)**: **818 件 ALL PASS**（0 fail / 100% 成功）
- **新規テスト**:
  - `MainViewModelTests.LoadPictureCommand_WhenOpeningGuidBlobUri_WithSingleStreamRead_AutoExtractsPaletteAndRestoresOriginalFileName`:
    - ブラウザ特有の「拡張子なし GUID blob URI」および「2回目の OpenReadAsync 禁止（例外スロー）」環境を完全再現し、パレット抽出・タブ追加・タイトル復元が正常動作することを検証。
  - `PictureRepositoryTests`: 256色PNGのデコード時に赤と青が反転せず正確な色でロードされることの検証
