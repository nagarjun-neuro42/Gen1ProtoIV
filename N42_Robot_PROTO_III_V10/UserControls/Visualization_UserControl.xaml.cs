using Accord.Math;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using HelixToolkit.Wpf;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;

/**
 * Author: Hamidreza Hoshyarmanesh (Feb 2023)
 * This code reads/loads the 3d models of all the parts of the n42 robot and opens them in the robot_viewport.
 * The relationships among the joints and links of the robot are determined and the movement of the robot is demonstrated in 3D space
**/

namespace n42_Robot_PROTO_III
{

    //-------------------------------------------------------------------------------------------------------------
    // *** Initialization of the joints ***
    //------------------------------------------------------------------------------------------------------------- 
    class Joint
    {
        public Model3D model3d = null;
        public float angle = 0;
        public float angleMin = -180;
        public float angleMax = +180;
        public float rotPointX = 0;
        public float rotPointY = 0;
        public float rotPointZ = 0;
        public float rotAxisX = 0;
        public float rotAxisY = 0;
        public float rotAxisZ = 0;

        public float transAxisX = 0;
        public float transAxisY = 0;
        public float transAxisZ = 0;
        public float transMin = -100;
        public float transMax = +100;

        public Joint(Model3D pModel)
        {
            model3d = pModel;
        }
    }

    public partial class Visualization_UserControl : UserControl, INotifyPropertyChanged
    {

        Model3DGroup robot = new Model3DGroup();
        GeometryModel3D currentSelectedModel = null;
        Color currentColor = Colors.White;
        public Model3D robot_Model { get; set; }
        private string _tbX_Val;
        public string TbX_Val
        {
            get { return _tbX_Val; }
            set
            {
                _tbX_Val = value;
                OnPropertyChanged("TbX_Val");
            }
        }

        private string _tbY_Val;
        public string TbY_Val
        {
            get { return _tbY_Val; }
            set
            {
                _tbY_Val = value;
                OnPropertyChanged("TbY_Val");
            }
        }

        private string _tbZ_Val;
        public string TbZ_Val
        {
            get { return _tbZ_Val; }
            set
            {
                _tbZ_Val = value;
                OnPropertyChanged("TbZ_Val");
            }
        }

        public Model3D model3d;
        ModelVisual3D visual;
        ModelVisual3D visual2;
        List<Joint> joints;
        bool isAnimating = false;
        Model3D sphere;
        Model3D end_effector;
        string basePath = "";
        Transform3DGroup F1;
        Transform3DGroup F2;
        Transform3DGroup F3;
        Transform3DGroup F4;
        Transform3DGroup F5;
        Transform3DGroup F6;
        Transform3DGroup F7;
        Transform3DGroup F8;
        Transform3DGroup F9;
        Transform3DGroup F12; //Brain
        RotateTransform3D R;
        TranslateTransform3D T;
        Vector3D reachingPoint;
        float SamplingDistance = 0.15f;
        float DistanceThreshold = 0.5f;
        float LearningRate = 0.01f;
        float start_pos_DOF1 = 0f;
        float start_pos_DOF2 = 0f;
        float start_pos_needle = 0f;
        float final_pos_DOF1 = 0f;
        float final_pos_DOF2 = 0f;
        float final_pos_needle = 0f;
        int total_time = 500;
        int counter = 0;
        bool is_simulate = false;
        bool go_to_enable = false;
        float[] go_angles = { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        private double[] jointValues = new double[2];
        private double[] IKjointValues = new double[2];

        System.Windows.Forms.Timer timer1;

        // Event handler for SetTargetButton
        public event EventHandler SetTargetButtonClicked;
        // Event handler for IKTargetButton
        public event EventHandler SetIK_TargetButtonClicked;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Visualization_UserControl()
        {
            Initialize_3DModel();
        }
        public void Initialize_3DModel()
        {
           
            InitializeComponent();
            basePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Robot\\";
            //basePath = "C:\\Users\\hrhos\\Documents\\Aug_10_2023\\N42_Robot_PROTO_III_V09\\bin\\x64\\3D_VisualizationProj\\Robot";
            //basePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\3D_VisualizationProj\\Robot\\";
            //Debug.WriteLine("Base path in Visualization_UserControl:" + basePath);

            List<string> model_list = new List<string>();

            model_list.Add("ROT_Link_v2.STL");
            model_list.Add("Nut_v2.STL");
            model_list.Add("RCM_CenterLink_v2.STL");
            model_list.Add("RCM_SwingArm_Back_v2.STL");
            model_list.Add("RCM_Parallelogram_Up_v2.STL");
            model_list.Add("RCM_Parallelogram_Down_v2.STL");
            model_list.Add("RCM_SwingArm_Front_v2.STL");
            model_list.Add("NeedleHolder_v2.STL");
            model_list.Add("Needle_v2.STL");
            model_list.Add("Workspace_v2.STL");
            model_list.Add("Frame_v2.STL");
            model_list.Add("Brain.STL");

            //Initialize_Environment(model_list, MainWindowUpdate.Count_Show_Workspace);
            Initialize_Environment(model_list);

            /** Debug sphere to check in which point the joint is rotating**/
            var builder = new MeshBuilder(true, true);
            var builder2 = new MeshBuilder(true, true);
            var position = new Point3D(0, 0, 0);
            builder.AddSphere(position, 5, 15, 15);
            builder2.AddSphere(position, 5, 15, 15);
            sphere = new GeometryModel3D(builder.ToMesh(), Materials.Brown);
            end_effector = new GeometryModel3D(builder.ToMesh(), Materials.Brown);
            visual = new ModelVisual3D();
            visual.Content = sphere;
            visual2 = new ModelVisual3D();
            visual2.Content = end_effector;

            robot_viewport.RotateGesture = new MouseGesture(MouseAction.RightClick);
            robot_viewport.PanGesture = new MouseGesture(MouseAction.LeftClick);
            robot_viewport.Children.Add(visual);
            robot_viewport.Children.Add(visual2);
            CommunicationToVisUserControl_YZ.Updated += UpdateTextBoxes_YZ;
            CommunicationToVisUserControl_X.Updated += UpdateTextBoxes_X;
            //float[] angles = { joints[0].angle, joints[1].angle, joints[2].angle, joints[3].angle, joints[4].angle, joints[5].angle,
            //                    joints[6].angle, joints[7].angle, joints[8].angle };

            joint_DOF1.Value = 0;
            joint_DOF2.Value = 0;
            Needle.Value = 1;
            timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 1;
            timer1.Tick += new System.EventHandler(timer1_Tick);

        }

        //-------------------------------------------------------------------------------------------------------------
        // *** Update the Y, Z input box in the front end ***
        //------------------------------------------------------------------------------------------------------------- 
        private void UpdateTextBoxes_YZ(string tyVal, string tzVal)
        {

            double tyValDouble;
            double tzValDouble;

            if (double.TryParse(tyVal, out tyValDouble) && double.TryParse(tzVal, out tzValDouble))
            {
                // If parsing is successful, format the value to two decimal places
                TbY_Val = string.Format("{0:0.00}", tyValDouble);
                TbZ_Val = string.Format("{0:0.00}", tzValDouble);
            }
            else
            {
                // If parsing is not successful, assign default values 0.00
                TbY_Val = "0.00";
                TbZ_Val = "0.00";
            }
        }
    

        //-------------------------------------------------------------------------------------------------------------
        // *** Update the X input box in the front end ***
        //------------------------------------------------------------------------------------------------------------- 
        private void UpdateTextBoxes_X(string txVal)
        {

            double txValDouble;

            if (double.TryParse(txVal, out txValDouble))
            {
                // If parsing is successful, format the value to two decimal places
                TbX_Val = string.Format("{0:0.00}", txValDouble);
            }
            else
            {
                // If parsing is not successful, assign default values 0.00
                TbX_Val = "0.00";
            }
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** Change the color of the selected part ***
        //------------------------------------------------------------------------------------------------------------- 
        /// <summary>
        /// select the 3D model hit by the mouse and change its color to "#ff3333"
        /// </summary>
        private void selectModel(Model3D pModel)
        {
            try
            {
                Model3DGroup models = ((Model3DGroup)pModel);
                currentSelectedModel = models.Children[0] as GeometryModel3D;
            }
            catch (Exception exc)
            {
                currentSelectedModel = (GeometryModel3D)pModel;
            }
            if (currentSelectedModel != robot.Children[10])
            {
                currentColor = changeModelColor(currentSelectedModel, ColorHelper.HexToColor("#FF38BF9D"));

            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** Recover the original color of the selected part ***
        //------------------------------------------------------------------------------------------------------------- 
        private void unselectModel()
        {
            changeModelColor(currentSelectedModel, currentColor);
        }


        public HitTestResultBehavior ResultCallback(HitTestResult result)
        {
            // Did we hit 3D?
            RayHitTestResult rayResult = result as RayHitTestResult;
            if (rayResult != null)
            {
                // Did we hit a MeshGeometry3D?
                RayMeshGeometry3DHitTestResult rayMeshResult = rayResult as RayMeshGeometry3DHitTestResult;
                robot.Transform = new TranslateTransform3D(new Vector3D(rayResult.PointHit.X, rayResult.PointHit.Y, rayResult.PointHit.Z));

                if (rayMeshResult != null)
                {
                    // Yes we did!
                }
            }

            return HitTestResultBehavior.Continue;
        }

        //-------------------------------------------------------------------------------------------------------------
        // *** A changing color helper function ***
        //------------------------------------------------------------------------------------------------------------- 
        private Color changeModelColor(GeometryModel3D pModel, Color newColor)
        {
            if (pModel == null)
                return currentColor;

            Color previousColor = Colors.Black;

            MaterialGroup mg = (MaterialGroup)pModel.Material;
            if (mg.Children.Count > 0)
            {
                try
                {
                    previousColor = ((EmissiveMaterial)mg.Children[0]).Color;
                    ((EmissiveMaterial)mg.Children[0]).Color = newColor;
                    ((DiffuseMaterial)mg.Children[1]).Color = newColor;
                }
                catch (Exception exc)
                {
                    previousColor = currentColor;
                }
            }

            return previousColor;
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** Get joint values when moved slider ***
        //-------------------------------------------------------------------------------------------------------------
        public double[] GetCurrentJointValues()
        {
            jointValues[0] = joint_DOF1.Value;
            jointValues[1] = joint_DOF2.Value;
            return jointValues;
        }


        //-------------------------------------------------------------------------------------------------------------
        // *** Get Final joint values from IK button ***
        //-------------------------------------------------------------------------------------------------------------
        public double[] GetCurrentIK_JointValues()
        {
            IKjointValues[0] = final_pos_DOF1;
            IKjointValues[1] = final_pos_DOF2;
            return IKjointValues;
        }

    }
}
