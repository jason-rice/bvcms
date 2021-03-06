using CmsData.Infrastructure;
using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace CmsData
{
    [Table(Name = "dbo.FamilyExtra")]
    public partial class FamilyExtra : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private static PropertyChangingEventArgs emptyChangingEventArgs => new PropertyChangingEventArgs("");

        #region Private Fields

        private int _FamilyId;

        private string _Field;

        private string _StrValue;

        private DateTime? _DateValue;

        private DateTime _TransactionTime;

        private string _Data;

        private int? _IntValue;

        private bool? _BitValue;

        private string _FieldValue;

        private bool? _UseAllValues;

        private string _Type;

        private EntityRef<Family> _Family;

        #endregion

        #region Extensibility Method Definitions

        partial void OnLoaded();
        partial void OnValidate(System.Data.Linq.ChangeAction action);
        partial void OnCreated();

        partial void OnFamilyIdChanging(int value);
        partial void OnFamilyIdChanged();

        partial void OnFieldChanging(string value);
        partial void OnFieldChanged();

        partial void OnStrValueChanging(string value);
        partial void OnStrValueChanged();

        partial void OnDateValueChanging(DateTime? value);
        partial void OnDateValueChanged();

        partial void OnTransactionTimeChanging(DateTime value);
        partial void OnTransactionTimeChanged();

        partial void OnDataChanging(string value);
        partial void OnDataChanged();

        partial void OnIntValueChanging(int? value);
        partial void OnIntValueChanged();

        partial void OnBitValueChanging(bool? value);
        partial void OnBitValueChanged();

        partial void OnFieldValueChanging(string value);
        partial void OnFieldValueChanged();

        partial void OnUseAllValuesChanging(bool? value);
        partial void OnUseAllValuesChanged();

        partial void OnTypeChanging(string value);
        partial void OnTypeChanged();

        #endregion

        public FamilyExtra()
        {
            _Family = default(EntityRef<Family>);

            OnCreated();
        }

        #region Columns

        [Column(Name = "FamilyId", UpdateCheck = UpdateCheck.Never, Storage = "_FamilyId", DbType = "int NOT NULL", IsPrimaryKey = true)]
        [IsForeignKey]
        public int FamilyId
        {
            get => _FamilyId;

            set
            {
                if (_FamilyId != value)
                {
                    if (_Family.HasLoadedOrAssignedValue)
                    {
                        throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
                    }

                    OnFamilyIdChanging(value);
                    SendPropertyChanging();
                    _FamilyId = value;
                    SendPropertyChanged("FamilyId");
                    OnFamilyIdChanged();
                }
            }
        }

        [Column(Name = "Field", UpdateCheck = UpdateCheck.Never, Storage = "_Field", DbType = "nvarchar(50) NOT NULL", IsPrimaryKey = true)]
        public string Field
        {
            get => _Field;

            set
            {
                if (_Field != value)
                {
                    OnFieldChanging(value);
                    SendPropertyChanging();
                    _Field = value;
                    SendPropertyChanged("Field");
                    OnFieldChanged();
                }
            }
        }

        [Column(Name = "StrValue", UpdateCheck = UpdateCheck.Never, Storage = "_StrValue", DbType = "nvarchar(200)")]
        public string StrValue
        {
            get => _StrValue;

            set
            {
                if (_StrValue != value)
                {
                    OnStrValueChanging(value);
                    SendPropertyChanging();
                    _StrValue = value;
                    SendPropertyChanged("StrValue");
                    OnStrValueChanged();
                }
            }
        }

        [Column(Name = "DateValue", UpdateCheck = UpdateCheck.Never, Storage = "_DateValue", DbType = "datetime")]
        public DateTime? DateValue
        {
            get => _DateValue;

            set
            {
                if (_DateValue != value)
                {
                    OnDateValueChanging(value);
                    SendPropertyChanging();
                    _DateValue = value;
                    SendPropertyChanged("DateValue");
                    OnDateValueChanged();
                }
            }
        }

        [Column(Name = "TransactionTime", UpdateCheck = UpdateCheck.Never, Storage = "_TransactionTime", DbType = "datetime NOT NULL")]
        public DateTime TransactionTime
        {
            get => _TransactionTime;

            set
            {
                if (_TransactionTime != value)
                {
                    OnTransactionTimeChanging(value);
                    SendPropertyChanging();
                    _TransactionTime = value;
                    SendPropertyChanged("TransactionTime");
                    OnTransactionTimeChanged();
                }
            }
        }

        [Column(Name = "Data", UpdateCheck = UpdateCheck.Never, Storage = "_Data", DbType = "nvarchar")]
        public string Data
        {
            get => _Data;

            set
            {
                if (_Data != value)
                {
                    OnDataChanging(value);
                    SendPropertyChanging();
                    _Data = value;
                    SendPropertyChanged("Data");
                    OnDataChanged();
                }
            }
        }

        [Column(Name = "IntValue", UpdateCheck = UpdateCheck.Never, Storage = "_IntValue", DbType = "int")]
        public int? IntValue
        {
            get => _IntValue;

            set
            {
                if (_IntValue != value)
                {
                    OnIntValueChanging(value);
                    SendPropertyChanging();
                    _IntValue = value;
                    SendPropertyChanged("IntValue");
                    OnIntValueChanged();
                }
            }
        }

        [Column(Name = "BitValue", UpdateCheck = UpdateCheck.Never, Storage = "_BitValue", DbType = "bit")]
        public bool? BitValue
        {
            get => _BitValue;

            set
            {
                if (_BitValue != value)
                {
                    OnBitValueChanging(value);
                    SendPropertyChanging();
                    _BitValue = value;
                    SendPropertyChanged("BitValue");
                    OnBitValueChanged();
                }
            }
        }

        [Column(Name = "FieldValue", UpdateCheck = UpdateCheck.Never, Storage = "_FieldValue", DbType = "nvarchar(251)", IsDbGenerated = true)]
        public string FieldValue
        {
            get => _FieldValue;

            set
            {
                if (_FieldValue != value)
                {
                    OnFieldValueChanging(value);
                    SendPropertyChanging();
                    _FieldValue = value;
                    SendPropertyChanged("FieldValue");
                    OnFieldValueChanged();
                }
            }
        }

        [Column(Name = "UseAllValues", UpdateCheck = UpdateCheck.Never, Storage = "_UseAllValues", DbType = "bit")]
        public bool? UseAllValues
        {
            get => _UseAllValues;

            set
            {
                if (_UseAllValues != value)
                {
                    OnUseAllValuesChanging(value);
                    SendPropertyChanging();
                    _UseAllValues = value;
                    SendPropertyChanged("UseAllValues");
                    OnUseAllValuesChanged();
                }
            }
        }

        [Column(Name = "Type", UpdateCheck = UpdateCheck.Never, Storage = "_Type", DbType = "varchar(22) NOT NULL", IsDbGenerated = true)]
        public string Type
        {
            get => _Type;

            set
            {
                if (_Type != value)
                {
                    OnTypeChanging(value);
                    SendPropertyChanging();
                    _Type = value;
                    SendPropertyChanged("Type");
                    OnTypeChanged();
                }
            }
        }

        #endregion

        #region Foreign Key Tables

        #endregion

        #region Foreign Keys

        [Association(Name = "FK_FamilyExtra_Family", Storage = "_Family", ThisKey = "FamilyId", IsForeignKey = true)]
        public Family Family
        {
            get => _Family.Entity;

            set
            {
                Family previousValue = _Family.Entity;
                if (((previousValue != value)
                            || (_Family.HasLoadedOrAssignedValue == false)))
                {
                    SendPropertyChanging();
                    if (previousValue != null)
                    {
                        _Family.Entity = null;
                        previousValue.FamilyExtras.Remove(this);
                    }

                    _Family.Entity = value;
                    if (value != null)
                    {
                        value.FamilyExtras.Add(this);

                        _FamilyId = value.FamilyId;

                    }

                    else
                    {
                        _FamilyId = default(int);

                    }

                    SendPropertyChanged("Family");
                }
            }
        }

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
