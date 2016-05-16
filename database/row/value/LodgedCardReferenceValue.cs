//==================================================================== 
// Credit Card Encryption/Decryption Tool
// 
// Copyright (c) 2009-2015 Egencia.  All rights reserved. 
// This software was developed by Egencia An Expedia Inc. Corporation
// La Defense. Paris. France
// The Original Code is Egencia 
// The Initial Developer is Samatar Hassan.
//
//===================================================================

using System;
using System.Data.SqlClient;


namespace SafeNetWS.database.row.value
{
    public class LodgedCardReferenceValue
    {
        private string Key;
        private string Label;
        private string Value;

        public LodgedCardReferenceValue(SqlDataReader reader)
        {
            SetKey(reader["KEY"].ToString());
            SetLabel(reader["LABEL"].ToString());
            SetValue(reader["VALUE"] == null ? null : reader["VALUE"].ToString());
        }

        public string GetLabel()
        {
            return this.Label;
        }
        private void SetLabel(string value)
        {
            this.Label = value;
        }

        public string GetKey()
        {
            return this.Key;
        }
        private void SetKey(string value)
        {
            this.Key = value;
        }
        public string GetValue()
        {
            return this.Value;
        }
        private void SetValue(string value)
        {
            this.Value = value;
        }

    }
}
