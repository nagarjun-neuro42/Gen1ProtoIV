using System;
using System.ComponentModel;


namespace n42_Robot_PROTO_III
{
    public partial class MainWindowUpdate : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** ROBOT & CONNECTION STATUS ***
        //-------------------------------------------------------------------------------------------------------------
        public string RobotStatus
        {
            get { return _robotstatus; }
            set
            {
                _robotstatus = value;
                OnPropertyChanged(nameof(RobotStatus));
            }
        }

        public string HandControllerConnectionImg
        {
            get { return _handControllerConnectionImg; }
            set
            {
                _handControllerConnectionImg = value;
                OnPropertyChanged(nameof(HandControllerConnectionImg));
            }
        }

        public string PLCControllerConnectionImg
        {
            get { return _plcControllerConnectionImg; }
            set
            {
                _plcControllerConnectionImg = value;
                OnPropertyChanged(nameof(PLCControllerConnectionImg));
            }
        }
        public string NIDAQConnection
        {
            get { return _nIDAQConnection; }
            set
            {
                _nIDAQConnection = value;
                OnPropertyChanged(nameof(NIDAQConnection));
            }
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** BUTTON ICONS IMAGE PATH ***
        //-------------------------------------------------------------------------------------------------------------
        public string StartButtonImagePath
        {
            get { return _startButtonImagePath; }
            set
            {
                _startButtonImagePath = value;
                OnPropertyChanged(nameof(StartButtonImagePath));
            }
        }
        public string StopButtonImagePath
        {
            get { return _stopButtonImagePath; }
            set
            {
                _stopButtonImagePath = value;
                OnPropertyChanged(nameof(StopButtonImagePath));
            }
        }
        public string HomeButtonImagePath
        {
            get { return _homeButtonImagePath; }
            set
            {
                _homeButtonImagePath = value;
                OnPropertyChanged(nameof(HomeButtonImagePath));
            }
        }
        public string CalibrateButtonImagePath
        {
            get { return _calibrateButtonImagePath; }
            set
            {
                _calibrateButtonImagePath = value;
                OnPropertyChanged(nameof(CalibrateButtonImagePath));
            }
        }
        public string MoveRobotButtonImagePath
        {
            get { return _moveRobotButtonImagePath; }
            set
            {
                _moveRobotButtonImagePath = value;
                OnPropertyChanged(nameof(MoveRobotButtonImagePath));
            }
        }

        public bool Btn_MoveRobot_Clicked { get; set; } = false;

        //-------------------------------------------------------------------------------------------------------------
        // *** ROBOT MODE (AUTO/MANUAL) & TOGGLE BUTTON STATUS ***
        //-------------------------------------------------------------------------------------------------------------
        public string RobotMode
        {
            get { return _robotMode; }
            set
            {
                _robotMode = value;
                OnPropertyChanged(nameof(RobotMode));
            }
        }

        public bool ToggleButtonIsEnabled
        {
            get { return _toggleButtonIsEnabled; }
            set
            {
                _toggleButtonIsEnabled = value;
                OnPropertyChanged(nameof(ToggleButtonIsEnabled));
            }

        }

        public string ToggleButtonColor
        {
            get { return _toggleButtonColor; }
            set
            {
                _toggleButtonColor = value;
                OnPropertyChanged(nameof(ToggleButtonColor));
            }

        }

        //-------------------------------------------------------------------------------------------------------------
        // *** ROBOT POSITION STATUS ***
        //-------------------------------------------------------------------------------------------------------------
        public bool IsHomeReached
        {
            get { return _isHomeReached; }
            set
            {
                _isHomeReached = value;
                OnPropertyChanged(nameof(IsHomeReached));
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** X-BOX POSITION ***
        //-------------------------------------------------------------------------------------------------------------
        public double RightThumbstick
        {
            get
            {
                return _rightThumbstick;
            }
            set
            {
                if (value == _rightThumbstick) return;
                _rightThumbstick = value;
                OnPropertyChanged();
            }
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** 3D VISUALIZATION POSITION ***
        //-------------------------------------------------------------------------------------------------------------
        public string DOF1_Angle
        {
            get { return Convert.ToString(_dof1_Angle); }
            set
            {
                _dof1_Angle = Convert.ToDouble(value);
                OnPropertyChanged(nameof(DOF1_Angle));
            }
        }

        public string DOF2_Angle
        {
            get { return Convert.ToString(_dof2_Angle); }
            set
            {
                _dof2_Angle = Convert.ToDouble(value);
                OnPropertyChanged(nameof(DOF2_Angle));
            }
        }

        public string Pot_Value
        {
            get { return _potvalue; }
            set
            {
                _potvalue = value;
                OnPropertyChanged(nameof(Pot_Value));
            }
        }

        public double XVPointer
        {
            get { return _xvPointer; }
            set
            {
                _xvPointer = value;
                OnPropertyChanged(nameof(XVPointer));
            }
        }

        public double XFPointer
        {
            get { return _xfPointer; }
            set
            {
                _xfPointer = value;
                OnPropertyChanged(nameof(XFPointer));
            }
        }
        public double YVPointer
        {
            get { return _yvPointer; }
            set
            {
                _yvPointer = value;
                OnPropertyChanged(nameof(YVPointer));
            }
        }
        public double YFPointer
        {
            get { return _yfPointer; }
            set
            {
                _yfPointer = value;
                OnPropertyChanged(nameof(YFPointer));
            }
        }

        public static int Count_Show_Workspace
        {
            get { return _count_Show_Workspace; }
            set
            {
                if (_count_Show_Workspace != value)
                {
                    _count_Show_Workspace = value;
                    OncSWChanged(nameof(Count_Show_Workspace));
                }
            }
        }

        static event PropertyChangedEventHandler cSWChanged = delegate { };
        static void OncSWChanged(string propertyName)
        {
            cSWChanged(
                typeof(MainWindowUpdate),
                new PropertyChangedEventArgs(propertyName));
        }
        
        void HandlecSWChange(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Count_Show_Workspace":
                    if (Count_Show_Workspace != _count_Show_Workspace++)
                        Count_Show_Workspace = _count_Show_Workspace++;
                    break;
            }
        }

    }
}
