//==================================================================== 
// Credit Card Encryption/Decryption Tool
// 
// Copyright (c) 2009-2015 Egencia.  All rights reserved. 
// This software was developed by Egencia An Expedia Inc. Corporation
// La Defense. Paris. France
// The Original Code is Egencia 
// The Initial Developer is Sunil Kumar Pidugu (from Sonata Hyderabad).
// Code was reviewed by Samatar Hassan
//===================================================================

using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;

namespace SafeNetWS.business.arguments.reader
{
    // Begin EGE-85532
    /// <summary>
    /// <CancelRequestVAN>
    ///      <ECN>223227</ECN>
    ///      <PaymentID>0</PaymentID>
    ///      <UserName>Website</UserName>
    ///      <CancelReason>Booking cancelled</CancelReason>
    /// </CancelRequestVAN>
    /// </summary>
    public class ENettCancelRequestVAN
    {
	
        private string ECNfield = string.Empty;
        private string PaymentIDfield = string.Empty;
        private string UserNamefield = string.Empty;
        private string CancelReasonfield = string.Empty;


        /// <summary>
        /// get, set ECN 
        /// </summary>
        public string ECN
        {
            get { return ECNfield; }
            set { ECNfield = value; }
        }

        /// <summary>
        /// get, set PaymentID
        /// </summary>
        public string PaymentID
        {
            get { return PaymentIDfield; }
            set { PaymentIDfield = value; }
        }

        /// <summary>
        /// get, set UserName
        /// </summary>
        public string UserName
        {
            get { return UserNamefield; }
            set { UserNamefield = value; }
        }

        /// <summary>
        /// get, set CancelReason
        /// </summary>
        public string CancelReason
        {
            get { return CancelReasonfield; }
            set { CancelReasonfield = value; }
        }

       
    }

   
}

// END EGE-85532