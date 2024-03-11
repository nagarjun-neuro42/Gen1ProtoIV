using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using NationalInstruments.DAQmx;

namespace n42_Robot_PROTO_III
{
    public partial class MainWindowUpdate
    {
        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE CONNECT BUTTON CLICK EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void Btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Btn_Connect_Clicked = true;
                this.Btn_Start_Clicked = false;
                this.MoCo_Connected = false;
                this.Leo_Connected = false;
                this.HC_Connected = false;
                this.NIDAQ_Connected = false;

                Connection_Establish();

                if (MoCo_Connected && HC_Connected && Leo_Connected && NIDAQ_Connected)
                {
                    // If all connections succeed, open serial ports for auduino
                    sp_R.PortName = MoCo_PortNumber;
                    sp_R.BaudRate = 57600;
                    // This is to perform serial reads
                    sp_R.DtrEnable = true;
                    sp_R.DataReceived += SerialPort_DataReceived_R;

                    sp_L.PortName = Leo_PortNumber;
                    sp_L.BaudRate = 57600;
                    // This is to perform serial reads
                    sp_L.DtrEnable = true;
                    sp_L.DataReceived += SerialPort_DataReceived_L;

                    // Enable toggle button & set robot status to "STAND BY"                   
                    ToggleButtonIsEnabled = true;
                    ToggleButtonColor = "#7FFFD4";

                    RobotStatus = "STAND BY";

                    // Enable Manual Mode
                    if (RobotMode == "Manual")
                    {
                        IsReading = true;
                        this.Btn_Start.IsEnabled = true;
                        this.Btn_Home.IsEnabled = false;

                        StartButtonImagePath = START_ENABLED;
                        Btn_Start.Cursor = System.Windows.Input.Cursors.Hand;
                        HomeButtonImagePath = HOME_DISABLED;

                        XBox_Position();
                    }
                    // Enable Auto Mode
                    else
                    {
                        this.Btn_Start.IsEnabled = false;
                        this.Btn_Stop.IsEnabled = false;
                        this.Btn_Home.IsEnabled = true;

                        HomeButtonImagePath = HOME_ENABLED;
                        Btn_Home.Cursor = System.Windows.Input.Cursors.Hand;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: Btn_Connect_Click " + ex.Message);
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE MICRO CONTROLLERS READS ***
        //-------------------------------------------------------------------------------------------------------------
        private void SerialPort_DataReceived_R(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp_r = (SerialPort)sender;
            // Check if there is data available in the serial port
            if (sp_r.BytesToRead > 0)
            {
                string data = sp_r.ReadLine();
                ++serial_read_R;
            }
        }
        private void SerialPort_DataReceived_L(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp_l = (SerialPort)sender;
            if (sp_l.BytesToRead > 0)
            {
                string data = sp_l.ReadLine();
                ++serial_read_L;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE START BUTTON CLICK EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("Initial Btn_Click");
            try
            {

                //Debug.WriteLine("Inside Try of Btn_Start_Click!!");
                this.Btn_Start_Clicked = true;
                this.Btn_Stop.IsEnabled = true;
                this.Btn_Home_Clicked = false;

                StartButtonImagePath = START_ENABLED;
                Btn_Start.Cursor = System.Windows.Input.Cursors.Hand;

                StopButtonImagePath = STOP_ENABLED;
                Btn_Stop.Cursor = System.Windows.Input.Cursors.Hand;

                RobotStatus = "RUNNING";

                // Calling the NI DAQ DI and read the values of the optical sensors
                if (!sp_R.IsOpen)
                {
                    //Debug.WriteLine("Getting Right Values^^^^");
                    sp_R.Open();
                }
                if (!sp_L.IsOpen)
                {
                    //Debug.WriteLine("Getting Left Values^^^^");
                    sp_L.Open();
                    //Debug.WriteLine("Getting Left after SPL Open^^^^");
                }

                //Debug.WriteLine("before timer starts!!");
                StartTimer();
                //Debug.WriteLine("timer end !!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: Btn_Start_Click " + ex.Message);
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE STOP BUTTON CLICK EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void Btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("Inside Stop button!!!");
                this.Btn_Stop_Clicked = true;
                this.Btn_Connect_Clicked = false;
                this.Btn_Start_Clicked = false;

                Btn_Stop.Cursor = System.Windows.Input.Cursors.Hand;
                                
                runTimer.Stop();

                sp_R.Write("<0>");
                sp_R.Write("<0>");
                sp_R.Close();
                sp_L.Close();
                StartButtonImagePath = START_DISABLED;

                RobotStatus = "STAND BY";

            }
            catch
            {
                MessageBox.Show("Error: Btn_Stop_Click");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE HOME BUTTON CLICK EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void Btn_Home_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                current_Pulse = new DigitalSingleChannelReader(ReadNITask.Stream);
                sensorC_Pulse_array = current_Pulse.ReadSingleSampleMultiLine();
                this.Btn_Home_Clicked = true;
                this.Btn_GoToPosition_Clicked = false;
                this.switch_confirm = 0;

                Btn_MoveRobot_Clicked = false;

                //mine
                IsHomeReached = false;

                pointA_dof1_counter = 0;
                pointA_dof2_counter = 0;
                RobotStatus = "RUNNING";

                //this.TargetPointComboBox.IsEnabled = true;
                //this.Btn_GoToPosition.IsEnabled= true;

                this.Btn_Stop.IsEnabled = true;
                Btn_Stop.Cursor = System.Windows.Input.Cursors.Hand;

                if (!sp_R.IsOpen || !sp_L.IsOpen)
                {
                    sp_R.Open();
                    sp_L.Open();
                }

                StartTimer();

                //Console.WriteLine($"HomeReached-0: {sensorC_Pulse_array[0]} and HomeReached-2: {sensorC_Pulse_array[2]}");


                if (sensorC_Pulse_array[0] == true && sensorC_Pulse_array[2] == true)
                {
                    IsHomeReached = true;
                    RobotStatus = "HOME";
                    MessageBox.Show("Error: You are already at Home!!!");
                }
            }
            catch
            {
                MessageBox.Show("Error: Btn_Home_Click");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE CALIBRATE BUTTON CLICK EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void Btn_Calibrate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.Btn_Connect_Clicked == false)
                {
                    MessageBox.Show("Error: please connect first");
                    return;
                }

                if (RobotMode != "Auto")
                {
                    System.Windows.MessageBox.Show("Error: Calibrate Button is only enabled under AUTO mode");
                    return;
                }
                else
                {

                    this.Btn_GoToPosition_Clicked = false;
                    this.Btn_Stop.IsEnabled = true;
                    this.Btn_Stop_Clicked = false;
                    this.Btn_Home_Clicked = false;
                    this.Btn_MoveRobot_Clicked = false;
                    StopButtonImagePath = STOP_ENABLED;
                    Btn_Stop.Cursor = System.Windows.Input.Cursors.Hand;
                    RobotStatus = "Running";
                    // Default positions, if calibration is clicked after FK button
                    previousJ1 = -40;
                    previousJ2 = 50.17;

                    if (!sp_R.IsOpen)
                    {
                        sp_R.Open();
                    }
                    if (!sp_L.IsOpen)
                    {
                        sp_L.Open();
                    }

                    if (this.calibrate == false)
                    {
                        // perform calibration
                        StartCalibration();
                    }
                    else
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show("Calibration is already done!! Do you want to re-calibrate?", "Calibration", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            // Do re-calibration
                            StartCalibration();
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: Btn_Calibrate_Click: " + ex.Message);
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE ROBOT MOVE BUTTON CLICK EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void Btn_MoveRobot_Click(object sender, RoutedEventArgs e)
        {
            // Check if robot is in AUTO mode
            if (RobotMode != "Auto")
            {
                System.Windows.MessageBox.Show("Error: Go To Position Button is only enabled under AUTO mode");
                return;
            }
            else
            {
                try
                {
                    StopButtonImagePath = STOP_ENABLED;
                    Btn_Stop.Cursor = System.Windows.Input.Cursors.Hand;
                    this.Btn_Stop.IsEnabled = true;
                    this.Btn_Stop_Clicked = false;
                    this.Btn_GoToPosition_Clicked = false;
                    this.Btn_Home_Clicked = false;

                    this.Btn_MoveRobot_Clicked = true;

                    this.switch_confirm = 0;



                    RobotStatus = "Running";

                    //for the encoder teeth
                    total_teeth_num_dof1 = 0;
                    total_teeth_num_dof2 = 0;


                    if (!sp_R.IsOpen)
                    {
                        sp_R.Open();
                    }
                    if (!sp_L.IsOpen)
                    {
                        sp_L.Open();
                    }
                    //// Getting the selected target point
                    //ComboBoxItem selectedItem = (ComboBoxItem)TargetPointComboBox.SelectedItem;
                    //targetPoint = selectedItem.Content.ToString().Trim();

                    StartTimer();


                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: Go FK Button. Exception: " + ex.Message);
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------
        // *** HANDLE TOGGLE BUTTON SWITCH EVENT (AUTO/MANUAL) ***  
        //--------------------------------------------------------------------------------------------------------------------------
        public void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            RobotMode = "Auto";

            this.Btn_Start.IsEnabled = false;
            StartButtonImagePath = START_DISABLED;

            this.Btn_Stop.IsEnabled = false;
            StopButtonImagePath = STOP_DISABLED;

            this.Btn_Home.IsEnabled = true;
            HomeButtonImagePath = HOME_ENABLED;
            Btn_Home.Cursor = System.Windows.Input.Cursors.Hand;

        }

        public void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            RobotMode = "Manual";

            this.Btn_Start.IsEnabled = true;
            StartButtonImagePath = START_ENABLED;
            Btn_Start.Cursor = System.Windows.Input.Cursors.Hand;

            //this.Btn_Stop.IsEnabled = false;
            //StopButtonImagePath = "/Resources/StopBtn_Disabled.png";

            this.Btn_Home.IsEnabled = false;
            HomeButtonImagePath = HOME_DISABLED;
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE MAXIMIZE 3D VISUALIZATION USER CONTROL EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void Btn_Zoom_Click(object sender, RoutedEventArgs e)
        {
            if (Grid.GetColumnSpan(ViewPanel) == 1 && Grid.GetRowSpan(ViewPanel) == 3)
            {
                // If the ViewPanel is not maximized, maximize it and hide VisDockPanel
                Grid.SetColumnSpan(ViewPanel, 2);
                Grid.SetRowSpan(ViewPanel, 3);
                VisDockPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                // If the ViewPanel is maximized, minimize it and show VisDockPanel
                Grid.SetColumnSpan(ViewPanel, 1);
                Grid.SetRowSpan(ViewPanel, 3);
                VisDockPanel.Visibility = Visibility.Visible;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE CLOSE SELECT TARGET POINT WINDOW EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            ViewPanel.Visibility = Visibility.Collapsed;
            RobotStatusPanel.Visibility = Visibility.Visible;
            ConnectionStatusPanel.Visibility = Visibility.Visible;
            ControlPanel.Visibility = Visibility.Visible;
            VisDockPanel.Visibility = Visibility.Visible;
            SimulatorPanel.Visibility = Visibility.Visible;

            // Set the view panel to default size
            Grid.SetColumnSpan(ViewPanel, 1);
            Grid.SetRowSpan(ViewPanel, 4);
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE SET TARGET BUTTON IS CLICKED ***
        //-------------------------------------------------------------------------------------------------------------
        private void VisualizationUserControl_SetTargetButtonClicked(object sender, EventArgs e)
        {
            SetTargetisClicked = true;
            joint1 = visualizationUserControl.GetCurrentJointValues()[0];
            joint2 = visualizationUserControl.GetCurrentJointValues()[1];
            Console.WriteLine($"vales J1: {joint1} and J2: {joint2}");
            //Tuple<double, double> pair = Tuple.Create(joint1, joint2);
            System.Tuple<double, double> pair = new System.Tuple<double, double>(joint1, joint2);
            setValues.Push(pair);
            Console.WriteLine($"setValues size: {setValues.Count}");
            MessageBox.Show("You have set the FK coordinates, please click the Move Robot Button", "Message!");
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE IK TARGET BUTTON IS CLICKED ***
        //-------------------------------------------------------------------------------------------------------------
        private void VisualizationUserControl_SetIK_TargetButtonClicked(object sender, EventArgs e) {
            SetIK_TargetisClicked = true;
            IKjoint1 = visualizationUserControl.GetCurrentIK_JointValues()[0];
            IKjoint2 = visualizationUserControl.GetCurrentIK_JointValues()[1];
            System.Tuple<double, double> pair = new System.Tuple<double, double>(IKjoint1, IKjoint2);
            setIK_Values.Push(pair);

            Console.WriteLine($"The IK J1: {IKjoint1} and J2: {IKjoint2}");

        }

    }
}