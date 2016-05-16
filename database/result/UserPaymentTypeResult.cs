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
    /// Cette classe permet de définir le retour
    /// de la méthode de récupération
    /// du moyen de paiement depuis Navision
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class UserPaymentTypeResult
    {
        private string PaymentType;
        private string Origin;
        private string Service;

        private string ErrorCode;
        private string ErrorMsg;

        private string CostCenter;

        public UserPaymentTypeResult()
        {
            // Initialisation
        }

        /// <summary>
        /// Affectation des valeurs
        /// </summary>
        /// <param name="paymentType">Type de paiement</param>
        /// <param name="origin">Origin du moyen de paiement</param>
        /// <param name="service">Service</param>
        /// <param name="errorCode">Code de l'erreur</param>
        /// <param name="errorMsg">Description du message d'erreur</param>
        /// <param name="cc1">Centre de cout 1</param>
        public void SetValues(string paymentType, string origin, string service
            , int errorCode, string errorMsg, string CostCenter)
        {
            this.PaymentType = paymentType;
            this.Origin = origin;
            this.Service = service;
            this.ErrorCode = errorCode.ToString();
            this.ErrorMsg = errorMsg;
            // Récupération du centre de cout 1 si 
            // le mode de paiement est rattaché au cc1
            this.CostCenter = CostCenter;
        }

        /// <summary>
        /// Retourne le type de moyen de paiement
        /// </summary>
        /// <returns>Type de moyen de paiement</returns>
        public string GetPaymentType()
        {
            return this.PaymentType;
        }

        /// <summary>
        /// Retourne l'origine du moyen de paiement
        /// </summary>
        /// <returns>Origine</returns>
        public string GetOrigin()
        {
            return this.Origin;
        }

        /// <summary>
        /// Retourne le service
        /// sur lequel est le moyen de paiement
        /// </summary>
        /// <returns>Service</returns>
        public string GetService()
        {
            return this.Service;
        }
     
        /// <summary>
        /// Retourn le code de l'erreur
        /// 0 = pas d'erreur
        /// </summary>
        /// <returns>Code erreur</returns>
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
        /// Indique s'il y a une erreur
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public bool IsError()
        {
            return (!GetErrorCode().Equals("0"));
        }

        /// <summary>
        /// Indique si le payment s'effectue par
        /// carte de credit
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public bool IsPaymentByCreditCard()
        {
            return GetPaymentType().Equals(Const.PaymentTypeCreditCardShort);
        }

        /// <summary>
        /// Retourne le centre de cout 1
        /// </summary>
        /// <returns>CC1</returns>
        public string GetCostCenter()
        {
            return this.CostCenter;
        }
    }
}