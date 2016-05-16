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


namespace SafeNetWS.business.response.writer
{
   /// <summary>
   /// Cette classe permet de construire la réponse apportée
   /// par la méthode qui retourne le type de paiement pour une companie aérienne
   /// La réponse est structurée de la manière suivante :
   /// 
   ///<?xml version="1.0" encoding="ISO-8859-1"?>
   ///  <Response>
   ///    <Duration>Valeur de retour</Duration>
   ///    <Value>PaymentType</Value>
   ///    <Exception>
   ///       <Count>0</Count>
   ///       <Message></Message>
   ///       <Code></Code>
   ///       <Severity></Severity>
   ///       <Type></Type>
   ///   </Exception>
   ///  </Response>
   ///  
   ///  Le client doit parser cet XML et extraire en premier le tag "Exception/Count"
   ///  
   ///  Date : 01/03/2012
   ///  Auteur : Samatar HASSAN
   ///  
   /// </summary> 
 
    public class GDSCustomerPaymentTypeResponse
    {
        
        private const string Xml_Response_Open_Tag="<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";
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
        private string CorporationCode;
        private string POS;

        private string PaymentType;

        public GDSCustomerPaymentTypeResponse(string pos, string corporation)
        {
            // Initialisation
            this.StartDate = DateTime.Now;
            this.CorporationCode =corporation;
            this.POS =  Util.CorrectPos(GetUser(),pos);
        }
        public void SetUser(UserInfo useri)
        {
            this.User = useri;
        }
        public UserInfo GetUser()
        {
            return this.User;
        }

        private string GetCorporationCode()
        {
            return this.CorporationCode;
        }

        public void SetPOS(string pos)
        {
            this.POS=pos;
        }
       

        public string GetPOS()
        {
            return this.POS;
        }
       
       
        private DateTime GetStartDate()
        {
            return this.StartDate;
        }

        /// <summary>
        /// Retourne le type de paiement
        /// </summary>
        /// <returns>Type de paiement</returns>
        private string GetPaymentType()
        {
            return this.PaymentType;
        }

        public void SetValue(string value)
        {
            this.PaymentType = value;
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
                // On va retourner le cryptogramme du contenu du fichier
                // sans les tags d'exception
                strData +=
                Xml_Response_Value_Open_Tag
                    + GetPaymentType()
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
           return String.Format("Payment type ={0}.", GetPaymentType());
       }

       /// <summary>
       /// On va répondre au client
       /// mais avant, nous devons tracer cette demande
       /// en informant Syslog
       /// </summary>
       private void LogResponse()
       {
           Services.WriteOperationStatusToLog(GetUser(), 
               String.Format(" and provided corporation code ={0} in {1}", GetCorporationCode(), GetPOS()),
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