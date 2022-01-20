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

        private IDataObject previousClipboard;
        private void setPreviousClipboard(IDataObject currentClipboardObj)
        {
            previousClipboard = currentClipboardObj;
        }
        private IDataObject getPreviousClipboard()
        {
            return previousClipboard;
        }

        private void restoreCurrentClipboard(ref IDataObject currentClipboardObj)
        {
            currentClipboardObj = previousClipboard;
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

            /*
             * Maybe should restore previous clipboard contents??
             */

            /* save current clipboard contents to previousClipboard variable */
            setPreviousClipboard(Clipboard.GetDataObject());

            /* clear current clipboard */
            Clipboard.Clear();

            /* CTRL + C and get current selected text */
            Thread.Sleep(100);
            SendKeys.SendWait("^c");
            Thread.Sleep(100);

            if (Clipboard.ContainsText())
            {
                var selectedText = Clipboard.GetText();
                Console.WriteLine(selectedText);
                Console.WriteLine(Clipboard.GetDataObject().GetData(Text));

            }
            else
            {
                Console.WriteLine("Nothing selected");
            }

            /* restore current clipboard with previous content */
            Clipboard.SetDataObject(getPreviousClipboard());
            
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

    }
}
