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
using SafeNetWS.creditcard;
using SafeNetWS.utils;

namespace SafeNetWS.database.result
{

    /// <summary>
    /// Cette classe permet de définir le retour de la méthode
    ///  d'insertion des cartes dans la base des données encryptées
    ///  hébergée par le Front Office
    ///  Date : 26 mars 2010
    ///  Auteur : Samatar
    /// </summary>
    public class InsertCardInEncryptedFODBResult
    {
        private string Token;
        private string TruncatedPan;
        private string CardType;
        private string ShortCardType;
        private string ExpirationDate;
        private string ShortExpirationDate;
        private string MII;
        private string MIIIssuerCategory;
        private string InformationCode;
        private string InformationMessage;

        private CardInfos ci;


        public InsertCardInEncryptedFODBResult()
        {
            // Initialisation des valeurs
        }

        public void SetValues(CardInfos card)
        {
            SetExpirationDate(Util.ConvertDateToString(card.GetExpirationDate(), Const.DateFormat_ddMMyyyyHHmmss));
            // Short Expiration
            SetShortExpirationDate(card.GetShortExpirationDate());
            // Type de carte
            SetCardType(card.GetCardType());
            // Type de carte (court)
            SetShortCardType(card.GetShortCardType());
            // PAN tronqué
            SetTruncatedPan(card.GetTruncatedPAN());
            // MII
            SetMII(card.GetMII());
            // MII Issuer Category
            SetMIIIssuerCategory(card.GetMIIIssuerCategory());
            this.ci = card;
        }
        public void SetInformationCode(string value)
        {
            this.InformationCode = value;
        }
        public string GetInformationCode()
        {
            return this.InformationCode;
        }
        public void SetInformationMessage(string value)
        {
            this.InformationMessage = value;
        }
        public string GetInformationMessage()
        {
            return this.InformationMessage;
        }
        private void SetMII(int mii)
        {
            this.MII = mii.ToString();
        }
        public string GetMII()
        {
            return this.MII;
        }
        private void SetMIIIssuerCategory(string miiIssuer)
        {
            this.MIIIssuerCategory = miiIssuer;
        }
        public string GetMIIIssuerCategory()
        {
            return this.MIIIssuerCategory;
        }
        public void SetToken(string token)
        {
            this.Token = token; 
        }
        private void SetCardType(string cardType)
        {
            this.CardType = cardType;
        }
        private void SetShortCardType(string shortCardType)
        {
            this.ShortCardType = shortCardType;
        }
        private void SetTruncatedPan(string truncatedPan)
        {
            this.TruncatedPan = truncatedPan;
        }
        private void SetExpirationDate(string expirationDate)
        {
            this.ExpirationDate = expirationDate;
        }
        private void SetShortExpirationDate(string shortExpirationDate)
        {
            this.ShortExpirationDate = shortExpirationDate;
        }
        public string GetToken()
        {
            return this.Token;
        }
        public string GetTruncatedPan()
        {
            return this.TruncatedPan;
        }
        public string GetCardType()
        {
            return this.CardType;
        }
        public string GetShortCardType()
        {
            return this.ShortCardType;
        }
        public string GetExpirationDate()
        {
            return this.ExpirationDate;
        }
        public string GetShortExpirationDate()
        {
            return this.ShortExpirationDate;
        }

        public CardInfos GetCardInfos()
        {
            return this.ci;
        }
    }
}