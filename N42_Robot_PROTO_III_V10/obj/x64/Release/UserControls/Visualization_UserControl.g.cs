﻿#pragma checksum "..\..\..\..\UserControls\Visualization_UserControl.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "219EA8901C7B1EB2B7090E8E7AB3DACDB898780DB8BAE5A22D717D1477079BE3"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using HelixToolkit.Wpf;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace n42_Robot_PROTO_III {
    
    
    /// <summary>
    /// Visualization_UserControl
    /// </summary>
    public partial class Visualization_UserControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 14 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid overall_grid;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal HelixToolkit.Wpf.HelixViewport3D robot_viewport;
        
        #line default
        #line hidden
        
        
        #line 64 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label Tx;
        
        #line default
        #line hidden
        
        
        #line 66 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label Ty;
        
        #line default
        #line hidden
        
        
        #line 68 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label Tz;
        
        #line default
        #line hidden
        
        
        #line 70 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider joint_DOF1;
        
        #line default
        #line hidden
        
        
        #line 71 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label DOF1Value;
        
        #line default
        #line hidden
        
        
        #line 72 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider joint_DOF2;
        
        #line default
        #line hidden
        
        
        #line 73 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label DOF2Value;
        
        #line default
        #line hidden
        
        
        #line 74 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider Needle;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label NeedleTransValue;
        
        #line default
        #line hidden
        
        
        #line 76 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TbX;
        
        #line default
        #line hidden
        
        
        #line 77 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TbY;
        
        #line default
        #line hidden
        
        
        #line 78 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TbZ;
        
        #line default
        #line hidden
        
        
        #line 79 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button button;
        
        #line default
        #line hidden
        
        
        #line 80 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button target;
        
        #line default
        #line hidden
        
        
        #line 81 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button home;
        
        #line default
        #line hidden
        
        
        #line 84 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ScaleBtn;
        
        #line default
        #line hidden
        
        
        #line 107 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Show_Workspace;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/N42_Robot_PROTO_III;component/usercontrols/visualization_usercontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.overall_grid = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.robot_viewport = ((HelixToolkit.Wpf.HelixViewport3D)(target));
            
            #line 27 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            this.robot_viewport.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.ViewPort3D_OnMouseLeftButtonUp);
            
            #line default
            #line hidden
            
            #line 28 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            this.robot_viewport.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.ViewPort3D_OnMouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Tx = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.Ty = ((System.Windows.Controls.Label)(target));
            return;
            case 5:
            this.Tz = ((System.Windows.Controls.Label)(target));
            return;
            case 6:
            this.joint_DOF1 = ((System.Windows.Controls.Slider)(target));
            
            #line 70 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            this.joint_DOF1.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.joint_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 7:
            this.DOF1Value = ((System.Windows.Controls.Label)(target));
            return;
            case 8:
            this.joint_DOF2 = ((System.Windows.Controls.Slider)(target));
            
            #line 72 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            this.joint_DOF2.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.joint_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 9:
            this.DOF2Value = ((System.Windows.Controls.Label)(target));
            return;
            case 10:
            this.Needle = ((System.Windows.Controls.Slider)(target));
            
            #line 74 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            this.Needle.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.joint_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 11:
            this.NeedleTransValue = ((System.Windows.Controls.Label)(target));
            return;
            case 12:
            this.TbX = ((System.Windows.Controls.TextBox)(target));
            
            #line 76 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            this.TbX.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.ReachingPoint_TextChanged);
            
            #line default
            #line hidden
            return;
            case 13:
            this.TbY = ((System.Windows.Controls.TextBox)(target));
            
            #line 77 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            this.TbY.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.ReachingPoint_TextChanged);
            
            #line default
            #line hidden
            return;
            case 14:
            this.TbZ = ((System.Windows.Controls.TextBox)(target));
            
            #line 78 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            this.TbZ.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.ReachingPoint_TextChanged);
            
            #line default
            #line hidden
            return;
            case 15:
            this.button = ((System.Windows.Controls.Button)(target));
            
            #line 79 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            this.button.Click += new System.Windows.RoutedEventHandler(this.GoToPosition);
            
            #line default
            #line hidden
            return;
            case 16:
            this.target = ((System.Windows.Controls.Button)(target));
            
            #line 80 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            this.target.Click += new System.Windows.RoutedEventHandler(this.SelectTargetPoint);
            
            #line default
            #line hidden
            return;
            case 17:
            this.home = ((System.Windows.Controls.Button)(target));
            
            #line 81 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            this.home.Click += new System.Windows.RoutedEventHandler(this.GoToHomePosition);
            
            #line default
            #line hidden
            return;
            case 18:
            this.ScaleBtn = ((System.Windows.Controls.Button)(target));
            
            #line 84 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            this.ScaleBtn.Click += new System.Windows.RoutedEventHandler(this.ClickMaximizeButton);
            
            #line default
            #line hidden
            return;
            case 19:
            this.Show_Workspace = ((System.Windows.Controls.Button)(target));
            
            #line 107 "..\..\..\..\UserControls\Visualization_UserControl.xaml"
            this.Show_Workspace.Click += new System.Windows.RoutedEventHandler(this.Show_Workspace_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

