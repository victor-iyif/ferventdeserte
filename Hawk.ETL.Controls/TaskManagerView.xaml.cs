﻿using System.Windows.Controls;
using Hawk.Core.Utils.Plugins;
namespace Hawk.ETL.Controls
{
    /// <summary>
    /// TaskManagerView.xaml 的交互逻辑
    /// </summary>
    [XFrmWork("key_304" )]
    public partial class TaskManagerView : UserControl,ICustomView
    {
        public TaskManagerView()
        {
            InitializeComponent();
        }

        public FrmState FrmState => FrmState.Mini2;
    }
}
