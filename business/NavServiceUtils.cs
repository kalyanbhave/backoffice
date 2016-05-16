//==================================================================== 
// Credit Card Encryption/Decryption Tool
// 
// Copyright (c) 2009-2015 Egencia.  All rights reserved. 
// This software was developed by Egencia An Expedia Inc. Corporation
// La Defense. Paris. France
// The Original Code is Egencia 
// The Initial Developer is Samatar Hassan
//===================================================================

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using SafeNetWS.creditcard;
using SafeNetWS.NavService;
using SafeNetWS.business.response.writer;
using SafeNetWS.utils;
using SafeNetWS.login;
using System.Collections;
using SafeNetWS.database.result;

namespace SafeNetWS.business
{

    public class NavServiceUtils
    {
        // Navision ws exceptions
        private const string ERROR_CODE_PROVIDER_NOT_FOUND = "PROVIDER_NOT_FOUND";
        private const string ERROR_CODE_CARD_TYPE_UNKNOWN = "CARD_TYPE_NOT_FOUND";
        private const string CREDIT_CARD_NO_ONLINE_VALIDATION = "NO_VALIDATION";

        // Navision ws credentials
        private static string NavWsLogin = ConfigurationManager.AppSettings["NavWsLogin"].ToString();
        private static string NavWsPassword = ConfigurationManager.AppSettings["NavWsPassword"].ToString();


        /// <summary>
        /// Return Financial and enhanced flow
        /// from navision
        /// </summary>
        /// <param name="user">User information</param>
        /// <param name="ci">Card information</param>
        /// <returns>Updated card information</returns>
        public static void GetPaymentSettings(UserInfo user, CardInfos ci)
        {
            Navision nws = null;
            try
            {
                // instanciate a new webservice
                nws = new Navision();

                // prepare return
                NAV_CardTypeProviders res = new NAV_CardTypeProviders();

                // call the method 
                nws.GetMerchantAndEnhancedFlow(NavWsLogin, NavWsPassword, ci.GetPOS(), ci.GetNavisionCardName()
                    , Util.ConvertIntToBool(ci.GetNavisionLodgedCard()), ci.GetCardNumber().Substring(0, 6), ref res);

                //nws.GetMerchantAndEnhancedFlow("s-sqlsvc-nav", "G3kt*138!", "france", "VISA", false, "411111", ref res);

                // Let's check if we have an exception code
                NavException10 navExcep = res.NavException[0];

                // retrieve exception code
                string exceptionCode = navExcep.NavExceptionCode[0];

                if (!String.IsNullOrEmpty(exceptionCode))
                {
                    // We have an exception
                    // Let's see how kind of error we have here
                    switch (exceptionCode)
                    {
                        case ERROR_CODE_PROVIDER_NOT_FOUND:
                            // Provider not found..no mapping for this card
                            // We will put default values
                            ci.SetNavisionFinancialFlow(string.Empty);
                            ci.SetNavisionEnhancedFlow(string.Empty);
                            ci.SetOnlineValidation(CREDIT_CARD_NO_ONLINE_VALIDATION);
                            return;
                        case ERROR_CODE_CARD_TYPE_UNKNOWN:
                            // unknow card type
                            // just raise the issue to the caller
                            throw new Exception(user.GetMessages().GetString("CardTypeNotAllowedByNavision", ci.GetTruncatedPAN(), ci.GetCardType(), true));
                        default:
                            throw new Exception(navExcep.NavExceptionDesc[0]);
                    }
                }


                // everything is fine
                // we have the mapping
                NAV_CardTypeProvider ret = res.NAV_CardTypeProvider[0];
                // let's put values
                ci.SetNavisionFinancialFlow(ret.FinancialFlow);
                ci.SetNavisionEnhancedFlow(ret.EnhancedFlow);
                ci.SetOnlineValidation(ret.OnlineCheck);
            }
            finally
            {
                if (nws != null) nws.Dispose();
            }
        }


        /// <summary>
        /// Get traveller payment means for a traveller on a service
        /// parameter retval will be updated
        /// </summary>
        /// <param name="retval">Traveller payment means</param>
        public static void GetTravellerPaymentMeans(TravelerPaymentMeansResponse retval)
        {
            Navision nws = null;
            try
            {
                // Define a new navision ws connection
                nws = new Navision();

                // instanciate a new webservice
                Nav_PaymentMeans pm = new Nav_PaymentMeans();

                // call the method and return the payment means
                nws.GetTravellerPaymentMeans(NavWsLogin, NavWsPassword, retval.GetArgPos(), retval.GetArgComcode(),
                    retval.GetArgPercode(), retval.GetArgServicesList(), retval.GetArgCostCenter(), ref pm);

                // Set result
                retval.SetValue(pm);
            }
            finally
            {
                // Let's dispose now
                if (nws != null) nws.Dispose();
            }
        }

        /// <summary>
        /// Insert credit card in Navision
        /// </summary>
        /// <param name="user">User information</param>
        /// <param name="pos">market</param>
        /// <param name="comcode">customer code</param>
        /// <param name="cc1">analytical code 1</param>
        /// <param name="percode">traveler code</param>
        /// <param name="service">product</param>
        /// <param name="retval">value returned</param>
        /// <param name="cardInfos">card informations</param>
        /// <param name="forceWarning">force warning flag</param>
        /// <param name="contextSource">context source for transactional card</param>
        /// <param name="context">context information for transactional card</param>
        public static void InsertPaymentCard(UserInfo user, string pos, string comcode, string cc1, string percode, string service,
            InsertCardInNavisionResult retval, CardInfos cardInfos, int forceWarning, string contextSource, string context)
        {
            Navision nws = null;
            try
            {
                // Define a new navision ws connection
                nws = new Navision();

                // prepare output
                Nav_InsertPaymentCard ni = new Nav_InsertPaymentCard();

                // insert credit card
                nws.InsertPaymentCard(NavWsLogin, NavWsPassword, pos, comcode, String.IsNullOrEmpty(percode) ? string.Empty : percode
                    , String.IsNullOrEmpty(cc1) ? string.Empty : cc1
                    , Util.ConvertDateToString(cardInfos.GetExpirationDate(), Const.DateFormat_yyyysMMsdd), cardInfos.GetNavisionCardType(),
                        cardInfos.GetDescription(), cardInfos.GetNavisionLodgedCard(), Util.CorrectServiceForNavision(service), user.GetLogin(),
                        cardInfos.GetToken(), cardInfos.GetTruncatedPAN(),
                        cardInfos.GetFirstCardReference(), cardInfos.IsNavisionTransactional(), forceWarning, contextSource, context, ref ni);
                // set value
                retval.SetValues(ni);
            }
            finally
            {
                // Let's dispose now
                if (nws != null) nws.Dispose();
            }
        }

        /// <summary>
        /// Test function for Navision webservice
        /// Ask payment means for customer 2
        /// </summary>
        public static void Test()
        {

            Navision nws = null;
            try
            {
                // Define a new navision ws connection
                nws = new Navision();

                // instanciate a new webservice
                Nav_PaymentMeans pm = new Nav_PaymentMeans();

                // call the method and return the payment means
                nws.GetTravellerPaymentMeans(NavWsLogin, NavWsPassword, Const.PosFrance, "2",
                    string.Empty, Const.ServiceAIR, string.Empty, ref pm);

            }
            finally
            {
                // Let's dispose now
                if (nws != null) nws.Dispose();
            }
        }

    }

}