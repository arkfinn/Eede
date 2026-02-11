---
name: dotnet-nullable-migration-safety
description: .NETプロジェクトにおけるNull許容参照型の段階的導入と、Moq等のテストコードで発生する特有の型推論エラー（CS8625等）を解消するためのパターン。
---

# Skill: .NET Nullable Migration Safety

## Description
レガシーな.NETコードベースを `Nullable Reference Types` (NRT) へ移行する際、単なる警告抑制（`!`）に頼らず、ランタイムの安全性を高めつつ、複雑なライブラリ（Moq等）との競合を解消するための戦略です。

## Key Patterns / Strategies

### 1. 階層的な移行プロセス (Inside-Out)
依存関係の最内部（Domain/SharedKernel）から開始し、外側（Application -> Presentation）へと段階的に `<Nullable>enable</Nullable>` を広げることで、下位層のNull安全性の恩恵を上位層で受けられます。

### 2. Moq における Nullable Value Type のイベント発火 (CS8625/CS8604 回避)
`Mock.Raise` メソッドの引数は `params object[]` であり、構造体のNull許容型（`PictureArea?` 等）に `null` リテラルを直接渡すと、ボクシングの過程でコンパイラが型を特定できずエラーになります。

**回避策:** 明示的に型を指定したローカル変数を使用します。

```csharp
// Error: CS8625 (Cannot convert null literal to non-nullable reference type)
_mock.Raise(x => x.Drew += null!, arg1, null); 

// Success: ローカル変数経由で渡す
PictureArea? nullArea = null;
_mock.Raise(x => x.Drew += null!, arg1, nullArea);
```

### 3. 初期化不全 (CS8618) の本質的解決
ViewModel等で `default!` を多用せず、コンストラクタによる初期化や、Null Object パターンを優先します。Avalonia等のDIやプレビューで引数なしコンストラクタが必要な場合は、連鎖コンストラクタを利用して一貫性を保ちます。

```csharp
public class MainViewModel : ViewModelBase
{
    // 必須依存性はコンストラクタで保証
    public MainViewModel(IDependency dep) => Dep = dep;

    // デザイン時/ツール用の引数なしコンストラクタ
    [Obsolete("Only for design time", true)]
    public MainViewModel() : this(new NullDependency()) { }
}
```

### 4. プロジェクト境界の厳守
テストコードを誤ってメインプロジェクト内に配置すると、テスト用ライブラリ（Moq, NUnit等）への参照がないため大量のビルドエラーが発生します。ファイル移動時は物理パスだけでなく、名前空間とプロジェクト参照の不整合を即座に確認します。
