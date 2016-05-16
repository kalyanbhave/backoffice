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
    /// Cette classe permet de définir le retour de
    /// la méthode de récupération des 
    /// informations sur les cartes dans Navision
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class UserBookingPaymentRSResult
    {
        private long Token;
        private string TruncatedPAN;
        private DateTime ExpirationDate;
        private string ShortExpirationDate;
        private DateTime CreationDate;
        private string CardType;
        private string CardType2;
        private int MII;
        private string MIIIssuerCategory;
        private string origin;
        private string Cvv2;
        private string service;
        private int lodgedCard;
        private string FinancialFlow;

        private string ErrorCode;
        private string ErrorMsg;

        public UserBookingPaymentRSResult()
        {
            // Initialisation
            this.Token = -1;
            SetMII(-1);
        }

        public void SetValues(string token, string truncatedPan, DateTime expirationDate, DateTime creationDate,
            string cardType, string cardType2, string origin, string cvv2, string service, int lodgedCard, String financialFlow
            , string errorCode, string errorMsg)
        {
            this.Cvv2 = string.Empty;
            this.Token = Util.ConvertStringToToken(Util.Nvl(token, "-1"));
            this.TruncatedPAN = truncatedPan;
            this.ExpirationDate = expirationDate;
            SetShortExpirationDate(Util.GetShortExpirationDate(GetExpirationDate()));
            this.CreationDate = creationDate;
            this.CardType = cardType;
            this.CardType2 = cardType2;
            this.origin = origin;
            this.Cvv2 = cvv2;
            this.service = service;
            this.lodgedCard = lodgedCard;
            this.FinancialFlow = financialFlow;
            this.ErrorCode = errorCode;
            this.ErrorMsg = errorMsg;
        }

        /// <summary>
        /// Returne le numéro de carte masqué
        /// </summary>
        /// <returns>Numéro de carte masqué</returns>
        public string GetTruncatedPAN()
        {
            return this.TruncatedPAN;
        }

        /// <summary>
        /// Retourne la date d'expiration
        /// </summary>
        /// <returns>Date d'expiration</returns>
        public DateTime GetExpirationDate()
        {
            return this.ExpirationDate;
        }

        /// <summary>
        /// Retour la date de création du moyen de paiement
        /// </summary>
        /// <returns>Date de création</returns>
        public DateTime GetCreationDate()
        {
            return this.CreationDate;
        }

        /// <summary>
        /// Retourne l'origine
        /// </summary>
        /// <returns>Origine</returns>
        public string GetOrigin()
        {
            return this.origin;
        }

        /// <summary>
        /// Retourne le CVV2
        /// </summary>
        /// <returns>CVV2</returns>
        public string GetCvv2()
        {
            return this.Cvv2;
        }

        /// <summary>
        /// Retourne le service
        /// </summary>
        /// <returns>Service</returns>
        public string GetService()
        {
            return this.service;
        }

        /// <summary>
        /// Retourne le type de carte
        /// </summary>
        /// <returns>Type de carte</returns>
        public string GetCardType()
        {
            return this.CardType;
        }

        /// <summary>
        /// return 1 if it's lodged card
        /// </summary>
        /// <returns>1 or 0</returns>
        public int IsLodgedCard()
        {
            return this.lodgedCard;
        }


        /// <summary>
        /// Retourne le type de carte court
        /// </summary>
        /// <returns>Type de carte court</returns>
        public string GetCardType2()
        {
            return this.CardType2;
        }

        /// <summary>
        /// Retourne le token
        /// </summary>
        /// <returns>Token</returns>
        public long GetToken()
        {
            return this.Token ;
        }

        /// <summary>
        /// Retourne le code d'erreur
        /// s'il y a une erreur
        /// 0 = pas d'erreur
        /// </summary>
        /// <returns></returns>
        public string GetErrorCode()
        {
            return this.ErrorCode;
        }
        /// <summary>
        /// Retourne le message d'erreur
        /// </summary>
        /// <returns>Message d'erreur</returns>
        public string GetErrorMsg()
        {
            return this.ErrorMsg;
        }

        /// <summary>
        /// Returns financial flow
        /// </summary>
        /// <returns>Financial flow</returns>
        public string GetFinancialFlow()
        {
            return this.FinancialFlow;
        }

        /// <summary>
        /// Indique s'il y a une erreur
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public bool IsError()
        {
            return (!GetErrorCode().Equals("0"));
        }
        public string GetMII()
        {
            return Util.ConvertTokenToString(this.MII);
        }
        public void SetMII(int mii)
        {
            this.MII = mii;
        }
        public void SetMIIIssuerCategory(string miiIssuer)
        {
            this.MIIIssuerCategory = miiIssuer;
        }
        public string GetMIIIssuerCategory()
        {
            return this.MIIIssuerCategory;
        }
        /// <summary>
        /// Affectation de la date d'expiration courte
        /// en MM/YY
        /// </summary>
        /// <param name="shortExpirationDate">Date d'expiration</param>
        public void SetShortExpirationDate(string shortExpirationDate)
        {
            this.ShortExpirationDate = shortExpirationDate;
        }

        /// <summary>
        /// Retourne la date d'expiration en MM/YY
        /// </summary>
        /// <returns>Date d'expiration</returns>
        public string GetShortExpirationDate()
        {
            return this.ShortExpirationDate;
        }
    }
}