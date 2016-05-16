//==================================================================== 
// Credit Card Encryption/Decryption Tool
// 
// Copyright (c) 2009-2015 Egencia.  All rights reserved. 
// This software was developed by Egencia An Expedia Inc. Corporation
// La Defense. Paris. France
// The Original Code is Egencia 
// The Initial Developer is Sunil Pidugu (Sonata - Hyderabad).
//
//===================================================================

using System;
using System.Collections.Generic;
using System.Web;

namespace SafeNetWS.business.arguments.reader
{
    /// <summary>
    /// <GetVANDetails> 
    //      <PaymentID>3B077D9CA5A9072</PaymentID>
    //  </GetVANDetails>
    /// </summary>
    public class ENettGetVANDetails
    {

        private string PaymentIDfield = string.Empty;

        public string PaymentID
        {
            get { return PaymentIDfield; }
            set { PaymentIDfield = value; }
        }
    }
}