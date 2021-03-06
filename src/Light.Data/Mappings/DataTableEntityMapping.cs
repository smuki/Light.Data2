﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace Light.Data
{
    /// <summary>
    /// Data table entity mapping.
    /// </summary>
    class DataTableEntityMapping : DataEntityMapping
    {
        readonly bool _isDataTableEntity;

        public bool IsDataTableEntity {
            get {
                return _isDataTableEntity;
            }
        }

        internal DataTableEntityMapping(Type type, string tableName, bool isDataEntity, bool isDataTableEntity)
            : base(type, tableName, isDataEntity)
        {
            _isDataTableEntity = isDataTableEntity;
            InitialFieldList();
        }

        DataFieldMapping _identityField;

        public DataFieldMapping IdentityField {
            get {
                return _identityField;
            }
        }

        public ReadOnlyCollection<DataFieldMapping> NoIdentityFields {
            get {
                return _noIdentityFieldList;
            }
        }

        ReadOnlyCollection<DataFieldMapping> _noIdentityFieldList;// = new List<DataFieldMapping> ();

        public ReadOnlyCollection<DataFieldMapping> PrimaryKeyFields {
            get {
                return _primaryKeyFieldList;
            }
        }

        ReadOnlyCollection<DataFieldMapping> _primaryKeyFieldList;// = new List<DataFieldMapping> ();

        public ReadOnlyCollection<DataFieldMapping> NoPrimaryKeyFields {
            get {
                return _noPrimaryKeyFieldList;
            }
        }

        ReadOnlyCollection<DataFieldMapping> _noPrimaryKeyFieldList;// = new List<DataFieldMapping> ();

        public ReadOnlyCollection<DataFieldMapping> CreateFieldList {
            get {
                return _createFieldList;
            }
        }

        ReadOnlyCollection<DataFieldMapping> _createFieldList;

        public ReadOnlyCollection<DataFieldMapping> UpdateFieldList {
            get {
                return _updateFieldList;
            }
        }

        ReadOnlyCollection<DataFieldMapping> _updateFieldList;

        public ReadOnlyCollection<DataFieldMapping> TimeStampFieldList {
            get {
                return _timeStampFieldList;
            }
        }

        ReadOnlyCollection<DataFieldMapping> _timeStampFieldList;

        /// <summary>
        /// Gets a value indicating whether this instance has identity.
        /// </summary>
        /// <value><c>true</c> if this instance has identity; otherwise, <c>false</c>.</value>
        public bool HasIdentity {
            get {
                return _identityField != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has primary key.
        /// </summary>
        /// <value><c>true</c> if this instance has primary key; otherwise, <c>false</c>.</value>
        public bool HasPrimaryKey {
            get {
                return _primaryKeyFieldList.Count > 0;
            }
        }

        /// <summary>
        /// Gets the primary key count.
        /// </summary>
        /// <value>The primary key count.</value>
        public int PrimaryKeyCount {
            get {
                return _primaryKeyFieldList.Count;
            }
        }

        void InitialFieldList()
        {
            List<DataFieldMapping> noIdentityTmpList = new List<DataFieldMapping>();
            List<DataFieldMapping> primaryKeyTmpList = new List<DataFieldMapping>();
            List<DataFieldMapping> noPrimaryKeyTmpList = new List<DataFieldMapping>();
            List<DataFieldMapping> createTmpList = new List<DataFieldMapping>();
            List<DataFieldMapping> updateTmpList = new List<DataFieldMapping>();

            foreach (FieldMapping field in _fieldList) {
                if (field is PrimitiveFieldMapping pfmapping) {
                    if (pfmapping.IsIdentity) {
                        if (IdentityField == null) {
                            _identityField = pfmapping;
                        }
                        else {
                            throw new LightDataException(string.Format(SR.MultipleIdentityField, ObjectType));
                        }
                    }
                    else {
                        noIdentityTmpList.Add(pfmapping);
                    }
                    if (pfmapping.IsPrimaryKey) {
                        primaryKeyTmpList.Add(pfmapping);
                    }
                    else {
                        noPrimaryKeyTmpList.Add(pfmapping);
                    }
                }
                else {
                    DataFieldMapping mapping = field as DataFieldMapping;
                    noIdentityTmpList.Add(mapping);
                    noPrimaryKeyTmpList.Add(mapping);
                }
            }

            _noIdentityFieldList = new ReadOnlyCollection<DataFieldMapping>(noIdentityTmpList);
            _primaryKeyFieldList = new ReadOnlyCollection<DataFieldMapping>(primaryKeyTmpList);
            _noPrimaryKeyFieldList = new ReadOnlyCollection<DataFieldMapping>(noPrimaryKeyTmpList);
            _createFieldList = new ReadOnlyCollection<DataFieldMapping>(noIdentityTmpList.FindAll(x => (x.FunctionControl & FunctionControl.Create) == FunctionControl.Create));
            _updateFieldList = new ReadOnlyCollection<DataFieldMapping>(noPrimaryKeyTmpList.FindAll(x => (x.FunctionControl & FunctionControl.Update) == FunctionControl.Update));
            _timeStampFieldList = new ReadOnlyCollection<DataFieldMapping>(noPrimaryKeyTmpList.FindAll(x => x.IsTimeStamp));

        }

        public object[] GetPrimaryKeys(object data)
        {
            object[] rawkeys = new object[PrimaryKeyCount];
            for (int i = 0; i < PrimaryKeyCount; i++) {
                DataFieldMapping field = PrimaryKeyFields[i];
                rawkeys[i] = field.Handler.Get(data);
            }
            return rawkeys;
        }

        public override object LoadAliasJoinTableData(DataContext context, IDataReader datareader, QueryState queryState, string aliasName)
        {
            object data = base.LoadAliasJoinTableData(context, datareader, queryState, aliasName);
            if (_isDataTableEntity) {
                UpdateDateTableEntity(data);
            }
            return data;
        }

        public override object LoadData(DataContext context, IDataReader datareader, object state)
        {
            object data = base.LoadData(context, datareader, state);
            if (_isDataTableEntity) {
                UpdateDateTableEntity(data);
            }
            return data;
        }

        public override object LoadJoinTableData(DataContext context, IDataReader datareader, QueryState queryState, string fieldPath)
        {
            object data = base.LoadJoinTableData(context, datareader, queryState, fieldPath);
            if (_isDataTableEntity) {
                UpdateDateTableEntity(data);
            }
            return data;
        }

        private void UpdateDateTableEntity(object data)
        {
            DataTableEntity tableEntity = data as DataTableEntity;
            tableEntity.LoadData();
            //if (tableEntity.IsAllowUpdatePrimaryKey()) {
            //    tableEntity.SetRawPrimaryKeys(GetRawKeys(tableEntity));
            //}
        }
    }
}
