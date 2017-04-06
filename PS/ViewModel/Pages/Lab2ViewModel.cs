using System.Collections.ObjectModel;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media;
using System.Xml.Serialization;

using GalaSoft.MvvmLight.Command;

using PS.Model;

namespace PS.ViewModel.Pages {
    public class Lab2ViewModel : BaseViewModel {
        #region Variables
        private readonly Config _config;
        private Timer _timer;

        private string _status = "Rozłączono";
        private Brush _statusColor = Brushes.Red;
        private bool _isLoading;
        private bool _connectButtonState;
        private bool _disconnectButtonState;
        private int _allMessagesCounter;
        private int _newMessagesCounter;
        public ObservableCollection<Mail> Mails { get; set; }

        private RelayCommand _connectCommand;
        private RelayCommand _disconnectCommand;

        public RelayCommand ConnectCommand => _connectCommand ?? (_connectCommand = new RelayCommand(Connect));
        public RelayCommand DisconnectCommand => _disconnectCommand ?? (_disconnectCommand = new RelayCommand(Disconnect));

        public string Status {
            get { return _status; }
            set {
                _status = value;
                RaisePropertyChanged();
            }
        }
        public Brush StatusColor {
            get { return _statusColor; }
            set {
                _statusColor = value;
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
        public bool ConnectButtonState {
            get { return _connectButtonState; }
            set {
                _connectButtonState = value;
                RaisePropertyChanged();
            }
        }
        public bool DisconnectButtonState {
            get { return _disconnectButtonState; }
            set {
                _disconnectButtonState = value;
                RaisePropertyChanged();
            }
        }
        public int AllMessagesCounter {
            get { return _allMessagesCounter; }
            set {
                _allMessagesCounter = value;
                RaisePropertyChanged();
            }
        }
        public int NewMessagesCounter {
            get { return _newMessagesCounter; }
            set {
                _newMessagesCounter = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public Lab2ViewModel() {
            ConnectButtonState = true;
            Mails = new ObservableCollection<Mail>();

            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            try {
                StreamReader reader = new StreamReader("config.xml");
                _config = (Config)serializer.Deserialize(reader);
                reader.Close();
            } catch {
                DisplayDialog("Błąd", "Wystąpił błąd podczas wczytywania konfiguracji poczty");
            }
        }

        private void Connect() {
            ConnectButtonState = false;
            DisconnectButtonState = true;
            RetriveMessages(true);

            _timer = new Timer(_config.Incoming.Interval * 1000);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e) {
            RetriveMessages();
        }

        private async void RetriveMessages(bool initialization = false) {
            _timer?.Stop();
            using (var connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                connection.Connect(_config.Incoming.Host, _config.Incoming.Port);

                if (!connection.Connected) {
                    Disconnect();
                    App.Current.Dispatcher.Invoke(
                        delegate {
                            DisplayDialog("Błąd", "Nie udało się nawiązać połączenia z serwerem");
                        }
                    );
                        
                    return;
                }

                Status = "Połączono";
                StatusColor = Brushes.Green;

                StreamReader reader;
                StreamWriter writer;

                if (_config.Incoming.SSL) {
                    var ssl = new SslStream(new NetworkStream(connection));
                    try {
                        ssl.AuthenticateAsClient(_config.Incoming.Host);
                    } catch {
                        Disconnect();
                        App.Current.Dispatcher.Invoke(
                            delegate {
                                DisplayDialog("Błąd", "Wystąpił błąd z połączeniem SSL");
                            }
                        );

                        return;
                    }

                    reader = new StreamReader(ssl);
                    writer = new StreamWriter(ssl);
                } else {
                    reader = new StreamReader(new NetworkStream(connection));
                    writer = new StreamWriter(new NetworkStream(connection));
                }
                    

                IsLoading = true;
                await Task.Run(() => {
                    reader.ReadLine(); // Powitanie serwera
                        
                    // AUTHENTICATION mode
                    writer.WriteLine($"USER {_config.Incoming.Username}");
                    writer.Flush();
                    reader.ReadLine(); // Potwierdzenie odbioru loginu

                    writer.WriteLine($"PASS {_config.Incoming.Password}");
                    writer.Flush();
                    reader.ReadLine(); // potwierdzenie odbioru hasła
                        
                    // TRANSACTION mode
                    writer.WriteLine("UIDL");
                    writer.Flush();

                    string strTemp;
                    var line = 0;

                    while ((strTemp = reader.ReadLine()) != null) {
                        if (".".Equals(strTemp) || strTemp.IndexOf("-ERR") != -1) {
                            break;
                        }

                        if (line != 0) {
                            if (!Mails.Contains(new Mail(strTemp.Substring(0, strTemp.IndexOf(" ")), strTemp.Substring(strTemp.IndexOf(" ") + 1), false))) {
                                System.Windows.Application.Current.Dispatcher.Invoke(
                                    delegate {
                                        Mails.Add(new Mail(strTemp.Substring(0, strTemp.IndexOf(" ")), strTemp.Substring(strTemp.IndexOf(" ") + 1), !initialization));
                                    }
                                );
                                AllMessagesCounter++;
                                if (!initialization)
                                    NewMessagesCounter++;
                            }
                        } else
                            line++;
                    }

                    writer.WriteLine("QUIT");
                    writer.Flush();
                    reader.ReadLine(); // Potwierdzenie rozłączenia sesji
                });
                IsLoading = false;
            }
            _timer?.Start();
        }

        private void Disconnect() {
            _timer?.Stop();
            Status = "Rozłączono";
            StatusColor = Brushes.Red;
            ConnectButtonState = true;
            DisconnectButtonState = false;
        }
    }
}