#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace AnvilClicker.Runtime
{
    /// <summary>
    /// In a WebGL build files are written to an in-memory file system. <see cref="Flush"/> asks the browser to
    /// copy it to IndexedDB so the save survives reloading the page. A no-op on every other platform.
    /// </summary>
    public static class WebGlStorage
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void Anvil_FlushFileSystem();
#endif

        public static void Flush()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Anvil_FlushFileSystem();
#endif
        }
    }
}
