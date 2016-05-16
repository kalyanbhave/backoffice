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
using SafeNetWS.database.result;
using SafeNetWS.login;
using SafeNetWS.exception;

namespace SafeNetWS.business.response.writer
{
    /**
     * Cette classe permet de construire la réponse apportée
     * par la méthode de génération de numéros de cartes
     * La réponse est structurée de la manière suivante :
     * 
     * <?xml version="1.0" encoding="ISO-8859-1"?>
     * <Response>
     *    <Duration>Valeur de retour</Duration>
     *   <Value>    
     *     <Cards>
     *       <Card>
     *          <CardNumber>Valeur de retour</CardNumber>    
     *          <Size>Valeur de retour</Size>
     *          <Type>Valeur de retour</Type>   
     *      </Card>
     *      <Card>
     *          <CardNumber>Valeur de retour</CardNumber>    
     *          <Size>Valeur de retour</Size>
     *          <Type>Valeur de retour</Type>   
     *      </Card>
     *    </Cards>
     *   </Value>  
     *   <Exception>
     *      <Count>0</Count>
     *      <Code></Code>
     *      <Severity></Severity>
     *      <Type></Type>
     *      <Message></Message>
     *  </Exception>
     * </Response>
     * 
     * Le client doit parser cet XML et extraire en premier le tag "Exception/Count"
     * 
     * Date : 13/10/2009
     * Auteur : Samatar HASSAN
     * 
     * 
     */
    public class CreditCardGeneratedResponse
    {
        private const string Xml_Response_Open_Tag="<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";

        // Value to return (serialized into string)
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        // Card to return (serialized into string)
        private const string Xml_Cards_Open_Tag = "<Cards>";
        private const string Xml_Cards_Close_Tag = "</Cards>";
        // Card to return (serialized into string)
        private const string Xml_Card_Open_Tag = "<Card>";
        private const string Xml_Card_Close_Tag = "</Card>";
        // CardNumber to return (serialized into string)
        private const string Xml_CardNumber_Open_Tag = "<CardNumber>";
        private const string Xml_CardNumber_Close_Tag = "</CardNumber>";
        // Size to return (serialized into string)
        private const string Xml_CardSize_Open_Tag = "<CardSize>";
        private const string Xml_CardSize_Close_Tag = "</CardSize>";
        // Type to return (serialized into string)
        private const string Xml_CardType_Open_Tag = "<CardType>";
        private const string Xml_CardType_Close_Tag = "</CardType>";
       
        // Exception 
        private const string Xml_Response_Exception_Open_Tag = "<Exception>";
        private const string Xml_Response_Exception_Close_Tag = "</Exception>";
        // Exception code (0 = no error otherwise 1)
        private const string Xml_Response_Exception_Count_Open_Tag = "<Count>";
        private const string Xml_Response_Exception_Count_Close_Tag = "</Count>";
        // Exception code
        private const string Xml_Response_Exception_Code_Open_Tag = "<Code>";
        private const string Xml_Response_Exception_Code_Close_Tag = "</Code>";
        // Exception severity
        private const string Xml_Response_Exception_Severity_Open_Tag = "<Severity>";
        private const string Xml_Response_Exception_Severity_Close_Tag = "</Severity>";
        // Exception type
        private const string Xml_Response_Exception_Type_Open_Tag = "<Type>";
        private const string Xml_Response_Exception_Type_Close_Tag = "</Type>";

        // Exception message
        private const string Xml_Response_Exception_Message_Open_Tag = "<Message>";
        private const string Xml_Response_Exception_Message_Close_Tag = "</Message>";
        
        // Value Duration In Milliseconds to return (serialized into string)
        private const string Xml_Response_Duration_Open_Tag = "<Duration>";
        private const string Xml_Response_Duration_Close_Tag = "</Duration>";


        private string[] CardNumber;
        private int CardSize;
        private int HowMany;
        private string CardType;

        private int ExceptionCount;
        private string ExceptionCode;
        private string ExceptionType;
        private string ExceptionSeverity;
        private string ExceptionMessage;

        private DateTime StartDate;

        /// <summary>
        /// Instanciation
        /// </summary>
        public CreditCardGeneratedResponse()
        {
            // Initialisation
            this.StartDate = DateTime.Now;
        }

        /// <summary>
        /// Affectation des valeurs
        /// </summary>
        /// <param name="type">Type des cartes</param>
        /// <param name="size">Taille des cartes</param>
        /// <param name="howMany">Nombre de numéros de carte</param>
        /// <param name="pan">Tableaux de numéros de carte</param>
        public void SetValues(string type, int size, int howMany, string[] pan)
        {
            this.CardType = type;
            this.CardNumber = pan;
            this.CardSize = size;
            this.HowMany = howMany;
        }

        /// <summary>
        /// Affectation du nombre d'erreurs
        /// </summary>
        /// <param name="Counti">Nombre d'erreurs</param>
        private void SetExceptionCount(int Counti)
        {
            this.ExceptionCount = Counti;
            // Ok, on a construire la réponse
            // mais avant on va extraire les différents informations
            // depuis le message d'exception
            SplitException();
        }

        /// <summary>
        /// Affectation de l'exception
        /// </summary>
        /// <param name="message">Message</param>
        public void SetException(string message)
        {
            this.ExceptionMessage = message;
            SetExceptionCount(1);
        }

        /// <summary>
        /// Affectation d'une exception
        /// </summary>
        /// <param name="exception">Exception</param>
        public void SetException(Exception exception)
        {
            SetException(exception.Message);
        }
        /// <summary>
        /// Retourne TRUE si le traitement
        /// de cette demande a échoué
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        private bool IsError()
        {
            return (this.ExceptionCount > 0);
        }

        /// <summary>
        /// Retourne le message de l'exception
        /// </summary>
        /// <returns>Message exception</returns>
        private string GetExceptionMessage()
        {
            return this.ExceptionMessage;
        }

        /// <summary>
        /// Retourne le nombre de caractères (taille)
        /// de chacune des numéros de carte
        /// </summary>
        /// <returns>Taille</returns>
        private int GetCardSize()
        {
            return this.CardSize;
        }

        /// <summary>
        /// Retourne le nombre de numéros cartes demandés
        /// </summary>
        /// <returns>Nombre de numéros</returns>
        private int GetHowMany()
        {
            return this.HowMany;
        }

        /// <summary>
        /// Retourne la durée du traitement
        /// en ms
        /// </summary>
        /// <returns>Durée (ms)</returns>
        private string GetDuration()
        {
            return Util.GetDuration(this.StartDate).ToString();
        }

        /// <summary>
        /// Retour de la réponse structurée en XML
        /// </summary>
        /// <returns>Réponse (XML)</returns>
        public string GetResponse()
        {
            // Ok, maintenant on va construire la réponse
            string strData = Const.XmlHeader
                + Xml_Response_Open_Tag
                    +Xml_Response_Duration_Open_Tag
                        + GetDuration()
                    + Xml_Response_Duration_Close_Tag;
                if (!IsError())
                {
                    // Pas d'exception
                    // Tout va bien
                    strData +=
                        Xml_Response_Value_Open_Tag
                            + Xml_Cards_Open_Tag;
                    for (int i = 0; i < GetHowMany(); i++)
                    {
                        strData +=
                            Xml_Card_Open_Tag
                                + Xml_CardNumber_Open_Tag
                                    + GetCardNumber(i)
                                + Xml_CardNumber_Close_Tag
                                + Xml_CardSize_Open_Tag
                                    + GetCardSize().ToString()
                                + Xml_CardSize_Close_Tag
                                + Xml_CardType_Open_Tag
                                    + GetCardType()
                                + Xml_CardType_Close_Tag
                           + Xml_Card_Close_Tag;
                    }
                    strData +=
                        Xml_Cards_Close_Tag
                     + Xml_Response_Value_Close_Tag;   
                }
                else
                {
                    // On a une exception
                    strData +=
                     Xml_Response_Exception_Open_Tag
                        + Xml_Response_Exception_Count_Open_Tag
                            + GetExceptionCount()
                        + Xml_Response_Exception_Count_Close_Tag
                        + Xml_Response_Exception_Code_Open_Tag
                            + GetExceptionCode()
                        + Xml_Response_Exception_Code_Close_Tag
                        + Xml_Response_Exception_Severity_Open_Tag
                            + GetExceptionSeverity()
                        + Xml_Response_Exception_Severity_Close_Tag
                        + Xml_Response_Exception_Type_Open_Tag
                             + GetExceptionType()
                        + Xml_Response_Exception_Type_Close_Tag
                        + Xml_Response_Exception_Message_Open_Tag
                            + GetExceptionMessage()
                        + Xml_Response_Exception_Message_Close_Tag
                    + Xml_Response_Exception_Close_Tag;
                }
                    strData +=
                 Xml_Response_Close_Tag;
            return strData;
        }

       /// <summary>
       /// Décomposition de l'exception si cette dernière est enrichie
       /// On va extraire le code de l'exception
       /// le degré de sévérité de l'exception
       /// le type d'exception
       /// </summary>
       private void SplitException()
       {
           if (GetExceptionMessage().StartsWith(CCEExceptionUtil.EXCEPTION_TAG_OPEN))
           {
               // Ce message est enrichi
               // par le code, le type et la sévérité du message
               this.ExceptionCode = CCEExceptionUtil.GetExceptionCode(GetExceptionMessage());
               this.ExceptionSeverity = CCEExceptionUtil.GetExceptionSeverity(GetExceptionMessage());
               this.ExceptionType = CCEExceptionUtil.GetExceptionType(GetExceptionMessage());
               this.ExceptionMessage = CCEExceptionUtil.GetExceptionOnlyMessage(GetExceptionMessage());
           }
           else
           {
               // Cette exception n'est pas enrichie
               // On va mettre les valeurs par défaut
               this.ExceptionCode = CCEExceptionMap.EXCEPTION_CODE_DEFAULT;
               this.ExceptionSeverity = CCEExceptionMap.EXCEPTION_SEVERITY_DEFAULT;
               this.ExceptionType = CCEExceptionMap.EXCEPTION_TYPE_SYSTEM;
           }
       }
         /// <summary>
        /// Retourne le type d'exception
        /// </summary>
        /// <returns>Type d'exception</returns>
        private string GetExceptionType()
        {
            return this.ExceptionType;
        }

        /// <summary>
        /// Retourne le code d'exception
        /// </summary>
        /// <returns>Code d'exception</returns>
        private string GetExceptionCode()
        {
            return this.ExceptionCode;
        }

        /// <summary>
        /// Retourne la gravité de l'exception
        /// </summary>
        /// <returns>Gravité exception</returns>
        private string GetExceptionSeverity()
        {
            return this.ExceptionSeverity;
        }

        /// <summary>
        /// Retourne le nombre d'erreur
        /// </summary>
        /// <returns>Nombre d'erreurs</returns>
        private int GetExceptionCount()
        {
            return this.ExceptionCount;
        }

        /// <summary>
        /// Retourne le type des cartes
        /// </summary>
        /// <returns>Type</returns>
        private string GetCardType()
        {
            return this.CardType;
        }
        /// <summary>
        /// Retourne la carte 
        ///  l'index i du tableau
        /// </summary>
        /// <param name="i">Index de la carte</param>
        /// <returns>Numéro de carte</returns>
        private string GetCardNumber(int i)
        {
            return this.CardNumber[i];
        }

    }
}
