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
    /// Cette classe permet de définir le retour
    /// de la méthode de sauvegarde (insertion ou mise à jour)
    /// des cartes dans Navision
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class SaveCardResult
    {
        public const string OperationUpdate = "UPDATE";
        public const string OperationUpdateNoChange = "UPDATE_NO_CHANGE";
        public const string OperationInsert = "INSERT";
        public const string OperationLookup = "LOOKUP";

        private string Reference;
        private int ServiceProvided;
        private string Operation;
        private int ServiceReturned;

        public SaveCardResult(int providedServiceGroup)
        {
            // Initialisation
            SetServiceProvided(providedServiceGroup);
        }

        /// <summary>
        /// Affectation des valeurs
        /// </summary>
        /// <param name="reference">Référence de la carte</param>
        /// <param name="operation">Opération</param>
        /// <param name="serviceReturned">Service surlequel la carte a été inséré/modifié</param>
        public void SetValues(string reference, string operation, int serviceReturned)
        {
            this.Reference = reference;
            this.Operation = operation;
            this.ServiceReturned = serviceReturned;
        }

        /// <summary>
        /// Affectation service
        /// </summary>
        /// <param name="service"></param>
        private void SetServiceProvided(int service)
        {
            this.ServiceProvided = service;
        }


        /// <summary>
        /// Retourne le service que le client a envoyé
        /// </summary>
        /// <returns></returns>
        public int GetServiceProvided()
        {
            return this.ServiceProvided;
        }
        /// <summary>
        /// Affectation service
        /// </summary>
        /// <param name="serviceReturned"></param>
        public void SetServiceReturned(int serviceReturned)
        {
            this.ServiceReturned = serviceReturned;
        }


        /// <summary>
        /// Retourne le service que le client a envoyé
        /// </summary>
        /// <returns></returns>
        public int GetServiceReturned()
        {
            return this.ServiceReturned;
        }

        /// <summary>
        /// Retourne la référence de la carte
        /// </summary>
        /// <returns>Référence de la carte</returns>
        public string GetReference()
        {
            return this.Reference;
        }
        /// <summary>
        /// Affectation référence carte
        /// </summary>
        /// <param name="value">Référence carte</param>
        public void SetReference(string value)
        {
            this.Reference = value;
        }
        /// <summary>
        /// Retourne l'opération
        /// </summary>
        /// <returns>Opération</returns>
        public string GetOperation()
        {
            return this.Operation;
        }
        /// <summary>
        /// Affectation de l'opération
        /// </summary>
        /// <returns>Opération</returns>
        public void SetOperation(string operation)
        {
            this.Operation = operation;
        }
    }
}