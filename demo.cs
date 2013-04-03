using System;
using System.Windows.Forms;
using SkypePublicApiConnector;

namespace DemoApp
{
    public partial class Form1 : Form
    {
        TextBox logBox;
        TextBox editBox;

        SkypeConnector skype;

        public Form1()
        {
            InitializeComponent();
            MakeGUI();

            skype = new SkypeConnector(this);
            skype.OnAttach += skype_OnAttach;
            skype.OnReceive += skype_OnReceive;
        }

        void editBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (editBox.Text == "") return;
                if (skype.IsDisconnected) return;

                logBox.AppendText(" < " + editBox.Text + "\r\n");
                skype.SendCommand(editBox.Text);
                editBox.Clear();
            }
        }

        void skype_OnAttach(SkypeConnector.OnAttachArgs e)
        {
            logBox.AppendText("** attachment state = " + e.state.ToString() + "\r\n");
        }

        void skype_OnReceive(SkypeConnector.OnReceiveArgs e)
        {
            logBox.AppendText(" > " + e.msg + "\r\n");
        }

        public void MakeGUI()
        {
            this.Text = "SkypeConnector demo";

            editBox = new TextBox()
            {
                Left = 0, Height = 12, Top = this.Height - (this.Height - this.ClientSize.Height) - 20, Width = this.Width,
                Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right),
            };
            this.Controls.Add(editBox);
            editBox.KeyPress += editBox_KeyPress;

            logBox = new TextBox()
            {
                Left = 0, Top = 0, Height = this.Height - editBox.Height, Width = this.Width,
                Anchor = (AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right),
                ReadOnly = true, Multiline = true, TabStop = false
            };
            this.Controls.Add(logBox);
        }
    }
}
