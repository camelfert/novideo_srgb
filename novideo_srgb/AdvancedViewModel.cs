using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using EDIDParser;

namespace novideo_srgb
{
    public class AdvancedViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MonitorData _monitor;

        private double _redScaler;
        private double _greenScaler;
        private double _blueScaler;
        private int _target;
        private bool _useIcc;
        private string _profilePath;
        private bool _calibrateGamma;
        private int _selectedGamma;
        private double _customGamma;
        private double _customPercentage;
        private bool _disableOptimization;
        private bool _linearScaleSpace;
        private bool _customContrastSet;
        private int _customContrast;


        private int _ditherState;
        private int _ditherMode;
        private int _ditherBits;

        public AdvancedViewModel()
        {
            throw new NotSupportedException();
        }

        public AdvancedViewModel(MonitorData monitor, Novideo.DitherControl dither)
        {
            _monitor = monitor;

            _target = monitor.Target;
            _useIcc = monitor.UseIcc;
            _profilePath = monitor.ProfilePath;
            _calibrateGamma = monitor.CalibrateGamma;
            _selectedGamma = monitor.SelectedGamma;
            _customGamma = monitor.CustomGamma;
            _customPercentage = monitor.CustomPercentage;
            _disableOptimization = monitor.DisableOptimization;
            _ditherBits = dither.bits;
            _ditherMode = dither.mode;
            _ditherState = dither.state;
            _redScaler = monitor.RedScaler;
            _greenScaler = monitor.GreenScaler;
            _blueScaler = monitor.BlueScaler;
            _linearScaleSpace = monitor.LinearScaleSpace;
            _customContrast = monitor.CustomContrast;
            _customContrastSet = monitor.CustomContrastSet;
        }

        public void ApplyChanges()
        {
            ChangedCalibration |= _monitor.CustomContrastSet != _customContrastSet;
            _monitor.CustomContrastSet = _customContrastSet;
            ChangedCalibration |= _monitor.CustomContrast != _customContrast;
            _monitor.CustomContrast = _customContrast;
            ChangedCalibration |= _monitor.Target != _target;
            _monitor.Target = _target;
            ChangedCalibration |= _monitor.UseIcc != _useIcc;
            _monitor.UseIcc = _useIcc;
            ChangedCalibration |= _monitor.ProfilePath != _profilePath;
            _monitor.ProfilePath = _profilePath;
            ChangedCalibration |= _monitor.CalibrateGamma != _calibrateGamma;
            _monitor.CalibrateGamma = _calibrateGamma;
            ChangedCalibration |= _monitor.SelectedGamma != _selectedGamma;
            _monitor.SelectedGamma = _selectedGamma;
            ChangedCalibration |= _monitor.CustomGamma != _customGamma;
            _monitor.CustomGamma = _customGamma;
            ChangedCalibration |= _monitor.CustomPercentage != _customPercentage;
            _monitor.CustomPercentage = _customPercentage;
            ChangedCalibration |= _monitor.DisableOptimization != _disableOptimization;
            _monitor.DisableOptimization = _disableOptimization;
            ChangedCalibration |= _monitor.RedScaler != _redScaler;
            _monitor.RedScaler = _redScaler;
            ChangedCalibration |= _monitor.GreenScaler != _greenScaler;
            _monitor.GreenScaler = _greenScaler;
            ChangedCalibration |= _monitor.BlueScaler != _blueScaler;
            _monitor.BlueScaler = _blueScaler;
            ChangedCalibration |= _monitor.LinearScaleSpace != _linearScaleSpace;
            _monitor.LinearScaleSpace = _linearScaleSpace;
        }

        public ChromaticityCoordinates Coords => _monitor.Edid.DisplayParameters.ChromaticityCoordinates;

        public bool UseEdid
        {
            set
            {
                if (!value == _useIcc) return;
                _useIcc = !value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UseIcc));
                OnPropertyChanged(nameof(EdidWarning));
            }
            get => !_useIcc;
        }

        public bool UseIcc
        {
            set
            {
                if (value == _useIcc) return;
                _useIcc = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UseEdid));
                OnPropertyChanged(nameof(EdidWarning));
            }
            get => _useIcc;
        }

        public string ProfilePath
        {
            set
            {
                if (value == _profilePath) return;
                _profilePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProfileName));
            }
            get => _profilePath;
        }

        public string ProfileName => Path.GetFileName(ProfilePath);

        public bool CalibrateGamma
        {
            set
            {
                if (value == _calibrateGamma) return;
                _calibrateGamma = value;
                OnPropertyChanged();
            }
            get => _calibrateGamma;
        }

        public bool LinearScaleSpace
        {
            set
            {
                if (value == _linearScaleSpace) return;
                _linearScaleSpace = value;
                OnPropertyChanged();
            }
            get => _linearScaleSpace;
        }

        public bool CustomContrastSet
        {
            set
            {
                if (value == _customContrastSet) return;
                _customContrastSet = value;
                OnPropertyChanged();
            }
            get => _customContrastSet;
        }

        public int CustomContrast
        {
            set
            {
                if (value == _customContrast) return;
                _customContrast = value;
                OnPropertyChanged();
            }
            get => _customContrast;
        }

        public double ScaledRedX => Colorimetry.D65.X + (Colorimetry.ColorSpaces[Target].Red.X - Colorimetry.D65.X) * RedScaler / 100;

        public double ScaledRedY => Colorimetry.D65.Y + (Colorimetry.ColorSpaces[Target].Red.Y - Colorimetry.D65.Y) * RedScaler / 100;

        public double ScaledGreenX => Colorimetry.D65.X + (Colorimetry.ColorSpaces[Target].Green.X - Colorimetry.D65.X) * GreenScaler / 100;

        public double ScaledGreenY => Colorimetry.D65.Y + (Colorimetry.ColorSpaces[Target].Green.Y - Colorimetry.D65.Y) * GreenScaler / 100;

        public double ScaledBlueX => Colorimetry.D65.X + (Colorimetry.ColorSpaces[Target].Blue.X - Colorimetry.D65.X) * BlueScaler / 100;

        public double ScaledBlueY => Colorimetry.D65.Y + (Colorimetry.ColorSpaces[Target].Blue.Y - Colorimetry.D65.Y) * BlueScaler / 100;


        public int SelectedGamma
        {
            set
            {
                if (value == _selectedGamma) return;
                _selectedGamma = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UseCustomGamma));
            }
            get => _selectedGamma;
        }

        public Visibility UseCustomGamma =>
            SelectedGamma == 2 || SelectedGamma == 3 ? Visibility.Visible : Visibility.Collapsed;

        public double CustomGamma
        {
            set
            {
                if (value == _customGamma) return;
                _customGamma = value;
                OnPropertyChanged();
            }
            get => _customGamma;
        }

        public int Target
        {
            set
            {
                if (value == _target) return;
                _target = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EdidWarning));
                OnPropertyChanged(nameof(ScaledRedX));
                OnPropertyChanged(nameof(ScaledRedY));
                OnPropertyChanged(nameof(ScaledGreenX));
                OnPropertyChanged(nameof(ScaledGreenY));
                OnPropertyChanged(nameof(ScaledBlueX));
                OnPropertyChanged(nameof(ScaledBlueY));
            }
            get => _target;
        }


        public double RedScaler
        {
            set
            {
                // return early if no updated is needed.
                if (value == RedScaler)
                {
                    return;
                }
                _redScaler = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ScaledRedX));
                OnPropertyChanged(nameof(ScaledRedY));
                OnPropertyChanged(nameof(ScaledGreenX));
                OnPropertyChanged(nameof(ScaledGreenY));
                OnPropertyChanged(nameof(ScaledBlueX));
                OnPropertyChanged(nameof(ScaledBlueY));
            }
            get => _redScaler;
        }

        public double GreenScaler
        {
            set
            {
                // return early if no updated is needed.
                if (value == GreenScaler)
                {
                    return;
                }
                _greenScaler = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ScaledRedX));
                OnPropertyChanged(nameof(ScaledRedY));
                OnPropertyChanged(nameof(ScaledGreenX));
                OnPropertyChanged(nameof(ScaledGreenY));
                OnPropertyChanged(nameof(ScaledBlueX));
                OnPropertyChanged(nameof(ScaledBlueY));

            }
            get => _greenScaler;
        }

        public double BlueScaler
        {
            set
            {
                // return early if no updated is needed.
                if (value == BlueScaler)
                {
                    return;
                }
                _blueScaler = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ScaledRedX));
                OnPropertyChanged(nameof(ScaledRedY));
                OnPropertyChanged(nameof(ScaledGreenX));
                OnPropertyChanged(nameof(ScaledGreenY));
                OnPropertyChanged(nameof(ScaledBlueX));
                OnPropertyChanged(nameof(ScaledBlueY));

            }
            get => _blueScaler;
        }

        public Visibility HdrWarning => _monitor.HdrActive ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EdidWarning => HdrWarning != Visibility.Visible && UseEdid && Colorimetry.ColorSpaces[_target].Equals(_monitor.EdidColorSpace)
            ? Visibility.Visible
            : Visibility.Collapsed;

        public double CustomPercentage
        {
            set
            {
                if (value == _customPercentage) return;
                _customPercentage = value;
                OnPropertyChanged();
            }
            get => _customPercentage;
        }

        public bool DisableOptimization
        {
            set
            {
                if (value == _disableOptimization) return;
                _disableOptimization = value;
                OnPropertyChanged();
            }
            get => _disableOptimization;
        }

        public bool ChangedCalibration { get; set; }

        public int DitherState
        {
            set
            {
                if (value == _ditherState) return;
                _ditherState = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CustomDither));
                OnPropertyChanged(nameof(DitherMode));
                OnPropertyChanged(nameof(DitherBits));
                ChangedDither = true;
            }
            get => _ditherState;
        }

        public int DitherMode
        {
            set
            {
                if (value == _ditherMode) return;
                _ditherMode = value;
                OnPropertyChanged();
                ChangedDither = true;
            }
            get => _ditherState == 0 ? -1 : _ditherMode;
        }

        public int DitherBits
        {
            set
            {
                if (value == _ditherBits) return;
                _ditherBits = value;
                OnPropertyChanged();
                ChangedDither = true;
            }
            get => _ditherState == 0 ? -1 : _ditherBits;
        }

        public bool CustomDither => DitherState == 1;

        public bool ChangedDither { get; set; }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}