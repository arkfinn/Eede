# Specification: ゼロクローン変換（ReadOnlySpan<byte> 等を用いたネイティブ転送）の導入

## 1. 背景
現在の画像データ転送（`Picture` ドメインモデルと Avalonia `Bitmap` の相互変換）において、`byte[]` の複製（`CloneImage()`）や `Marshal.Copy` が多用されており、巨大な画像（4096px以上）を扱う際のメモリ消費とパフォーマンスが課題となっている。

## 2. 目標
- **パフォーマンス向上**: 巨大な画像の転送や描画更新における遅延を最小化する。
- **メモリ効率の改善**: 画像転送時の中間バッファによるメモリピークを削減し、GC 負荷を軽減する。
- **技術スタックの純化**: `System.Drawing` への依存を排除し、Avalonia ネイティブかつゼロコピーな実装へ移行する。

## 3. 機能要件
### 3.1 Picture ドメインモデルの強化
- `Picture` 内部バッファに対して、`ReadOnlySpan<byte>` による高速アクセスを提供し、外部（Infrastructure層）からの直接書き込みを可能にする `PictureSpanAction` (delegate) を導入する。

### 3.2 高速変換サービスの実装
- Avalonia の `WriteableBitmap.Lock()` で得られる生ポインタを `Span<byte>` でラップし、`Picture.WriteTo(Span<byte>)` を通じてゼロコピー転送を実現する。

## 4. 受入条件
- [ ] `BitmapConverter` (System.Drawing依存) が完全に削除されていること。
- [ ] 既存の描画ツール、Undo/Redo がこれまで通り正しく動作すること。
- [ ] 4096px 以上の巨大画像においても、転送時のメモリ不足が発生しないこと。
