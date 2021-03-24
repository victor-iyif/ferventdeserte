﻿using System.Collections.Generic;
using Hawk.Core.Utils.Plugins;

namespace Hawk.Core.Connectors
{
    [Interface("展示数据")]
    public interface IDataViewer
    {
        object SetCurrentView( IList<IDictionarySerializable> datas);
        /// <summary>
        /// 指示是否可编辑
        /// </summary>
        bool IsEditable { get; }
    }
}
