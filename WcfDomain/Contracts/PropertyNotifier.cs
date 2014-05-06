using System;
using System.ComponentModel;

namespace WcfDomain.Contracts
{
    [Serializable]
    public class PropertyNotifier : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void Refresh(string pr)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(pr));
            }
        }
    }
}