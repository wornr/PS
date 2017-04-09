using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using GalaSoft.MvvmLight.Command;
using PS.Model;
using PS.Services;

namespace PS.ViewModel.Pages {
    public class Lab4ViewModel : BaseViewModel {
        private Config _config;
        private FtpService _ftpService;

        private string _connectionStatusDescription = "Rozłączono";
        private Brush _connectionStatusColor = Brushes.Red;
        private List<Dir> _structure;
        private string _actualDir;

        private RelayCommand _connectCommand;
        private RelayCommand _disconnectCommand;
        private RelayCommand _listActualCommand;
        private RelayCommand _listAllCommand;
        private RelayCommand _cdupCommand;
        private RelayCommand _cwdCommand;

        public RelayCommand ConnectCommand => _connectCommand ?? (_connectCommand = new RelayCommand(Connect));
        public RelayCommand DisconnectCommand => _disconnectCommand ?? (_disconnectCommand = new RelayCommand(Disconnect));
        public RelayCommand ListActualCommand => _listActualCommand ?? (_listActualCommand = new RelayCommand(ListActual));
        public RelayCommand ListAllCommand => _listAllCommand ?? (_listAllCommand = new RelayCommand(ListAll));
        public RelayCommand CdupCommand => _cdupCommand ?? (_cdupCommand = new RelayCommand(Cdup));
        public RelayCommand CwdCommand => _cwdCommand ?? (_cwdCommand = new RelayCommand(Cwd));


        private void LoadConfig() {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            try {
                StreamReader reader = new StreamReader("config.xml");
                _config = (Config)serializer.Deserialize(reader);
                reader.Close();
            } catch {
                DisplayDialog("Błąd", "Wystąpił błąd podczas wczytywania konfiguracji serwera");
            }
        }

        private void Connect() {
            LoadConfig();

            _ftpService = new FtpService(_config.Ftp.Host, _config.Ftp.Port, _config.Ftp.Username, _config.Ftp.Password, _config.Ftp.KeepAlive);
            ActualDir = _ftpService.Pwd();

            if (_ftpService.Connected) {
                ConnectionStatusDescription = "Połączono";
                ConnectionStatusColor = Brushes.Green;

                ListActual();
            }
        }

        private void Disconnect() {
            _ftpService?.Disconnect();

            if (_ftpService == null || !_ftpService.Connected) {
                ConnectionStatusDescription = "Rozłączono";
                ConnectionStatusColor = Brushes.Red;
                Structure = new List<Dir>();
                ActualDir = string.Empty;
            }
        }

        private void ListActual() {
            try {
                Structure = _ftpService?.List();
            } catch(Exception e) {
                DisplayDialog("Błąd", e.Message);
            }
        }

        private void ListAll() {
            try {
                Structure = _ftpService?.RecursiveList("/");
            } catch(Exception e) {
                DisplayDialog("Błąd", e.Message);
            }
        }

        private void Cdup() {
            try {
                ActualDir = _ftpService?.Cdup();
                ListActual();
            } catch(Exception e) {
                DisplayDialog("Błąd", e.Message);
            }
        }

        private void Cwd() {
            try {
                ActualDir = _ftpService?.Cwd(ChangeDir);
                ListActual();
            } catch(Exception e) {
                DisplayDialog("Błąd", e.Message);
            }
        }

        public string ConnectionStatusDescription {
            get { return _connectionStatusDescription; }
            set {
                _connectionStatusDescription = value;
                RaisePropertyChanged();
            }
        }

        public Brush ConnectionStatusColor {
            get { return _connectionStatusColor; }
            set {
                _connectionStatusColor = value;
                RaisePropertyChanged();
            }
        }

        public List<Dir> Structure {
            get { return _structure; }
            set {
                _structure = value;
                RaisePropertyChanged();
            }
        }

        public string ActualDir {
            get { return _actualDir; }
            set {
                _actualDir = value;
                RaisePropertyChanged();
            }
        }

        public string ChangeDir { get; set; }
    }
}