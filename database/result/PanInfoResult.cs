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

namespace SafeNetWS.database.result
{

    /// <summary>
    /// Cette classe permet de définir le retour de la méthode
    /// de récupération des informations sur les cartes dans Navision
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class PanInfoResult
    {
        private string ExpirationDate;
        private string ShortExpirationDate;
        private string ExtendedNo;
        private string CVC;
        public string TruncatedPAN;
        private string CardType;
        private string ShortCardType;
        private string MIIIssuerCategory;
        private int MII;
        private string MerchantFlow;
        private string EnhancedFlow;

        public PanInfoResult()
        {
            // Initialisation
        }

        public void SetValues(string expirationDate, string shortExpirationDate,string extendedNo, 
            string cVC, string truncatedPAN, 
            string cardType, string shortCardType, int mii, string miiIssuerCategory)
        {
            this.ExpirationDate = expirationDate;
            this.ShortExpirationDate = shortExpirationDate;
            this.ExtendedNo = extendedNo;
            this.CVC = cVC;
            this.TruncatedPAN = truncatedPAN;
            this.CardType = cardType;
            this.ShortCardType = shortCardType;
            this.MII = mii;
            this.MIIIssuerCategory = miiIssuerCategory;
        }
        
        public void SetValues(DateTime expirationDate, string extendedNo,
            string cVC, string truncatedPAN, string cardType, string shortCardType, string merchantFlow, string enhancedFlow)
        {
            this.ExpirationDate = Util.ConvertExpirationDateToString(expirationDate);
            this.ShortExpirationDate = Util.GetShortExpirationDate(expirationDate);
            this.ExtendedNo = extendedNo;
            this.CVC = cVC;
            this.TruncatedPAN = truncatedPAN;
            this.CardType = cardType;
            this.ShortCardType = shortCardType;
            this.MerchantFlow = merchantFlow;
            this.EnhancedFlow = enhancedFlow;
        }
        public string GetExpirationDate()
        {
            return this.ExpirationDate;
        }
        public string GetShortExpirationDate()
        {
            return this.ShortExpirationDate;
        }
        public string GetExtendedNo()
        {
            return this.ExtendedNo;
        }
        public string GetCvc()
        {
            return this.CVC;
        }
        public string GetTruncatedPan()
        {
            return this.TruncatedPAN;
        }
        public string GetCardType()
        {
            return this.CardType;
        }
        public string GetShortCardType()
        {
            return this.ShortCardType;
        }
        public string GetMerchantFlow()
        {
            return this.MerchantFlow;
        }

        public string GetEnhancedFlow()
        {
            return this.EnhancedFlow;
        }
        public int GetMII()
        {
            return this.MII;
        }
        public string GetMIIIssuerCategory()
        {
            return this.MIIIssuerCategory;
        }
    }
}