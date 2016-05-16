using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using SafeNetWS.utils;
using SafeNetWS.database.result;
using SafeNetWS.login;
using SafeNetWS.exception;
using SafeNetWS.creditcard.creditcardvalidator;
using SafeNetWS.NavService;
using SafeNetWS.business.arguments.quality;
using SafeNetWS.creditcard;

namespace SafeNetWS.business.response.writer
{
    /// <summary>
    /// This class allows to build the response provided
    /// By the hierarchical information retrieval method
    /// Relating to the InsertPaymentCard
    /// The answer is structured as follows:
    /// 
    /// <?xml version="1.0" encoding="ISO-8859-1"?>
    /// <Response>
    ///      <Duration>284</Duration>
    ///      <Value>
    ///           
    /////            <InsertPaymentCard>
    /////            <CardReference>B</CardReference>
    //               <MerchandFlow>BTA</MerchandFlow>
    //               <EnhancedFlow>BTAINVOICE</EnhancedFlow>
    //               <OnlineCheck>NO_VALIDATION</OnlineCheck>
    //               <Operation>UPDATE</Operation>
    ///              <InsertPaymentCard>               
    ///                
    ///      </Value>
    ///      <Exception>
    ///      </Exception>
    /// </Response>
    /// 
    /// 
    /// </summary>
    public class InsertPaymentCardResponse
    {
        private const string Xml_Response_Open_Tag = "<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";

        // Value to return (serialized into string)
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        // --> InsertPaymentCard
        private const string Xml_InsertPaymentCard_Open_Tag = "<InsertPaymentCard>";
        private const string Xml_InsertPaymentCard_Close_Tag = "</InsertPaymentCard>";

        // --> CardReference
        private const string Xml_CardReference_Open_Tag = "<CardReference>";
        private const string Xml_CardReference_Close_Tag = "</CardReference>";

        // --> MerchandFlow
        private const string Xml_MerchandFlow_Tag = "<MerchandFlow>";
        private const string Xml_MerchandFlow_Close_Tag = "</MerchandFlow>";

        // --> EnhancedFlow
        private const string Xml_EnhancedFlow_Open_Tag = "<EnhancedFlow>";
        private const string Xml_EnhancedFlow_Close_Tag = "</EnhancedFlow>";

        // --> OnlineCheck
        private const string Xml_OnlineCheck_Open_Tag = "<OnlineCheck>";
        private const string Xml_OnlineCheck_Close_Tag = "</OnlineCheck>";

        // --> Operation
        private const string Xml_Operation_Open_Tag = "<Operation>";
        private const string Xml_Operation_Close_Tag = "</Operation>";


        // Handle exceptions

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

        private UserInfo user;
        private DateTime StartDate;


        // Arguments
        private string argPos;
        private string argComcode;
        private string argCostCenter;
        private string argPercode;
        private string argServiceslist;

        // Result from Navision WS


        private Nav_InsertPaymentCard pm;
      


        public InsertPaymentCardResponse(UserInfo user, string pos, string customer, string cc1, string percode, string token
            , string servicegroup, CardInfos cardInfos, int contextSource, string context, int forceWarning)
        {
           
            //InsertCardResponse card = new InsertCardResponse();
          
            

            NavService.Nav_InsertPaymentCard o = new Nav_InsertPaymentCard();
            //RequestedService ss = new RequestedService();



            // initialisation
            this.StartDate = DateTime.Now;
            //>> EGE-66896
            this.argPos = pos;
            this.argCostCenter = cc1;
            this.argComcode = customer;
            this.argPercode = percode;
            this.argServiceslist = servicegroup;
            this.argPos = String.IsNullOrEmpty(pos) ? pos : ((String.Compare(pos, "null", true) == 0) ? String.Empty : pos);
            //this.argCostCenter = String.IsNullOrEmpty(cc1) ? cc1 : ((String.Compare(cc1, "null", true) == 0) ? String.Empty : cc1);
            //this.argComcode = String.IsNullOrEmpty(comcode) ? comcode : ((String.Compare(comcode, "null", true) == 0) ? String.Empty : comcode);
            //this.argPercode = String.IsNullOrEmpty(percode) ? percode : ((String.Compare(percode, "null", true) == 0) ? String.Empty : percode);
            //this.argServiceslist = String.IsNullOrEmpty(argServiceslist) ? argServiceslist : ((String.Compare(argServiceslist, "null", true) == 0) ? String.Empty : argServiceslist);
            //>> EGE-66896
        }

        /// <summary>
        /// Set result from Navision ws
        /// </summary>
        /// <param name="value">NavPaymentMeans</param>
        public void SetValue(Nav_InsertPaymentCard value)
        {
            this.pm = value;
            // Let's check if we have an exception code
            string exceptionCode = value.NavException == null ? null : value.NavException[0].NavExceptionCode[0];

            if (!String.IsNullOrEmpty(exceptionCode))
            {

                // We have an exception here
                this.ExceptionCount = 1;
                // set exception code and message
                this.ExceptionCode = exceptionCode;
                this.ExceptionMessage = value.NavException[0].NavExceptionDesc[0];
                this.ExceptionSeverity = CCEExceptionMap.EXCEPTION_SEVERITY_ERROR;
                this.ExceptionType = CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL;
            }
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
        /// Il y a t-il des exceptions dans le traitement
        /// Si on a au  moins une exception
        /// alors la réponse va être flaggée en erreur
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        private bool IsError()
        {
            return (this.ExceptionCount > 0);
        }

        /// <summary>
        /// Retourne le message d'exception
        /// Cette méthode est appelée lorsque la fonction
        /// IsError return e TRUE
        /// </summary>
        /// <returns>Message d'exception</returns>
        private string GetExceptionMessage()
        {
            return this.ExceptionMessage;
        }
        private string GetValueMessage()
        {
            return String.Format("Payment means for all services");
        }
        public void SetUser(UserInfo useri)
        {
            this.user = useri;
        }


        /// <summary>
        /// On va répondre au client
        /// mais avant, nous devons tracer cette demande
        /// en informant Syslog
        /// </summary>
        private void LogResponse()
        {
            Services.WriteOperationStatusToLog(GetUser(),
             String.Format(" and provided {0}.", GetInputValue()),
             String.Format("The following values were returned to user : {0}", GetValueMessage()),
             String.Format("Unfortunately, the process failed for the following reason: {0}", GetExceptionMessage()),
             IsError(),
             GetDuration());
        }

        /// <summary>
        /// Retourne les informatiosn fournies
        /// par le client lors de l'appel
        /// </summary>
        /// <returns>Informations fournies</returns>
        private string GetInputValue()
        {
            return String.Format("pos = {0}, comcode = {1}, percode = {2}, services = {3}", GetArgPos(), GetArgComcode(), GetArgPercode(), GetArgServicesList());
        }
        /// <summary>
        /// Retourne la durée de traitement
        /// </summary>
        /// <returns>Durée de traitement (ms)</returns>
        private string GetDuration()
        {
            return Util.GetDuration(this.StartDate).ToString();
        }
        /// <summary>
        /// Retourne le compte utilisateur
        /// </summary>
        /// <returns>Compte utilisateur</returns>
        public UserInfo GetUser()
        {
            return this.user;
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
        /// Retourne le nombre d'erreur
        /// </summary>
        /// <returns>Nombre d'erreurs</returns>
        private int GetExceptionCount()
        {
            return this.ExceptionCount;
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
        /// Retourne le code client
        /// </summary>
        /// <returns>Code client</returns>
        public string GetArgComcode()
        {
            return this.argComcode;
        }

        /// <summary>
        /// Retourne le code voyageur
        /// </summary>
        /// <returns>Code voyageur</returns>
        public string GetArgPercode()
        {
            return this.argPercode;
        }

        /// <summary>
        /// Retourne le centre de cout
        /// </summary>
        /// <returns>cc1</returns>
        public string GetArgCostCenter()
        {
            return this.argCostCenter;
        }


        /// <summary>
        /// Retourne le marché
        /// </summary>
        /// <returns>Marché</returns>
        public string GetArgPos()
        {
            return this.argPos;
        }

        /// <summary>
        /// Retourne le marché
        /// </summary>
        /// <returns>Marché</returns>
        public string GetArgServicesList()
        {
            return this.argServiceslist;
        }

    }
}