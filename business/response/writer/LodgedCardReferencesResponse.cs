//==================================================================== 
// Credit Card Encryption/Decryption Tool
// 
// Copyright (c) 2009-2015 Egencia.  All rights reserved. 
// This software was developed by Egencia An Expedia Inc. Corporation
// La Defense. Paris. France
// The Original Code is Egencia 
// The Initial Developer is Samatar Hassan.
//===================================================================

using System;
using System.Collections;
using SafeNetWS.utils;
using SafeNetWS.exception;
using SafeNetWS.login;
using SafeNetWS.business.arguments.reader;
using SafeNetWS.database.row;
using SafeNetWS.database.row.value;

namespace SafeNetWS.business.response.writer
{
    /// <summary>
    /// This class will return lodged card reference
    /// response will be xml 
    /// 
    ///<?xml version="1.0" encoding="ISO-8859-1"?>
    ///  <LodgedCardReferenceResponse>
    ///    <Duration></Duration>
    ///    <Value>
    ///     <LodgedCardReference>
    ///        <Key>PK</Key>
    ///        <Label>CC4</Label>
    ///        <Value>TEST</Value>
    ///     </LodgedCardReference>
    ///     <LodgedCardReference>
    ///        <Key>PL</Key>
    ///        <Label>CC3</Label>
    ///        <Value></Value>
    ///     </LodgedCardReference>
    ///    </Value>
    ///    <Exception>
    ///       <Count>0</Count>
    ///       <Message></Message>
    ///       <Code></Code>
    ///       <Severity></Severity>
    ///       <Type></Type>
    ///   </Exception>
    ///  </LodgedCardReferenceResponse>
    ///  
    ///  Caller need first to extract tag "Exception/Count"
    ///  
    ///  
    /// </summary> 
    public class LodgedCardReferencesResponse
    {
        private ArgsLodgedCardReferences RequestorDetail;
        private LodgedCardReferencesData references;



        // Exception handling
        private int ExceptionCount;
        private string ExceptionCode;
        private string ExceptionType;
        private string ExceptionSeverity;
        private string ExceptionMessage;

        private UserInfo User;
        private DateTime StartDate;

        private const string Xml_Response_Open_Tag = "<LodgedCardReferenceResponse>";
        private const string Xml_Response_Close_Tag = "</LodgedCardReferenceResponse>";
        private const string Xml_Response_Value_LodgedCardReference_Open_Tag = "<LodgedCardReference>";
        private const string Xml_Response_Value_LodgedCardReference_Close_Tag = "</LodgedCardReference>";
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        private const string Xml_Response_Value_Label_Open_Tag = "<Label>";
        private const string Xml_Response_Value_Label_Close_Tag = "</Label>";
        private const string Xml_Response_Value_Key_Open_Tag = "<Key>";
        private const string Xml_Response_Value_Key_Close_Tag = "</Key>";
        private const string Xml_Response_Value_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Value_Close_Tag = "</Value>";

     
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


        public LodgedCardReferencesResponse(ArgsLodgedCardReferences requestorDetail)
        {
            // We started initializing new response
            // let's record the process duration
            this.StartDate = DateTime.Now;
            this.RequestorDetail = requestorDetail;
        }


        /// <summary>
        /// set References
        /// </summary>
        /// <param name="value">References</param>
        private void SetReferences(LodgedCardReferencesData value)
        {
            this.references = value;
        }

        /// <summary>
        /// returns references
        /// </summary>
        /// <returns>references</returns>
        private LodgedCardReferencesData GetReferences()
        {
            return this.references;
        }




        /// <summary>
        /// Set user
        /// </summary>
        /// <param name="useri">User</param>
        public void SetUser(UserInfo useri)
        {
            this.User = useri;
        }

        /// <summary>
        /// Returns user
        /// </summary>
        /// <returns>user</returns>
        public UserInfo GetUser()
        {
            return this.User;
        }


        public void SetValues(LodgedCardReferencesData result)
        {
            SetReferences(result);
        }

        /// <summary>
        /// Set exceptions count
        /// </summary>
        /// <param name="useri">user</param>
        /// <param name="count">count</param>
        private void SetExceptionCount(UserInfo useri, int count)
        {
            SetUser(useri);
            this.ExceptionCount = count;
            // Ok, on a construire la réponse
            // mais avant on va extraire les différents informations
            // depuis le message d'exception
            SplitException();
        }

        /// <summary>
        /// Set exception
        /// </summary>
        /// <param name="useri">user</param>
        /// <param name="message">exception message</param>
        public void SetException(UserInfo useri, string message)
        {
            this.ExceptionMessage = message;
            SetExceptionCount(useri, 1);
        }

        /// <summary>
        /// Set exception
        /// </summary>
        /// <param name="useri">user</param>
        /// <param name="exception">exception</param>
        public void SetException(UserInfo useri, Exception exception)
        {
            SetException(useri, exception.Message);
        }

        /// <summary>
        /// Returns duration
        /// </summary>
        /// <returns>Duration</returns>
        private string GetDuration()
        {
            return Util.GetDuration(this.StartDate).ToString();
        }

        /// <summary>
        /// Split exception
        /// </summary>
        private void SplitException()
        {
            if (GetExceptionMessage().StartsWith(CCEExceptionUtil.EXCEPTION_TAG_OPEN))
            {
                // the exception message contains details
                // we will extract the code, severity, type and message
                this.ExceptionCode = CCEExceptionUtil.GetExceptionCode(GetExceptionMessage());
                this.ExceptionSeverity = CCEExceptionUtil.GetExceptionSeverity(GetExceptionMessage());
                this.ExceptionType = CCEExceptionUtil.GetExceptionType(GetExceptionMessage());
                this.ExceptionMessage = CCEExceptionUtil.GetExceptionOnlyMessage(GetExceptionMessage());
            }
            else
            {
                // the exception do not contain details
                // on will put default values
                this.ExceptionCode = CCEExceptionMap.EXCEPTION_CODE_DEFAULT;
                this.ExceptionSeverity = CCEExceptionMap.EXCEPTION_SEVERITY_DEFAULT;
                this.ExceptionType = CCEExceptionMap.EXCEPTION_TYPE_SYSTEM;
            }
        }
        /// <summary>
        /// returns exception count
        /// </summary>
        /// <returns>count</returns>
        private int GetExceptionCount()
        {
            return this.ExceptionCount;
        }

        /// <summary>
        /// returns exception code
        /// </summary>
        /// <returns>exception code</returns>
        private string GetExceptionCode()
        {
            return this.ExceptionCode;
        }

        /// <summary>
        /// returns exception severity
        /// </summary>
        /// <returns>exception severity</returns>
        private string GetExceptionSeverity()
        {
            return this.ExceptionSeverity;
        }

        /// <summary>
        /// returns exception type
        /// </summary>
        /// <returns>exception type</returns>
        private string GetExceptionType()
        {
            return this.ExceptionType;
        }


        /// <summary>
        /// returns exception message
        /// </summary>
        /// <returns>exception message</returns>
        private string GetExceptionMessage()
        {
            return this.ExceptionMessage;
        }

        /// <summary>
        /// returns true is the request failed
        /// </summary>
        /// <returns>request status</returns>
        private bool IsError()
        {
            return (GetExceptionCount() > 0);
        }

        /// <summary>
        /// Prepare response for log operation
        /// </summary>
        private void LogResponse()
        {
            Services.WriteOperationStatusToLog(GetUser(),
                String.Format(" and provided request detail"),
                String.Format(".The following values were returned to user : {0}", GetValueMessage()),
                String.Format(".Unfortunately, the process failed for the following reason: {0}", GetExceptionMessage()),
                IsError(),
                GetDuration());
        }

        /// <summary>
        /// Returns value in case of success
        /// what the caller expect
        /// of course no sensitive data (credit card, csc, ...)
        /// MUST appear here
        /// </summary>
        /// <returns>value returned</returns>
        private string GetValueMessage()
        {
            return String.Format("Found {0} references.", GetReferences()==null?0:GetReferences().GetSize());
        }




        /// <summary>
        /// Build the response to return to caller
        /// </summary>
        /// <returns>Response (XML)</returns>
        public string GetResponse()
        {
            // Let's first log that we have processed request for the caler
            LogResponse();
            // we can know build the response
            string strData =
            Const.XmlHeader
            + Xml_Response_Open_Tag
                + Xml_Response_Duration_Open_Tag
                    + GetDuration()
                + Xml_Response_Duration_Close_Tag;
              
            if (!IsError())
            {
                // The request was successfully processed
                strData +=
                     Xml_Response_Value_Open_Tag;
                    // Get all references
                    IDictionaryEnumerator it = GetReferences().GetReferences();
                    while (it.MoveNext())
                    {
                        // Fetch and add each references to response
                        LodgedCardReferenceValue reference= (LodgedCardReferenceValue) it.Value;
                        strData +=
                        Xml_Response_Value_LodgedCardReference_Open_Tag
                          + Xml_Response_Value_Key_Open_Tag
                                + reference.GetKey()
                           + Xml_Response_Value_Key_Close_Tag
                           + Xml_Response_Value_Label_Open_Tag
                                + reference.GetLabel()
                            + Xml_Response_Value_Label_Close_Tag;
                            if (!String.IsNullOrEmpty(reference.GetValue()))
                            {
                                strData +=
                                Xml_Response_Value_Value_Open_Tag
                                    + reference.GetValue()
                                + Xml_Response_Value_Value_Close_Tag;
                            }
                         strData += Xml_Response_Value_LodgedCardReference_Close_Tag;
                    }
                     strData += Xml_Response_Value_Close_Tag;
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
            strData += Xml_Response_Close_Tag;
            return strData;
        }
    }
}
