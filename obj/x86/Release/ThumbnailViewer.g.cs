﻿#pragma checksum "..\..\..\ThumbnailViewer.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "49EFD2A2D71146EFED61A10A13DDE8B8"
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
    /// ThumbnailViewer
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class ThumbnailViewer : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\..\ThumbnailViewer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Lefter;
        
        #line default
        #line hidden
        
        
        #line 9 "..\..\..\ThumbnailViewer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Righter;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\..\ThumbnailViewer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox DisplayModer;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\ThumbnailViewer.xaml"
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
            System.Uri resourceLocater = new System.Uri("/BiblioRap;component/thumbnailviewer.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\ThumbnailViewer.xaml"
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
            
            #line 4 "..\..\..\ThumbnailViewer.xaml"
            ((BiblioRap.ThumbnailViewer)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Lefter = ((System.Windows.Controls.Button)(target));
            
            #line 6 "..\..\..\ThumbnailViewer.xaml"
            this.Lefter.Click += new System.Windows.RoutedEventHandler(this.Lefter_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Righter = ((System.Windows.Controls.Button)(target));
            
            #line 9 "..\..\..\ThumbnailViewer.xaml"
            this.Righter.Click += new System.Windows.RoutedEventHandler(this.Righter_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.DisplayModer = ((System.Windows.Controls.ComboBox)(target));
            
            #line 12 "..\..\..\ThumbnailViewer.xaml"
            this.DisplayModer.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DisplayModer_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.Displayer = ((System.Windows.Controls.Image)(target));
            
            #line 14 "..\..\..\ThumbnailViewer.xaml"
            this.Displayer.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.Displayer_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

