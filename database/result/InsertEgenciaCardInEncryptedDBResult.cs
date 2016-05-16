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
    ///  d'insertion des cartes Egencia dans la base des données encryptées
    ///  hébergée par le Front Office
    ///  Date : 19 avril 2012
    ///  Auteur : Samatar
    /// </summary>
    public class InsertEgenciaCardInEncryptedDBResult
    {
        private string Token;
        private string TruncatedPan;
        private string CardType;
        private string ShortCardType;
        private string ExpirationDate;
        private string ShortExpirationDate;
        private string CSC;
        private string MII;
        private string MIIIssuerCategory;

        // Informations de la carte
        private CardInfos ci;


        public InsertEgenciaCardInEncryptedDBResult()
        {
            // Initialisation des valeurs
        }

        public void SetValues(CardInfos card, string csc)
        {
            SetExpirationDate(Util.ConvertDateToString(card.GetExpirationDate(), Const.DateFormat_ddMMyyyyHHmmss));
            // Short Expiration
            SetShortExpirationDate(card.GetShortExpirationDate());
            // CSC
            SetCSC(csc);
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
        private void SetExpirationDate(string expirationDate)
        {
            this.ExpirationDate = expirationDate;
        }
        private void SetShortExpirationDate(string shortExpirationDate)
        {
            this.ShortExpirationDate = shortExpirationDate;
        }
        public void SetCSC(string value)
        {
            this.CSC = value;
        }
       
        public string GetCSC()
        {
            return this.CSC;
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

        public CardInfos GetCardInfos()
        {
            return this.ci;
        }
        public string GetExpirationDate()
        {
            return this.ExpirationDate;
        }
        public string GetShortExpirationDate()
        {
            return this.ShortExpirationDate;
        }

    }
}