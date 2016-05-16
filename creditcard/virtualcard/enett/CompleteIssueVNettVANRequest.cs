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
    /// <summary>
    /// This class store reference id and IssueVNettVANRequest
    /// reference id is generated and IssueVNettVANRequest is 
    /// returned from ENett API
    /// </summary>
    public class CompleteIssueVNettVANRequest
    {
        private string referenceId;
        private IssueVNettVANRequest issuedVNettRequest;

        public CompleteIssueVNettVANRequest()
        {

        }

        /// <summary>
        /// Return Issued VNett resquest
        /// </summary>
        /// <returns>Issued VNett resquest</returns>
        public IssueVNettVANRequest GetIssuedVNettRequest()
        {
            return this.issuedVNettRequest;
        }

        /// <summary>
        /// Set Issued VNett resquest
        /// </summary>
        /// <param name="value">Issued VNett resquest</param>
        public void SetIssuedVNettRequest(IssueVNettVANRequest value)
        {
            this.issuedVNettRequest = value;
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
