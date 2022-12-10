using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextBinderEX
{
    [Serializable]
    public class MyAppSettings
    {
        public int Left { get; set; } = 0;
        public int Top { get; set; } = 0;
        public bool IsTopMost { get; set; } = false;
        public bool IsFixedWindowsPos { get; set; } = true;
        public bool IsDirecotryMode { get; set; } = false;
        public int ReadEncodingIndex { get; set; } = 0;
        public int WriteEncodingIndex { get; set; } = 0;
        public string ReadEncoding { get; set; } = "shift_jis";         // 結合元ファイルの文字エンコード
        public string WriteEncoding { get; set; } = "shift_jis";        // 作成するファイルの文字エンコード
        public int NewLineNum { get; set; } = 0;                        // 結合するファイルとファイルの間の改行コード数
        public bool IsDeleteFile { get; set; } = false;
        public string SaveDirectory { get; set; } = string.Empty;       // ファイルモード時の保存先フォルダ名
        public string SaveFileName { get; set; } = string.Empty;        // ファイルモード時の保存ファイル名
    }
}
