using PCSC;
using PCSC.Exceptions;
using PCSC.Iso7816;
using PCSC.Monitoring;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PCSC;
using PCSC.Iso7816;
using PCSC.Exceptions;


namespace NFC
{
    public partial class Form1 : Form

    {
        private NfcCardReader cardReader = null;
        private delegate void AddListBoxDelegate(string cardId);
        public Form1()
        {
            InitializeComponent();
        }

        public void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        public void Form1_Load(object sender, EventArgs e)
        {
            this.cardReader = new NfcCardReader();
            this.cardReader.CardRead += CardReader_CardRead;
            this.cardReader.InitializeCardReader();
            dataGridView.AllowUserToResizeColumns = true;
            dataGridView.AllowUserToResizeRows = false;
            dataGridView.RowHeadersVisible = false;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dataGridView.Columns.Add("seq", "連番");
            dataGridView.Columns.Add("datetime", "読み取り日時");
            dataGridView.Columns.Add("uid", "UID/IDm");

            dataGridView.Columns["seq"].Width = 80;
            dataGridView.Columns["datetime"].Width = 200;
            dataGridView.Columns["uid"].Width = 250;
        }

                            var bytesReceived = rfidReader.Transmit(
                                sendPci, // Protocol Control Information (T0, T1 or Raw)
                                command, // command APDU
                                command.Length,
                                receivePci, // returning Protocol Control Information
                                receiveBuffer,
                                receiveBuffer.Length); // data buffer

        private void AddListBox(string cardId)
                            {
            if (string.IsNullOrEmpty(cardId))
                            {
                return;
                }
            int seq = dataGridView.Rows.Count + 1;
            dataGridView.Rows.Add(seq, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), cardId);
                }

        private void pictureBox1_Click(object sender, EventArgs e)
                {
                    MessageBox.Show(ex.Message, "スマートカードエラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
                    {
            this.cardReader?.StopMonitor();
                    }

        private void CardReader_CardRead(string cardId)
                    {
            Invoke(new AddListBoxDelegate(this.AddListBox), cardId);
            }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
            {

            }
            comboBoxDevice.SelectedIndex = 0;

            // データグリッドビューの初期設定
            dataGridView.AllowUserToResizeColumns = true;
            dataGridView.AllowUserToResizeRows = false;
            dataGridView.RowHeadersVisible = false;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

            dataGridView.Columns["seq"].Width = 80;
            dataGridView.Columns["datetime"].Width = 200;
            dataGridView.Columns["uid"].Width = 250;
        }
    }
}
