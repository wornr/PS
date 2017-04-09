using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using PS.Model;
using Timer = System.Timers.Timer;
using Type = PS.Model.Type;

namespace PS.Services {
    public class FtpService {
        private Socket _controlConnection;
        private Socket _dataConnection;

        private StreamWriter _controlWriter;
        private StreamReader _controlReader;
        private StreamReader _dataReader;

        private string _passiveModeHost;
        private int _passiveModePort;

        private Timer _timer;
        private bool _helper;
        private string _actualDir;

        public bool Connected { get; set; }
        public string Response { get; set; }

        public FtpService(string host, int port, string username, string password, bool keepAlive = true) {
            _actualDir = "/";
            Connect(host, port, username, password, keepAlive);
        }

        public void Connect(string host, int port, string username, string password, bool keepAlive = true) {
            _helper = host.Contains("cba");
            // Establish Control Connection
            _controlConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            _controlConnection.Connect(host, port);

            var controlStream = new NetworkStream(_controlConnection);

            _controlWriter = new StreamWriter(controlStream) {AutoFlush = true};
            _controlReader = new StreamReader(controlStream);

            _controlWriter.WriteLine($"USER {username}");
            _controlWriter.WriteLine($"PASS {password}");
            Thread.Sleep(500);
            ReadResponse(_controlReader);

            if (keepAlive) {
                _timer = new Timer(30 * 1000);
                _timer.Elapsed += OnTimedEvent;
                _timer.AutoReset = true;
                _timer.Start();
            }

            Connected = true;
        }

        public void Disconnect() {
            if (Connected) {
                _controlWriter.WriteLine("QUIT");

                _controlReader.Close();
                _controlWriter.Close();
                _controlConnection.Close();
                _timer?.Stop();
                Connected = false;
            }
        }

        public List<Dir> List() {
            return List(_actualDir);
        }

        public List<Dir> List(string dir) {
            if(!Connected)
                throw new NotConnectedException("Connection to FTP server is not established");

            EstablishDataConnection();
            _controlWriter.WriteLine($"MLSD {dir}");

            ReadResponseLine(_controlReader);
            ReadResponseLine(_controlReader);
            if (_helper)
                ReadResponseLine(_controlReader);

            var response = ReadResponse(_dataReader);
            _dataReader.Close();
            _dataConnection.Close();

            return ParseList(response);
        }

        public string Pwd() {
            if(!Connected)
                throw new NotConnectedException("Connection to FTP server is not established");

            _controlWriter.WriteLine("PWD");
            _actualDir = ReadResponseLine(_controlReader).Split('"')[1];

            return _actualDir;
        }

        public string Cdup() {
            if(!Connected)
                throw new NotConnectedException("Connection to FTP server is not established");

            _controlWriter.WriteLine("CDUP");
            ReadResponseLine(_controlReader);

            return Pwd();
        }

        public string Cwd(string dir) {
            if(!Connected)
                throw new NotConnectedException("Connection to FTP server is not established");

            _controlWriter.WriteLine($"CWD {dir}");
            ReadResponseLine(_controlReader);

            return Pwd();
        }

        public string Noop() {
            if(!Connected)
                throw new NotConnectedException("Connection to FTP server is not established");
            
            _controlWriter.WriteLine("NOOP");
            
            return ReadResponseLine(_controlReader);
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e) {
            Console.WriteLine(Noop());
        }

        private List<Dir> ParseList(string list) {
            var structure = new List<Dir>();

            var elements = list.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            if (elements.Length != 0) {
                foreach (var element in elements) {
                    Dir elementDir = new Dir();

                    var informations = element.Split(';');
                    if (informations.Length != 0) {
                        if (string.IsNullOrEmpty(informations[0]))
                            break;

                        foreach (var info in informations) {
                            var parameters = info.Split('=');
                            switch (parameters.Length) {
                                case 1:
                                    elementDir.Name = parameters[0].Trim();
                                    break;
                                case 2:
                                    switch (parameters[0]) {
                                        case "type":
                                            if ("dir".Equals(parameters[1]))
                                                elementDir.Type = Type.Directory;
                                            else if ("file".Equals(parameters[1]))
                                                elementDir.Type = Type.File;
                                            break;
                                        case "modify":
                                            elementDir.ModifiedDate = parameters[1];
                                            break;
                                        case "size":
                                            elementDir.Size = parameters[1];
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                    if (elementDir.Type != Type.Unknown)
                        structure.Add(elementDir);
                }
            }

            return structure;
        }

        public List<Dir> RecursiveList(string dir, Dir parent = null) {
            var list = List(dir);

            if (list.Count != 0)
                foreach (var element in list) {
                    if (element.Type == Type.Directory)
                        element.Children = RecursiveList(dir + element.Name + "/", element);
                }

            return list;
        }

        private void EstablishDataConnection() {
            _controlWriter.WriteLine("PASV");
            var passiveModeData = ReadResponseLine(_controlReader);

            if(!string.IsNullOrEmpty(passiveModeData) && passiveModeData.Contains("227 Entering Passive Mode (")) {
                _passiveModeHost = GetPassiveModeHost(passiveModeData);
                _passiveModePort = GetPassiveModePort(passiveModeData);
            } else {
                Connected = false;
                return;
            }

            // Establish Data Connection
            _dataConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            _dataConnection.Connect(_passiveModeHost, _passiveModePort);

            var dataStream = new NetworkStream(_dataConnection);
            _dataReader = new StreamReader(dataStream);
        }

        private string ReadResponseLine(StreamReader reader) {
            return reader.ReadLine();
        }

        private string ReadResponse(StreamReader reader) {
            var response = new StringBuilder();
            while(reader.Peek() >= 0) {
                response.AppendLine(reader.ReadLine());
            }

            return response.ToString();
        }

        private string GetPassiveModeHost(string line) {
            var host = line.Substring(27, line.Length - 28).Split(',');

            return host[0] + "." + host[1] + "." + host[2] + "." + host[3];
        }

        private int GetPassiveModePort(string line) {
            var port = line.Substring(27, line.Length - 28).Split(',');

            return int.Parse(port[4]) * 256 + int.Parse(port[5]);
        }
    }
}