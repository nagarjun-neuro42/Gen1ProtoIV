using Accord.MachineLearning;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using HelixToolkit.Wpf;
using MathNet.Numerics;
using SharpDX.XInput;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Web.UI.WebControls;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Windows.Web.AtomPub;
using static System.Windows.Forms.LinkLabel;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;

namespace n42_Robot_PROTO_III
{
    public partial class Visualization_UserControl : UserControl

    {

        //-------------------------------------------------------------------------------------------------------------
        // *** Initialize the working environment ***
        //-------------------------------------------------------------------------------------------------------------     
        // Show Workspace

        //private Model3DGroup Initialize_Environment(List<string> model_list, int Count_Show_Workspace)
        private Model3DGroup Initialize_Environment(List<string> model_list)
        {

            try
            {

                //if (Count_Show_Workspace == 0)
                //{
                ModelImporter importer = new ModelImporter();

                //String base_path = "C:\\Users\\Neuro42_Robot\\Downloads\\Aug_10_2023\\Aug_10_2023\\3D_VisualizationProj\\Robot";

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

                var link0 = importer.Load(@"C:\Users\Neuro42_Robot\Downloads\Aug_10_2023\Aug_10_2023\3D_VisualizationProj\Robot\ROT_Link_v2.STL");
                GeometryModel3D base_model0 = link0.Children[0] as GeometryModel3D;
                base_model0.Material = base_materialGroup;
                base_model0.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link0));

                var link1 = importer.Load(@"C:\Users\Neuro42_Robot\Downloads\Aug_10_2023\Aug_10_2023\3D_VisualizationProj\Robot\Nut_v2.STL");
                GeometryModel3D base_model1 = link1.Children[0] as GeometryModel3D;
                base_model1.Material = base_materialGroup;
                base_model1.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link1));

                var link2 = importer.Load(@"C:\Users\Neuro42_Robot\Downloads\Aug_10_2023\Aug_10_2023\3D_VisualizationProj\Robot\RCM_CenterLink_v2.STL");
                GeometryModel3D base_model2 = link2.Children[0] as GeometryModel3D;
                base_model2.Material = base_materialGroup;
                base_model2.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link2));

                var link3 = importer.Load(@"C:\Users\Neuro42_Robot\Downloads\Aug_10_2023\Aug_10_2023\3D_VisualizationProj\Robot\RCM_SwingArm_Back_v2.STL");
                GeometryModel3D base_model3 = link3.Children[0] as GeometryModel3D;
                base_model3.Material = base_materialGroup;
                base_model3.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link3));

                var link4 = importer.Load(@"C:\Users\Neuro42_Robot\Downloads\Aug_10_2023\Aug_10_2023\3D_VisualizationProj\Robot\RCM_Parallelogram_Up_v2.STL");
                GeometryModel3D base_model4 = link4.Children[0] as GeometryModel3D;
                base_model4.Material = base_materialGroup;
                base_model4.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link4));

                var link5 = importer.Load(@"C:\Users\Neuro42_Robot\Downloads\Aug_10_2023\Aug_10_2023\3D_VisualizationProj\Robot\RCM_Parallelogram_Down_v2.STL");
                GeometryModel3D base_model5 = link5.Children[0] as GeometryModel3D;
                base_model5.Material = base_materialGroup;
                base_model5.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link5));

                var link6 = importer.Load(@"C:\Users\Neuro42_Robot\Downloads\Aug_10_2023\Aug_10_2023\3D_VisualizationProj\Robot\RCM_SwingArm_Front_v2.STL");
                GeometryModel3D base_model6 = link6.Children[0] as GeometryModel3D;
                base_model6.Material = base_materialGroup;
                base_model6.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link6));

                var link7 = importer.Load(@"C:\Users\Neuro42_Robot\Downloads\Aug_10_2023\Aug_10_2023\3D_VisualizationProj\Robot\NeedleHolder_v2.STL");
                GeometryModel3D base_model7 = link7.Children[0] as GeometryModel3D;
                base_model7.Material = base_materialGroup;
                base_model7.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link7));

                var link8 = importer.Load(@"C:\Users\Neuro42_Robot\Downloads\Aug_10_2023\Aug_10_2023\3D_VisualizationProj\Robot\Needle_v2.STL");
                GeometryModel3D base_model8 = link8.Children[0] as GeometryModel3D;
                base_model8.Material = base_materialGroup;
                base_model8.BackMaterial = base_materialGroup;
                joints.Add(new Joint(link8));

                //var link9 = importer.Load(@"C:\Users\hrhos\Documents\Aug_10_2023\N42_Robot_PROTO_III_V09\Robot\Workspace_v2.STL");
                //joints.Add(new Joint(link9));

                var link10 = importer.Load(@"C:\Users\Neuro42_Robot\Downloads\Aug_10_2023\Aug_10_2023\3D_VisualizationProj\Robot\Frame_v2.STL");
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
                var Workspace = importer.Load(@"C:\Users\Neuro42_Robot\Downloads\Aug_10_2023\Aug_10_2023\3D_VisualizationProj\Robot\Workspace_v2.STL");
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
                var Brain = importer.Load(@"C:\Users\Neuro42_Robot\Downloads\Aug_10_2023\Aug_10_2023\3D_VisualizationProj\Robot\Frame_v2.STL");
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
                    joints[2].rotPointX = joints[1].transAxisX + 12.12f;  // 12.12f is the frame offset from the end of the RCM center link to the nut
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
                    
                    //Brain
                    joints[11].rotAxisX = 0;
                    joints[11].rotAxisY = 1;
                    joints[11].rotAxisZ = 0;
                    joints[11].rotPointX = 0;
                    joints[11].rotPointY = 60;
                    joints[11].rotPointZ = 0;

                //}
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Robot Build Error:" + e.StackTrace);
            }
            return robot;
        }
    }
}