using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using My.Collections;
using My.IO;
using My.Security;

namespace TextBinderEX
{
    public partial class Form1 : Form
    {
        readonly string _ConfigPath = @"config.dat";
        MyAppSettings _AppSettings = new MyAppSettings();
        List<string> _InputNames = new List<string>();

        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadConfigFile();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveConfigFile();
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            //コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                //ファイル以外は受け付けない
                e.Effect = DragDropEffects.None;
            }
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (radioButton1.Checked)
            {
                //コントロール内にドロップされたとき実行される
                //ドロップされたすべてのファイル名を取得する
                string[] Files = (string[])e.Data.GetData(DataFormats.FileDrop, false);

                Files = Files.Where(x => File.Exists(x) && IsValidExtension(Path.GetExtension(x))).ToArray();

                foreach (string File in Files)
                {
                    //フルパスからファイル名だけを取得してListBoxに追加する
                    listBox1.Items.Add(Path.GetFileName(File));
                    _InputNames.Add(File);
                }
            }
            else
            {
                string[] Directories = (string[])e.Data.GetData(DataFormats.FileDrop, false);

                Directories = Directories.Where(x => Directory.Exists(x)).ToArray();

                foreach (string Directory in Directories)
                {
                    //フルパスからファイル名だけを取得してListBoxに追加する
                    listBox1.Items.Add(Path.GetFileName(Directory));
                    _InputNames.Add(Directory);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e) // ファイルの結合を開始するボタン
        {
            int NewLineNum = (int)numericUpDown1.Value;
            string SavePath;

            if (radioButton1.Checked) // ファイルモード
            {
                string SaveDirectory = textBox1.Text;
                string SaveFileName = textBox2.Text;

                if (!Directory.Exists(SaveDirectory) || SaveFileName == string.Empty)
                {
                    return;
                }

                SavePath = SaveDirectory + Path.DirectorySeparatorChar + SaveFileName;

                foreach (string SourceFile in _InputNames)
                {
                    if (File.Exists(SourceFile) && SourceFile != SavePath)
                    {
                        string txt = string.Empty;

                        if (File.Exists(SavePath))
                        {
                            for (int i = 0; i < NewLineNum; i++)
                            {
                                txt += Environment.NewLine;
                            }
                        }

                        txt += File.ReadAllText(SourceFile, Encoding.GetEncoding(_AppSettings.ReadEncoding));

                        File.AppendAllText(SavePath, txt, Encoding.GetEncoding(_AppSettings.WriteEncoding));

                        if (checkBox1.Checked)
                        {
                            File.Delete(SourceFile);
                        }
                    }
                }
            }
            else // フォルダモード
            {
                foreach (string Dir in _InputNames)
                {
                    if (!Directory.Exists(Dir))
                    {
                        break;
                    }

                    string[] Files = Directory.GetFiles(Dir);

                    Files = Files.Where(x => File.Exists(x) && IsValidExtension(Path.GetExtension(x)))
                                 .OrderBy(x => x, new NaturalSortComparer())
                                 .ToArray();

                    foreach (string SourceFile in Files)
                    {
                        SavePath = Dir + Path.DirectorySeparatorChar + Path.GetFileName(Dir) + Path.GetExtension(SourceFile);

                        if (SourceFile != SavePath)
                        {
                            string txt = string.Empty;

                            if (File.Exists(SavePath))
                            {
                                for (int i = 0; i < NewLineNum; i++)
                                {
                                    txt += Environment.NewLine;
                                }
                            }

                            txt += File.ReadAllText(SourceFile, Encoding.GetEncoding(_AppSettings.ReadEncoding));

                            File.AppendAllText(SavePath, txt, Encoding.GetEncoding(_AppSettings.WriteEncoding));

                            if (checkBox1.Checked)
                            {
                                File.Delete(SourceFile);
                            }
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e) // 追加ボタン
        {
            if (radioButton1.Checked)
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    listBox1.Items.Add(openFileDialog1.SafeFileName);
                    _InputNames.Add(openFileDialog1.FileName);
                }
            }
            else
            {
                if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
                {
                    listBox1.Items.Add(Path.GetFileName(folderBrowserDialog2.SelectedPath));
                    _InputNames.Add(folderBrowserDialog2.SelectedPath);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e) // 削除ボタン
        {
            int Index = listBox1.SelectedIndex;

            if (-1 < Index)
            {
                listBox1.Items.RemoveAt(Index);
                _InputNames.RemoveAt(Index);
            }
        }

        private void button4_Click(object sender, EventArgs e) // 全消去ボタン
        {
            listBox1.Items.Clear();
            _InputNames.Clear();
        }

        private void button5_Click(object sender, EventArgs e) // ↑ボタン
        {
            int Index = listBox1.SelectedIndex;

            if (0 < Index)
            {
                string temp;

                temp = listBox1.Items[Index - 1].ToString();
                listBox1.Items[Index - 1] = listBox1.Items[Index];
                listBox1.Items[Index] = temp;

                temp = _InputNames[Index - 1];
                _InputNames[Index - 1] = _InputNames[Index];
                _InputNames[Index] = temp;

                listBox1.SelectedIndex -= 1;
            }
        }

        private void button6_Click(object sender, EventArgs e) // ↓ボタン
        {
            int Index = listBox1.SelectedIndex;

            if (-1 < Index && Index < listBox1.Items.Count - 1)
            {
                string temp;

                temp = listBox1.Items[Index + 1].ToString();
                listBox1.Items[Index + 1] = listBox1.Items[Index];
                listBox1.Items[Index] = temp;

                temp = _InputNames[Index + 1];
                _InputNames[Index + 1] = _InputNames[Index];
                _InputNames[Index] = temp;

                listBox1.SelectedIndex += 1;
            }
        }

        private void button7_Click(object sender, EventArgs e) // 参照ボタン
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int Index = comboBox1.SelectedIndex;

            if (Index == 0)
            {
                _AppSettings.ReadEncoding = "shift_jis";
            }
            else if (Index == 1)
            {
                _AppSettings.ReadEncoding = "euc-jp";
            }
            else if (Index == 2)
            {
                _AppSettings.ReadEncoding = "utf-16";
            }
            else
            {
                _AppSettings.ReadEncoding = "utf-8";
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int Index = comboBox2.SelectedIndex;

            if (Index == 0)
            {
                _AppSettings.WriteEncoding = "shift_jis";
            }
            else if (Index == 1)
            {
                _AppSettings.WriteEncoding = "euc-jp";
            }
            else if (Index == 2)
            {
                _AppSettings.WriteEncoding = "utf-16";
            }
            else
            {
                _AppSettings.WriteEncoding = "utf-8";
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                button7.Enabled = true;
                textBox1.Text = string.Empty;
                textBox2.Text = string.Empty;
            }
            else
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                button7.Enabled = false;
                textBox1.Text = "元フォルダが保存先フォルダになります";
                textBox2.Text = "元フォルダ名がファイル名になります";
            }

            listBox1.Items.Clear();
            _InputNames.Clear();
        }

        private bool IsValidExtension(string Extension) // 拡張子が定められたものかどうか判断するメソッド。
        {
            switch (Extension)
            {
                case ".txt":
                case ".csv":
                case ".log":
                    return true;

                default:
                    return false;
            }
        }

        private void LoadConfigFile() // 設定ファイルの読み込みメソッド。
        {
            if (FileAdvanced.Exists(_ConfigPath))
            {
                byte[] bs = FileAdvanced.LoadFromBinaryFile<byte[]>(_ConfigPath);

                _AppSettings = Cryptography.Decrypt<MyAppSettings>(bs);
                TopMost = _AppSettings.IsTopMost;

                if (_AppSettings.IsFixedWindowsPos)
                {
                    Left = _AppSettings.Left;
                    Top = _AppSettings.Top;
                }
                if (_AppSettings.IsDirecotryMode)
                {
                    radioButton2.Checked = true;
                }

                comboBox1.SelectedIndex = _AppSettings.ReadEncodingIndex;
                comboBox2.SelectedIndex = _AppSettings.WriteEncodingIndex;
                numericUpDown1.Value = _AppSettings.NewLineNum;
                checkBox1.Checked = _AppSettings.IsDeleteFile;
                textBox1.Text = _AppSettings.SaveDirectory;
                textBox2.Text = _AppSettings.SaveFileName;
            }
        }

        private void SaveConfigFile() // 設定ファイルへの書き込みメソッド。
        {
            if (WindowState == FormWindowState.Normal)
            {
                _AppSettings.Left = Left;
                _AppSettings.Top = Top;
            }

            _AppSettings.IsDirecotryMode = radioButton2.Checked;
            _AppSettings.ReadEncodingIndex = comboBox1.SelectedIndex;
            _AppSettings.WriteEncodingIndex = comboBox2.SelectedIndex;
            _AppSettings.NewLineNum = (int)numericUpDown1.Value;
            _AppSettings.IsDeleteFile = checkBox1.Checked;
            _AppSettings.SaveDirectory = textBox1.Text;
            _AppSettings.SaveFileName = textBox2.Text;

            FileAdvanced.SaveToBinaryFile(_ConfigPath, Cryptography.Encrypt(_AppSettings));
        }
    }
}
