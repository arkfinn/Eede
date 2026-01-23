---
name: refactor-five-lines-strategy
description: |
  リファクタリングを実行: 複雑なコードを「5行ルール」に基づき分割・構造化する。
  ポリモーフィズム導入によるif文排除や、重複コードの統合を行う。
  コードが読みづらい時や、大規模な改修前の整理に使用。
version: 1.0.0
---

# [Five-Lines Refactoring Strategy]

**Context:** Apply to complex methods, legacy code, or high cognitive
load logic.

## Refactoring Rules

適用すべき3つの機械的ルール。コードの形状（Shape）に基づき適用する。

### 1. The Five-Lines Rule (Method Extraction)

- **Trigger**: メソッド本体が空行・括弧を除き **5行** を超えている。
- **Action**:
  1.  処理の塊（段落）を識別し、プライベートメソッドへ抽出する。
  2.  `if`, `for`, `while` の中身は必ず1行（メソッド呼び出し）にする。
  3.  元のメソッドは、抽出したメソッドを呼ぶだけの「オーケストラ」にする。

### 2. Replace Type Code with Classes (Polymorphism)

- **Trigger**: `switch`文、または `enum`/型コードによる `if-else` 連鎖がある。
- **Action**:
  1.  分岐条件（型）をインターフェース化する。
  2.  各分岐のロジックを実装クラスのメソッドへ移動する。
  3.  呼び出し元をインターフェース経由の実行に変更する。

### 3. Unify Similar Code (Harmonize & Extract)

- **Trigger**: 構造は似ているが、値や一部の呼び出しが異なるコードブロックがある。
- **Action**:
  1.  **Harmonize**: 差異（定数・関数）を引数化し、コードをテキストレベルで完全に一致させる。
  2.  **Extract**: 一致したブロックをメソッド化する。

## Example

**Before (Mixed & Long)**

```python
def handle(val, mode):
    if mode == "A":  # Type Check
        d = db.load(val)
        # ... complex logic A (>5 lines) ...
    elif mode == "B":
        d = api.fetch(val)
        # ... complex logic B ...

```

**After (Structured & Polymorphic)**

```python
# 1. Classes replace if-else
# 2. Methods < 5 lines
class HandlerA(IHandler):
    def handle(self, val):
        data = self._load(val)
        self._process(data)

    def _load(self, v): return db.load(v)
    def _process(self, d): ... # logic A extracted

# Usage
handler_factory(mode).handle(val)

```

## When to Use

以下の兆候（Smell）がある場合に発動：

1. **長さ**: 5行を超えるメソッド。
2. **分岐**: 型やモードによる `switch/if` 分岐。
3. **重複**: コピー＆ペーストされた類似コード。
4. **説明**: コメントがないと意図が不明なブロック（即抽出対象）。
