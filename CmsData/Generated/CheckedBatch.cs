using System;
using System.ComponentModel;
using System.Data.Linq.Mapping;

namespace CmsData
{
    [Table(Name = "dbo.CheckedBatches")]
    public partial class CheckedBatch : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private static PropertyChangingEventArgs emptyChangingEventArgs => new PropertyChangingEventArgs("");

        #region Private Fields

        private string _BatchRef;

        private DateTime? _CheckedX;

        #endregion

        #region Extensibility Method Definitions

        partial void OnLoaded();
        partial void OnValidate(System.Data.Linq.ChangeAction action);
        partial void OnCreated();

        partial void OnBatchRefChanging(string value);
        partial void OnBatchRefChanged();

        partial void OnCheckedXChanging(DateTime? value);
        partial void OnCheckedXChanged();

        #endregion

        public CheckedBatch()
        {
            OnCreated();
        }

        #region Columns

        [Column(Name = "BatchRef", UpdateCheck = UpdateCheck.Never, Storage = "_BatchRef", DbType = "nvarchar(50) NOT NULL", IsPrimaryKey = true)]
        public string BatchRef
        {
            get => _BatchRef;

            set
            {
                if (_BatchRef != value)
                {
                    OnBatchRefChanging(value);
                    SendPropertyChanging();
                    _BatchRef = value;
                    SendPropertyChanged("BatchRef");
                    OnBatchRefChanged();
                }
            }
        }

        [Column(Name = "Checked", UpdateCheck = UpdateCheck.Never, Storage = "_CheckedX", DbType = "datetime")]
        public DateTime? CheckedX
        {
            get => _CheckedX;

            set
            {
                if (_CheckedX != value)
                {
                    OnCheckedXChanging(value);
                    SendPropertyChanging();
                    _CheckedX = value;
                    SendPropertyChanged("CheckedX");
                    OnCheckedXChanged();
                }
            }
        }

        #endregion

        #region Foreign Key Tables

        #endregion

        #region Foreign Keys

        #endregion

        public event PropertyChangingEventHandler PropertyChanging;
        protected virtual void SendPropertyChanging()
        {
            if ((PropertyChanging != null))
            {
                PropertyChanging(this, emptyChangingEventArgs);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void SendPropertyChanged(string propertyName)
        {
            if ((PropertyChanged != null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
