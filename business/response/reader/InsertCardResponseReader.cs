﻿//==================================================================== 
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
using System.Xml;
using System.IO;
using SafeNetWS.utils;

namespace SafeNetWS.business.response.reader
{

    /// <summary>
    //
    // Cette classe permet de lire la réponse apportée
    // par les méthodes d'insertion de cartes dans la base 
    // des données encryptées et Navision
    // L'entrée réponse est structurée de la manière suivante :
    // <?xml version="1.0" encoding="ISO-8859-1"?>
    // <Response>
    //   <Duration>Valeur de retour</Duration>
    //   <Value>
    //      <Token>Valeur de retour</Token>
    //      <CardReferences>
    //          <CardReference>
    //              <ServiceProvided>Valeur</ServiceProvided>
    //              <ServiceReturned>Valeur</ServiceReturned>
    //              <Reference>Valeur</Reference>
    //              <Operation>Valeur</Operation>
    //          </CardReference>
    //          <CardReference>
    //              <ServiceProvided>Valeur</ServiceProvided>
    //              <ServiceReturned>Valeur</ServiceReturned>
    //              <Reference>Valeur</Reference>
    //              <Operation>Valeur</Operation>
    //          </CardReference>
    //      </CardReferences>	
    //      <ExpirationDate>Valeur de retour</ExpirationDate>
    //      <CardType>Valeur de retour</CardType>
    //      <TruncatedPAN>Valeur de retour</TruncatedPAN>
    //      <Operation>Valeur de retour</Operation>
    //      <CardUsedByAnotherCustomer>Valeur de retour</CardUsedByAnotherCustomer>
    //   </Value>
    //   <Exception>
    //      <Count>0</Count>
    //      <Message></Message>
    //      <Code></Code>
    //      <Severity></Severity>
    //      <Type></Type>
    //  </Exception>
    // </Response>
    // 
    // Le client doit parser cet XML et extraire en premier le tag "Exception/Count"
    // 
    // Date : 13/10/2009
    // Auteur : Samatar HASSAN
    // 
    // 
    //
    /// </summary>
    public class InsertCardResponseReader
    {

        // Value Token to return (serialized into string)
        private const string Xml_Response_Token_TagName = "Token";
        // Value CardReference to return (serialized into string)
        private const string Xml_Response_CardReferences_TagName = "CardReferences";
        private const string Xml_Response_Reference_TagName = "Reference";
        private const string Xml_Response_ServiceProvided_TagName = "ServiceProvided";
        private const string Xml_Response_ServiceReturned_TagName = "ServiceReturned";
        private const string Xml_Response_Operation_TagName = "Operation";

        // Value ExpirationDate to return (serialized into string)
        private const string Xml_Response_ExpirationDate_TagName = "ExpirationDate";
        // Value CardType to return (serialized into string)
        private const string Xml_Response_CardType_TagName = "CardType";
        // Value TruncatedPAN to return (serialized into string)
        private const string Xml_Response_TruncatedPAN_TagName = "TruncatedPAN";
        // Value Duration In Milliseconds to return (serialized into string)
        private const string Xml_Response_Duration_TagName = "Duration";

        // Exception 
        // Exception code (0 = no error otherwise 1)
        private const string Xml_Response_Exception_Code_TagName = "Count";
        // Exception message
        private const string Xml_Response_Exception_Message_TagName = "Message";


        private string InputResponse;
        // Valeurs de retour
        private string Token;
        private string[] CardReference;
        private string[] ServiceProvided;
        private string[] ServiceReturned;
        private string[] Operation;
        private string ExpirationDate;
        private string CardType;
        private string TruncatedPAN;

        private double Duration;

        private int ExceptionCount;
        private string ExceptionMessage;

        /// <summary>
        /// Lecture de la réponse de la méthode d'insertion/mise à jour
        /// des cartes dans Navision
        /// Il faut en tout premier lieu vérifier s'il y a des erreurs
        /// en appelant la méthode
        /// IsError()
        /// Si cette méthode retourne TRUE, le message d'erreur est retourné par
        /// GetExceptionMessage()
        ///
        /// </summary>
        /// <param name="inputResponse">Réponse (XML)</param>
        public InsertCardResponseReader(string inputResponse)
        {
            SetInputResponse(inputResponse);
            // On va lire l'entrée
            ParseResponse();
        }

        /// <summary>
        /// Lecture de la réponse
        /// et extraction des différentes informations
        /// </summary>
        private void ParseResponse()
        {
            XmlDocument doc = new XmlDocument();
            //The XmlResolver property is set to null. External resources are not resolved.
            doc.XmlResolver = null;

            try
            {
                // On charge la réponse
                doc.Load(new StringReader(GetInputResponse()));

                // Extraction des erreurs potentiels
                ExtractError(doc);

                // Extraction des différentes valeurs
                ExtractValues(doc);
            }
            catch (Exception e)
            {
                throw new Exception("Erreur lors de la lecture de la réponse d'insertion des cartes! Erreur :" + e.Message);
            }
        }
        /// <summary>
        /// Extraction des erreurs potentiels
        /// </summary>
        /// <param name="doc">Document XML</param>
        private void ExtractError(XmlDocument doc)
        {
            try
            {
                // On récupère en premier l'état d'exception
                SetExceptionCount(Util.ConvertStringToInt(doc.GetElementsByTagName(Xml_Response_Exception_Code_TagName)[0].InnerXml));

                if (IsError())
                {
                    // On récupère le message d'exception
                    SetExceptionMessage(doc.GetElementsByTagName(Xml_Response_Exception_Message_TagName)[0].InnerXml);
                }
            }
            catch (Exception)
            {
                // Visiblement pas de tag d'exception
                // 
            }
        }

        /// <summary>
        /// Extraction des différentes valeurs
        /// </summary>
        /// <param name="doc">Document XML</param>
        private void ExtractValues(XmlDocument doc)
        {
            // Pas d'exception, on récupère la valeur
            if (!IsError())
            {
                // On récupère directement la valeur de la node <Value></Value>
                // Il ne doit y avoir qu'une node de ce type
                SetToken(doc.GetElementsByTagName(Xml_Response_Token_TagName)[0].InnerXml);
                SetTruncatedPAN(doc.GetElementsByTagName(Xml_Response_TruncatedPAN_TagName)[0].InnerXml);
                // On va lire les références cartes
                // On peut en avoir plusieurs
                // dans le cas ou l'utilisateur a renseigné plusieurs services
                // dans ce cas, on aura une référence carte par service
                XmlNode refs = doc.GetElementsByTagName(Xml_Response_CardReferences_TagName)[0];
                // On récupère le nombre de références
                int nr = refs.ChildNodes.Count;
                Allocate(nr);
                for (int i = 0; i < nr; i++)
                {
                    SetCardReference(i, refs.ChildNodes[i].SelectSingleNode(Xml_Response_Reference_TagName).InnerXml);
                    SetServiceProvided(i, refs.ChildNodes[i].SelectSingleNode(Xml_Response_ServiceProvided_TagName).InnerXml);
                    SetServiceReturned(i, refs.ChildNodes[i].SelectSingleNode(Xml_Response_ServiceReturned_TagName).InnerXml);
                    SetOperation(i, refs.ChildNodes[i].SelectSingleNode(Xml_Response_Operation_TagName).InnerXml);
                }

                SetExpirationDate(doc.GetElementsByTagName(Xml_Response_ExpirationDate_TagName)[0].InnerXml);
                SetCardType(doc.GetElementsByTagName(Xml_Response_CardType_TagName)[0].InnerXml);
                SetDuration(Util.ConvertStringToDouble(doc.GetElementsByTagName(Xml_Response_Duration_TagName)[0].InnerXml));
            }
        }

        /// <summary>
        /// Retourne la réponse
        /// </summary>
        /// <returns>Réponse (XML)</returns>
       private string GetInputResponse()
       {
           return this.InputResponse;
       }
        /// <summary>
        /// Affectation réponse XML
        /// </summary>
        /// <param name="inputResponse">Réponse XML</param>
       private void SetInputResponse(string inputResponse)
       {
           this.InputResponse = inputResponse;
       }

       private void Allocate(int nr)
       {
           this.CardReference= new string[nr];
           this.ServiceProvided = new string[nr];
           this.ServiceReturned = new string[nr];
           this.Operation = new string[nr];
       }
        /// <summary>
        /// Retourne le token
        /// </summary>
        /// <returns>Token</returns>
       public long GetToken()
       {
           if(String.IsNullOrEmpty(this.Token)) return Const.EmptyBoToken; else return Util.ConvertStringToToken(this.Token);
       }

        /// <summary>
        /// Affectation du token
        /// </summary>
        /// <param name="value">Token</param>
       private void SetToken(string value)
       {
           this.Token = value;
       }
        /// <summary>
        /// Retourne le nombre de références
        /// Une par service renseigné
        /// </summary>
        /// <returns>Nombre de références carte</returns>
       public int GetCardReferencesCount()
       {
           return this.CardReference.Length;
       }
       public string GetCardReference(int i)
       {
           return this.CardReference[i];
       }
       private void SetCardReference(int i, string value)
       {
           this.CardReference[i] = value;
       }
       public string GetServiceProvided(int i)
       {
           return this.ServiceProvided[i];
       }
       private void SetServiceProvided(int i, string value)
       {
           this.ServiceProvided[i] = value;
       }
       public string GetServiceReturned(int i)
       {
           return this.ServiceReturned[i];
       }
       private void SetServiceReturned(int i, string value)
       {
           this.ServiceReturned[i] = value;
       }
       public string GetOperation(int i)
       {
           return this.Operation[i];
       }
        /// <summary>
        /// Affectation de l'opération
        /// </summary>
        /// <param name="i">Position dans le tableau</param>
        /// <param name="value">Opération</param>
       private void SetOperation(int i, string value)
       {
           this.Operation[i] = value;
       }
        /// <summary>
        /// Retourne la date d'expiration
        /// </summary>
        /// <returns>Date d'expiration</returns>
       public string GetExpirationDate()
       {
           return this.ExpirationDate;
       }
        /// <summary>
        /// Affectation de la date d'expiration
        /// </summary>
        /// <param name="expirationDate">Date d'expiration</param>
       private void SetExpirationDate(string expirationDate)
       {
           this.ExpirationDate=expirationDate;
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
        /// Affectation du type de carte
        /// </summary>
        /// <param name="cardType">Type de carte</param>
       private void SetCardType(string cardType)
       {
           this.CardType = cardType;
       }
        /// <summary>
        /// Retourne le numéro de carte masqué
        /// </summary>
        /// <returns>Numéro de carte masqué</returns>
       public string GetTruncatedPAN()
       {
           return this.TruncatedPAN;
       }
        /// <summary>
        /// Affectation numéro de carte masquée
        /// </summary>
        /// <param name="truncatedPan">Numéro de carte masqué</param>
       private void SetTruncatedPAN(string truncatedPan)
       {
           this.TruncatedPAN = truncatedPan;
       }

        /// <summary>
        /// Retourne le message d'exception
        /// </summary>
        /// <returns>Message d'exception</returns>
       public string GetExceptionMessage()
       {
           return this.ExceptionMessage;
       }
        /// <summary>
        /// Affectation du message d'erreur
        /// </summary>
        /// <param name="message">Message d'erreur</param>
       private void SetExceptionMessage(string message)
       {
           this.ExceptionMessage = message;
       }
       /// <summary>
       /// Indicateur d'erreur
       /// Retourne TRUE si le traitement a échoué
       /// Vous devez lire cet indicateur avant de commencer à lire les
       /// autres informations de la réponse
       /// </summary>
       /// <returns>Indicateur d'erreur</returns>
       public bool IsError()
       {
           return (GetExceptionCount() > 0);
       }

       /// <summary>
       /// Affectation du nombre d'exceptions
       /// </summary>
       /// <param name="count">Nombre d'exceptions</param>
       private void SetExceptionCount(int count)
       {
           this.ExceptionCount = count;
       }
       /// <summary>
       /// Retourne le nombre d'exceptions
       /// </summary>
       /// <returns>Nombre d'exceptions</returns>
       private int GetExceptionCount()
       {
           return this.ExceptionCount;
       }

       /// <summary>
       /// Retourne la durée de traitement
       /// </summary>
       /// <returns>Durée de traitement (ms)</returns>
       public double GetDuration()
       {
           return this.Duration;
       }

        /// <summary>
        /// Affectation de la durée
        /// </summary>
        /// <param name="value">Durée en ms</param>
       private void SetDuration(double value)
       {
           this.Duration = value;
       }
    }
}
