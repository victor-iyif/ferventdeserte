using Hawk.Core.Utils.MVVM;

namespace Hawk.Core.Utils.Plugins
{
    /// <summary>
    /// ��ϵͳ��������ʾ�˵��Ľӿ�
    /// </summary>
    public interface IMainFrmMenu
    {
        IAction BindingCommands { get; }
    }
}