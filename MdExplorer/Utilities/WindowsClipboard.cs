using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Gli appunti di Windows con le chiamate dirette a <c>user32</c>/<c>kernel32</c>, senza Windows Forms (07/11/2026).
    /// <para>
    /// Windows Forms serviva SOLO a questo, e portava con sé l'intero runtime desktop di Windows (Windows Forms + WPF,
    /// ~70 MB nel pacchetto) e <c>EnableUnsafeBinaryFormatterSerialization</c>. Qui: testo (CF_UNICODETEXT), immagini in
    /// lettura (il formato registrato «PNG» se un'app lo offre, altrimenti CF_DIBV5/CF_DIB convertiti in PNG) e in
    /// scrittura (PNG + CF_DIB, così incollano sia i browser e Office sia le app che conoscono solo le bitmap).
    /// </para>
    /// <para>
    /// Nessuna STA né pompa dei messaggi: i dati si consegnano subito (niente rendering differito). Per scrivere serve una
    /// finestra proprietaria — con <c>OpenClipboard(NULL)</c> la documentazione dice che SetClipboardData fallisce — e si usa
    /// una finestra «solo messaggi», creata e distrutta nello stesso thread.
    /// </para>
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static class WindowsClipboard
    {
        private const uint CF_UNICODETEXT = 13;
        private const uint CF_DIB = 8;
        private const uint CF_DIBV5 = 17;
        private const uint GMEM_MOVEABLE = 0x0002;
        private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

        [DllImport("user32.dll", SetLastError = true)] private static extern bool OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool CloseClipboard();
        [DllImport("user32.dll", SetLastError = true)] private static extern bool EmptyClipboard();
        [DllImport("user32.dll", SetLastError = true)] private static extern bool IsClipboardFormatAvailable(uint format);
        [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr GetClipboardData(uint format);
        [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr SetClipboardData(uint format, IntPtr hMem);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)] private static extern uint RegisterClipboardFormat(string format);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(int exStyle, string className, string windowName, int style, int x, int y, int w, int h,
            IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool DestroyWindow(IntPtr hWnd);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr GlobalAlloc(uint flags, UIntPtr bytes);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GlobalUnlock(IntPtr hMem);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern UIntPtr GlobalSize(IntPtr hMem);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr GlobalFree(IntPtr hMem);

        private static readonly uint CF_PNG = RegisterClipboardFormat("PNG");

        /// <summary>Il testo negli appunti; <c>null</c> se non c'è testo.</summary>
        public static string GetText()
        {
            using var _ = Open(IntPtr.Zero);
            if (!IsClipboardFormatAvailable(CF_UNICODETEXT)) return null;
            var bytes = ReadGlobal(GetClipboardData(CF_UNICODETEXT));
            if (bytes == null) return null;
            var text = Encoding.Unicode.GetString(bytes);
            var end = text.IndexOf('\0');
            return end >= 0 ? text.Substring(0, end) : text;
        }

        /// <summary>L'immagine negli appunti come PNG; <c>null</c> se non c'è un'immagine.</summary>
        public static byte[] GetImagePng()
        {
            byte[] png = null, dib = null;
            using (Open(IntPtr.Zero))
            {
                if (CF_PNG != 0 && IsClipboardFormatAvailable(CF_PNG)) png = ReadGlobal(GetClipboardData(CF_PNG));
                // CF_DIB prima di CF_DIBV5: Windows lo ricava sempre dall'altro, e GDI+ legge con sicurezza l'intestazione da 40 byte.
                else if (IsClipboardFormatAvailable(CF_DIB)) dib = ReadGlobal(GetClipboardData(CF_DIB));
                else if (IsClipboardFormatAvailable(CF_DIBV5)) dib = ReadGlobal(GetClipboardData(CF_DIBV5));
            }
            if (png != null && png.Length > 8) return png;
            return dib == null ? null : DibToPng(dib);
        }

        /// <summary>Mette negli appunti un'immagine PNG, come PNG e come CF_DIB.</summary>
        public static void SetImagePng(byte[] png)
        {
            if (png == null || png.Length == 0) throw new ArgumentException("Nessuna immagine da copiare.", nameof(png));
            var dib = PngToDib(png);
            var owner = CreateWindowEx(0, "STATIC", null, 0, 0, 0, 0, 0, HWND_MESSAGE, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if (owner == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error(), "Non riesco a creare la finestra per gli appunti");
            try
            {
                using var _ = Open(owner);
                if (!EmptyClipboard()) throw new Win32Exception(Marshal.GetLastWin32Error(), "EmptyClipboard");
                Put(CF_DIB, dib);
                if (CF_PNG != 0) Put(CF_PNG, png);
            }
            finally
            {
                DestroyWindow(owner);
            }
        }

        /// <summary>Apre gli appunti riprovando: un'altra app può tenerli occupati per qualche millisecondo.</summary>
        private static IDisposable Open(IntPtr owner)
        {
            for (var attempt = 0; ; attempt++)
            {
                if (OpenClipboard(owner)) return new Closer();
                if (attempt >= 10) throw new Win32Exception(Marshal.GetLastWin32Error(), "Gli appunti sono occupati da un'altra applicazione");
                Thread.Sleep(30);
            }
        }

        private sealed class Closer : IDisposable
        {
            public void Dispose() => CloseClipboard();
        }

        private static byte[] ReadGlobal(IntPtr hMem)
        {
            if (hMem == IntPtr.Zero) return null;
            var size = (long)(ulong)GlobalSize(hMem);
            var ptr = GlobalLock(hMem);
            if (ptr == IntPtr.Zero || size <= 0) return null;
            try
            {
                var bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, (int)size);
                return bytes;
            }
            finally
            {
                GlobalUnlock(hMem);
            }
        }

        /// <summary>Consegna i byte agli appunti: da qui la memoria è del sistema, la si libera solo se SetClipboardData fallisce.</summary>
        private static void Put(uint format, byte[] data)
        {
            var hMem = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)(ulong)data.Length);
            if (hMem == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error(), "GlobalAlloc");
            var ptr = GlobalLock(hMem);
            Marshal.Copy(data, 0, ptr, data.Length);
            GlobalUnlock(hMem);
            if (SetClipboardData(format, hMem) == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                GlobalFree(hMem);
                throw new Win32Exception(error, $"SetClipboardData({format})");
            }
        }

        /// <summary>
        /// Da DIB (BITMAPINFOHEADER o BITMAPV5HEADER + eventuali maschere e tavolozza + pixel) a PNG: si ricostruisce il file BMP
        /// mettendo davanti la BITMAPFILEHEADER da 14 byte, e lo si fa leggere a GDI+.
        /// </summary>
        internal static byte[] DibToPng(byte[] dib)
        {
            if (dib == null || dib.Length < 40) return null;
            var headerSize = BitConverter.ToInt32(dib, 0);
            var bitCount = BitConverter.ToInt16(dib, 14);
            var compression = BitConverter.ToInt32(dib, 16);
            var clrUsed = BitConverter.ToInt32(dib, 32);
            const int BI_BITFIELDS = 3;
            var masks = headerSize == 40 && compression == BI_BITFIELDS ? 12 : 0;
            var colors = clrUsed > 0 ? clrUsed : (bitCount <= 8 ? 1 << bitCount : 0);
            var pixelOffset = 14 + headerSize + masks + colors * 4;

            using var bmp = new MemoryStream(14 + dib.Length);
            using (var w = new BinaryWriter(bmp, Encoding.ASCII, leaveOpen: true))
            {
                w.Write((byte)'B'); w.Write((byte)'M');
                w.Write(14 + dib.Length);
                w.Write(0);
                w.Write(pixelOffset);
                w.Write(dib);
            }
            bmp.Position = 0;
            using var image = Image.FromStream(bmp);
            using var png = new MemoryStream();
            image.Save(png, ImageFormat.Png);
            return png.ToArray();
        }

        /// <summary>Da PNG a CF_DIB: il BMP che GDI+ scrive, senza i 14 byte della BITMAPFILEHEADER.</summary>
        internal static byte[] PngToDib(byte[] png)
        {
            using var input = new MemoryStream(png);
            using var image = Image.FromStream(input);
            using var bitmap = new Bitmap(image);
            using var bmp = new MemoryStream();
            bitmap.Save(bmp, ImageFormat.Bmp);
            var all = bmp.ToArray();
            var dib = new byte[all.Length - 14];
            Buffer.BlockCopy(all, 14, dib, 0, dib.Length);
            return dib;
        }
    }
}
