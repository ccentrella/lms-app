﻿#pragma checksum "..\..\UpdateRecord.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "C0575CE9C5DC9FB0C99A45787C42C76D1533354D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using RecordPro;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
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


namespace RecordPro {
    
    
    /// <summary>
    /// UpdateRecord
    /// </summary>
    public partial class UpdateRecord : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 29 "..\..\UpdateRecord.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox Grade;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\UpdateRecord.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox User;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\UpdateRecord.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker RecordDate;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\UpdateRecord.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid data;
        
        #line default
        #line hidden
        
        
        #line 72 "..\..\UpdateRecord.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SaveButton;
        
        #line default
        #line hidden
        
        
        #line 77 "..\..\UpdateRecord.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button CancelButton;
        
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
            System.Uri resourceLocater = new System.Uri("/Record Pro;component/updaterecord.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\UpdateRecord.xaml"
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
            
            #line 8 "..\..\UpdateRecord.xaml"
            ((RecordPro.UpdateRecord)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 14 "..\..\UpdateRecord.xaml"
            ((System.Windows.Input.CommandBinding)(target)).CanExecute += new System.Windows.Input.CanExecuteRoutedEventHandler(this.CommandBinding_CanExecute);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Grade = ((System.Windows.Controls.ComboBox)(target));
            
            #line 34 "..\..\UpdateRecord.xaml"
            this.Grade.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.Grade_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.User = ((System.Windows.Controls.ComboBox)(target));
            
            #line 43 "..\..\UpdateRecord.xaml"
            this.User.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.User_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.RecordDate = ((System.Windows.Controls.DatePicker)(target));
            return;
            case 6:
            this.data = ((System.Windows.Controls.DataGrid)(target));
            
            #line 60 "..\..\UpdateRecord.xaml"
            this.data.CellEditEnding += new System.EventHandler<System.Windows.Controls.DataGridCellEditEndingEventArgs>(this.data_CellEditEnding);
            
            #line default
            #line hidden
            
            #line 61 "..\..\UpdateRecord.xaml"
            this.data.AutoGeneratingColumn += new System.EventHandler<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs>(this.data_AutoGeneratingColumn);
            
            #line default
            #line hidden
            return;
            case 7:
            this.SaveButton = ((System.Windows.Controls.Button)(target));
            
            #line 76 "..\..\UpdateRecord.xaml"
            this.SaveButton.Click += new System.Windows.RoutedEventHandler(this.SaveButton_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.CancelButton = ((System.Windows.Controls.Button)(target));
            
            #line 81 "..\..\UpdateRecord.xaml"
            this.CancelButton.Click += new System.Windows.RoutedEventHandler(this.CancelButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

