using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Hiale.Win32Forms
{
    //https://support.microsoft.com/en-us/kb/145994
    //https://support.microsoft.com/en-us/kb/125681

    public class DialogUnitCalculation
    {
        private const string Gdi32 = "gdi32.dll";

        [DllImport(Gdi32, CharSet = CharSet.Auto)]
        private static extern bool GetTextMetrics(IntPtr hdc, out TEXTMETRIC lptm);

        [DllImport(Gdi32, CharSet = CharSet.Auto)]
        private static extern bool GetTextExtentPoint32(IntPtr hdc, string lpString, int cbString, out SIZE lpSize);

        [DllImport(Gdi32, CharSet = CharSet.Auto)]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport(Gdi32, CharSet = CharSet.Auto)]
        private static extern bool DeleteObject(IntPtr hdc);

        public delegate void CalculateDialogUnits(int pixelX, int pixelY, out int dialogUnitX, out int dialogUnitY);

        private bool _initiated;

        private int _baseUnitX;
        private int _baseUnitY;

        public void Init(Graphics graphics, Font font)
        {
            var hdc = graphics.GetHdc();
            var hFont = font.ToHfont();
            try
            {
                var hFontPreviouse = SelectObject(hdc, hFont);

                //Y
                TEXTMETRIC textMetric;
                GetTextMetrics(hdc, out textMetric);
                _baseUnitY = textMetric.tmHeight;

                //X
                SIZE size;
                GetTextExtentPoint32(hdc, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", 52, out size);
                _baseUnitX = (size.cx / 26 + 1) / 2;

                SelectObject(hdc, hFontPreviouse);
            }
            finally
            {
                DeleteObject(hFont);
                graphics.ReleaseHdc(hdc);
            }
            _initiated = true;
        }

        public void ToDialogUnits(int pixelX, int pixelY, out int dialogUnitX, out int dialogUnitY)
        {
            if (!_initiated)
                throw new Exception("Call Init() first!");
            dialogUnitX = (int) Math.Round((pixelX*4)/(double) _baseUnitX);
            dialogUnitY = (int) Math.Round((pixelY*8)/(double) _baseUnitY);
        }

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable InconsistentNaming

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]

        private struct TEXTMETRIC
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE
        {
            public int cx;
            public int cy;
        }

        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore MemberCanBePrivate.Local
        // ReSharper restore InconsistentNaming
    }
}
