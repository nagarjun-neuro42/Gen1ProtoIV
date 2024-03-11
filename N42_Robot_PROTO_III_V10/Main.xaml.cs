using Microsoft.Win32;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using Windows.Devices.Printers;

//Nagarjun:
using NationalInstruments.DAQmx;

//using AmRoMessageDialog;

namespace n42_Robot_PROTO_III
{
    public partial class MainWindowUpdate : Window, INotifyPropertyChanged /*IDisposable*/
    {
        //Visualization_UserControl Visualization_UserControl;
        public MainWindowUpdate()
        {
            InitializeComponent();
            CopyResources();
            this.DataContext = this;
            cSWChanged += this.HandlecSWChange;
            //Visualization_UserControl = new Visualization_UserControl();

            _calibrateButtonImagePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\N42_Robot_PROTO_III_V10\\Resources\\calibrateBtn.png";
            _moveRobotButtonImagePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\N42_Robot_PROTO_III_V10\\Resources\\robot.png";            
            visualizationUserControl.SetTargetButtonClicked += VisualizationUserControl_SetTargetButtonClicked;
            visualizationUserControl.SetIK_TargetButtonClicked += VisualizationUserControl_SetIK_TargetButtonClicked;
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** INITIALIZE PRIVATE FIELDS ***
        //-------------------------------------------------------------------------------------------------------------

        // Constants for icon image paths
        private static string START_ENABLED = "/Resources/StartBtnActivated.png";
        private static string START_DISABLED = "/Resources/StartBtnDeactivated.png";
        private static string STOP_ENABLED = "/Resources/StopBtnActivated.png";
        private static string STOP_DISABLED = "/Resources/StopBtnDeactivated.png";
        private static string HOME_ENABLED = "/Resources/HomeBtnActivated.png";
        private static string HOME_DISABLED = "/Resources/HomeBtnDeactivated.png";
        private static string CONNECTED = "/Resources/GreenLED.png";
        private static string DISCONNECTED = "/Resources/GreyLED.png";

        // Robot & Connection Status
        private string _robotstatus = "DISCONNECTED";
        private string _handControllerConnectionImg = DISCONNECTED;
        private string _plcControllerConnectionImg = DISCONNECTED;
        private string _nIDAQConnection = "Disconnected";

        private bool MoCo_Connected;
        private bool Leo_Connected;
        private bool HC_Connected;
        private bool NIDAQ_Connected;

        private string MoCo_PortNumber;
        private string Leo_PortNumber;

        // Button properties
        public bool Btn_Stop_Clicked { get; set; } = false;
        public bool Btn_Connect_Clicked { get; set; } = false;
        public bool Btn_Start_Clicked { get; set; } = false;
        public bool Btn_Home_Clicked { get; set; } = false;
        public bool Btn_GoToPosition_Clicked { get; set; } = false;
        private string _stopButtonImagePath = "/Resources/StopBtnDeactivated.png";
        private string _startButtonImagePath = START_DISABLED;
        private string _homeButtonImagePath = "/Resources/HomeBtnDeactivated.png";
        private string _calibrateButtonImagePath = "";
        private string _moveRobotButtonImagePath = "";
        private string _toggleButtonColor = "#808285";
        private bool _toggleButtonIsEnabled = false;
        public bool calibrate = false;
        private static bool SetTargetisClicked = false;

        //For the Ik button
        private static bool SetIK_TargetisClicked = false;

        // Robot mode (auto/manual) & position
        private string _robotMode = "Manual";
        private bool _isHomeReached = false;
        public string targetPoint = "";

        //private double _dof1_Angle = 00.0;
        //private double _dof2_Angle = 00.0;
        private double _dof1_Angle = -40.0;
        private double _dof2_Angle = 50.17;
        private double _xvPointer;
        private double _yvPointer;
        private double _xfPointer;
        private double _yfPointer;
        private string _potvalue;

        // Switch motor bool
        private int switch_confirm = 0;

        // XBox position
        public XBox_XInputController XBox_connected { get; set; }
        public bool IsReading { get; set; } = false;
        private Controller _controller;
        private double _rightThumbstick;
        public Gamepad gamepad;
        public int LVY;
        public int RVX;
        public GamepadButtonFlags Y_Button;

        // Pulse
        private bool current_Pulse_M1;
        private bool current_Pulse_M2;
        private bool last_Pulse_M1;
        private bool last_Pulse_M2;

        //NI DAQ connection pulses:
        private DigitalSingleChannelReader last_Pulse;
        private DigitalSingleChannelReader current_Pulse;

        // Last pulse
        bool[] sensorL_Pulse_array;
        // Current pulse
        bool[] sensorC_Pulse_array;
        XBoxController controller = new XBoxController();

        // Pre-determined number of pulse needed to reach Point A and B
        public int POINT_B_DOF1 = 1500;
        public int POINT_B_DOF2 = 675;

        public int POINT_A_DOF1 = 300;
        public int POINT_A_DOF2 = 300;

        public int pointA_dof1_counter = 0;
        public int pointA_dof2_counter = 0;
        public int pointB_dof1_counter = 0;
        public int pointB_dof2_counter = 0;

        // Data & Port
        private Task ReadNITask;
        private static SerialPort sp_L = new SerialPort();
        private static SerialPort sp_R = new SerialPort();
        // Serial read initialization (Read happens when write is provided)
        private int serial_read_R = 0;
        private int serial_read_L = 0;

        // Timer & Others
        public System.Windows.Forms.Timer runTimer = new System.Windows.Forms.Timer();
        private string ResourcePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        public RelayCommand StopCommand { get; set; }
        public Func<double, string> Formatter { get; set; }

        private static int _count_Show_Workspace;

        //Calibrate counters
        private int calibrateR = 50;
        private int calibrateL = 100;

        //Joints angles
        private double joint1;
        private double joint2;
        Stack<Tuple<double, double>> setValues = new Stack<Tuple<double, double>>();

        //IK FinalJoints
        private double IKjoint1;
        private double IKjoint2;
        Stack<Tuple<double, double>> setIK_Values = new Stack<Tuple<double, double>>();

        //Default states:
        private double previousJ1 = -40;
        private double previousJ2 = 50.17;

        //Serial write counters
        public int counterR = 0;
        public int counterL = 0;

        //Optical Sensors  
        bool current_state_dof1, current_state_dof2;
        bool previous_state_dof1, previous_state_dof2;
        bool isfirstiteration = true;
        private int teeth_num_dof1 = 0; private int teeth_num_dof2 = 0;
        private int total_teeth_num_dof1 = 0; private int total_teeth_num_dof2 = 0;
        private int rotation_degree_dof1 = 0; private int rotation_degree_dof2 = 0;

        //-------------------------------------------------------------------------------------------------------------
        // *** Connection_Establish ***
        //-------------------------------------------------------------------------------------------------------------
        private void Connection_Establish()
        {
            /// <summary>
            /// Connecting to the Hand Controller through XBox_XInputController.cs
            /// </summary>
            XBox_XInputController _xbox_connected = new XBox_XInputController();
            if (_xbox_connected.XBox_connected)
            {
                //_ = MessageBox.Show(string.Format("The XBox controller is connected"));
                HC_Connected = true;
                HandControllerConnectionImg = CONNECTED;
                Debug.WriteLine("XBox controller connected!!");
            }
            else
            {
                IsReading = false;
            }

            if (MoCo_Connected && HC_Connected && Leo_Connected)
            {
                this.Btn_Stop.IsEnabled = true;
                this.Btn_Start.IsEnabled = true;
                this.Btn_Connect.IsEnabled = false;

            }


            /// <summary>
            /// Connecting to the Motion Controller (Arduino Mega2560) through COM port
            /// </summary>
            List<string> portname2 = ComPortNames("2341", "0042"); //Mega2560
                                                                   //List<string> portname2 = ComPortNames("2341", "003D"); //Due

            //string[] all_connected_stuff = SerialPort.GetPortNames();

            //foreach(string p in all_connected_stuff)
            //{
            //    Console.WriteLine(p + " ");
            //}

            //string[] array = serialport.getportnames();
            //for (int i = 0; i < array.length; i++)
            //{
            //    string p = array[i];
            //    console.writeline(p);
            //}

            //Debug.WriteLine(portname2);

            //foreach (var item in portname2)
            //{
            //    Console.WriteLine(item);
            //}



            if (portname2.Count > 0)
            {
                //foreach (String port2 in SerialPort.GetPortNames())
                //{
                //    if (portname2.Contains(port2))
                //    {
                //        //_ = MessageBox.Show(string.Format("The Mega Controller is + to: {0}", port2));
                //        MoCo_PortNumber = port2;
                //        MoCo_Connected = true;
                //        Debug.WriteLine("Mega controller connected!!");
                //    }
                //}
                MoCo_PortNumber = "COM11";
                MoCo_Connected = true;
                if (MoCo_Connected is false)
                {
                    MessageBoxImage icon = MessageBoxImage.Exclamation;
                    MessageBox.Show("Error! Mega Controller Not Found", "Motion Controller Error!", MessageBoxButton.OKCancel, icon);
                }
                else
                {
                    //MegaControllerConnection = "Connected";
                    Debug.WriteLine("Mega controller connected!!");
                }

                // This logic dosnt make any sense
                //else
                //{
                //    MessageBoxImage icon = MessageBoxImage.Exclamation;
                //    MessageBox.Show("Error! NI USB-6501 Not Found", "NI USB-6501 Scan Error!", MessageBoxButton.OKCancel, icon);
                //}


                //Nagarjun:
                // Create the reader
                // Reading the last data from the created channel
                ReadNITask = new Task();
                ReadNITask.DIChannels.CreateChannel("Dev1/Port2/line0:7", "", ChannelLineGrouping.OneChannelForAllLines);
                last_Pulse = new DigitalSingleChannelReader(ReadNITask.Stream);
                sensorL_Pulse_array = last_Pulse.ReadSingleSampleMultiLine();

                string[] devs = DaqSystem.Local.Devices;
                if (devs.Contains("Dev1") || devs.Contains("Dev2") || devs.Contains("Dev3"))
                {
                    NIDAQ_Connected = true;
                    NIDAQConnection = "Connected";
                    Debug.WriteLine("NI DAQ connected!!");

                    // Create a channel for each line P2.0 - P2.7 --> here lines 0 to 7
                    //DataReadTask.DIChannels.CreateChannel("Dev1/Port2/line0:7", "", ChannelLineGrouping.OneChannelForAllLines);
                    //Debug.WriteLine("Channel created");
                }
                else
                {
                    NIDAQ_Connected = false;
                    NIDAQConnection = "Disconnected";
                    MessageBoxImage icon = MessageBoxImage.Exclamation;
                    System.Windows.MessageBox.Show("Error! NI USB-6501 Not Found", "NI USB-6501 Scan Error!", MessageBoxButton.OKCancel, icon);
                }
            }


            /// <summary>
            /// Connecting to the Motion Controller (Arduino Leonardo) through COM port
            /// </summary>
            List<string> portname3 = ComPortNames("2341", "8036"); //Leonardo

            /*
            foreach (string p in SerialPort.GetPortNames())
            {
                Console.WriteLine(p);


            }
            */

            if (portname3.Count > 0)
            {
                // commented below temporarily for using this in future
                //foreach (String port3 in SerialPort.GetPortNames())
                //{
                //    if (portname3.Contains(port3))
                //    {
                //        //_ = MessageBox.Show(string.Format("The Leonardo Controller is connected to: {0}", port3));
                //        Leo_PortNumber = port3;
                //        Leo_Connected = true;
                //        Debug.WriteLine("Leonardo controller connected!!");
                //    }
                //}
                //Nagarjun
                Leo_PortNumber = "COM5";
                Leo_Connected = true;
                if (Leo_Connected is false)
                {
                    MessageBoxImage icon = MessageBoxImage.Exclamation;
                    MessageBox.Show("Error! Leonardo Controller Not Found", "Motion Controller Error!", MessageBoxButton.OKCancel, icon);
                }
                else
                {
                    //create a getter and setter:
                    //LeonardoControllerConnection = "Connected"; // previous code

                    Debug.WriteLine("Leonardo controller connected!!");
                }
            }
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** Read XBox Position: DOF_1 and DOF_2 ***
        //-------------------------------------------------------------------------------------------------------------     
        private void XBox_Position()
        {
            try
            {

                var guiDisp = Application.Current.Dispatcher;

                //Thumb Positions Left, Right
                controller.RightThumbstick.ValueChanged += (s, e) => guiDisp.Invoke(() =>
                {
                    //RightThumbPositionsCircle.Margin = new Thickness(180.0 * e.Value.X, -180.0 * e.Value.Y, 0.0, 20);
                    XBox360_RightTS.Margin = new Thickness(20.0 * e.Value.X+70, -20.0 * e.Value.Y+5, 0.0, 0);

                });

                controller.LeftThumbstick.ValueChanged += (s, e) => guiDisp.Invoke(() =>
                {
                    //LeftThumbPositionsCircle.Margin = new Thickness(180.0 * e.Value.X, -180.0 * e.Value.Y, 0.0, 20);
                    XBox360_LeftTS.Margin = new Thickness(20.0 * e.Value.X-155, -20.0 * e.Value.Y-90, 0.0, 0);
                });

            }

            catch
            {
                if (!IsReading)
                {
                    MessageBox.Show("Error in Reading DOF_1 and DOF_2 Positions");
                }
            }
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** COM port Names ***
        //-------------------------------------------------------------------------------------------------------------
        private List<string> ComPortNames(String VID, String PID)
        {
            String pattern = String.Format("^VID_{0}.PID_{1}", VID, PID);
            Regex _rx = new Regex(pattern, RegexOptions.IgnoreCase);
            List<string> comports = new List<string>();
            RegistryKey rk1 = Registry.LocalMachine;
            RegistryKey rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");
            foreach (String s3 in rk2.GetSubKeyNames())
            {
                RegistryKey rk3 = rk2.OpenSubKey(s3);
                foreach (String s in rk3.GetSubKeyNames())
                {
                    if (_rx.Match(s).Success)
                    {
                        RegistryKey rk4 = rk3.OpenSubKey(s);
                        foreach (String s2 in rk4.GetSubKeyNames())
                        {
                            RegistryKey rk5 = rk4.OpenSubKey(s2);
                            RegistryKey rk6 = rk5.OpenSubKey("Device Parameters");
                            comports.Add((string)rk6.GetValue("PortName"));
                        }
                    }
                }
            }
            return comports;
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** Copy Resources to ProgramData ***
        //-------------------------------------------------------------------------------------------------------------
        private void CopyResources()
        {
            var aaa = AssemblyDirectory.Length;
            string destinationFolder = ResourcePath + @"\\Resources_N42\\";
            if (!System.IO.Directory.Exists(destinationFolder))
            {
                //Get the files from the source folder.
                string[] Resource_files = System.IO.Directory.GetFiles(AssemblyDirectory);
                Directory.CreateDirectory(destinationFolder);

                // Copy the files if destination files do not already exist.
                foreach (string _file in Resource_files)
                {
                    string fileName = System.IO.Path.GetFileName(_file);
                    string destFile = System.IO.Path.Combine(destinationFolder, fileName);
                    System.IO.File.Copy(_file, destFile, true);
                }
            }
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** Assembly Directory Path ***
        //-------------------------------------------------------------------------------------------------------------
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri_Assembly = new UriBuilder(codeBase);         // Gives the path in URI format
                string Assembly_path = Uri.UnescapeDataString(uri_Assembly.Path);    // removes the "File://" at the beginning
                string result = System.IO.Path.GetDirectoryName(Assembly_path); // Canges the path to normal windows format

                int index = result.LastIndexOf("\\", System.StringComparison.InvariantCulture);
                return $"{result}\\Resources";
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            MessageBoxImage icon = MessageBoxImage.Warning;
            if (MessageBox.Show("Are You Sure You Want To Exit?", "", MessageBoxButton.OKCancel, icon) == MessageBoxResult.OK)
            {

                Environment.Exit(0);
            }

        }

    }
}


