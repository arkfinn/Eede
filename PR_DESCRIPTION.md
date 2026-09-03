# 🎨 feat(palettes): 画像からのパレット自動インポート & fix(wasm): Web版ファイルオープン例外の根本解決

## 🎯 概要 (Overview)
1. **画像からのカラーパレット自動抽出・新タブ展開**:
   - 256色以下のインデックスカラーPNG（PLTEチャンク）、ARV画像、およびダイレクトカラーPNGからユニーク色を高速スキャンし、新しいパレットタブとして自動展開。
   - インポートしたタブの誤操作による元画像破壊を防ぐライフサイクル隔離規約（`SourceIdentity` による重複検知と安全なクローズハンドリング）を完備。
2. **Web版（WASM / Avalonia.Browser）でのファイルオープン完全修復**:
   - WebAssembly 環境（`blob:https://...` 等の非ファイルURI）において、`Uri.LocalPath` への無防備なアクセスが `InvalidOperationException` を誘発してファイルが開けなくなっていた致命的バグを根本解決。
   - **DDD（ドメイン駆動設計）に基づく概念の抽出**: 泥臭い文字列判定を排除し、純粋ドメインモデル `FileClassification`（判定ポリシー）および `FileIdentity`（値オブジェクト）を新設。

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
- **テストスイート (`dotnet test`)**: **813 件 ALL PASS**（0 fail / 100% 成功）
- **新規テスト**:
  - `FileClassificationTests`: 形式判定・拡張子抽出・クエリパラメータ除去（19件）
  - `FileIdentityTests`: ローカルパスおよびブラウザ仮想URIの属性検証
  - `AvaloniaFileStorageTests`: 非ファイルURI（`blob:`）における `LocalPath` 例外防衛とストリーム解決
  - `MainViewModelTests`: WebAssembly 環境を模した非ファイルURIでの画像オープン結合テスト
