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
using System.Collections;
using SafeNetWS.utils;


namespace SafeNetWS.database.result
{

    /// <summary>
    /// Cette classe permet de définir le retour
    /// de la méthode d'insertion des cartes
    /// dans la base des données encryptées et navision
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class InsertCardResult
    {
        private long Token;
        private string CardReference;
        private string Service;
        private string ExpirationDate;
        private string CardType;
        private string TruncatedPAN;
        private string CardUsedByAnotherCustomer;
        private string Merchantflow;
        private string Operation;


        public InsertCardResult()
        {
            // Initialisation
            this.Token = -1;
        }

        public void SetValues(long token, InsertCardInNavisionResult reference, string expirationDate,string cardType, string truncatedPAN, string service
            , int transactionalcard, string contextSource)
        {
            this.Token = token;
            // this.CardReferences = Convert.ToInt32(reference.GetCardReferences()); ;
            this.CardReference = reference.GetCardReference();
            this.Operation = reference.Operation();
            //Any service returned. i take the service provided as ServiceProvided and  ServiceReturned
            this.Service = service;
            this.ExpirationDate = expirationDate;
            this.CardType = cardType;
            this.TruncatedPAN = truncatedPAN;
            if (transactionalcard==1 && contextSource.Equals(Const.Context_Source_BU))
            {
                // set only for transactional card and context BU
                this.CardUsedByAnotherCustomer = reference.GetCardUsedByAnotherCustomer();
            }
            this.Merchantflow = reference.MerchantFlow();
        }


        public string GetCardReference()
        {
            return this.CardReference;
        }
        public long GetToken()
        {
            return this.Token;
        }

        public string GetExpirationDate()
        {
            return this.ExpirationDate;
        }
        public string GetCardType()
        {
            return this.CardType;
        }
        public string GetTruncatedPAN()
        {
            return this.TruncatedPAN;
        }
        public string GetCardUsedByAnotherCustomer()
        {
            return this.CardUsedByAnotherCustomer;
        }
        public string GetCardService()
        {
            return this.Service;
        }
        public string GetOperation()
        {
            return this.Operation;
        }

    }
}