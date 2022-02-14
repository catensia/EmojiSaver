using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Hotkeys;

namespace EmojiSaver
{
    public partial class Form1 : Form
    {

        // for console debug
        static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AllocConsole();
        }


        Dictionary<string, string> specialChDic = new Dictionary<string, string>();

        private void addChList(string newKey, string newValue)
        {
            specialChDic.Add(newKey, newValue);
        }

        int key = 0;
        IDataObject ClipboardObject;
        SortedList<int, Dictionary<string, object>> previousClipboard = new SortedList<int, Dictionary<string, object>>();

        private void savePreviousClipboard()
        {
            previousClipboard.Clear();
            ClipboardObject = Clipboard.GetDataObject();
            var formats = ClipboardObject.GetFormats(false);
            Dictionary<string, object> clipboardFormats = new Dictionary<string, object>();
            foreach(var format in formats)
            {
                if(format.Contains("Text")||format.Contains("Hyperlink")||format.Contains("Bitmap"))
                {
                    clipboardFormats.Add(format, ClipboardObject.GetData(format));
                }
            }
            previousClipboard.Add(key, clipboardFormats);
        }

        private void restoreClipboard()
        {
            DataObject data = new DataObject();
            foreach(KeyValuePair<string, object> kvp in previousClipboard[key])
            {
                if(kvp.Value != null)
                {
                    data.SetData(kvp.Key, kvp.Value);
                }
            }
            Clipboard.SetDataObject(data, true);
        }


        private Hotkeys.GlobalHotkey ghk;

        private Hotkeys.GlobalHotkey[] ghk2;
        public Form1()
        {
            InitializeComponent();

            // for console debugging
            NativeMethods.AllocConsole();

            //initialize global hotkeys 
            ghk = new Hotkeys.GlobalHotkey(Constants.ALT + Constants.SHIFT, Keys.O, this);
            ghk2 = new Hotkeys.GlobalHotkey[10];
            for(int i = 0; i < ghk2.Length; i++)
            {
                ghk2[i] = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.D1 + i, this);
            }
        }

        private void WriteLine(string text)
        {
            textBox1.Text += text + Environment.NewLine;
        }

        private void HandleHotkey(ref Message m)
        {
            Console.WriteLine(m.LParam.ToString("X"));
            int HotkeyIdx = (m.LParam.ToInt32() & 0x0F0000) >> 16;
            Console.WriteLine("CTRL + " + HotkeyIdx + " is pressed!");

            string charDictKey;
            charDictKey = "CTRL+" + HotkeyIdx;
            Console.WriteLine(charDictKey);
            

            /* save current clipboard contents to previousClipboard variable */
            savePreviousClipboard();

            /* clear current clipboard */
            Clipboard.Clear();

            /* CTRL + C and get current selected text */
            Thread.Sleep(100);
            SendKeys.SendWait("^c");
            Thread.Sleep(100);

            if (Clipboard.ContainsText())
            {
                string selectedText = Clipboard.GetText();
                Console.WriteLine(selectedText);
                Console.WriteLine("==================");
                

                /* if key already exists */
                if (specialChDic.ContainsKey(charDictKey))
                {
                    /* replace current value with new value */
                    specialChDic[charDictKey] = selectedText;
                    dataGridView1[3, HotkeyIdx].Value = selectedText;
                }
                else
                {
                    addChList(charDictKey, selectedText);
                    dataGridView1[3, HotkeyIdx].Value = selectedText;
                }

            }
            else
            {
                Console.WriteLine("Nothing selected");

                /* character corresponding to hotkey registered */
                string printChar = "";
                if(specialChDic.TryGetValue(charDictKey,out printChar))
                {
                    /* if exists, print the corresponding special character */
                    SendKeys.SendWait(printChar);
                    //maybe not simple sendwait, but rich text? like colors, size, etc?
                }
                else
                {
                    //do nothing
                }
            }

            /* restore current clipboard with previous content */
            restoreClipboard();
            
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Hotkeys.Constants.WM_HOTKEY_MSG_ID)
            {
                HandleHotkey(ref m);
            }
            base.WndProc(ref m);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setupGridView();
            

            WriteLine("Trying to register SHIFT+ALT+O");
          
            if (ghk.Register()) WriteLine("Hotkey registered.");
            else WriteLine("Hotkey failed to register");
            
            for(int i = 0; i < ghk2.Length; i++)
            {
                if (ghk2[i].Register()) Console.WriteLine("Hotkey CTRL + " + (i + 1) + " registered");
                else Console.WriteLine("Hotkey CTRL + " + (i + 1) + " failed to register");
            }
            
        }


        private void Use_Notify()
        {
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.BalloonTipText = "Test입니다";
            notifyIcon1.BalloonTipTitle = "Form1";
            notifyIcon1.ShowBalloonTip(1);
        }



        private void Form1_Resize(object sender, EventArgs e)
        {
            if(FormWindowState.Minimized == WindowState)
            {
                Use_Notify();
                notifyIcon1.Visible = true;
                this.Hide();
            }
            else if(FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            Use_Notify();
            notifyIcon1.Visible = true;
        }

        private void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Visible = true;
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
            }
        }

        private void 프로그램종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (!ghk.Unregiser()) MessageBox.Show("Hotkey failed to unregister!");
            notifyIcon1.Dispose();
            Application.ExitThread();
        }

        private void 창활성화ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = true;
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }
        private void setupGridView()
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("#");
            dt.Columns.Add(" ", typeof(bool));
            dt.Columns.Add("HotKey");
            dt.Columns.Add("Character");

            for (int i = 0; i < 10; i++)
            {
                string hk = "CTRL" + "+" + i.ToString();
                dt.Rows.Add( i.ToString(), false, hk, "");
            }

            dataGridView1.DataSource = dt;


            DataGridViewColumn column0 = dataGridView1.Columns[0];
            DataGridViewColumn column1 = dataGridView1.Columns[1];
            DataGridViewColumn column2 = dataGridView1.Columns[2];
            DataGridViewColumn column3 = dataGridView1.Columns[3];
            int clientWidth = dataGridView1.ClientRectangle.Width;
            column0.Width = 25;
            column1.Width = 25;
            column2.Width = 200;
            this.dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[2].ReadOnly = true;
            dataGridView1.Columns[3].ReadOnly = true;

        }


        private void updateDGV(int mode, int N, int []idx)
        {
            //when mode is 0 => update all
            //when mode is 1 => update based on index
            if(mode == 0)
            {
                int rowidx = 0;
                foreach(DataGridViewRow row in dataGridView1.Rows)
                {
                    //empty the UI part first
                    dataGridView1[3, rowidx].Value = "";
                    foreach (KeyValuePair<string, string> entry in specialChDic)
                    {
                        if(entry.Key == row.Cells[2].Value.ToString())
                        {
                            //fill in the UI based on dictionary value
                            dataGridView1[3, rowidx].Value = entry.Value.ToString();
                        }
                    }
                    rowidx++;
                }
            }
            else
            {
                //change values of certain indices
                for(int i = 0; i < N; i++)
                {
                    int cur = idx[i];

                }
            }

        }


        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Button 1 pressed!");
            int i = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToBoolean(row.Cells[1].Value))
                {
                    //remove from dictionary 
                    string checkedHotkey = row.Cells[2].Value.ToString();

                    if (specialChDic.ContainsKey(checkedHotkey))
                    {
                        specialChDic.Remove(checkedHotkey);
                    }
                    specialChDic.Remove(row.Cells[2].Value.ToString());

                    row.Cells[1].Value = false;
                }
                else Console.WriteLine("Line " + i + " is unchecked");
                i++;
            }
            updateDGV(0, 0, null);
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
