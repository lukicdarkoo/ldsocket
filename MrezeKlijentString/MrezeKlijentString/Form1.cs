using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LDSocket;

namespace MrezeKlijentString
{
    public partial class Form1 : Form
    {
        LDSocket.SocketClient klijent = new LDSocket.SocketClient("192.168.10.56");

        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            klijent.ip = textBox2.Text;
            klijent.connect();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            klijent.sendMessage(textBox1.Text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            klijent.DataReceived += new SocketClient.DataReceivedHandler(klijent_DataReceived);
        }

        void klijent_DataReceived(string data)
        {
            Invoke(new MethodInvoker(
                 delegate
                 {
                     label1.Text = data;
                 }
             ));
        }
    }
}
