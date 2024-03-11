using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;

namespace n42_Robot_PROTO_III
{

    public partial class SideView_UserControl : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        Model3DGroup robot = new Model3DGroup();
        public Model3D robot_Model { get; set; }

        public object TbX_Val { get; set; }
        public object TbY_Val { get; set; }
        public object TbZ_Val { get; set; }

        public Model3D model3d = null;
        ModelVisual3D visual;
        ModelVisual3D visual2;
        List<Joint> joints = null;
        Model3D sphere = null;
        Model3D end_effector = null;
        string basePath = "";



        public static T Clamp<T>(T value, T min, T max)
            where T : System.IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        }

        private double _txVal;
        public double TxVal
        {
            get { return _txVal; }
            set
            {
                _txVal = value;
                OnPropertyChanged("TxVal");
                HandleUserControlCommunication();
            }
        }

        private double _tzVal;
        public double TzVal
        {
            get { return _tzVal; }
            set
            {
                _tzVal = value;
                OnPropertyChanged("TzVal");
                HandleUserControlCommunication();
            }
        }

        static PolynomialRegression polyreg_X, polyreg_Y;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public SideView_UserControl()
        {
            InitializeComponent();
            basePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\3D_VisualizationProj\\Robot\\";

            List<string> model_list = new List<string>();

            model_list.Add("ROT_Link.STL");
            model_list.Add("Nut.STL");
            model_list.Add("RCM_CenterLink.STL");
            model_list.Add("RCM_SwingArm_Back.STL");
            model_list.Add("RCM_Parallelogram_Up.STL");
            model_list.Add("RCM_Parallelogram_Down.STL");
            model_list.Add("RCM_SwingArm_Front.STL");
            model_list.Add("NeedleHolder.STL");
            model_list.Add("Needle.STL");
            model_list.Add("Workspace.STL");
            model_list.Add("Frame.STL");

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


            robot_viewport.MouseLeftButtonUp += ViewPort3D_OnMouseLeftButtonUp;
            robot_viewport.MouseLeftButtonDown += ViewPort3D_OnMouseLeftButtonDown;
            //robot_viewport.RotateGesture = new MouseGesture(MouseAction.RightClick);
            //robot_viewport.PanGesture = new MouseGesture(MouseAction.RightClick);
            robot_viewport.Children.Add(visual);
            robot_viewport.Children.Add(visual2);

            //float[] angles = { joints[0].angle, joints[1].angle, joints[2].angle, joints[3].angle, joints[4].angle, joints[5].angle,
            //                    joints[6].angle, joints[7].angle, joints[8].angle };

            TransformCoordinates();
        }

        // Read polynomial regression training data set from csv
        public static double[][] ReadCoordinatesFromCSV(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var coordinates = new List<double[]>();

            foreach (var line in lines)
            {
                var splitLine = line.Split(',');

                if (splitLine.Length == 2) // We expect 2 coordinates (X, Y)
                {
                    var x = double.Parse(splitLine[0]);
                    var y = double.Parse(splitLine[1]);

                    coordinates.Add(new[] { x, y });
                }
            }

            return coordinates.ToArray();
        }


        static void TransformCoordinates()
        {
            // Read training data from csv file
            string sideViewCoordinatesPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Dataset\\x_z_sideViewData.csv";
            string mainViewCoordinatesPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Dataset\\x_z_mainViewData.csv";

            double[][] coordinates_SideView = ReadCoordinatesFromCSV(sideViewCoordinatesPath);
            double[][] coordinates_MainView = ReadCoordinatesFromCSV(mainViewCoordinatesPath);

            // Split coordinates into separate X and Y arrays
            double[] coordinates_SideView_X = coordinates_SideView.Select(point => point[0]).ToArray();
            double[] coordinates_SideView_Y = coordinates_SideView.Select(point => point[1]).ToArray();
            double[] coordinates_MainView_X = coordinates_MainView.Select(point => point[0]).ToArray();
            double[] coordinates_MainView_Y = coordinates_MainView.Select(point => point[1]).ToArray();

            // Create a new polynomial regression with 2 degree
            polyreg_X = new PolynomialRegression();
            polyreg_Y = new PolynomialRegression();

            // Perform the polynomial regression
            polyreg_X.Fit(coordinates_SideView_X, coordinates_MainView_X, degree: 2);
            polyreg_Y.Fit(coordinates_SideView_Y, coordinates_MainView_Y, degree: 2);
        }


        private void ViewPort3D_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            Point mousePos = e.GetPosition(robot_viewport);

            // Transform the coordinates
            double[] transformedCoordinates = new double[]
            {
                polyreg_X.Compute(mousePos.X),
                polyreg_Y.Compute(mousePos.Y)
            };

            this.TxVal = transformedCoordinates[0];
            this.TzVal = transformedCoordinates[1];

            // Clear previously added points
            point_canvas.Children.Clear();

            // Create a new point
            Ellipse point = new Ellipse
            {
                Width = 5,
                Height = 5,
                Fill = Brushes.Red
            };

            // Set the point's position
            Canvas.SetLeft(point, mousePos.X - point.Width / 2);
            Canvas.SetTop(point, mousePos.Y - point.Height / 2);

            // Add the point to the canvas
            point_canvas.Children.Add(point);
        }

        private void ViewPort3D_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Perform the hit test on the mouse's position relative to the viewport.
            HitTestResult result = VisualTreeHelper.HitTest(robot_viewport, e.GetPosition(robot_viewport));

            // mesh_result will be the mesh hit by the mouse
            RayMeshGeometry3DHitTestResult mesh_result = result as RayMeshGeometry3DHitTestResult;

        }

        private void HandleUserControlCommunication()
        {
            string TxVal = this.TxVal.ToString();

            CommunicationToVisUserControl_X.OnUpdated(TxVal);
        }


        public HitTestResultBehavior ResultCallback(HitTestResult result)
        {
            RayHitTestResult rayResult = result as RayHitTestResult;
            if (rayResult != null)
            {
                RayMeshGeometry3DHitTestResult rayMeshResult = rayResult as RayMeshGeometry3DHitTestResult;
                robot.Transform = new TranslateTransform3D(new Vector3D(rayResult.PointHit.X, rayResult.PointHit.Y, rayResult.PointHit.Z));
            }

            return HitTestResultBehavior.Continue;
        }



        // ****************************************************************************************************************
        // ****************************************************************************************************************
        private Model3DGroup Initialize_Environment(List<string> model_list)
        {
            try
            {
                ModelImporter importer = new ModelImporter();

                joints = new List<Joint>();

                System.Windows.Media.Media3D.Material mat_White = MaterialHelper.CreateMaterial(
                                                                  new SolidColorBrush(Color.FromRgb(253, 253, 253)));

                // Load the STL components (dimensions in mm) and assign white color to all ... All model3Ds are listed in [joints]
                foreach (string modelName in model_list)
                {
                    var base_materialGroup = new MaterialGroup();
                    Color baseColor = Colors.White;
                    EmissiveMaterial base_emissMat = new EmissiveMaterial(new SolidColorBrush(baseColor));
                    DiffuseMaterial base_diffMat = new DiffuseMaterial(new SolidColorBrush(baseColor));
                    SpecularMaterial base_specMat = new SpecularMaterial(new SolidColorBrush(baseColor), 200);
                    base_materialGroup.Children.Add(base_emissMat);
                    base_materialGroup.Children.Add(base_diffMat);
                    base_materialGroup.Children.Add(base_specMat);

                    var link = importer.Load(basePath + modelName);
                    GeometryModel3D base_model = link.Children[0] as GeometryModel3D;
                    base_model.Material = base_materialGroup;
                    base_model.BackMaterial = base_materialGroup;
                    joints.Add(new Joint(link));
                }

                // Assemble the STL parts and create the robot as a Modele3DGroup
                var ROT_Link = joints[0];
                this.robot.Children.Add(joints[0].model3d);

                var Nut = joints[1];
                this.robot.Children.Add(joints[1].model3d);

                var RCM_CenterLink = joints[2];
                this.robot.Children.Add(joints[2].model3d);

                var RCM_SwingArm_Back = joints[3];
                this.robot.Children.Add(joints[3].model3d);

                var RCM_Parallelogram_Up = joints[4];
                this.robot.Children.Add(joints[4].model3d);

                var RCM_Parallelogram_Down = joints[5];
                this.robot.Children.Add(joints[5].model3d);

                var RCM_SwingArm_Front = joints[6];
                this.robot.Children.Add(joints[6].model3d);

                var NeedleHolder = joints[7];
                this.robot.Children.Add(joints[7].model3d);

                var Needle = joints[8];
                this.robot.Children.Add(joints[8].model3d);

                var Frame = joints[10];
                this.robot.Children.Add(joints[10].model3d);

                // Create the workspace and make it transparent (alpha = 20%))
                var Workspace = importer.Load(basePath + model_list[9]);
                this.robot.Children.Add(new Joint(Workspace).model3d);
                GeometryModel3D Workspace_gem = Workspace.Children[0] as GeometryModel3D;
                var materialGroup = new MaterialGroup();
                //Color mainColor = Colors.White;
                Color transp = new Color()
                {
                    A = 20,
                    R = Colors.Transparent.R,
                    G = Colors.Transparent.G,
                    B = Colors.Transparent.B
                };
                EmissiveMaterial emissMat = new EmissiveMaterial(new SolidColorBrush(transp));
                materialGroup.Children.Add(emissMat);
                Workspace_gem.Material = materialGroup;
                Workspace_gem.BackMaterial = materialGroup;

                // Robot is complete ... assign it to the public variable
                this.robot_Model = robot;
                overall_grid.DataContext = this;

                //// Add the robot rotation to the Transform3DGroup
                Transform3DGroup myTransform3DGroup = new Transform3DGroup();
                RotateTransform3D robotTransformX = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 88));
                robotTransformX.CenterX = 0;
                robotTransformX.CenterY = 0;
                robotTransformX.CenterZ = 0;
                myTransform3DGroup.Children.Add(robotTransformX);
                robot.Transform = myTransform3DGroup;


                joints[0].angleMin = -40;
                joints[0].angleMax = +40;
                joints[0].rotAxisX = 1;
                joints[0].rotAxisY = 0;
                joints[0].rotAxisZ = 0;
                joints[0].rotPointX = 0;
                joints[0].rotPointY = 0;
                joints[0].rotPointZ = 0;

                joints[1].transMin = 0; // ? Determined by DOF2 slider
                joints[1].transMax = 0; // ? Determined by DOF2 slider

                joints[2].transMin = 0; // ? Determined by DOF2 slider
                joints[2].transMax = 0; // ? Determined by DOF2 slider
                joints[2].angleMin = 0; // ? Determined by DOF2 slider
                joints[2].angleMax = 0; // ? Determined by DOF2 slider
                joints[2].rotAxisX = 0;
                joints[2].rotAxisY = 0;
                joints[2].rotAxisZ = 1;
                joints[2].rotPointX = joints[1].transAxisX - 10;
                joints[2].rotPointY = 0;
                joints[2].rotPointZ = 0;

                joints[3].angleMin = -45;
                joints[3].angleMax = +45;
                joints[3].rotAxisX = 0;
                joints[3].rotAxisY = 0;
                joints[3].rotAxisZ = 1;
                joints[3].rotPointX = 68.5F;
                joints[3].rotPointY = 0;
                joints[3].rotPointZ = 0;

                joints[6].angleMin = -45;
                joints[6].angleMax = +45;
                joints[6].rotAxisX = 0;
                joints[6].rotAxisY = 0;
                joints[6].rotAxisZ = 1;
                joints[6].rotPointX = 118.5F;
                joints[6].rotPointY = 0;
                joints[6].rotPointZ = 0;

                joints[7].angleMin = -29.24f;
                joints[7].angleMax = +50.17f;
                joints[7].rotAxisX = 0;
                joints[7].rotAxisY = 0;
                joints[7].rotAxisZ = 1;
                joints[7].rotPointX = 167.7F;
                joints[7].rotPointY = 0;
                joints[7].rotPointZ = 0;

                joints[8].transMin = 0;
                joints[8].transMax = +150f;
                joints[8].angleMin = 0;
                joints[8].angleMax = +150f;

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Robot Build Error:" + e.StackTrace);
            }
            return robot;
        }

    }

    public static class CommunicationToVisUserControl_X
    {
        public delegate void UpdatedEventHandler(string txVal);

        public static event UpdatedEventHandler Updated;

        public static void OnUpdated(string txVal)
        {
            Updated?.Invoke(txVal);
        }
    }

}



