﻿using System.Collections.Generic;
using Hawk.Core.Utils;
using System.ComponentModel;
using System.Windows.Controls.WpfPropertyGrid.Attributes;
using Hawk.Core.Connectors;
using Hawk.Core.Utils.Plugins;
using Hawk.ETL.Interfaces;
using Hawk.ETL.Plugins.Transformers;
using Hawk.ETL.Process;

namespace Hawk.ETL.Plugins.Sorters
{
    public class ColumnDataSorterBase :ToolBase, IColumnDataSorter
    {
        public ColumnDataSorterBase()
        {
            Column = "";
            Enabled = true;
           
        }

        

        public override  FreeDocument DictSerialize(Scenario scenario = Scenario.Database)
        { 
            var dict = base.DictSerialize();
            dict.Add("Group", GlobalHelper.Get("key_104"));
            return dict;
        }

   
     

        [LocalizedCategory("key_211")]
        [LocalizedDisplayName("key_466")]
        public SortType SortType { get; set; }

     




        public virtual int Compare(IFreeDocument a, IFreeDocument b)
        {
            return 0;
        }

        public virtual bool Init(IList<IFreeDocument> datas)
        {
            return false;
        }

   


        public int Compare(object x, object y)
        {
            var a = x as IFreeDocument;
            if (a == null) return 0;
            var b = y as IFreeDocument;
            if (b == null) return 0;
            return Compare(a, b);
        }
    }
}