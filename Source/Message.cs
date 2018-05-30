using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Pipes;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Data;

namespace QTouch_UART_Tool
{
    class Logger
    {
        private FileStream  _file;
        private StreamWriter _log;

        public Logger(string sFileName = null)
        {
            if (sFileName == null)
                sFileName = DateTime.Today.ToString("yymmdd") + ".log";

            _file = new FileStream(sFileName, FileMode.Append, FileAccess.Write);
            _log = new StreamWriter(_file);

            Write(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss:"));
        }

        ~Logger()
        {
            if (_file != null)
                _file.Close();
        }

        public void Write(string value)
        {
            _log.WriteLine(value);
            _log.Flush();
        }

    }

    public partial class QTouch_UART_Tool
    {
        const string pipeName = "QTouch Ctrl Pipe";
        private Thread _receiveDataThread;
        private bool _threadExit = false;
        int _intervalExitTimeout = 100; //milliseconds

        private Logger Log;
        private string _pipeStatus;
        //private Dictionary<string, string> _pipeResponse;

        private void SetPipeStatus(string state, bool do_log=true)
        {
            _pipeStatus = state;

            if (do_log)
                Log.Write(state);
        }

        private string GetPipeStatus()
        {
            return _pipeStatus;
        }
        
        //call this function to enable pipe communication
        private void PipeStart()
        {
            Log = new Logger();
            _receiveDataThread = new Thread(new ThreadStart(ReceiveDataFromClient));
            _receiveDataThread.IsBackground = true;
            _receiveDataThread.Start();
        }
        
        private List<KeyValuePair<String, String>> GetForms()
        {
            Dictionary<string, string> forms = new Dictionary<string, string>();
            foreach (var ctl in this.Controls)
            {
                Type t = ctl.GetType();
                if (t.Equals(typeof(System.Windows.Forms.Button)))
                {
                    Button btn = ctl as Button;
                    forms.Add(btn.Name, "Button");
                }
                else if (t.Equals(typeof(System.Windows.Forms.Label)))
                {
                    Label lab = ctl as Label;
                    forms.Add(lab.Name, "Label");
                }
            }

            List<KeyValuePair<String, String>> myList = forms.ToList();
            myList.Sort(
                delegate (KeyValuePair<String, String> pair0, KeyValuePair<String, String> pair1)
                {
                    return pair0.Value.CompareTo(pair1.Value);
                });

            return myList;
        }

        private bool BtnPerformClick(Button btn)
        {
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            // If these threads are different, it returns true.  
            //Action<Button> click = b => b.PerformClick();
            Func<Button, bool> click = delegate (Button b)
            {
                if (b.Enabled)
                {
                    b.PerformClick();
                    return true;
                }
                else
                    return false;
            };

            if (btn.InvokeRequired)
            {
                return (bool) this.Invoke(click, new object[] { btn });
            }
            else
            {
                return click(btn);
            }
        }

        private bool LabelSetText(Label lb, string text)
        {
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            Func<Label, string, bool> set = delegate (Label l, string t)
            {
                if (l.Enabled)
                {
                    if (String.IsNullOrEmpty(t))
                        l.Text = "";
                    else
                        l.Text = t;
                    return true;
                }
                else
                    return false;
            };

            if (lb.InvokeRequired)
            {
                return (bool)this.Invoke(set, new object[] { lb, text });
            }
            else
            {
                return set(lb, text);
            }
        }

        private string LabelGetText(Label lb)
        {
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            Func<Label, string> get = delegate (Label l)
            {
                return l.Text;
            };

            if (lb.InvokeRequired)
            {
                return (string)this.Invoke(get, new object[] { lb });
            }
            else
            {
                return get(lb);
            }
        }

        private bool RichTextSet(RichTextBox rtx, string text, bool do_scroll=false)
        {
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            Func<RichTextBox, string, bool, bool> set = delegate (RichTextBox r, string t, bool scroll)
            {
                if (r.Enabled)
                {
                    if (text == null)
                        r.Clear();
                    else
                    {
                        r.AppendText(text);
                        if (scroll)
                            r.ScrollToCaret();
                    }
                    return true;
                }
                else
                    return false;
            };

            if (rtx.InvokeRequired)
            {
                return (bool)this.Invoke(set, new object[] { rtx, text, do_scroll });
            }
            else
            {
                return set(rtx, text, do_scroll);
            }
        }

        //main function to received message from ohter process
        private void ReceiveDataFromClient()
        {
            NamedPipeServerStream pipe;
            string status;

            while (!_threadExit)
            {

                using (pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 2))
                {
                    SetPipeStatus("Init");
                    try
                    {            
                        pipe.WaitForConnection(); //Waiting  

                        if (_threadExit)
                            break;

                        SetPipeStatus("Connected");

                        StreamReader sr = new StreamReader(pipe);
                        StreamWriter sw = new StreamWriter(pipe);

                        while (pipe.IsConnected)
                        {
                            string recData = sr.ReadLine();
                            if (String.IsNullOrWhiteSpace(recData))
                                continue;

                            SetPipeStatus("Message: " + recData);
                            Log.Write(recData);

                            string[] msgs = recData.Split(new char[] { ' ' }, 6).Select(p => p.Trim()).ToArray();
                            SetPipeStatus("NAK", false);

                            string seq = msgs[0];
                            bool seqIsNum = Regex.IsMatch(seq, @"^[\d]+.?[\d]+$"); //is Number

                            if (seqIsNum)
                            {
                                if (msgs.Length >= 2)
                                {
                                    string evt = msgs[1];
                                    if (evt == "Event")
                                    {
                                        string typ = msgs[2];
                                        if (typ == "Button" && msgs.Length >= 5)
                                        {
                                            try
                                            {
                                                string name = msgs[3];
                                                string action = msgs[4];
                                                Button btn = this.Controls.Find(name, true).FirstOrDefault() as Button;
                                                if (btn != null)
                                                {
                                                    if (action == "Click")
                                                    {
                                                        bool res = BtnPerformClick(btn);
                                                        if (res)
                                                        {
                                                            SetPipeStatus("ACK");
                                                        }
                                                    }
                                                }
                                            }
                                            catch (System.ArgumentException e)
                                            {
                                                Console.WriteLine("Error parsing msgs {0}, error = {1}", msgs, e.Message);
                                                SetPipeStatus("error: " + e.Message);
                                            }
                                        }
                                        else if (typ == "Label" && msgs.Length >= 5)
                                        {
                                            try
                                            {
                                                string name = msgs[3];
                                                string action = msgs[4];
                                                Label lab = this.Controls.Find(name, true).FirstOrDefault() as Label;
                                                if (lab != null)
                                                {
                                                    if (action == "Get")
                                                    {
                                                        SetPipeStatus(LabelGetText(lab));
                                                    }
                                                    else if (action == "Set" && msgs.Length >= 6)
                                                    {
                                                        if (LabelSetText(lab, msgs[6]))
                                                            SetPipeStatus("ACK");
                                                    }
                                                }
                                            }
                                            catch (System.ArgumentException e)
                                            {
                                                Console.WriteLine("Error parsing msgs {0}, error = {1}", msgs, e.Message);
                                                SetPipeStatus("error: " + e.Message);
                                            }
                                        }
                                        else if (typ == "Result" && msgs.Length >= 4)
                                        {
                                            string action = msgs[3];
                                            if (action == "Clr")
                                            {
                                                foreach (var ctl in this.Controls)
                                                {
                                                    Type t = ctl.GetType();
                                                    if (t.Equals(typeof(System.Windows.Forms.RichTextBox)))
                                                    {
                                                        RichTextBox rtx = ctl as RichTextBox;
                                                        RichTextSet(rtx, null);
                                                    }
                                                }
                                                cTestOutputSteam.Clear();
                                                SetPipeStatus("ACK");
                                            }
                                            else
                                            {
                                                string result;
                                                if (cTestOutputSteam.TryGetValue(action, out result))
                                                {
                                                    if (!String.IsNullOrWhiteSpace(result))
                                                        SetPipeStatus(result);
                                                }
                                            }
                                        }
                                        else if (typ == "Form" && msgs.Length >= 4)
                                        {
                                            string action = msgs[3];
                                            if (action == "List")
                                            {
                                                var forms = GetForms();
                                                SetPipeStatus(String.Join(",", forms));
                                            }
                                        }
                                        else
                                        {
                                            //Command Type Error
                                        }
                                    }
                                    else
                                    {
                                        // Command Event Error
                                    }
                                }
                                else
                                {
                                    //Ping command
                                    SetPipeStatus("ACK");
                                }

                                sw.WriteLine(seq + " " + GetPipeStatus());
                                sw.Flush();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex.Message);
                        //pipe.Disconnect();
                    }

                    Thread.Sleep(500);
                }
            }
        }

        //call this function to disable pipe communication
        public void PipeStop()
        {
            Log.Write("Pipe Exit");

            _threadExit = true;
       
            bool status = _receiveDataThread.Join(_intervalExitTimeout);
            if (!status)
            {
                _receiveDataThread.Abort();
            }
        }
    }
}
