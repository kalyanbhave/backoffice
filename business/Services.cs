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
using System.IO;
using System.Configuration;
using System.Collections;
using System.Net;
using SafeNetWS.utils;
using SafeNetWS.creditcard;
using SafeNetWS.database;
using SafeNetWS.database.row;
using SafeNetWS.database.row.value;
using SafeNetWS.database.result;
using SafeNetWS.business.arguments.quality;
using SafeNetWS.business.response.writer;
using SafeNetWS.log;
using SafeNetWS.login;
using SafeNetWS.utils.crypting;
using SafeNetWS.utils.crypting.safenet;
using SafeNetWS.business.arguments.reader;
using SafeNetWS.creditcard.creditcardvalidator.bibit;
using SafeNetWS.creditcard.creditcardvalidator;
using SafeNetWS.www;
using SafeNetWS.exception;
using SafeNetWS.utils.cache;
using SafeNetWS.ENettService;
using SafeNetWS.creditcard.virtualcard.enett;
using System.Collections.Generic;
using SafeNetWS.business;





namespace SafeNetWS.business
{

    /// <summary>
    /// Cette classe contient les methodes permettant
    /// d'insérer des cartes bancaires dans la base
    /// mais également la consultation de numéro de cartes
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class Services
    {
        /// <summary>
        /// Récupération du token à partir du PAN
        /// depuis la base de données des numéros de cartes encryptés
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pan">Numéro de la carte en clair</param>
        /// <returns>Token</returns>
        public static long GetTokenFromPan(UserInfo user, string pan)
        {
            long token = Const.EmptyBoToken;

            //On vérifie le numéro de carte
            // en passant un contrôle de Luhn
            CardInfos rt = CheckCreditCard(user, pan);

            // La carte est valide
            // On encrypte le numéro de carte
            // si nécéssaire, car si il n'y a pas eu
            // de validation BIBIT en ligne
            string encryptedPAN = rt.GetEncryptedPan();

            if (encryptedPAN == null)
            {
                // Nous devons encrypter ce PAN
                encryptedPAN = EncryptBOCard(user, pan);
            }

            EncryptedDataConnection conn = null;

            try
            {
                // On définit une nouvelle connexion
                conn = new EncryptedDataConnection(user);

                // On se connecte
                conn.Open();

                // on retourne le PAN à partir du Token
                token = conn.GetToken(encryptedPAN);

                if (Util.IsEmptyToken(token))
                {
                    throw new Exception(user.GetMessages().GetString("Services.GetTokenFromPan.TokenNotFound", true));
                }
            }
            catch (Exception e)
            {
                // Gérer cette exception
                throw new Exception(e.Message);
            }
            finally
            {
                // On se deconnecte
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette erreur
                }
            }

            return token;
        }


        /// <summary>
        /// Retourne le numéro de carte en clair à partir du Token BO
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="token">Token</param>
        /// <returns>Numéro de carte</returns>
        public static string GetPanFromBOToken(UserInfo user, long token)
        {
            string pan = null;
            try
            {
                // on retourne le cryptogramme du PAN à partir du Token
                string EncryptedPAN = GetEncryptedPanFromBOToken(user, token);

                // Ok on a le PAN encrypté
                // On doit maintenant le décrypter en sollicitant le module SafeNet
                pan = DecryptBOCard(user, EncryptedPAN);
            }
            catch (Exception e)
            {
                // Gérer cette exception
                throw new Exception(e.Message);
            }

            return pan;
        }

        /// <summary>
        /// Effectue une rotation de clé pour les Tokens
        /// 1-Tous les cryptogrammes correspondant aux Tokens
        ///     sont extraits de la base des données encryptées
        /// 2-Les cryptogrammes sont décryptés avec l'ancienne version 
        ///     de la clé afin de retrouver le PAN en clair
        /// 3- Enfin, le PAN est re encrypté avec la nouvelle version de la clé
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <returns>Infos sur le nombre de token traités, succès et en erreur</returns>
        public static KeyRotationResult RotateKeyForAllTokens(UserInfo user)
        {
            int TokensCount = 0;
            int TokensCountSuccess = 0;
            int RemainingFOCards = 0;
            int TokensCountError = 0;
            int ClearedBOBibitCacheEntries = 0;
            int ClearedFOBibitCacheEntries = 0;

            KeyRotationResult retval = new KeyRotationResult();
            EncryptedDataConnection conn = null;
            SafeNetSession sf = null;

            try
            {
                // Nettoyage des caches Bibit (BackOffice et FrontOffice)
                ClearedBOBibitCacheEntries = ClearBOBibitCache(user);
                ClearedFOBibitCacheEntries = ClearFOBibitCache(user);

                // Avant de commencer la mise à jour des cryptogrammes
                // nous devons vérifier qu'il ne reste plus de cartes
                // dans la base FrontOffice
                // Les éventuelles cartes restantes vont être récupérées
                // et insérées dans la base des cartes encryptées et la table des
                // mapping sera mise à jour
                RemainingFOCards = GetRemainingFOCards(user);

                // Avant d'aller plus loin, on va vider les connexions
                // presentes dans le cache
                NetCache.ClearCache();

                // On définit une nouvelle connexion
                // vers la base des données encryptées
                conn = new EncryptedDataConnection(user);

                // On se connecte à la base de données
                conn.Open();

                // on retourne la liste de tous les tokens
                // dont le cyptogramme doit être
                // mis à jour après une rotation de clé
                EncryptedData Tokens = conn.GetAllTokens();
                TokensCount = Tokens.GetSize();

                // Ouverture d'une session SafeNet
                // afin d'effectuer le décryptage et le ré encryptage
                sf = GetStandardBOSafeNetSession(user);

                if (TokensCount > 0)
                {
                    // On va parcourir la liste des tokens
                    // afin de mettre à jour le cryptogramme
                    IDictionaryEnumerator tokenslist = Tokens.GetTokens().GetEnumerator();
                    while (tokenslist.MoveNext())
                    {
                        // On a le Token à mettre à jour
                        long Token = Util.ConvertStringToToken(tokenslist.Key.ToString());
                        // On extrait le cryptoramme (crypté avec l'ancienne version de la clé)
                        string EncryptedPAN = tokenslist.Value.ToString();

                        try
                        {
                            // Décryptage du cryptogramme
                            // avec la version de la clé
                            // qui a permis le cryptage
                            string DecryptedData = sf.Decrypt(EncryptedPAN);

                            // Encryptage avec la version courante
                            // de la clé (nouvelle version)
                            string ReEncryptedData = sf.Encrypt(DecryptedData);

                            // On met à jour la base des données encryptées
                            // avec le nouveau cryptogramme
                            conn.UpdateEncryptedCard(Token, ReEncryptedData);

                            // Ce token a été correctement traité
                            TokensCountSuccess++;
                        }
                        catch (Exception e)
                        {
                            // Erreur lors de la mise à jour pour un token particulier
                            // On trace l'erreur et on continue ...
                            TokensCountError++;
                            Logger.WriteErrorToLog(user.GetMessages().GetString("KeyRotation.ErrorUpdatingToken",
                                Token, EncryptedPAN, e, true));
                        }
                    }
                    // Si tous les Tokens ont été mis à jour
                    // On remet tous les indicateurs à leur valeur initiale
                    if (TokensCountError == 0)
                    {
                        conn.SetRotationEnded();
                    }
                }

                // On va traiter les cartes Egencia
                // et mettre à jour le cryptogramme et le csc
                KeyRotationResult ec = RotateKeyForAllEgenciaTokens(user, sf);
                // on récupère les indicateurs de traitement
                retval.SetEgenciaCardsValues(ec.GetCount(), ec.GetSuccessCount(), ec.GetErrorCount());
            }
            catch (Exception e)
            {
                // Gérer cette exception
                throw new Exception(e.Message);
            }
            finally
            {
                // On se deconnecte
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette erreur
                }
                if (sf != null) sf.CloseSession();
            }
            // On prépare les indicateurs à retourner
            retval.SetValues(TokensCount, TokensCountSuccess, TokensCountError, RemainingFOCards
                , ClearedBOBibitCacheEntries, ClearedFOBibitCacheEntries);

            return retval;
        }


        /// <summary>
        /// Mise à jour des cryptogrammes pour le PAN et le CSC des cartes egencia
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="sf">Session ingrian</param>
        /// <returns>Résultat du traitement</returns>
        public static KeyRotationResult RotateKeyForAllEgenciaTokens(UserInfo user, SafeNetSession sf)
        {
            int TokensCount = 0;
            int TokensCountSuccess = 0;
            int TokensCountError = 0;


            KeyRotationResult retval = new KeyRotationResult();
            EncryptedFODataConnection conn = null;

            try
            {
                // On définit une nouvelle connexion
                // vers la base des données encryptées
                conn = new EncryptedFODataConnection(user);

                // On se connecte à la base de données
                conn.Open();

                // on retourne la liste de tous les tokens
                // dont le cyptogramme doit être
                // mis à jour après une rotation de clé
                EncryptedEgenciaData Tokens = conn.GetAllEgenciaTokens();
                TokensCount = Tokens.GetSize();

                if (TokensCount > 0)
                {
                    // On va parcourir la liste des tokens
                    // afin de mettre à jour le cryptogramme
                    IDictionaryEnumerator tokenslist = Tokens.GetTokens().GetEnumerator();
                    while (tokenslist.MoveNext())
                    {
                        // On a le Token à mettre à jour
                        string Token = tokenslist.Key.ToString();
                        // On extrait le cryptoramme PAN et CSC (crypté avec l'ancienne version de la clé)
                        EncryptedEgenciaValue value = (EncryptedEgenciaValue)tokenslist.Value;
                        //... PAN
                        string EncryptedPAN = value.GetEncryptedPAN();
                        //.. et CSC
                        string EncryptedCSC = value.GetEncryptedCSC();


                        try
                        {
                            // Décryptage du cryptogramme
                            // avec la version de la clé
                            // qui a permis le cryptage
                            string DecryptedPAN = sf.Decrypt(EncryptedPAN);
                            // et du CSC
                            string DecryptedCSC = sf.Decrypt(EncryptedCSC);

                            // Encryptage avec la version courante
                            // de la clé (nouvelle version)
                            string ReEncryptedPAN = sf.Encrypt(DecryptedPAN);
                            // et du CSC
                            string ReEncryptedCSC = sf.Encrypt(DecryptedCSC);

                            // On met à jour la base des données encryptées
                            // avec le nouveau cryptogramme
                            conn.UpdateEncryptedEgenciaCard(Token, ReEncryptedPAN, ReEncryptedCSC);

                            // Ce token a été correctement traité
                            TokensCountSuccess++;
                        }
                        catch (Exception e)
                        {
                            // Erreur lors de la mise à jour pour un token particulier
                            // On trace l'erreur et on continue ...
                            TokensCountError++;
                            Logger.WriteErrorToLog(user.GetMessages().GetString("KeyRotation.ErrorUpdatingToken",
                                Token, EncryptedPAN, e, true));
                        }
                    }
                    // Si tous les Tokens ont été mis à jour
                    // On remet tous les indicateurs à leur valeur initiale
                    if (TokensCountError == 0)
                    {
                        conn.SetRotationEndedForEgenciaCard();
                    }
                }
            }
            catch (Exception e)
            {
                // Gérer cette exception
                throw new Exception(e.Message);
            }
            finally
            {
                // On se deconnecte
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette erreur
                }
            }
            // On prépare les indicateurs à retourner
            retval.SetValues(TokensCount, TokensCountSuccess, TokensCountError);

            return retval;
        }


        /// <summary>
        /// Extraction du cryptogramme à partir du Token
        /// </summary>
        /// <param name="user">Compte utilisateur du client</param>
        /// <param name="token">Numéro du token identifiant la carte</param>
        /// <returns>Numéro de carte encrypté</returns>
        public static string GetEncryptedPanFromBOToken(UserInfo user, long token)
        {
            string EncryptedPAN = null;
            EncryptedDataConnection conn = null;
            try
            {
                // On définit une nouvelle connexion vers la base des données encryptées
                conn = new EncryptedDataConnection(user);

                // On se connecte
                conn.Open();

                // on retourne le cryptogramme du PAN à partir du Token
                EncryptedPAN = conn.GetEncryptedPAN(token);

                if (String.IsNullOrEmpty(EncryptedPAN))
                {
                    // Aucune carte avec le Token
                    // n'a été trouvé dans la base
                    throw new Exception(user.GetMessages().GetString("NoPANFoundForToken", token, true));
                }
                // Ok on a le PAN encrypté
            }
            catch (Exception e)
            {
                // Gérer cette exception
                throw new Exception(e.Message);
            }
            finally
            {
                // On se deconnecte
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette erreur
                }
            }
            // On retour le cryptogramme
            return EncryptedPAN;
        }


        /// <summary>
        /// Retour des informations d'une carte depuis Navison
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="Pos">Pos Navision</param>
        /// <param name="token">Numéro du token identifiant la carte</param>
        /// <returns></returns>
        public static PanInfoResult GetPanInfos(UserInfo user, string Pos, long token)
        {
            PanInfoResult PanInfo = null;
            NavisionDbConnection conn = null;
            try
            {
                // On définit une nouvelle connexion
                conn = new NavisionDbConnection(user, Pos);

                // On se connecte
                conn.Open();

                // on retourne les informations sur le PAN à partir du Token
                // Si le pos n'a pas été spécifié, le token sera utilisé pour retourner le POS
                // Lors de la recherche du POS, une priorité sera accordée aux grands pays (FRANCE, UK,...)
                PanInfo = conn.GetPanInfos(token);

            }
            catch (Exception e)
            {
                // Gérer cette exception
                throw new Exception(e.Message);
            }
            finally
            {
                // On se deconnecte
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette erreur
                }
            }

            return PanInfo;
        }

        /// <summary>
        /// Retour des informations étendues d'une carte depuis Navison
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="Pos">Pos Navision</param>
        /// <param name="token">Numéro du token identifiant la carte</param>
        /// <returns></returns>
        public static ExtendedPanInfoResult GetExtendedPanInfos(UserInfo user, string Pos, long token)
        {
            ExtendedPanInfoResult ExPanInfo = new ExtendedPanInfoResult();

            try
            {
                // On récupère le PAN depuis la base des données encryptées
                // Le cryptogramme sera extrait de la base de données et
                // décrypté à la volée
                string Pan = GetPanFromBOToken(user, token);

                // On a récupéré le token
                // On va maintenant essayer de récupérer les
                // information complémentaires (date expiration, cvc, ...) depuis Navision
                ExPanInfo.SetValues(token, Pan, GetPanInfos(user, Pos, token));
            }
            catch (Exception e)
            {
                // Gérer cette exception
                throw new Exception(e.Message);
            }

            return ExPanInfo;
        }
        /// <summary>
        /// Retour des informations d'une carte depuis Navison
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="customer">Identifiant du client (comcode)</param>
        /// <param name="cc1">Indentifiant du centre de cout (cc1)</param>
        /// <param name="traveller">Identifiant du voyageur (percode)</param>
        /// <param name="service">Service group (AIR, RAIL, ...)</param>
        /// <returns>Informations sur la carte</returns>
        public static UserBookingPaymentRSResult GetUserBookingPaymentRS(UserInfo user, string pos, string customer,
            string cc1, string traveller, string service)
        {
            UserBookingPaymentRSResult PaymentRS = null;
            NavisionDbConnection conn = null;
            try
            {
                // On définit une nouvelle connexion
                conn = new NavisionDbConnection(user, pos);

                // On se connecte
                conn.Open();
                // On récupère les informations liées à la carte 
                // dans Navision
                // à ce stade nous avons le token
                // et pas la carte en clair
                PaymentRS = conn.GetUserBookingPayment(customer, cc1, traveller, service);

            }
            catch (Exception e)
            {
                // Gérer cette exception
                throw new Exception(e.Message);
            }
            finally
            {
                // On se deconnecte
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette erreur
                }
            }

            return PaymentRS;
        }


        /// <summary>
        /// Retour des informations sur le mode paiement depuis Navison
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="UserPaymentTypeReader">Paramètres appel</param>
        /// <returns>Informations sur le mode de paiement</returns>
        public static UserPaymentTypeResult GetUserPaymentType(UserInfo user, UserPaymentTypeReader reader)
        {
            UserPaymentTypeResult PaymentType = null;

            // Extraction des paramètres d'appel
            string customer = reader.GetComCode();
            string costCenter = reader.GetCostCenter();
            string traveller = reader.GetPerCode();
            string service = reader.GetService();
            string pos = reader.GetPosCode();

            if (String.IsNullOrEmpty(traveller))
            {
                // Il faut obligatoirement une valeur!
                throw new Exception(user.GetMessages().GetString("PercodeEmpty", true));
            }

            NavisionDbConnection conn = null;
            try
            {
                // On définit une nouvelle connexion
                conn = new NavisionDbConnection(user, pos);

                // On se connecte
                conn.Open();

                // On récupère les informations liées 
                // au mode de paiement dans Navision
                PaymentType = conn.GetUserPaymentType(customer, costCenter, traveller, service);

            }
            catch (Exception e)
            {
                // Gérer cette exception
                throw new Exception(e.Message);
            }
            finally
            {
                // On se deconnecte
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette erreur
                }
            }

            if (PaymentType.IsError())
            {
                // Navision a renvoyé une erreur
                // Il faut la prendre en compte
                throw new Exception(PaymentType.GetErrorMsg());
            }

            return PaymentType;
        }


        /// <summary>
        /// Insertion d'une carte private ou corporate dans la base des cartes encryptées
        /// et retour du token identifiant le numéro de carte
        /// et ensuite insertion dans Navision
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pos">Pos Navision</param>
        /// <param name="customer">Identifiant du client (comcode)</param>
        /// <param name="cc1">Identifiant du centre de out (cc1)</param>
        /// <param name="traveller">Identifiant du voyageur (percode)</param>
        /// <param name="expirationDate">Date d'expiration de la carte</param>
        /// <param name="service">Service group (AIR, RAIL, ...)</param>
        /// <param name="pan">Numéro de la carte en clair</param>
        /// <param name="description">Description</param>
        /// <param name="holderName">holderName</param>
        /// <param name="lodgedCard">lodgedCard</param>
        /// <param name="firstCardReference">firstCardReference</param>
        /// <returns>Informations sur la carte insérée</returns>
        public static InsertCardResult InsertProfilCard(UserInfo user, string pos, string customer,
            string cc1, string traveller, string expirationDate, string service, string pan, string description,
            string holderName, int lodgedCard, string firstCardReference, int forcewarning)
        {
            return InsertCardAndReturnRef(user, pos, customer, cc1, traveller, expirationDate,
                service, pan, description, 0, string.Empty, string.Empty, holderName, lodgedCard, firstCardReference, forcewarning);
        }

        /// <summary>
        /// Insertion d'une carte transactionnelle la base des cartes encryptées
        /// et retour du token identifiant le numéro de carte
        /// et ensuite insertion dans Navision
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pos">Pos Navision</param>
        /// <param name="customer">Identifiant du client (comcode)</param>
        /// <param name="traveller">Identifiant du voyageur (percode)</param>
        /// <param name="expirationDate">Date d'expiration de la carte</param>
        /// <param name="service">Service group (AIR, RAIL, ...)</param>
        /// <param name="pan">Numéro de la carte en clair</param>
        /// <param name="transactionalCard">Indicateur carte transactionnelle</param>
        /// <param name="contextSource">Source contexte (billing unit/préfacture/CLE)</param>
        /// <param name="context">Context (n°ticket ou n°préfacture)</param>
        /// <param name="holderName">holderName</param>
        /// <param name="lodgedCard">lodgedCard</param>
        /// <returns>Informations sur la carte insérée</returns>
        public static InsertCardResult InsertTransactionalCard(UserInfo user, string pos, string customer,
          string traveller, string expirationDate, string service, string pan, string contextSource, string context, string holderName, int lodgedCard, string firstCardReference, int forcewarning)
        {
            return InsertCardAndReturnRef(user, pos, customer, string.Empty,
             traveller, expirationDate, service, pan, string.Empty, 1, contextSource, context, holderName, lodgedCard, firstCardReference, forcewarning);
        }

        /// <summary>
        /// Insertion d'une carte dans la base des cartes encryptées
        /// et retour du token identifiant le numéro de carte
        /// et ensuite insertion dans Navision
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pos">Pos Navision</param>
        /// <param name="customer">Identifiant du client (comcode)</param>
        /// <param name="cc1">Identifiant du centre de out (cc1)</param>
        /// <param name="traveller">Identifiant du voyageur (percode)</param>
        /// <param name="expirationDate">Date d'expiration de la carte</param>
        /// <param name="service">Service group (AIR, RAIL, ...)</param>
        /// <param name="pan">Numéro de la carte en clair</param>
        /// <param name="description">Description</param>
        /// <param name="transactionalCard">Indicateur carte transactionnelle</param>
        /// <param name="contextSource">Source contexte (billing unit/préfacture/CLE)</param>
        /// <param name="context">Context (n°ticket, n°préfacture ou n°entree CLE)</param>
        /// <param name="holderName">holderName)</param>
        /// <param name="lodgedCard">lodgedCard)</param>
        /// <param name="firstCardReference">firstCardReference)</param>
        /// <returns>Informations sur la carte insérée</returns>
        private static InsertCardResult InsertCardAndReturnRef(UserInfo user, string pos, string customer,
             string cc1, string traveller, string expirationDate, string serviceGroup, string pan, string description,
             int transactionalCard, string contextSource, string context, string holderName, int lodgedCard, string firstCardReference, int forcewarning)
        {
            InsertCardResult result = new InsertCardResult();

            // Sanity check for POS
            ArgsChecker.ValidatePOS(user, pos);

            // Sanity check for customer code
            ArgsChecker.ValidateComCode(user, customer, true);

            // La date d'expiration doit être spécifiée
            if (String.IsNullOrEmpty(expirationDate))
            {
                throw new Exception(user.GetMessages().GetString("ExpirationDateEmpty", true));
            }

            // validate service
            string service = Util.CorrectService(user, serviceGroup);

            if (transactionalCard == 1)
            {
                // Il s'agit d'une carte transactionnelle
                // On a besoin du contexte
                // Ticket number, Invoice/Credit memo, numéro de l'entrée CLE
                if (String.IsNullOrEmpty(context))
                {
                    throw new Exception(user.GetMessages().GetString("TransactionalCardContextEmpty", true));
                }
                // Sanity check to traveler, mandatory
                ArgsChecker.ValidatePerCode(user, traveller, true);

                // Sanity check on context source
                ArgsChecker.ValidateContextSource(user, contextSource);
            }

            try
            {
                // On récupère la date d'expiration en date 
                DateTime DateExpirationDate = Util.ConvertExpirationDateToDate(user, expirationDate);

                // On vérifie si la carte est valide
                // Luhn plus contrôle sur la date de validité
                // plus validation en ligne si nécéssaire
                CardInfos CardInfo = CheckCreditCard(user, pan, DateExpirationDate, holderName, pos,
                    BibitVerifier.RBSRequestBODefaultTimeOut, true, true, service, customer, traveller, string.Empty, lodgedCard, firstCardReference);

                // La carte est valide
                if (transactionalCard == 1)
                {
                    // La carte est une carte transactionnelle
                    CardInfo.SetNavisionTransactional(transactionalCard);
                }


                // Tout d'abord, on récupère le numéro du Token
                // depuis la base dans laquelle sont stockés les numéros
                // de cartes encryptés
                // si le numéro de carte est introuvable, il sera inséré
                long token = InsertCardInEncryptedDB(user, pan, CardInfo, expirationDate, lodgedCard).GetToken();

                // assign token
                CardInfo.SetToken(Util.ConvertTokenToString(token));

                // Prepare response from Navision webservice
                InsertCardInNavisionResult insertResult = new InsertCardInNavisionResult();

                // We have everything..we can insert in the system
                // Call Nav Webservice
                // we will get all response (Exception included)
                NavServiceUtils.InsertPaymentCard(user, pos, customer, cc1, traveller, service, insertResult, CardInfo, forcewarning, contextSource, context);

                //First we need to raise an exception in case of failure
                if (insertResult.isError())
                {
                    // something went wrong at insertion
                    // We need to build new exception and retrieve values (code and message returned by Navision ws)
                    CEEException Ex = CCEExceptionUtil.BuildCCEException(insertResult.GetExceptionCode(),
                        CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                         CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                        insertResult.GetExceptionDesc());

                    // Throw the exception
                    throw new Exception(Ex.GetCompleteExceptionMessage());
                }

                // set result
                result.SetValues(token, insertResult, Util.GetShortExpirationDate(DateExpirationDate),
                    CardInfo.GetCardType(), CardInfo.GetTruncatedPAN(), service, transactionalCard, contextSource);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return result;
        }


        /// <summary>
        /// Insertion d'une carte dans la base des cartes encryptées
        /// et retour du token identifiant le numéro de carte
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pan">Numéro de carte en clair</param>
        /// <param name="ci">Vérifier le numéro de carte</param>
        /// <param name="expirationDate">Date d'expiration</param>
        /// <param name="lodgedCard">lodgedCard</param>
        /// <returns>Informations sur la carte</returns>
        public static InsertCardInEncryptedDBResult InsertCardInEncryptedDB(UserInfo user, string pan
            , CardInfos ci, string expirationDate, int lodgedCard)
        {
            InsertCardInEncryptedDBResult result = new InsertCardInEncryptedDBResult();

            CardInfos rt = ci;
            // A-t-on besoin de contrôler le numéro de carte?
            if (rt == null)
            {
                // On vérifier le numéro de carte (test de Luhn)
                rt = CheckCreditCard(user, pan, expirationDate, lodgedCard);

                // La carte est valide
                // On récupère quelques infos intéressantes
                // Type de carte
                result.SetCardType(rt.GetCardType());
                // Type de carte (court)
                result.SetShortCardType(rt.GetShortCardType());
                // PAN tronqué
                result.SetTruncatedPan(rt.GetTruncatedPAN());
            }

            EncryptedDataConnection conn = null;
            try
            {
                // On encrypte le PAN ...
                // si necéssaire
                string encryptedPAN = rt.GetEncryptedPan();
                if (encryptedPAN == null)
                {
                    encryptedPAN = EncryptBOCard(user, pan);
                }

                // On définit une nouvelle connexion
                // vers la base des données encryptées
                conn = new EncryptedDataConnection(user);

                // On se connecte à la base de données
                conn.Open();

                // On vérifie si la carte est déjà enregistrée dans la base
                // On compare le cryptogramme avec ceux enregistrés dans la base
                // Si la fonction renvoit un Token
                // alors la carte existe dans la base de données
                // autrement la carte n'existe pas encore (token = -1)
                long token = conn.GetToken(encryptedPAN);

                if (token < 0)
                {
                    // La carte n'existe pas!
                    // On insère et on récupére le token
                    token = conn.InsertEncryptedCard(encryptedPAN);
                }
                // On a le token, on le retourne
                result.SetToken(token);
            }
            catch (Exception e)
            {
                throw new Exception(user.GetMessages().GetString("Services.InsertCardInEncryptedDB.Error", e.Message, true));
            }
            finally
            {
                // On se deconnecte
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
            }

            return result;
        }

      
        /// <summary>
        /// Encyptage d'une chaine de caractères via SafeNet
        /// Le numéro de carte est représenté par le token
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="stringToEncrypt">Chaîne à encrypter</param>
        /// <param name="username">Compte utilisateur SafeNet</param>
        /// <param name="encryptedPassword">Mot de passe du compte utilisateur SafeNet</param>
        /// <param name="callerType">callerType (BO ou FO)</param>
        /// <returns>Cryptogramme</returns>
        public static string EncryptData(UserInfo user, string stringToEncrypt, string username,
            string encryptedPassword, int callerType)
        {
            string retval = null;
            SafeNetSession sf = null;
            try
            {
                // Ouverture d'une session Ingrian
                sf = new SafeNetSession(user, username, EncDec.DecryptPassword(encryptedPassword), callerType);

                // Encryptage de la donnée
                retval = sf.Encrypt(stringToEncrypt);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                if (sf != null) sf.CloseSession();
            }

            return retval;
        }


        /// <summary>
        /// Encyptage d'une chaine de caractères via SafeNet
        /// Le numéro de carte est représenté par le token
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="encryptedData">Donnée encrypté</param>
        /// <param name="username">Compte utilisateur SafeNet</param>
        /// <param name="encryptedPassword">Mot de passe du compte utilisateur SafeNet</param>
        /// <param name="callerType">callerType (BO ou FO)</param>
        /// <returns>Donnée décryptée</returns>
        private static string DecryptData(UserInfo user, string encryptedData, string username,
            string encryptedPassword, int callerType)
        {
            string retval = null;
            SafeNetSession sf = null;
            try
            {
                // Open a new SafeNet session
                sf = new SafeNetSession(user, username, EncDec.DecryptPassword(encryptedPassword), callerType);

                // Decrypt
                retval = sf.Decrypt(encryptedData);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                if (sf != null) sf.CloseSession();
            }

            return retval;
        }


        /// <summary>
        /// Vérification de la conformité d'un numéro de carte bancaire
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pan">Numéro de carte en clair</param>
        /// <returns></returns>
        public static CardInfos CheckCreditCard(UserInfo user, string pan)
        {
            //>>EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check 
            //return CheckCreditCard(user, pan, Const.EmptyDate, null, null, null, false, true, null, null, null, null, false, 0);
            //return CheckCreditCard(user, pan, Const.EmptyDate, null, null, null, false, true, null, null, null, null, 0, string.Empty);
            return CheckCreditCard(user, pan, null, 0);
            //<<EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check 
        }

        /// <summary>
        /// Vérification de la conformité d'un numéro de carte bancaire
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pan">Numéro de carte en clair</param>
        /// <param name="expirationDate">Date d'expiratiion</param>
        /// <returns></returns>
        public static CardInfos CheckCreditCard(UserInfo user, string pan, string expirationDate, int lodgeCard)
        {
            // Let's put defaut expiration date
            DateTime DateExpirationDate = Const.EmptyDate;

            if (!String.IsNullOrEmpty(expirationDate))
            {
                // Let's convert into expiration date
                DateExpirationDate = Util.ConvertExpirationDateToDate(user, expirationDate);
            }

            return CheckCreditCard(user, pan, DateExpirationDate, null, null, null, false, true, null, null, null, null, lodgeCard, string.Empty);
        }

        ///
        /// <summary>
        /// Vérification de la conformité d'un numéro de carte bancaire
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pan">Numéro de carte en clair</param>
        /// <param name="expirationDate">Date d'expiration de la carte</param>
        /// <param name="cardHolder">Détenteur de la carte</param>
        /// <param name="pos">pos</param>
        /// <param name="timeout">timeout</param>
        /// <param name="onlineCheck">Activation de la vérification en ligne</param>
        /// <param name="failIfInvalid">Echec si la carte est invalide</param>
        /// <param name="service">Service group</param>
        /// <param name="customercode">Code du client</param>
        /// <param name="travellercode">Code du voyageur</param>
        /// <param name="token">Token</param>
        /// <param name="lodgedCard">LodgedCard TRUE or FALSE</param>
        /// <param name="firstCardReference">firstCardReference</param>
        /// <returns>Infos sur la carte (validité, type)</returns>
        public static CardInfos CheckCreditCard(UserInfo user, string pan, DateTime expirationDate,
            string cardHolder, string pos, string timeout, bool onlineCheck, bool failIfInvalid,
            string service, string customercode, string travellercode, string token, int lodgedCard, string firstCardReference)
        {

            CardInfos rt = CreditCardVerifier.CheckCardNumber(user, pan, expirationDate, onlineCheck, cardHolder,
                pos, Util.ConvertStringToInt(Util.Nvl(timeout, HttpUtil.NoTimeOutString)),
                service, customercode, travellercode, token, lodgedCard, firstCardReference);

            if (!rt.IsCardValid() && failIfInvalid)
            {

                // Si la carte est invalide, alors
                // il faut lever une exception
                throw new Exception(rt.GetUnValidMsg());
            }
            return rt;
        }


        /// <summary>
        /// Nettoyage des fichiers dans lesquels sont enregistrées
        /// le nombre de visualisation de PAN
        /// Pour chaque client et par jour correspond un fichier
        /// Le nettoyage se fait lors du premier appel du jour suivant
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        public static void RemoveDisplayPANCount(UserInfo user)
        {
            try
            {
                // L'idée consiste à supprimer les fichiers une fois dans la journée!
                // Pour cela la date du dernier traitement est sauvegardée dans un fichier
                // (voir répertoire displayCardsFilesFolder dans Web.config).
                String automaticFilename = Util.GetLastAutomaticRunDateFileName();

                if (String.IsNullOrEmpty(automaticFilename))
                {
                    // On ne recupere le fichier...on ne peut pas aller plus loin
                    return;
                }

                // Récupération de la date courante
                string CurrentDate = DateTime.Now.ToString(Const.DateFormat_yyyyMMdd);
                // Récupération de la valeur enregistrée dans le fichier
                string SavedDate = Util.Nvl(Util.GetContentDispayCardsFilesFolder(automaticFilename, true), string.Empty);

                if (!CurrentDate.Equals(SavedDate))
                {
                    // On démarre une nouvelle journée
                    // Le répertoire doit être vidée afin de remettre à zéro les pendules
                    string FolderName = @ConfigurationManager.AppSettings["DisplayCardsFilesFolder"];

                    if (String.IsNullOrEmpty(FolderName))
                    {
                        // Le repertoire est introuvable (vide)...on ne peut pas aller plus loin
                        return;
                    }
                    try
                    {
                        // On supprime tous les fichiers dans lesquels le nombre de visualisation
                        // des numéros de cartes est enregistré
                        string[] list = Directory.GetFiles(FolderName);

                        // On supprime tous les fichiers dans lesquels le nombre de visualisation
                        // des numéros de cartes est enregistré
                        if (list != null && list.Length > 0)
                        {
                            foreach (string fileName in Directory.GetFiles(FolderName))
                            {
                                // On va éviter de supprimer le fichier qui contient
                                // la date de dernier traitement (dans le cas ou ce dernier
                                // est dans le même répertoire
                                if (!fileName.Equals(automaticFilename))
                                {
                                    File.Delete(fileName);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(user.GetMessages().GetString("Services.RemoveDisplayPANCount.Error", FolderName, e.Message, true));
                    }
                    // Les indicateurs ont été remis à zéro
                    // On indique la date de traitrement
                    // le prochain passage c'est le lendemain
                    Util.SaveContentToFile(automaticFilename, CurrentDate);
                }
            }
            catch (Exception)
            {
                // On ignore cette erreur
            }

        }


        /// <summary>
        /// Récupération des informations sur un utilisateur
        /// </summary>
        /// <param name="local">Language de dialogue avec le client</param>
        /// <param name="ip">Adresse IP du client</param>
        /// <param name="checkLogin">Nom du compte</param>
        /// <param name="checkPassword">Mot de passe du compte</param>
        /// <param name="passwordRequired">Indique si le mot de passe doit etre verifie</param>
        /// <param name="application">Nom de l'application du client</param>
        /// <returns>Structure XML sur les infos du compte</returns>
        /*public static string GetUserInfo(string local, RemoteHost ip, string checkLogin, string checkPassword,
            bool passwordRequired, int application)
        {
            return GetUserInfo(local, ip, checkLogin, checkPassword, passwordRequired, application, null);
        }*/


        /// <summary>
        /// Récupération des informations sur un utilisateur
        /// </summary>
        /// <param name="local">Language de dialogue avec le client</param>
        /// <param name="ip">Adresse IP du client</param>
        /// <param name="checkLogin">Nom du compte</param>
        /// <param name="checkPassword">Mot de passe du compte</param>
        /// <param name="passwordRequired">Indique si le mot de passe doit etre verifie</param>
        /// <param name="application">Nom de l'application du client</param>
        /// <param name="requestedGroups">Le compte doit appartenir a la liste de groupes LDAP</param>
        /// <returns>Structure XML sur les infos du compte</returns>
        /*public static string GetUserInfo(string local, RemoteHost ip, string checkLogin, string checkPassword,
            bool passwordRequired, int application, string requestedGroups)
        {
            // On prépare la réponse que l'on va
            // apporter (initialisation)
            UserInfoResponse response = new UserInfoResponse();

            try
            {
                // On passe les informations minimales
                response.SetValue(new UserInfo(local, checkLogin, checkPassword, passwordRequired, ip,
                     application, UserInfo.ACTION_RETURN_RIGHT, requestedGroups));

                // On récupère les informations sur l'utilisateur
                // depuis l'AD
                // On ne vérifie pas les droits
                // On a besoin de les retourner au client!

                // On a les informations sur l'utilisateur
                response.SetValue(ArgsChecker.CheckLogin(response.GetValue()));
            }
            catch (Exception e)
            {
                // Une exception a été levée 
                // On retour l'erreur au client
                response.SetException(e);
            }
            // On retourne la réponse au client
            return response.GetResponse();
        }*/

        /// <summary>
        /// Insertion d'une carte dans la base des cartes encryptées
        /// hébergées par le FrontOffice
        /// et retour du token identifiant le numéro de carte
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pan">Numéro de carte en clair</param>
        /// <param name="expirationDate">Date d'expiration de la carte</param>
        /// <param name="cardHolder">Holder name</param>
        /// <param name="pos">Pos</param>
        /// <param name="timeOut">timeOut (secondes)</param>
        /// <param name="onlineCheck">Activation vérification cartes en ligne</param>
        /// <param name="lodgedCard">Lodged card</param>
        /// <returns>Informations sur la carte</returns>
        public static InsertCardInEncryptedFODBResult InsertCardInEncryptedFODB(UserInfo user, string pan, string expirationDate,
            string cardHolder, string pos, string timeOut, bool onlineCheck, bool lodgedCard)
        {
            // La date d'expiration doit être spécifiée
            if (String.IsNullOrEmpty(expirationDate))
            {
                // Pas de date d'expiration, on ne peut pas aller plus loin
                throw new Exception(user.GetMessages().GetString("ExpirationDateEmpty", true));
            }

            if (onlineCheck)
            {
                // Pour les contrôles via des APIs partenaires
                // Par exemple RBS
                // Nous devons avoir un POS
                if (String.IsNullOrEmpty(pos))
                {
                    // Pas de POS, on arrête là
                    throw new Exception(user.GetMessages().GetString("PosEmpty", true));
                }
            }

            // On récupère la date d'expiration en date 
            DateTime DateExpirationDate = Util.ConvertExpirationDateToDate(user, expirationDate);

            // On vérifie le numéro de carte (test de Luhn)
            CardInfos rt = CheckCreditCard(user, pan, DateExpirationDate, cardHolder, pos, timeOut, onlineCheck, true,
             string.Empty, string.Empty, string.Empty, string.Empty, lodgedCard ? 1 : 0, string.Empty);


            // On prépare la réponse à apporter
            InsertCardInEncryptedFODBResult Result = new InsertCardInEncryptedFODBResult();
            Result.SetInformationCode(rt.GetInformationCode());
            Result.SetInformationMessage(rt.GetInformationMessage());

            // La carte est valide
            // On récupère quelques infos intéressantes
            Result.SetValues(rt);

            // On va maintenant insérer le numéro de carte encrypté
            // dans la base des données encryptées
            // hébergée par FrontOffice
            EncryptedFODataConnection Conn = null;
            try
            {
                // On encrypte le PAN ...
                // mais avant, vérifions si nous ne l'avons pas fait déjà
                // lors de l'éventuelle validation BIBIT
                string encryptedPAN = rt.GetEncryptedPan();
                if (encryptedPAN == null)
                {
                    encryptedPAN = EncryptFOCard(user, pan);
                }

                // On définit une nouvelle connexion
                // vers la base des données encryptées
                Conn = new EncryptedFODataConnection(user);

                // On se connecte à la base de données
                Conn.Open();

                // On vérifie si la carte est déjà enregistrée dans la base
                // On compare le cryptogramme avec ceux enregistrés dans la base
                // Si la fonction renvoit un Token
                // alors la carte existe dans la base de données
                // autrement la carte n'existe pas encore
                FOTokenResult TokenStored = Conn.GetToken(encryptedPAN);

                // retrieve token
                string Token = TokenStored.GetToken();

                if (!TokenStored.isFound())
                {
                    // La carte n'existe pas!
                    // On insère et on récupére le token
                    Token = Conn.InsertEncryptedCard(encryptedPAN, DateExpirationDate);
                }
                else
                {
                    // Ce cryptogramme est déjà stoqué dans la base de données
                    // Nous allons vérifier si la date d'expiration n'a pas changé
                    if (!DateExpirationDate.Equals(TokenStored.GetExpirationDate()))
                    {
                        // La date d'expiration a changé
                        TokenStored.SetExpirationDate(DateExpirationDate);
                        // Nous devons la mettre à jour
                        Conn.UpdateEncryptedCard(TokenStored);
                    }
                }
                // On a le token, on le retourne
                Result.SetToken(Token);
            }
            catch (Exception e)
            {
                throw new Exception(user.GetMessages().GetString("Services.InsertCardInEncryptedDB.Error", e.Message, true));
            }
            finally
            {
                // On se deconnecte
                if (Conn != null)
                {
                    try
                    {
                        Conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
            }

            return Result;
        }

        /// <summary>
        /// Récupération du PAN et de la date d'expiration
        /// depuis le token FrontOffice
        /// Le cryptogramme est extrait de la table hébergée côté Front
        /// et inséré dans la table côté BackOffice
        /// Ensuite le cryptogramme est décrypté à la volée
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="FOtoken">Token FrontOffice</param>
        /// <param name="returnOnlyToken">Retourne uniquement le token BackOffice</param>
        /// <returns>FOPanInfoResult</returns>
        public static FOPanInfoResult GetPanInfosFromFOToken(UserInfo user, string FOtoken, bool returnOnlyToken)
        {
            // On initialise la réponse
            FOPanInfoResult result = new FOPanInfoResult();

            // On défini une connexion à la base BO
            EncryptedDataConnection BOConn = null;
            // On défini une connexion à la base FO
            EncryptedFODataConnection FOConn = null;

            try
            {
                // Initialisation de la connexion
                BOConn = new EncryptedDataConnection(user);

                // On se connecte à la base
                BOConn.Open();

                // On recherche le token dans la table de mapping
                TokensMappingResult TokenMapping = BOConn.GetBOTokenFromMapping(FOtoken);

                // On récupère le token BO correspondant au token FO
                // dans la table de mapping
                long BOToken = TokenMapping.GetBOToken();
                DateTime expirationDate;
                string encryptedPAN = null;

                if (Util.IsEmptyToken(BOToken))
                {
                    // La carte n'est pas dans la table de mapping
                    // et donc cette donnée n'est pas disponible dans
                    // la base des cartes encryptées BackOffice
                    // Nous sommes donc obliger de rechercher la carte dans la base FrontOffice
                    FOConn = new EncryptedFODataConnection(user);

                    // On va se connecter à la base des cartes cryptées
                    // hébergée par FrontOffice
                    FOConn.Open();

                    // On lance la recherche pour le token
                    FOEncryptedPanInfoResult FOEncryptedPAN = FOConn.GetEncryptedPANAndExpirationDate(FOtoken);

                    // On a le retour de l'appel
                    // Il faut récupérer le cryptogramme
                    encryptedPAN = FOEncryptedPAN.GetEncryptedPAN();

                    if (encryptedPAN == null)
                    {
                        // Ce token est introuvable!
                        // On ne peut plus continuer.
                        throw new Exception(user.GetMessages().GetString("Services.GetPanFromFOToken.FOTokenUnknow", FOtoken, true));
                    }

                    // Au passage, on récupère la date d'expiration
                    expirationDate = FOEncryptedPAN.GetExpirationDate();

                    // On a le cryptogramme
                    // On vérifie si la carte est déjà enregistrée dans la base BO
                    // On compare le cryptogramme avec ceux enregistrés dans la base
                    // Si la fonction renvoit un Token
                    // alors la carte existe dans la base de données
                    // autrement la carte n'existe pas encore
                    BOToken = BOConn.GetToken(encryptedPAN);

                    if (Util.IsEmptyToken(BOToken))
                    {
                        // La carte n'existe pas!
                        // on peut insérer ce cryptogramme dans la base des cartes cryptées
                        // ainsi on aura un token BackOffice
                        BOToken = BOConn.InsertEncryptedCard(encryptedPAN);
                    }

                    // On a bien inséré la carte dans la table BackOffice,
                    // on va mettre à jour la table des mapping
                    BOConn.SetTokensMapping(BOToken, FOtoken, expirationDate);

                    // La table mapping a été correctement mise à jour
                    // On peut maintenant supprimer la carte de la table FrontOffice
                    FOConn.DeleteCard(FOtoken);
                }
                else
                {
                    // La carte est dans la table de mapping
                    // On va récupérer le cryptogramme depuis la base BO
                    encryptedPAN = BOConn.GetEncryptedPAN(BOToken);
                    // et la date d'expiration
                    expirationDate = TokenMapping.GetExpirationDate();
                }

                if (!returnOnlyToken)
                {
                    // On a besoin de retourner toutes les informations
                    // et pas uniquement le Token BackOffice
                    // On a le cryptogramme, on peut donc
                    // solliciter un décryptage
                    string pan = DecryptBOCard(user, encryptedPAN);

                    // On a tout ce qu'il nous faut
                    // On va préparer la réponse renvoyée au client
                    result.SetValues(BOToken, pan, expirationDate, Services.CheckCreditCard(user, pan));
                }
                else
                {
                    // On retourne uniquement le token BackOffice
                    // donc pas besoin de décrypter le PAN
                    result.SetValues(BOToken, null, expirationDate, null);
                }
            }
            catch (Exception e)
            {
                throw new Exception(user.GetMessages().GetString("Services.GetPanFromFOToken.Error", FOtoken, e.Message, true));
            }
            finally
            {
                // On se deconnecte de la base BO
                if (BOConn != null)
                {
                    try
                    {
                        BOConn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
                // On se deconnecte de la base FO
                if (FOConn != null)
                {
                    try
                    {
                        FOConn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
            }


            return result;
        }


        /// <summary>
        /// Retrait de toutes les entrées du cache
        /// Bibit au niveau de la base FrontOffice
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <returns>Nombre d'entrées retirées</returns>
        private static int ClearFOBibitCache(UserInfo user)
        {
            int retval = 0;

            // On défini une connexion à la base FO
            EncryptedFODataConnection Conn = null;

            try
            {
                // Initialisation de la connexion
                Conn = new EncryptedFODataConnection(user);

                // On se connecte à la base
                Conn.Open();

                // On vide le cache et retourne le nombre d'entrées supprimées
                retval = Conn.ClearBibitCache();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                // On se deconnecte de la base FO
                if (Conn != null)
                {
                    try
                    {
                        Conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
            }
            return retval;
        }
        /// <summary>
        /// Retrait de toutes les entrées du cache
        /// Bibit au niveau de la base BackOffice
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <returns>Nombre d'entrées retirées</returns>
        private static int ClearBOBibitCache(UserInfo user)
        {
            int retval = 0;

            // On défini une connexion à la base BO
            EncryptedDataConnection Conn = null;

            try
            {
                // Initialisation de la connexion
                Conn = new EncryptedDataConnection(user);

                // On se connecte à la base
                Conn.Open();

                // On vide le cache et retourne le nombre d'entrées supprimées
                retval = Conn.ClearBibitCache();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                // On se deconnecte de la base
                if (Conn != null)
                {
                    try
                    {
                        Conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
            }
            return retval;
        }


        /// <summary>
        /// Cette fonction permet de récupération les dernières cartes
        /// restantes dans la base FrontOffice
        /// avant de mettre à jour les cryptogrammes
        /// après une rotation de version de clé
        /// </summary>
        /// <returns>Nombre de cartes récupérées</returns>
        public static int GetRemainingFOCards(UserInfo user)
        {
            int retval = 0;

            // On défini une connexion à la base BO
            EncryptedDataConnection BOConn = null;
            // On défini une connexion à la base FO
            EncryptedFODataConnection FOConn = null;

            try
            {
                // Initialisation de la connexion
                FOConn = new EncryptedFODataConnection(user);

                // On se connecte à la base
                FOConn.Open();

                // On va extraire toutes les cartes qui restent dans cette base
                FORemainingEncryptedData cards = FOConn.GetAllRemainingTokens();

                if (cards.GetSize() > 0)
                {
                    // Il reste des cartes dans la base
                    // Nous aurons donc a copier des cryptogrammes
                    // dans la base des cartes BackOffice

                    // Initialisation de la connexion
                    BOConn = new EncryptedDataConnection(user);

                    // On se connecte à la base
                    BOConn.Open();

                    // On va parcourir la liste des tokens
                    IDictionaryEnumerator cardsList = cards.GetTokens().GetEnumerator();

                    while (cardsList.MoveNext())
                    {
                        // On a le Token FronOffice
                        string FOToken = cardsList.Key.ToString();
                        // On extrait le cryptogramme et la date d'expiration
                        FORemainingEncryptedValue cardValue = (FORemainingEncryptedValue)cardsList.Value;

                        // On a le cryptogramme
                        // on peut insérer ce cryptogramme dans la base des cartes cryptées
                        // ainsi on aura un token BackOffice
                        long BOToken = BOConn.InsertEncryptedCard(cardValue.GetEncryptedPAN());

                        // On a bien inséré la carte dans la table BackOffice,
                        // on va mettre à jour la table des mapping
                        // avec la date d'expiration
                        BOConn.SetTokensMapping(BOToken, FOToken, cardValue.GetExpirationDate());

                        // La table mapping a été correctement mise à jour
                        // On peut maintenant supprimer la carte de la table FrontOffice
                        FOConn.DeleteCard(FOToken);

                        // On incrémente le nombre de carte récupéré
                        retval++;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(user.GetMessages().GetString("Services.GetRemainingFOCards.Error", e.Message, true));
            }
            finally
            {
                // On se deconnecte de la base BO
                if (BOConn != null)
                {
                    try
                    {
                        BOConn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
                // On se deconnecte de la base FO
                if (FOConn != null)
                {
                    try
                    {
                        FOConn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
            }
            return retval;
        }

        /// <summary>
        /// Encryption d'un PAN
        /// Cette fonction est utilisée par le FRONT
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pan">Numéro de carte en clair</param>
        /// <returns>Cryptogramme</returns> 
        public static string EncryptFOCard(UserInfo user, string pan)
        {
            // On encrypte le PAN et returne le cryptogramme
            return EncryptData(user, pan,
                ConfigurationManager.AppSettings["SafeNetSessionFOUsername"],
                ConfigurationManager.AppSettings["SafeNetSessionFOPassword"],
                SafeNetSession.CallerTypeFO);
        }


        /// <summary>
        /// Encryption d'un PAN
        /// Cette fonction est utilisée par le BackOffice
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pan">Numéro de carte en clair</param>
        /// <returns>Cryptogramme</returns> 
        public static string EncryptBOCard(UserInfo user, string pan)
        {
            // On encrypte le PAN et returne le cryptogramme
            return EncryptData(user, pan,
                ConfigurationManager.AppSettings["SafeNetSessionUsername"],
                ConfigurationManager.AppSettings["SafeNetSessionPassword"],
                SafeNetSession.CallerTypeBO);
        }

        /// <summary>
        /// Décryptage d'un cyptogramme
        /// Cette fonction est utilisée par le BackOffice
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="EncryptedPAN">Numéro de carte crypté</param>
        /// <returns>Numéro de carte en clair</returns> 
        public static string DecryptBOCard(UserInfo user, string EncryptedPAN)
        {
            // On décrypte le PAN encrypté et returne le PAN en clair
            return DecryptData(user, EncryptedPAN,
                ConfigurationManager.AppSettings["SafeNetSessionUsername"],
                ConfigurationManager.AppSettings["SafeNetSessionPassword"],
                SafeNetSession.CallerTypeBO);
        }

        /// <summary>
        /// Retourne la carte utilisée
        /// par un voyageur dans Navision
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="reader">Paramètres appel</param>
        /// <param name="failIfNoCardFound">Flag fail if no card was found</param>
        /// <returns>Informations sur la carte</returns>
        public static UserBookingPaymentResponse GetUserBookingPayment(UserInfo user
            , UserBookingPaymentReader reader, bool failIfNoCardFound)
        {
            UserBookingPaymentResponse retval = new UserBookingPaymentResponse(reader.GetInputString());
            try
            {
                // Extraction des paramètres
                string customer = reader.GetComCode();
                string cc1 = reader.GetCc1();
                string traveller = reader.GetPerCode();
                string service = reader.GetService();
                string token = reader.GetToken();
                string pos = reader.GetPos();

                if (String.IsNullOrEmpty(customer) || String.IsNullOrEmpty(traveller))
                {
                    // Il faut obligatoirement les deux valeurs (le comcode ET le percode)
                    throw new Exception(user.GetMessages().GetString("ComcodeAndPercodeEmpty", true));
                }

                if (String.IsNullOrEmpty(token))
                {
                    // Aucun token envoyé par le client
                    // On a extrait toutes les informations nécéssaires
                    // On va retourner les informations depuis Navision
                    // Token compris
                    UserBookingPaymentRSResult rs = GetUserBookingPaymentRS(user, pos, customer, cc1, traveller, service);

                    if (rs.IsError())
                    {
                        // Navision a renvoyé une erreur
                        // Il faut la prendre en compte
                        throw new Exception(rs.GetErrorMsg());
                    }

                    if (!Util.IsEmptyToken(rs.GetToken()))
                    {
                        // Navision nous a renvoyé toutes les informations relatives
                        // à la carte mais sans le PAN!
                        // CAR NAVISION NE CONNAIT PAS LE NUMERO DE LA CARTE EN CLAIR
                        // On va maintenant récupérer le PAN à partir du token
                        string PAN = GetPanFromBOToken(user, rs.GetToken());


                        // On a tout ce qui est nécéssaire
                        // On peut répondre au client
                        retval.SetValues(user, PAN, rs);
                    }
                    else
                    {
                        // Aucune carte n'a été trouvé dans Navision
                        // lors de la recherche
                        if (failIfNoCardFound)
                        {
                            throw new Exception(user.GetMessages().GetString("UserBookingPaymentResponse.NoPaymentCardFound", traveller, customer, cc1, service, true));
                        }
                    }

                }
                else
                {
                    // Le client nous a envoyé un token
                    // BO ou FO
                    switch (ArgsChecker.GuessTokenType(user, token))
                    {
                        case ArgsChecker.TOKEN_BO:
                            // Il s'agit donc d'un token BackOffice
                            // Tout d'abord, on récupère le token
                            long TokenBO = Util.ConvertStringToToken(token);

                            // On a récupéré le token
                            // On va maintenant essayer de récupérer les
                            // informations complémentaires (date expiration, cvc, ...) depuis Navision
                            retval.SetValues(user, service, GetExtendedPanInfos(user, pos, TokenBO));
                            break;
                        default:
                            // Nous devons nous baser sur ce token pour
                            // effectuer la recherche
                            // Aucune connexion ne sera effectuer côté Navision
                            retval.SetValues(user, service, GetPanInfosFromFOToken(user, token, false));
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                // Gérer cette exception
                throw new Exception(e.Message);
            }
            return retval;
        }


        /// <summary>
        /// Cette fonction permet de vérifier la validité d'une carte 
        /// via la service RBS (BIBIT)
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="inputValue">inputValue (Token BO, FO ou numéro de carte)</param>
        /// <param name="expirationDate">Date d'expiration</param>
        /// <param name="cardHolder">Détenteur de la carte</param>
        /// <param name="pos">pos</param>
        /// <param name="timeOut">TimeOut (s)</param>
        /// <param name="onlineCheck">Online check activated</param>
        /// <param name="lodgedCard">Lodged card TRUe or FALSE</param>
        /// <returns>Information validité carte</returns>
        public static CreditCardValidationResponse ValidateCreditCard(UserInfo user, string inputValue,
             string expirationDate, string cardHolder, string pos, string timeOut, bool onlineCheck, bool lodgedCard)
        {
            // On prépare la réponse que l'on va
            // apporter (initialisation)
            CreditCardValidationResponse Response = new CreditCardValidationResponse(inputValue, false);

            try
            {
                // Vérifions si une valeur a été renseignée
                if (String.IsNullOrEmpty(inputValue))
                {
                    throw new Exception(user.GetMessages().GetString("CreditCardValidationResponse.InputValueEmpty", true));
                }


                // Le client nous a envoyé un token
                // BO, FO ou un numéro de carte
                // La date d'expiration doit être spécifiée
                if (String.IsNullOrEmpty(expirationDate))
                {
                    throw new Exception(user.GetMessages().GetString("ExpirationDateEmpty", true));
                }
                if (onlineCheck)
                {
                    // Pour les contrôles via des APIs externes
                    // comme RBS
                    // Nous devons avoir un POS
                    if (String.IsNullOrEmpty(pos))
                    {
                        throw new Exception(user.GetMessages().GetString("PosEmpty", true));
                    }
                }

                // La première étape va consister à retourner le PAN depuis le token
                // ou le token depuis le PAN
                string Pan = null;
                string Token = null;
                switch (ArgsChecker.GuessTokenType(user, inputValue))
                {
                    case ArgsChecker.TOKEN_FO:
                        // Nous devons nous baser sur ce token pour
                        // récupérer le pan correspondant
                        Token = inputValue;
                        Pan = Services.GetPanInfosFromFOToken(user, Token, false).GetPAN();
                        break;
                    case ArgsChecker.TOKEN_BO:
                        // Il s'agit donc d'un token BackOffice
                        Token = inputValue;
                        try
                        {
                            // On va continuer et récupérer le Pan correspondant
                            Pan = Services.GetPanFromBOToken(user, Util.ConvertStringToToken(inputValue));
                        }
                        catch (Exception e)
                        {
                            // Impossible de récupérer le PAN depuis le token
                            // avant de considérer qu'il s'agit d'une réelle erreur
                            // nous devons considérer qu'il s'agit peut-être d'un PAN
                            if (e.Message.Contains(CCEExceptionUtil.EXCEPTION_CODE_TAG_OPEN + "BO_TOKEN_UNKNOWN" + CCEExceptionUtil.EXCEPTION_CODE_TAG_CLOSE))
                            {
                                // On ne connait pas ce token
                                // peut-être qu'il s'agit d'un PAN
                                Token = null;
                                Pan = inputValue;
                            }
                            else
                            {
                                throw new Exception(e.Message);
                            }
                        }
                        break;
                    default:
                        // Il ne s'agit pas de token
                        // probablement d'un numéro de carte
                        Pan = inputValue;
                        break;
                }

                // On a le pan

                // On va maintenant valider cette carte
                // via le service en ligne RBS (BIBIT) si nécéssaire
                // Si la carte est rejetée par RBS, alors une exception sera levée
                // Validation de la date d'expiration
                DateTime ExpirationDate = Util.ConvertExpirationDateToDate(user, expirationDate);

                // let's validate credit card number
                CardInfos ci = CheckCreditCard(user, Pan, ExpirationDate, cardHolder, pos, timeOut, onlineCheck, true,
                    null, null, null, Token, lodgedCard ? 1 : 0, string.Empty);

                // On a le statut de la carte
                Response.SetValues(user, Token, Pan, ci.GetCardType(), Const.StatusAuthorised, ci.GetInformationCode(),
                    ci.GetInformationMessage());
            }
            catch (Exception e)
            {
                // Il faut retourner cette exception
                throw new Exception(e.Message);
            }
            return Response;
        }


        /// <summary>
        /// Insertion des cartes rejetées lors du processus de
        /// validation des cartes via le service RBS
        /// 
        /// On ne doit pas effectuer ce processus à tout prix!
        /// Les rejets carte dans la table n'est pas une chose à faire
        /// à n'importe quel prix!
        /// On ignore cette erreur et on envoi un mail au support BackOffice
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="card">Informations carte</param>
        /// <param name="orderCode">Numéro de la transaction BIBIT</param>
        /// <param name="status">Time out, error</param>
        /// <param name="error">Description complète de l'erreur</param>
        /// <param name="completeResponse">Reponse complète</param>
        public static void LogRejectedCreditCard(UserInfo user, CardInfos card, string orderCode, string status,
            string error, string completeResponse)
        {
            CreditCardLogConnection conn = null;
            try
            {
                // On définit une nouvelle connexion
                // vers la base des données encryptées
                conn = new CreditCardLogConnection(user);

                // On se connecte à la base de données
                conn.Open();

                // Nous allons enregistrer l'erreur
                // sur cette carte
                conn.LogCard(card, orderCode, status, error, completeResponse);
            }
            catch (Exception)
            {
                // On doit ignorer cette erreur car l'insertion
                // des rejets carte dans la table n'est pas une chose à faire
                // à n'importe quel prix!
                // On ignore cette erreur 
            }
            finally
            {
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette erreur
                }
            }

        }


        /// <summary>
        /// Mise en cache de la réponse de RBS
        /// Si une carte est valide un instant précis
        /// alors on garde cet état pendant 24 heures
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="encryptedPan">Cryptogramme du Pan</param>
        public static void CacheBibitResponseStatus(UserInfo user, string encryptedPan)
        {
            EncryptedDataConnection conn = null;
            try
            {
                // On définie une nouvelle connexion vers Navision
                conn = new EncryptedDataConnection(user);

                // On se connecte à la base de données
                conn.Open();

                // Cette entrée existe déjà dans la table?
                CachedValidationResult status = conn.GetCachedBibitResponseStatus(encryptedPan);
                switch (status.GetStatus())
                {
                    case CachedValidationResult.CacheStatus.NotFound:
                        // Le statut est introuvable, on insère
                        conn.InsertBibitResponseStatus(encryptedPan);
                        break;
                    case CachedValidationResult.CacheStatus.FoundExpired:
                        // On a trouvé une entrée, mais ancienne
                        // On met à jour
                        conn.UpdateBibitResponseStatus(encryptedPan);
                        break;
                    default: break;
                }
            }
            catch (Exception)
            {
                // On redirige cette exception
                //throw new Exception(e.Message);


                // On doit ignorer cette erreur car la mise en cache
                // n'est pas une chose à faire à n'importe quel prix!
                // on doit laisser le processus se poursuivre
                // On ignore cette erreur
            }
            finally
            {
                // On se deconnecte
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
            }

        }


        /// <summary>
        /// Vérification si la carte a déjà été validée
        /// il y a moins de 24 heures
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="encryptedPan">Cryptogramme</param>
        /// <returns>TRUE ou FALSE</returns>
        public static CachedValidationResult GetCachedBibitResponse(UserInfo user, string encryptedPan)
        {
            CachedValidationResult retval = new CachedValidationResult();
            EncryptedDataConnection conn = null;
            try
            {
                // On définie une nouvelle connexion vers Navision
                conn = new EncryptedDataConnection(user);

                // On se connecte à la base de données
                conn.Open();

                // Cette entrée existe déjà dans la table?
                retval = conn.GetCachedBibitResponseStatus(encryptedPan);
            }
            catch (Exception)
            {
                // On redirige cette exception
                //throw new Exception(e.Message);

                // On doit ignorer cette erreur car la mise en cache
                // n'est pas une chose à faire à n'importe quel prix!
                // on doit laisser le processus se poursuivre
                // On ignore cette erreur
            }
            finally
            {
                // On se deconnecte
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
            }
            return retval;
        }

        /// <summary>
        /// Mise en cache de la réponse de RBS
        /// Si une carte est valide un instant précis
        /// alors on garde cet état pendant 24 heures
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="encryptedPan">Cryptogramme du Pan</param>
        public static void CacheFOBibitResponseStatus(UserInfo user, string encryptedPan)
        {
            EncryptedFODataConnection conn = null;
            try
            {
                // On définie une nouvelle connexion vers Navision
                conn = new EncryptedFODataConnection(user);

                // On se connecte à la base de données
                conn.Open();

                // Cette entrée existe déjà dans la table?
                CachedValidationResult status = conn.GetCachedBibitResponseStatus(encryptedPan);
                switch (status.GetStatus())
                {
                    case CachedValidationResult.CacheStatus.NotFound:
                        // Le statut est introuvable, on insère
                        conn.InsertBibitResponseStatus(encryptedPan);
                        break;
                    case CachedValidationResult.CacheStatus.FoundExpired:
                        // On a trouvé une entrée, mais ancienne
                        // On met à jour
                        conn.UpdateBibitResponseStatus(encryptedPan);
                        break;
                    default: break;
                }
            }
            catch (Exception)
            {
                // On doit ignorer cette erreur car la mise en cache
                // n'est pas une chose à faire à n'importe quel prix!
                // on doit laisser le processus se poursuivre
                // On ignore cette erreur 
            }
            finally
            {
                // On se deconnecte
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
            }

        }


        /// <summary>
        /// Vérification si la carte a déjà été validée
        /// il y a moins de 24 heures
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="encryptedPan">Cryptogramme</param>
        /// <returns>TRUE ou FALSE</returns>
        public static CachedValidationResult GetCachedFOBibitResponse(UserInfo user, string encryptedPan)
        {
            CachedValidationResult retval = new CachedValidationResult();
            EncryptedFODataConnection conn = null;
            try
            {
                // On définie une nouvelle connexion vers Navision
                conn = new EncryptedFODataConnection(user);

                // On se connecte à la base de données
                conn.Open();

                // Cette entrée existe déjà dans la table?
                retval = conn.GetCachedBibitResponseStatus(encryptedPan);

            }
            catch (Exception)
            {

                // On doit ignorer cette erreur car la mise en cache
                // n'est pas une chose à faire à n'importe quel prix!
                // on doit laisser le processus se poursuivre
                // On ignore cette erreur
            }
            finally
            {
                // On se deconnecte
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
            }
            return retval;
        }



        /// <summary>
        /// Génération d'un ID VPayment
        /// et insertion dans la table historique
        /// </summary>
        /// <param name="response">Informations sur la reponse</param>
        /// <returns>ID VPayment</returns>
        public static string GenerateVPaymentID(VPaymentIDResponse response)
        {
            // On va générer un ID
            string id = Util.GenerateVPaymentID();

            // We need now to get the point of sale
            string pos = null;
            // the value is not located in the same place
            // depending of the product
            switch (response.GetBookingType())
            {
                case VPaymentIDResponse.BookingTypeHotel:
                    pos = response.GetHotelArguments().GetPOS();
                    break;
                case VPaymentIDResponse.BookingTypeLC:
                    pos = response.GetLCArguments().GetPOS();
                    break;
                default: break;
            }

            // On a l'ID
            // On va maintenant l'insérer dans la table historique
            NavisionDbConnection conn = null;

            try
            {
                // on va se connecter à la base navision et y insérer l'id
                conn = new NavisionDbConnection(response.GetUser(), pos);
                // On ouvre la connexion
                conn.Open();
                // on insère l'id
                switch (response.GetBookingType())
                {
                    case VPaymentIDResponse.BookingTypeHotel:
                        conn.InsertVPaymentIDForHotel(response.GetUser(), id, response.GetHotelArguments());
                        break;
                    case VPaymentIDResponse.BookingTypeLC:
                        // Avant d'effectuer l'insertion, nous devons vérifier
                        // que le vendor lié à cette companie low cost supporte les VPayments
                        conn.CheckVPaymentForCorporation(response.GetLCArguments().GetCompany());
                        // On peut utiliser VPayment pour cette corporation
                        conn.InsertVPaymentIDForLC(response.GetUser(), id, response.GetLCArguments());
                        break;
                    default: break;
                }
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }


            return id;
        }


        /// <summary>
        /// Vérification si un ID VPayment
        /// est valide
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="id">ID VPayment</param>
        public static VPaymentIDValidationResponse ValidateVPaymentID(UserInfo user, string id)
        {

            VPaymentIDValidationResponse retval = new VPaymentIDValidationResponse(id);
            retval.SetUser(user);

            try
            {
                // On vérifie si l'ID est valide structurellement
                ArgsChecker.ValidateVPaymentID(user, id);

            }
            catch (Exception e)
            {
                // L'ID est invalide
                retval.SetRefusalReason(CCEExceptionUtil.GetExceptionCode(e.Message), CCEExceptionUtil.GetExceptionOnlyMessage(e.Message));
                return retval;
            }

            // L'ID est valide structurellement
            // On va allez voir s'il s'agit bien d'un ID généré
            // ie présent dans la table historique
            VPaymentIDData IdHist = GetVPaymentInfos(user, id);
            if (IdHist == null)
            {
                // L'ID est introuvable!
                retval.SetRefusalReason("UNKNOWN_ID",
                    user.GetMessages().GetString("Services.ValidateVPaymentID.UnknownID", id, false));
                return retval;
            }

            // L'Id est valide
            retval.SetValid(true);
            retval.SetIDInformation(IdHist);

            return retval;
        }

        /// <summary>
        /// Récupération des informations sur un ID VPayment
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="id">ID Vpayment</param>
        /// <returns>Informations sur l'id</returns>
        private static VPaymentIDData GetVPaymentInfos(UserInfo user, string id)
        {
            VPaymentIDData IdHist = null;
            NavisionDbConnection conn = null;
            try
            {
                // Pour cela nous allons ouvrir une nouvelle connexion
                conn = new NavisionDbConnection(user, Const.PosFrance);
                // On va l'ouvrir
                conn.Open();
                // et récupérer des information sur cet ID
                // Si la fonction retourne nulle, alors l'ID est introuvable
                IdHist = conn.GetVPaymentInfos(user, id);
            }
            finally
            {
                // On n'oubli pas de fermer la connexion
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return IdHist;
        }



        /// <summary>
        /// Cette fonction permet d'étendre les traces écrites dans Syslog
        /// </summary>
        /// <param name="user">Compte client</param>
        /// <param name="replyInfo">Information à enrichir</param>
        /// <param name="successResponse">Information à ajouter si succès</param>
        /// <param name="failedResponse">Information à ajouter si échec</param>
        /// <param name="isError">Indicateur erreur</param>
        /// <param name="duration">Durée de traitement de la réponse (ms)</param>
        public static void WriteOperationStatusToLog(UserInfo user, string replyInfo,
            string successResponse, string failedResponse, bool isError, string duration)
        {
            // On va ajouter les informations sur le client
            string richResponse = user != null ? user.GetLogMessageBeforeReply() : Const.EmptyUserLog;

            if (!String.IsNullOrEmpty(replyInfo))
            {
                // Ajoutons des informations
                // sur le client et l'application qui a été appelée
                richResponse += replyInfo;
            }

            // On ajoute le temps de traitement (en ms)
            richResponse += String.Format(Const.Log_Duration, duration);

            if (isError)
            {
                // Le traitement a rencontré un problème
                // Impossible de satisfaire la demande du client
                // il faut logger cet échec
                richResponse += failedResponse;
                richResponse += Const.Status_Failed;
                Logger.WriteWarningToLog(richResponse);
            }
            else
            {
                // Tout s'est bien passé, 
                // Nous allons satisfaire la demande
                // du client et loggé ce succès
                richResponse += successResponse;
                richResponse += Const.Status_Success;
                Logger.WriteInformationToLog(richResponse);
            }
        }

        /// <summary>
        /// Display information in the log
        /// </summary>
        /// <param name="user">user</param>
        /// <param name="message">message to display</param>
        public static void WriteOperationStatusToLog(UserInfo user, string message)
        {
            // On va ajouter les informations sur le client
            string richResponse = user != null ? user.GetLogMessageBeforeReply() : Const.EmptyUserLog;

            // On ajoute le temps de traitement (en ms)
            richResponse += message + Const.Status_Info;

            Logger.WriteInformationToLog(richResponse);    
        }

        /// <summary>
        /// Cette méthode supprime un enregistrement unique
        /// (customer, traveler, card reference)
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pos">Marché</param>
        /// <param name="customer">Code client</param>
        /// <param name="cc1Id">Cost center id</param>
        /// <param name="traveler">Code du voyageur</param>
        /// <param name="cardReference">Référence de la carte</param>
        public static void DeleteProfilCard(UserInfo user, string pos, string customer, string cc1Id, string traveler, string service)
        {
            NavisionDbConnection conn = null;

            try
            {
                // Quelques contrôles avant d'aller plus loin
                // Le code du client doit être renseigné
                if (String.IsNullOrEmpty(customer)) throw new Exception(user.GetMessages().GetString("DeleteCardInNavision.EmptyCustomer", true));

                // Check service 
                int NavServiceGroup = Util.GetNavisionServiceGroup(user, service);

                String cc1 = string.Empty;
                if (String.IsNullOrEmpty(cc1Id))
                {
                    // First, we need to return the Cost center label from cost center id
                    cc1 = Services.GetCostCenter1ValueFromId(user, pos, customer, cc1Id);
                }

                // Ok nous avons tout ce qu'il nous faut
                // nous allons nous connecter à la base de donnée et supprimer cette carte
                // On commence par ouvrir une nouvelle connexion
                conn = new NavisionDbConnection(user, pos);

                // On va ouvrir la connexion
                conn.Open();
                // La connexion est ouverte
                // nous pouvons effectuer notre action de suppression
                conn.DeleteProfilCard(customer, cc1, traveler, NavServiceGroup);
            }
            finally
            {
                // On n'oubli pas de fermer la connexion
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }


        /// <summary>
        /// Récupération des informations carte
        /// pour un voyageur
        /// La carte peut être au niveau voyageur/ centre de cout/ company
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="comcode">Code de la companie</param>
        /// <param name="cc1">Centre de cout 1</param>
        /// <param name="percode">Code du voyageur</param>
        /// <param name="service">Service sur lequel la carte doit être récupérée</param>
        /// <param name="response">Les informations de carte</param>
        public static UserBookingPaymentResponse GetTravelerPaymentCard(UserInfo user, string poscode,
            string comcode,
            string cc1, string percode, string service)
        {
            // Le paiement se fait par carte
            // Nous devons retourner les informations sur la carte
            UserBookingPaymentReader reader = new UserBookingPaymentReader(user.GetMessages().GetLang(),
                poscode, comcode, cc1,
                percode, service);

            // on peut maintenant rechercher la carte
            // Ne pas échouer si une carte n'est pas trouvée
            return Services.GetUserBookingPayment(user, reader, false);
        }


        /// <summary>
        /// Retourne une session Ingrian standard
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <returns>Session SafeNetSession</returns>
        private static SafeNetSession GetStandardBOSafeNetSession(UserInfo user)
        {
            return new SafeNetSession(user, ConfigurationManager.AppSettings["SafeNetSessionUsername"],
                    EncDec.DecryptPassword(ConfigurationManager.AppSettings["SafeNetSessionPassword"]),
                    SafeNetSession.CallerTypeBO);
        }

        /// <summary>
        /// Retourne une session Ingrian standard
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <returns>Session SafeNetSession</returns>
        private static SafeNetSession GetStandardFOSafeNetSession(UserInfo user)
        {
            return new SafeNetSession(user, ConfigurationManager.AppSettings["SafeNetSessionFOUsername"],
                    EncDec.DecryptPassword(ConfigurationManager.AppSettings["SafeNetSessionFOPassword"]),
                    SafeNetSession.CallerTypeFO);
        }
     

        /// <summary>
        /// Insertion des cartes  Egencia dans
        /// la base de données de cartes encryptées
        /// Le PAN, la date d'expiration et le CSC
        /// seront vérifiés et s'ils sont conformes
        /// alors le PAN et CSC cryptés sertont insérés
        /// dans la base et un token sera retourné
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pan">Numéro de carte</param>
        /// <param name="CSC">Code de sécurité</param>
        /// <param name="expirationDate">Date d'expiration</param>
        /// <param name="pos">Marché</param>
        /// <param name="cardHolder">Détenteur de la carte</param>
        /// <returns>Informations d'insertion</returns>
        public static InsertEgenciaCardInEncryptedDBResult InsertEgenciaCardInEncryptedFODB(UserInfo user, string pan, string CSC,
            string expirationDate, string cardHolder, string pos)
        {

            // La date d'expiration doit être spécifiée
            if (String.IsNullOrEmpty(expirationDate))
            {
                // Pas de date d'expiration, on ne peut pas aller plus loin
                throw new Exception(user.GetMessages().GetString("ExpirationDateEmpty", true));
            }


            // Le CSC doit être spécifié
            if (String.IsNullOrEmpty(CSC))
            {
                // Pas de CSC, on ne peut pas aller plus loin
                throw new Exception(user.GetMessages().GetString("CSCEmpty", true));
            }

            // Pour les contrôles Bibit
            // Nous devons avoir un POS
            if (String.IsNullOrEmpty(pos))
            {
                // Pas de POS, on arrête là
                throw new Exception(user.GetMessages().GetString("PosEmpty", true));
            }

            // On récupère la date d'expiration en date 
            DateTime DateExpirationDate = Util.ConvertExpirationDateToDate(user, expirationDate);

            // On vérifier le numéro de carte (test de Luhn)
            // et date d'expiration
            // plus vérification en ligne
            CardInfos ci = CheckCreditCard(user, pan, DateExpirationDate, cardHolder, pos, HttpUtil.NoTimeOutString
                //>>EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check 
                //, true, true, null, null, null, null, false, 0);
                , true, true, null, null, null, null, 0, string.Empty);
            //<<EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check 

            // On vérifie le CSC
            CreditCardVerifier.IsValidCSC(user, ci, CSC);

            // On prépare la réponse à apporter
            InsertEgenciaCardInEncryptedDBResult Result = new InsertEgenciaCardInEncryptedDBResult();

            // La carte est valide
            // On récupère quelques infos intéressantes
            Result.SetValues(ci, CSC);

            // On va maintenant insérer le numéro de carte encrypté
            // dans la base des données encryptées
            // hébergée par FrontOffice
            EncryptedFODataConnection Conn = null;
            try
            {
                // On encrypte le PAN ...
                // mais avant, vérifions si nous ne l'avons pas fait déjà
                // lors de l'éventuelle validation BIBIT
                string encryptedPAN = ci.GetEncryptedPan();
                if (encryptedPAN == null)
                {
                    // Nous avons besoin de l'encrypter
                    encryptedPAN = EncryptFOCard(user, pan);
                }

                // On définit une nouvelle connexion
                // vers la base des données encryptées
                Conn = new EncryptedFODataConnection(user);

                // On se connecte à la base de données
                Conn.Open();

                // On vérifie si la carte est déjà enregistrée dans la base
                // On compare le cryptogramme avec ceux enregistrés dans la base
                // Si la fonction renvoit un Token
                // alors la carte existe dans la base de données
                // autrement la carte n'existe pas encore
                EgenciaCardTokenResult TokenStored = Conn.GetEgenciaCardToken(encryptedPAN);
                string Token = TokenStored.GetToken();

                if (!TokenStored.isFound())
                {
                    // C'est une nouvelle carte
                    // On va encrypter le CSC
                    string encryptedCSC = EncryptFOCard(user, CSC);

                    // La carte n'existe pas!
                    // On insère et on récupére le token
                    Token = Conn.InsertEgenciaEncryptedCard(encryptedPAN, encryptedCSC);
                }

                // On a le token, on le retourne
                Result.SetToken(Token);
            }
            catch (Exception e)
            {
                throw new Exception(user.GetMessages().GetString("Services.InsertCardInEncryptedDB.Error", e.Message, true));
            }
            finally
            {
                // On se deconnecte
                if (Conn != null)
                {
                    try
                    {
                        Conn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
            }

            return Result;
        }


        /// <summary>
        /// Retourne une carte Egencia (pan, csc) depuis un token
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="token">Token</param>
        /// <returns>Carte Egencia</returns>
        public static EgenciaPanInfoResult GetPanInfosFromEgenciaToken(UserInfo user, string token)
        {
            // On initialise la réponse
            EgenciaPanInfoResult result = new EgenciaPanInfoResult();

            // On défini une connexion à la base FO
            EncryptedFODataConnection FOConn = null;

            try
            {
                // Initialisation d'une nouvelle connexion
                FOConn = new EncryptedFODataConnection(user);

                // On va se connecter à la base des cartes cryptées
                // hébergée par FrontOffice
                FOConn.Open();

                // On lance la recherche pour le token
                EgenciaEncryptedPanInfoResult FOEncryptedPAN = FOConn.GetEgenciaEncryptedPANAndCSC(token);

                // On a le retour de l'appel
                if (!FOEncryptedPAN.IsTokenExists())
                {
                    // Ce token est introuvable!
                    // On ne peut plus continuer.
                    throw new Exception(user.GetMessages().GetString("Services.GetPanFromEgenciaToken.EgenciaTokenUnknow", token, true));
                }

                // Il faut récupérer le cryptogramme du PAN
                string encryptedPAN = FOEncryptedPAN.GetEncryptedPAN();

                // On récupère le CSC encrypté
                string encryptedCSC = FOEncryptedPAN.GetEncryptedCSC();

                // On a besoin de retourner toutes les informations
                // et pas uniquement le Token BackOffice
                // On a le cryptogramme, on peut donc
                // solliciter un décryptage
                string pan = DecryptBOCard(user, encryptedPAN);

                // On décrypte le CSC
                string csc = DecryptBOCard(user, encryptedCSC);

                // On a tout ce qu'il nous faut
                // On va préparer la réponse renvoyée au client
                // et en profiter pour valider la carte
                // afin de retourner les informations relatives au type de carte
                result.SetValues(token, pan, csc, Services.CheckCreditCard(user, pan));

            }
            catch (Exception e)
            {
                throw new Exception(user.GetMessages().GetString("Services.GetPanFromFOToken.Error", token, e.Message, true));
            }
            finally
            {
                // On se deconnecte de la base FO
                if (FOConn != null)
                {
                    try
                    {
                        FOConn.Close();
                    }
                    catch (Exception) { }; // On ignore cette exception
                }
            }


            return result;
        }

        /// <summary>
        /// Cette méthode retourne le type de paiement pour une companie 
        /// aérienne
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pos">Marché</param>
        /// <param name="corporationCode">Code de la compagnie</param>
        /// <returns>Type de paiement</returns>
        public static string GetGDSPaymentTypeForCorporation(UserInfo user, string pos, string corporationCode)
        {
            // Nous devons avoir un POS
            if (String.IsNullOrEmpty(pos))
            {
                throw new Exception(user.GetMessages().GetString("PosEmpty", true));
            }

            // Nous devons avoir un code companie
            if (String.IsNullOrEmpty(corporationCode))
            {
                throw new Exception(user.GetMessages().GetString("CorporationEmpty", true));
            }

            // Nous allons maintenant nous connecter à la base de données
            // pour extraire le type de paiement depuis la compagnie aérienne
            NavisionDbConnection conn = null;

            try
            {

                // Pour cela nous allons définir une nouvelle connexion
                conn = new NavisionDbConnection(user, pos);

                // On va l'ouvrir
                conn.Open();

                // et récupérer le type de paiement
                return conn.GetGDSPaymentTypeForCorporation(corporationCode);
            }
            finally
            {
                // On n'oubli pas de fermer la connexion
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Cette méthode retourne le type de paiement pour un client 
        /// 
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="pos">Marché</param>
        /// <param name="corporationCode">Code du client</param>
        /// <returns>Type de paiement</returns>
        public static string GetGDSPaymentTypeForCustomer(UserInfo user, string pos, string customerCode)
        {
            // Nous devons avoir un POS
            if (String.IsNullOrEmpty(pos))
            {
                throw new Exception(user.GetMessages().GetString("PosEmpty", true));
            }

            // Nous devons avoir un code client
            if (String.IsNullOrEmpty(customerCode))
            {
                throw new Exception(user.GetMessages().GetString("ComcodeEmpty", true));
            }

            // Nous allons maintenant nous connecter à la base de données
            // pour extraire le type de paiement depuis la compagnie aérienne
            NavisionDbConnection conn = null;

            try
            {

                // Pour cela nous allons définir une nouvelle connexion
                conn = new NavisionDbConnection(user, pos);

                // On va l'ouvrir
                conn.Open();

                // et récupérer le type de paiement
                return conn.GetGDSPaymentTypeForCustomer(customerCode);
            }
            finally
            {
                // On n'oubli pas de fermer la connexion
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Save ENC access information in the database
        /// First we will check if the requestor ECN is already in the database
        /// </summary>
        /// <param name="user">Client username</param>
        /// <param name="reader">ENett ECN Requestor Access</param>
        public static void SaveENettECN(UserInfo user, ENettECNRequestorAccess reader)
        {
            EncryptedFODataConnection conn = null;
            try
            {
                // Define a new connection
                conn = new EncryptedFODataConnection(user);

                // connect ...
                conn.Open();

                // First we need to check if the ECN already exist
                if (conn.CheckENettECN(reader.GetRequestorECN()))
                {
                    // We already know this ECN
                    throw new Exception(user.GetMessages().GetString("Services.SaveENettECN.Error", reader.GetRequestorECN(), true));
                }

                // This is a new ECN, we need to save it in the database
                conn.InsertENettECNDetails(reader);
            }
            finally
            {
                // Let's disconnect
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // We will ignore this error
                }
            }
        }


        /// <summary>
        /// Delete ENC access information from the database
        /// </summary>
        /// <param name="user">Client username</param>
        /// <param name="requestorECN">requestor ECN</param>
        public static void DeleteENettECNDetails(UserInfo user, int requestorECN)
        {
            EncryptedFODataConnection conn = null;
            try
            {
                // Define a new connection
                conn = new EncryptedFODataConnection(user);

                // connect ...
                conn.Open();

                // Let's check if the requestor ECN access information
                if (!conn.CheckENettECN(requestorECN))
                {
                    // The ECN in unknown!
                    throw new Exception(user.GetMessages().GetString("Services.GetENettECN.UnknowECN", requestorECN, true));
                }

                // This is an existinf ECN, we can delete it now
                conn.DeleteENettECNDetails(requestorECN);
            }
            finally
            {
                // Let's disconnect
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // We will ignore this error
                }
            }
        }



        /// <summary>
        /// Request an ENett Virtual Account Number (VAN)
        /// This method will build a request and send to ENett request VAn API
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="reader">ENettVANRequestReader</param>
        /// <returns>AmendVNettVANResponse</returns>
        public static CompleteIssueVNettVANResponse GetNewENettVAN(UserInfo user, ENettRequestVAN reader)
        {
            vNettService client = null;
            CompleteIssueVNettVANResponse retval = new CompleteIssueVNettVANResponse();
            try
            {

                // Set service point manager
                EnettUtils.SetServicePointManager();

                // Define a new vNett service connection
                client = new vNettService();

                // Let's request a new VAN
                // prepare the request
                CompleteIssueVNettVANRequest amdReq = EnettUtils.GetENettVANRequest(user, reader);
                // keep reference id
                // we need to return it to caller
                retval.SetReferenceId(amdReq.GetReferenceId());
                // and send to the API
                retval.SetIssuedVNettResponse(client.IssueVAN(amdReq.GetIssuedVNettRequest()));

                // we need to log this request
                LogENettRequestVAN(user, reader, retval);

                return retval;

            }
            finally
            {
                // dispose
                if (client != null) client.Dispose();
            }
        }


        /// <summary>
        /// Get eNett VAN details
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="reader">ENettGetVANDetails</param>
        /// <returns>GetVNettVANResponse</returns>
        public static GetVNettVANResponse GetENettVANDetails(UserInfo user, ENettGetVANDetails reader)
        {
            vNettService client = null;
            GetVNettVANResponse retval = new GetVNettVANResponse();

            try
            {
                // Set service point manager
                EnettUtils.SetServicePointManager();

                // Define a new vNett service connection
                client = new vNettService();

                // Let's get details for that VAN
                // prepare the request
                GetVNettVANRequest amdReq = EnettUtils.GetENettVANDetails(user, reader);

                // and send to the API and return result
                retval = client.GetVANDetails(amdReq);

                //  we need to log this request
                //LogVNettAmendAN(user, reader, retval);

                return retval;
            }
            finally
            {
                // dispose
                if (client != null) client.Dispose();
            }
        }

        // Begin EGE-85532
        /// <summary>
        /// Cancel an ENett Virtual Account Number (VAN)
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="reader">ENettCancelRequestVAN</param>
        /// <returns>CancelVNettVANResponse</returns>
        public static CancelVNettVANResponse CancelVAN(UserInfo user, ENettCancelRequestVAN reader)
        {
            vNettService client = null;
            CancelVNettVANResponse retval = new CancelVNettVANResponse();

            try
            {
                // Set service point manager
                EnettUtils.SetServicePointManager();

                // Define a new vNett service connection
                client = new vNettService();

                // Let's get details for Cancel VAN
                // prepare the  cancel request
                CancelVNettVANRequest CancelReq = EnettUtils.GetCancelVANRequest(user, reader);

                // and send to the API and return result
                retval = client.CancelVAN(CancelReq);

                //  we need to log this request

                // LogVNettCancelVAN(reader, retval); for now there is no required in future might be use for log cancel van details in vcardbooking table.

                return retval;
            }
            finally
            {
                // dispose
                if (client != null) client.Dispose();
            }
        }
        // END EGE-85532

        /// <summary>
        /// Amend an ENett Virtual Account Number (VAN)
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="reader">ENettAmendVAN</param>
        /// <returns>AmendVNettVANResponse</returns>
        public static AmendVNettVANResponse AmendENettVAN(UserInfo user, ENettAmendVAN reader)
        {
            vNettService client = null;
            AmendVNettVANResponse retval = new AmendVNettVANResponse();

            try
            {
                // Set service point manager
                EnettUtils.SetServicePointManager();

                // Define a new vNett service connection
                client = new vNettService();

                // Let's amend VAN
                // prepare the request
                AmendVNettVANRequest amdReq = EnettUtils.ENettAmendVAN(user, reader);
                //Check validation

                //Start EGE -100347 
                // retval1 is getvan response
                // amdReq1 are getvan request 
                // Checking each van details before amending. 
                // After Fetching van details checking van history collection, In the collection checking validation van activity type is close or not. 
                // After validating van activity (Status), if it is false then van request inforamtion will amended and logged. 
                // If staus is true then request will not call vcs service. just by pass and set the values supportlogId "D".
                // This "D" will help on setting values to output amendvan response as per expected. 
             
                GetVNettVANResponse retval1 = new GetVNettVANResponse();
                GetVNettVANRequest amdReq1 = EnettUtils.AmendGetENettVANDetails(user, reader.PaymentID);

                retval1 = client.GetVANDetails(amdReq1);
                Boolean Status = EnettUtils.CheckStatus(retval1);

                if (!Status)
                {
                    retval = client.AmendVAN(amdReq);
                    //  we need to log this request
                    LogVNettAmendAN(user, reader, retval);
                    return retval;
                }
                else
                {
                   //Setting Duplicate value with "D" and assigning getvandetails of retval1 to main response.
                    retval.SupportLogId = "D";
                    retval.VNettTransactionID = retval1.VNettTransactionID;
                   // retval.
                   // LogVNettAmendAN(user, reader, retval);
                    return retval;
                    
                }

                //End EGE -100347 

            }
            finally
            {
                // dispose
                if (client != null) client.Dispose();
            }
        }

        /// <summary>
        /// Log VNett request VAN details in database
        /// All available information will be logged
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="reader">ENettRequestVAN</param>
        public static void LogENettRequestVAN(UserInfo user, ENettRequestVAN reader, CompleteIssueVNettVANResponse response)
        {
            VCardLogConnection Conn = null;
            try
            {
                // Declare new connection
                Conn = new VCardLogConnection(user);

                //let's open the connection
                Conn.Open();

                // the connection was opened
                // We need now to log the request
                if (String.IsNullOrEmpty(reader.PerCode))
                {
                    // We are on phase 2 with multiple travellers
                    // We need to insert for each traveller
                    List<ENettRequestVAN.Traveller> travs = reader.Travellers;

                    foreach (ENettRequestVAN.Traveller trav in travs)
                    {
                        // we will save request in log
                        Conn.InsertNewRequest(reader, trav, response);
                    }
                }
                else
                {
                    // We have XML for phase 1
                    // We have only one traveler
                    Conn.InsertNewRequest(reader, 1, reader.PerCode, reader.TravellerName, null, null, null, Const.ENettChannelMobile, response);
                }


            }
            finally
            {
                // Close the connection
                if (Conn != null)
                {
                    Conn.Close();
                }
            }
        }


        /// <summary>
        /// Log VNett amend VAN details in database
        /// Log table will be updated with corresponding entry
        /// the key is the payment id
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="reader">ENettAmendVAN</param>
        private static void LogVNettAmendAN(UserInfo user, ENettAmendVAN reader, AmendVNettVANResponse response)
        {
            VCardLogConnection Conn = null;
            try
            {
                // Declare new connection
                Conn = new VCardLogConnection(user);

                //let's open the connection
                Conn.Open();

                if (Conn.IsVPaymentIDExist(reader.PaymentID))
                {
                    // the connection was opened
                    // we will save request in log
                    Conn.UpdateForAmend(reader, response);
                }
            }
            finally
            {
                // Close the ocnnection
                if (Conn != null)
                {
                    Conn.Close();
                }
            }
        }




        /// <summary>
        /// Returns lodged card references for a specific customer
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="requestorDetail">Request arguments</param>
        /// <returns>Lodged card references</returns>
        public static LodgedCardReferencesData GetLodgedCardReferences(UserInfo user
            , ArgsLodgedCardReferences requestorDetail)
        {
            NavisionDbConnection conn = null;
            try
            {
                // Define a new connection
                conn = new NavisionDbConnection(user, requestorDetail.GetPOS());

                // connect ...
                conn.Open();

                // return lodged references
                return conn.GetLodgedCardReferences(requestorDetail);
            }
            finally
            {
                // Let's disconnect
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception) { }; // We will ignore this error
                }
            }
        }


        //>>EGE-66905 
        /// <summary>
        /// Return Cost center 1 value from id
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="pos">Market</param>
        /// <param name="customer">Customer</param>
        /// <param name="cc1Id">Cost center 1 Id</param>
        /// <returns>Cost center 1 value</returns>
        /// <returns></returns>
        public static String GetCostCenter1ValueFromId(UserInfo user, string pos, string customer, string cc1Id)
        {
            if (String.IsNullOrEmpty(cc1Id))
            {
                // No Cost center id submitted
                return string.Empty;
            }

            NavisionDbConnection conn = null;
            try
            {
                // Define a new connection
                conn = new NavisionDbConnection(user, pos);

                // connect ...
                conn.Open();

                // connection is fine, let's return Cost center 1 value from value
                return conn.GetCostCenter1ValueFromId(customer, cc1Id);
            }
            finally
            {
                // Let's disconnect
                try
                {
                    conn.Close();
                }
                catch (Exception) { }; // We will ignore this error
            }
        }
        //<<EGE-66905


    }
}
