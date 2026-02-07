# Implementation Plan: ゼロクローン変換

## フェーズ 1: 現状の振る舞い固定（仕様化テスト） [DONE]
リファクタリング前の正しい挙動をテストコードで記録し、デグレードを防止します。

- [x] Task: 仕様化テストの作成
    - [x] `Picture` クラスのピクセルアクセス（PickColor, CutOut）の正確性を検証するテストを `Eede.Domain.Tests` に追加。
    - [x] 既存の `IBitmapAdapter` の動作を検証するテストを `Eede.Presentation.Tests` に追加。

## フェーズ 2: Picture ドメインモデルのリファクタリング [DONE]
`System.Drawing.Bitmap` への依存を排除し、純粋なバイト配列ベースの不変モデルに変換します。

- [x] Task: `Picture` クラスから `System.Drawing` 依存を削除
- [x] Task: `DirectImageBlender` 等の周辺クラスのクリーンアップ

## フェーズ 3: Bitmap 変換サービスの強化（ゼロクローン） [DONE]
Avalonia とドメイン間のデータ転送を最適化し、中間コピーを排除します。

- [x] Task: `AvaloniaBitmapAdapter` の実装（unsafe ポインタ、Stride考慮）
- [x] Task: RGBA/BGRA フォーマットの自動検知と変換

## フェーズ 4: 不要な依存関係の削除 [DONE]
- [x] Task: `BitmapConverter` (System.Drawing版) の削除
- [x] Task: `Eede.Application.csproj` から `System.Drawing.Common` を削除

## フェーズ 5: 最終検証
- [ ] Task: 巨大画像（4096px超）でのメモリ使用量検証（手動またはプロファイラ）
- [x] Task: 既存の全テストがパスすることを確認
