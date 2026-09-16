using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        public Form1()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var readerName = comboBoxDevice.Text;  // 選択中のカードリーダ名を得る

            using (var context = ContextFactory.Instance.Establish(SCardScope.System))
            {
                try
                {
                    using (var rfidReader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any))
                    {
                        // APDUコマンドの作成
                        var apdu = new CommandApdu(IsoCase.Case2Short, rfidReader.Protocol)
                        {
                            CLA = 0xFF,
                            Instruction = InstructionCode.GetData,
                            P1 = 0x00,
                            P2 = 0x00,
                            Le = 0 // We don't know the ID tag size
                        };
                        // 読み取りコマンド送信
                        using (rfidReader.Transaction(SCardReaderDisposition.Leave))
                        {
                            var sendPci = SCardPCI.GetPci(rfidReader.Protocol);
                            var receivePci = new SCardPCI(); // IO returned protocol control information.

                            var receiveBuffer = new byte[256];
                            var command = apdu.ToArray();

                            var bytesReceived = rfidReader.Transmit(
                                sendPci, // Protocol Control Information (T0, T1 or Raw)
                                command, // command APDU
                                command.Length,
                                receivePci, // returning Protocol Control Information
                                receiveBuffer,
                                receiveBuffer.Length); // data buffer

                            var responseApdu = new ResponseApdu(receiveBuffer, bytesReceived, IsoCase.Case2Short, rfidReader.Protocol);
                            if (responseApdu.HasData)
                            {
                                // バイナリ文字列の整形
                                StringBuilder id = new StringBuilder(BitConverter.ToString(responseApdu.GetData()));
                                id.Replace("-", string.Empty);
                                // データグリッドビューに結果を追加し、連番をカウントアップする
                                int seq = dataGridView.Rows.Count + 1;
                                DateTime dt = DateTime.Now;
                                dataGridView.Rows.Add(seq, dt.ToString("yyyy/MM/dd HH:mm:ss"), id.ToString());
                            }
                            else
                            {
                                MessageBox.Show("このカードではIDを取得できません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
                catch (RemovedCardException)
                {
                    MessageBox.Show("スマートカードが取り外されたため、通信できません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                catch (PCSCException ex)
                {
                    MessageBox.Show(ex.Message, "スマートカードエラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // リーダーの機器情報を取得
            try
            {
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                {
                    var readerNames = context.GetReaders();
                    if (readerNames == null || readerNames.Length == 0)
                    {
                        MessageBox.Show("スマートカードリーダが見つかりません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        Application.Exit();  // スマートカードリーダが無ければ終了する
                    }
                    foreach (var readerName in readerNames)
                    {
                        comboBoxDevice.Items.Add(readerName);  // カードリーダの名前をコンボボックスに追加
                    }
                }
            }
            catch (NoServiceException)
            {
                MessageBox.Show("スマートカードリソースマネージャが稼働していません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Application.Exit();
            }
            comboBoxDevice.SelectedIndex = 0;

            // データグリッドビューの初期設定
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
    }
}
