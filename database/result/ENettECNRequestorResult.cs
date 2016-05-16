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
using SafeNetWS.utils;

namespace SafeNetWS.database.result
{

    /// <summary>
    /// This class returns ENettECN requestor from database
    /// Date : 15 May 2013
    /// Auteur : Samatar
    /// </summary>
    public class ENettECNRequestorResult
    {
        private int RequestorECN;
        private string IntegratorCode;
        private string IntegratorAccessKey;
        private string ClientAccessKey;
        private DateTime CreationDate;
        private string CreationUser;
       
        public ENettECNRequestorResult()
        {
            // Initialisation
            this.IntegratorCode = null;
        }

        public void SetValues(int RequestorECN, SqlDataReader dr)
        {
            this.RequestorECN = RequestorECN;
            this.IntegratorCode = dr["IntegratorCode"].ToString();
            this.IntegratorAccessKey = dr["IntegratorAccessKey"].ToString();
            this.ClientAccessKey = dr["ClientAccessKey"].ToString();
            this.CreationDate = Util.GetSQLDataTime(dr, "CreationDate");
            this.CreationUser = dr["CreationUser"].ToString();

        }
        public int GetRequestorECN()
        {
            return this.RequestorECN;
        }
        public string GetIntegratorCode()
        {
            return this.IntegratorCode;
        }
        public string GetIntegratorAccessKey()
        {
            return this.IntegratorAccessKey;
        }
        public string GetClientAccessKey()
        {
            return this.ClientAccessKey;
        }
        public DateTime GetCreationDate()
        {
            return this.CreationDate;
        }
        public string GetCreationUser()
        {
            return this.CreationUser;
        }
  
    }
}