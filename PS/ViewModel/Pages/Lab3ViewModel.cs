using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

using GalaSoft.MvvmLight.Command;

using PS.Model;

namespace PS.ViewModel.Pages {
    public class Lab3ViewModel : BaseViewModel {
        #region Variables
        private readonly Config _config;

        private string _mailFrom;
        private bool _isLoading;
        private bool _sendButtonState;

        private RelayCommand _sendCommand;

        public RelayCommand SendCommand => _sendCommand ?? (_sendCommand = new RelayCommand(SendMessage));

        public string MailFrom {
            get { return _mailFrom; }
            set {
                _mailFrom = value;
                RaisePropertyChanged();
            }
        }
        public bool IsLoading {
            get { return _isLoading; }
            set {
                _isLoading = value;
                RaisePropertyChanged();
            }
        }
        public bool SendButtonState {
            get { return _sendButtonState; }
            set {
                _sendButtonState = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public Lab3ViewModel() {
            SendButtonState = true;

            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            try {
                StreamReader reader = new StreamReader("config.xml");
                _config = (Config)serializer.Deserialize(reader);
                reader.Close();
            } catch {
                DisplayDialog("Błąd", "Wystąpił błąd podczas wczytywania konfiguracji poczty");
            }
        }

        private async void SendMessage() {
            using (var connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                connection.Connect(_config.Outgoing.Host, _config.Outgoing.Port);

                if (!connection.Connected) {
                    App.Current.Dispatcher.Invoke(() => {
                        DisplayDialog("Błąd", "Nie udało się nawiązać połączenia z serwerem");
                    });

                    return;
                }

                Stream stream;

                if (_config.Outgoing.SSL) {
                    var ssl = new SslStream(new NetworkStream(connection));
                    try {
                        ssl.AuthenticateAsClient(_config.Outgoing.Host);
                    } catch {
                        App.Current.Dispatcher.Invoke(() => {
                            DisplayDialog("Błąd", "Wystąpił błąd z połączeniem SSL");
                        });

                        return;
                    }

                    
                    stream = ssl;
                } else {
                    stream = new NetworkStream(connection);
                }

                SendButtonState = false;
                IsLoading = true;
                await Task.Run(() => {
                    using (var writer = new StreamWriter(stream)) {
                        using (var reader = new StreamReader(stream)) {
                            try {
                                writer.WriteLine($"HELO {_config.Outgoing.Host}");
                                writer.Flush();
                                Console.WriteLine(reader.ReadLine());

                                writer.WriteLine("AUTH LOGIN");
                                writer.Flush();
                                Console.WriteLine(reader.ReadLine());

                                writer.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(_config.Outgoing.Username)));
                                writer.Flush();
                                Console.WriteLine(reader.ReadLine());

                                writer.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(_config.Outgoing.Password)));
                                writer.Flush();
                                Console.WriteLine(reader.ReadLine());

                                writer.WriteLine($"MAIL FROM <{_config.Outgoing.MailAddress}>");
                                writer.Flush();
                                Console.WriteLine(reader.ReadLine());

                                writer.WriteLine($"RCPT TO <{_mailFrom}>");
                                writer.Flush();
                                Console.WriteLine(reader.ReadLine());

                                writer.WriteLine("DATA");
                                writer.WriteLine($"From: {_config.Outgoing.MailAddress}");
                                writer.WriteLine($"To: {_mailFrom}");
                                writer.WriteLine("Subject: PS LAB LATO 2017 14B");
                                writer.WriteLine();
                                writer.WriteLine("Marek Kamińśki");
                                writer.WriteLine(".");
                                writer.Flush();
                                Console.WriteLine(reader.ReadLine());
                                
                                writer.WriteLine("QUIT");
                                writer.Flush();
                                Console.WriteLine(reader.ReadLine());
                            } catch {
                                App.Current.Dispatcher.Invoke(() => {
                                    DisplayDialog("Błąd", "Wystąpił błąd podczas próby wysłania wiadomości");
                                });
                            }
                        }
                    }
                });
                IsLoading = false;
                SendButtonState = true;
            }
        }

        /*private void SendMessage() {
            using(var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                socket.Connect(_config.Outgoing.Host, _config.Outgoing.Port);
                var sslStream = new SslStream(new NetworkStream(socket));
                sslStream.AuthenticateAsClient(_config.Outgoing.Host);
                Task.Run(() => {
                    using (var streamWriter = new StreamWriter(sslStream)) {
                        using (var streamReader = new StreamReader(sslStream)) {
                            streamWriter.AutoFlush = true;

                            streamWriter.WriteLine($"HELO {_config.Outgoing.Host}");
                            streamWriter.WriteLine("AUTH LOGIN");
                            streamWriter.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(_config.Outgoing.Username)));
                            streamWriter.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(_config.Outgoing.Password)));
                            streamWriter.WriteLine($"MAIL FROM: <{_config.Outgoing.MailAddress}>");
                            streamWriter.WriteLine($"RCPT TO: <{_mailFrom}>");
                            streamWriter.WriteLine("DATA");
                            streamWriter.WriteLine($"From: {_config.Outgoing.MailAddress}\r\n" +
                                                   $"To: {_mailFrom}\r\n" +
                                                   "Subject: Test\r\n" +
                                                   "\r\n" +
                                                   "Zażółć gęślą jaźń\r\n" +
                                                   ".");
                            streamWriter.WriteLine("QUIT");
                            Console.WriteLine(streamReader.ReadLine());
                            Console.WriteLine(streamReader.ReadLine());
                            Console.WriteLine(streamReader.ReadLine());
                            Console.WriteLine(streamReader.ReadLine());
                            Console.WriteLine(streamReader.ReadLine());
                            Console.WriteLine(streamReader.ReadLine());
                            Console.WriteLine(streamReader.ReadLine());
                            Console.WriteLine(streamReader.ReadLine());
                            Console.WriteLine(streamReader.ReadLine());
                            Console.WriteLine(streamReader.ReadLine());

                        }
                    }
                });
            }
        }*/
    }
}