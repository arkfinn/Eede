# Eede セキュリティ脆弱性・堅牢性総合調査レポート (Comprehensive Security Audit Report)

- **監査対象**: Eede (`Eede.Domain`, `Eede.Application`, `Eede.Infrastructure`, `Eede.Presentation`, `Eede.Presentation.Desktop`, `Eede.Presentation.Browser`)
- **技術スタック**: C# / .NET 10.0 (`net10.0` / `net10.0-windows`), Avalonia UI 12.1.1, ReactiveUI 24.0.0, SkiaSharp 3.119.4, WebAssembly (WASM)
- **監査方針**: 物理契約（Design by Contract）・メモリ安全性・境界値検証・並行処理整合性に基づく深層静的解析および防御的改善案の策定

---

## 1. エグゼクティブサマリー & 検出リスク一覧

Eede はドメイン層の不変性（`Picture`, `Palette` 等）や `LocalFileSessionStorage` における一時ファイル経由のアトミック置換・正規表現による `payloadRef` 検証など、堅牢性を意識した設計が随所に導入されている。一方で、**32bit 符号付き整数演算のオーバーフローによる `unsafe` ポインタ境界外アクセス（Critical）**、**ファイル名（Basename）キャッシュ衝突による別ディレクトリ同名ファイルのサイレント上書き（High）**、**PNG パレットチャンク長未検証による 2GB 即時割り当て DoS（High）**、および **`SessionRecoveryCoordinator` のキャンセル連鎖における `FlushAsync` 早期完了レース（High）** が確認された。

| ID | Severity | 対象ベクトル / 脆弱性タイトル | CWE | 該当箇所 (File & Lines) | CIA影響 |
| :--- | :---: | :--- | :--- | :--- | :---: |
| **SEC-01** | **Critical** | 画像寸法の32bit整数オーバーフローと `unsafe` ポインタ境界外読み書き | CWE-190, CWE-787, CWE-125 | `Picture.cs:L15-L27`, `SkiaSharpPictureCodec.cs:L113-L149`, `AntiAliasFilter.cs:L34-L91`, `AvaloniaBitmapAdapter.cs:L34-L80` | I / A (C) |
| **SEC-02** | **High** | `AvaloniaFileStorage` の Basename キャッシュ衝突による意図しないファイル上書き | CWE-706, CWE-22 | `AvaloniaFileStorage.cs:L12-L61, L81-L91, L130-L136`, `PictureRepository.cs:L39-L47, L128-L138` | I |
| **SEC-03** | **High** | `PngPaletteReader` におけるチャンク長 (`length`) 未検証による巨大配列割り当て DoS | CWE-770, CWE-400, CWE-190 | `PngPaletteReader.cs:L50, L60-L64, L98-L106`, `ImagePaletteExtractor.cs:L55-L66` | A |
| **SEC-04** | **High** | `SessionRecoveryCoordinator` の多重キャンセル時における `FlushAsync` 早期完了競合 | CWE-362 | `SessionRecoveryCoordinator.cs:L244-L293`, `MainViewModel.cs:L1145-L1153` | I / A |
| **SEC-05** | **Medium** | セッション/パレット自動復元時の未検証パス・UNCパス自動接続 (SMB Hash Leak) | CWE-22, CWE-400 | `MainViewModel.cs:L293-L315, L1432-L1452`, `PaletteContainerViewModel.cs:L52-L74` | C / A |
| **SEC-06** | **Medium** | デシリアライズ時のドメイン契約例外の未捕捉・`NaN` 透過・設定値未検証クラッシュ | CWE-20, CWE-248 | `LocalFileSessionStorage.cs:L216-L224`, `DocumentSnapshot.cs:L25-L28`, `AppSettings.cs:L7-L12`, `MainViewModel.cs:L516-L521, L563-L567` | A |
| **SEC-07** | **Medium** | `ArvFileReader` の切り詰めストリーム・不正パレット長に対する負数 `ReadBytes` 例外 | CWE-20, CWE-125 | `ArvFileReader.cs:L34-L47, L64, L82, L88-L93, L128-L176` | A |
| **SEC-08** | **Medium** | リサンプラー座標計算の整数オーバーフロー・Undo履歴無制限・`Bitmap` 非解放リーク | CWE-190, CWE-400, CWE-401 | `NearestNeighborResampler.cs:L18, L33, L38`, `DrawingSession.cs:L22-L23, L146-L186`, `AvaloniaClipboard.cs:L64, L82, L105, L131`, `MainViewModel.cs:L825-L841` | A |
| **SEC-09** | **Medium** | UIスレッド同期 `Invoke` デッドロックリスクおよび静的 `Dictionary` の非同期競合 | CWE-362, CWE-833 | `SessionRecoveryCoordinator.cs:L62, L71-L82`, `MainViewModel.cs:L1219-L1224`, `AvaloniaFileStorage.cs:L12-L14` | A |
| **SEC-10** | **Medium** | WASM 境界における CSP 欠如・外部スクリプト (`gtag.js`) 動的読込・引数注入 | CWE-829, CWE-353, CWE-1022 | `wwwroot/index.html:L4-L12`, `wwwroot/main.js:L213`, `WebExternalBrowserLauncher.cs:L55-L56` | C / I |
| **SEC-11** | **Low** | 共有 Temp フォールバック時の予測可能パスと設定・パレットの非アトミック書き込み | CWE-377, CWE-59 | `LocalFileSessionStorage.cs:L48-L57`, `App.axaml.cs:L186-L192`, `JsonSettingsRepository.cs:L48-L49` | I / A |
| **SEC-12** | **Low** | スタックトレース・ローカルパスのコンソール/UI露出および無同意テレメトリ送信 | CWE-209, CWE-532, CWE-359 | `Browser/Program.cs:L29-L40`, `wwwroot/main.js:L243, L257`, `MainViewModel.cs:L1412`, `UpdateManagerWrapper.cs:L14` | C |

---

## 2. 調査ベクトル別の深層検証 (Deep Inspection)

### ベクトル 1: ファイル I/O・パス走査・アトミック書き込み (CWE-22, CWE-377, CWE-59, CWE-706)

#### 現行の防御策
- `LocalFileSessionStorage.cs` (`L29-L30`, `L275-L294`) は `SafePayloadRefRegex` (`^[a-zA-Z0-9_\-\.]+$`) と `".."` 拒否、`Path.GetFullPath` によるベースディレクトリ前方一致検証を実装しており、`SavePayloadAsync` / `LoadPayloadAsync` 単体でのパストラバーサルは適切に防御されている。また `SaveSnapshotAsync` (`L136-L162`) は GUID 付き `.tmp` ファイルへの書き込み後に `File.Move(tempFilePath, sessionFilePath, overwrite: true)` でアトミック置換を行っている。

#### 検出された脆弱性・欠陥
1. **[SEC-02 / High] `AvaloniaFileStorage` の Basename キャッシュ衝突による別ファイルのサイレント上書き (CWE-706)**
   - **事実 (Code Reality)**: `AvaloniaFileStorage.cs` の `CacheFile` (`L33-L38`) と `RegisterCacheData` (`L53-L60`) は、フルパスだけでなくファイル名単体（`file.Name`, `"/" + name`, `"\\" + name`）をプロセス共有の `StaticFileCache` / `StaticDataCache` に登録する。さらに `TryOpenWriteStreamStaticAsync(string path)` (`L130-L136`) と `TryOpenReadStreamStaticAsync(string path)` (`L81-L91`) は、指定パスがキャッシュに存在しない場合、`Path.GetFileName(path)` でフォールバック検索を行う。`PictureRepository.cs` (`L39-L41`, `L129-L131`) はローカルの `FileStream` を開く前に必ずこれらの静的メソッドを優先呼び出しする。
   - **影響シナリオ**: ユーザーが FilePicker で `C:\ProjectA\sprite.png` を開く（または保存する）と、`StaticFileCache["sprite.png"]` に ProjectA の `IStorageFile` が登録される。その後、「最近使ったファイル」やセッション復元（FilePicker を介さない経路）から別ディレクトリの `C:\ProjectB\sprite.png` を開いて上書き保存すると、`C:\ProjectB\sprite.png` のフルパスは `StaticFileCache` に存在しないため、フォールバックにより `StaticFileCache["sprite.png"]`（＝**`C:\ProjectA\sprite.png`**）の書き込みストリームが返却され、**ProjectA 側のファイルがサイレントに破壊・上書きされる**。読み込み時も同様に別ディレクトリの同名キャッシュが誤読込される。
2. **[SEC-05 / Medium] セッション復元・パレット復元時の未検証パス自動アクセスと `DocumentId` 未検証 (CWE-22, CWE-400)**
   - **事実**: `MainViewModel.RestoreDocumentsAsync` (`L1432-L1440`) は `doc.Snapshot.ImagePayloadRef is null` の場合に `doc.Snapshot.OriginalFilePath` を自動的に `PictureRepository.LoadAsync` で読み込む。また `PaletteContainerViewModel.LoadSessionAsync` (`L59-L61`) は起動時に `palettes_session.json` 内の全パスを自動オープンする。さらに `MainViewModel.cs` (`L1449`) は復元時に `vm.Id = doc.Snapshot.DocumentId` を無検証で代入しており、`DocumentId` に `../` 等が含まれる場合、次回自動保存時 (`L1262`: `$"doc_{picVm.Id}.png"`) に `ValidatePayloadRef` が `ArgumentException` をスローし続け、以降のセッション保存が恒久的に機能停止する。
   - **影響シナリオ**: Windows 環境において `session.json` や `palettes_session.json` に UNC パス（`\\attacker.example\share\a.png`）やデバイスパス（`\\.\GLOBALROOT\...`）、NTFS 代替データストリーム（`file.png:stream`）が混入した場合、起動時にユーザー操作なしで外部 SMB サーバへ NTLMv2 認証ハッシュが送信（SMB Relay / Hash Capture）されるか、ネットワークタイムアウトによる UI フリーズが発生する。
3. **[SEC-11 / Low] 共有 Temp フォールバックと設定ファイル類の非アトミック書き込み (CWE-377, CWE-59)**
   - **事実**: `LocalFileSessionStorage.cs` (`L48-L57`) と `App.axaml.cs` (`L186-L192`) は `Environment.SpecialFolder.ApplicationData` が空の場合、全ユーザー共有の `Path.GetTempPath()` 配下に予測可能なパス（`Eede\recovery\pid_{ProcessId}` や `Eede\session_recovery`）を生成する。また、`JsonSettingsRepository.cs` (`L48-L49`), `PaletteSessionRepository.cs` (`L28`), `PaletteRepository.cs` (`L28`), `PictureRepository.cs` (`L162`) は既存ファイルを直接 `File.Create` / `File.WriteAllTextAsync` で切り詰めており、書き込み中のクラッシュや電源断でファイルが 0 バイト破損する。

---

### ベクトル 2: デシリアライズ・パース・フォーマット処理 (CWE-502, CWE-20, CWE-770)

#### 現行の防御策
- JSON デシリアライズはすべて `System.Text.Json.JsonSerializer` の静的型指定（`SessionSnapshot`, `AppSettings`, `PaletteDto`, `AnimationPattern`）を使用しており、`BinaryFormatter` 等の危険な多態的型解決（CWE-502）は使用されていない。
- `ActPaletteReader.cs` (`L20`: `stream.Length is not 768 and not 772`) や `GplPaletteReader.cs` / `JascPalPaletteReader.cs` / `HexPaletteReader.cs` は最大 256 色でループを打ち切る防御が入っている。

#### 検出された脆弱性・欠陥
1. **[SEC-03 / High] `PngPaletteReader` におけるチャンク長未検証による 2GB 即時割り当て DoS (CWE-770, CWE-400, CWE-190)**
   - **事実**: `PngPaletteReader.cs` (`L50`) は PNG チャンクヘッダから `uint length = BinaryPrimitives.ReadUInt32BigEndian(chunkHeader[..4]);` を読み取る。
     - `IHDR` 処理 (`L60-L64`): `if (length < 13) return null;` のみで上限を検証せず、`byte[] ihdrData = new byte[length];` を実行する（PNG 仕様上 `IHDR` は厳密に 13 バイト固定）。
     - `tRNS` 処理 (`L98-L106`): 上限チェックが一切なく、`byte[] trnsData = new byte[length];` を実行する（インデックスカラーの `tRNS` は最大 256 バイト）。
     - `PLTE` 処理 (`L79`): `length > 256 * 3` の上限チェックがあるが、`length == 0` のチェックがない。
   - **影響シナリオ**: 約 40 バイトの細工された PNG ファイルで `IHDR` または `tRNS` の `length` に `0x7FFFFFC7`（約 2.14GB）を指定すると、ストリームの実データ長に関わらず即座に 2GB の Large Object Heap (LOH) 割り当てが試行され `OutOfMemoryException` でプロセスがクラッシュする。また `length >= 0x80000000` の場合、C# の配列サイズ上限を超えて `OverflowException` がスローされるが、呼び出し元の `ImagePaletteExtractor.ExtractAsync` (`L63`) は `IOException` と `InvalidDataException` しか捕捉しないため、例外が未処理のまま伝播する。
2. **[SEC-07 / Medium] `ArvFileReader` におけるストリーム末尾・不正パレット長・幅アライメント未検証 (CWE-20, CWE-125)**
   - **事実**: `ArvFileReader.cs` において：
     - `ReadHeaderFlags` (`L36-L37`): `reader.ReadBytes(6)` はストリームが短い場合に 6 バイト未満の配列を返すが、要素数を確認せずに `arvHeaderFlags[0]` (`L64`) や `arvHeaderFlags[1]` (`L82`) にアクセスするため `IndexOutOfRangeException` が発生する。
     - `ReadPaletteBytes` (`L90-L92`): `byte paletteLength = reader.ReadByte(); ... return reader.ReadBytes(paletteLength - 2);` において、ファイル内の `paletteLength` が `0` または `1` の場合、`ReadBytes(-2)` / `ReadBytes(-1)` が呼ばれ `ArgumentOutOfRangeException` がスローされる。
     - `CreateFrameBuffer` (`L151`, `L160-L176`): `int bytesPerRow = width / 8;` で `width <= 0` や `width % 8 != 0` の検証がなく、`width < 8` の場合に `bytesPerRow == 0` となり不正なフレーム処理となる。
3. **[SEC-06 / Medium] デシリアライズ時のドメイン例外漏出・`float.NaN` 透過・`AppSettings` 未検証クラッシュ (CWE-20, CWE-248)**
   - **事実**:
     - `LocalFileSessionStorage.LoadLatestSnapshotAsync` (`L216-L224`) は `catch (JsonException)` のみを捕捉して破損セッションをクリーンアップする。しかし `JsonSerializer.DeserializeAsync<SessionSnapshot>` の実行中にドメインコンストラクタ（`SessionSnapshot`, `DocumentSnapshot`, `PictureSize`, `PaletteSnapshot`）がスローする `ArgumentException` / `ArgumentOutOfRangeException` / `ArgumentNullException` は `JsonException` にラップされないため、破損した `session.json` から起動時クラッシュを引き起こす。
     - `DocumentSnapshot.cs` (`L25-L28`) の `if (magnification <= 0f)` は IEEE 754 の仕様上 `float.NaN <= 0f` が `false` となるため、`NaN` や `PositiveInfinity` を透過させる。
     - `AppSettings.cs` (`L7-L12`) と `MainViewModel.cs` (`L516-L521`, `L563-L567`): `settings.json`（またはブラウザ `localStorage`）の `GridWidth` / `GridHeight` に `0` や負数が設定されていた場合、`LoadSettingsAsync` が `MinCursorWidth = settings.GridWidth` を設定し、`WhenAnyValue` の **`async void` 購読ラムダ (`L564-L566`) 内で `new PictureSize(w, h)` が `ArgumentException` をスローしてアプリケーションが即死する**。
     - `AnimationViewModel.cs` (`L249`, `L333-L337`): `ImportCommand` で JSON サイズ上限がなく、`AnimationPattern` の `CellSize.Width + Padding` が整数オーバーフローまたは `0` 以下になる入力を防いでいない。

---

### ベクトル 3 & 4: メモリ安全性・ポインタ演算・DoS・整数オーバーフロー (CWE-190, CWE-787, CWE-125, CWE-400, CWE-401)

#### 現行の防御策
- `Picture` は `ImmutableArray<byte>` を内部保持し、`DrawingBuffer` を介した Copy-On-Write 方式で不変性を維持している。また `Picture.Create` (`L15-L23`) はデータ長の一致チェックを行っている。

#### 検出された脆弱性・欠陥
1. **[SEC-01 / Critical] 32bit 符号付き整数オーバーフローによるバッファ過小割り当てと `unsafe` ポインタ境界外読み書き (CWE-190, CWE-787, CWE-125)**
   - **事実 (Code Reality)**:
     1. `PictureSize.cs` (`L13-L21`) は `width < 0 || height < 0` のみを検査し、最大寸法（例: `16384 x 16384`）の上限を持たない。
     2. `Picture.cs` (`L15-L27`) の `Create` / `CreateEmpty` は **非 `checked` の 32bit `int` 乗算** を行っている：
        ```csharp
        int stride = size.Width * COLOR_32BIT; // 4
        if (stride * size.Height != imageData.Length) throw new ArgumentException(...);
        // CreateEmpty: new byte[size.Width * COLOR_32BIT * size.Height]
        ```
        ここで `(long)Width * Height * 4` が $2^{32}$ の倍数またはその近傍となる寸法（例: `Width = 32768, Height = 32768` のとき `stride = 131072`, `stride * Height = 0`；`Width = 32768, Height = 32769` のとき `stride * Height = 131072`）を与えると、**わずか 0 バイトまたは 128KB の `byte[]` で `Width = 32768, Height = 32769` の `Picture` インスタンスが合法的に生成され、不変条件チェックを通過する**。
     3. 到達経路：
        - `SessionRecoverer.RestoreSessionAsync` (`L58-L61`): `docSnapshot.ImagePayloadRef is null` の場合、`try/catch` ブロックの外で `Picture.CreateEmpty(docSnapshot.Size)` を直接呼び出す。
        - `NewPictureWindowViewModel.cs` (`L11-L36`) / `ScalingDialogViewModel.cs` (`L126-L202`): 寸法上限がない。
        - `SkiaSharpPictureCodec.DecodeFromPng` (`L107-L151`): Decompression Bomb（巨大 IHDR 寸法）に対する上限検証がなく、`byte[] pixels = new byte[width * height * 4];` (`L128`) が 32bit オーバーフローする。さらに直後の `unsafe` ブロック (`L132-L149`) で `byte* srcRow = src + y * srcRowBytes;` / `byte* dstRow = dst + y * dstRowBytes;` を実行するため、過小割り当てされたヒープ配列 `pixels` の境界を越えて **ヒープバッファオーバーフロー（Out-of-Bounds Write, CWE-787）** または `y * dstRowBytes` の符号反転による負方向メモリ破壊を引き起こす。
     4. 下流の `unsafe` ポインタ演算への波及：
        - `AntiAliasFilter.cs` (`L34-L91`, `L201-L210`) および `ArgbAntiAliasStrategy.cs` (`L23-L35`): `fixed (byte* srcPtr = sourceData)` / `fixed (byte* dstPtr = resultData)` でポインタを固定し、`sourceData.Length` と `(long)height * stride` の整合性を検証しないまま `y * stride + x * 4` で生ポインタ読み書きを行う。
        - `AvaloniaBitmapAdapter.cs` (`L34`, `L40-L80`, `L124-L168`): `new byte[width * 4 * height]` および `srcBase + y * srcStride` / `destBase + y * destStride` がすべて 32bit `int` 演算で実装されており、同様に `unsafe` 境界外アクセスが発生する。
2. **[SEC-08 / Medium] リサンプラーのオーバーフロー、Undo履歴・静的キャッシュのメモリ無制限増加、および `Bitmap` リーク (CWE-190, CWE-400, CWE-401)**
   - **事実**:
     - `NearestNeighborResampler.cs` (`L18`, `L33`, `L38`): `new byte[newSize.Width * newSize.Height * 4]` に加え、`xMap[x] = (x * sourceWidth) / newWidth;` (`L33`) および `int srcY = (y * sourceHeight) / newHeight;` (`L38`) が `x * sourceWidth > int.MaxValue`（例: `46341` 以上）で負数にオーバーフローし、`IndexOutOfRangeException` を引き起こす。
     - `DrawingSession.cs` (`L22-L23`, `L146-L186`): `UndoStack` / `RedoStack` に履歴件数または総バイト数の上限がなく、編集操作ごとに完全な `Picture` バッファが蓄積され続ける。
     - `AvaloniaFileStorage.cs` (`L12-L14`, `L50-L61`, `L97-L115`): `StaticFileCache` / `StaticDataCache` / `StaticNameCache` に Eviction（上限・LRU・ドキュメントクローズ時解放）がなく、保存・読込のたびに `byte[]` がプロセス終了まで残留する。
     - `AvaloniaClipboard.cs` (`L64`, `L82`, `L105`, `L131`): `new Bitmap(ms)`, `new Bitmap(stream)`、および `HasPictureAsync()` 内の `var bitmap = await TryGetBitmapAsync();` (`L131`) が一切 `Dispose` (`using`) されていない。特に `MainView.axaml.cs` (`L348-L378`) は `Window.Activated`, `PointerEntered`, `GotFocus` のたびに `CheckClipboardAsync()` -> `HasPictureAsync()` を呼び出すため、クリップボードに画像がある状態でマウスがウィンドウに出入りするたびにアンマネージド `Bitmap` がリークする。
     - `MainViewModel.CleanupDockPicture` (`L825-L841`): タブを閉じる際に `vm.PremultipliedBitmap` を `Dispose()` していない。

---

### ベクトル 5: WebAssembly (WASM) & ブラウザサンドボックス境界 (CWE-79, CWE-345, CWE-829, CWE-1022)

#### 現行の防御策
- `main.js` (`L39-L176`) の IndexedDB ラッパーや Blob ダウンロード処理は DOM への `innerHTML` 注入を行っておらず、ファイル名も `a.download = fileName` プロパティ代入 (`L23`) のため、ファイル名由来の直接的な DOM XSS は発生しない。また `WebExternalBrowserLauncher.cs` (`L20-L24`) は `Uri.UriSchemeHttp` / `Uri.UriSchemeHttps` のみを許可し `javascript:` スキームを遮断している。

#### 検出された脆弱性・欠陥
1. **[SEC-10 / Medium] CSP (Content Security Policy) 欠如、外部スクリプト動的読込、URLクエリ引数注入、`window.open` の `noopener` 欠如**
   - **事実**:
     - `Eede.Presentation.Browser/wwwroot/index.html` (`L4-L12`) および `docs/app/index.html`: `<meta http-equiv="Content-Security-Policy" ...>` が設定されておらず、`https://www.googletagmanager.com/gtag/js?id=G-G5FWMCKBSD` を SRI (Subresource Integrity) なしでロードし、インラインスクリプトを実行している。
     - `wwwroot/main.js` (`L213`): `.withApplicationArgumentsFromQuery()` が有効化されており、攻撃者が細工した URL クエリパラメータがそのまま WASM ランタイムおよび `Program.Main(string[] args)` に渡される。
     - `WebExternalBrowserLauncher.cs` (`L55-L56`): `[JSImport("globalThis.open")] internal static partial void Open(string url, string target = "_blank");` で第3引数 `"noopener,noreferrer"` を指定していない（CWE-1022）。
     - `BrowserIndexedDbSessionStorage.cs` (`L47-L61`, `L104-L128`): `LocalFileSessionStorage` と異なり `ValidatePayloadRef(payloadRef)` によるキー検証を行っていない。また `Eede.Presentation.Browser.csproj` (`L15`) で `<WasmAllowUndefinedSymbols>true</WasmAllowUndefinedSymbols>` が有効化されている。

---

### ベクトル 6: 並行性・競合状態・ライフサイクル管理 (CWE-362, CWE-833)

#### 現行の防御策
- `SessionRecoveryCoordinator.cs` は `_stateLock` による `_activeOperation` 管理、`SemaphoreSlim(Math.Max(1, Environment.ProcessorCount / 2))` による並行 PNG エンコード数制限 (`L165`)、および `Interlocked.Exchange` による `_isDisposed` ガード (`L301`) を備えている。

#### 検出された脆弱性・欠陥
1. **[SEC-04 / High] `SessionRecoveryCoordinator.ExecuteSaveAsync` の多重キャンセル時における `FlushAsync` 早期完了レース (CWE-362)**
   - **事実 (Code Reality)**: `SessionRecoveryCoordinator.cs` (`L244-L293`) において、先行する保存処理 `Op1` が後続の `Op2` によってキャンセルされた際、`catch (OperationCanceledException)` ブロック (`L255-L292`) で `nextTask = GetNextOperationTask()`（＝`Op2` の `currentTcs.Task`）を取得するが、**`L285` の `currentTcs.TrySetResult()` は `nextTask` の完了を待たずに即座に実行される**（特に `Op1` が `NotifyDirty` 由来の `throwOnError: false` の場合、`await nextTask` すら行われない）：
     ```csharp
     // SessionRecoveryCoordinator.cs:L274-L286
     if (throwOnError && nextTask != null)
     {
         try { await nextTask.ConfigureAwait(false); }
         catch (Exception ex) { currentTcs.TrySetException(ex); return; }
     }
     currentTcs.TrySetResult(); // <-- throwOnError==false の場合、nextTask 完了前に即座に完了状態になる！
     ```
   - **影響シナリオ**:
     1. アプリ終了時 (`MainViewModel.RequestCloseAsync` `L1151`) に `FlushAsync()` (`Op1`, `throwOnError: true`) が呼ばれる。
     2. ほぼ同時にデバウンスタイマー等から `Op2` (`throwOnError: false`) が発火し、`Op1` をキャンセルする。`Op1` は `Op2` の `currentTcs.Task` を `await` する。
     3. 直後に別の変更で `Op3` が発火し、`Op2` をキャンセルする。`Op2` は `throwOnError == false` のため、`Op3` の完了を待たずに `L285` で自身の `currentTcs.TrySetResult()` を呼ぶ。
     4. その結果、`Op3` がまだ PNG エンコードやディスク書き込みの最中であるにもかかわらず、**`Op1` (`FlushAsync`) が完了して復帰し、プロセスが終了して書き込み中のリカバリデータが失われる**。
2. **[SEC-09 / Medium] `_captureFactory` のスレッドプール実行と `Dispatcher.UIThread.Invoke` デッドロックリスク・静的 `Dictionary` 競合 (CWE-362, CWE-833)**
   - **事実**:
     - `SessionRecoveryCoordinator.cs` (`L62`, `L71-L82`) は既定スケジューラに `TaskPoolScheduler.Default` を使用しており、`.Throttle(_debounceDuration, _scheduler)` のコールバック内で `_captureFactory()` (`L81`) をスレッドプール上から呼び出す（`L77` のコメント「UIスレッド/呼び出し側コンテキスト同期」と乖離している）。
     - これに対し `MainViewModel.cs` (`L1223`) は `_ => Dispatcher.UIThread.Invoke(CaptureSession)` で同期ブロッキング呼び出しを行っている。UI スレッド側がロック待機や `FlushAsync` の同期待機に入った瞬間にデバウンスタイマーが発火すると、UI スレッドと ThreadPool スレッドの間で古典的なデッドロック（CWE-833）が発生し得る。
     - `AvaloniaFileStorage.cs` (`L12-L14`) の `StaticFileCache`, `StaticDataCache`, `StaticNameCache` は非スレッドセーフな `Dictionary<K, V>` であり、UI スレッドと非同期 I/O タスクから排他制御なしで読み書きされている。

---

### ベクトル 7 & 8: サプライチェーン・依存関係・情報漏洩・例外処理 (CWE-1395, CWE-209, CWE-532, CWE-359)

1. **[SEC-12 / Low] 例外メッセージ・ローカルパスの露出、無同意テレメトリ、およびアップデータ検証**
   - **事実**:
     - `Eede.Presentation.Browser/Program.cs` (`L29-L40`) と `wwwroot/main.js` (`L243`, `L257`) は、未処理例外の完全なスタックトレースをブラウザコンソールおよび `#loading-text` DOM 要素に直接出力する。
     - `MainViewModel.cs` (`L663`, `L863`, `L1215`, `L1412`) や `AbstractImageFile.cs` (`L82`, `L112`) は `Console.WriteLine` に生の `Exception`（ローカルファイルパスやユーザー名を含む）を出力し、`L1412` では `RecoveryPromptMessage = $"セッションの復元に失敗しました: {ex.Message}";` として内部例外メッセージを直接 UI に表示する。
     - `wwwroot/index.html` (`L5-L12`) はユーザー同意（Opt-in）やプライバシー通知なしに Google Analytics (`G-G5FWMCKBSD`) へアクセス情報を送信する。
     - `UpdateManagerWrapper.cs` (`L14`) は `new GithubSource(githubUrl, null, false)` で GitHub Releases を参照する。HTTPS 通信には保護されているものの、リリースアセットの Authenticode 署名検証フック等は明示されていない。また `Eede.Presentation.csproj` (`L55-L57`) に一時ファイル名（`Region.axaml.cs~RF1e241b7c.TMP` 等）の残骸が含まれている。

---

## 3. 防御的修正方針 (Defensive Remediation Strategy)

### 3.1 [SEC-01 対策] `PictureSize` / `Picture` / `SkiaSharpPictureCodec` における 64bit 境界検証と `checked` 演算の徹底
ドメインの物理契約として最大寸法（例: `MaxDimension = 16384`）および 64bit 演算による総バイト数検証を `PictureSize` と `Picture` に導入し、`unsafe` ポインタ演算のオフセットを `(long)y * stride` に統一する。

```csharp
// Eede.Domain/SharedKernel/PictureSize.cs & Eede.Domain/ImageEditing/Picture.cs
public const int MaxDimension = 16384; // 最大 16384x16384 (1GB)

public static Picture Create(PictureSize size, byte[] imageData)
{
    ArgumentNullException.ThrowIfNull(imageData);
    if (size.Width > MaxDimension || size.Height > MaxDimension)
        throw new ArgumentOutOfRangeException(nameof(size), $"Dimensions exceed maximum ({MaxDimension}).");

    long expectedBytes = (long)size.Width * COLOR_32BIT * size.Height;
    if (expectedBytes > Array.MaxLength || expectedBytes != imageData.Length)
        throw new ArgumentException("ImageData length does not match Width * Height * 4.");
    return new Picture(size, ImmutableCollectionsMarshal.AsImmutableArray(imageData));
}
```
また `AntiAliasFilter.cs`, `ArgbAntiAliasStrategy.cs`, `AvaloniaBitmapAdapter.cs`, `SkiaSharpPictureCodec.cs` のすべての `unsafe` ブロック先頭で `if ((long)height * stride > sourceData.Length) throw new ArgumentException();` を強制し、ポインタオフセット計算を `src + (nint)y * srcStride` に変更する。`NearestNeighborResampler.cs` (`L33`, `L38`) の座標計算も `(int)(((long)x * sourceWidth) / newWidth)` に修正する。

### 3.2 [SEC-02 対策] `AvaloniaFileStorage` からの Basename フォールバック完全廃止と `ConcurrentDictionary` 化
ファイル名単体（`Path.GetFileName(path)`）でのキャッシュ登録・検索を廃止し、正規化されたフルパスまたはブラウザ仮想 URI（`browser://...`）のみをキーとする `ConcurrentDictionary`（上限付き）に置き換える。

```csharp
// Eede.Presentation/Common/Adapters/AvaloniaFileStorage.cs
private static readonly ConcurrentDictionary<string, IStorageFile> StaticFileCache = new(StringComparer.Ordinal);
// CacheFile 内の file.Name / "/" + name 登録、および TryOpenWriteStreamStaticAsync / TryOpenReadStreamStaticAsync 内の
// Path.GetFileName(path) フォールバックを削除し、完全一致キーのみで解決する。
```

### 3.3 [SEC-03 / SEC-07 対策] `PngPaletteReader` と `ArvFileReader` のチャンク・ヘッダ境界値検証
```csharp
// Eede.Infrastructure/Palettes/PngPaletteReader.cs
if (chunkType.SequenceEqual("IHDR"u8))
{
    if (length != 13) return null; // PNG IHDR は 13 bytes 固定
    byte[] ihdrData = new byte[13];
    ...
}
else if (chunkType.SequenceEqual("tRNS"u8))
{
    if (length == 0 || length > 256) return null; // インデックスカラー tRNS は最大 256 bytes
    byte[] trnsData = new byte[length];
    ...
}
```
`ArvFileReader.cs` では `ReadBytes(n)` の戻り値長が `n` と一致することを検証するヘルパー `ReadExactBytes(BinaryReader reader, int count)` を導入し、`paletteLength < 2` や `width <= 0 || width % 8 != 0` の場合は即座に `InvalidDataException` をスローする。

### 3.4 [SEC-04 / SEC-09 対策] `SessionRecoveryCoordinator` の完了待機連鎖と非同期 UI キャプチャ
`ExecuteSaveAsync` がキャンセルされた際は、`throwOnError` の真偽に関わらず必ず後続の `nextTask` を `await` してから `currentTcs` を完了させることで、`FlushAsync` が常に最新の保存処理の物理完了まで待機する契約を保証する。

```csharp
// Eede.Application/Recovery/SessionRecoveryCoordinator.cs (catch (OperationCanceledException) 内)
Task? nextTask = GetNextOperationTask();
if (nextTask != null)
{
    try
    {
        await nextTask.ConfigureAwait(false);
    }
    catch (Exception ex) when (throwOnError)
    {
        currentTcs.TrySetException(ex);
        return;
    }
    catch
    {
        // throwOnError == false の場合は例外を握りつぶすが、nextTask の完了自体は確実に待つ
    }
}
currentTcs.TrySetResult();
```
併せて `MainViewModel.cs` (`L1223`) の同期 `Dispatcher.UIThread.Invoke` を非同期 `InvokeAsync` ベースのキャプチャ（または UI スケジューラ上での `ObserveOn`）へ変更し、デッドロック要因を排除する。

### 3.5 [SEC-05 / SEC-06 / SEC-10 対策] 復元データ検証の厳格化と WASM セキュリティ強化
- **復元・設定検証**: `LocalFileSessionStorage.LoadLatestSnapshotAsync` で `JsonException` に加えて `ArgumentException`（派生クラス含む）を捕捉して破損スナップショットとして安全に退避・破棄する。`DocumentSnapshot` では `!float.IsFinite(magnification) || magnification <= 0f` を検証し、`DocumentId` にも `SafePayloadRefRegex` と同等のフォーマット検証を課す。`OriginalFilePath` や `palettes_session.json` の自動読込時は `new Uri(path).IsUnc` や `\\.\` デバイスパスを拒否する。`AppSettings` 読込時は `GridWidth > 0 && GridHeight > 0` を検証してから `MinCursorWidth` / `MinCursorHeight` に反映する。
- **WASM 境界**: `wwwroot/index.html` に `<meta http-equiv="Content-Security-Policy" content="default-src 'self'; script-src 'self' 'wasm-unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' blob: data:; connect-src 'self';">` を導入し、`main.js` (`L213`) の `.withApplicationArgumentsFromQuery()` を削除、`WebExternalBrowserLauncher.cs` の `window.open` に `"noopener,noreferrer"` を追加する。

---

## 4. 優先度別ロードマップ (Remediation Roadmap)

1. **Phase 1: 即時対応 (Critical / High)**
   - **SEC-01**: `PictureSize` / `Picture` / `SkiaSharpPictureCodec` / `unsafe` フィルタ群への最大寸法制限と 64bit 境界チェック導入。
   - **SEC-02**: `AvaloniaFileStorage` の Basename キャッシュフォールバック削除と `ConcurrentDictionary` 化。
   - **SEC-03**: `PngPaletteReader` の `IHDR` (`length == 13`)・`tRNS` (`length <= 256`) 上限チェック追加。
   - **SEC-04**: `SessionRecoveryCoordinator.ExecuteSaveAsync` のキャンセル時における `nextTask` 待機保証。
2. **Phase 2: 短期対応 (Medium)**
   - **SEC-05 / SEC-06 / SEC-07**: `LocalFileSessionStorage` のドメイン例外捕捉、`DocumentSnapshot` の `float.IsFinite` 検証、`AppSettings` の正数バリデーション、`UNC` パス自動接続拒否、`ArvFileReader` の境界チェック追加。
   - **SEC-08 / SEC-09**: `AvaloniaClipboard` / `CleanupDockPicture` の `Bitmap.Dispose()` 徹底、`DrawingSession` の Undo 履歴上限導入、`Dispatcher.UIThread.InvokeAsync` 化。
3. **Phase 3: 中長期・ハードニング (Medium / Low)**
   - **SEC-10 / SEC-11 / SEC-12**: WASM 版への CSP 導入・`withApplicationArgumentsFromQuery()` 削除、`JsonSettingsRepository` 等のアトミック書き込み統一、エラー表示メッセージのサニタイズ。
