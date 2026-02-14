- [x] 「透過ボタン」を中央の縦置きツールバーに移動し、活性化する。
- [ ] **メモリ・パフォーマンスのさらなる最適化:**
    - [x] `FreeCurve` (ペン) ツール等での `AffectedArea` 計算の精密化（現在はストローク全体の矩形）
    - [x] `Fill` (塗りつぶし) ツールでの `AffectedArea` 計算の実装（現在はキャンバス全体）
    - [x] ゼロクローン変換（`ReadOnlySpan<byte>` 等を用いたネイティブ転送）
    - [ ] **[優先度:高] 差分Undoの全ツール適用**: `CanvasHistoryItem` (全体保存) を廃止し、全ての描画操作を `DiffHistoryItem` で管理することで、履歴によるメモリ増大を抑える。
    - [ ] **[優先度:高] 表示用 WriteableBitmap の再利用**: 描画更新のたびに `WriteableBitmap` を新生成せず、1つのインスタンスを `Lock()` して更新するように改修し、UI レスポンスを向上させる。
    - [ ] **[優先度:中] ArrayPool<byte> によるバッファ再利用**: `Picture` 生成や合成処理での `new byte[]` を排除し、LOH への負荷と GC 停止時間を削減する。
    - [ ] **[優先度:中] アニメーション編集における差分更新の導入**
    - [ ] **[優先度:低] Picture のタイル化 (Tiling)**: 巨大な単一配列をタイル単位の管理に切り替え、部分更新時のコピーコストと履歴サイズを極小化する。
    - [ ] **[優先度:低] ネイティブメモリ管理への移行**: `NativeMemory.Alloc` 等を使用して GC 管理外でバッファを保持し、GC パフォーマンスへの影響をゼロにする。
    - [ ] 超巨大画像（8192px等）を用いたストレステストとピークメモリの監視
- [ ] 「透過ボタン」のアイコンを正式なものにする
- [ ] Presentation層全体でのインターフェース依存への移行：テスト容易性の向上
- [ ] テストケースのリファクタリング。単体テストの使い方・考え方に基づく。

条件は不明、ドックパネルのズームボタンを押したら以下のエラー
System.NullReferenceException
  HResult=0x80004003
  Message=Object reference not set to an instance of an object.
  Source=Avalonia.Controls
  スタック トレース:
   場所 Avalonia.Controls.Image.MeasureOverride(Size availableSize)
   場所 Avalonia.Layout.Layoutable.MeasureCore(Size availableSize)
   場所 Avalonia.Layout.Layoutable.Measure(Size availableSize)
   場所 Avalonia.Layout.Layoutable.MeasureOverride(Size availableSize)
   場所 Avalonia.Layout.Layoutable.MeasureCore(Size availableSize)
   場所 Avalonia.Layout.Layoutable.Measure(Size availableSize)
   場所 Avalonia.Layout.Layoutable.MeasureOverride(Size availableSize)
   場所 Avalonia.Layout.Layoutable.MeasureCore(Size availableSize)
   場所 Avalonia.Layout.Layoutable.Measure(Size availableSize)
   場所 Avalonia.Layout.LayoutManager.Measure(Layoutable control)
   場所 Avalonia.Layout.LayoutManager.ExecuteLayoutPass()
   場所 Avalonia.Media.MediaContext.FireInvokeOnRenderCallbacks()
   場所 Avalonia.Media.MediaContext.RenderCore()
   場所 Avalonia.Media.MediaContext.Render()
   場所 Avalonia.Threading.DispatcherOperation.InvokeCore()
   場所 Avalonia.Threading.DispatcherOperation.Execute()
   場所 Avalonia.Threading.Dispatcher.ExecuteJob(DispatcherOperation job)
   場所 Avalonia.Threading.Dispatcher.ExecuteJobsCore(Boolean fromExplicitBackgroundProcessingCallback)
   場所 Avalonia.Threading.Dispatcher.Signaled()
   場所 Avalonia.Win32.Win32Platform.WndProc(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam)
   場所 Avalonia.Win32.Interop.UnmanagedMethods.DispatchMessage(MSG& lpmsg)
   場所 Avalonia.Win32.Win32DispatcherImpl.RunLoop(CancellationToken cancellationToken)
   場所 Avalonia.Threading.DispatcherFrame.Run(IControlledDispatcherImpl impl)
   場所 Avalonia.Threading.Dispatcher.PushFrame(DispatcherFrame frame)
   場所 Avalonia.Threading.Dispatcher.MainLoop(CancellationToken cancellationToken)
   場所 Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime.StartCore(String[] args)
   場所 Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime.Start(String[] args)
   場所 Avalonia.ClassicDesktopStyleApplicationLifetimeExtensions.StartWithClassicDesktopLifetime(AppBuilder builder, String[] args, Action`1 lifetimeBuilder)
   場所 Eede.Presentation.Desktop.Program.Main(String[] args) (F:\mydata\program\EedeWin\Eede\Eede.Presentation.Desktop\Program.cs):行 15

  この例外は、最初にこの呼び出し履歴
    [外部コード]
    Eede.Presentation.Desktop.Program.Main(string[]) (Program.cs 内) でスローされましたSystem.NullReferenceException
  HResult=0x80004003
  Message=Object reference not set to an instance of an object.
  Source=Avalonia.Controls
  スタック トレース:
   場所 Avalonia.Controls.Image.MeasureOverride(Size availableSize)
   場所 Avalonia.Layout.Layoutable.MeasureCore(Size availableSize)
   場所 Avalonia.Layout.Layoutable.Measure(Size availableSize)
   場所 Avalonia.Layout.Layoutable.MeasureOverride(Size availableSize)
   場所 Avalonia.Layout.Layoutable.MeasureCore(Size availableSize)
   場所 Avalonia.Layout.Layoutable.Measure(Size availableSize)
   場所 Avalonia.Layout.Layoutable.MeasureOverride(Size availableSize)
   場所 Avalonia.Layout.Layoutable.MeasureCore(Size availableSize)
   場所 Avalonia.Layout.Layoutable.Measure(Size availableSize)
   場所 Avalonia.Layout.LayoutManager.Measure(Layoutable control)
   場所 Avalonia.Layout.LayoutManager.ExecuteLayoutPass()
   場所 Avalonia.Media.MediaContext.FireInvokeOnRenderCallbacks()
   場所 Avalonia.Media.MediaContext.RenderCore()
   場所 Avalonia.Media.MediaContext.Render()
   場所 Avalonia.Threading.DispatcherOperation.InvokeCore()
   場所 Avalonia.Threading.DispatcherOperation.Execute()
   場所 Avalonia.Threading.Dispatcher.ExecuteJob(DispatcherOperation job)
   場所 Avalonia.Threading.Dispatcher.ExecuteJobsCore(Boolean fromExplicitBackgroundProcessingCallback)
   場所 Avalonia.Threading.Dispatcher.Signaled()
   場所 Avalonia.Win32.Win32Platform.WndProc(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam)
   場所 Avalonia.Win32.Interop.UnmanagedMethods.DispatchMessage(MSG& lpmsg)
   場所 Avalonia.Win32.Win32DispatcherImpl.RunLoop(CancellationToken cancellationToken)
   場所 Avalonia.Threading.DispatcherFrame.Run(IControlledDispatcherImpl impl)
   場所 Avalonia.Threading.Dispatcher.PushFrame(DispatcherFrame frame)
   場所 Avalonia.Threading.Dispatcher.MainLoop(CancellationToken cancellationToken)
   場所 Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime.StartCore(String[] args)
   場所 Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime.Start(String[] args)
   場所 Avalonia.ClassicDesktopStyleApplicationLifetimeExtensions.StartWithClassicDesktopLifetime(AppBuilder builder, String[] args, Action`1 lifetimeBuilder)
   場所 Eede.Presentation.Desktop.Program.Main(String[] args) (F:\mydata\program\EedeWin\Eede\Eede.Presentation.Desktop\Program.cs):行 15

  この例外は、最初にこの呼び出し履歴
    [外部コード]
    Eede.Presentation.Desktop.Program.Main(string[]) (Program.cs 内) でスローされました
