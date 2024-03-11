using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;


namespace n42_Robot_PROTO_III
{
    public partial class Visualization_UserControl : UserControl
    {
        
        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE MOUSE CLICK EVENT ***
        //-------------------------------------------------------------------------------------------------------------

        private void ViewPort3D_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = e.GetPosition(robot_viewport);
            PointHitTestParameters hitParams = new PointHitTestParameters(mousePos);
            VisualTreeHelper.HitTest(robot_viewport, null, ResultCallback, hitParams);
        }

        private void ViewPort3D_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Perform the hit test on the mouse's position relative to the viewport.
            HitTestResult result = VisualTreeHelper.HitTest(robot_viewport, e.GetPosition(robot_viewport));

            // mesh_result will be the mesh hit by the mouse
            RayMeshGeometry3DHitTestResult mesh_result = result as RayMeshGeometry3DHitTestResult;

            if (currentSelectedModel != null)
                unselectModel();

            if (mesh_result != null)
            {
                // mesh_result.ModelHit --> is the 3D model hit by the mouse
                selectModel(mesh_result.ModelHit);
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE SELECT TARGET POINT BUTTON CLICK EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void SelectTargetPoint(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindowUpdate)System.Windows.Application.Current.MainWindow;
            var visDockPanel = mainWindow.VisDockPanel;

            // Check if UserControl is already maximized
            if (Grid.GetRowSpan(visDockPanel) == 4 && Grid.GetColumnSpan(visDockPanel) == 2)
            {
                // Minimize the UserControl
                Grid.SetRowSpan(visDockPanel, 4);
                Grid.SetColumnSpan(visDockPanel, 1);
                Grid.SetColumn(visDockPanel, 1);
                Grid.SetRow(visDockPanel, 0);

                // Restore the original Grid definitions
                mainWindow.Col1.Width = new GridLength(356, GridUnitType.Pixel);
                mainWindow.Row4.Height = new GridLength(140, GridUnitType.Pixel);
            }

            mainWindow.ViewPanel.Visibility = Visibility.Visible;
            mainWindow.RobotStatusPanel.Visibility = Visibility.Collapsed;
            mainWindow.ConnectionStatusPanel.Visibility = Visibility.Collapsed;
            mainWindow.ControlPanel.Visibility = Visibility.Collapsed;
            mainWindow.SimulatorPanel.Visibility = Visibility.Collapsed;
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE GO TO POSITION BUTTON CLICK EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void GoToPosition(object sender, RoutedEventArgs e)
        {

            if (TbX.Text == "" || TbY.Text == "" || TbZ.Text == "") // Handle empty input
            {
                MessageBox.Show("Please reselect your target position.", "Target position error!");
            }
            else
            {
                if (timer1.Enabled)
                {
                    button.Content = "Go to Position";
                    isAnimating = false;
                    go_to_enable = false;
                    counter = 0;
                    go_angles[0] = 0f;
                    go_angles[1] = 0f;
                    go_angles[7] = 0f;
                    go_angles[8] = 0f;
                    timer1.Stop();
                }
                else
                {
                    
                    start_pos_DOF1 = (float)joint_DOF1.Value;
                    start_pos_DOF2 = (float)joint_DOF2.Value;
                    start_pos_needle = (float)Needle.Value;
                    float[] angles = { (float)joint_DOF1.Value, 0, 0, 0, 0, 0, 0, (float)joint_DOF2.Value, (float)Needle.Value };
                    angles = IK_numerical_result(reachingPoint, angles);
                    final_pos_DOF1 = angles[0];
                    final_pos_DOF2 = angles[7];
                    final_pos_needle = angles[8];
                    if (angles[0] <= 40 && angles[0] >= -40 && angles[7] <= 50.17 && angles[7] >= -29.24 && angles[8] <= 150 && angles[8] >= 0)
                    {
                        sphere.Transform = new TranslateTransform3D(reachingPoint);
                        button.Content = "STOP";
                        isAnimating = true;
                        go_to_enable = true;

                        //IK stuff invented
                        onIK_ButtonClicked();

                        // This timer will trigger the model movement in the simulation
                        timer1.Start();
                    }
                    else
                    {
                        MessageBox.Show("Target point is out of the workspace! Please try again.", "Error!");
                        return;
                    }

                }
                MessageBox.Show("You have set the IK coordinates, please click the Move Robot Button", "Message!");
            }

        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE HOME POSITION BUTTON CLICK EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void GoToHomePosition(object sender, RoutedEventArgs e)
        {
            if (timer1.Enabled)
            {
                button.Content = "Go to Position";
                isAnimating = false;
                go_to_enable = false;
                counter = 0;
                go_angles[0] = 0f;
                go_angles[1] = 0f;
                go_angles[7] = 0f;
                go_angles[8] = 0f;
                timer1.Stop();
            }
            else
            {
                start_pos_DOF1 = (float)joint_DOF1.Value;
                start_pos_DOF2 = (float)joint_DOF2.Value;
                start_pos_needle = (float)Needle.Value;

                float[] angles = { (float)joint_DOF1.Value, 0, 0, 0, 0, 0, 0, (float)joint_DOF2.Value, (float)Needle.Value };
                reachingPoint = new Vector3D(Double.Parse("168.89"), Double.Parse("0"), Double.Parse("-0.92"));
                angles = IK_numerical_result(reachingPoint, angles);
                final_pos_DOF1 = angles[0];
                final_pos_DOF2 = angles[7];
                final_pos_needle = angles[8];
                if (Math.Abs(start_pos_DOF1 - final_pos_DOF1) < 0.1 && Math.Abs(start_pos_DOF2 - final_pos_DOF2) < 0.1 && Math.Abs(start_pos_needle - final_pos_needle) < 0.1)
                {
                    MessageBox.Show("You are at Home position.", "Warning!");
                }
                else
                {
                    sphere.Transform = new TranslateTransform3D(reachingPoint);
                    button.Content = "STOP";
                    isAnimating = true;
                    go_to_enable = true;
                    timer1.Start();
                }

            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE MAXIMIZE/MINIMIZE BUTTON CLICK EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void ClickMaximizeButton(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindowUpdate)System.Windows.Application.Current.MainWindow;
            var visDockPanel = mainWindow.VisDockPanel;

            // Check if UserControl is already maximized
            if (Grid.GetRowSpan(visDockPanel) == 4 && Grid.GetColumnSpan(visDockPanel) == 2)
            {
                // Minimize the UserControl
                Grid.SetRowSpan(visDockPanel, 4);
                Grid.SetColumnSpan(visDockPanel, 1);
                Grid.SetColumn(visDockPanel, 1);
                Grid.SetRow(visDockPanel, 0);

                // Restore the original Grid definitions
                mainWindow.Col1.Width = new GridLength(356, GridUnitType.Pixel);
                mainWindow.Row4.Height = new GridLength(140, GridUnitType.Pixel);
            }
            else
            {
                // Maximize the UserControl
                Grid.SetRowSpan(visDockPanel, 4);
                Grid.SetColumnSpan(visDockPanel, 2);
                Grid.SetColumn(visDockPanel, 0);
                Grid.SetRow(visDockPanel, 0);

                // Remove unnecessary Grid definitions
                mainWindow.Col1.Width = new GridLength(0, GridUnitType.Pixel);
                mainWindow.Row4.Height = new GridLength(0, GridUnitType.Pixel);
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE SLIDER VALUE CHANGE EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void joint_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isAnimating)
                return;
            if (joints == null)
                return;
            joints[0].angle = (float)joint_DOF1.Value;
            double DOF2_theta_prime = (90 - (float)joint_DOF2.Value) * Math.PI / 180;
            double nut_trans = 107 - (114.22 / Math.Sin(DOF2_theta_prime)) * Math.Sin(((180 - Math.Asin(41.57 / 114.22 * Math.Sin(DOF2_theta_prime)) * 180 / Math.PI - DOF2_theta_prime * 180 / Math.PI) * Math.PI / 180));
            joints[1].transAxisX = (float)nut_trans;
            joints[7].angle = (float)joint_DOF2.Value;
            joints[8].transAxisY = (float)Needle.Value;
            execute_fk();
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE TARGET POINT TEXTBOX TEXT CHANGE EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void ReachingPoint_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                reachingPoint = new Vector3D(Double.Parse(TbX.Text), Double.Parse(TbY.Text), Double.Parse(TbZ.Text));
                sphere.Transform = new TranslateTransform3D(reachingPoint);
            }
            catch (Exception exc)
            {

            }
        }

        private void Show_Workspace_Click(object sender, RoutedEventArgs e)
        {
            //MainWindowUpdate MW = new MainWindowUpdate();
            MainWindowUpdate.Count_Show_Workspace++;
            Initialize_3DModel();
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** HANDLE TARGET POINT TEXTBOX TEXT CHANGE EVENT ***
        //-------------------------------------------------------------------------------------------------------------
        private void Btn_SetTarget_Click(object sender, RoutedEventArgs e)
        {
            onGoTargetClicked();
        }

        protected virtual void onGoTargetClicked()
        {
            SetTargetButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void onIK_ButtonClicked()
        {
            SetIK_TargetButtonClicked?.Invoke(this, EventArgs.Empty);
        }

    }
}