﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls.WpfPropertyGrid.Attributes;
using Hawk.Core.Connectors;
using Hawk.Core.Utils;
using Hawk.Core.Utils.Logs;
using Hawk.Core.Utils.Plugins;
using Hawk.ETL.Interfaces;

namespace Hawk.ETL.Plugins.Executor
{
    [XFrmWork("TableEX", "TableEX_desc", "column_three")]
    public class TableEX : DataExecutorBase
    {
        private readonly IDataManager dataManager;
        private DataCollection collection;

        public TableEX()
        {
            dataManager = MainDescription.MainFrm.PluginDictionary["DataManager"] as IDataManager;
        }
        [Browsable(false)]
        public override string KeyConfig => Table;

        [LocalizedDisplayName("key_22")]
        public string Table { get; set; }

        public override bool Init(IEnumerable<IFreeDocument> datas)
        {
        
            if (string.IsNullOrEmpty(Table))
                throw new ArgumentNullException("Table is empty");
            return base.Init(datas);
        }

        public override IEnumerable<IFreeDocument> Execute(IEnumerable<IFreeDocument> documents)
        {
            foreach (var document in documents)
            {
                var name = AppHelper.Query(Table, document);
                Monitor.Enter(dataManager);
                collection = dataManager.DataCollections.FirstOrDefault(d => d.Name == name);
                if (collection == null)
                {
                  
                    if (string.IsNullOrEmpty(name) == false)
                    {
                        collection = new DataCollection(new List<IFreeDocument>()) { Name = name };
                        dataManager.AddDataCollection(collection);
                    }

                }
                if (collection == null)
                {
                    XLogSys.Print.Error(GlobalHelper.Get("create_collection_error"));
                    yield return document;
                    continue;
                }
                
                Monitor.Exit(dataManager);
             
                    ControlExtended.UIInvoke(() =>
                    {
                        var data = document.Clone();
                        Monitor.Enter(collection);
                        collection.ComputeData.Add(data);
                        collection.OnPropertyChanged("Count");
                        Monitor.Exit(collection);
                    });
             
               

                yield return document;
            }
        }
    }
}