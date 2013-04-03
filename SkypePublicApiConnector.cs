/*************************************************************************************************
 * The SkypeConnector class exposes simple access to the Skype Public API (Skype aka Desktop API).
 * For more information on Skype Public API http://dev.skype.com/accessories
 * 
 * 1. Establishing connection to Skype desktop client
 * Attempt to establish connection is made automatically at SkypeConnector instantiation.
 * Sending and receiving messages becomes possible after OnAttach event fires with state == success.
 * 
 * 2. Sending commands
 * Once connection is established, you can use SendCommand method to transmit Public API protocol 
 * commands. For more information see http://dev.skype.com/desktop-api-reference
 * 
 * 3. Receiving messages
 * Feedback messages from Skype can be had via OnReceive event.
 * 
 * This file can be copied into an empty class library project and compiled as dll. If you paste it
 * directly into your project, you will need to modify the namespace.
 * 
 * NB! The Public API transport goes over Windows messaging system. Thus a valid window handle 
 * and a WndProc are required for this lib to work. The SkypeConnector constructor takes a Form 
 * argument (presumably the main form but any will do) and inserts a hidden control into it,
 * which provides hwind and WndProc for the transport. A side effect of this is dependancy
 * on Windows.Forms, but a making a WPF version should not be terribly difficult.
 * 
 *************************************************************************************************/

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace SkypePublicApiConnector
{
    public class SkypeConnector
    {
        public bool IsConnected { get { return Internal.connectionState == AttachmentState.Success; } }
        public bool IsDisconnected { get { return Internal.connectionState != AttachmentState.Success; } }
        public AttachmentState Attachment { get { return Internal.connectionState; } }

        public enum AttachmentState
        {
            Success = 0,
            Pending = 1,
            Refused = 2,
            Unavailable = 3,
            UnknownState = 4
        }

        private UTF8Encoding utf8 = new UTF8Encoding();

        public SkypeConnector(Form mainForm)
        {
            Internal.mainForm = mainForm;
            MsgReceiver msgReceiver = new MsgReceiver(this);
            mainForm.Controls.Add(msgReceiver);
            Internal.receiverHWND = msgReceiver.Handle.ToInt32();
            AttatchToSkype();
        }

        public void SendCommand(string cmd)
        {
            if (this.IsDisconnected) throw new Exception("Skype instance is not connected.");

            int cmdLength = utf8.GetByteCount(cmd) + 1;
            byte[] utf8cmd = new byte[cmdLength];
            utf8.GetBytes(cmd, 0, cmdLength - 1, utf8cmd, 0);

            Native.CopyDataStruct copyStruct = new Native.CopyDataStruct();
            copyStruct.dwData = IntPtr.Zero;
            copyStruct.cbData = cmdLength;

            copyStruct.lpData = Marshal.AllocHGlobal(cmdLength);
            Marshal.Copy(utf8cmd, 0, copyStruct.lpData, cmdLength);

            Native.SendMessage(Internal.skypeHwnd.ToInt32(), (uint)Internal.WM_COPYDATA, Internal.receiverHWND, ref copyStruct);
        }

        private void AttatchToSkype()
        {
            Internal.msgAttach = Native.RegisterWindowMessage("SkypeControlAPIAttach");
            Internal.msgDiscover = Native.RegisterWindowMessage("SkypeControlAPIDiscover");
            Native.PostMessage(Internal.HWND_BROADCAST, Internal.msgDiscover, Internal.receiverHWND, 0);
        }

        private class MsgReceiver : Control
        {
            SkypeConnector connector;

            public MsgReceiver(SkypeConnector connector)
            {
                this.connector = connector;
                this.Visible = false;
            }

            private static string Utf8PtrToString(IntPtr utf8)
            {
                int len = Native.MultiByteToWideChar(65001, 0, utf8, -1, null, 0);
                if (len == 0) throw new Exception("Null message received from Skype");
                StringBuilder buf = new StringBuilder(len);
                len = Native.MultiByteToWideChar(65001, 0, utf8, -1, buf, len);
                return buf.ToString();
            }

            protected override void WndProc(ref Message msg)
            {
                if (msg.Msg == Internal.msgAttach)
                {
                    Internal.skypeHwnd = msg.WParam;
                    AttachmentState state = (AttachmentState)msg.LParam.ToInt32();
                    Internal.connectionState = state;
                    connector.FireOnAttachEvent(state);
                    msg.Result = (IntPtr)1;
                    return;
                }

                if ((msg.Msg == Internal.WM_COPYDATA) & (msg.WParam == Internal.skypeHwnd))
                {
                    Native.CopyDataStruct copyStruct = (Native.CopyDataStruct)Marshal.PtrToStructure(msg.LParam, typeof(Native.CopyDataStruct));
                    string fromSkype = Utf8PtrToString(copyStruct.lpData);

                    connector.FireOnReceiveEvent(fromSkype);

                    msg.Result = (IntPtr)1;
                    return;
                }
                base.WndProc(ref msg);
            } // WndProc

        } // MsgReceiver


        /* OnReceive event */
        public event OnReceiveDelegate OnReceive;
        public delegate void OnReceiveDelegate(OnReceiveArgs e);

        public class OnReceiveArgs : EventArgs
        {
            public String msg;
            public OnReceiveArgs(String msg) { this.msg = msg; }
        }

        internal void FireOnReceiveEvent(String msg)
        {
            if (OnReceive == null) return; // Event not assigned    
            OnReceiveArgs args = new OnReceiveArgs(msg);
            Internal.mainForm.BeginInvoke(OnReceive, new object[] { args }); // Syncing to GUI thread
        }

        /* OnAttach event */
        public event OnAttachDelegate OnAttach;
        public delegate void OnAttachDelegate(OnAttachArgs e);

        public class OnAttachArgs : EventArgs
        {
            public AttachmentState state;
            public OnAttachArgs(AttachmentState state) { this.state = state; }
        }

        internal void FireOnAttachEvent(AttachmentState state)
        {
            if (OnAttach == null) return; // Event not assigned    
            OnAttachArgs args = new OnAttachArgs(state);
            Internal.mainForm.BeginInvoke(OnAttach, new object[] { args }); // Syncing to GUI thread
        }


        static class Internal
        {
            public static AttachmentState connectionState = AttachmentState.UnknownState;

            public static int receiverHWND = 0;
            public static IntPtr skypeHwnd = IntPtr.Zero;

            public static int WM_COPYDATA = 0x004A;
            public static IntPtr HWND_BROADCAST = (IntPtr)0xffff;

            public static uint msgAttach;
            public static uint msgDiscover;

            public static Form mainForm;
        } // Internal


        static class Native
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct CopyDataStruct
            {
                public IntPtr dwData;
                public int cbData;
                public IntPtr lpData;
            }

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern uint RegisterWindowMessage(string lpString);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int MultiByteToWideChar(int codepage, int flags, IntPtr utf8, int utf8len, StringBuilder buffer, int buflen);

            [DllImport("user32.dll")]
            public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

            [DllImport("user32.dll")]
            public static extern int SendMessage(int hWnd, uint Msg, int wParam, ref CopyDataStruct lParam);
        } // Native

    } // SkypeConnector
}
