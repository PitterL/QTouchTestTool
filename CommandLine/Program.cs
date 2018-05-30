using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.IO.Pipes;
using System.Security.Principal;
using NDesk.Options;

namespace QTController
{
    public struct ArgOptions
    {
        public string evt;
        public string typ;
        public string name;
        public string op;
        public string value;
        public int verbosity;
        public bool showHelp;

        public ArgOptions(bool sHelp)
        {
            //action = null;
            evt = null;
            typ = null;
            name = null;
            op = null;
            value = null;
            verbosity = 0;
            showHelp = sHelp;
        }
    }

    class Program
    {

        static private ArgOptions parse_options(string[] args)
        {
            ArgOptions opts = new ArgOptions(false);

            var p = new OptionSet() {
                { "e|event=", "Set the event content.\n",
                   v => opts.evt = v },
                { "t|type=", "Set the type content.\n",
                   v => opts.typ = v },  
                { "n|name=", "set the name to be performed\n",
                   v => opts.name = v },
                { "o|op=", "Set the action operation\n",
                   v => opts.op = v },
                { "v|value", "Set action",
                   v => opts.value = v },
                { "verbose", "increase debug message verbosity",
                   v => { if (v != null) ++opts.verbosity; } },
                { "h|help",  "show this message and exit",
                   v => {opts.showHelp = true; } },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);  //return un handled args
                if (extra.Count > 0)
                    Console.WriteLine("Unhandled args: " + String.Join(" ", extra));
            }
            catch (OptionException e)
            {
                Console.Write("greet: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `greet --help' for more information.");
            }
            
            if (opts.showHelp)
            {
                ShowHelp(p);
            }
            
            return opts;
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: greet [OPTIONS]+ message");
            Console.WriteLine("Greet a list of individuals with an optional message.");
            Console.WriteLine("If no message is specified, a generic greeting is used.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        /*
        static void Debug(string format, params object[] args)
        {
            if (verbosity > 0)
            {
                Console.Write("# ");
                Console.WriteLine(format, args);
            }
        }*/

        static void Main(string[] args)
        {
            // args = "-t form -o list".Split();
            // args = "-n button_open -o click".Split();
            // args = "-t result -o data".Split();

            ArgOptions opts = parse_options(args);
            if (!String.IsNullOrEmpty(opts.op))
            {
                Controller ctl = new Controller(opts);
                ctl.PipeStart();
                ctl.PipeStop();
            }
        }
    }
}

namespace QTController
{
    class Logger
    {
        private FileStream _file;
        private StreamWriter _log;

        public Logger(string sFileName = null)
        {
            if (sFileName == null)
                sFileName = DateTime.Today.ToString("yymmdd") + ".log";

            _file = new FileStream(sFileName, FileMode.Append, FileAccess.Write);
            _log = new StreamWriter(_file);

            Write("-------------------------------------------");
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

    public partial class Controller
    {
        const string pipeName = "QTouch Ctrl Pipe";
        private Thread _receiveDataThread;
        int _intervalExitTimeout = 3000; //milliseconds
        
        private Logger Log;

        private string _pipeStatus;
        private ArgOptions cmdOpts;
        //private List<string> cmds;

        public Controller(ArgOptions opts)
        {
            this.cmdOpts = opts;
            _pipeStatus = "";
        }

        private void SetPipeStatus(string state, bool console=false)
        {
            _pipeStatus = state;

            Log.Write(state);
            if (console)
                Console.WriteLine(state);
        }

        private string GetPipeStatus()
        {
            return _pipeStatus;
        }

        
        private Dictionary<string, string> GetPipeForms(StreamWriter sw, StreamReader sr)
        {
            string cmd = "Event Form List";
            string msg = "";
            Dictionary<string, string> formTable = new Dictionary<string, string>();

            bool result = PipeSendData(sw, cmd, sr, ref msg);
            if (result)
            {
                if (!msg.StartsWith("NAK"))
                {
                    string newvalue = msg.Replace("[", "").Replace("]", "");
                    string[] pairs = newvalue.Split(',').Select(p => p.Trim()).ToArray();
                    if (pairs.Length > 2)
                        //formTable = pairs.ToDictionary(c => c.Split(',')[0], y => y.Split(',')[1]);
                        for(int i = 0; i < pairs.Length; i += 2)
                        {
                            formTable[pairs[i]] = pairs[i + 1];
                        }
                }
            }

            return formTable;
        }

        delegate string FirstCharToUpperOnly(string input);
        private string ArgsToPipeCmd(Dictionary<string, string> formTable, ArgOptions cmdOpts)
        {
            List<string> raw = new List<string>();
            FirstCharToUpperOnly UpperFirst = input =>
            {
                if (String.IsNullOrEmpty(input))
                    throw new ArgumentException("ARGH!");
                return input.First().ToString().ToUpper() + input.Substring(1).ToLower();
            };


            if (!String.IsNullOrEmpty(cmdOpts.evt))
            {
                raw.Add(UpperFirst(cmdOpts.evt));
            }
            else
            {
                raw.Add("Event");
            }

            if (!String.IsNullOrEmpty(cmdOpts.typ))
            {
                raw.Add(UpperFirst(cmdOpts.typ));
            }

            if (!String.IsNullOrEmpty(cmdOpts.name))
            {
                if (formTable.ContainsKey(cmdOpts.name))
                {
                    string type = formTable[cmdOpts.name];
                    if (String.IsNullOrEmpty(cmdOpts.typ))
                        raw.Add(type);
                    raw.Add(cmdOpts.name);
                }
            }

            if (!String.IsNullOrEmpty(cmdOpts.op))
            {
                raw.Add(UpperFirst(cmdOpts.op));
            }

            return String.Join(" ", raw.ToArray());
        }

        public void PipeStart()
        {
            Log = new Logger();

            _receiveDataThread = new Thread(new ThreadStart(ReceiveDataFromClient));
            _receiveDataThread.IsBackground = true;
            _receiveDataThread.Start();
        }

        private bool PipeSendData(StreamWriter sw, string cmd, StreamReader sr, ref string output)
        {
            string seq = DateTime.Now.ToString("HHmmss");

            SetPipeStatus("Command: " + cmd);
            sw.WriteLine(seq + " " + cmd);
            sw.Flush();

            string recData = sr.ReadLine();
            SetPipeStatus("Message: " + recData);


            string[] msgs = recData.Split(new char[] { ' ' }, 2);
            if (msgs.Length >= 2)
            {
                string seq2 = msgs[0];
                string data = msgs[1];

                if (seq == seq2)
                {
                    SetPipeStatus("Result: " + data);
                    output = data;
                    return true;
                }
            }

            return false;
        }

        private void ReceiveDataFromClient()
        {
            NamedPipeClientStream pipe;

            using (pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation))
            {
                try
                {
                    SetPipeStatus("Init");

                    pipe.Connect(); //Waiting  
                    SetPipeStatus("Connected");
                    
                    StreamReader sr = new StreamReader(pipe);
                    StreamWriter sw = new StreamWriter(pipe);

                    //string cmd = "Event Btn button_test";
                    //string cmd = "Event Btn button_open";
                    var forms = GetPipeForms(sw, sr);
                    var cmd = ArgsToPipeCmd(forms, this.cmdOpts);
                    if (!String.IsNullOrEmpty(cmd))
                    {
                        string seq = DateTime.Now.ToString("HHmmss");
                        sw.WriteLine(seq + " " + cmd);
                        sw.Flush();
                        SetPipeStatus("Command: " + cmd);

                        string recData = sr.ReadLine();
                        SetPipeStatus("Message: " + recData);

                        string[] msgs = recData.Split(new char[] { ' ' }, 2);
                        if (msgs.Length >= 2)
                        {
                            string seq_get = msgs[0];
                            string data = msgs[1];

                            if (seq == seq_get)
                            {
                                SetPipeStatus("Result: " + data);
                            }
                        }
                    }else
                    {
                        SetPipeStatus("Could covert args to cmd", true);
                    }
                    //Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Log.Write(ex.Message);
                }
            }
        }

        public void PipeStop()
        {
            bool status = _receiveDataThread.Join(_intervalExitTimeout);
            if (!status)
            {
                _receiveDataThread.Abort();
            }

            string result = GetPipeStatus();
            Console.WriteLine(result);

            if (result.Contains("ACK") || result.Contains("NAK"))
            {
                if (result.EndsWith("ACK") && !result.Contains("NAK"))
                    SetPipeStatus("Pass", true);
            }
            else
            {
                if (result.Contains("Good") && !result.Contains("Bad"))
                    SetPipeStatus("Pass", true);
            }
        }
    }
}