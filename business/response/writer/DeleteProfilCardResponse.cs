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
using SafeNetWS.database.result;
using SafeNetWS.login;
using SafeNetWS.exception;
using SafeNetWS.creditcard.creditcardvalidator;

namespace SafeNetWS.business.response.writer
{
   /// <summary>
   /// Cette classe permet de construire la réponse apportée
   /// par la méthode de suppression des cartes dans la base des données Navision
   /// La réponse est structurée de la manière suivante :
   /// 
   ///<?xml version="1.0" encoding="ISO-8859-1"?>
    ///  <DeleteProfilCardResponse>
   ///    <Duration>Valeur de retour</Duration>
   ///    <Value>OK</Value>
   ///    <Exception>
   ///       <Count>0</Count>
   ///       <Message></Message>
   ///       <Code></Code>
   ///       <Severity></Severity>
   ///       <Type></Type>
   ///   </Exception>
    ///  </DeleteProfilCardResponse>
   ///  
   ///  Le client doit parser cet XML et extraire en premier le tag "Exception/Count"
   ///  
   ///  Date : 01/03/2012
   ///  Auteur : Samatar HASSAN
   ///  
   /// </summary> 
 
    public class DeleteProfilCardResponse
    {

        private const string Xml_Response_Open_Tag = "<DeleteProfilCardResponse>";
        private const string Xml_Response_Close_Tag = "</DeleteProfilCardResponse>";
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
      
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


        private int ExceptionCount;
        private string ExceptionCode;
        private string ExceptionType;
        private string ExceptionSeverity;
        private string ExceptionMessage;

        private UserInfo User;
        private DateTime StartDate;
        private string pos;
        private string customer;
        private string cc1;
        private string traveler;
        private string service;

        // Statut de le suppression
        // en réalité il n'y a qu'un seul statut "OK"
        // La suppression a été faite avec succès (OK)
        // autrement une exception sera lévée

        public DeleteProfilCardResponse(string pos, string customer, string cc1, string traveler, string service)
        {
            // Initialisation
            this.StartDate = DateTime.Now;
            this.pos = Util.CorrectPos(GetUser(),pos);
            this.customer = customer;
            this.cc1 = cc1;
            this.traveler = traveler;
            this.service = service;
        }
        public void SetUser(UserInfo useri)
        {
            this.User = useri;
        }
        public UserInfo GetUser()
        {
            return this.User;
        }

        public string GetPOS()
        {
            return this.pos;
        }
        private string GetCustomer()
        {
            return this.customer;
        }
        private string GetTraveler()
        {
            return this.traveler;
        }
        private string GetService()
        {
            return this.service;
        }
        private string GetCC1()
        {
            return this.cc1;
        }
        private DateTime GetStartDate()
        {
            return this.StartDate;
        }
        private void SetExceptionCount(UserInfo useri, int count)
        {
            SetUser(useri);
            this.ExceptionCount = count;
            // Ok, on a construire la réponse
            // mais avant on va extraire les différents informations
            // depuis le message d'exception
            SplitException();
        }
        public void SetException(UserInfo useri, string message)
        {
            this.ExceptionMessage = message;
            SetExceptionCount(useri, 1);
        }

        public void SetException(UserInfo useri, Exception exception)
        {
            SetException(useri, exception.Message);
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
            // On trace la demande
            LogResponse();
            // Ok, on a construire la réponse
            string strData = 
            Const.XmlHeader
            + Xml_Response_Open_Tag
                +Xml_Response_Duration_Open_Tag
                    + GetDuration()
                + Xml_Response_Duration_Close_Tag;
            if (!IsError())
            {
                // Il n'y a aucune erreur
                // On va retourner la valeur
                // sans les tags d'exception
                strData +=
                Xml_Response_Value_Open_Tag
                    + Const.Success
                + Xml_Response_Value_Close_Tag;
            }
            else
            {
                // On a rencontré une erreur
                // On va retourner les tags d'exception
                // sans les tags de valeur
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
            strData+= Xml_Response_Close_Tag;
            return strData;
       }
       private int GetExceptionCount()
       {
           return this.ExceptionCount;
       }
       private string GetExceptionCode()
       {
           return this.ExceptionCode;
       }
       private string GetExceptionSeverity()
       {
           return this.ExceptionSeverity;
       }
       private string GetExceptionType()
       {
           return this.ExceptionType;
       }
       private string GetExceptionMessage()
       {
           return this.ExceptionMessage;
       }
       private bool IsError()
       {
           return (GetExceptionCount() > 0);
       }

       private string GetValueMessage()
       {
           return String.Format("Payment card for POS ={0}, Customer ={1}, CC={2}, Traveler ={3}, Service ={4} was deleted.",GetPOS()
               , GetCustomer(), Util.Nvl(GetCC1(), string.Empty), Util.Nvl(GetTraveler(), string.Empty), GetService()) ;
       }

       /// <summary>
       /// On va répondre au client
       /// mais avant, nous devons tracer cette demande
       /// en informant Syslog
       /// </summary>
       private void LogResponse()
       {
           Services.WriteOperationStatusToLog(GetUser(),
               String.Format(" and provided POS ={0}, Customer ={1}, CC1 ={2}, Traveler ={3}, Service ={4}", GetPOS(), GetCustomer(), Util.Nvl(GetCC1(), string.Empty)
               , Util.Nvl(GetTraveler(), string.Empty), GetService()),
               String.Format(".The following values were returned to user : {0}", GetValueMessage()),
               String.Format(".Unfortunately, the process failed for the following reason: {0}", GetExceptionMessage()),
               IsError(),
               GetDuration());
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
    }
}