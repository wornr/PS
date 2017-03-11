using GalaSoft.MvvmLight.Command;

using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS.ViewModel.Pages {
    public class Lab1ViewModel : BaseViewModel {
        private const string Base64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

        private string _filePath;
        private string _fileName;
        private bool _isActive;

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

        public bool IsActive {
            get { return _isActive; }
            set {
                _isActive = value;
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

            try {
                fileBytes = File.ReadAllBytes(FilePath);
            } catch {
                DisplayDialog("Błąd", "Nie udało się otworzyć wybranego pliku");
                return;
            }

            var sb = new StringBuilder(fileBytes.Length * 4 / 3);

            IsActive = true;
            await Task.Run(() => {
                for (var i = 0; i < fileBytes.Length; i += 3) {
                    // 0xFC = 11111100
                    var oneByte = (fileBytes[i] & 0xFC) >> 2;
                    sb.Append(Base64[oneByte]);
                    // 0x03 = 00000011
                    oneByte = (fileBytes[i] & 0x03) << 4;
                    if (i + 1 < fileBytes.Length) {
                        // 0xF0 = 11110000
                        oneByte |= (fileBytes[i + 1] & 0xF0) >> 4;
                        sb.Append(Base64[oneByte]);
                        // 0x0F = 00001111
                        oneByte = (fileBytes[i + 1] & 0x0F) << 2;
                        if (i + 2 < fileBytes.Length) {
                            // 0xC0 = 11000000
                            oneByte |= (fileBytes[i + 2] & 0xC0) >> 6;
                            sb.Append(Base64[oneByte]);
                            // 0x3F = 00111111
                            oneByte = fileBytes[i + 2] & 0x3F;
                            sb.Append(Base64[oneByte]);
                        } else {
                            sb.Append(Base64[oneByte]);
                            sb.Append('=');
                        }
                    } else {
                        sb.Append(Base64[oneByte]);
                        sb.Append("==");
                    }
                }
            });
            IsActive = false;

            try {
                File.WriteAllText(Application.StartupPath + Path.DirectorySeparatorChar + _fileName + ".b64", sb.ToString());
            } catch {
                DisplayDialog("Błąd", "Nie udało się zapisać pliku");
            }
        }

        public async void Decode() {
            string fileString;

            try {
                fileString = File.ReadAllText(FilePath);
            } catch {
                DisplayDialog("Błąd", "Nie udało się otworzyć wybranego pliku");
                return;
            }

            if(fileString.Length % 4 != 0) {
                
            }
            var decoded = new byte[fileString.Length * 3 / 4 - (fileString.IndexOf('=') > 0 ? fileString.Length - fileString.IndexOf('=') : 0)];
            var inChars = fileString.ToCharArray();
            var j = 0;
            var b = new int[4];

            IsActive = true;
            await Task.Run(() => {
                for (var i = 0; i < inChars.Length; i += 4) {
                    b[0] = Base64.IndexOf(inChars[i]);
                    b[1] = Base64.IndexOf(inChars[i + 1]);
                    b[2] = Base64.IndexOf(inChars[i + 2]);
                    b[3] = Base64.IndexOf(inChars[i + 3]);
                    decoded[j++] = (byte) ((b[0] << 2) | (b[1] >> 4));
                    if (b[2] < 64) {
                        decoded[j++] = (byte) ((b[1] << 4) | (b[2] >> 2));
                        if (b[3] < 64) {
                            decoded[j++] = (byte) ((b[2] << 6) | b[3]);
                        }
                    }
                }
            });
            IsActive = false;

            try {
                File.WriteAllBytes(Application.StartupPath + Path.DirectorySeparatorChar + _fileName.Substring(0, _fileName.Length - 4), decoded);
            } catch {
                DisplayDialog("Błąd", "Nie udało się zapisać pliku");
            }
        }
    }
}