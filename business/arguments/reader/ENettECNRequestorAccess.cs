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
using SafeNetWS.utils;

namespace SafeNetWS.business.arguments.reader
{

    public class ENettECNRequestorAccess
    {
        private int RequestorECN;
        private string IntegratorCode;
        private string IntegratorAccessKey;
        private string ClientAccessKey;


        public ENettECNRequestorAccess(int RequestorECN, string IntegratorCode
            , string IntegratorAccessKey, string ClientAccessKey)
        {
            SetRequestorECN(RequestorECN);
            SetIntegratorCode(IntegratorCode);
            SetIntegratorAccessKey(IntegratorAccessKey);
            SetClientAccessKey(ClientAccessKey);

            // Validate values
            Validate();
        }


        private void Validate()
        {

        }

        /// <summary>
        /// Set Integrator Access Key
        /// </summary>
        /// <param name="value">Integrator Access Key</param>
        public void SetIntegratorAccessKey(string value)
        {
            this.IntegratorAccessKey = value;
        }


        /// <summary>
        /// Returns Integrator Access Key
        /// </summary>
        /// <returns>Integrator Access Key</returns>
        public string GetIntegratorAccessKey()
        {
            return this.IntegratorAccessKey;
        }

       /// <summary>
        /// Returns Requestor ECN
       /// </summary>
        /// <returns>Requestor ECN</returns>
        public int GetRequestorECN()
       {
           return this.RequestorECN;
       }

        /// <summary>
       /// Set Requestor ECN
        /// </summary>
       /// <param name="value">Requestor ECN</param>
       private void SetRequestorECN(int value)
       {
           this.RequestorECN = value;
       }


       /// <summary>
       /// Returns Client Access Key
       /// </summary>
       /// <returns>Client Access Key</returns>
       public string GetClientAccessKey()
       {
           return this.ClientAccessKey;
       }
       /// <summary>
       /// Set Client Access Key
       /// </summary>
       /// <param name="value">Client Access Key</param>
       private void SetClientAccessKey(string value)
       {
           this.ClientAccessKey = value;
       }

       /// <summary>
       /// Returns Integrator Code
       /// </summary>
       /// <returns>Integrator Code</returns>
       public string GetIntegratorCode()
       {
           return this.IntegratorCode;
       }

       /// <summary>
       /// Set Integrator Code
       /// </summary>
       /// <param name="value">Integrator Code</param>
       public void SetIntegratorCode(string value)
       {
           this.IntegratorCode = value;
       }

    }
}
