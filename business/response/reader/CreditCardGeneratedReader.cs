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
using System.Xml;
using System.IO;
using SafeNetWS.utils;

namespace SafeNetWS.business.response.reader
{

    /// <summary>
    //
    // Cette classe permet de lire la réponse apportée
    // par la méthode de génération des numéros de carte 
    // L'entrée réponse est structurée de la manière suivante :
    // <?xml version="1.0" encoding="ISO-8859-1"?>
    // <Response>
    //   <Duration>Valeur de retour</Duration>
    //      <Value>
    //          <Cards>
    //              <Card>
    //                  <CardNumber>Valeur de retour</CardNumber>
    //                  <Size>Valeur de retour</Size>
    //                  <Type>Valeur de retour</Type>
    //              </Card>
    //              <Card>
    //                  <CardNumber>Valeur de retour</CardNumber>
    //                  <Size>Valeur de retour</Size>
    //                  <Type>Valeur de retour</Type>
    //              </Card>
    //          </Cards>    
    //      </Value>
    //     <Exception>
    //          <Count>0</Count>
    //          <Code></Code>
    //          <Severity></Severity>
    //          <Type></Type>
    //          <Message></Message>
    //  </Exception>
    // /Response>
    // 
    // Le client doit parser cet XML et extraire en premier le tag "Exception/Count"
    // 
    // Date : 13/10/2009
    // Auteur : Samatar HASSAN
    // 
    // 
    //
    /// </summary>
    public class CreditCardGeneratedReader
    {
        // Value Card to return (serialized into string)
        private const string Xml_Response_Cards_TagName = "Cards";
        private const string Xml_Response_CardNumber_TagName = "CardNumber";
        private const string Xml_Response_CardSize_TagName = "CardSize";
        private const string Xml_Response_CardType_TagName = "CardType";

        // Value Duration In Milliseconds to return (serialized into string)
        private const string Xml_Response_Duration_TagName = "Duration";

        // Exception 
        // Exception code (0 = no error otherwise 1)
        private const string Xml_Response_Exception_Code_TagName = "Count";
        // Exception message
        private const string Xml_Response_Exception_Message_TagName = "Message";


        private string InputResponse;
        // Valeurs de retour
        private string[] CardNumber;
        private string[] CardSize;
        private string[] CardType;

        private double Duration;
        private int ExceptionCount;
        private string ExceptionMessage;

        /// <summary>
        /// Lecture de la réponse de la méthode de génération
        /// de numéros de cartes
        /// Il faut en tout premier lieu vérifier s'il y a des erreurs
        /// en appelant la méthode
        /// IsError()
        /// Si cette méthode retourne TRUE, le message d'erreur est retourné par
        /// GetExceptionMessage()
        ///
        /// </summary>
        /// <param name="inputResponse">Réponse (XML)</param>
        public CreditCardGeneratedReader(string inputResponse)
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
                // On va lire les numéros de cartes
                // On peut en avoir plusieurs
                XmlNode refs = doc.GetElementsByTagName(Xml_Response_Cards_TagName)[0];
                // On récupère le nombre de numéros
                int nr = refs.ChildNodes.Count;
                Allocate(nr);
                for (int i = 0; i < nr; i++)
                {
                    SetCardNumber(i, refs.ChildNodes[i].SelectSingleNode(Xml_Response_CardNumber_TagName).InnerXml);
                    SetCardType(i, refs.ChildNodes[i].SelectSingleNode(Xml_Response_CardType_TagName).InnerXml);
                    SetCardSize(i, refs.ChildNodes[i].SelectSingleNode(Xml_Response_CardSize_TagName).InnerXml);
                }
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

        /// <summary>
        /// Réservation des tableaux
        /// Numéros de carte
        /// Taille des cartes
        /// Type des cartes
        /// </summary>
        /// <param name="nr">Nombre de numéros de carte</param>
       private void Allocate(int nr)
       {
           this.CardNumber= new string[nr];
           this.CardSize = new string[nr];
           this.CardType = new string[nr];
       }


        /// <summary>
        /// Retourne le nombre de références
        /// Une par service renseigné
        /// </summary>
        /// <returns>Nombre de références carte</returns>
       public int GetCardCount()
       {
           return this.CardNumber.Length;
       }

        /// <summary>
        /// Retourne le numéro de carte
        /// à l'index i
        /// </summary>
        /// <param name="i">Index</param>
        /// <returns>Numéro de carte</returns>
       public string GetCardNumber(int i)
       {
           return this.CardNumber[i];
       }
       /// <summary>
       /// Affectation du numéro de carte
       /// à l'index i
       /// </summary>
       /// <param name="i">Index</param>
       /// <param name="value">Numéro de carte</param>
       private void SetCardNumber(int i, string value)
       {
           this.CardNumber[i] = value;
       }

       /// <summary>
       /// Retourne le type de carte
       /// à l'index i
       /// </summary>
       /// <param name="i">Index</param>
       /// <returns>Type de carte</returns>
       public string GetCardType(int i)
       {
           return this.CardType[i];
       }
        /// <summary>
        /// Affectation du type de carte
        /// à l'index i
        /// </summary>
        /// <param name="i">Index</param>
        /// <param name="value">Type de carte</param>
       private void SetCardType(int i, string value)
       {
           this.CardType[i] = value;
       }


       /// <summary>
       /// Retourne la taille de la carte
       /// à l'index i
       /// </summary>
       /// <param name="i">Index</param>
       /// <returns>Taille de la carte</returns>
       public string GetCardSize(int i)
       {
           return this.CardSize[i];
       }
       /// <summary>
       /// Affectation de la taille de carte
       /// à l'index i
       /// </summary>
       /// <param name="i">Index</param>
       /// <param name="value">Taille de carte</param>
       private void SetCardSize(int i, string value)
       {
           this.CardSize[i] = value;
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
