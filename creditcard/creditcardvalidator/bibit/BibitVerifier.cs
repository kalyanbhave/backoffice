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
using System.Net;
using System.Configuration;
using SafeNetWS.creditcard.creditcardvalidator;
using SafeNetWS.www;
using SafeNetWS.utils.crypting;
using SafeNetWS.login;
using SafeNetWS.business;
using SafeNetWS.log;
using SafeNetWS.utils;

namespace SafeNetWS.creditcard.creditcardvalidator.bibit
{
    public class BibitVerifier
    {
        // Paramètres de l'appel au service web RBS
        public static string RBSServiceUrl = ConfigurationManager.AppSettings["RBSServiceUrl"];
        public static string RBSServiceLogin = ConfigurationManager.AppSettings["RBSUsername"];
        public static string RBSServicePassword = EncDec.DecryptPassword(ConfigurationManager.AppSettings["RBSPassword"]);
        public static string RBSRequestDescription = ConfigurationManager.AppSettings["RBSRequestDescription"];
        public static string RBSRequestDefaultHolderName = ConfigurationManager.AppSettings["RBSRequestDefaultHoldername"];
        public static string RBSRequestSessionId = ConfigurationManager.AppSettings["RBSRequestSessionId"];
        public static string RBSRequestShopperIPAddress = ConfigurationManager.AppSettings["RBSRequestShopperIPAddress"];
        // Time out (secondes) par défaut pour la création des cartes depuis les procédures BO
        public static string RBSRequestBODefaultTimeOut = ConfigurationManager.AppSettings["RBSRequestBODefaultTimeOut"];

        // Activation Validation en ligne des cartes BIBIT
        public static bool RBSRequestOn = Util.IsOptionOn("RBSRequestActivate");
        // Activation cache Bibit
        public static bool RBSRequestCacheOn = Util.IsOptionOn("RBSRequestUseCache");



        /// <summary>
        /// Vérification des cartes BIBIT
        /// via un service RBS (Royal Bank of Scotland)
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="card">Informations carte</param>
        /// <param name="timeOut">Timeout (s)</param>
        /// <returns>BibitVerifierResult</returns>
        public static ProviderVerifierResult CheckCard(UserInfo user, CardInfos card, int timeOut)
        {
            ProviderVerifierResult retval = new ProviderVerifierResult();

            try
            {
                // Do we need to do zero amount validation?
                bool doZeroValidation = (card.GetOnlineValidation() == CardInfos.CardOnlineValidations.ZERO_AMOUNT);

                // Construction de la requête
                RBSPaymentServiceRequest rs = new RBSPaymentServiceRequest(user, card, doZeroValidation);

                Services.WriteOperationStatusToLog(user, String.Format(" - Info : Online validation : Try {0} for card type = {1}, truncated PAN = {2}, POS = {3}",
                    (doZeroValidation ? "Zero Amount" : "One Amount"), card.GetCardType(), card.GetTruncatedPAN(), card.GetPOS()));

                // Appel du service et récupération de la réponse
                // if the zero amount is enabled then we will send zero amount first
                // if the card was refused, then we will try with one amount
                string response = HttpUtil.HttpPost(user, RBSServiceUrl, RBSServiceLogin, RBSServicePassword, rs.GetXML(), timeOut);
 
                // Traitement de la réponse
                RBSPaymentServiceResponse res = new RBSPaymentServiceResponse(response);

                Services.WriteOperationStatusToLog(user, String.Format(" - Info : Online validation : {0} validation done (Valid = {1}) for card type = {2}, truncated PAN = {3}, POS = {4}",
                          (doZeroValidation ? "Zero Amount" : "One Amount"), res.isSuccess(),card.GetCardType(), card.GetTruncatedPAN(), card.GetPOS()));

                // We have our response
                // that's not the end. If the card was refused and we have send zero amount validation
                // if that case, we need to try with one amount
                if (!res.isSuccess() && doZeroValidation)
                {
                    Services.WriteOperationStatusToLog(user, String.Format(" - Info : Online validation - Zero Amount Validation : the card was refused..we will try with One amount for card type = {0}, truncated PAN = {1}, POS = {2}",
                          card.GetCardType(), card.GetTruncatedPAN(), card.GetPOS()));

                    // the card was refused with Zero amount
                    // let's try with one amount
                    rs = new RBSPaymentServiceRequest(user, card, false);

                    // let's try again with one amount
                    response = HttpUtil.HttpPost(user, RBSServiceUrl, RBSServiceLogin, RBSServicePassword, rs.GetXML(), timeOut);

                    // let's parse response
                    res = new RBSPaymentServiceResponse(response);

                    Services.WriteOperationStatusToLog(user, String.Format(" - Info : Online validation - One Amount validation after Zero done (Valid = {0}) for card type = {1}, truncated PAN = {2}, POS = {3}"
                         ,res.isSuccess(), card.GetCardType(), card.GetTruncatedPAN(), card.GetPOS()));
                }


                // et retour du statut
                // on va egalement retourner la réponse entière du service RBS
                // de plus on ajoute le numéro de la transaction
                retval.SetSuccess(res.isSuccess());
                retval.SetCompleteResponse(res.GetInputResponse());
                retval.SetOrderCode(rs.GetOrderCode());
                
            }
            catch (WebException ex)
            {
                // Récupération de l'exception
                string code = Const.StatusConnectionError;
                if (ex.Status.Equals(WebExceptionStatus.Timeout))
                {
                    // On a eu un timeout
                    // il faut tracer cette erreur dans une table
                    // avant de lever l'exception
                    code = Const.StatusTimeOut;
                }
                retval.SetInformationCode(code);
                retval.SetInformationMessage(ex.Message);
            }
            catch (Exception e)
            {
                // Erreur lors de la vérication de la carte
                // via le service RBS
                // L'erreur peut être un TimeOut
                // Dans tous les cas, on doit ignorer cette exception
                // et informer le client que la carte est valide

                retval.SetInformationCode(Const.StatusConnectionError);
                retval.SetInformationMessage(e.ToString());
            }
            return retval;
        }


        /// <summary>
        /// Vérification des cartes BIBIT
        /// via un service RBS (Royal Bank of Scotland)
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="card">Informations carte</param>
        /// <returns>BibitVerifierResult</returns>
        public static ProviderVerifierResult CheckCard(UserInfo user, CardInfos card)
        {
            return CheckCard(user, card, HttpUtil.NoTimeOut);
        }


        /// <summary>
        /// Méthode de vérification de la connectivité RBS
        /// On soumet la validation pour une carte fictive
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        public static void CheckBibitVerifier(UserInfo user)
        {
            try
            {
                // Construction de la requête
                // Les valeurs sont fictives
                RBSPaymentServiceRequest rs = new RBSPaymentServiceRequest(user, "FRANCE", "4111111111111111",DateTime.Now.AddYears(2), string.Empty, "VISA", false);

                // Appel du service et récupération de la réponse
                string response = HttpUtil.HttpPost(user, RBSServiceUrl, RBSServiceLogin, RBSServicePassword, rs.GetXML(), HttpUtil.NoTimeOut);

                // L'appel a été fait avec succes
                // pas besoin de connaitre si la carte a été acceptée ou rejectée!
                // On a juste de tester la connectivité
            }
            catch (Exception e)
            {
                // Erreur lors de l'appel
                throw new Exception(e.Message);
            }
        }
    }
}
