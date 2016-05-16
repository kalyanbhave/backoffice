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
    public class FOPanInfoResult
    {
        private long BOToken;
        private string PAN;
        private string TruncatedPAN;
        private DateTime ExpirationDate;
        private string ShortExpirationDate;
        private string CardType;
        private string ShortCardType;
        private int MII;
        private string MIIIssuerCategory;
        private string Cvc;
        private string NavisionCardLabel;
        private string NavisionCardType;
        private int IsNavisionPaymentAirplus;
        private int IsNavisionPaymentBTA;
        private int IsNavisionLodgedCard;

        public FOPanInfoResult()
        {
            // Initialisation
            BOToken = Const.EmptyBoToken;
        }

        /// <summary>
        /// Affectation des valeurs
        /// </summary>
        /// <param name="BOtoken">Token FrontOffice</param>
        /// <param name="pan">Numéro de carte</param>
        /// <param name="expirationDate">Date d'expiration</param>
        /// <param name="ri">Information validation numéro de carte</param>
        public void SetValues(long BOtoken, string pan, DateTime expirationDate,CardInfos ri)
        {
            this.BOToken = BOtoken;
            this.PAN = pan;
            this.TruncatedPAN = CreditCardVerifier.TruncatePan(pan);
            this.ExpirationDate = expirationDate;
            SetShortExpirationDate(Util.GetShortExpirationDate(GetExpirationDate()));
            if (ri != null)
            {
                this.Cvc = ri.GetCvc();
                this.CardType = ri.GetCardType();
                this.ShortCardType = ri.GetShortCardType();
                this.MII = ri.GetMII();
                this.MIIIssuerCategory = ri.GetMIIIssuerCategory();
                //--> EGE-62034 : Revamp - CCE - Change Financial flow update
                //this.NavisionCardLabel = ri.GetNavisionCardLabel();
                this.NavisionCardLabel = ri.GetNavisionFinancialFlow();
                //<-- EGE-62034 : Revamp - CCE - Change Financial flow update
                this.NavisionCardType = ri.GetNavisionCardType();
                this.IsNavisionPaymentAirplus = ri.GetNavisionPaymentAirPlus();
                this.IsNavisionPaymentBTA = ri.GetNavisionPaymentBTA();
                this.IsNavisionLodgedCard = ri.GetNavisionLodgedCard();
            }
        }

        /// <summary>
        /// Retourne le token FrontOffice
        /// </summary>
        /// <returns>Token FrontOffice</returns>
        public long GetBOToken()
        {
            return this.BOToken;
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
        /// Retourne la label Navision
        /// </summary>
        /// <returns>Label Navision</returns>
        public string GetNavisionCardLabel()
        {
            return this.NavisionCardLabel;
        }

        /// <summary>
        /// Retourne le CVC
        /// </summary>
        /// <returns>VC</returns>
        public string GetCvc()
        {
            return this.Cvc;
        }

        /// <summary>
        /// Retourne le type de carte Navision
        /// </summary>
        /// <returns>Type carte Navision</returns>
        public string GetNavisionCardType()
        {
            return this.NavisionCardType;
        }

        /// <summary>
        /// Indicateur payment Airplus (Navision)
        /// </summary>
        /// <returns>Indicateur payment Airplus</returns>
        public int GetNavisionPaymentAirPlus()
        {
            return this.IsNavisionPaymentAirplus;
        }

        /// <summary>
        /// Indicateur payment Diners (Navision)
        /// </summary>
        /// <returns>Indicateur payment Diners</returns>
        public int GetNavisionLodgedCard()
        {
            return this.IsNavisionLodgedCard;
        }

        /// <summary>
        /// Indicateur payment BTa (Navision)
        /// </summary>
        /// <returns>Indicateur payment BTA</returns>
        public int GetNavisionPaymentBTA()
        {
            return this.IsNavisionPaymentBTA;
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
        public void SetShortExpirationDate(string shortExpirationDate)
        {
            this.ShortExpirationDate = shortExpirationDate;
        }
        public string GetShortExpirationDate()
        {
            return this.ShortExpirationDate;
        }
    }
}