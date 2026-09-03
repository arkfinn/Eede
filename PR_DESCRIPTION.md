# 🌐 fix(wasm): Web版パレット自動抽出の完全修復 & feat(wasm): IndexedDBによるWeb版セッション復元・容量超過防護

## 🎯 概要 (Overview)
本PRは、WebAssembly（ブラウザ版）において画像オープン時にパレットタブが生成されなかった問題の完全修復と、ブラウザリロード（F5）やタブ再開時にも前回の作業状態を安全に復元可能にする IndexedDB セッション永続化機構の追加を行います。

### 1. Web版でのパレット自動抽出の完全修復
- **ブラウザでのストリーム再オープン不可の克服**:
  - `PictureRepository` が画像ロード時にストリームを読み切った後、`TryExtractPaletteFromImageAsync` で再度ストレージから開こうとした際に、WebAssembly（`Avalonia.Browser`）の JS Interop 制約（2回目のオープン拒絶/破棄例外）によってパレット抽出が失敗していた問題を解決。
  - `AvaloniaFileStorage.StaticDataCache`（バイト配列インメモリキャッシュ）を新設し、初回ロード時の全バイトをメモリ保持して 2 回目以降は即座に `MemoryStream` を複製して返すよう改善。
- **GUID blob URI による拡張子・元ファイル名欠落の解決**:
  - ブラウザ環境で URI が `blob:.../3e9b16f3-...`（拡張子のないGUID）となり、従来の `.png` 拡張子判定を素通りしていた問題を解決。
  - `StaticNameCache` による元のファイル名復元に加え、ストリーム先頭 8 バイトの PNG シグネチャ（`0x89 0x50 0x4E 0x47`）自動検知ロジックを追加し、拡張子のない仮想 URI でも 100% 確実に PNG パレット抽出を実行。
- **チャンク化ストリームの完全読み切り**:
  - WebAssembly の JSStream による部分読み込みで PNG チャンク検証が途中で不正終了していた問題を、`PngPaletteReader` の `TryReadExactly` 化により解決。

### 2. Web版（WASM）セッション復元対応（IndexedDB による大容量・非同期・容量オーバー防護）
- **IndexedDB による大容量非同期ストア**:
  - Web版でブラウザをリロード（F5）したりタブを閉じた際に、仮想インメモリファイルシステムが初期化されて「前回の作業を再開」が消えていた制約を克服。
  - `BrowserIndexedDbSessionStorage` および `window.eedeSessionDb` を新設し、ブラウザの **IndexedDB** にセッションメタデータと画像ペイロードを安全に永続化。
- **容量オーバー（QuotaExceededError）時の優雅な縮退（Graceful Degradation）**:
  - ディスク逼迫やクォータ超過時には画像ペイロードを間引き、タブ構成・名前・キャンバスサイズなどのメタデータを死守して保存。
  - ストレージ例外を内部で安全に吸収し、ユーザーが描画中のペン操作やUIを決してクラッシュ・停止させない多層防御を配備。

---

## 🧪 テスト・検証結果 (Verification)
- **テストスイート (`dotnet test`)**: **823 件 ALL PASS**（0 fail / 100% 成功）
- **ソリューション全体ビルド**: **0 警告・0 エラー**（`Eede.Presentation.Browser` 含む）
- **新規テスト**:
  - `BrowserIndexedDbSessionStorageTests` (5件):
    - IndexedDB ストレージの保存・復元
    - クリーン終了マーカー追跡（正常終了とクラッシュ復旧の分離）
    - 容量オーバー（`QuotaExceededError`）時の安全縮退
    - セッション消去
  - `MainViewModelTests.LoadPictureCommand_WhenOpeningGuidBlobUri_WithSingleStreamRead_AutoExtractsPaletteAndRestoresOriginalFileName`:
    - ブラウザ特有の「拡張子なし GUID blob URI」および「2回目の OpenReadAsync 禁止（例外スロー）」環境を完全再現し、パレット抽出・タブ追加・タイトル復元が正常動作することを実証。
