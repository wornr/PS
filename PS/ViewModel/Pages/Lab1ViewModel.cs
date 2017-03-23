using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using GalaSoft.MvvmLight.Command;

namespace PS.ViewModel.Pages {
    public class Lab1ViewModel : BaseViewModel {
        private const int MaxPart = 10485759;
        private const string Base64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

        private string _filePath;
        private string _fileName;
        private int _fileSize;
        private int _progress;
        private bool _buttonState = true;

        private RelayCommand _openFileBrowserCommand;
        private RelayCommand _encodeCommand;
        private RelayCommand _decodeCommand;

        public string FilePath {
            get { return _filePath; }
            set {
                _filePath = value;
                RaisePropertyChanged();
            }
        }

        public int FileSize {
            get { return _fileSize; }
            set {
                _fileSize = value;
                RaisePropertyChanged();
            }
        }

        public int Progress {
            get { return _progress; }
            set {
                _progress = value;
                RaisePropertyChanged();
            }
        }

        public bool ButtonState {
            get { return _buttonState; }
            set {
                _buttonState = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand EncodeCommand => _encodeCommand ?? (_encodeCommand = new RelayCommand(Encode));
        public RelayCommand DecodeCommand => _decodeCommand ?? (_decodeCommand = new RelayCommand(Decode));

        public RelayCommand OpenFileBrowserCommand => _openFileBrowserCommand ?? (_openFileBrowserCommand = new RelayCommand(() => {
            var fileBrowserDialog = new OpenFileDialog();

            if (fileBrowserDialog.ShowDialog() == DialogResult.OK) {
                FilePath = fileBrowserDialog.FileName;
                _fileName = fileBrowserDialog.SafeFileName;
            }
        }));

        public async void Encode() {
            byte[] fileBytes;
            StringBuilder sb;

            
            try {
                fileBytes = File.ReadAllBytes(FilePath);
                FileSize = fileBytes.Length;
            } catch {
                DisplayDialog("Błąd", "Nie udało się otworzyć wybranego pliku");
                return;
            }

            var isNumberRound = FileSize - MaxPart * (FileSize / MaxPart) == 0;
            var partsCount = FileSize / MaxPart + (isNumberRound ? 0 : 1);
            var partBoundary = 0;
            
            ButtonState = false;
            await Task.Run(() => {
                File.WriteAllText(Application.StartupPath + Path.DirectorySeparatorChar + _fileName + ".b64", "");
                for (var i = 0; i < partsCount; i++) {
                    var partSize = MaxPart;

                    if (i == partsCount - 1 && !isNumberRound) {
                        partSize = FileSize - MaxPart * i;
                    }

                    partBoundary += partSize;
                    sb = new StringBuilder(partSize / 3 * 4);
                    for (var j = i * MaxPart; j < partBoundary; j += 3) {
                        // 0xFC = 11111100
                        var oneByte = (fileBytes[j] & 0xFC) >> 2;
                        sb.Append(Base64[oneByte]);
                        // 0x03 = 00000011
                        oneByte = (fileBytes[j] & 0x03) << 4;
                        if (j + 1 < FileSize) {
                            // 0xF0 = 11110000
                            oneByte |= (fileBytes[j + 1] & 0xF0) >> 4;
                            sb.Append(Base64[oneByte]);
                            // 0x0F = 00001111
                            oneByte = (fileBytes[j + 1] & 0x0F) << 2;
                            if (j + 2 < FileSize) {
                                // 0xC0 = 11000000
                                oneByte |= (fileBytes[j + 2] & 0xC0) >> 6;
                                sb.Append(Base64[oneByte]);
                                // 0x3F = 00111111
                                oneByte = fileBytes[j + 2] & 0x3F;
                                sb.Append(Base64[oneByte]);
                            } else {
                                sb.Append(Base64[oneByte]);
                                sb.Append('=');
                            }
                        } else {
                            sb.Append(Base64[oneByte]);
                            sb.Append("==");
                        }
                        if (j % 1000 == 0)
                            Progress = j;
                    }

                    try {
                        File.AppendAllText(Application.StartupPath + Path.DirectorySeparatorChar + _fileName + ".b64", sb.ToString());
                    } catch {
                        DisplayDialog("Błąd", "Wystąpił błąd podczas zapisu");
                        File.Delete(Application.StartupPath + Path.DirectorySeparatorChar + _fileName + ".b64");
                    }
                }
            });
            ButtonState = true;

            DisplayDialog("Sukces", "Kodowanie zostało pomyślnie zakończone");
        }

        public async void Decode() {
            byte[] fileBytes;

            try {
                fileBytes = File.ReadAllBytes(FilePath);
            } catch {
                DisplayDialog("Błąd", "Nie udało się otworzyć wybranego pliku");
                return;
            }

            if(fileBytes.Length % 4 != 0) {
                DisplayDialog("Błąd", "Plik nie zawiera poprawnego ciągu base64");
                return;
            }

            FileSize = fileBytes.Length;

            var offset = 0;
            if (Convert.ToChar(fileBytes[fileBytes.Length - 1]) == '=') {
                offset++;
                if (Convert.ToChar(fileBytes[fileBytes.Length - 2]) == '=')
                    offset++;
            }
                       
            var decoded = new byte[fileBytes.Length / 4 * 3 - offset];
            var j = 0;
            var b = new int[4];

            ButtonState = false;
            await Task.Run(() => {
                Progress = 0;
                for (var i = 0; i < fileBytes.Length; i += 4) {
                    b[0] = Base64.IndexOf(Convert.ToChar(fileBytes[i]));
                    b[1] = Base64.IndexOf(Convert.ToChar(fileBytes[i + 1]));
                    b[2] = Base64.IndexOf(Convert.ToChar(fileBytes[i + 2]));
                    b[3] = Base64.IndexOf(Convert.ToChar(fileBytes[i + 3]));
                    decoded[j++] = (byte) ((b[0] << 2) | (b[1] >> 4));
                    if (b[2] < 64) {
                        decoded[j++] = (byte) ((b[1] << 4) | (b[2] >> 2));
                        if (b[3] < 64) {
                            decoded[j++] = (byte) ((b[2] << 6) | b[3]);
                        }
                    }
                    if (i % 1000 == 0)
                        Progress = i;
                }
            });
            ButtonState = true;

            try {
                File.WriteAllBytes(Application.StartupPath + Path.DirectorySeparatorChar + _fileName.Substring(0, _fileName.Length - 4), decoded);
            } catch {
                DisplayDialog("Błąd", "Nie udało się zapisać pliku");
            }

            DisplayDialog("Sukces", "Dekodowanie zostało pomyślnie zakończone");
        }
    }
}