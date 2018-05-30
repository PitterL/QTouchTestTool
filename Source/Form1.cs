using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;

namespace QTouch_UART_Tool
{
    public struct LimType
    {
        public ushort low;
        public ushort high;
    }

    public struct Dtype
    {
        public LimType reference;
        public LimType delta;
        public LimType cc;
    }

    //this class used for store test parameters from JSON format file
    public class Product
    {
        //Version indicate the tools format, also include JSON format parsing.
        public string version;

        //Flash firmware tools name
        public string tool;

        //Chip device name
        public string device;

        //Firmware flash interface
        public string interf;

        //Indicate whether do reference check
        public bool rCheck;

        //Indicate whether do delta check
        public bool dCheck;

        //Indicate how many keys will be checked
        public int btnCount;

        //Store the check limitation parameter
        public Dictionary<int, Dtype> tLimit;

        //default paramenter
        public Product()
        {
            //this.version = QTouch_UART_Tool.cVersion;
            this.version = "-";
            this.tool = "atmelice";
            this.device = "attiny816";
            this.interf = "updi";
            this.rCheck = true;
            this.dCheck = false;
            this.btnCount = 1;
            this.tLimit = new Dictionary<int, Dtype>();
            for(int i = 0; i < btnCount; i++)
            {
                this.tLimit.Add(i, new Dtype());
            }
        }
    }

    //Store the current button information data
    public enum NodeState { IDLE = 0, INIT, PASS, FAILED };
    public struct NodeData
    {
        public byte id;

        public int delta;
        public int dlimit_low;
        public int dlimit_high;
        public NodeState dstatus;

        public int reference;
        public int rlimit_low;
        public int rlimit_high;
        public NodeState rstatus;

        public int cc;
        public int climit_low;
        public int climit_high;
        public NodeState cstatus;
    }

    public partial class QTouch_UART_Tool : Form
    {
        //Indicate test product information
        public Product cTestProd;

        //Indicate current all buttion informations
        public NodeData[] cButtonNodes;

        //Indicate test result pass or failed
        public bool cFinalTestResult;

        //Indicate test error occure count
        public int cTestErrorCount;
        public const int cMaxTestErrorCount = 10;

        //Store test result string
        public Dictionary<string, string> cTestOutputSteam;

        //Max Button supported
        public const int cMaxTestNode = 255;

        //Current software version
        public const string cVersion = "1.2.5";

        //Load the test parameter information or create the new if not exist, the data is stored into a JSON file
        private Product CreatDatabase()
        {
            Product prod;
            string output;

            string configIni = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Assembly.GetCallingAssembly().GetName().Name + ".ini";
            if (File.Exists(configIni))
            {
                output = System.IO.File.ReadAllText(configIni);
                prod = JsonConvert.DeserializeObject<Product>(output);
                if (prod.btnCount > cMaxTestNode)
                    prod.btnCount = cMaxTestNode;

                if (prod.btnCount != prod.tLimit.Count())
                {
                    var t = prod.tLimit;
                    prod.tLimit = new Dictionary<int, Dtype>();
                    for (int i = 0; i < prod.btnCount; i++)
                    {
                        Dtype data;
                        if (!t.TryGetValue(i, out data))
                        {
                            data = new Dtype();
                        }
                        prod.tLimit.Add(i, data);
                    }
                }
            }
            else
            {
                prod = new Product();
            }

            if (prod.version != QTouch_UART_Tool.cVersion)
            {
                prod.version = QTouch_UART_Tool.cVersion;
                output = JsonConvert.SerializeObject(prod, Newtonsoft.Json.Formatting.Indented);
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(configIni, false))
                {
                    file.WriteLine(output);
                }
            }

            return prod;
        }

        public QTouch_UART_Tool()
        {
            //Form inlitialize
            InitializeComponent();

            //Pipe communication initialize
            PipeStart();
        }

        private void QTouch_UART_Tool_Load(object sender, EventArgs e)
        {
            cTestProd = CreatDatabase();

            string[] PortNames = SerialPort.GetPortNames();
            int I = PortNames.Length;
            for (int i = 0; i < I; i++)
            {
                comboBox_com_port.Items.Add(PortNames[i]);
            }
            serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPort1_DataReceivedInvoke);
            
            for (int i = 0; i < cTestProd.btnCount; i++)
            {
                this.dataGridView_Test.Rows.Add();
                this.dataGridView_Test.Rows[i].Cells[0].Value = "Button" + i.ToString();
                this.dataGridView_Test.Rows[i].Cells[1].Value = 0;
                this.dataGridView_Test.Rows[i].Cells[2].Value = cTestProd.tLimit[i].reference.low.ToString();
                this.dataGridView_Test.Rows[i].Cells[3].Value = cTestProd.tLimit[i].reference.high.ToString();
                this.dataGridView_Test.Rows[i].Cells[4].Value = 0;
                this.dataGridView_Test.Rows[i].Cells[5].Value = cTestProd.tLimit[i].delta.low.ToString();
                this.dataGridView_Test.Rows[i].Cells[6].Value = cTestProd.tLimit[i].delta.high.ToString();
                this.dataGridView_Test.Rows[i].Cells[7].Value = "";
            }


            label_result.Text = "Waiting Test";
            label_result.BackColor = Color.Gray;

            textBox_Tool.Text = cTestProd.tool;
            textBox_Device.Text = cTestProd.device;
            textBox_Interface.Text = cTestProd.interf;
            checkBox_reference.CheckState = cTestProd.rCheck ? CheckState.Checked : CheckState.Unchecked;
            checkBox_Delta.CheckState = cTestProd.dCheck ? CheckState.Checked : CheckState.Unchecked;
            
            button_open.Enabled = true;
            button_close.Enabled = false;
            button_test.Enabled = false;
            button_stop.Enabled = false;

            cTestOutputSteam = new Dictionary<string, string>();
            cButtonNodes = new NodeData[cTestProd.btnCount];

            //hander user to control test procss

            //start test
            NowTestingStartEvent += new EventHandler(TestingStartEvent);

            //receive button data information
            NowTestingProgressEvent += new EventHandler(TestingProgressEvent);

            //finished test
            NowTestingFinishedEvent += new EventHandler(TestingFinishedEvent);

            //handle error
            NowTestingErrorEvent += new EventHandler(TestingErrorEvent);
        }

        private void QTouch_UART_Tool_Close(object sender, FormClosingEventArgs e)
        {
            //PipeStop();
        }

        public event EventHandler NowTestingStartEvent;
        public event EventHandler NowTestingProgressEvent;
        public event EventHandler NowTestingFinishedEvent;
        public event EventHandler NowTestingErrorEvent;

        public class TestingEventArgs : EventArgs
        {
            public byte id { get; set; }
            public byte cmd { get; set; }
            public int data { get; set; }
        }

        private void SerialReset()
        {
            serialPort1.DiscardInBuffer();
            serialPort1.DiscardOutBuffer();
        }  

        private void SerialSendFrame(byte[] data)
        {
            byte crc_calc = calculate_crc(data, data.Count());
            byte[] raw = new byte[data.Count() + 1];

            Array.Copy(data, raw, data.Count());
            raw[data.Count()] = crc_calc;

            serialPort1.Write(data, 0, data.Count());
        }

        private bool checkResult(bool check, NodeState result)
        {
            if (!check)
                return true;
            else
                return result == NodeState.PASS;
        }

        private bool checkTested(bool check, NodeState result)
        {
            if (!check)
                return true;
            else
                return result > NodeState.INIT;
        }


        private delegate void NowResetCommand();
        private delegate void NowSendCommand(byte[] data);
        private void TestingStartEvent(object sender, EventArgs e)
        {
            int[] cells = new int[] { 1, 4, 7 };
            byte id = Convert.ToByte(sender);

            foreach (var cid in cells)
            {
                this.dataGridView_Test.Rows[id].Cells[cid].Value = "";
                this.dataGridView_Test.Rows[id].Cells[cid].Style.BackColor = Color.Gray;
            }

            ref NodeData node = ref cButtonNodes[id];
            node.rlimit_low = int.Parse(this.dataGridView_Test.Rows[id].Cells[2].Value.ToString());
            node.rlimit_high = int.Parse(this.dataGridView_Test.Rows[id].Cells[3].Value.ToString());
            node.dlimit_low = int.Parse(this.dataGridView_Test.Rows[id].Cells[5].Value.ToString());
            node.dlimit_high = int.Parse(this.dataGridView_Test.Rows[id].Cells[6].Value.ToString());

            NowResetCommand reset = new NowResetCommand(SerialReset);
            this.Invoke(reset);

            if (cTestProd.rCheck)
            {
                node.rstatus = NodeState.INIT;
                byte[] data = { 0x31, id };
                NowSendCommand cmd = new NowSendCommand(SerialSendFrame);
                this.Invoke(cmd, data);

            }

            if (cTestProd.dCheck)
            {
                node.dstatus = NodeState.INIT;
                byte[] data = { 0x33, id };
                NowSendCommand cmd = new NowSendCommand(SerialSendFrame);
                this.Invoke(cmd, data);
            }
        }

        private void TestingProgressEvent(object sender, EventArgs e)
        {
            byte id = Convert.ToByte(sender);
            TestingEventArgs evt = e as TestingEventArgs;

            if (id < cTestProd.btnCount)
            {
                ref NodeData node = ref cButtonNodes[id];
                node.id = id;

                if (evt.cmd == 0x31)
                {
                    node.reference = evt.data;
                }
                else if (evt.cmd == 0x33)
                {
                    node.delta = evt.data;
                }

                //name
                this.dataGridView_Test.Rows[id].Cells[0].Value = "Button" + id.ToString();

                if (cTestProd.rCheck)
                {
                    this.dataGridView_Test.Rows[id].Cells[1].Value = node.reference;
                    if (node.rlimit_low <= node.reference && node.rlimit_high > node.reference)
                    {
                        this.dataGridView_Test.Rows[id].Cells[1].Style.BackColor = Color.Green;
                        node.rstatus = NodeState.PASS;
                    }
                    else
                    {
                        this.dataGridView_Test.Rows[id].Cells[1].Style.BackColor = Color.Red;
                        node.rstatus = NodeState.FAILED;
                    }
                }

                if (cTestProd.dCheck)
                {
                    this.dataGridView_Test.Rows[id].Cells[4].Value = node.delta;
                    if (node.dlimit_low <= node.delta && node.dlimit_high > node.delta)
                    {
                        this.dataGridView_Test.Rows[id].Cells[4].Style.BackColor = Color.Green;
                        node.dstatus = NodeState.PASS;
                    }
                    else
                    {
                        this.dataGridView_Test.Rows[id].Cells[4].Style.BackColor = Color.Red;
                        node.dstatus = NodeState.FAILED;
                    }
                }

                if (checkResult(cTestProd.rCheck, node.rstatus) && checkResult(cTestProd.dCheck, node.dstatus))
                {
                    this.dataGridView_Test.Rows[id].Cells[7].Value = "PASS";
                    this.dataGridView_Test.Rows[id].Cells[7].Style.BackColor = Color.Green;
                }else
                {
                    this.dataGridView_Test.Rows[id].Cells[7].Value = "FAIL";
                    this.dataGridView_Test.Rows[id].Cells[7].Style.BackColor = Color.Red;
                }

                if (id < cTestProd.btnCount - 1) //next key
                    NowTestingStartEvent.Invoke(id + 1, new EventArgs());
                else
                {
                    if (checkTested(cTestProd.rCheck, node.rstatus) && checkTested(cTestProd.dCheck, node.dstatus))
                        NowTestingFinishedEvent.BeginInvoke(id, new EventArgs(), null, null);
                    //NowTestingFinishedEvent.Invoke(id, new EventArgs());
                }
            }
            else
            {

            }
        }

        private void TestingFinishedEvent(object sender, EventArgs e)
        {
            bool result = true;

            foreach (var node in cButtonNodes)
            {
                if (!(checkResult(cTestProd.rCheck, node.rstatus) && checkResult(cTestProd.dCheck, node.dstatus)))
                {
                    result = false;
                    break;
                }
            }

            cFinalTestResult = result;

            BtnPerformClick(this.button_stop);
        }

        private void TestingErrorEvent(object sender, EventArgs e)
        {
            byte id = Convert.ToByte(sender);
            TestingEventArgs evt = e as TestingEventArgs;

            cTestErrorCount++;
            if (cTestErrorCount < cMaxTestErrorCount)
            {
                NowTestingStartEvent.Invoke(id, new EventArgs());
            }
            else
            {
                NowTestingFinishedEvent.BeginInvoke(id, new EventArgs(), null, null);
            }
        }

        public byte crc8(byte crc, byte val)
        {
            byte fb;
            const byte CRC_PLOY = 0x8c;

            for (int i = 0; i < 8; i++)
            {
                fb = (byte)((crc ^ val) & 0x01);
                val >>= 1;
                crc >>= 1;
                if (fb != 0)
                    crc ^= CRC_PLOY;
            }

            return crc;
        }

        public byte calculate_crc(byte[] data, int len)
        {
            byte crc = 0;

            for(int i = 0; i < len; i++)
		        crc = crc8(crc, data[i]);

	        return crc;
        }

        private void serialPort1_DataReceivedInvoke(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            this.Invoke(new SerialDataReceivedEventHandler(serialPort1_DataReceived), new object[] { sender, e });
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] magicW = new byte[] { 0xa5, 0x5a };
            byte[] header = new byte[5];
            byte cmd, size, crc, crc_calc, id;
            //int[] buf;
            int value;

            string text;

            try {
                try
                {
                    serialPort1.Read(header, 0, header.Count());
                }
                catch (System.SystemException error)
                {
                    richTextBox_com.AppendText("Series Read failed 1: " + error.Message + "\n");
                }

                if (header[0] == magicW[0] && header[1] == magicW[1])
                {
                    cmd = header[2];
                    size = header[3];
                    crc = header[4];

                    crc_calc = calculate_crc(header, header.Count() - 1);
                    if (crc_calc != crc)
                        throw new System.ArgumentException("Serial read header crc excepetion", "crc: " + crc.ToString() + " crc calc: " + crc_calc.ToString());

                    byte[] val = new byte[size + 1];
                    try
                    {
                        serialPort1.Read(val, 0, val.Count());
                    }
                    catch (System.SystemException error)
                    {
                        richTextBox_com.AppendText("Series Read failed 2: " + error.Message + "\n");
                    }

                    crc = val[size];
                    crc_calc = calculate_crc(val, size);
                    if (crc_calc != crc)
                        throw new System.ArgumentException("Serial read body crc excepetion", "crc: " + crc.ToString() + " crc calc: " + crc_calc.ToString());

                    id = val[0];
                    if (id > cTestProd.btnCount)
                        throw new System.ArgumentException("Serial id excepetion", "Key index: " + id.ToString());

                    value = val[1] + val[2] * 256;
                    if (value > 32767)
                    {
                        value -= 65536;
                    }
                    text = "Button " + id.ToString() + " =  " + Convert.ToString(value, 10) + "\r\n";
                    RichTextSet(richTextBox_com, text, true);

                    TestingEventArgs evt = new TestingEventArgs { };
                    evt.id = id;
                    evt.cmd = cmd;
                    evt.data = value;
                    NowTestingProgressEvent.Invoke(id, evt);
                }
            } 
            catch(System.SystemException error)
            {
                richTextBox_com.AppendText("Series data crashed: " + error.Message + "\n");
                foreach(var node in cButtonNodes)
                {
                    if (node.rstatus <= NodeState.INIT) {
                        NowTestingErrorEvent.Invoke(node.id, new EventArgs());
                        break;
                    }
                }
            }
        }


        private void button_open_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = comboBox_com_port.Text;
                serialPort1.BaudRate = 38400;
                serialPort1.DtrEnable = true;
                serialPort1.Parity = Parity.None;
                serialPort1.StopBits = StopBits.One;
                serialPort1.ReadTimeout = 300;
                serialPort1.WriteTimeout = 300;
                serialPort1.Open();
                serialPort1.DiscardInBuffer();
                if (serialPort1.IsOpen == true)
                {
                    richTextBox_com.AppendText("Open Com Port Success!\n");
                    richTextBox_com.AppendText("Please Press Test Button!\n");
                    button_open.Enabled = false;
                    button_close.Enabled = true;
                    button_test.Enabled = true;
                }
            }
            catch
            {
                richTextBox_com.AppendText("Open Com Port Failed!\n");
            }
        }
        private void button_close_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Close();
                button_open.Enabled = true;
                button_close.Enabled = false;
                button_test.Enabled = false;
                if (serialPort1.IsOpen == false)
                {
                    richTextBox_com.AppendText("Close Com Port Success!\n");
                }
            }
            catch
            {
                richTextBox_com.AppendText("Close Com Port Failed!\n");
            }
        }

        private void button_test_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < cTestProd.btnCount; i++)
            {
                this.dataGridView_Test.Rows[i].Cells[1].Value = 0;
                this.dataGridView_Test.Rows[i].Cells[4].Value = 0;
            }

            button_open.Enabled = false;
            button_close.Enabled = false;
            button_test.Enabled = false;
            button_stop.Enabled = true;
            richTextBox_com.AppendText("Test Start, Please press the buttons!\n");
            label_result.Text = "Start Test";
            label_result.BackColor = Color.Gray;

            cFinalTestResult = false;
            cTestErrorCount = 0;
            Array.Clear(cButtonNodes, 0, cButtonNodes.Count());
            NowTestingStartEvent.Invoke((byte)0, new EventArgs());
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            richTextBox_com.AppendText("Button Test Process Stop!\n");
            Save_Result_To_Csv();
            if (cFinalTestResult == true)
            {
                label_result.Text = "PASS";
                label_result.BackColor = Color.Green;
            }
            else
            {
                label_result.Text = "FAIL";
                label_result.BackColor = Color.Red;
            }
            button_open.Enabled = true;
            button_close.Enabled = false;
            button_test.Enabled = false;
            button_stop.Enabled = false;

            try
            {
                if (serialPort1.IsOpen)
                    serialPort1.Close();
                if (serialPort1.IsOpen == false)
                {
                    richTextBox_com.AppendText("Close Com Port Success!\n");
                }
            }
            catch
            {
                richTextBox_com.AppendText("Close Com Port Failed!\n");
            }
        }

        public delegate string DataTabletoStr<DataTable> (DataTable t);
        private void Save_Result_To_Csv()
        {
            DataTable item = new DataTable("item");
            item.Columns.Add("Lines");
            for (int i = 0; i < cTestProd.btnCount * 10; i++)
            {
                item.Columns.Add("Line" + i.ToString());
            }
            item.Rows.Add("Items");
            item.Rows[0][0] = "Result";
            item.Rows[0][1] = "Time";
            for (int i = 0; i < cTestProd.btnCount; i++)
            {
                item.Rows[0][i * 8 + 2] = "Button_Index";
                item.Rows[0][i * 8 + 3] = "Button_Reference" + i.ToString();
                item.Rows[0][i * 8 + 4] = "Reference_Lowlimit" + i.ToString();
                item.Rows[0][i * 8 + 5] = "Reference_Highlimit" + i.ToString();
                item.Rows[0][i * 8 + 6] = "Button_Delta" + i.ToString();
                item.Rows[0][i * 8 + 7] = "Delta_Lowlimit" + i.ToString();
                item.Rows[0][i * 8 + 8] = "Delta_Highlimit" + i.ToString();
                item.Rows[0][i * 8 + 9] = "Button" + i.ToString() + "_Result";
            }

            DataTable result = new DataTable("result");
            result.Columns.Add("Lines");
            for (int i = 0; i < cTestProd.btnCount * 10; i++)
            {
                result.Columns.Add("Line" + i.ToString());
            }
            result.Rows.Add("Result");
            if (cFinalTestResult == true)
            {
                result.Rows[0][0] = "Good";
            }
            else
            {
                result.Rows[0][0] = "Bad";
            }
            result.Rows[0][1] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            for (int i = 0; i < cTestProd.btnCount; i++)
            {
                ref NodeData node = ref cButtonNodes[i];

                result.Rows[0][i * 8 + 2] = "Button" + i.ToString();
                result.Rows[0][i * 8 + 3] = node.reference;
                result.Rows[0][i * 8 + 4] = node.rlimit_low;
                result.Rows[0][i * 8 + 5] = node.rlimit_high;
                result.Rows[0][i * 8 + 6] = node.delta;
                result.Rows[0][i * 8 + 7] = node.dlimit_low;
                result.Rows[0][i * 8 + 8] = node.dlimit_high;
                if ((checkResult(cTestProd.rCheck, node.rstatus) && checkResult(cTestProd.dCheck, node.dstatus)))
                {
                    result.Rows[0][i * 8 + 9] = "Good";
                }
                else
                {
                    result.Rows[0][i * 8 + 9] = "Bad";
                }
            }

            string filename = string.Empty;
            filename = Directory.GetCurrentDirectory() + "/log.csv";
            if (!File.Exists(filename))
            {
                try
                {
                    using (FileStream fs = File.Create(filename))
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes("Test_");
                        fs.Write(info, 0, info.Length);

                    }
                    CsvLib.CsvStreamWriter CsvWriter = new CsvLib.CsvStreamWriter(filename);
                    CsvWriter.AddData(item, 1);
                    CsvWriter.Save();

                }
                catch
                {
                    richTextBox_com.AppendText("Creat log.csv Fail!\n");
                }
            }
            CsvLib.CsvStreamWriter CsvWriter1 = new CsvLib.CsvStreamWriter(filename);
            CsvWriter1.AddData(result, 1);
            CsvWriter1.Save();

            DataTabletoStr<DataTable> ToStr = tb => { return String.Join(Environment.NewLine, tb.Rows.OfType<DataRow>().Select(x => string.Join(" ; ", x.ItemArray))); };
            cTestOutputSteam["Title"] = ToStr(item);
            cTestOutputSteam["Data" ] = ToStr(result);
        }

        private void textBox_Tool_TextChanged(object sender, EventArgs e)
        {
            cTestProd.tool = textBox_Tool.Text.Trim();
        }

        private void textBox_Device_TextChanged(object sender, EventArgs e)
        {
            cTestProd.device = textBox_Device.Text;
        }

        private void textBox_Interface_TextChanged(object sender, EventArgs e)
        {
            cTestProd.interf = textBox_Interface.Text;
        }

        private void checkBox_reference_CheckedChanged(object sender, EventArgs e)
        {
            cTestProd.rCheck = checkBox_reference.CheckState == CheckState.Checked;
        }

        private void checkBox_Delta_CheckedChanged(object sender, EventArgs e)
        {
            cTestProd.dCheck = checkBox_Delta.CheckState == CheckState.Checked;
        }

        private void button_ProgramTestFW_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process program_testfw = new System.Diagnostics.Process();
            //program_testfw.StartInfo.FileName = Directory.GetCurrentDirectory() +  "/atprogram/atbackend/atprogram.exe";
            program_testfw.StartInfo.FileName = "atprogram.exe";
            program_testfw.StartInfo.Arguments = " -t " + cTestProd.tool + " -i " + cTestProd.interf + " -d " + cTestProd.device + " program " + "-c -fl -f " + "Test_FW.elf" + " verify -f "+ "Test_FW.elf";
            program_testfw.Start();
            program_testfw.WaitForExit();
            //string program_testfw_result = program_testfw.ExitCode.ToString();
            //richTextBox_com.AppendText(program_testfw_result + "\n");
            int program_testfw_result = program_testfw.ExitCode;
            if (program_testfw_result == 0)
            {
                label_program_result.Text = "Test FW Update PASS";
                label_program_result.BackColor = Color.Green;
            }
            else
            {
                label_program_result.Text = "Test FW Update FAIL";
                label_program_result.BackColor = Color.Red;
            }
            program_testfw.Close();

        }

        private void button_ProgramMPFW_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process program_mpfw = new System.Diagnostics.Process();
            //program_mpfw.StartInfo.FileName = Directory.GetCurrentDirectory() + "/atprogram/atbackend/atprogram.exe";
            program_mpfw.StartInfo.FileName = "atprogram.exe";
            program_mpfw.StartInfo.Arguments = " -t " + cTestProd.tool + " -i " + cTestProd.interf + " -d " + cTestProd.device + " program " + "-c -fl -f " + "MP_FW.elf" + " verify -f " + "MP_FW.elf";
            program_mpfw.Start();
            program_mpfw.WaitForExit();
            int program_mpfw_result = program_mpfw.ExitCode;
            if (program_mpfw_result == 0)
            {
                label_program_result.Text = "MP FW Update PASS";
                label_program_result.BackColor = Color.Green;
            }
            else
            {
                label_program_result.Text = "MP FW Update FAIL";
                label_program_result.BackColor = Color.Red;
            }
            program_mpfw.Close();
        }

        private void button_program_fw_and_bootloader_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process program_fw_and_bootloader = new System.Diagnostics.Process();
            //program_mpfw.StartInfo.FileName = Directory.GetCurrentDirectory() + "/atprogram/atbackend/atprogram.exe";
            program_fw_and_bootloader.StartInfo.FileName = "atprogram.exe";
            program_fw_and_bootloader.StartInfo.Arguments = " -t " + cTestProd.tool + " -i " + cTestProd.interf + " -d " + cTestProd.device + " chiperase program " + "-f " + "Bootloader.elf" + " verify -f " + "Bootloader.elf";
            program_fw_and_bootloader.StartInfo.Arguments += " program -f " + "MP_FW.hex" + " verify -f " + "MP_FW.hex";
            program_fw_and_bootloader.Start();
            program_fw_and_bootloader.WaitForExit();
            int program_fw_and_bootloader_result = program_fw_and_bootloader.ExitCode;
            if (program_fw_and_bootloader_result == 0)
            {
                label_program_result.Text = "Bootloader and FW Update PASS";
                label_program_result.BackColor = Color.Green;
            }
            else
            {
                label_program_result.Text = "Bootloader and FW Update FAIL";
                label_program_result.BackColor = Color.Red;
            }
            program_fw_and_bootloader.Close();
        }
    }
}