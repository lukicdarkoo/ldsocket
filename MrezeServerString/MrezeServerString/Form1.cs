using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LDSocket;

namespace MrezeServerString
{
    public partial class Form1 : Form
    {
        LDSocket.SocketServer server = new LDSocket.SocketServer();

        public Form1()
        {
            InitializeComponent();

            server.DataReceived += new SocketServer.DataReceivedHandler(server_DataReceived);
            server.ClientConnectionChanged += new SocketServer.ClientConnectinChangedHandler(server_ClientConnectionChanged);
        }

        void server_ClientConnectionChanged(bool state, int index)
        {
            if (state)
                MessageBox.Show("Klijent " + index + " je konektovan");
            else MessageBox.Show("Klijent " + index + " je diskonektovan");
        }

        void server_DataReceived(string data, int index)
        {
            Invoke(new MethodInvoker(
                delegate
                {
                    label1.Text = data;
                }
            ));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            server.sendMessage(textBox1.Text);
        }
    }
}
