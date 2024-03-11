using System;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace n42_Robot_PROTO_III
{
    public partial class Visualization_UserControl : UserControl
    {

        //-------------------------------------------------------------------------------------------------------------
        // *** Perform Forward Kinematics on 3D visualization Model ***
        //-------------------------------------------------------------------------------------------------------------     

        public Matrix4x4 ForwardKinematics(float[] angles)
        {

            double theta_1 = angles[0]; // rotation DOF in deg
            double theta_2 = angles[7]; // translation DOF in deg
            double theta_2_prime = (90 - theta_2) * Math.PI / 180;
            // The relationship between the nut translation and the needle holder angle
            double theta_2_trans = 107 - (114.22 / Math.Sin(theta_2_prime)) * Math.Sin(((180 - Math.Asin(41.57 / 114.22 * Math.Sin(theta_2_prime)) * 180 / Math.PI - theta_2_prime * 180 / Math.PI) * Math.PI / 180));
            double theta_3 = angles[8]; // needle DOF in mm


            // ROT_Link 
            F1 = new Transform3DGroup();
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(joints[0].rotAxisX, joints[0].rotAxisY, joints[0].rotAxisZ), angles[0]), new Point3D(joints[0].rotPointX, joints[0].rotPointY, joints[0].rotPointZ));
            F1.Children.Add(R);

            // Nut linear movement 
            F2 = new Transform3DGroup();
            T = new TranslateTransform3D(joints[1].transAxisX, joints[1].transAxisY, joints[1].transAxisZ);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 0), new Point3D(0, 0, 0));
            F2.Children.Add(T);
            F2.Children.Add(R);
            F2.Children.Add(F1);

            // RCM central link
            float new_degree = (float)Math.Asin((41.57f / 114.22f) * (float)Math.Sin(Math.PI / 2 - angles[7] * (float)Math.PI / 180));
            float original_degree = 180 / (float)Math.PI * (float)Math.Asin(41.57f / 114.22f);
            float angle_joint2 = -(original_degree - 180 / (float)Math.PI * new_degree - 1.1f); // The rotation of RCM central link in degree
            F3 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(joints[2].rotAxisX, joints[2].rotAxisY, joints[2].rotAxisZ), angle_joint2), new Point3D(joints[2].rotPointX, joints[2].rotPointY, joints[2].rotPointZ));
            F3.Children.Add(T);
            F3.Children.Add(R);
            F3.Children.Add(F2);

            // RCM Swing arm back
            F4 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(joints[3].rotAxisX, joints[3].rotAxisY, joints[3].rotAxisZ), angles[7]), new Point3D(joints[3].rotPointX, joints[3].rotPointY, joints[3].rotPointZ));
            F4.Children.Add(T);
            F4.Children.Add(R);
            F4.Children.Add(F1);

            // RCM parallelogram up
            //Parabolic movement
            float theta_abs = Math.Abs(angles[7]) / 2 * (float)Math.PI / 180;
            float joint4_x = -2 * (float)Math.Sin(theta_abs) * (float)Math.Cos(theta_abs) * 41.57f;
            if (angles[7] < 0) // RCM parallelogram up forms a right triangle at 0, which is the threshhold
            {
                joint4_x = -joint4_x;
            }
            float joint4_y = -2 * (float)Math.Pow((float)Math.Sin(theta_abs), 2f) * 41.57f;
            F5 = new Transform3DGroup();
            T = new TranslateTransform3D(joint4_x - theta_2_trans, joint4_y, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 0), new Point3D(joints[4].rotPointX, joints[4].rotPointY, joints[4].rotPointZ));
            F5.Children.Add(T);
            F5.Children.Add(R);
            F5.Children.Add(F2);

            //// RCM parallelogram down
            //Parabolic movement
            float off_set = -25.31f; // RCM parallelogram down need -25.31 degree to be a right triangle
            float degree_threshhold = -25.31f * 2;
            float angle_for_greater_offset = (Math.Abs(angles[7]) - Math.Abs(off_set)) / 2 * (float)Math.PI / 180; // The angle that is angle < -25.31
            float angle_for_between = (Math.Abs(off_set) - Math.Abs(angles[7])) / 2 * (float)Math.PI / 180; // The angle that is -25.31< angle < 0
            float angle_for_greater_zero = (Math.Abs(angles[7]) + Math.Abs(off_set)) / 2 * (float)Math.PI / 180; // The angle that is 0 < angle
            float angle_default_to_highest = Math.Abs(off_set) / 2 * (float)Math.PI / 180; // Angle -25.31 in radian 
            float distance_default_to_highest = 2 * (float)Math.Sin(angle_default_to_highest) * (float)Math.Cos(angle_default_to_highest) * 20f; // The x distance from angle 0 to angle -25.31
            float joint5_x = 0;
            if (angles[7] >= 0) // Based on the angle, add/subtract x distance from the distance_default_to_highest
            {
                joint5_x = -(Math.Abs(2 * (float)Math.Sin(angle_for_greater_zero) * (float)Math.Cos(angle_for_greater_zero) * 20f) - distance_default_to_highest);
            }
            else if (angles[7] < 0 && angles[7] > off_set)
            {
                joint5_x = distance_default_to_highest - (Math.Abs(2 * (float)Math.Sin(angle_for_between) * (float)Math.Cos(angle_for_between) * 20f));
            }
            else
            {
                joint5_x = Math.Abs(2 * (float)Math.Sin(angle_for_greater_offset) * (float)Math.Cos(angle_for_greater_offset) * 20f) + distance_default_to_highest;
            }

            float height_default = 2 * (float)Math.Pow((float)Math.Sin(angle_default_to_highest), 2f) * 20f; // The y height value of angle 0
            float height_to_the_top = 20f - 20f * (float)Math.Sin(64.69 * (float)Math.PI / 180); // The y distance between default angle to the highest point
            float joint5_y = 0;
            if (0 < angles[7] || degree_threshhold > angles[7]) // Based on the angle, add y distance from the height_default/height_to_the_top
            {
                joint5_y = height_default - 2 * (float)Math.Pow((float)Math.Sin(angle_for_greater_zero), 2f) * 20f;
            }
            else
            {
                joint5_y = height_to_the_top - 2 * (float)Math.Pow((float)Math.Sin(angle_for_greater_offset), 2f) * 20f;
            }

            F6 = new Transform3DGroup();
            T = new TranslateTransform3D(joint5_x - theta_2_trans, joint5_y, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 0), new Point3D(joints[5].rotPointX, joints[5].rotPointY, joints[5].rotPointZ));
            F6.Children.Add(T);
            F6.Children.Add(R);
            F6.Children.Add(F2);

            // Swing arm front
            F7 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(joints[6].rotAxisX, joints[6].rotAxisY, joints[6].rotAxisZ), angles[7]), new Point3D(joints[6].rotPointX, joints[6].rotPointY, joints[6].rotPointZ));
            F7.Children.Add(T);
            F7.Children.Add(R);
            F7.Children.Add(F1);

            // Needle holder 
            F8 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(joints[7].rotAxisX, joints[7].rotAxisY, joints[7].rotAxisZ), angles[7]), new Point3D(joints[7].rotPointX, joints[7].rotPointY, joints[7].rotPointZ));
            F8.Children.Add(T);
            F8.Children.Add(R);
            F8.Children.Add(F1);

            // Needle 
            F9 = new Transform3DGroup();
            T = new TranslateTransform3D(0, -angles[8], 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 0), new Point3D(0, 0, 0));
            F9.Children.Add(T);
            F9.Children.Add(R);
            F9.Children.Add(F8);

            Matrix4x4 R0 = Matrix4x4.CreateFromYawPitchRoll(0, Convert.ToSingle((-theta_1) * Math.PI / 180), 0);
            Vector3D T0 = new Vector3D(theta_2_trans - 24.5, 0, 0);
            R0.M14 = Convert.ToSingle(T0.X);
            R0.M24 = Convert.ToSingle(T0.Y);
            R0.M34 = Convert.ToSingle(T0.Z);

            Matrix4x4 R1 = Matrix4x4.CreateFromYawPitchRoll(Convert.ToSingle((theta_2) * Math.PI / 180), 0, 0);
            Vector3D T1 = new Vector3D(193 - theta_2_trans, 0, 0);
            R1.M14 = Convert.ToSingle(T1.X);
            R1.M24 = Convert.ToSingle(T1.Y);
            R1.M34 = Convert.ToSingle(T1.Z);

            Matrix4x4 R2 = Matrix4x4.CreateFromYawPitchRoll(0, 0, 0);
            Vector3D T2 = new Vector3D(0, 0, -theta_3);
            R2.M14 = Convert.ToSingle(T2.X);
            R2.M24 = Convert.ToSingle(T2.Y);
            R2.M34 = Convert.ToSingle(T2.Z);

            Matrix4x4 E_final = R0 * R1 * R2;

            // The postion of the end effector
            Vector3D end_eff_coord = new Vector3D(E_final.M14, E_final.M24, E_final.M34);


            if (!is_simulate) // If not on IK calculation, simulate will be false, and the model will move
            {
                joints[0].model3d.Transform = F1; //ROT_Link joint (DOF1)
                joints[1].model3d.Transform = F2; //NUT (translation)
                joints[2].model3d.Transform = F3; //RCM_CenterLink (rotation)
                joints[3].model3d.Transform = F4; //RCM_SwingArm_Back
                joints[4].model3d.Transform = F5; //RCM_Parallelogram_Up
                joints[5].model3d.Transform = F6; //RCM_Parallelogram_Down
                joints[6].model3d.Transform = F7; //RCM_SwingArm_Front
                joints[7].model3d.Transform = F8; //Needle Holder (DOF2)
                joints[8].model3d.Transform = F9; //The Needle (translation)
                joints[11].model3d.Transform = F12; //The Needle (translation)
                Tx.Content = E_final.M14;
                Ty.Content = E_final.M24;
                Tz.Content = E_final.M34;
                end_effector.Transform = new TranslateTransform3D(end_eff_coord);

            }

            return E_final;
        }

    }
}
