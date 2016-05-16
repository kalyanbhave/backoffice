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
using SafeNetWS.login;

namespace SafeNetWS.business.arguments.reader
{
    /// <summary>
    /// This class validate arguments
    /// </summary>
    public class ArgsLodgedCardReferences
    {
        private string Pos;
        private string CustomerCode;
        private string TravelerCode;

        private string Provider;



        public ArgsLodgedCardReferences(string pos, string CustomerCode, string TravelerCode
            , string Provider)
        {
            this.Pos = pos;
            this.CustomerCode = CustomerCode;
            this.TravelerCode = TravelerCode;
            this.Provider = Provider;
        }

        /// <summary>
        /// Validate arguments and trow exception
        /// is any informatation is missing
        /// </summary>
        public void Validate(UserInfo user)
        {
            if(String.IsNullOrEmpty(GetPOS())) throw new Exception("POS missing");
            if(String.IsNullOrEmpty(GetCustomerCode())) throw new Exception("Customer code is missing");
            if(String.IsNullOrEmpty(GetProvider())) throw new Exception("Payment provider is missing");
            // Values are provided
            // Let's correct market and provider
            SetPOS(Util.CorrectPos(user, GetPOS()));
            SetProvider(GetProvider().ToUpper());
        }

        /// <summary>
        /// Returns raveler code
        /// </summary>
        /// <returns>Traveler code</returns>
        public string GetTravelerCode()
        {
            return this.TravelerCode;
        }

        /// <summary>
        /// Set traveler code
        /// </summary>
        /// <param name="value">Traveler code</param>
        public void SetTravelerCode(string value)
        {
            this.TravelerCode = value;
        }

        /// <summary>
        /// Returns customer code
        /// </summary>
        /// <returns>Customer code</returns>
        public string GetCustomerCode()
        {
            return this.CustomerCode;
        }

        /// <summary>
        /// Set customer code
        /// </summary>
        /// <param name="value">Customer code</param>
        public void SetCustomerCode(string value)
        {
            this.CustomerCode = value;
        }

        /// <summary>
        /// Returns market
        /// </summary>
        /// <returns>Market</returns>
        public string GetPOS()
        {
            return this.Pos;
        }

        /// <summary>
        /// Set market
        /// </summary>
        /// <param name="value">Market</param>
        public void SetPOS(string value)
        {
            this.Pos=value;
        }

        /// <summary>
        /// Returns provider
        /// </summary>
        /// <returns>Provider</returns>
        public string GetProvider()
        {
            return this.Provider;
        }
        /// <summary>
        /// Set provider
        /// </summary>
        /// <param name="value">Provider</param>
        public void SetProvider(string value)
        {
            this.Provider = value;
        }

        /// <summary>
        /// Returns arguments
        /// </summary>
        /// <returns>Arguments</returns>
        public string GetValue()
        {
            return String.Format("Pos={0}, Customer code={1}, Traveler code={2}, Provider={3}",
                GetPOS()==null?String.Empty:GetPOS(),
                GetCustomerCode() == null ? String.Empty : GetCustomerCode(),
                GetTravelerCode() == null ? String.Empty : GetTravelerCode(),
                GetProvider() == null ? String.Empty : GetProvider());
        }
    }
}
