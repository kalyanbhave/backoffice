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
using System.Web;
using System.ComponentModel;
using System.Web.Services;
using System.Configuration;
using System.IO;
using SafeNetWS.utils;
using SafeNetWS.utils.crypting;
using SafeNetWS.business;
using SafeNetWS.database.result;
using SafeNetWS.business.arguments.quality;
using SafeNetWS.business.arguments.reader;
using SafeNetWS.business.response.writer;
using SafeNetWS.login;
using SafeNetWS.creditcard.creditcardvalidator;
using SafeNetWS.creditcard.creditcardgenerator;
using SafeNetWS.creditcard;
using SafeNetWS.utils.crypting.safenet;
using SafeNetWS.business.response.reader;
using SafeNetWS.www;
using SafeNetWS.log;
using SafeNetWS.creditcard.virtualcard.enett;
using SafeNetWS.test.connectivity;
using System.Runtime.InteropServices;
using System.Reflection;



namespace SafeNetWS
{
    /// <summary>
    /// Ce service web offre divers methodes pour la création
    /// et la visualisation des cartes bancaires
    /// </summary>
    [WebService(Namespace = "urn:BackWebService", Description = "BackOffice payment service")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // Pour autoriser l'appel de ce service Web depuis un script à l'aide d'ASP.NET AJAX, supprimez les marques de commentaire de la ligne suivante. 
    //[System.Web.Script.Services.ScriptService]


    public class WSS : System.Web.Services.WebService  
    {
        
        /// <summary>
        /// Récupération du numéro de carte bancaire depuis le token extrait
        /// d'un fichier de paiement (Détokenisation)
        /// </summary>
        /// <param name="local">Langue</param>
        /// <param name="login">Compte utilisateur</param>
        /// <param name="password">password</param>
        /// <param name="token">Token</param>
        /// <returns>Numéro de carte</returns>
        [WebMethod(Description = "Untokenize (Returns PAN from token.)")]
        public string UnTokenizeCard(string local, string login, string password, string token)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             PanResponse response = new PanResponse(token);
           
             try
             {
                 // On passe les informations minimales
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                    UserInfo.APPLICATION_UNTOKENIZATION, UserInfo.ACTION_DECRYPT));

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // L'utilisateur a les droits suffisants pour la suite
                 // Vérification du token
                 ArgsChecker.ValidateBOToken(response.GetUser(), token);

                 // On récupère le PAN depuis la base des données encryptées
                 response.SetValues(response.GetUser(), Services.GetPanFromBOToken(response.GetUser(), Util.ConvertStringToToken(token)));

                 // On incrémente l'indicateur sur le nombre de visualisation de cartes
                 response.GetUser().IncrDisplayCardsCount();
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }

             // On retourne la réponse au client
             return response.GetResponse();
         }

        /// <summary>
        /// Récupération du numéro de carte bancaire depuis le token
        /// </summary>
        /// <param name="local">Langue</param>
        /// <param name="login">Compte utilisateur</param>
        /// <param name="password">Password</param>
        /// <param name="token">Token</param>
        /// <returns>Numéro de carte</returns>
        [WebMethod(Description = "Returns PAN from token (Lookup).")]
        public string GetPanFromToken (string local, string login, string password, string token)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             PanResponse response = new PanResponse(token);

             try
             {
                 // On passe les informations minimales
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                   UserInfo.APPLICATION_PAN_LOOKUP, UserInfo.ACTION_DECRYPT));

                 // Les droits d'accès à cette méthode
                 // seront également vérifiés
                 // depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));
           
                 // L'utilisateur a les droits suffisants pour la suite
                 // Vérification du token
                 ArgsChecker.ValidateBOToken(response.GetUser(), token);

                 // On récupère le PAN depuis la base des données encryptées
                 response.SetValues(response.GetUser(), Services.GetPanFromBOToken(response.GetUser(), Util.ConvertStringToToken(token)));
  
                 // On incrémente l'indicateur sur le nombre de visualisation de cartes
                 response.GetUser().IncrDisplayCardsCount();
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }

             // On retourne la réponse au client
             return response.GetResponse();
         }


        /// <summary>
        /// Récupération du token correspondant à un numéro de carte (Reverse Lookup)
        /// </summary>
        /// <param name="local">Langue</param>
        /// <param name="login">Compte utilisateur</param>
        /// <param name="ppassword">Password</param>
        /// <param name="pan">Numéro de la carte bancaire</param>
        /// <returns>Token</returns>
        [WebMethod(Description = "Returns token from PAN (Reverse lookup).")]
        public string GetTokenFromPan(string local, string login, string password, string pan)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             TokenResponse response = new TokenResponse(pan);

             try
             {
                 // On passe les informations minimales
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                   UserInfo.APPLICATION_TOKEN_LOOKUP, UserInfo.ACTION_DECRYPT));

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));
                 
                 // L'utilisateur a les droits suffisants pour la suite
                 response.SetValues(response.GetUser(), Services.GetTokenFromPan(response.GetUser(), pan));
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // On retourne la réponse au client
             return response.GetResponse();
        }
        
        /// <summary>
        /// Insertion d'une carte bancaire corporate ou privée dans Navision
        /// Le traitement se fait en deux temps :
        /// 1 - Insertion du numéro de carte dans la base des cartes encryptées (Tokenisation)
        /// 2 - Insertion de la carte dans Navision
        /// </summary>
        /// <param name="local">Langue de retour</param>
        /// <param name="login">Compte utilisateur</param>
        /// <param name="password">password</param>
        /// <param name="pos">Pos Navision</param>
        /// <param name="customer">Compte client (comcode)</param>
        /// <param name="cc1">Centre de cout 1 (cc1)</param>
        /// <param name="traveller">Compte utilisateur (percode)</param>
        /// <param name="expirationDate">Date d'expiration de la carte</param>
        /// <param name="service">Service Group (AIR, RAIL, ...)</param>
        /// <param name="pan">Numéro de la carte</param>
        /// <param name="description">Description</param>
        /// <param name="holderName">holderName</param>
        /// <param name="lodgedCard">lodgedCard</param>
        /// <param name="firstCardReference">firstCardReference</param>
        /// <returns>Statut traitement (Token et référence carte, ...)</returns>
        [WebMethod(Description = "Insert corporate or private card in encrypted and Navision databases and return token, reference card.")]
        public string InsertProfilCard(string local, string login, string password, string pos, string customer, string cc1, string traveller,
           string expirationDate, string service, string pan, string description, string holderName, int lodgedCard, string firstCardReference, int forcewarning)
         {
            // On prépare la réponse que l'on va
            // apporter (initialisation)
            InsertCardResponse response = new InsertCardResponse(pan, false);

            try
            {
                // On passe les informations minimales
                response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                    UserInfo.APPLICATION_CARD_INSERT, UserInfo.ACTION_ENCRYPT));

                // On récupère les informations sur l'utilisateur
                // Les droits d'accès à cette méthode
                // seront également vérifiés depuis l'AD
                response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));
              
                // L'utilisateur a les droits suffisants pour la suite
                // On va insérer la carte dans la base des données encryptées
                // et dans Navision
                InsertCardResult res = Services.InsertProfilCard(response.GetUser(), pos, customer, cc1,
                    traveller, expirationDate, service, pan, description, holderName, lodgedCard, firstCardReference,forcewarning);

                // La carte a été correctement insérée dans la base des
                // données encryptées et Navision
                // On va récupérer les informations utiles (Token Référence carte)
                response.SetValues(response.GetUser(), res);
            }
            catch (Exception e)
            {
                // Une exception a été levée 
                // Impossible donc d'insérer cette carte
                // On retourne l'erreur au client
                response.SetException(response.GetUser(), e);
            }
             // On retourne la réponse au client
            return response.GetResponse();
        }
        


        /// <summary>
        /// Insertion d'une carte bancaire transactionnelle dans Navision
        /// Le traitement se fait en deux temps :
        /// 1 - Insertion du numéro de carte dans la base des cartes encryptées (Tokenisation)
        /// 2 - Insertion de la carte dans Navision
        /// </summary>
        /// <param name="local">Langue de retour</param>
        /// <param name="login">Compte utilisateur</param>
        /// <param name="password">Password</param>
        /// <param name="pos">Pos Navision</param>
        /// <param name="customer">Compte client (comcode)</param>
        /// <param name="traveller">Compte voyageur (percode)</param>
        /// <param name="contextBU">Indicateur contexte BU, préfacture, CLE)</param>
        /// <param name="context">Numéro de billet sir contexte BU, autremet numéro de préfacture</param>
        /// <param name="expirationDate">Date d'expiration de la carte</param>
        /// <param name="service">Service Group (AIR, RAIL, ...)</param>
        /// <param name="inputValue">Numéro de carte or Token</param>
        /// <param name="lodgedCard">Lodged card flag</param>
        /// <returns>Statut traitement (Token et référence carte, ...)</returns>
        [WebMethod(Description = "Insert transactional card in encrypted and Navision databases and return token, reference card.")]
        public string InsertTransactionalCard(string local, string login, string password, string pos, string customer, string traveller,
             string contextBU, string context, string expirationDate, string service, string inputValue, string holderName, int lodgedCard, string firstCardReference, int forcewarning)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             InsertTransactCardResponse response = new InsertTransactCardResponse(inputValue);

             try
             {
                 // On passe les informations minimales
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                    UserInfo.APPLICATION_TRANSACTIONAL_CARD_INSERT, UserInfo.ACTION_ENCRYPT));

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));
                
                 // L'utilisateur a les droits suffisants pour la suite
                 // We have to check the input string

                 // Au final, on doit transmettre un PAN
                 string pan = inputValue;

                 // Le client nous a envoyé un token BO, FO ou une carte de credit
                 switch (ArgsChecker.GuessTokenType(response.GetUser(), inputValue))
                 {
                     case ArgsChecker.TOKEN_BO:
                         // Il s'agit donc d'un token BackOffice
                         // Tout d'abord, on récupère le token
                         long TokenBO = Util.ConvertStringToToken(inputValue);

                         // On a récupéré le token
                         // On va maintenant récupérer le PAN depuis le token
                         pan = Services.GetPanFromBOToken(response.GetUser(), TokenBO);
                         break;
                     case ArgsChecker.TOKEN_FO:
                         // Il s'agit d'un token Front Office
                         // On va récupérer le PAN
                         pan = Services.GetPanInfosFromFOToken(response.GetUser(), inputValue, false).GetPAN();
                         break;
                     default:
                         // It's not a token
                         // on will suppose that it's a pan
                         break;
                 }

                 // send the pan and create transactional card
                 InsertCardResult res = Services.InsertTransactionalCard(response.GetUser(), pos, customer,
                    traveller, expirationDate, service, pan, contextBU, context, holderName, lodgedCard, firstCardReference, forcewarning);

                 // Set values
                 response.SetValues(response.GetUser(), res);
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // On retourne la réponse au client
             return response.GetResponse();
         }

        /// <summary>
        /// Insertion d'un numéro de carte bancaire extrait
        /// d'un fichier AIR (Tokenisation fichiers AIR)
        /// </summary>
        /// <param name="local">Langue de retour</param>
        /// <param name="login">Compte utilisateur</param>
        /// <param name="password">Password</param>
        /// <param name="pan">Numéro de carte bancaire</param>
        /// <returns>Token</returns>
        [WebMethod(Description = "Tokenize PAN (insert card in encrypted database and return token.)")]
        public string TokenizeCard(string local, string login, string password, string pan)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             InsertEncryptedCardResponse response = new InsertEncryptedCardResponse(pan);

             try
             {
                 // On passe les informations minimales
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                     UserInfo.APPLICATION_TOKENIZATION, UserInfo.ACTION_ENCRYPT));

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // L'utilisateur a les droits suffisants pour la suite
                 // let's tokenize the card (we dont have expiry date)
                 response.SetValues(response.GetUser(), Services.InsertCardInEncryptedDB(response.GetUser(), pan, null, null, 0));
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // On retourne la réponse au client
             return response.GetResponse();
         }
        


        /// <summary>
        /// Retourne la carte utilisée
        /// par un voyageur dans Navision
        /// </summary>
        /// <param name="login">Compte utilisateur</param>
        /// <param name="password">Password</param>
        /// <param name="askforcard">Arguments</param>
        /// <returns>Informations carte</returns>
        [WebMethod(Description = "Returns hierarchical payment card infos.")]
        public string GetUserBookingPayment(string login, string password, string askforcard)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             UserBookingPaymentResponse response = new UserBookingPaymentResponse(askforcard);

             try
             {
                 // On va parser la chaîne envoyée par le client
                 // pour extraire toutes les informations
                 UserBookingPaymentReader reader = new UserBookingPaymentReader(askforcard);

                 // On passe les informations minimales
                 response.SetUser(new UserInfo(reader.GetLang(), login, password, true, GetRemoteHost(),
                     UserInfo.APPLICATION_MID_OFFICE_ROBOTIC_TOOL, UserInfo.ACTION_DECRYPT));


                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // L'utilisateur a les droits suffisants pour la suite
                 // on peut maintenant rechercher la carte
                 response = Services.GetUserBookingPayment(response.GetUser(), reader, true);
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // On retourne la réponse au client
             return response.GetResponse();
         }

        /// <summary>
        /// Retourne les informations hiérarchiques
        /// sur le mode de paiement dans Navision
        /// </summary>
        /// <param name="login">Compte utilisateur</param>
        /// <param name="password">Password</param>
        /// <param name="askforpaymenttype">Arguments</param>
        /// <returns>Informations sur le mode de paiement</returns>
        [WebMethod(Description = "Returns hierarchical payment mode infos.")]
        public string GetUserPaymentType(string login, string password,  string askforpaymenttype)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             UserPaymentTypeResponse response = new UserPaymentTypeResponse(askforpaymenttype);
        
             try
             {

                 // On va parser la chaîne envoyée par le client
                 // Déclaration
                 UserPaymentTypeReader reader = new UserPaymentTypeReader();
                 // On va extraire les arguments
                 reader.ParseInput(askforpaymenttype);

                 // On passe les informations minimales
                 response.SetUser(new UserInfo(reader.GetLanguage(), login, password, true, GetRemoteHost(),
                     UserInfo.APPLICATION_MID_OFFICE_ROBOTIC_TOOL, UserInfo.ACTION_NONE));

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // L'utilisateur a les droits suffisants pour la suite
                 // On a extrait toutes les informations nécéssaires
                 // On va retourner les informations depuis Navision
                 // Il n'y a aucune information relative à la carte
                 // Il s'agit juste du mode de paiement
                 UserPaymentTypeResult rs = Services.GetUserPaymentType(response.GetUser(), reader);


                 // On a tout ce qui est nécéssaire
                 // On peut répondre au client
                 response.SetValues(response.GetUser(), rs);
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // On retourne la réponse au client
             return response.GetResponse();
         }

        /// <summary>
        /// Update encrypted PAN after key rotation
        /// </summary>
        /// <param name="local">Language</param>
        /// <param name="login">Username</param>
        /// <param name="password">Password</param>
        /// <returns>Processing status</returns>
        [WebMethod(Description = "Update crypted value for all tokens after a key rotation.")]
        public string UpdateAllTokensAfterKeyRotation(string local, string login, string password)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             KeyRotationResponse response = new KeyRotationResponse();

             try
             {
                 // On passe les informations minimales
                 //>>EGE-67026
                 //response.SetUser(new UserInfo(local, login, GetRemoteHost(),
                 //    UserInfo.APPLICATION_KEY_ROTATION, UserInfo.ACTION_ENCRYPT));
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                   UserInfo.APPLICATION_KEY_ROTATION, UserInfo.ACTION_ENCRYPT));
                 //<<EGE-67026

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));
                 // L'utilisateur a les droits suffisants pour la suite

                 // Tous les cryptogrammes
                 // vont etre mis à jour dans la base des données encryptées
                 // On va retourner le nombre de tokens et le nombre en succès et en erreur
                 response.SetValues(response.GetUser(), Services.RotateKeyForAllTokens(response.GetUser()));

                 // La mise à jour s'est déroulé correctement
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // On retourne la réponse au client
             return response.GetResponse();
         }




         /// <summary>
         /// Insertion d'un numéro de carte dans la base des cartes encryptées (Tokenisation)
         /// Hébergée par le FrontOffice
         /// </summary>
         /// <param name="local">Langue</param>
         /// <param name="login">Compte utlisateur</param>
         /// <param name="password">Mot de passe</param>
        /// <param name="inputValue">Numéro de carte bancaire</param>
         /// <param name="expirationDate">Date d'expiration de la carte bancaire</param>
         /// <param name="cardHolder">Détenteur de la carte</param>
         /// <param name="pos">pos</param>
         /// <param name="timeOut">Timeout (s)</param>
         /// <param name="isBibitCheck">Activation vérification cartes BIBIT</param>
         /// <param name="lodgedCard">lodgedcard</param>
         /// <returns>Token</returns>
         [WebMethod(Description = "Inserts card from Front Office in encrypted database and return token.")]
         public string TokenizeFO(string local, string login, string password, string inputValue, string expirationDate,
             string cardHolder, string pos, string timeOut, int isBibitCheck, int lodgedCard)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             InsertEncryptedFOCardResponse response = new InsertEncryptedFOCardResponse(inputValue);

             try
             {
                 // On passe les informations minimales
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                     UserInfo.APPLICATION_FO_TOKENIZATION, UserInfo.ACTION_ENCRYPT));
            
                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // L'utilisateur a les droits suffisants pour la suite
                 response.SetValues(response.GetUser(), Services.InsertCardInEncryptedFODB(response.GetUser(), inputValue, expirationDate, cardHolder, pos,
                     timeOut, Util.ConvertIntToBool(isBibitCheck), Util.ConvertIntToBool(lodgedCard)));
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // On retourne la réponse au client
             return response.GetResponse();
         }

         /// <summary>
         /// Validation d'une carte via le service RBS (BIBIT)
         /// </summary>
         /// <param name="local">Langue de retour des exceptions</param>
         /// <param name="login">Compte utilisateur</param>
         /// <param name="password">Mot de passe</param>
         /// <param name="inputValue">Token FO, Ou ou PAN</param>
         /// <param name="expirationDate">Expiration date</param>
         /// <param name="cardHolder">Détenteur de la carte</param>
         /// <param name="pos">pos</param>
         /// <param name="timeOut">Timeout (s)</param>
         /// <param name="isBibitCheck">Activation vérification cartes BIBIT</param>
         /// <param name="lodgedCard">lodged card (TRUE or FALSE)</param>
         /// <returns>Informations validité de la carte</returns>
         [WebMethod(Description = "Validate credit card thanks to RBS service (BIBIT).")]
         public string ValidateCreditCard(string local, string login, string password, string inputValue,
             string expirationDate, string cardHolder, string pos, string timeOut, int isBibitCheck, int lodgedCard)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             CreditCardValidationResponse response = new CreditCardValidationResponse(inputValue, true);

             try
             {
                 // On passe les informations minimales
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                     UserInfo.APPLICATION_CARD_VALIDATION, UserInfo.ACTION_DECRYPT));

                 // On récupère les informations sur l'utilisateur
                 // depuis l'AD
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // Le client a le droit d'exécuter cette méthode
                 // On va vérifier la carte en tenant compte du timeout
                 response = Services.ValidateCreditCard(response.GetUser(), inputValue, expirationDate, cardHolder, pos,
                     timeOut, Util.ConvertIntToBool(isBibitCheck), Util.ConvertIntToBool(lodgedCard));
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // On retourne la réponse au client
             return response.GetResponse();
         }



         /// <summary>
         /// Récupération des informations d'une carte bancaire depuis le token
         /// FrontOffice (base hébergée par FrontOffice)
         /// </summary>
         /// <param name="local">Langue</param>
         /// <param name="login">Compte utilisateur</param>
         /// <param name="password">Password</param>
         /// <param name="token">Token FrontOffice</param>
         /// <returns>Infos carte</returns>
         [WebMethod(Description = "Save temporary FO card and returns card information (without PAN).")]
         public string SaveTemporaryFOCard(string local, string login, string password, string token)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             FOCardResponse response = new FOCardResponse(token);

             try
             {
                 // On passe les informations minimales
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                 UserInfo.APPLICATION_TOKEN_VALIDATION, UserInfo.ACTION_DECRYPT));
            

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // L'utilisateur a les droits suffisants pour la suite
                 // On va verifier la validité du token
                 ArgsChecker.ValidateFOToken(response.GetUser(), token);

                 // On récupère les informations sur la carte
                 // numéro de carte tronqué, date d'expiration, type de carte
                 response.SetValues(response.GetUser(), Services.GetPanInfosFromFOToken(response.GetUser(), token, false));
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }

             // On retourne la réponse au client
             return response.GetResponse();
         }


         /// <summary>
         /// Récupération des informations d'une carte bancaire depuis le token
         /// FrontOffice (base hébergée par FrontOffice) ou BackOffice
         /// </summary>
         /// <param name="local">Langue</param>
         /// <param name="login">Compte utilisateur</param>
         /// <param name="password">Mot de passe</param>
         /// <param name="pos">Market</param>
         /// <param name="token">Token FrontOffice ou BackOffice</param>
         /// <returns>Infos carte</returns>
         [WebMethod(Description = "Returns PAN from Front Office or BackOffice token (Lookup).")]
         public string UntokenizeFO(string local, string login, string password, string pos, string token)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             FOPanResponse response = new FOPanResponse(token);

             try
             {
                 // On passe les informations minimales
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                     UserInfo.APPLICATION_FO_UNTOKENIZATION, UserInfo.ACTION_DECRYPT));
            
       
                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 if (String.IsNullOrEmpty(token))
                 {
                     // Le token ne peut être vide!
                     throw new Exception(response.GetUser().GetMessages().GetString("EmptyToken", true));
                 }

                 // Le client nous a envoyé un token BO ou FO
                 switch (ArgsChecker.GuessTokenType(response.GetUser(), token))
                 {
                     case ArgsChecker.TOKEN_BO:
                         // Il s'agit donc d'un token BackOffice
                         // Tout d'abord, on récupère le token
                         long TokenBO = Util.ConvertStringToToken(token);

                         // On a récupéré le token
                         // On va maintenant essayer de récupérer les
                         // informations complémentaires (date expiration, cvc, ...) depuis Navision
                         response.SetValues(response.GetUser(), Services.GetExtendedPanInfos(response.GetUser(), pos, TokenBO)); 
                         break;
                     case ArgsChecker.TOKEN_FO:
                         // Il s'agit d'un token Front Office
                         // On récupère les informations sur la carte
                         // numéro de carte tronqué, date d'expiration, type de carte
                         response.SetValues(response.GetUser(), Services.GetPanInfosFromFOToken(response.GetUser(), token, false));
                         break;
                     default:
                         // Ce token est inconnu!
                         throw new Exception(response.GetUser().GetMessages().GetString("UnknownToken",token, true));
                 }
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }

             // On retourne la réponse au client
             return response.GetResponse();
         }

         /// <summary>
         /// Récupération du Token BackOffice depuis le token
         /// FrontOffice (base hébergée par FrontOffice)
         /// </summary>
         /// <param name="local">Langue</param>
         /// <param name="login">Compte utilisateur</param>
         /// <param name="token">Token FrontOffice</param>
         /// <returns>token BackOffice</returns>
         [WebMethod(Description = "Returns BackOffice Token from FrontOffice token (Lookup).")]
         public string GetTokenFromFOToken(string local, string login, string token)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             TokenResponse response = new TokenResponse(token);

             try
             {
                 // On passe les informations minimales
                 // qui permettent d'identifier de client
                 response.SetUser(new UserInfo(local, login, GetRemoteHost()));

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // L'utilisateur a les droits suffisants pour la suite
                 // On va verifier la validité du token
                 ArgsChecker.ValidateFOToken(response.GetUser(), token);

                 // On récupère les informations sur la carte
                 // en réalité que le token BackOffice
                 response.SetValues(response.GetUser(), Services.GetPanInfosFromFOToken(response.GetUser(), token, true).GetBOToken());
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }

             // On retourne la réponse au client
             return response.GetResponse();
         }



         /// <summary>
         /// Retourne des informations sur le statut
         /// de chaque composant sur lequel se base le service web
         /// </summary>
         /// <param name="local">Langue</param>
         /// <param name="login">Compte utilisateur</param>
         /// <param name="password">Password</param>
         /// <returns>Statut par application</returns>
         [WebMethod(Description = "Checks webservice components availability.")]
         public string RunConnectivityTest(string local, string login, string password)
         {
             // Préparation de la réponse à apporter
             TestAllComponentsResponse response = new TestAllComponentsResponse();
             try
             {
                 // On passe les informations minimales
                 // qui permettent d'identifier de client
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                    UserInfo.APPLICATION_AUTO_TEST, UserInfo.ACTION_NONE));

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // On récupère les valeurs
                 ConnectivityTestor.TestAllComponents(response);
             }
             catch (Exception e)
             {
                 // On retourne cette information
                 // au client
                 // On a un souci au niveau LDAP
                 response.SetActiveDirectoryFailed(e);
             }
             // On peut répondre au client
             return response.GetResponse();
         }


        /// <summary>
        /// Génératin d'ID VPayment (AMEX)
        /// L'ID est une structure composée d'un GUID court
        /// doublé d'un checksum
        /// </summary>
        /// <param name="local">Langue de réponse du service</param>
        /// <param name="login">Compte utilisateur</param>
        /// <param name="pos">Point Of Sale</param>
        /// <param name="travelerCode">Code voyageur</param>
        /// <param name="travelerName">Nom du voyageur</param>
        /// <returns></returns>
         [WebMethod(Description = "Generate a VPayment ID for Low Cost")]
         public string GenerateVPaymentIDForLC(string local, string login, string pos, string travelerCode,
             string travelerName, string cc1, string cc2, string tripType, string departureFrom, string goingTo, 
             string departureDate, string returnDate, string company, string bookingDate)
         {
             // On prépare la réponse que l'on va apportée
             VPaymentIDResponse response = new VPaymentIDResponse(VPaymentIDResponse.BookingTypeLC);

             // On prend les arguments
             ArgsForVPaymentIDLC args = new ArgsForVPaymentIDLC(pos, travelerCode, travelerName, cc1, cc2, tripType,
                 departureFrom, goingTo, departureDate, returnDate, company, bookingDate);
             // On prend soin de arguments
             response.SetArguments(args);

             try
             {
                 // On passe les informations minimales
                 // qui permettent d'identifier de client
                 UserInfo user = new UserInfo(local, login, GetRemoteHost(), 
                     UserInfo.APPLICATION_VPAYMENT_ID_GENERATION, UserInfo.ACTION_NONE);

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(user));

                 // On lance la validation des controles
                 // sur les arguments envoyés par le client
                 // On va vérifier les champs obligatoires
                 response.Validate();

                 if (!response.IsError())
                 {
                     // Aucune erreur sur les arguments
                     // On génère l'ID
                     response.SetValues(response.GetUser(), Services.GenerateVPaymentID(response));
                 }

             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.AddException(response.GetUser(), e);
             }
             return response.GetResponse();
         }

         /// <summary>
         /// Génératin d'ID VPayment (AMEX)
         /// L'ID est une structure composée d'un GUID court
         /// doublé d'un checksum
         /// </summary>
         /// <param name="local">Langue de réponse du service</param>
         /// <param name="login">Compte utilisateur</param>
         /// <param name="pos">Point Of Sale</param>
         /// <param name="travelerCode">Code voyageur</param>
         /// <param name="travelerName">Nom du voyageur</param>
         /// <param name="cc1">Main cost center</param>
         /// <param name="cc2">Secondary cost center</param>
         /// <param name="hotelType">Hotel type (Hotelcom or Other)</param>
         /// <param name="hotelType">hotelName</param>
         /// <param name="city">hotel city</param>
         /// <param name="zipCode">hotel city zipcode</param>
         /// <param name="country">hotel country</param>
         /// <param name="arrivalDate">arrival date</param>
         /// <param name="departureDate">departure date from hotel</param>
         /// <param name="bookingDate">bookibg date</param>
         /// <returns></returns>
         [WebMethod(Description = "Generate a VPayment ID for Hotel")]
         public string GenerateVPaymentIDForHotel(string local, string login, string pos, string travelerCode,
             string travelerName, string cc1, string cc2, string hotelType, string hotelName, string city,
             string zipCode, string country, string arrivalDate, string departureDate, string bookingDate)
         {
 
             // On prépare la réponse que l'on va apportée
             VPaymentIDResponse response = new VPaymentIDResponse(VPaymentIDResponse.BookingTypeHotel);
             
             // On prend les arguments
             ArgsForVPaymentIDHotel args = new ArgsForVPaymentIDHotel(pos, travelerCode, travelerName, cc1, cc2, hotelType,
                 hotelName, city, zipCode, arrivalDate, departureDate, country, bookingDate);
             // On prend prend soin de ces arguments
             response.SetArguments(args);

             try
             {
                 // On passe les informations minimales
                 // qui permettent d'identifier de client
                 UserInfo user = new UserInfo(local, login, GetRemoteHost(),
                     UserInfo.APPLICATION_VPAYMENT_ID_GENERATION, UserInfo.ACTION_NONE);

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(user));

                 // On lance la validation des controles
                 // sur les arguments envoyés par le client
                 // On va vérifier les champs obligatoires
                 response.Validate();

                 if (!response.IsError())
                 {
                     // Aucune erreur sur les arguments
                     // On génère l'ID
                     response.SetValues(response.GetUser(), Services.GenerateVPaymentID(response));
                 }

             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.AddException(response.GetUser(), e);
             }
             return response.GetResponse();
         }


        /// <summary>
        /// Validation d'ID VPayment (AMEX)
        /// La validation consiste à vérifier si un ID
        /// passé en paramètre est valide
        /// </summary>
        /// <param name="local">Langue de réponse du service</param>
        /// <param name="login">Compte utilisateur</param>
        /// <param name="id">ID VPayment</param>
        /// <returns></returns>
         [WebMethod(Description = "Validate a VPayment ID")]
         public string ValidateVPaymentID(string local, string login, string id)
         {
             // On prépare la réponse que l'on va apportée
             VPaymentIDValidationResponse response = new VPaymentIDValidationResponse(id);

             try
             {
                 // On passe les informations minimales
                 // qui permettent d'identifier de client
                 UserInfo user = new UserInfo(local, login, GetRemoteHost(),
                     UserInfo.APPLICATION_VPAYMENT_ID_VALIDATION, UserInfo.ACTION_NONE);

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(user));

                 // On valide l'ID
                 response = Services.ValidateVPaymentID(response.GetUser(), id);   
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             return response.GetResponse();
         }


         /// <summary>
         /// Retourne le mode de paiement pour un voyageur dans Navision
         /// et cela sur tous les services
         /// </summary>
         /// <param name="local">Langue de réponse du service</param>
         /// <param name="pos">marché</param>
         /// <param name="login">Compte utilisateur</param>
         /// <param name="password">Mot de passe</param>
         /// <param name="comcode">Code client</param>
         /// <param name="costCenter">Cost center 1 id</param>
         /// <param name="percode">Code voyageur</param>
         /// <param name="servicesList">Liste de services (séparateur par ,) : Optionnel</param>
         /// <returns>Informations sur le mode de paiement</returns>
         [WebMethod(Description = "Returns payment means for a traveler, cost center or customer (services separated by , are optional).")]
         public string GetTravelerPaymentMeans(string local, string login, string password, string pos,
             string comcode, string costcenter, string percode, string servicesList)
         {
             // On prépare la réponse que l'on va
             TravelerPaymentMeansResponse response = new TravelerPaymentMeansResponse(pos, comcode, costcenter, percode, servicesList);

             try
             {
                 // On passe les informations minimales
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                        UserInfo.APPLICATION_PAYMENT_MEANS_LOOKUP, UserInfo.ACTION_NONE));

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // sanity check first
                 response.ValidateArguments();

                 //We are ready to get traveller payment means
                 NavServiceUtils.GetTravellerPaymentMeans(response);   
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // On retourne la réponse au client
             return response.GetResponse();
         }




         /// <summary>
         /// Retourne le mode de paiement pour un voyageur dans Navision
         /// et cela sur tous les services
         /// </summary>
         /// <param name="local">language</param>
         /// <param name="pos">point of sale</param>
         /// <param name="login">login</param>
         /// <param name="password">password</param>
         /// <param name="pos">point of sale</param>
         /// <param name="comcode">customer code</param>
         /// <param name="costCenter">analytical code 1 id</param>
         /// <param name="percode">traveller code</param>
         /// <param name="service">product</param>
         /// <param name="corporation">corporation</param>
         /// <param name="token">token</param>
         /// <returns>Form of payment</returns>
         [WebMethod(Description = "Returns form of payment for a specific booking.")]
         public string GetBookingFormOfPayment(string local, string login, string password, string pos,
             string comcode, string costcenter, string percode, string service, string corporation, string token)
         {
             // On prépare la réponse que l'on va
             BookingFormOfPaymentResponse response = new BookingFormOfPaymentResponse(pos, comcode, costcenter, percode, service, corporation, token);
 
             try
             {

                 // On passe les informations minimales
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                        UserInfo.APPLICATION_FORM_OF_PAYMENT_LOOKUP, UserInfo.ACTION_DECRYPT));

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // sanity check first
                 response.ValidateArguments();


                 // we will check in we have exception on customer
                 if (!String.IsNullOrEmpty(response.GetArgComcode()))
                 {
                     // is there any CASH exception on customer?
                     string paymentType = Services.GetGDSPaymentTypeForCustomer(response.GetUser(), response.GetArgPos(), response.GetArgComcode());

                     if (!String.IsNullOrEmpty(paymentType) && paymentType.Equals(Const.PaymentTypeStringCASH))
                     {
                         // this customer must pay by CASH
                         // credit card is not allowed
                         response.SetPaymentType(Const.PaymentTypeStringCASH_FR);

                         // We have ended here as we have payment type = CASH
                         return response.GetResponse();
                     }
                 }

                 // we will check in we have exception on corporation
                 if (!String.IsNullOrEmpty(response.GetArgCorporation()))
                 {
                     string paymentType = Services.GetGDSPaymentTypeForCorporation(response.GetUser(), response.GetArgPos(), response.GetArgCorporation());

                     if (!String.IsNullOrEmpty(paymentType) && paymentType.Equals(Const.PaymentTypeStringCASH))
                     {
                         // this corporarion must pay by CASH
                         // credit card is not allowed
                         response.SetPaymentType(Const.PaymentTypeStringCASH_FR);

                         // We have ended here as we have payment type = CASH
                         return response.GetResponse();
                     }
                 }



                 // no exception
                 // Let's check if we have a token
                 if (!String.IsNullOrEmpty(response.GetArgToken()))
                 {
                     // Le client nous a envoyé un token
                     // BO ou FO
                     switch (ArgsChecker.GuessTokenType(response.GetUser(), response.GetArgToken()))
                     {
                         //case ArgsChecker.TOKEN_BO:
                         //    // Il s'agit donc d'un token BackOffice
                         //    // Tout d'abord, on récupère le token
                         //    long TokenBO = Util.ConvertStringToToken(token);

                         //    // On a récupéré le token
                         //    // On va maintenant essayer de récupérer les
                         //    // informations complémentaires (date expiration, cvc, ...) depuis Navision
                         //    response.SetExtendedPanInfos(Services.GetExtendedPanInfos(response.GetUser(), response.GetArgPos(), TokenBO));
                         //    break;
                         default:
                             // Nous devons nous baser sur ce token pour
                             // effectuer la recherche
                             // Aucune connexion ne sera effectuer côté Navision
                             response.SetFOPanInfos(Services.GetPanInfosFromFOToken(response.GetUser(), token, false));
                             break;
                     }
                     // We have with the supplied token
                     // we finish here and return payment type = CREDIT CARD
                     return response.GetResponse();
                 }

                 
                 // we don't have token an exception on customer and corporation
                 // We are ready to get traveller payment means
                 response.BuildTravelerPaymentMeansResponse();
                 NavServiceUtils.GetTravellerPaymentMeans(response.GetTravelerPaymentMeansResponse());
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // We have done
             // let's reply to the caller
             return response.GetResponse();
         }



         /// <summary>
         /// Retourne la version du service web
         /// </summary>
         /// <returns>Version du service</returns>
         [WebMethod(Description = "Returns webservice version.")]
         public string GetVersion()
         {
             return Const.GetVersion();
         }



         /// <summary>
         /// Insertion d'une carte bancaire corporate ou privée dans Navision
         /// Le traitement se fait en deux temps :
         /// 1 - Insertion du numéro de carte dans la base des cartes encryptées (Tokenisation)
         /// 2 - Insertion de la carte dans Navision
         /// La carte est créée depuis un token (BO ou FO)
         /// </summary>
         /// <param name="local">Langue de retour</param>
         /// <param name="login">Compte utilisateur</param>
         /// <param name="password">Password</param>
         /// <param name="pos">Pos Navision</param>
         /// <param name="customer">Compte client (comcode)</param>
         /// <param name="cc1Id">Centre de cout 1 (cc1) Id</param>
         /// <param name="traveller">Compte utilisateur (percode)</param>
         /// <param name="expirationDate">Date d'expiration de la carte</param>
         /// <param name="service">Service Group (AIR, RAIL, ...)</param>
         /// <param name="token">Token (BO ou FO)</param>
         /// <param name="description">Description</param>
         /// <param name="holderName">holderName</param>
         /// <returns>Statut traitement (Token et référence carte, ...)</returns>
         [WebMethod(Description = "Insert corporate or private card in encrypted and Navision databases and reference card.")]
         public string InsertProfilCardFromToken(string local, string login, string password, string pos, string customer, string cc1Id
             , string traveller, string expirationDate, string service, string token, string description, string holderName, int lodgedCard)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             InsertCardResponse response = new InsertCardResponse(token, true);

             try
             {
                 // On passe les informations minimales
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                    UserInfo.APPLICATION_CARD_INSERT_FROM_TOKEN, UserInfo.ACTION_DECRYPT));

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));


                 // Au final, on doit retouner un PAN
                 string pan = null;

                 // Le client nous a envoyé un token BO, FO ou une carte de credit
                 switch (ArgsChecker.GuessTokenType(response.GetUser(), token))
                 {
                     case ArgsChecker.TOKEN_BO:
                         // Il s'agit donc d'un token BackOffice
                         // Tout d'abord, on récupère le token
                         long TokenBO = Util.ConvertStringToToken(token);

                         // On a récupéré le token
                         // On va maintenant récupérer le PAN depuis le token
                         pan = Services.GetPanFromBOToken(response.GetUser(), TokenBO);
                         break;
                     case ArgsChecker.TOKEN_FO:
                         // Il s'agit d'un token Front Office
                         // On va récupérer le PAN
                         pan = Services.GetPanInfosFromFOToken(response.GetUser(), token, false).GetPAN();
                         break;
                     default:
                         // Ce n'est pas un token
                         // On va arrêter !!!!
                        throw new Exception(response.GetUser().GetMessages().GetString("UnknownToken",token, true));
                 }

   
                 // L'utilisateur a les droits suffisants pour la suite
                 // Sanity check for cost center, not mandatory
                 ArgsChecker.ValidateCostCenterId(response.GetUser(), cc1Id, false);

                 // First, we need to return the Cost center label from cost center id
                 String cc1 = Services.GetCostCenter1ValueFromId(response.GetUser(), pos, customer, cc1Id);
                 
                 // On va insérer la carte dans la base des données encryptées
                 // et dans Navision puis que l'on a un PAN
                 // force warning as this method do not support force warning
                 InsertCardResult res = Services.InsertProfilCard(response.GetUser(), pos, customer, cc1,
                      traveller, expirationDate, service, pan, description, holderName, 0, string.Empty, 1);                    

                 // La carte a été correctement insérée dans la base des
                 // données encryptées et Navision
                 // On va récupérer les informations utiles (Token Référence carte)
                 response.SetValues(response.GetUser(), res);
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // Impossible donc d'insérer cette carte
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // On retourne la réponse au client
             return response.GetResponse();
         }

         /// <summary>
         /// Supression d'une carte dans Navision
         /// Cette méthode supprime un enregistrement unique
         /// (customer, cc1, traveler, service)
         /// </summary>
         /// <param name="local">Langue de réponse du service</param>
         /// <param name="login">Compte utilisateur</param>
         /// <param name="pos">Marché</param>
         /// <param name="comcode">customer</param>
         /// <param name="cc1Id">Analytical code 1 id</param>
         /// <param name="percode">traveler</param>
         /// <param name="service">service (AIR/RAIL/HOTEL/LOW COST/CAR/SEA)</param>
         /// <returns></returns>
         [WebMethod(Description = "Delete a credit card.")]
         public string DeleteProfilCard(string local, string login, string password, string pos, string comcode, string cc1Id, string percode, string service)
         {
             // On prépare la réponse que l'on va apportée
             DeleteProfilCardResponse response = new DeleteProfilCardResponse(pos, comcode, cc1Id, percode, service);

             try
             {
                 // On passe les informations minimales
                 // qui permettent d'identifier de client
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                    UserInfo.APPLICATION_CARD_DELETION, UserInfo.ACTION_NONE));

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // On supprimer la carte
                 Services.DeleteProfilCard(response.GetUser(), response.GetPOS(), comcode, cc1Id, percode, service);

                 // La carte a été supprimée
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             return response.GetResponse();
         }


         /// <summary>
         /// Encryption des cartes Egencia dans la base des cartes
         /// cryptées hébergée côte FO
         /// Cette méthode crypte le PAN et le CSC
         /// </summary>
         /// <param name="local">Langue de l'utilisateur</param>
         /// <param name="login">Compte utilisateur</param>
         /// <param name="password">Mot de passe</param>
         /// <param name="pan">Numéro de carte</param>
         /// <param name="expirationDate">Date d'expiration</param>
         /// <param name="csc">Code de sécurité carte</param>
         /// <param name="cardHolder">Détenteur de la carte</param>
         /// <param name="pos">Marché</param>
         /// <returns>Informations d'insertion</returns>
         [WebMethod(Description = "Inserts Egencia card from Front Office in encrypted database and return token.")]
         public string TokenizeEgenciaCard(string local, string login, string password, string pan,  string expirationDate
             , string csc, string cardHolder, string pos, int lodgedCard)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             InsertEgenciaCardResponse response = new InsertEgenciaCardResponse(pan);

             try
             {
                 // On passe les informations minimales
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                     UserInfo.APPLICATION_FO_TOKENIZATION, UserInfo.ACTION_ENCRYPT));

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // L'utilisateur a les droits suffisants pour la suite
                 response.SetValues(response.GetUser(), Services.InsertEgenciaCardInEncryptedFODB(response.GetUser(), pan, csc
                     , expirationDate, cardHolder, pos));
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // On retourne la réponse au client
             return response.GetResponse();
         }


         /// <summary>
         /// Récupération du PAN et du CSC
         /// La méthode attend le token et
         /// récupére le cyptogramme du PAN et du CSC
         /// décrypte à la volée
         /// et retourne les données en clair
         /// </summary>
         /// <param name="local">Langue</param>
         /// <param name="login">Compte utilisateur</param>
         /// <param name="password">Mot de passe</param>
         /// <param name="token">Token</param>
         /// <returns>Informations carte</returns>
         [WebMethod(Description = "Returns PAN and CSC for Egencia card (Lookup).")]
         public string UntokenizeEgenciaCard(string local, string login, string password, string token)
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             EgenciaCardResponse response = new EgenciaCardResponse(token);

             try
             {
                 // On passe les informations minimales
                response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                     UserInfo.APPLICATION_FO_UNTOKENIZATION, UserInfo.ACTION_ENCRYPT));

                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 if (String.IsNullOrEmpty(token))
                 {
                     // Le token ne peut être vide!
                     throw new Exception(response.GetUser().GetMessages().GetString("EmptyToken", true));
                 }

                 // Il s'agit d'un token Egencia
                 // On récupère les informations sur la carte
                 // numéro de carte, csc
                 response.SetValues(response.GetUser(), Services.GetPanInfosFromEgenciaToken(response.GetUser(), token));
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }

             // On retourne la réponse au client
             return response.GetResponse();
         }

         /// <summary>
         /// Retourne le type de paiement autorisé par Amadeus (GDS)
         /// pour une companie aérienne
         /// </summary>
         /// <param name="local">Language</param>
         /// <param name="login">Compte utilisateur</param>
         /// <param name="pos">Point de vente</param>
         /// <param name="corporationCode">Code compagnie aérienne</param>
         /// <returns>Informations sur le mode de paiement</returns>
         [WebMethod(Description = "Returns payment type requested by Amadeus for a corporation.")]
         //>> EGE-57710 : request password for method GetGDSPaymentTypeForCorporation
         //public string GetGDSPaymentTypeForCorporation(string local, string login, string pos, string corporationCode)
         public string GetGDSPaymentTypeForCorporation(string local, string login, string password, string pos, string corporationCode)
         //<< EGE-57710 : request password for method GetGDSPaymentTypeForCorporation
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             GDSCorporationPaymentTypeResponse response = new GDSCorporationPaymentTypeResponse(pos, corporationCode);

             try
             {
                 // On passe les informations minimales
                 //>> EGE-57710 : request password for method GetGDSPaymentTypeForCorporation
                 //response.SetUser(new UserInfo(local, login, GetRemoteHost(),
                 //     UserInfo.APPLICATION_MID_OFFICE_ROBOTIC_TOOL, UserInfo.ACTION_NONE));
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                     UserInfo.APPLICATION_MID_OFFICE_ROBOTIC_TOOL, UserInfo.ACTION_NONE));
                 //<< EGE-57710 : request password for method GetGDSPaymentTypeForCorporation


                 // L'utilisateur a les droits suffisants pour la suite
                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                // On a tout ce qui est nécéssaire
                // On peut répondre au client
                 response.SetValue(Services.GetGDSPaymentTypeForCorporation(response.GetUser(), response.GetPOS()
                    , corporationCode));
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // On retourne la réponse au client
             return response.GetResponse();
         }




         /// <summary>
         /// Retourne le type de paiement autorisé
         /// pour une client
         /// </summary>
         /// <param name="local">Language</param>
         /// <param name="login">Compte utilisateur</param>
         /// <param name="password">Password</param>
         /// <param name="pos">Point de vente</param>
         /// <param name="customerCode">Code client</param>
         /// <returns>Informations sur le mode de paiement</returns>
         [WebMethod(Description = "Returns payment type required by Amadeus for a customer.")]
         //>> EGE-57711 : CCE - request password for method GetGDSPaymentTypeForCustomer
         //public string GetGDSPaymentTypeForCustomer(string local, string login, string pos, string customerCode)
         public string GetGDSPaymentTypeForCustomer(string local, string login, string password, string pos, string customerCode)
         //<< EGE-57711 : CCE - request password for method GetGDSPaymentTypeForCustomer
         {
             // On prépare la réponse que l'on va
             // apporter (initialisation)
             GDSCustomerPaymentTypeResponse response = new GDSCustomerPaymentTypeResponse(pos, customerCode);

             try
             {
                 // On passe les informations minimales
                 //>> EGE-57711 : CCE - request password for method GetGDSPaymentTypeForCustomer
                 //response.SetUser(new UserInfo(local, login, GetRemoteHost(),
                 //    UserInfo.APPLICATION_MID_OFFICE_ROBOTIC_TOOL, UserInfo.ACTION_NONE));
                 response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                        UserInfo.APPLICATION_MID_OFFICE_ROBOTIC_TOOL, UserInfo.ACTION_NONE));
                 //<< EGE-57711 : CCE - request password for method GetGDSPaymentTypeForCustomer

                 // L'utilisateur a les droits suffisants pour la suite
                 // On récupère les informations sur l'utilisateur
                 // Les droits d'accès à cette méthode
                 // seront également vérifiés depuis l'AD
                 response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                 // On a tout ce qui est nécéssaire
                 // On peut répondre au client
                 response.SetValue(Services.GetGDSPaymentTypeForCustomer(response.GetUser(), response.GetPOS()
                    , customerCode));
             }
             catch (Exception e)
             {
                 // Une exception a été levée 
                 // On retourne l'erreur au client
                 response.SetException(response.GetUser(), e);
             }
             // On retourne la réponse au client
             return response.GetResponse();
         }



        /// <summary>
        /// Returns a new ENett VAN
        /// </summary>
        /// <param name="local">language</param>
        /// <param name="login">login</param>
        /// <param name="password">password</param>
        /// <param name="RequestDetail">requestDetail (from caller)</param>
        /// <returns>VAN detail</returns>
        [WebMethod(Description = "Get new Virtual Account Number from ENett.")]
        public string GetENettVAN(string local, string login, string password, string RequestXMLDetail)
        {
            // Prepare response
            ENettRequestVANResponse response = new ENettRequestVANResponse(RequestXMLDetail);

            try
            {
                // Send minimal information related to user
                response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                  UserInfo.APPLICATION_VNETT_VAN_RETRIEVAL, UserInfo.ACTION_DECRYPT));

                // Let's now check user access rights
                response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                // User has the ability to use this method
                // Read and validate input
                ENettRequestVAN reader = EnettUtils.ReadRequest(RequestXMLDetail);

                // User has the ability to use this method
                // We will send a request to ENett service and return a VAN
                response.SetValues(response.GetUser(), Services.GetNewENettVAN(response.GetUser(), reader), reader);
            }
            catch (Exception e)
            {
                // An exception was raised
                // call should ne notified
                response.SetException(response.GetUser(), e);
            }
            // Let's now reply to caller
            return response.GetResponse();
        }

        // Begin EGE-85532
        /// <summary>
        /// Return CANCEL ENett VAN details
        /// </summary>
        /// <param name="local">Language</param>
        /// <param name="login">user</param>
        /// <param name="password">password</param>
        /// <param name="RequestXMLDetail">XML request
        ///      <CancelRequestVAN>
        ///      <ECN>223227</ECN>
        ///      <PaymentID>0</PaymentID>
        ///      <UserName>Website</UserName>
        ///      <CancelReason>Booking cancelled</CancelReason>
        ///      </CancelRequestVAN>
        /// </param>
        /// <returns></returns>
        [WebMethod(Description = "Cancel VAN Details")]
        public string CancelVAN(string local, string login, string password, string RequestXMLDetail)
        {
            // Prepare response
            ENettCancelVANResponse response = new ENettCancelVANResponse(RequestXMLDetail);

            try
            {
                // Send minimal information related to user
                response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                  UserInfo.APPLICATION_VNETT_VAN_CANCEL, UserInfo.ACTION_NONE));

                // Let's now check user access rights
                response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                // User has the ability to use this method
                // Read and validate input
                ENettCancelRequestVAN reader = EnettUtils.ReadCancelVANDetails(RequestXMLDetail);

                // User has the ability to use this method
                // We will send a request to ENett service and return a VAN
                response.SetValues(response.GetUser(), Services.CancelVAN(response.GetUser(), reader), reader);
            }
            catch (Exception e)
            {
                // An exception was raised
                // call should ne notified
                response.SetException(response.GetUser(), e);
            }
            // Let's now reply to caller
            return response.GetResponse(); ;
        }

        // End EGE-85532


        /// <summary>
        /// Return ENett VAN details
        /// </summary>
        /// <param name="local">Language</param>
        /// <param name="login">user</param>
        /// <param name="password">password</param>
        /// <param name="RequestXMLDetail">XML request
        ///  <GetVAN> 
        //    <PaymentID>3B077D9CA5A9072</PaymentID>
        //   </GetVAN>
        /// </param>
        /// <returns></returns>
        [WebMethod(Description = "Return Get VAN Details")]
        public string GetENettVANDetails(string local, string login, string password, string RequestXMLDetail)
        {
            // Prepare response
            ENettGetVANDetailsResponse response = new ENettGetVANDetailsResponse(RequestXMLDetail);

            try
            {
                // Send minimal information related to user
                response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                  UserInfo.APPLICATION_VNETT_VAN_DETAILS_LOOKUP, UserInfo.ACTION_NONE));

                // Let's now check user access rights
                response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                // User has the ability to use this method
                // Read and validate input
                ENettGetVANDetails reader = EnettUtils.ReadGetVANDetails(RequestXMLDetail);

                // User has the ability to use this method
                // We will send a request to ENett service and return a VAN
                response.SetValues(response.GetUser(), Services.GetENettVANDetails(response.GetUser(), reader), reader);
            }
            catch (Exception e)
            {
                // An exception was raised
                // call should ne notified
                response.SetException(response.GetUser(), e);
            }
            // Let's now reply to caller
            return response.GetResponse(); ;
        }

        /// <summary>
        /// Amend ENett VAN
        /// </summary>
        /// <param name="local">language</param>
        /// <param name="login">login</param>
        /// <param name="password">password</param>
        /// <param name="RequestDetail">requestDetail (from caller)</param>
        /// <returns>VAN amended</returns>
        [WebMethod(Description = "Amend ENett VAN.")]
        public string AmendENettVAN(string local, string login, string password, string RequestXMLDetail)
        {
            // Prepare response
            ENettAmendVANResponse response = new ENettAmendVANResponse(RequestXMLDetail);

            try
            {
                // Send minimal information related to user
                response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                    UserInfo.APPLICATION_VNETT_VAN_AMEND, UserInfo.ACTION_NONE));

                // Let's now check user access rights
                response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                // User has the ability to use this method
                // Read and validate input
                ENettAmendVAN reader = EnettUtils.ReadAmend(RequestXMLDetail);

                // User has the ability to use this method
                // We will send a request to ENett service and return a VAN
                response.SetValues(response.GetUser(), Services.AmendENettVAN(response.GetUser(), reader), reader);
            }
            catch (Exception e)
            {
                // An exception was raised
                // call should ne notified
                response.SetException(response.GetUser(), e);
            }
            // Let's now reply to caller
            return response.GetResponse();
        }




        /// <summary>
        /// Returns lodged card references for a customer
        /// </summary>
        /// <param name="local">language</param>
        /// <param name="login">login</param>
        /// <param name="password">password</param>
        /// <param name="pos">market</pos>
        /// <param name="customerCode">Customer code</customerCode>
        /// <param name="travelerCode">traveler code</travelerCode>
        /// <param name="provider">payment provider</provider>
        /// <returns>Lodged card references</returns>
        [WebMethod(Description = "Returns lodged Card references for a provider.")]
        public string GetLodgedCardReferences(string local, string login, string password
            , string pos, string customerCode, string travelerCode, string provider)
        {
            // First read arguments
            ArgsLodgedCardReferences requestorDetail = new ArgsLodgedCardReferences(pos, customerCode, travelerCode, provider);

            // Prepare response
            LodgedCardReferencesResponse response = new LodgedCardReferencesResponse(requestorDetail);

            try
            {
                // Send minimal information related to user
                response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                    UserInfo.APPLICATION_LODGED_CARD_REFERENCES, UserInfo.ACTION_NONE));


                // Validate arguments
                requestorDetail.Validate(response.GetUser());

                // Let's now check user access rights
                response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                // Connect to Navision and returns lodged card references for the customer
                response.SetValues(Services.GetLodgedCardReferences(response.GetUser(), requestorDetail));
            }
            catch (Exception e)
            {
                // An exception was raised
                // call should ne notified
                response.SetException(response.GetUser(), e);
            }
            // Let's now reply to caller
            return response.GetResponse();
        }



        //>>EGE-66904
        /// <summary>
        /// Validate token
        /// </summary>
        /// <param name="local">language</param>
        /// <param name="login">Account</param>
        /// <param name="password">Password</param>
        /// <param name="token">Token to validate</param>
        /// <returns>true or false</returns>
        [WebMethod(Description = "Validate a token.")]
        public string ValidateToken(string local, string login, string password, string token)
        {
            // Prepare response
            TokenValidationResponse response = new TokenValidationResponse(token);

            try
            {
                // Send minimal infos on user
                response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                     UserInfo.APPLICATION_TOKEN_VALIDATION, UserInfo.ACTION_NONE));

                // Check user right
                response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                // let's check token now
                ArgsChecker.ValidateBOToken(response.GetUser(), token);

                // Token is valid
                response.SetValue(true);

            }
            catch (Exception e)
            {
                // We have exception
                response.SetException(response.GetUser(), e);
            }
            return response.GetResponse();
        }
        //<<EGE-66904




        /// <summary>
        /// Encrypt a string with Crypto server
        /// </summary>
        /// <param name="local">local (default english)</param>
        /// <param name="login">application account name</param>
        /// <param name="password">application account password</param>
        /// <param name="valueToEncrypt">Value string to encrypt</param>
        /// <returns>encypted value</returns>
        [WebMethod(Description = "Encrypt a value with crypto server and return the encrypted value.")]
        public string EncryptString(string local, string login, string password, string valueToEncrypt)
        {
            // Prepare response
            SimpleValueResponse response = new SimpleValueResponse(valueToEncrypt);

            try
            {
                // Send minimal infos on user
                response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                    UserInfo.APPLICATION_ENCRYPT_STRING, UserInfo.ACTION_NONE));


                // Check user right
                response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));


                // User is valid
                response.SetValue(Services.EncryptBOCard(response.GetUser(), valueToEncrypt));
            }
            catch (Exception e)
            {
                // We have exception
                response.SetException(response.GetUser(), e);
            }
            return response.GetResponse();
        }


        /// <summary>
        /// Decrypt a string with Crypto server
        /// </summary>
        /// <param name="local">local (default english)</param>
        /// <param name="login">application account name</param>
        /// <param name="password">application account password</param>
        /// <param name="valueToEncrypt">Value string to decrypt</param>
        /// <returns>decrypted value</returns>
        [WebMethod(Description = "Decrypt a value with crypto server and return the plaint text value.")]
        public string DecryptString(string local, string login, string password, string valueToDecrypt)
        {
            // Prepare response
            SimpleValueResponse response = new SimpleValueResponse(valueToDecrypt);

            try
            {
                // Send minimal infos on user
                response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                    UserInfo.APPLICATION_DECRYPT_STRING, UserInfo.ACTION_NONE));


                // Check user right
                response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                // User is valid
                response.SetValue(Services.DecryptBOCard(response.GetUser(), valueToDecrypt));
            }
            catch (Exception e)
            {
                // We have exception
                response.SetException(response.GetUser(), e);
            }
            return response.GetResponse();
        }


         /// <summary>
        /// Save temporary FO cards to definitive table
        /// </summary>
        /// <param name="local">local</param>
        /// <param name="login">login</param>
        /// <param name="password">password</param>
        /// <returns>Returns number of cards saved</returns>
        [WebMethod(Description = "Save temporary FO cards in encrypted database.")]
        public string SaveTemporaryFOCards(string local, string login, string password)
        {
            // Prepare response
            SimpleValueResponse response = new SimpleValueResponse();

            try
            {
                // Send minimal infos on user
                response.SetUser(new UserInfo(local, login, password, true, GetRemoteHost(),
                    UserInfo.APPLICATION_DECRYPT_STRING, UserInfo.ACTION_NONE));


                // Check user right
                response.SetUser(ArgsChecker.CheckLogin(response.GetUser()));

                // User is valid
                // let's move remaining FO card to BO table
                response.SetValue(Services.GetRemainingFOCards(response.GetUser()).ToString());
            }
            catch (Exception e)
            {
                // We have exception
                response.SetException(response.GetUser(), e);
            }
            return response.GetResponse();
        }

         /// <summary>
         /// Cette fonction permet de récupérer
         /// l'adresse IP et le nom du client (visiteur)
         /// </summary>
         /// <returns>RemoteHost</returns>
         public RemoteHost GetRemoteHost()
         {
             HttpRequest request = Context.Request;
             string remoteIP = request.ServerVariables["REMOTE_ADDR"];
             if (!String.IsNullOrEmpty(request.ServerVariables["HTTP_X_FORWARDED_FOR"]))
             {
                 remoteIP = request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split(',')[0].Trim();
             }
             return new RemoteHost(remoteIP,
                 request.ServerVariables["REMOTE_HOST"],
                 request.ServerVariables["HTTPS"]);
         }




        
            
         /// <summary>
         /// Enrypte un mot de passe
         /// AES (clé de 256 bits)
         /// </summary>
         /// <param name="plainPassword">Mot de passe</param>
         /// <returns>Mot de passe enrypté</returns>
        [WebMethod(Description = "Encrypt a string")]
         public string EncryptPassword(string plainPassword)
         {
             return EncDec.EncryptPassword(plainPassword);
         }
        
         /// <summary>
         /// Décrypte un mot de passe
         /// AES (clé de 256 bits)
         /// </summary>
         /// <param name="cryptedpass">Mot de passe encrypté</param>
         /// <returns>Mot de passe décrypté</returns>
        [WebMethod(Description = "Decrypt a string")]
        public string DecryptPassword(string cryptedpass)
        {
            return EncDec.DecryptPassword(cryptedpass);
        }
        
        
        /// <summary>
        /// TEST !  TEST ! TEST
        /// Cette méthode permet de générer des numéros de cartes
        /// valides aléatoires
        /// ATTENTION, il s'agit uniquement de la validité de la structure
        /// numéro de carte (contrôle de Luhn)
        /// </summary>
        /// <param name="CardType">Type de carte (Amex, Visa, ...)</param>
        /// <param name="Size">Taille du numéro</param>
        /// <param name="HowMany">Le nombre de numéros à retourner</param>
        /// <returns>Tableaux de numéros de carte</returns>
       [WebMethod(Description = "Returns generated credit card numbers. FOR TEST ONLY!")]
        public string GenerateCreditCardNumbers(string CardType,int Size, int HowMany)
        {
             // Préparation de la réponse à apporter
             CreditCardGeneratedResponse response = new CreditCardGeneratedResponse();
             string cardtype = CardType.ToUpper();
             try
             {
                 // On génère les numéros de carte
                 string[] Cards = RandomCreditCardNumberGenerator.GenerateCreditCardNumbers(cardtype, Size, HowMany);
                 // On récupère les valeurs
                 response.SetValues(cardtype, Size, HowMany, Cards); 
             }
             catch (Exception e)
             {
                 // On retourne cette information
                 // au client
                 response.SetException(e);
             }
             // On peut répondre au client
             return response.GetResponse();
        }
        

    }
    
}