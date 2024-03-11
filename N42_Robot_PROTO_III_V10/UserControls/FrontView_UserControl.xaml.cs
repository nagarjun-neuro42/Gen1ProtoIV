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

    public partial class FrontView_UserControl : UserControl, INotifyPropertyChanged
    {
        Model3DGroup robot = new Model3DGroup();
        public Model3D robot_Model { get; set; }

        public object TbX_Val { get; set; }
        public object TbY_Val { get; set; }
        public object TbZ_Val { get; set; }

        private Model3D model3d = null;
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

        private double _tyVal;
        public double TyVal
        {
            get { return _tyVal; }
            set
            {
                _tyVal = value;
                OnPropertyChanged(nameof(TyVal));
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
                OnPropertyChanged(nameof(TzVal));
                HandleUserControlCommunication();
            }
        }

        static PolynomialRegression polyreg_X, polyreg_Y;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public FrontView_UserControl()
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
            model_list.Add("Brain.STL");

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
            string frontViewCoordinatesPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Dataset\\y_z_frontViewData.csv";
            string mainViewCoordinatesPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Dataset\\y_z_mainViewData.csv";

            double[][] coordinates_FrontView = ReadCoordinatesFromCSV(frontViewCoordinatesPath);
            double[][] coordinates_MainView = ReadCoordinatesFromCSV(mainViewCoordinatesPath);

            // Split coordinates into separate X and Y arrays
            double[] coordinates_FrontView_X = coordinates_FrontView.Select(point => point[0]).ToArray();
            double[] coordinates_FrontView_Y = coordinates_FrontView.Select(point => point[1]).ToArray();
            double[] coordinates_MainView_X = coordinates_MainView.Select(point => point[0]).ToArray();
            double[] coordinates_MainView_Y = coordinates_MainView.Select(point => point[1]).ToArray();

            // Create a new polynomial regression with 2 degree
            polyreg_X = new PolynomialRegression();
            polyreg_Y = new PolynomialRegression();

            // Perform the polynomial regression
            polyreg_X.Fit(coordinates_FrontView_X, coordinates_MainView_X, degree: 2);
            polyreg_Y.Fit(coordinates_FrontView_Y, coordinates_MainView_Y, degree: 2);
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

            this.TyVal = transformedCoordinates[0];
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
            string TyVal = this.TyVal.ToString();
            string TzVal = this.TzVal.ToString();

            CommunicationToVisUserControl_YZ.OnUpdated(TyVal, TzVal);
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
        private Model3DGroup Initialize_Environment(List<string> model_list_name)
        {
            try
            {

                //if (Count_Show_Workspace == 0)
                //{
                ModelImporter importer = new ModelImporter();

                joints = new List<Joint>();

                Material mat_White = MaterialHelper.CreateMaterial(
                                     new SolidColorBrush(Color.FromRgb(253, 253, 253)));
                var base_materialGroup = new MaterialGroup();
                Color baseColor = Colors.White;
                EmissiveMaterial base_emissMat = new EmissiveMaterial(new SolidColorBrush(baseColor));
                DiffuseMaterial base_diffMat = new DiffuseMaterial(new SolidColorBrush(baseColor));
                SpecularMaterial base_specMat = new SpecularMaterial(new SolidColorBrush(baseColor), 200);
                base_materialGroup.Children.Add(base_emissMat);
                base_materialGroup.Children.Add(base_diffMat);
                base_materialGroup.Children.Add(base_specMat);

                var link0 = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\ROT_Link_v2.STL");
                GeometryModel3D base_model0 = link0.Children[0] as GeometryModel3D;
                base_model0.Material = base_materialGroup;
                base_model0.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link0));

                var link1 = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\Nut_v2.STL");
                GeometryModel3D base_model1 = link1.Children[0] as GeometryModel3D;
                base_model1.Material = base_materialGroup;
                base_model1.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link1));

                var link2 = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\RCM_CenterLink_v2.STL");
                GeometryModel3D base_model2 = link2.Children[0] as GeometryModel3D;
                base_model2.Material = base_materialGroup;
                base_model2.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link2));

                var link3 = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\RCM_SwingArm_Back_v2.STL");
                GeometryModel3D base_model3 = link3.Children[0] as GeometryModel3D;
                base_model3.Material = base_materialGroup;
                base_model3.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link3));

                var link4 = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\RCM_Parallelogram_Up_v2.STL");
                GeometryModel3D base_model4 = link4.Children[0] as GeometryModel3D;
                base_model4.Material = base_materialGroup;
                base_model4.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link4));

                var link5 = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\RCM_Parallelogram_Down_v2.STL");
                GeometryModel3D base_model5 = link5.Children[0] as GeometryModel3D;
                base_model5.Material = base_materialGroup;
                base_model5.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link5));

                var link6 = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\RCM_SwingArm_Front_v2.STL");
                GeometryModel3D base_model6 = link6.Children[0] as GeometryModel3D;
                base_model6.Material = base_materialGroup;
                base_model6.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link6));

                var link7 = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\NeedleHolder_v2.STL");
                GeometryModel3D base_model7 = link7.Children[0] as GeometryModel3D;
                base_model7.Material = base_materialGroup;
                base_model7.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link7));

                var link8 = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\Needle_v2.STL");
                GeometryModel3D base_model8 = link8.Children[0] as GeometryModel3D;
                base_model8.Material = base_materialGroup;
                base_model8.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link8));

                //var link9 = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\Workspace_v2.STL");
                //joints.Add(new Joint(link9));

                var link10 = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\Frame_v2.STL");
                GeometryModel3D base_model10 = link10.Children[0] as GeometryModel3D;
                base_model10.Material = base_materialGroup;
                base_model10.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link10));

                //var link11 = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\Brain.STL");
                //joints.Add(new Joint(link11));

                // Load the STL components (dimensions in mm) and assign white color to all ... All model3Ds are listed in [joints]
                //foreach (string modelName in model_list)
                //{
                //    var base_materialGroup100 = new MaterialGroup();
                //    Color baseColor100 = Colors.White;
                //    EmissiveMaterial base_emissMat100 = new EmissiveMaterial(new SolidColorBrush(baseColor100));
                //    DiffuseMaterial base_diffMat100 = new DiffuseMaterial(new SolidColorBrush(baseColor100));
                //    SpecularMaterial base_specMat100 = new SpecularMaterial(new SolidColorBrush(baseColor100), 200);
                //    base_materialGroup100.Children.Add(base_emissMat100);
                //    base_materialGroup100.Children.Add(base_diffMat100);
                //    base_materialGroup100.Children.Add(base_specMat100);
                //    if (modelName != "Workspace_v2.STL" && modelName != "Brain.STL")
                //    {
                //        var link100 = importer.Load(Convert.ToString(basePath) + modelName);
                //        GeometryModel3D base_model100 = link100.Children[0] as GeometryModel3D;
                //        base_model100.Material = base_materialGroup100;
                //        base_model100.BackMaterial = base_materialGroup100;
                //        joints.Add(new Joint(link100));
                //    }

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

                var Frame = joints[9];
                this.robot.Children.Add(joints[9].model3d);

                //var Brain = joints[10];
                //this.robot.Children.Add(joints[10].model3d);


                // Create the workspace on the scene
                var Workspace = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\Workspace_v2.STL");
                var materialGroup_WS = new MaterialGroup();
                Color transp_WS = new Color();
                {
                    transp_WS.A = 10;
                    transp_WS.R = Colors.Transparent.R;
                    transp_WS.G = Colors.Transparent.G;
                    transp_WS.B = Colors.Transparent.B;
                };
                EmissiveMaterial emissMat_WS = new EmissiveMaterial(new SolidColorBrush(transp_WS));
                materialGroup_WS.Children.Add(emissMat_WS);
                GeometryModel3D Workspace_gem = Workspace.Children[0] as GeometryModel3D;
                Workspace_gem.Material = materialGroup_WS;
                Workspace_gem.BackMaterial = materialGroup_WS;
                joints.Add(new Joint(Workspace));
                this.robot.Children.Add(joints[10].model3d);

                // Create the Brain on the scene
                var Brain = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\Brain.STL");
                var materialGroup_Brain = new MaterialGroup();
                Color transp_B = new Color();
                {
                    transp_B.A = 40;
                    transp_B.R = Colors.Purple.R;
                    transp_B.G = Colors.Purple.G;
                    transp_B.B = Colors.Purple.B;
                };
                EmissiveMaterial emissMat_B = new EmissiveMaterial(new SolidColorBrush(transp_B));
                materialGroup_Brain.Children.Add(emissMat_B);
                GeometryModel3D Brain_gem = Brain.Children[0] as GeometryModel3D;
                Brain_gem.Material = materialGroup_Brain;
                Brain_gem.BackMaterial = materialGroup_Brain;
                joints.Add(new Joint(Brain));
                this.robot.Children.Add(joints[11].model3d);

                // Make the Workspace transparent or hidden (alpha = 0% or 20%))


                this.robot_Model = robot;
                overall_grid.DataContext = this;

                Transform3DGroup myTransform3DGroup = new Transform3DGroup();
                RotateTransform3D robotTransformX = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90));
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

    public static class CommunicationToVisUserControl_YZ
    {
        public delegate void UpdatedEventHandler(string tyVal, string tzVal);

        public static event UpdatedEventHandler Updated;

        public static void OnUpdated(string tyVal, string tzVal)
        {
            Updated?.Invoke(tyVal, tzVal);
        }
    }

}

