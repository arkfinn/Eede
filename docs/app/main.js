import { dotnet } from './_framework/dotnet.js';

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error("Expected to be running in a browser");

// キーボードショートカット防衛: ブラウザの既定ショートカット（Ctrl+S, Ctrl+O, Ctrl+W等）をブロックしてEedeへ専有
window.addEventListener('keydown', (e) => {
    if (e.ctrlKey || e.metaKey) {
        const key = e.key.toLowerCase();
        // ブロック対象のキー: s(保存), o(開く), w(タブ閉じる), n(新規), t(変形), z(元に戻す), y(やり直し), d(選択解除), a(アンチエイリアス), r(回転), e(アニメパネル)
        if (['s', 'o', 'w', 'n', 't', 'z', 'y', 'd', 'r', 'e'].includes(key)) {
            // テキスト入力中でない限りブラウザ挙動を抑制
            const tag = document.activeElement ? document.activeElement.tagName.toLowerCase() : '';
            if (tag !== 'input' && tag !== 'textarea') {
                e.preventDefault();
            }
        }
    } else if (e.key === 'F5' || e.key === 'F1' || e.key === 'F2' || e.key === 'F3' || e.key === 'F4') {
        // F5(ドック読込), F1~F4(レイヤースタイル/アニメ)
        const tag = document.activeElement ? document.activeElement.tagName.toLowerCase() : '';
        if (tag !== 'input' && tag !== 'textarea') {
            e.preventDefault();
        }
    }
}, { capture: true });

// プログレスバー更新
const progressBar = document.getElementById('loading-bar-fill');
const progressText = document.getElementById('loading-text');

const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

const config = getConfig();

// ローディング完了時のUI非表示
const loader = document.getElementById('loading-screen');
if (loader) {
    loader.style.opacity = '0';
    setTimeout(() => {
        loader.style.display = 'none';
    }, 300);
}

await dotnet.run();
