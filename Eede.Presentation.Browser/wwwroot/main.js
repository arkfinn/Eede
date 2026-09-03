import { dotnet } from './_framework/dotnet.js';

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error("Expected to be running in a browser");

// ==========================================
// Eede Web Session Recovery (IndexedDB)
// ==========================================
(function() {
    const DB_NAME = 'eede_session_db';
    const DB_VERSION = 1;
    const STORE_META = 'metadata';
    const STORE_PAYLOADS = 'payloads';

    let dbPromise = null;
    function getDb() {
        if (!dbPromise) {
            dbPromise = new Promise((resolve, reject) => {
                if (!window.indexedDB) {
                    reject(new Error("IndexedDB is not supported in this browser."));
                    return;
                }
                const req = indexedDB.open(DB_NAME, DB_VERSION);
                req.onupgradeneeded = (e) => {
                    const db = e.target.result;
                    if (!db.objectStoreNames.contains(STORE_META)) {
                        db.createObjectStore(STORE_META);
                    }
                    if (!db.objectStoreNames.contains(STORE_PAYLOADS)) {
                        db.createObjectStore(STORE_PAYLOADS);
                    }
                };
                req.onsuccess = () => resolve(req.result);
                req.onerror = () => reject(req.error);
            });
        }
        return dbPromise;
    }

    window.eedeSessionDb = {
        saveSnapshot: async function(snapshotJson, payloadsJson) {
            try {
                const db = await getDb();
                const payloads = payloadsJson ? JSON.parse(payloadsJson) : {};

                // まず全データを保存するトランザクションを試行
                try {
                    const tx = db.transaction([STORE_META, STORE_PAYLOADS], 'readwrite');
                    const metaStore = tx.objectStore(STORE_META);
                    const payloadStore = tx.objectStore(STORE_PAYLOADS);

                    metaStore.put(snapshotJson, 'latest');
                    metaStore.delete('clean_exit');
                    payloadStore.clear();

                    for (const [key, base64Val] of Object.entries(payloads)) {
                        payloadStore.put(base64Val, key);
                    }

                    await new Promise((res, rej) => {
                        tx.oncomplete = res;
                        tx.onerror = () => rej(tx.error);
                        tx.onabort = () => rej(tx.error);
                    });
                    return true;
                } catch (quotaErr) {
                    console.warn("[Eede IndexedDB] Storage quota exceeded or transaction failed. Gracefully degrading to metadata-only snapshot:", quotaErr);
                    // 優雅な縮退: ペイロードを全消去し、メタデータのみを保存
                    const fallbackTx = db.transaction([STORE_META, STORE_PAYLOADS], 'readwrite');
                    const fallbackMeta = fallbackTx.objectStore(STORE_META);
                    const fallbackPayload = fallbackTx.objectStore(STORE_PAYLOADS);

                    fallbackPayload.clear();
                    fallbackMeta.put(snapshotJson, 'latest');
                    fallbackMeta.delete('clean_exit');

                    await new Promise((res, rej) => {
                        fallbackTx.oncomplete = res;
                        fallbackTx.onerror = () => rej(fallbackTx.error);
                    });
                    return true;
                }
            } catch (err) {
                console.error("[Eede IndexedDB] Failed to save snapshot completely:", err);
                return false;
            }
        },

        loadLatestSnapshot: async function() {
            try {
                const db = await getDb();
                const tx = db.transaction([STORE_META], 'readonly');
                const store = tx.objectStore(STORE_META);
                return await new Promise((res, rej) => {
                    const req = store.get('latest');
                    req.onsuccess = () => res(req.result || null);
                    req.onerror = () => rej(req.error);
                });
            } catch (err) {
                console.warn("[Eede IndexedDB] Failed to load latest snapshot:", err);
                return null;
            }
        },

        loadPayload: async function(payloadRef) {
            try {
                const db = await getDb();
                const tx = db.transaction([STORE_PAYLOADS], 'readonly');
                const store = tx.objectStore(STORE_PAYLOADS);
                return await new Promise((res, rej) => {
                    const req = store.get(payloadRef);
                    req.onsuccess = () => res(req.result || null);
                    req.onerror = () => rej(req.error);
                });
            } catch (err) {
                console.warn(`[Eede IndexedDB] Failed to load payload '${payloadRef}':`, err);
                return null;
            }
        },

        clearSession: async function() {
            try {
                const db = await getDb();
                const tx = db.transaction([STORE_META, STORE_PAYLOADS], 'readwrite');
                tx.objectStore(STORE_META).clear();
                tx.objectStore(STORE_PAYLOADS).clear();
                await new Promise((res, rej) => {
                    tx.oncomplete = res;
                    tx.onerror = () => rej(tx.error);
                });
                return true;
            } catch (err) {
                console.warn("[Eede IndexedDB] Failed to clear session:", err);
                return false;
            }
        },

        hasActiveSession: async function() {
            try {
                const db = await getDb();
                const tx = db.transaction([STORE_META], 'readonly');
                const store = tx.objectStore(STORE_META);
                return await new Promise((res, rej) => {
                    const req = store.get('latest');
                    req.onsuccess = () => res(req.result != null);
                    req.onerror = () => rej(req.error);
                });
            } catch (err) {
                return false;
            }
        },

        markCleanExit: async function() {
            try {
                const db = await getDb();
                const tx = db.transaction([STORE_META], 'readwrite');
                tx.objectStore(STORE_META).put(true, 'clean_exit');
                await new Promise((res, rej) => {
                    tx.oncomplete = res;
                    tx.onerror = () => rej(tx.error);
                });
                return true;
            } catch (err) {
                console.warn("[Eede IndexedDB] Failed to mark clean exit:", err);
                return false;
            }
        },

        hasCleanExit: async function() {
            try {
                const db = await getDb();
                const tx = db.transaction([STORE_META], 'readonly');
                const store = tx.objectStore(STORE_META);
                return await new Promise((res, rej) => {
                    const req = store.get('clean_exit');
                    req.onsuccess = () => res(req.result === true);
                    req.onerror = () => rej(req.error);
                });
            } catch (err) {
                return false;
            }
        }
    };
})();

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
const hideLoader = () => {
    const loader = document.getElementById('loading-screen');
    if (loader) {
        loader.style.opacity = '0';
        setTimeout(() => {
            loader.style.display = 'none';
        }, 300);
    }
};

try {
    if (progressText) progressText.innerText = "Starting Eede application...";
    
    // dotnet.run() を開始（常駐型アプリのため完了待機せずエラー監視）
    dotnet.run().catch((err) => {
        console.error("[Eede Runtime Error]", err);
        const loader = document.getElementById('loading-screen');
        if (loader) {
            loader.style.display = 'flex';
            loader.style.opacity = '1';
        }
        if (progressText) {
            progressText.style.color = '#ef4444';
            progressText.style.fontWeight = 'bold';
            progressText.innerText = `起動エラー: ${err.message || err}`;
        }
        if (progressBar) {
            progressBar.style.background = '#ef4444';
        }
    });

    // Avalonia WebGL キャンバスのマウントに合わせてローダーを非表示
    setTimeout(hideLoader, 400);
} catch (err) {
    console.error("[Eede JS Bootstrap Error]", err);
    if (progressText) {
        progressText.style.color = '#ef4444';
        progressText.style.fontWeight = 'bold';
        progressText.innerText = `起動エラー: ${err.message || err}`;
    }
    if (progressBar) {
        progressBar.style.background = '#ef4444';
    }
}
