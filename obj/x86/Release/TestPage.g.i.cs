﻿#pragma checksum "..\..\..\TestPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "20D657F8FF2C07C911D88027E82382C7"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
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


namespace BiblioRap {
    
    
    /// <summary>
    /// TestPage
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class TestPage : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\..\TestPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Lefter;
        
        #line default
        #line hidden
        
        
        #line 9 "..\..\..\TestPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Right;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\..\TestPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox DisplayModer;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\TestPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image Displayer;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/BiblioRap;component/testpage.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\TestPage.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 4 "..\..\..\TestPage.xaml"
            ((BiblioRap.TestPage)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Lefter = ((System.Windows.Controls.Button)(target));
            
            #line 6 "..\..\..\TestPage.xaml"
            this.Lefter.Click += new System.Windows.RoutedEventHandler(this.Lefter_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Right = ((System.Windows.Controls.Button)(target));
            
            #line 9 "..\..\..\TestPage.xaml"
            this.Right.Click += new System.Windows.RoutedEventHandler(this.Righter_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.DisplayModer = ((System.Windows.Controls.ComboBox)(target));
            
            #line 12 "..\..\..\TestPage.xaml"
            this.DisplayModer.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DisplayModer_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.Displayer = ((System.Windows.Controls.Image)(target));
            
            #line 14 "..\..\..\TestPage.xaml"
            this.Displayer.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.Displayer_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
