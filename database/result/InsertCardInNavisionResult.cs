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
using SafeNetWS.NavService;

namespace SafeNetWS.database.result
{

    /// <summary>
    /// Cette classe permet de définir le retour
    /// de la méthode d'insertion des cartes
    /// dans la base Navision
    /// Cette insertion retourne la référence de la carte
    /// et pour les cartes transactionnelles,
    /// on indique si un autre client utilise cette carte
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class InsertCardInNavisionResult
    {
        private string ExceptionCode;
        private string ExceptionDesc;
        private string CardReference;
        private string CardUsedByAnotherCustomer;
        private string Merchantflow;
        private string Enhancedflow;
        private string Operationin;
        private string Onlinecheck;

        public InsertCardInNavisionResult()
        {
        }


        public void SetValues(Nav_InsertPaymentCard ni)
        {
            // we need to handle exceptions

            // Let's check if we have an exception code
            NavException7 navExcep = ni.NavException[0];

            // retrieve exception code
            string exceptionCode = navExcep.NavExceptionCode[0];

            if (!String.IsNullOrEmpty(exceptionCode))
            {
                // We have an exception
                // Let's see how kind of error we have here
                this.ExceptionCode = exceptionCode;
                this.ExceptionDesc = navExcep.NavExceptionDesc[0];
            }
            else
            {
                this.CardReference = ni.InsertCardResult[0].CardReference;
                this.Merchantflow = ni.InsertCardResult[0].MerchandFlow;
                this.Enhancedflow = ni.InsertCardResult[0].EnhancedFlow;
                this.CardUsedByAnotherCustomer = ni.InsertCardResult[0].CardUsedByAnotherCustomer;
                this.Operationin = ni.InsertCardResult[0].Operation;
                this.Onlinecheck = ni.InsertCardResult[0].OnlineCheck;            
            }

        }

        public bool isError()
        {
            return (!String.IsNullOrEmpty(this.ExceptionCode));
        }

        public string GetExceptionCode()
        {
            return this.ExceptionCode;
        }

        public string GetExceptionDesc()
        {
            return this.ExceptionDesc;
        }


        /// <summary>
        /// Indicateur si la carte est utilisée
        /// par un autre client
        /// Uniquement renseigné pour ls cartes transactionnelles
        /// </summary>
        /// <returns></returns>
        public string GetCardUsedByAnotherCustomer()
        {
            return this.CardUsedByAnotherCustomer;
        }

        public string MerchantFlow()
        {
            return this.Merchantflow;
        }
        public string EnhancedFlow()
        {
            return this.Enhancedflow;
        }

        public string Operation()
        {
            return this.Operationin;
        }
        public string OnlineCheck()
        {
            return this.Onlinecheck;
        }
        public string GetCardReference()
        {
            return this.CardReference;
        }

    }
}