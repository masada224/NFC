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

namespace NFC
{
    internal class NfcCardReader
    {

        public string CardReaderName = null;
        public ISCardContext context = null;
        public ISCardMonitor monitor = null;
        public event Action<string> CardRead;

        public void InitializeCardReader()
        {


            try
            {
                this.context = ContextFactory.Instance.Establish(SCardScope.System);
                var readerNames = context.GetReaders();
                if (readerNames == null || readerNames.Length == 0)
                {
                    throw new Exception("対象のICカードリーダーが見つかりません。");
                }
                this.CardReaderName = readerNames[0];
            }
            catch (NoServiceException nsex)
            {
                Debug.WriteLine($"カードエラー : {nsex.InnerException}");
                throw;
            }

            

            this.StartMonitor();
        }

        public void StartMonitor()
        {
            this.monitor = MonitorFactory.Instance.Create(SCardScope.System);

            // イベント登録
            this.monitor.StatusChanged += Monitor_StatusChanged;

            // 開始
            this.monitor.Start(CardReaderName);
        }

        public void Monitor_StatusChanged(object sender, StatusChangeEventArgs e)
        {
            Debug.WriteLine($"Status Changed New State : {e.NewState}, Last State : {e.LastState}");

            if (e != null && e.NewState == SCRState.Present && e.LastState == SCRState.Empty)
            {
                // ID取得
                var cardId = this.ReadCardId();
                CardRead?.Invoke(cardId);
            }
        }

        public string ReadCardId()
        {
            string cardId = string.Empty;

            ICardReader reader = null;
            try
            {
                reader = this.context.ConnectReader(CardReaderName, SCardShareMode.Shared, SCardProtocol.Any);
            }
            catch (Exception)
            {
                // Exceptionを握りつぶす
            }

            if (reader == null)
            {
                return cardId;
            }

            // APDUコマンドの作成
            var apdu = new CommandApdu(IsoCase.Case2Short, reader.Protocol)
            {
                CLA = 0xFF,
                Instruction = InstructionCode.GetData,
                P1 = 0x00,
                P2 = 0x00,
                Le = 0 // We don't know the ID tag size
            };

            // 読み取りコマンド送信
            using (reader.Transaction(SCardReaderDisposition.Leave))
            {
                var sendPci = SCardPCI.GetPci(reader.Protocol);
                var receivePci = new SCardPCI(); // IO returned protocol control information.

                var receiveBuffer = new byte[256];
                var command = apdu.ToArray();

                var bytesReceived = reader.Transmit(
                    sendPci, // Protocol Control Information (T0, T1 or Raw)
                    command, // command APDU
                    command.Length,
                    receivePci, // returning Protocol Control Information
                    receiveBuffer,
                    receiveBuffer.Length); // data buffer

                var responseApdu = new ResponseApdu(receiveBuffer, bytesReceived, IsoCase.Case2Short, reader.Protocol);
                if (responseApdu.HasData)
                {
                    // バイナリ文字列の整形
                    StringBuilder id = new StringBuilder(BitConverter.ToString(responseApdu.GetData()));
                    id.Replace("-", string.Empty);
                    cardId = id.ToString();
                }
                else
                {
                    Debug.WriteLine("ReadCardId:このカードではIDを取得できません");
                }
            }

            reader.Dispose();

            return cardId;
        }

        public void StopMonitor()
        {
            if (this.monitor == null)
            {
                return;
            }
            this.monitor.Cancel();
            this.monitor.StatusChanged -= Monitor_StatusChanged;
            this.monitor.Dispose();
            this.monitor = null;
        }
    }
}
