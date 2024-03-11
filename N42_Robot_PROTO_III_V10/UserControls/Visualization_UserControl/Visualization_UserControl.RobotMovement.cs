using System;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media.Media3D;



namespace n42_Robot_PROTO_III
{
    public partial class Visualization_UserControl : UserControl
    {

        //-------------------------------------------------------------------------------------------------------------
        // *** Directly perform FK when changing the slide bar ***
        //-------------------------------------------------------------------------------------------------------------     

        private void execute_fk()
        {
            /** Debug sphere, it takes the x,y,z of the textBoxes and update its position
             * This is useful when using x,y,z in the "new Point3D(x,y,z)* when defining a new RotateTransform3D() to check where the joints is actually  rotating */
            float[] angles = { joints[0].angle, joints[1].angle, joints[2].angle, joints[3].angle, joints[4].angle, joints[5].angle,
                               joints[6].angle, joints[7].angle, joints[8].transAxisY };
            ForwardKinematics(angles);
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** Timer for 3D visualization model only ***
        //-------------------------------------------------------------------------------------------------------------     

        // visualization Ticking
        public void timer1_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine("When do youn trigger?");
            if (go_to_enable)
            {
                model_movement((final_pos_DOF1 - start_pos_DOF1) / total_time,
                                (final_pos_DOF2 - start_pos_DOF2) / total_time,
                                (final_pos_needle - start_pos_needle) / total_time);
            }
            counter += 1;
            if (counter >= total_time)
            {
                //Console.WriteLine("will you come inside???");
                counter = 0;
                button.Content = "Go to position";
                isAnimating = false;
                go_to_enable = false;
                go_angles[0] = 0f;
                go_angles[1] = 0f;
                go_angles[7] = 0f;
                go_angles[8] = 0f;
                Console.WriteLine("Stopped");
                timer1.Stop();
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** A general method for moving the whole model with given angles ***
        //-------------------------------------------------------------------------------------------------------------     
        // Input DOF1 & DOF2 angles to perform FK
        private void model_movement(float DOF1_angle, float DOF2_angle, float needle_value)
        {
            joint_DOF1.Value += DOF1_angle;
            joint_DOF2.Value += DOF2_angle;
            Needle.Value += needle_value;
            joints[0].angle = (float)joint_DOF1.Value;
            double DOF2_theta_prime = (90 - (float)joint_DOF2.Value) * Math.PI / 180;
            double nut_trans = 107 - (114.22 / Math.Sin(DOF2_theta_prime)) * Math.Sin(((180 - Math.Asin(41.57 / 114.22 * Math.Sin(DOF2_theta_prime)) * 180 / Math.PI - DOF2_theta_prime * 180 / Math.PI) * Math.PI / 180));
            joints[1].transAxisX = (float)nut_trans;
            joints[7].angle = (float)joint_DOF2.Value;
            joints[8].transAxisY = (float)Needle.Value;
            go_angles[0] = joints[0].angle;
            // Something Fishy here... why are we not taking go_angles[1] = joints[1].transAxisX calculated aboove, is this a typing mistake?
            //go_angles[1] = joints[1].angle;
            go_angles[1] = joints[1].transAxisX;
            go_angles[7] = joints[7].angle;
            go_angles[8] = joints[8].transAxisY;
            if (joint_DOF1.Value >= final_pos_DOF1)
            {
                go_angles[0] = 0;
            }
            if (joint_DOF2.Value >= final_pos_DOF2)
            {
                go_angles[1] = 0;
                go_angles[7] = 0;
            }
            if (Needle.Value >= final_pos_needle)
            {
                go_angles[8] = 0;
            }
            ForwardKinematics(go_angles);
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** A function to exam whether the robot hits any limit switches ***
        //-------------------------------------------------------------------------------------------------------------     
        //private bool stop_conditions()
        //{
        //    return false;
        //}


        //-------------------------------------------------------------------------------------------------------------
        // *** Auto mode - Timer for synchronizating the real robot with the 3D visualization model (Incomplete) ***
        //-------------------------------------------------------------------------------------------------------------     
        // Auto mode timer function
        //public void synchronization_auto_mode_timer_Tick(object sender, EventArgs e)
        //{
        //    model_movement(DOF1_pulse_degree,
        //                    DOF2_pulse_degree,
        //                    20);   // DOF1_pulse_degree, DOF2_pulse_degree are placeholder for the converted input degree from the pulse.
        //    if (joint_DOF1.Value >= final_pos_DOF1 && joint_DOF2.Value >= final_pos_DOF2 && Needle.Value >= final_pos_needle)
        //    {
        //        button.Content = "Go to position";
        //        isAnimating = false;
        //        go_to_enable = false;
        //        go_angles[0] = 0f;
        //        go_angles[1] = 0f;
        //        go_angles[7] = 0f;
        //        go_angles[8] = 0f;
        //        Console.WriteLine("Stopped");
        //        timer1.Stop(); // Change the name to the corresponding timer
        //    }
        //}


        //-------------------------------------------------------------------------------------------------------------
        // *** Manual mode - Timer for synchronizating the real robot with the 3D visualization model (Incomplete) ***
        //-------------------------------------------------------------------------------------------------------------     
        // Manual mode timer function
        //public void synchronization_manual_mode_timer_Tick(object sender, EventArgs e)
        //{
        //     DOF1_pulse_degree, DOF2_pulse_degree are placeholder for the converted input degree from the pulse.
        //    if (DOF1_pulse_degree!= 0 || DOF2_pulse_degree != 0)
        //    {
        //        model_movement(DOF1_pulse_degree,
        //                        DOF2_pulse_degree,
        //                        20);   
        //    }
        //    else
        //    {
        //        go_angles[0] = 0f;
        //        go_angles[1] = 0f;
        //        go_angles[7] = 0f;
        //        go_angles[8] = 0f;
        //        timer1.Stop();  // Change the name to the corresponding timer
        //    }
        //}


        //-------------------------------------------------------------------------------------------------------------
        // *** Inverse Kinematic method to calculate the goal angles ***
        //-------------------------------------------------------------------------------------------------------------  
        // Simulated IK instead of changing the visualization
        public float[] IK_numerical_result(Vector3D target, float[] angles)
        {
            is_simulate = true;
            if (DistanceFromTarget(target, angles) < DistanceThreshold)
            {
                return angles;
            }
            float[] oldAngles = { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
            angles.CopyTo(oldAngles, 0);

            while (DistanceFromTarget(target, angles) > DistanceThreshold)
            {
                for (int i = 0; i <= 8; i++)
                {

                    if (i == 0 || i == 7 || i == 8)
                    {
                        float gradient = PartialGradient(target, angles, i);
                        LearningRate = 0.1f; // Change the learning rate to adjust the calculation time
                        angles[i] -= LearningRate * gradient;
                        if (DistanceFromTarget(target, angles) <= DistanceThreshold || checkAngles(oldAngles, angles))
                        {
                            break;
                        }
                    }
                }
            }
            is_simulate = false;
            return angles;
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** To check the distance between the current end effect to the goal position ***
        //-------------------------------------------------------------------------------------------------------------  
        public float DistanceFromTarget(Vector3D target, float[] angles)
        {
            Matrix4x4 E_final = ForwardKinematics(angles);
            Vector3D point = new Vector3D(E_final.M14, E_final.M24, E_final.M34);
            return (float)Math.Sqrt(Math.Pow((point.X - target.X), 2.0) + Math.Pow((point.Y - target.Y), 2.0) + Math.Pow((point.Z - target.Z), 2.0));
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** To check every angle between the current model and the previous-step model ***
        //-------------------------------------------------------------------------------------------------------------  
        public bool checkAngles(float[] oldAngles, float[] angles)
        {
            for (int i = 0; i <= 8; i++)
            {
                if (oldAngles[i] != angles[i])
                    return false;
            }

            return true;
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** To perform gradient descend ***
        //------------------------------------------------------------------------------------------------------------- 
        public float PartialGradient(Vector3D target, float[] angles, int i)
        {
            // Saves the angle,
            // it will be restored later
            float angle = angles[i];

            // Gradient : [F(x+SamplingDistance) - F(x)] / h
            float f_x = DistanceFromTarget(target, angles);

            angles[i] += SamplingDistance;

            float f_x_plus_d = DistanceFromTarget(target, angles);

            float gradient = (f_x_plus_d - f_x) / SamplingDistance;
            angles[i] = angle;

            return gradient;
        }
    }
}
