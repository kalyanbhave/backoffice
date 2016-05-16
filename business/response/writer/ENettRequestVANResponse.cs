//==================================================================== 
// Credit Card Encryption/Decryption Tool
// 
// Copyright (c) 2009-2015 Egencia.  All rights reserved. 
// This software was developed by Egencia An Expedia Inc. Corporation
// La Defense. Paris. France
// The Original Code is Egencia 
// The Initial Developer is Sunil Kumar Pidugu (from Sonata Hyderabad).
// Code was reviewed by Samatar Hassan
//===================================================================

using System;
using SafeNetWS.utils;
using SafeNetWS.exception;
using SafeNetWS.login;
using SafeNetWS.ENettService;
using SafeNetWS.business.arguments.reader;
using SafeNetWS.creditcard.virtualcard.enett;

namespace SafeNetWS.business.response.writer
{
    /// <summary>
    /// This class will return a VAN (VNett Account Number)
    /// response will be xml 
    /// 
    ///<?xml version="1.0" encoding="ISO-8859-1"?>
    ///  <RequestVANResponse>
    ///    <Duration></Duration>
    ///    <Value>
    ///        <VNettTransactionID>123</VNettTransactionID>
    ///        <VAN>5123450270722869</VAN>
    ///        <IsMultiUse>true</IsMultiUse>
    ///        <PaymentID>45B49C9BB9CE077</PaymentID>
    ///        <CSC>269</CSC>
    ///        <Currency>EUR</Currency>
    ///        <ActivationDate>20130531</ActivationDate>
    ///        <ExpiryDate>20130531</ExpiryDate>
    ///        <CardHolderName>EGENCIA</CardHolderName>
    ///        <FundedAmount>5000</FundedAmount>
    ///    </Value>
    ///    <Exception>
    ///       <SupportLogId>455</SupportLogId>
    ///       <Count>0</Count>
    ///       <Message></Message>
    ///       <Code></Code>
    ///       <Severity></Severity>
    ///       <Type></Type>
    ///   </Exception>
    ///  </RequestVANResponse>
    ///  
    ///  Caller need first to extract tag "Exception/Count"
    ///  
    ///  
    /// </summary> 
    public class ENettRequestVANResponse
    {
        private string RequestorDetail;
        // VNett details

        private bool MultiUse;
        private string PaymentID;
        private string SupportLogId;
        private int VNettTransactionID;
        // credit card detail
        private string VAN;
        private DateTime ActivationDate;
        private DateTime ExpirationDate;
        private string CardSecurityCode;
        private string CardHolderName;
        private string Currency;
        private long FundedAmount;



        // Exception handling
        private int ExceptionCount;
        private string ExceptionCode;
        private string ExceptionType;
        private string ExceptionSeverity;
        private string ExceptionMessage;

        private UserInfo User;
        private DateTime StartDate;

        private const string Xml_Response_Open_Tag = "<RequestVANResponse>";
        private const string Xml_Response_Close_Tag = "</RequestVANResponse>";
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        private const string Xml_Response_SupportLogId_Open_Tag = "<SupportLogId>";
        private const string Xml_Response_SupportLogId_Close_Tag = "</SupportLogId>";

        private const string Xml_Response_VNettTransactionID_Open_Tag = "<VNettTransactionID>";
        private const string Xml_Response_VNettTransactionID_Close_Tag = "</VNettTransactionID>";
        private const string Xml_Response_VAN_Open_Tag = "<VAN>";
        private const string Xml_Response_VAN_Close_Tag = "</VAN>";
        private const string Xml_Response_PaymentID_Open_Tag = "<PaymentID>";
        private const string Xml_Response_PaymentID_Close_Tag = "</PaymentID>";
        private const string Xml_Response_IsMultiUse_Open_Tag = "<IsMultiUse>";
        private const string Xml_Response_IsMultiUse_Close_Tag = "</IsMultiUse>";
        private const string Xml_Response_Currency_Open_Tag = "<Currency>";
        private const string Xml_Response_Currency_Close_Tag = "</Currency>";
        private const string Xml_Response_ActivationDate_Open_Tag = "<ActivationDate>";
        private const string Xml_Response_ActivationDate_Close_Tag = "</ActivationDate>";
        private const string Xml_Response_ExpirationDate_Open_Tag = "<ExpiryDate>";
        private const string Xml_Response_ExpirationDate_Close_Tag = "</ExpiryDate>";
        private const string Xml_Response_CardSecurityCode_Open_Tag = "<CSC>";
        private const string Xml_Response_CardSecurityCode_Close_Tag = "</CSC>";
        private const string Xml_Response_CardHolderName_Open_Tag = "<CardHolderName>";
        private const string Xml_Response_CardHolderName_Close_Tag = "</CardHolderName>";
        private const string Xml_Response_FundedAmount_Open_Tag = "<FundedAmount>";
        private const string Xml_Response_FundedAmount_Close_Tag = "</FundedAmount>";

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


        public ENettRequestVANResponse(string requestorDetail)
        {
            // We started initializing new response
            // let's record the process duration
            this.StartDate = DateTime.Now;
            this.RequestorDetail = requestorDetail;
        }

        /// <summary>
        /// set PaymentID
        /// </summary>
        /// <param name="value">VNett PaymentID</param>
        private void SetPaymentID(string value)
        {
            this.PaymentID = value;
        }

        /// <summary>
        /// returns PaymentID
        /// </summary>
        /// <returns>PaymentID</returns>
        private string GetPaymentID()
        {
            return this.PaymentID;
        }


        /// <summary>
        /// set FundedAmount
        /// </summary>
        /// <param name="value">FundedAmount</param>
        private void SetFundedAmount(long value)
        {
            this.FundedAmount = value;
        }

        /// <summary>
        /// returns FundedAmount
        /// </summary>
        /// <returns>FundedAmount</returns>
        private long GetFundedAmount()
        {
            return this.FundedAmount;
        }

        /// <summary>
        /// set cardSecurityCode
        /// </summary>
        /// <param name="value">card Security Code</param>
        private void SetCardSecurityCode(string value)
        {
            this.CardSecurityCode = value;
        }

        /// <summary>
        /// returns card Security Code
        /// </summary>
        /// <returns>card Security Code</returns>
        private string GetCardSecurityCode()
        {
            return this.CardSecurityCode;
        }
        /// <summary>
        /// set activationDate
        /// </summary>
        /// <param name="value">activationDate</param>
        private void SetActivationDate(DateTime value)
        {
            this.ActivationDate = value;
        }

        /// <summary>
        /// returns activationDate
        /// </summary>
        /// <returns>activationDate</returns>
        private DateTime GetActivationDate()
        {
            return this.ActivationDate;
        }

        /// <summary>
        /// set expirationDate
        /// </summary>
        /// <param name="value">expirationDate</param>
        private void SetExpirationDate(DateTime value)
        {
            this.ExpirationDate = value;
        }

        /// <summary>
        /// returns expirationDate
        /// </summary>
        /// <returns>expirationDate</returns>
        private DateTime GetExpirationDate()
        {
            return this.ExpirationDate;
        }

        /// <summary>
        /// set CardHolderName
        /// </summary>
        /// <param name="value">CardHolderName</param>
        private void SetCardHolderName(string value)
        {
            this.CardHolderName = value;
        }

        /// <summary>
        /// returns SupportLogId
        /// </summary>
        /// <returns>SupportLogId</returns>
        private string GetSupportLogId()
        {
            return this.SupportLogId;
        }

        /// <summary>
        /// set SupportLogId
        /// </summary>
        /// <param name="value">SupportLogId</param>
        private void SetSupportLogId(string value)
        {
            this.SupportLogId = value;
        }


        /// <summary>
        /// returns VNettTransactionID
        /// </summary>
        /// <returns>VNettTransactionID</returns>
        private int GetVNettTransactionID()
        {
            return this.VNettTransactionID;
        }

        /// <summary>
        /// set VNettTransactionID
        /// </summary>
        /// <param name="value">VNettTransactionID</param>
        private void SetVNettTransactionID(int value)
        {
            this.VNettTransactionID = value;
        }

        /// <summary>
        /// returns CardHolderName
        /// </summary>
        /// <returns>CardHolderName</returns>
        private string GetCardHolderName()
        {
            return this.CardHolderName;
        }


        /// <summary>
        /// set VAN
        /// </summary>
        /// <param name="value">VAN</param>
        private void SetVAN(string value)
        {
            this.VAN = value;
        }

        /// <summary>
        /// returns VAN
        /// </summary>
        /// <returns>VAN</returns>
        private string GetVan()
        {
            return this.VAN;
        }

        /// <summary>
        /// set Currency
        /// </summary>
        /// <param name="value">Currency</param>
        private void SetCurrency(string value)
        {
            this.Currency = value;
        }

        /// <summary>
        /// returns Currency
        /// </summary>
        /// <returns>Currency</returns>
        private string GetCurrency()
        {
            return this.Currency;
        }

        /// <summary>
        /// set Multiuse
        /// </summary>
        /// <param name="value">Multiuse</param>
        private void SetMultiUse(bool value)
        {
            this.MultiUse = value;
        }

        /// <summary>
        /// returns Multiuse
        /// </summary>
        /// <returns>Multiuse</returns>
        private bool IsMultiUse()
        {
            return this.MultiUse;
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

        /// <summary>
        /// Return values from ENett API
        /// and prepare response to caller
        /// </summary>
        /// <param name="user">user</param>
        /// <param name="vNettresponse">CompleteIssueVNettVANResponse</param>
        /// <param name="ENettRequestVAN">ENettRequestVAN</param>
        public void SetValues(UserInfo user, CompleteIssueVNettVANResponse vNettResponse, ENettRequestVAN reader)
        {

            if (!vNettResponse.GetIssuedVNettResponse().IsSuccessful)
            {
                // The request failed
                // we need to get support log id from ENett
                SetSupportLogId(vNettResponse.GetIssuedVNettResponse().SupportLogId);

                // The request was not succesfully processed
                // Set the exception code and message
                SetException(user, new CEEException(vNettResponse.GetIssuedVNettResponse().ErrorCode.ToString()
                    , vNettResponse.GetIssuedVNettResponse().ErrorDescription).GetCompleteExceptionMessage());

                // No need to go further!
                return;
            }

            // The response is success
            // Let's extract and return to caller all information
            SetVNettTransactionID(vNettResponse.GetIssuedVNettResponse().VNettTransactionID);
            SetPaymentID(vNettResponse.GetReferenceId());
            SetVAN(vNettResponse.GetIssuedVNettResponse().VirtualAccountNumber);
            SetActivationDate(Util.ConvertStringToDate(vNettResponse.GetIssuedVNettResponse().ActivationDate, Const.DateFormat_yyyyMMdd));
            /// EGE - 70723 ENett Response Expiry Date send to FO
            SetExpirationDate(Util.ConvertStringToDate(vNettResponse.GetIssuedVNettResponse().ExpiryDate, Const.DateFormat_MMyyyy));
            SetCardSecurityCode(vNettResponse.GetIssuedVNettResponse().CardSecurityCode);
            SetCardHolderName(vNettResponse.GetIssuedVNettResponse().CardHolderName);
            SetFundedAmount(vNettResponse.GetIssuedVNettResponse().FundedAmount);
            SetMultiUse(Util.ConvertStringToBool(reader.IsMultiUse));
            SetCurrency(vNettResponse.GetIssuedVNettResponse().CurrencyCode);
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
            return String.Format("payment ID ={0}.", GetPaymentID());
        }


        /// <summary>
        /// Returns successfull in string
        /// </summary>
        /// <returns>sucessfull</returns>
        private string GetMultiUseString()
        {
            return (IsMultiUse() ? "true" : "false");
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
                     Xml_Response_Value_Open_Tag
                       + Xml_Response_VNettTransactionID_Open_Tag
                            + GetVNettTransactionID()
                       + Xml_Response_VNettTransactionID_Close_Tag
                       + Xml_Response_VAN_Open_Tag
                            + GetVan()
                       + Xml_Response_VAN_Close_Tag
                       + Xml_Response_IsMultiUse_Open_Tag
                            + GetMultiUseString()
                       + Xml_Response_IsMultiUse_Close_Tag
                       + Xml_Response_PaymentID_Open_Tag
                            + GetPaymentID()
                       + Xml_Response_PaymentID_Close_Tag
                       + Xml_Response_CardSecurityCode_Open_Tag
                            + GetCardSecurityCode()
                       + Xml_Response_CardSecurityCode_Close_Tag
                       + Xml_Response_Currency_Open_Tag
                            + GetCurrency()
                       + Xml_Response_Currency_Close_Tag
                       + Xml_Response_ActivationDate_Open_Tag
                            + Util.ConvertDateToString(GetActivationDate(), Const.DateFormat_yyyyMMdd)
                       + Xml_Response_ActivationDate_Close_Tag
                       + Xml_Response_ExpirationDate_Open_Tag
                            + Util.ConvertDateToString(GetExpirationDate(), Const.DateFormat_yyyyMMdd)
                       + Xml_Response_ExpirationDate_Close_Tag

                       + Xml_Response_CardHolderName_Open_Tag
                            + GetCardHolderName()
                       + Xml_Response_CardHolderName_Close_Tag

                      + Xml_Response_FundedAmount_Open_Tag
                            + GetFundedAmount()
                       + Xml_Response_FundedAmount_Close_Tag

                    + Xml_Response_Value_Close_Tag;
            }
            else
            {
                // On a rencontré une erreur
                // On va retourner les tags d'exception
                // sans les tags de valeur
                strData +=
                Xml_Response_Exception_Open_Tag;
                if(!String.IsNullOrEmpty(GetSupportLogId()))
                {
                    // The rejction is coming from ENett
                    // we have support log id
                    strData += Xml_Response_SupportLogId_Open_Tag
                             + GetSupportLogId()
                            + Xml_Response_SupportLogId_Close_Tag;
                }
                strData += Xml_Response_Exception_Count_Open_Tag
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
