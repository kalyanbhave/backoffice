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

namespace SafeNetWS.database.result
{

    /// <summary>
    /// Cette classe permet de définir le retour de la méthode
    /// de récupération des informations étendues sur les cartes dans Navision
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class ExtendedPanInfoResult
    {
        private long Token;
        private string Pan;
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

        public ExtendedPanInfoResult()
        {
            // Initialisation
            this.MII = -1;
        }

        public void SetValues(long token, string pan, PanInfoResult pi)
        {
            this.Token = token;
            this.Pan = pan;
            this.ExpirationDate = pi.GetExpirationDate();
            this.ShortExpirationDate = pi.GetShortExpirationDate();
            this.ExtendedNo = pi.GetExtendedNo();
            this.CVC = pi.GetCvc();
            this.TruncatedPAN = pi.GetTruncatedPan();
            this.CardType = pi.GetCardType();
            this.ShortCardType = pi.GetShortCardType();
            this.MIIIssuerCategory = pi.GetMIIIssuerCategory();
            this.MII = pi.GetMII();
            this.MerchantFlow = pi.GetMerchantFlow();
            this.EnhancedFlow = pi.GetEnhancedFlow();
        }
        public string GetMerchantFlow()
        {
            return this.MerchantFlow;
        }
        public string GetEnhancedFlow()
        {
            return this.EnhancedFlow;
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
        public int GetMII()
        {
            return this.MII;
        }
        public string GetMIIIssuerCategory()
        {
            return this.MIIIssuerCategory;
        }

        public long GetToken()
        {
            return this.Token;
        }

        public string GetPan()
        {
            return this.Pan;
        }
    }
}