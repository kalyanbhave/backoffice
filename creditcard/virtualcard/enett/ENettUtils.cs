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
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using SafeNetWS.business.arguments.reader;
using SafeNetWS.ENettService;
using System.Security.Cryptography;
using SafeNetWS.utils;
using SafeNetWS.business;
using SafeNetWS.login;
using SafeNetWS.database.result;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SafeNetWS.creditcard.virtualcard.enett
{
    public class EnettUtils
    {
        private const string CardTypeName = "MASTERCARD";
        private const string XMLHeaderRequestVAN = "RequestVAN";
        private const string XMLHeaderAmendVAN = "AmendVAN";
        private const string XMLHeaderGetVANDetails = "GetVANDetails";
        private const string XMLHeaderCancelVANDetails = "CancelRequestVAN";

        // Requestor ECN detail are stored in config file
        public static string ENettRequestorECN = ConfigurationManager.AppSettings["ENettRequestorECN"].ToString();
        public static string ENettIntegratorCode = ConfigurationManager.AppSettings["ENettIntegratorCode"].ToString();
        public static string ENettIntegratorAccessKey = ConfigurationManager.AppSettings["ENettIntegratorAccessKey"].ToString();
        public static string ENettClientAccessKey = ConfigurationManager.AppSettings["ENettClientAccessKey"].ToString();


        /// <summary>
        /// Return requestor ECN
        /// </summary>
        /// <returns>Requestor ECN</returns>
        private static int GetRequestorECN()
        {
            return Util.ConvertStringToInt(ENettRequestorECN);
        }

        /// <summary>
        /// Return integrator codeskyro
        /// </summary>
        /// <returns>Integrator code</returns>
        private static string GetIntegratorCode()
        {
            return ENettIntegratorCode;
        }

        /// <summary>
        /// Return integrator access key
        /// </summary>
        /// <returns>Integrator access key</returns>
        private static string GetIntegratorAccessKey()
        {
            return ENettIntegratorAccessKey;
        }

        /// <summary>
        /// Return client access key
        /// </summary>
        /// <returns>Client access key</returns>
        private static string GetClientAccessKey()
        {
            return ENettClientAccessKey;
        }

        // Begin EGE-85532
        /// <summary>
        /// Cancel VAN
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="reader">ENettCancelRequestVAN</param>
        /// <returns>CancelVNettVANResponse</returns>
        /// The below method is used for CANCELVAN
        public static CancelVNettVANRequest GetCancelVANRequest(UserInfo user, ENettCancelRequestVAN reader)
        {
            // Let's request For cancel VAN
            CancelVNettVANRequest CancelReq = new CancelVNettVANRequest();

            // Set ENett Details
       
            CancelReq.IntegratorCode = GetIntegratorCode();
            CancelReq.RequesterECN = GetRequestorECN();
            CancelReq.IntegratorAccessKey = GetIntegratorAccessKey();
            // set and send ECN, PaymentID, User to CANCEL van method
            CancelReq.IssuedToEcn = Util.ConvertStringToInt(reader.ECN);
            CancelReq.IssuedToEcnSpecified = true;
            CancelReq.CancelReason = CancelReasonType.BookingCancelled;
            CancelReq.IntegratorReference = reader.PaymentID;
            CancelReq.Username = reader.UserName;
         
            // Get message digest (Accesskey + PaymentID (integratorReference) + ECN + UserName )
            StringBuilder builder = new StringBuilder();
            builder.Append(GetClientAccessKey());
            builder.Append(reader.PaymentID);
            builder.Append(reader.ECN);
            builder.Append(reader.UserName);

            // Send message digest
            string msgDigest = builder.ToString();

            byte[] arr = Encoding.Default.GetBytes(msgDigest);
            CancelReq.MessageDigest = SHA1HashEncode(arr);

          

            return CancelReq;
        }
        // END EGE-85532



        /// <summary>
        /// Prepare new ENett Virtual Account Number (VAN)
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="reader">ENettVANRequestReader</param>
        /// <returns>CompleteIssueVNettVANRequest</returns>
        /// The below method is used for RequestVAN
        public static CompleteIssueVNettVANRequest GetENettVANRequest(UserInfo user, ENettRequestVAN reader)
        {
            // Let's request a new VAN
            IssueVNettVANRequest amdReq = new IssueVNettVANRequest();
            CompleteIssueVNettVANRequest retval = new CompleteIssueVNettVANRequest();

            // Will be returned from ECN
            // Get ECN details
            int issuedECN = Util.ConvertStringToInt(reader.ECN);

            // Generate a new reference
            string referenceId = GenerateENettTransactionID();
            retval.SetReferenceId(referenceId);

            amdReq.IntegratorReference = referenceId;
            amdReq.IntegratorCode = GetIntegratorCode();
            amdReq.IntegratorAccessKey = GetIntegratorAccessKey();
            amdReq.RequesterECN = GetRequestorECN();
            amdReq.IssuedToEcn = issuedECN;

            amdReq.CountryCode = reader.Market;
            amdReq.CurrencyCode = reader.Currency;
            amdReq.CardTypeName = CardTypeName;

            amdReq.FundingCurrencyCode = reader.Currency;
            amdReq.MaximumAuthorisationAmount = Util.ConvertStringToLong(reader.MaxAuthAmount);
            string activationDate = Util.ConvertDateToString(Util.ConvertStringToDate(reader.ActivationDate, Const.ExpirationDateFormat), Const.DateFormat_yyyyMMdd);
            amdReq.ActivationDate = activationDate;
            amdReq.ExpiryDate = Util.ConvertDateToString(Util.ConvertStringToDate(reader.ExpiryDate, Const.ExpirationDateFormat), Const.DateFormat_yyyyMMdd);
            amdReq.IsMultiUse = Util.ConvertStringToBool(reader.IsMultiUse);

            long minumumAuthorisationAmount = Util.ConvertStringToLong(reader.MinAuthAmount);
            if (amdReq.IsMultiUse)
            {
                amdReq.IsMultiUseSpecified = true;
                amdReq.MultiUseClosePercentage = Util.ConvertStringToInt(reader.MultiUseClosePercentage);
                amdReq.MultiUseClosePercentageSpecified = true;
                minumumAuthorisationAmount = 0L;
            }

            amdReq.MinimumAuthorisationAmount = minumumAuthorisationAmount;

            // Add user references
            amdReq.MerchantCategoryName = reader.MerchantCategory;
            amdReq.UserReference1 = referenceId;
            amdReq.UserReference2 = reader.UserName;
            amdReq.UserReference3 = reader.Product;



            string travelerName = "Unknown";
            string travelerPercode = "Unknown";

            // We need here to return traveler code and name
            // It depends on which Enett implementation we are
            // in the phase 1, percode and traveler name are provided for one traveller
            //   <PerCode>2001</PerCode>
            //   <TravellerName>Sudhakar</TravellerName>
            // in phase 2, multiple travallers are sent
            // so we need to extract the main traveller
            // <Travellers>
            //    <Traveller>
            //      <IsMainTraveller>true</IsMainTraveller>
            //      <Name>Claire Burns</Name>
            //      ...
            //    </Traveller> 
            // <//Travellers>
            //
            if (String.IsNullOrEmpty(reader.PerCode))
            {
                // We have on phase 2 with multiple travellers
                // we need to find the main traveler
                List<ENettRequestVAN.Traveller> travs = reader.Travellers;

                foreach (ENettRequestVAN.Traveller trav in travs)
                {
                    if (Util.ConvertStringToBool(trav.IsMainTraveller))
                    {
                        // This is the main traveller
                        travelerPercode = trav.Percode;
                        travelerName = trav.Name;
                    }
                }
            }
            else
            {
                // We are in phase 1, only one traveler is provided
                travelerPercode = reader.PerCode;
                travelerName = reader.TravellerName;
            }




            amdReq.UserReference4 = travelerName;
            amdReq.Username = reader.UserName;
            

            UserReference[] str = new UserReference[5];
            str[0] = new UserReference();
            str[0].Value = travelerPercode;
            str[1] = new UserReference();
            str[1].Value = Util.ConvertDateToString(Util.ConvertStringToDate(reader.BookingDate, Const.ExpirationDateFormat), Const.DateFormat_yyyyMMdd);

            // Payment ID
            str[2] = new UserReference();
            str[2].Value = reader.SupplierName;
            // Username
            str[3] = new UserReference();
            str[3].Value = string.Empty;
            //>>EGE-6880      
            // new User Refernce for Channel 
            str[4] = new UserReference();
            str[4].Value = reader.Channel;
            //<<EGE-6880  

            amdReq.UserReferences = str;


            // Get message digest
            StringBuilder builder = new StringBuilder();
            builder.Append(GetClientAccessKey());
            builder.Append(referenceId);
            builder.Append(activationDate);
            builder.Append(CardTypeName);
            builder.Append(reader.Currency);
            builder.Append(issuedECN);
            builder.Append(reader.MaxAuthAmount);
            builder.Append(minumumAuthorisationAmount);
            builder.Append(reader.MerchantCategory);
            builder.Append(referenceId);
            builder.Append(reader.UserName);

            // Send message digest
            string msgDigest = builder.ToString();

            byte[] arr = Encoding.Default.GetBytes(msgDigest);
            amdReq.MessageDigest = SHA1HashEncode(arr);

            retval.SetIssuedVNettRequest(amdReq);

            return retval;
        }


        /// <summary>
        /// Prepare new ENett Virtual Account Number (VAN)
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="reader">ENettGetVANDetails</param>
        /// <returns>GetVNettVANRequest</returns>
        public static GetVNettVANRequest GetENettVANDetails(UserInfo user, ENettGetVANDetails reader)
        {
            // Let's request VAN details
            GetVNettVANRequest amdReq = new GetVNettVANRequest();

            amdReq.IntegratorReference = reader.PaymentID;
            amdReq.IntegratorCode = GetIntegratorCode();
            amdReq.IntegratorAccessKey = GetIntegratorAccessKey();
            amdReq.RequesterECN = GetRequestorECN();


            // Please follow the same order. Client Access key + ECN + PaymentID  (IntegratorReference)
            string msgDigest = GetClientAccessKey() + GetRequestorECN() + reader.PaymentID;
            byte[] arr = System.Text.Encoding.Default.GetBytes(msgDigest);
            amdReq.MessageDigest = SHA1HashEncode(arr);

            return amdReq;
        }

        /// <summary>
        /// Get information of existing van Details(VAN)
        /// Start EGE-100347 
        /// This method is used for set the values for amend van request

        public static GetVNettVANRequest AmendGetENettVANDetails(UserInfo user, string PaymentId)
        {
            // Let's request VAN details
            GetVNettVANRequest amdReq = new GetVNettVANRequest();

            amdReq.IntegratorReference = PaymentId;
            amdReq.IntegratorCode = GetIntegratorCode();
            amdReq.IntegratorAccessKey = GetIntegratorAccessKey();
            amdReq.RequesterECN = GetRequestorECN();


            // Please follow the same order. Client Access key + ECN + PaymentID  (IntegratorReference)
            string msgDigest = GetClientAccessKey() + GetRequestorECN() + PaymentId;
            byte[] arr = System.Text.Encoding.Default.GetBytes(msgDigest);
            amdReq.MessageDigest = SHA1HashEncode(arr);

            return amdReq;
        }
        // This function is used for check the status of van history active type is closed or not ?
        public static Boolean CheckStatus(GetVNettVANResponse retval1)
        {
            VanHistory[] VH = retval1.VanHistoryCollection;
            int VHlength = VH.Length;
            Boolean status =  VH[VHlength - 1].ActivityType.ToString() == "Close";
            return status;
        }

        // End EGE-100347

        /// Prepare new Amend ENett Virtual Account Number (VAN)
        /// </summary>
        /// <param name="reader">ENettAmendVAN</param>
        /// <returns>AmendVNettVANRequest</returns>
        public static AmendVNettVANRequest ENettAmendVAN(UserInfo user, ENettAmendVAN reader)
        {
            // Let's request a new VAN
            AmendVNettVANRequest amdReq = new AmendVNettVANRequest();

            // Will be returned from ECN
            // Extract ECN details
            int issuedECN = Util.ConvertStringToInt(reader.ECN);

            amdReq.IntegratorCode = GetIntegratorCode();
            amdReq.IntegratorAccessKey = GetIntegratorAccessKey();
            amdReq.RequesterECN = GetRequestorECN();
            amdReq.IssuedToEcn = issuedECN;

            amdReq.IntegratorReference = reader.PaymentID;
            amdReq.FundingCurrencyCode = reader.Currency;
            amdReq.MaximumAuthorisationAmount = Util.ConvertStringToLong(reader.MaxAuthAmount);
            string activationDate = Util.ConvertDateToString(Util.ConvertStringToDate(reader.ActivationDate, Const.ExpirationDateFormat), Const.DateFormat_yyyyMMdd);
            amdReq.ActivationDate = activationDate;
            string expirydDate = Util.ConvertDateToString(Util.ConvertStringToDate(reader.ExpiryDate, Const.ExpirationDateFormat), Const.DateFormat_yyyyMMdd);
            amdReq.ExpiryDate = expirydDate;
            amdReq.IsMultiUse = Util.ConvertStringToBool(reader.IsMultiUse);
            long minumumAuthorisationAmount = Util.ConvertStringToLong(reader.MinAuthAmount);
            if (amdReq.IsMultiUse)
            {
                amdReq.IsMultiUseSpecified = true;
                amdReq.MultiUseClosePercentage = Util.ConvertStringToInt(reader.MultiUseClosePercentage);
                amdReq.MultiUseClosePercentageSpecified = true;
                minumumAuthorisationAmount = 0L;
            }
            amdReq.MinimumAuthorisationAmount = minumumAuthorisationAmount;
            amdReq.MerchantCategoryName = reader.MerchantCategory;
            amdReq.Username = reader.UserName;
            amdReq.UserReference1 = reader.PaymentID;
            amdReq.UserReference2 = reader.UserName;
            string bookingDate = Util.ConvertDateToString(Util.ConvertStringToDate(reader.BookingDate, Const.ExpirationDateFormat), Const.DateFormat_yyyyMMdd);

            amdReq.UserReference3 = reader.Product;


            string travelerName="Unknown";
            string travelerPercode= "Unknown";

            // We need here to return traveler code and name
            // It depends on which Enett implementation we are
            // in the phase 1, percode and traveler name are provided for one traveller
            //   <PerCode>2001</PerCode>
            //   <TravellerName>Sudhakar</TravellerName>
            // in phase 2, multiple travallers are sent
            // so we need to extract the main traveller
            // <Travellers>
            //    <Traveller>
            //      <IsMainTraveller>true</IsMainTraveller>
            //      <Name>Claire Burns</Name>
            //      ...
            //    </Traveller> 
            // <//Travellers>
            //
            if (String.IsNullOrEmpty(reader.PerCode))
            {
                // We have on phase 2 with multiple travellers
                // we need to find the main traveler
                List<ENettAmendVAN.Traveller> travs = reader.Travellers;

                foreach (ENettAmendVAN.Traveller trav in travs)
                {
                    if (Util.ConvertStringToBool(trav.IsMainTraveller))
                    {
                        // This is the main traveller
                        travelerPercode = trav.Percode;
                        travelerName = trav.Name;
                    }
                }
            }
            else
            {
                // We are in phase 1, only one traveler is provided
                travelerPercode = reader.PerCode;
                travelerName = reader.TravellerName;
            }
            

            amdReq.UserReference4 = travelerName;
            // send user references
            UserReference[] str = new UserReference[5];
            // product
            str[0] = new UserReference();
            str[0].Value = travelerPercode;

            // PNR
            str[1] = new UserReference();
            str[1].Value = bookingDate;
            // Payment ID
            str[2] = new UserReference();
            str[2].Value = reader.SupplierName;
            // Username
            str[3] = new UserReference();
            str[3].Value = reader.PNR;
            //>>EGE-6880  
            // new User Refernce for Channel 
            str[4] = new UserReference();
            str[4].Value = reader.Channel;
            //<<EGE-6880  

            amdReq.UserReferences = str;


            // Get message digest
            StringBuilder builder = new StringBuilder();
            builder.Append(GetClientAccessKey());
            builder.Append(reader.PaymentID);
            builder.Append(activationDate);
            builder.Append(issuedECN);
            builder.Append(reader.MaxAuthAmount);
            builder.Append(minumumAuthorisationAmount);
            builder.Append(reader.MerchantCategory);
            builder.Append(reader.PaymentID);
            builder.Append(reader.UserName);


            // Send message digest
            string msgDigest = builder.ToString();

            byte[] arr = Encoding.Default.GetBytes(msgDigest);
            amdReq.MessageDigest = SHA1HashEncode(arr);

            return amdReq;
        }


        /// <summary>
        /// Returns hashcode
        /// </summary>
        /// <param name="arr1">data</param>
        /// <returns>SHA1 hashcode</returns>
        private static string SHA1HashEncode(byte[] arr1)
        {
            SHA1 a = new SHA1CryptoServiceProvider();
            byte[] arr = new byte[1000];
            string hash = string.Empty;
            arr = a.ComputeHash(arr1);
            hash = ByteArrayToString(arr);
            return hash;
        }

        private static string ByteArrayToString(byte[] arrInput)
        {
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            return Convert.ToBase64String(arrInput);

        }


        /// <summary>
        /// Generate ENettTransactionID
        /// </summary>
        /// <returns>Returns ENett transaction</returns>
        private static string GenerateENettTransactionID()
        {
            string randomString = Util.GetUniqueKey(12).ToUpper();
            randomString = randomString + Util.CalculateAlphaNumberWithPadding(randomString);

            return randomString;
        }

        /// <summary>
        /// Reads request for requestVAN and
        /// parse input
        /// </summary>
        /// <param name="xmlRequest">XML request</param>
        /// <returns>ENettRequestVAN</returns>
        public static ENettRequestVAN ReadRequest(string xmlRequest)
        {
            //XML Serialization
            ENettRequestVAN RequestObject = new ENettRequestVAN();
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = XMLHeaderRequestVAN;
            xRoot.IsNullable = true;
            

            XmlSerializer mySerializer = new XmlSerializer(typeof(ENettRequestVAN), xRoot);

            using (TextReader tr = new StringReader(xmlRequest))
            {
                RequestObject = (ENettRequestVAN)mySerializer.Deserialize(tr);
            }

            return RequestObject;
        }



        /// <summary>
        /// Reads request for requestVANDetails and
        /// parse input
        /// </summary>
        /// <param name="xmlRequest">XML request</param>
        /// <returns>ENettRequestVAN</returns>
        public static ENettGetVANDetails ReadGetVANDetails(string xmlRequest)
        {
            //XML Serialization
            ENettGetVANDetails RequestObject = new ENettGetVANDetails();
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = XMLHeaderGetVANDetails;
            xRoot.IsNullable = true;


            XmlSerializer mySerializer = new XmlSerializer(typeof(ENettGetVANDetails), xRoot);

            using (TextReader tr = new StringReader(xmlRequest))
            {
                RequestObject = (ENettGetVANDetails)mySerializer.Deserialize(tr);
            }
           return RequestObject;
        }
        // EGE-85532
        /// <summary>
        /// Reads request for Request Cancel VAN and
        /// parse input
        /// </summary>
        /// <param name="xmlRequest">XML request</param>
        /// <returns>cnacelvanresponse</returns>
        public static ENettCancelRequestVAN ReadCancelVANDetails(string xmlRequest)
        {
            //XML Serialization
            ENettCancelRequestVAN RequestObject = new ENettCancelRequestVAN();
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = XMLHeaderCancelVANDetails;
            xRoot.IsNullable = true;


            XmlSerializer mySerializer = new XmlSerializer(typeof(ENettCancelRequestVAN), xRoot);

            using (TextReader tr = new StringReader(xmlRequest))
            {
                RequestObject = (ENettCancelRequestVAN)mySerializer.Deserialize(tr);
            }
            return RequestObject;
        }


        /// <summary>
        /// Reads request for amendVAN and
        /// parse input
        /// </summary>
        /// <param name="xmlRequest">XML request</param>
        /// <returns>ENettAmendVAN</returns>
        public static ENettAmendVAN ReadAmend(string xmlRequest)
        {
            //XML Serialization
            ENettAmendVAN RequestObject = new ENettAmendVAN();
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = XMLHeaderAmendVAN;
            xRoot.IsNullable = true;

            XmlSerializer mySerializer = new XmlSerializer(typeof(ENettAmendVAN), xRoot);

            using (TextReader tr = new StringReader(xmlRequest))
            {
                RequestObject = (ENettAmendVAN)mySerializer.Deserialize(tr);
            }
            return RequestObject;
        }


        /// <summary>
        /// The following method is invoked by the RemoteCertificateValidationDelegate.
        /// This allows you to check the certificate and accept or reject it
        /// return true will accept the certificate
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="certificate">X509Certificate certificate</param>
        /// <param name="chain">X509Chain chain</param>
        /// <param name="sslPolicyErrors">SslPolicyErrors errors</param>
        /// <returns>Accept certificate</returns>
        private static bool RemoteCertificateCallback(Object sender, X509Certificate certificate
            , X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }


        /// <summary>
        /// Set service point manager
        /// </summary>
        public static void SetServicePointManager()
        {
            //  Accept an invalid SSL certificate programmatically.
            //System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Specifies the security protocols that are supported by the Schannel security package. 
            // Use TLS protocol in place of SSLv3.0 due to POODLE attack
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;


            // Create an object from the (server) certificate file  
            ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateCallback;
        }
    }
}
