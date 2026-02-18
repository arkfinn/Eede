---
name: reactive-status-ux-sync
description: 外部サービスの非同期な状態（Enum）を ReactiveUI で UI 向けプロパティへ変換・同期し、既存テストを壊さずに TDD で検証可能にするパターン。
---

# Skill: reactive-status-ux-sync

## Description
外部サービス（アップデート、通信、重い計算など）が持つ多値の状態（Enum）を、ViewModel 側で UI 制御に最適化された複数のプロパティ（Boolean やメッセージ文字列）へ安全にマッピング・同期する手法です。また、大規模な ViewModel において、既存の膨大なテストコードへの影響を最小限に抑えながら新機能を導入する設計も含みます。

## Key Patterns / Strategies

### 1. Safe Dependency Injection (オプション引数による導入)
既存のテストコードが `new MainViewModel(...)` を多数行っている場合、新機能の依存関係を必須にするとビルドエラーが多発します。これを防ぐため、コンストラクタではオプション引数（デフォルト `null`）として受け取り、内部でガード節を設けます。

```csharp
public MainViewModel(
    // 既存の引数...
    IUpdateService? updateService = null, // 追加分をオプションに
    CheckUpdateUseCase? checkUpdateUseCase = null)
{
    _updateService = updateService;
    _checkUpdateUseCase = checkUpdateUseCase;

    if (_updateService != null)
    {
        // サービスが存在する場合のみバインディングを実行
        _updateService.StatusChanged
            .Select(s => s == UpdateStatus.ReadyToApply)
            .ToPropertyEx(this, x => x.IsUpdateReady);
    }
}
```

### 2. Reactive Status Mapping (UI 向けプロパティ変換)
XAML 側で複雑な `ValueConverter` を使う代わりに、ViewModel 側で `ToPropertyEx` (ObservableAsPropertyHelper) を用いて、一つの状態 Enum から複数の UI 制御用プロパティを生成します。

```csharp
// UpdateStatus (Enum) -> UI 用プロパティ
this.WhenAnyValue(x => x.UpdateStatus)
    .Select(s => s == UpdateStatus.Downloading)
    .ToPropertyEx(this, x => x.IsUpdateDownloading);

this.WhenAnyValue(x => x.UpdateStatus)
    .Select(s => s switch {
        UpdateStatus.Checking => "確認中...",
        UpdateStatus.ReadyToApply => "準備完了",
        _ => ""
    })
    .ToPropertyEx(this, x => x.UpdateMessage);
```

### 3. State-Based Testing with BehaviorSubject
モック作成時、`BehaviorSubject` を用いて「サービスの状態が変化した際の ViewModel の挙動」を擬似的に再現し、テストを記述します。

```csharp
[Test]
public void StatusSyncTest()
{
    var statusSubject = new BehaviorSubject<UpdateStatus>(UpdateStatus.Idle);
    _updateServiceMock.SetupGet(x => x.StatusChanged).Returns(statusSubject);
    
    var vm = new MainViewModel(..., _updateServiceMock.Object);
    Assert.That(vm.IsUpdateReady, Is.False);

    // サービス側で状態が変化したことをエミュレート
    statusSubject.OnNext(UpdateStatus.ReadyToApply);

    // ViewModel 側のプロパティが同期されているか検証
    Assert.That(vm.IsUpdateReady, Is.True);
}
```

## Notes
- NUnit を使用する場合、テストクラス内の `IDisposable`（`BehaviorSubject` 等）は必ず `[TearDown]` で `Dispose` してください (NUnit1032 対策)。
- `_ = InitializeAsync()` のような「Fire and Forget」な初期化を行う場合は、テストコード側で呼び出し回数のクリア (`Invocations.Clear()`) や適切な待機が必要になることに注意してください。
