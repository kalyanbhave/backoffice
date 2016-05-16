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
using SafeNetWS.creditcard.creditcardvalidator;
using SafeNetWS.utils;

namespace SafeNetWS.database.result
{

    /// <summary>
    /// Cette classe permet de définir le retour de la méthode
    /// de récupération des informations sur les cartes dans FrontOffice
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class EgenciaPanInfoResult
    {
        private string Token;
        private string PAN;
        private string CSC;
        private string TruncatedPAN;
        private string CardType;
        private string ShortCardType;
        private int MII;
        private string MIIIssuerCategory;


        public EgenciaPanInfoResult()
        {
            // Initialisation
        }

        /// <summary>
        /// Affectation des valeurs
        /// </summary>
        /// <param name="BOtoken">Token FrontOffice</param>
        /// <param name="pan">Numéro de carte</param>
        /// <param name="expirationDate">Date d'expiration</param>
        /// <param name="ri">Information validation numéro de carte</param>
        public void SetValues(string token, string pan, string csc, CardInfos ri)
        {
            this.Token = token;
            this.PAN = pan;
            this.CSC = csc;

            if (ri != null)
            {
                this.TruncatedPAN = ri.GetTruncatedPAN();
                this.CardType = ri.GetCardType();
                this.ShortCardType = ri.GetShortCardType();
                this.MII = ri.GetMII();
                this.MIIIssuerCategory = ri.GetMIIIssuerCategory();
            }
            else
            {
                this.TruncatedPAN = CreditCardVerifier.TruncatePan(pan);
            }
        }

        /// <summary>
        /// Retourne le token FrontOffice
        /// </summary>
        /// <returns>Token FrontOffice</returns>
        public string GetToken()
        {
            return this.Token;
        }

        /// <summary>
        /// Retourne le CSC
        /// </summary>
        /// <returns>VC</returns>
        public string GetCSC()
        {
            return this.CSC;
        }


        /// <summary>
        /// Retourne le numéro de carte
        /// </summary>
        /// <returns>Numéro de carte</returns>
        public string GetPAN()
        {
            return this.PAN;
        }

        /// <summary>
        /// Numéro de carte masqué
        /// </summary>
        /// <returns>Numéro de carte masqué</returns>
        public string GetTruncatedPAN()
        {
            return this.TruncatedPAN;
        }

        /// <summary>
        /// Retourne le type de carte
        /// AMEX, VISA, ...
        /// </summary>
        /// <returns>Type de carte</returns>
        public string GetCardType()
        {
            return this.CardType;
        }

        /// <summary>
        /// Retourne le type de carte court
        /// AX, VI, ...
        /// </summary>
        /// <returns>Type de carte court</returns>
        public string GetShortCardType()
        {
            return this.ShortCardType;
        }
        public int GetMII()
        {
            return this.MII;
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

    }
}