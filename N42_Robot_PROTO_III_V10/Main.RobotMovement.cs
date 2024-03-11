using DocumentFormat.OpenXml.Bibliography;
using NationalInstruments.DAQmx;
using SharpDX.Direct3D11;
using SharpDX.XInput;
using System;
using System.Diagnostics;
using System.Threading;


namespace n42_Robot_PROTO_III
{
    public partial class MainWindowUpdate
    {

        //--------------------------------------------------------------------------------------------------------------------------
        // *** Deg To Counter ***  
        //--------------------------------------------------------------------------------------------------------------------------
        private int[] degToCounter(double J1, double J2)
        {
            // The zero condition needs to get checked. 50.17 -50.17 case
            //double dof1MaxAngle = -40;
            //double dof2MaxAngle = 50.17;
            int[] ans = new int[2];
            double dof1A = Math.Abs(J1);
            double dof2A = Math.Abs(J2);
            ans[0] = (int)(dof1A * 10.3625); // -40 to +40 = 829 serial reads
            ans[1] = (int)(dof2A * 20.04785); // 79.41 deg ->1592 serial reads
            //ans[1] = (int)(dof2A * 20.0453); // 79.41 deg ->1591.8 serial reads
            //ans[1] = (int)(dof2A * 19.8975); // 80 deg ->1591.8 serial reads
            return ans;
        }

        //--------------------------------------------------------------------------------------------------------------------------
        // *** Start Timer ***  
        //--------------------------------------------------------------------------------------------------------------------------
        public void StartTimer()
        {
            //_controller = new Controller(UserIndex.One);

            // We need this because the runtimer is not stuuting done thoigh we have provided Stop inside LoopTimer, hence making it null.
            if (runTimer != null)
            {
                runTimer.Stop();
                runTimer = null;
            }

            runTimer = new System.Windows.Forms.Timer();
            runTimer.Interval = 10;
            runTimer.Tick += new EventHandler(LoopTimer_Tick);
            runTimer.Start();

        }


        //--------------------------------------------------------------------------------------------------------------------------
        // *** Event Timer Tick ***  
        //--------------------------------------------------------------------------------------------------------------------------
        private void LoopTimer_Tick(object sender, EventArgs e)
        {
            ReadWrite_Data();

        }

        //--------------------------------------------------------------------------------------------------------------------------
        // *** Data read from the Joystick, data read from the NI DAQ, and data write to the serial port (motion controllers) ***  
        //--------------------------------------------------------------------------------------------------------------------------
        private void ReadWrite_Data()
        {
            try
            {
                current_Pulse = new DigitalSingleChannelReader(ReadNITask.Stream);
                sensorC_Pulse_array = current_Pulse.ReadSingleSampleMultiLine();


                // Code for Optical Sensors:
                current_state_dof1 = sensorC_Pulse_array[5];
                current_state_dof2 = sensorC_Pulse_array[7];


                if (isfirstiteration == false)
                {
                    if (current_state_dof1 != previous_state_dof1)
                    {
                        teeth_num_dof1++;
                        total_teeth_num_dof1++;
                        //encoder degree
                        rotation_degree_dof1 = rotation_degree_dof1 + 12;
                    }
                    if (current_state_dof2 != previous_state_dof2)
                    {
                        teeth_num_dof2++;
                        total_teeth_num_dof2++;
                        //encoder degree
                        rotation_degree_dof2 = rotation_degree_dof2 + 12;
                    }

                    if (teeth_num_dof1 == 30)
                    {
                        teeth_num_dof1 = 0;
                    }

                    if (teeth_num_dof2 == 30)
                    {
                        teeth_num_dof2 = 0;
                    }
                }

                isfirstiteration = false;

                previous_state_dof1 = current_state_dof1;
                previous_state_dof2 = current_state_dof2;

                if (RobotMode == "Manual")
                {
                    _controller = new Controller(UserIndex.One);
                    var state = _controller.GetState();

                    LVY = state.Gamepad.LeftThumbY;
                    RVX = state.Gamepad.RightThumbX;

                    //if (sensorC_Pulse_array[4] != sensorL_Pulse_array[4] && RVX != 0)
                    //{
                    //    if (RVX > 0)
                    //    {
                    //        _dof1_Angle += 0.07;
                    //    }
                    //    else
                    //    {
                    //        _dof1_Angle -= 0.07;
                    //    }
                    //    DOF1_Angle = Convert.ToString(_dof1_Angle);
                    //}
                    //if (sensorC_Pulse_array[6] != sensorL_Pulse_array[6] && LVY != 0)
                    //{
                    //    if (LVY > 0)
                    //    {
                    //        _dof2_Angle += 0.07;
                    //    }
                    //    else
                    //    {
                    //        _dof2_Angle -= 0.07;
                    //    }
                    //    DOF2_Angle = Convert.ToString(_dof2_Angle);
                    //}

                    // Write XBox position to Arduino board
                    if (Btn_Start_Clicked)
                    {
                        string array_R = "<" + Convert.ToString(RVX) + ">";
                        string array_L = "<" + Convert.ToString(LVY) + ">";
                        const string zero_input = "<0>";
                        string inputVal = zero_input;

                        // right joystick
                        if (RVX > 0) // moving clockwise
                        {
                            //inputVal = !sensorC_Pulse_array[0] ? array_R : zero_input;
                            if (!sensorC_Pulse_array[0])
                            {
                                inputVal = array_R;
                                _dof1_Angle -= 0.0965;
                            }
                            else
                            {
                                inputVal = zero_input;
                                _dof1_Angle = -40.0;
                            }
                            
                        }
                        else if (RVX < 0) // moving counter-clockwise
                        {
                            //inputVal = !sensorC_Pulse_array[1] ? array_R : zero_input;
                            if (!sensorC_Pulse_array[1])
                            {
                                inputVal = array_R;
                                _dof1_Angle += 0.0965;
                            }
                            else
                            {
                                inputVal = zero_input;
                                _dof1_Angle = +40.0;
                            }
                        }
                        sp_R.Write(inputVal);
                        DOF1_Angle = Convert.ToString(_dof1_Angle);

                        inputVal = zero_input;

                        // left joystick
                        if (LVY > 0) // moving forward
                        {
                            //inputVal = !sensorC_Pulse_array[3] ? array_L : zero_input;
                            if (!sensorC_Pulse_array[3])
                            {
                                inputVal = array_L;
                                _dof2_Angle -= 0.04988;
                            }
                            else
                            {
                                inputVal = zero_input;
                                _dof2_Angle = -29.24;
                            }

                        }
                        else if (LVY < 0)
                        {
                            //inputVal = !sensorC_Pulse_array[2] ? array_L : zero_input;
                            if (!sensorC_Pulse_array[2])
                            {
                                inputVal = array_L;
                                _dof2_Angle += 0.04988;
                            }
                            else
                            {
                                inputVal = zero_input;
                                _dof2_Angle = 50.17;
                            }
                        }

                        sp_L.Write(inputVal);
                        DOF2_Angle = Convert.ToString(_dof2_Angle);

                        //// We can use this for multi-threading
                        //// Not allow rotating ccw when limit switch is hit
                        //if (sensorC_Pulse_array[0] == true)
                        //{
                        //    if (RVX < 0)
                        //    {
                        //        //System.Windows.MessageBox.Show("Reached limited position");
                        //        sp_R.Write("<0>");
                        //        return;
                        //    }
                        //    else
                        //    {
                        //        sp_R.Write(array_R);
                        //    }

                        //}

                        //// Not allow rotating cw when limit switch is hit
                        //if (sensorC_Pulse_array[1] == true)
                        //{
                        //    if (RVX > 0)
                        //    {
                        //        //System.Windows.MessageBox.Show("Reached limited position");
                        //        sp_R.Write("<0>");
                        //        return;
                        //    }
                        //    else
                        //    {
                        //        sp_R.Write(array_R);
                        //    }

                        //}

                        //// Not allow moving backward when limit switch is hit
                        //if (sensorC_Pulse_array[2] == true)
                        //{
                        //    if (LVY < 0)
                        //    {
                        //        //System.Windows.MessageBox.Show("Reached limited position");
                        //        sp_L.Write("<0>");
                        //        return;
                        //    }
                        //    else
                        //    {
                        //        sp_L.Write(array_L);
                        //    }

                        //}

                        //// Not allow moving forward when limit switch is hit
                        //if (sensorC_Pulse_array[3] == true)
                        //{
                        //    if (LVY > 0)
                        //    {
                        //        //System.Windows.MessageBox.Show("Reached limited position");
                        //        sp_L.Write("<0>");
                        //        return;
                        //    }
                        //    else
                        //    {
                        //        sp_L.Write(array_L);
                        //    }

                        //}

                        //// Moving freely
                        //if (sensorC_Pulse_array[0] == false && sensorC_Pulse_array[1] == false && sensorC_Pulse_array[2] == false && sensorC_Pulse_array[3] == false)
                        //{
                        //    //Debug.WriteLine("array_R : " + RVX);
                        //    //Debug.WriteLine("array_L : " + LVY);
                        //    string arrayStr_R = string.Join("", array_R);
                        //    string arrayStr_L = string.Join("", array_L);

                        //    sp_R.Write(array_R);
                        //    sp_L.Write(array_L);
                        //}
                    }
                }

                if (RobotMode == "Auto")
                {
                    // Send the robot to Home position
                    if (Btn_Home_Clicked && !Btn_GoToPosition_Clicked)
                    {
                        IsHomeReached = sensorC_Pulse_array[0] && sensorC_Pulse_array[2];
                        if (IsHomeReached)
                        {
                            sp_R.Write("<0>");
                            sp_L.Write("<0>");
                            sp_R.Close();
                            sp_L.Close();
                            _dof1_Angle = -40.0;
                            DOF1_Angle = Convert.ToString(_dof1_Angle);
                            _dof2_Angle = 50.17;
                            DOF2_Angle = Convert.ToString(_dof2_Angle);
                            runTimer.Stop();
                            RobotStatus = "HOME";
                            Thread.Sleep(500);
                            return;
                        }
                        else if (sensorC_Pulse_array[0] && !sensorC_Pulse_array[2])
                        {
                            string max_speed = "<" + Convert.ToString(-32768) + ">";
                            sp_L.Write(max_speed);
                            Thread.Sleep(1);
                            _dof2_Angle += 0.04988;
                            DOF2_Angle = Convert.ToString(_dof2_Angle);
                        }
                        else if (!sensorC_Pulse_array[0] && sensorC_Pulse_array[2])
                        {
                            string max_speed = "<" + Convert.ToString(32767) + ">";
                            sp_R.Write(max_speed);
                            Thread.Sleep(1);
                            _dof1_Angle -= 0.0965;
                            DOF1_Angle = Convert.ToString(_dof1_Angle);
                        }
                        else
                        {
                            string max_speed = "<" + Convert.ToString(32767) + ">";
                            if (!sensorC_Pulse_array[0])
                            {
                                sp_R.Write(max_speed);
                                Thread.Sleep(1);
                                _dof1_Angle -= 0.0965;
                                DOF1_Angle = Convert.ToString(_dof1_Angle);
                            }
                        }

                    }

                    //if (Btn_MoveRobot_Clicked && SetTargetisClicked)
                    if ((Btn_MoveRobot_Clicked && SetTargetisClicked) || (Btn_MoveRobot_Clicked && SetIK_TargetisClicked)) // we're in auto
                    {
                        Tuple<double, double> newAngles = new System.Tuple<double, double>(0, 0);

                        if (SetTargetisClicked)
                        {
                            newAngles = setValues.Peek();
                        }
                        else if(SetIK_TargetisClicked)
                        {
                            newAngles = setIK_Values.Peek();
                        }
                        int[] js = degToCounter(newAngles.Item1 - previousJ1, newAngles.Item2 - previousJ2);

                        // The stop dosnt work while another event is working, we need to do multi threading. 
                        if (this.Btn_Stop_Clicked == true)
                        {
                            Console.WriteLine("The stop inside FK works");
                            sp_R.Write("<0>");
                            sp_L.Write("<0>");
                            Console.WriteLine($"total_teeth_num_dof1: {total_teeth_num_dof1}");
                            Console.WriteLine($"total_teeth_num_dof2: {total_teeth_num_dof2}");
                            serial_read_R = 0;
                            serial_read_L = 0;
                            sp_R.Close();
                            sp_L.Close();
                            SetTargetisClicked = false;
                            SetIK_TargetisClicked = false;
                            runTimer.Stop();
                            Thread.Sleep(500);
                            previousJ1 = newAngles.Item1;
                            previousJ2 = newAngles.Item2;
                            setValues.Clear();
                            return;
                        }

                        if (newAngles.Item1 > previousJ1 && serial_read_R <= js[0])
                        {

                            _dof1_Angle += 0.0965;
                            DOF1_Angle = Convert.ToString(_dof1_Angle);
                            Thread.Sleep(1);
                            if (sensorC_Pulse_array[1])
                            {
                                Console.WriteLine("You have reached the DOF1 Target limit");
                                serial_read_R = js[0];
                                _dof1_Angle = 40;
                                DOF1_Angle = Convert.ToString(_dof1_Angle);
                            }
                            else
                            {
                                //Console.WriteLine($"j1Steps+: {serial_read_R}");
                                ++counterR;
                                // moving away
                                sp_R.Write("<-32768>");
                                Console.WriteLine($"j1Steps+: {serial_read_R} " + $"Writes+: {counterR}");
                                Thread.Sleep(5);
                            }
                        }
                        else if (newAngles.Item1 < previousJ1 && serial_read_R <= js[0])
                        {
                            _dof1_Angle -= 0.0965;
                            DOF1_Angle = Convert.ToString(_dof1_Angle);
                            Thread.Sleep(1);
                            if (sensorC_Pulse_array[0])
                            {
                                Console.WriteLine("You have reached the DOF1 Home limit");
                                serial_read_R = js[0];
                                _dof1_Angle = -40;
                                DOF1_Angle = Convert.ToString(_dof1_Angle);
                            }
                            else
                            {
                                ++counterR;
                                // Moving home
                                sp_R.Write("<32767>");
                                Console.WriteLine($"j1Steps-: {serial_read_R} " + $"Writes-: {counterR}");
                                Thread.Sleep(5);
                            }

                        }
                        //else if (joint2 >= 0.0 && j2Steps != js[1])
                        //else if (joint2>=0.0 && serial_read_L != js[1])
                        else if (newAngles.Item2 < previousJ2 && serial_read_L <= js[1])
                        {
                            //++j2Steps;
                            //Console.WriteLine($"j2Steps: {j2Steps}");
                            _dof2_Angle -= 0.04988;
                            DOF2_Angle = Convert.ToString(_dof2_Angle);
                            Thread.Sleep(1);
                            if (sensorC_Pulse_array[3])
                            {
                                Console.WriteLine("You have reached the DOF2 Target limit");
                                serial_read_L = js[1];
                            }
                            else
                            {
                                ++counterL;
                                sp_L.Write("<32767>");
                                Console.WriteLine($"j2Steps+: {serial_read_L} " + $"Writes+: {counterL}");
                                Thread.Sleep(5);
                            }
                        }
                        else if (newAngles.Item2 > previousJ2 && serial_read_L <= js[1])
                        {
                            //++j2Steps;
                            //Console.WriteLine($"j2Steps: {j2Steps}");
                            //Console.WriteLine($"j2Steps-: {serial_read_L}");

                            _dof2_Angle += 0.04988;
                            DOF2_Angle = Convert.ToString(_dof2_Angle);
                            Thread.Sleep(1);

                            if (sensorC_Pulse_array[2])
                            {
                                Console.WriteLine("You have reached the DOF2 Home limit");
                                serial_read_L = js[1];
                            }
                            else
                            {
                                ++counterL;
                                //string max_speed = "<" + Convert.ToString(-32768) + ">";
                                // because you are starting at home [2]
                                sp_L.Write("<-32768>");
                                Console.WriteLine($"j2Steps-: {serial_read_L} " + $"Writes-: {counterL}");
                                Thread.Sleep(5);
                            }

                        }
                        if (serial_read_R >= js[0] && serial_read_L >= js[1])
                        {
                            Console.WriteLine("are we inside final step??");
                            sp_R.Write("<0>");
                            sp_L.Write("<0>");
                            //j1Steps = 0;
                            //j2Steps = 0;
                            Console.WriteLine($"total_teeth_num_dof1: {total_teeth_num_dof1}");
                            Console.WriteLine($"rotation_degree_dof1: {rotation_degree_dof1}");
                            Console.WriteLine($"total_teeth_num_dof2: {total_teeth_num_dof2}");
                            Console.WriteLine($"rotation_degree_dof2: {rotation_degree_dof2}");
                            serial_read_R = 0;
                            serial_read_L = 0;
                            counterR = 0;
                            counterL = 0;
                            sp_R.Close();
                            sp_L.Close();
                            SetTargetisClicked = false;
                            SetIK_TargetisClicked = false;
                            runTimer.Stop();
                            RobotStatus = "HOME";
                            previousJ1 = newAngles.Item1;
                            previousJ2 = newAngles.Item2;
                            setValues.Clear();
                            Thread.Sleep(500);
                            return;
                        }

                    }

                }

                // Send the robot to predefined target Points (A/B)
                if (Btn_GoToPosition_Clicked && RobotMode == "Auto")
                {
                    Btn_Home_Clicked = false;

                    if (pointA_dof1_counter == POINT_A_DOF1 && pointA_dof2_counter == POINT_A_DOF2)
                    {
                        runTimer.Stop();
                        RobotStatus = "Target A Reached";
                        return;
                    }

                    if (targetPoint == "Target A (168.5, 0, -47)")
                    {
                        // DOF 1 Movement to Point A
                        bool switch_motor = false;
                        if (pointA_dof1_counter < POINT_A_DOF1 && !switch_motor)
                        {
                            sp_R.Write("<" + Convert.ToString(28000) + ">");
                            pointA_dof1_counter++;
                        }
                        else
                        {
                            switch_motor = !switch_motor;
                            switch_confirm += 1;
                            sp_R.Write("<0>");
                        }

                        if (switch_confirm == 1)
                        {
                            Thread.Sleep(3000);
                        }

                        if (switch_motor)
                        {
                            if (pointA_dof2_counter < POINT_A_DOF2)
                            {
                                sp_L.Write("<" + Convert.ToString(32500) + ">");
                                pointA_dof2_counter++;
                            }
                            else
                            {
                                sp_L.Write("<0>");
                            }
                        }
                    }
                    else if (targetPoint == "Target B (188.3, -18.1, -38.6)")
                    {
                        bool switch_motor = false;
                        if (pointB_dof1_counter != POINT_B_DOF1 && !switch_motor)
                        {
                            sp_R.Write("<" + Convert.ToString(32500) + ">");
                            pointB_dof1_counter++;
                        }
                        else
                        {
                            switch_motor = !switch_motor;
                            switch_confirm += 1;
                            sp_R.Write("<0>");
                        }

                        if (switch_confirm == 1)
                        {
                            Thread.Sleep(3000);
                        }

                        if (switch_motor)
                        {

                            if (pointB_dof2_counter != POINT_B_DOF2)
                            {
                                sp_L.Write("<" + Convert.ToString(32500) + ">");
                                pointB_dof2_counter++;
                            }
                            else
                            {
                                sp_L.Write("<0>");
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Debug.WriteLine("Error: Display Controller/Sensor Information. Exception: " + ex.Message);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------
        // *** The robot from the current position moves away and comes back by calibrateL steps for DOF-2 ***  
        //--------------------------------------------------------------------------------------------------------------------------
        public void MoveAwayComeBack_L()
        {
            while (calibrateL >= 0)
            {
                if (this.Btn_Stop_Clicked == true) { break; }
                current_Pulse = new DigitalSingleChannelReader(ReadNITask.Stream);
                sensorC_Pulse_array = current_Pulse.ReadSingleSampleMultiLine();
                calibrateL--;
                sp_L.Write("<32767>");
                Thread.Sleep(10);
                Btn_Stop.Cursor = System.Windows.Input.Cursors.Hand;
            }
            while (!sensorC_Pulse_array[2])
            {
                if (this.Btn_Stop_Clicked == true) { break; }
                current_Pulse = new DigitalSingleChannelReader(ReadNITask.Stream);
                sensorC_Pulse_array = current_Pulse.ReadSingleSampleMultiLine();
                sp_L.Write("<-32768>");
                Thread.Sleep(10);
                Btn_Stop.Cursor = System.Windows.Input.Cursors.Hand;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------
        // *** The robot from the current position moves away and comes back by calibrateR steps for DOF-1 ***  
        //--------------------------------------------------------------------------------------------------------------------------
        public void MoveAwayComeBack_R()
        {
            while (calibrateR >= 0)
            {
                if (this.Btn_Stop_Clicked == true) { break; }
                current_Pulse = new DigitalSingleChannelReader(ReadNITask.Stream);
                sensorC_Pulse_array = current_Pulse.ReadSingleSampleMultiLine();
                calibrateR--;
                sp_R.Write("<-32768>");
                Thread.Sleep(10);
                Btn_Stop.Cursor = System.Windows.Input.Cursors.Hand;
            }
            while (!sensorC_Pulse_array[0])
            {
                if (this.Btn_Stop_Clicked == true) { break; }
                current_Pulse = new DigitalSingleChannelReader(ReadNITask.Stream);
                sensorC_Pulse_array = current_Pulse.ReadSingleSampleMultiLine();
                sp_R.Write("<32767>");
                Thread.Sleep(10);
                Btn_Stop.Cursor = System.Windows.Input.Cursors.Hand;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------
        // *** Perform Calibration with nmin of 50 and 100 serial writes for right and left motor respectively ***  
        //--------------------------------------------------------------------------------------------------------------------------
        public void StartCalibration()
        {

            current_Pulse = new DigitalSingleChannelReader(ReadNITask.Stream);
            sensorC_Pulse_array = current_Pulse.ReadSingleSampleMultiLine();

            //Console.WriteLine("We are here: "+ $"Sen0: {sensorC_Pulse_array[0]} && Sen2: {sensorC_Pulse_array[2]}" );

            // If any of the robot DOF's are at home position
            if (sensorC_Pulse_array[0] || sensorC_Pulse_array[2])
            {
                // If initially the robot is at home position for DOF1
                if (sensorC_Pulse_array[0])
                {
                    //Console.WriteLine("very intial");
                    MoveAwayComeBack_R();
                }
                else
                {
                    // Come to DOF-1 home position atleast by calibrateR steps
                    while (!sensorC_Pulse_array[0])
                    {
                        if (this.Btn_Stop_Clicked == true) { break; }
                        current_Pulse = new DigitalSingleChannelReader(ReadNITask.Stream);
                        sensorC_Pulse_array = current_Pulse.ReadSingleSampleMultiLine();
                        calibrateR--;
                        sp_R.Write("<32767>");
                        Thread.Sleep(10);
                        Btn_Stop.Cursor = System.Windows.Input.Cursors.Hand;
                    }
                    // If still remains calibrateR steps, complete them
                    if (calibrateR >= 0)
                    {
                        //Console.WriteLine("One is not done yet");
                        MoveAwayComeBack_R();
                    }
                }
                // re-initiating to default
                calibrateR = 50;

                // Logic for DOF-2
                // If initially the robot is at home position for DOF2
                if (sensorC_Pulse_array[2])
                {
                    MoveAwayComeBack_L();
                }
                else
                {
                    // Come to DOF-2 home position atleast by calibrateL steps
                    while (!sensorC_Pulse_array[2])
                    {
                        if (this.Btn_Stop_Clicked == true) { break; }
                        current_Pulse = new DigitalSingleChannelReader(ReadNITask.Stream);
                        sensorC_Pulse_array = current_Pulse.ReadSingleSampleMultiLine();
                        calibrateL--;
                        sp_L.Write("<-32768>");
                        Thread.Sleep(10);
                        Btn_Stop.Cursor = System.Windows.Input.Cursors.Hand;
                    }
                    // If still remains calibrateL steps, complete them
                    if (calibrateL >= 0)
                    {
                        MoveAwayComeBack_L();
                    }
                }
                // re-initiating to default
                calibrateL = 100;
            }
            // both DOF's are not at home position
            else
            {
                //Console.WriteLine("I am not expoecting here");
                // First bring DOF-1 back to home
                while (!sensorC_Pulse_array[0])
                {
                    if (this.Btn_Stop_Clicked == true) { break; }
                    current_Pulse = new DigitalSingleChannelReader(ReadNITask.Stream);
                    sensorC_Pulse_array = current_Pulse.ReadSingleSampleMultiLine();
                    calibrateR--;
                    sp_R.Write("<32767>");
                    Thread.Sleep(10);
                    Btn_Stop.Cursor = System.Windows.Input.Cursors.Hand;
                }
                // if still remains calibrateR steps
                if (calibrateR >= 0)
                {
                    MoveAwayComeBack_R();
                }
                calibrateR = 50; // re-initiating to default

                // bring DOF-2 back to home by calibrateL steps
                while (!sensorC_Pulse_array[2])
                {
                    if (this.Btn_Stop_Clicked == true) { break; }
                    current_Pulse = new DigitalSingleChannelReader(ReadNITask.Stream);
                    sensorC_Pulse_array = current_Pulse.ReadSingleSampleMultiLine();
                    calibrateL--;
                    sp_L.Write("<-32768>");
                    Thread.Sleep(10);
                    Btn_Stop.Cursor = System.Windows.Input.Cursors.Hand;
                }
                // if still remains calibrateL steps
                if (calibrateL >= 0)
                {
                    MoveAwayComeBack_L();
                }
                calibrateL = 100; // re-initiating to default
            }
            this.Btn_Stop_Clicked = false;
            sp_R.Write("<0>");
            sp_L.Write("<0>");
            serial_read_R = 0;
            serial_read_L = 0;
            sp_R.Close();
            sp_L.Close();
            this.calibrate = true;
        }
    }
}
