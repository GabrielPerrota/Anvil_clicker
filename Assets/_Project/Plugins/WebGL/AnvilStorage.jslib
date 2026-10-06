// Unity WebGL keeps persistentDataPath in an in-memory file system backed by IndexedDB.
// Writes only reach the browser's storage after FS.syncfs, so the game calls this after every save.
mergeInto(LibraryManager.library, {
    Anvil_FlushFileSystem: function () {
        if (typeof FS === 'undefined' || typeof FS.syncfs !== 'function') return;

        FS.syncfs(false, function (error) {
            if (error) console.error('[Anvil] Could not persist the save to IndexedDB:', error);
        });
    }
});
