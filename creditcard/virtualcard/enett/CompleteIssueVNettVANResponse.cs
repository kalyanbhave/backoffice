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
using SafeNetWS.ENettService;

namespace SafeNetWS.creditcard.virtualcard.enett
{
    public class CompleteIssueVNettVANResponse
    {
        private string referenceId;
        private IssueVNettVANResponse issuedVNettReponse;

        public CompleteIssueVNettVANResponse()
        {
        }

        /// <summary>
        /// Return Issued VNett response
        /// </summary>
        /// <returns>Issued VNett response</returns>
        public IssueVNettVANResponse GetIssuedVNettResponse()
        {
            return this.issuedVNettReponse;
        }

        /// <summary>
        /// Set Issued VNett response
        /// </summary>
        /// <param name="value">Issued VNett response</param>
        public void SetIssuedVNettResponse(IssueVNettVANResponse value)
        {
            this.issuedVNettReponse = value;
        }

        /// <summary>
        /// Return reference id
        /// </summary>
        /// <returns>reference id</returns>
        public string GetReferenceId()
        {
            return this.referenceId;
        }

        /// <summary>
        /// Set reference id
        /// </summary>
        /// <param name="value">Reference id</param>
        public void SetReferenceId(string value)
        {
            this.referenceId = value;
        }
    }
}
