# WPF開発者向けAvaloniaUIチートシート

WPF開発者向けに、AvaloniaUIの機能と構文をWPFと比較し、AXAML、コードビハインド、MVVMなどの記述を支援するチートシートです。

## 基本UI要素

AvaloniaUIはWPFと同様のUIコントロール（Button、TextBlock、TextBox、ListBoxなど）をAXAMLで記述します。AXAMLはWPFのXAMLと同様のXMLベースのマークアップ言語です。

```XML
<Button Content="クリックミー"/>
```

WPFと同様に、Content、Width、Heightなどの属性でコントロールをカスタマイズできます。AvaloniaUIのコントロールは、描画、レイアウト、ユーザー、テンプレート化されたコントロールに分類でき、テンプレート化されたコントロールはXAMLで再テンプレート化可能です。

## レイアウト

AvaloniaUIはWPFと同様に、StackPanel、Grid、DockPanel、CanvasなどのレイアウトパネルをAXAMLで使用できます。

```XML
<StackPanel Orientation="Vertical">
    <TextBlock Text="項目1"/>
    <TextBlock Text="項目2"/>
    <Button Content="ボタン"/>
</StackPanel>
```

```XML
<Grid ColumnDefinitions="Auto,*" RowDefinitions="Auto,Auto">
    <TextBlock Text="名前:" Grid.Row="0" Grid.Column="0"/>
    <TextBox Grid.Row="0" Grid.Column="1"/>
    <TextBlock Text="メール:" Grid.Row="1" Grid.Column="0"/>
    <TextBox Grid.Row="1" Grid.Column="1"/>
</Grid>
```

各パネルは独自のレイアウト動作を実装し、子要素のMeasureOverrideとArrangeOverrideメソッドを実行します。

## データバインディング

AvaloniaUIのデータバインディング構文はWPFと類似しており、`{Binding PropertyName}`のように記述します。ElementNameやRelativeSourceも使用可能です。Mode、Converter、StringFormatなどのオプションも利用できます。

コンパイル済みバインディングを使用すると、パフォーマンスが向上し、コンパイル時の型チェックが可能です。`x:DataType`を指定し、`{CompiledBinding PropertyName}`を使用します。

例：

*   `DataContext`の`Name`プロパティ: `<TextBlock Text="{Binding Name}"/>`
*   `input`という名前のTextBoxの`Text`プロパティ: `<TextBlock Text="{Binding #input.Text}"/>`
*   親Windowの`Title`プロパティ: `<TextBlock Text="{Binding $parent[Window].Title}"/>`

## コマンド

AvaloniaUIでは、WPFと同様に`ICommand`インターフェースを使用します。ReactiveUIの`ReactiveCommand`も利用可能です。

```XML
<Button Content="実行" Command="{Binding MyCommand}"/>
```

```XML
<Button Content="削除" Command="{Binding DeleteCommand}" CommandParameter="{Binding ItemToDelete}"/>
```

`ReactiveCommand`の`CanExecute`ロジックでコマンドの実行可能性を制御できます。ViewModelのメソッドを直接バインディングすることも可能です。

```XML
<Button Content="処理" Command="{Binding RunTheThing}" CommandParameter="HelloWorld"/>
```

## スタイルとテンプレート

AvaloniaUIのスタイリングはWPFと異なり、`<Style>`要素はコントロールまたはアプリケーションの`Styles`コレクション内に定義されます。`TargetType`の代わりに、CSSのようなセレクタ構文を持つ`Selector`属性を使用します。

例：`Selector="Button"`、`Selector="Button.myClass"`

プロパティの設定には`<Setter Property="PropertyName" Value="Value"/>`を使用します。ネストされたスタイルやスタイルクラスもサポートされています。

`ControlTheme`はWPFの`ControlTemplate`に類似しており、`DataTemplate`はWPFと同様に`<Control.DataTemplates>`または`<Application.DataTemplates>`内で定義します。

## コードビハインドでのイベント処理

AvaloniaUIでは、AXAMLでイベント属性を使用してイベントハンドラメソッドを指定し、対応する`.axaml.cs`ファイルで実装します。

```XML
<Button Click="MyButton_Click">クリック</Button>
```

```C#
public void MyButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
{
    System.Console.WriteLine("ボタンがクリックされました！");
}
```

`x:Name`属性でコントロールに名前を付け、コードビハインドからアクセスできます。`InitializeComponent()`メソッドでAXAMLファイルをロードします。

## MVVMの実装

AvaloniaUIでMVVMパターンを実装する手順はWPFと類似しています。

1.  ViewModelを作成し、`INotifyPropertyChanged`を実装します。
2.  Viewの`DataContext`にViewModelのインスタンスを設定します。
3.  AXAMLで、ViewのUI要素をViewModelのプロパティやコマンドにバインドします。

```XML
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:YourNamespace.ViewModels"
        x:Class="YourNamespace.Views.MainWindow"
        x:DataType="vm:MainViewModel">
    <StackPanel>
        <TextBlock Text="{Binding Name}"/>
        <Button Content="挨拶" Command="{Binding GreetCommand}"/>
    </StackPanel>
</Window>
```

```C#
public partial class MainWindow : Avalonia.Controls.Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ViewModels.MainViewModel();
    }
}
```

ReactiveUIと組み合わせて使用すると、MVVM開発がより簡単になります。

## 結論

AvaloniaUIはWPFの知識を活かせるフレームワークであり、AXAML構文やデータバインディングはWPFと類似しています。コンパイル済みバインディングやCSSライクなスタイリングなどのAvaloniaUI独自の機能も活用できます。

### 引用文献

Avalonia Docs: （https://docs.avaloniaui.net/）（2025年3月22日閲覧）
