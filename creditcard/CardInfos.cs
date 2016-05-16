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
using SafeNetWS.utils;
using SafeNetWS.exception;
using SafeNetWS.login;
using System.Configuration;
using SafeNetWS.creditcard;
using SafeNetWS.creditcard.creditcardvalidator.bibit;
using SafeNetWS.creditcard.creditcardvalidator;
using SafeNetWS.business;
using SafeNetWS.log;

namespace SafeNetWS.creditcard
{

    /// <summary>
    /// Cette classe permet de définier l'ensemble des informations
    /// relatives à une carte bancaire valide
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class CardInfos
    {
        // Statut de la carte
        public const int NavisionCardStatusValid = 0;
        public const int NavisionCardStatusStolen = 1;
        public const int NavisionCardStatusTransactional = 6;

        // Type de cartes (private ou corporate)
        public const int NavisionSharingTypePrivate = 2;
        public const int NavisionSharingTypeCorporate = 1;

        // Informations cartes
        private string ShortCardType;
        private string CardType;
        private string NavisionCardType;
        //--> EGE-62034 : Revamp - CCE - Change Financial flow update
        //private string NavisionCardLabel;
        private string NavisionCardName;
        private string NavisionFinancialFlow;
        private string NavisionEnhancedFlow;
        //<-- EGE-62034 : Revamp - CCE - Change Financial flow update
        private string Cvc;
        private string TruncatedPAN;
        private string Token = string.Empty;
        private DateTime ExpirationDate;
        private string ShortExpirationDate;
        private string Card1;
        private string Card2;
        private string Card3;
        private string Card4;
        private string Card5;
        private int MII;
        private string MIIIssuerCategory;
        private string RBSPaymentMethod;

        // Numéro de carte en clair
        private string CardNumber;

        // Informations cartes (Navision)
        private int NavisionPaymentBTA;
        private int NavisionLodgedCard;
        private int NavisionPaymentAirPlus;
        private int NavisionFictiveBTA;
        private int NavisionTransactional;
        private int NavisionStatus;
        private int NavisionSharingType;
        private string Description;

        // Informations holder
        private string Pos;
        private string Service;
        private string CustomerCode;
        private string Travellercode;
        private string HolderName;


        // Informations sur les cartes BIBIT
        private string InformationCode;
        private string InformationMessage;

        // la vérification de la carte
        // depuis le service RBS est demandée
        private bool OnlineCheckRequested;


        // Erreurs 
        private bool CardValid;
        private string UnValidMsg;

        // Informations cache
        private string EncryptedPan;
        private bool BibitValidFromCache;
        private DateTime BibitValidFromCacheDate;


        //>>EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check  
        public enum CardOnlineValidations
        {
            NO_VALIDATION,
            ZERO_AMOUNT,
            ONE_AMOUNT
        };
        public const string CARD_ONLINE_NO_VALIDATION = "NO_VALIDATION";
        public const string CARD_ONLINE_ZERO_VALIDATION = "ZERO_AMOUNT";
        public const string CARD_ONLINE_ONE_VALIDATION = "ONE_AMOUNT";

        private CardOnlineValidations OnlineValidation;

        private const string FINANCIAL_FLOW_BIBIT = "BIBIT";
        //<<EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check  

        //>> EGE-79826 - [BO] Lodge Card - First card Enhancement
        private string FirstCardReference;
        //<< EGE-79826 - [BO] Lodge Card - First card Enhancement

        public CardInfos()
        {
            // Initialisation
            this.CardValid = false;
            this.CardType = string.Empty;
            this.ShortCardType = string.Empty;
            this.TruncatedPAN = string.Empty;
            this.Card1 = string.Empty;
            this.Card2 = string.Empty;
            this.Card3 = string.Empty;
            this.Card4 = string.Empty;
            this.Card5 = string.Empty;
            this.NavisionCardType = string.Empty;
            //--> EGE-62034 : Revamp - CCE - Change Financial flow update
            //this.NavisionCardLabel = string.Empty;
            this.NavisionFinancialFlow = string.Empty;
            this.NavisionEnhancedFlow = string.Empty;
            //<-- EGE-62034 : Revamp - CCE - Change Financial flow update
            this.Cvc = string.Empty;
            this.Token = String.Empty;
            this.ExpirationDate = Const.EmptyDate;
            this.ShortExpirationDate = string.Empty;
            this.NavisionTransactional = 0;
            this.NavisionStatus = NavisionCardStatusValid;
            this.NavisionFictiveBTA = 0;
            this.NavisionSharingType = NavisionSharingTypeCorporate;
            this.Description = string.Empty;
            this.MII = -1;
            this.MIIIssuerCategory = string.Empty;
            this.InformationCode = string.Empty;
            this.InformationMessage = string.Empty;

            this.BibitValidFromCache = false;
            this.BibitValidFromCacheDate = Const.EmptyDate;
            //>>EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check 
            this.OnlineValidation = CardOnlineValidations.NO_VALIDATION;
            //<<EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check 

            //>> EGE-79826 - [BO] Lodge Card - First card Enhancement
            this.FirstCardReference = string.Empty;
            //<< EGE-79826 - [BO] Lodge Card - First card Enhancement
        }


        //>> EGE-79826 - [BO] Lodge Card - First card Enhancement
        /// <summary>
        /// Set FirstCardReference
        /// </summary>
        /// <param name="value">Reference</param>
        public void SetFirstCardReference(string value)
        {
            this.FirstCardReference = String.IsNullOrEmpty(value) ? string.Empty : value.ToUpper();
        }

        /// <summary>
        /// Returns FirstCardReference
        /// </summary>
        /// <returns>FirstCardReference</returns>
        public string GetFirstCardReference()
        {
            return this.FirstCardReference;
        }

        //>> EGE-79826 - [BO] Lodge Card - First card Enhancement


        /// <summary>
        /// Affectation MII
        /// </summary>
        /// <param name="cardNumber">Numéro de carte</param>
        public void SetMII(string cardNumber)
        {
            // L'identification de l'insdustrie (MII) est le premier digit
            this.MII = Util.ConvertStringToInt(cardNumber.Substring(0, 1));
        }
        /// <summary>
        /// Retourne MII
        /// </summary>
        /// <returns>MII</returns>
        public int GetMII()
        {
            // L'identification de l'industrie (MII) est le premier digit
            return this.MII;
        }

        /// <summary>
        /// Retourne la MII Issuer Category
        /// </summary>
        /// <returns>MII Issuer Category</returns>
        public string GetMIIIssuerCategory()
        {
            return this.MIIIssuerCategory;
        }

        /// <summary>
        /// Affectation MIIIssuerCategory
        /// </summary>
        /// <param name="value">MII</param>
        public void SetMIIIssuerCategory(string value)
        {
            this.MIIIssuerCategory = value;
        }

        /// <summary>
        /// Affectation du RBSPaymentMethod
        /// </summary>
        /// <param name="value">Valeur</param>
        public void SetRBSPaymentMethod(string value)
        {
            this.RBSPaymentMethod = value;
        }
        /// <summary>
        /// Retourne le RBSPaymentMethodCode
        /// </summary>
        /// <param name="value">RBSPaymentMethod</param>
        public string GetRBSPaymentMethod()
        {
            return RBSPaymentMethod;
        }

        /// <summary>
        /// Retourne la description
        /// </summary>
        /// <returns>Description</returns>
        public string GetDescription()
        {
            return this.Description;
        }

        /// <summary>
        /// Affectation descripion
        /// </summary>
        /// <param name="description">Description</param>
        public void SetDescription(string description)
        {
            this.Description = description;
        }

        /// <summary>
        /// Retourne le type de carte
        /// Private
        /// Corporate
        /// </summary>
        /// <returns>Type de carte</returns>
        public int GetNavisionSharingType()
        {
            return this.NavisionSharingType;
        }

        /// <summary>
        /// Affectation du type de carte
        /// </summary>
        /// <param name="value">Type de carte</param>
        public void SetNavisionSharingType(int value)
        {
            this.NavisionSharingType = value;
        }

        /// <summary>
        /// Définit si la carte est une carte
        /// transactionnelle
        /// </summary>
        /// <param name="value">TRUE ou FALSE</param>
        public void SetNavisionTransactional(int value)
        {
            this.NavisionTransactional = value;
            if (value > 0)
            {
                // C'est une carte transactionnelle
                SetNavisionStatus(NavisionCardStatusTransactional);
                SetNavisionSharingType(NavisionSharingTypePrivate);
            }
            else
            {
                // Ce n'est une carte transactionnelle
                // On va dire qu'elle est valide
                SetNavisionStatus(NavisionCardStatusValid);
            }
        }

        /// <summary>
        /// Retourne TRUE si la carte est
        /// une carte transactionnelle
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public int IsNavisionTransactional()
        {
            return NavisionTransactional;
        }
        /// <summary>
        /// retourne le statut de la carte
        /// </summary>
        /// <returns>Status de la carte</returns>
        public int GetNavisionStatus()
        {
            return this.NavisionStatus;
        }
        /// <summary>
        /// Affectation du statut de la carte
        /// </summary>
        /// <param name="status">Statut</param>
        public void SetNavisionStatus(int status)
        {
            this.NavisionStatus = status;
        }

        /// <summary>
        /// Affectation de la date d'expiration
        /// de la carte
        /// </summary>
        /// <param name="expirationDate">Date d'expiration</param>
        public void SetShortExpirationDate()
        {
            this.ShortExpirationDate = Util.GetShortExpirationDate(GetExpirationDate());
        }
        /// <summary>
        /// Retourne la date d'expiration
        /// sour le format MM/YY
        /// </summary>
        /// <returns>Date d'expiration</returns>
        public string GetShortExpirationDate()
        {
            return this.ShortExpirationDate;
        }

        /// <summary>
        /// Affectation de la date d'expiration
        /// de la carte
        /// </summary>
        /// <param name="expirationDate">Date d'expiration</param>
        public void SetExpirationDate(DateTime expirationDate)
        {
            this.ExpirationDate = expirationDate;
            SetShortExpirationDate();
        }

        /// <summary>
        /// Affectation du type de carte
        /// </summary>
        /// <param name="cardType">Type de carte</param>
        public void SetCardType(string cardType)
        {
            this.CardType = cardType;
        }

        /// <summary>
        /// Retourne le type de carte
        /// </summary>
        /// <returns>Type de carte</returns>
        public string GetCardType()
        {
            return this.CardType;
        }

        /// <summary>
        /// Affectation du type de carte court
        /// </summary>
        /// <param name="shortCardType">Type de carte court</param>
        public void SetShortCardType(string shortCardType)
        {
            this.ShortCardType = shortCardType;
        }

        /// <summary>
        /// Retourne le type de carte court
        /// </summary>
        /// <returns>Type de carte court</returns>
        public string GetShortCardType()
        {
            return this.ShortCardType;
        }

        /// <summary>
        /// Affectation du numéro de carte masqué
        /// </summary>
        /// <param name="truncatedPAN">Numéro de carte masqué</param>
        public void SetTruncatedPAN(string truncatedPAN)
        {
            this.TruncatedPAN = truncatedPAN;
            SetCard1();
            SetCard2();
            SetCard3();
            SetCard4();
            SetCard5();
        }
        /// <summary>
        /// Retourne le numéro de carte masqué
        /// Sont uniquement visibles que les 4 premiers
        /// et les 6 derniers digits
        /// </summary>
        /// <returns>Numéro de carte masqué</returns>
        public string GetTruncatedPAN()
        {
            return this.TruncatedPAN;
        }
        /// <summary>
        /// Retourne les 4 premiers caractères
        /// du numéro de carte masqué
        /// </summary>
        /// <returns>4 premiers caractères</returns>
        public string GetCard1()
        {
            return this.Card1;
        }
        /// <summary>
        /// Affectation des 4 premiers caractères
        /// du numéro de carte masqué
        /// </summary>
        private void SetCard1()
        {
            this.Card1 = GetTruncatedPAN().Substring(0, 4);
        }
        /// <summary>
        /// Retourne les 4 suivants caractères
        /// du numéro de carte masqué
        /// </summary>
        /// <returns>4 suivants caractères</returns>
        public string GetCard2()
        {
            return this.Card2;
        }
        /// <summary>
        /// Affectation des 4 suivants caractères
        /// du numéro de carte masqué
        /// </summary>
        private void SetCard2()
        {
            this.Card2 = GetTruncatedPAN().Substring(4, 4);
        }

        /// <summary>
        /// Retourne les 4 premiers caractères
        /// du numéro de carte masqué
        /// </summary>
        /// <returns>4 premier caractères</returns>
        public string GetCard3()
        {
            return this.Card3;
        }
        /// <summary>
        /// Affectation des 4 premiers caractères
        /// du numéro de carte masqué
        /// </summary>
        private void SetCard3()
        {
            this.Card3 = GetTruncatedPAN().Substring(8, 4);
        }
        /// <summary>
        /// Retourne les 4 derniers (ou 3)  caractères
        /// du numéro de carte masqué
        /// </summary>
        /// <returns>et enfin le reste (3 ou 4)</returns>
        public string GetCard4()
        {
            return this.Card4;
        }
        /// <summary>
        /// Affectation des 4 derniers (ou 3)  caractères
        /// du numéro de carte masqué
        /// </summary>
        private void SetCard4()
        {
            this.Card4 = GetTruncatedPAN().Length <= 16 ? GetTruncatedPAN().Substring(12, GetTruncatedPAN().Length - 12) : GetTruncatedPAN().Substring(12, 4);
        }

        /// <summary>
        /// Affectation des 4 derniers (ou 3)  caractères
        /// du numéro de carte masqué
        /// </summary>
        private void SetCard5()
        {
            this.Card5 = GetTruncatedPAN().Length > 16 ? GetTruncatedPAN().Substring(16, GetTruncatedPAN().Length - 16) : string.Empty;
        }
        /// <summary>
        /// Retourne les 4 derniers (ou 3)  caractères
        /// du numéro de carte masqué
        /// </summary>
        /// <returns>et enfin le reste (3 ou 4)</returns>
        public string GetCard5()
        {
            return this.Card5;
        }

        /// <summary>
        /// Affectation type de carte Navision
        /// </summary>
        /// <param name="navisionCardType">Type de carte</param>
        public void SetNavisionCardType(string navisionCardType)
        {
            this.NavisionCardType = navisionCardType;
        }


        /// <summary>
        /// Affectation type de carte Navision
        /// </summary>
        /// <param name="navisionCardType">Type de carte</param>
        public void SetNavisionCardName(string navisionCardName)
        {
            this.NavisionCardName = navisionCardName;
        }

        /// <summary>
        /// Retourne le type de carte Navision
        /// </summary>
        /// <returns>Type de carte Navision</returns>
        public string GetNavisionCardName()
        {
            return this.NavisionCardName;
        }


        /// <summary>
        /// Retourne le type de carte Navision
        /// </summary>
        /// <returns>Type de carte Navision</returns>
        public string GetNavisionCardType()
        {
            return this.NavisionCardType;
        }

        /// <summary>
        /// Affectation validité de la carte
        /// </summary>
        /// <param name="cardIsValid">TRUE si la carte est valide</param>
        public void SetCardValid(bool cardIsValid)
        {
            this.CardValid = cardIsValid;
        }

        /// <summary>
        /// Retourne TRUE si la carte est valide
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public bool IsCardValid()
        {
            return this.CardValid;
        }

        /// <summary>
        /// Affectation du message si la carte est invalide
        /// </summary>
        /// <param name="unvalidMsg">Message</param>
        public void SetUnValidMsg(UserInfo user, string orderCode, string informationResponse,
            string completeResponse, string unvalidMsg)
        {
            this.UnValidMsg = unvalidMsg;
            // On va tracer ce rejet dans la table des traces des carte rejetées
            Services.LogRejectedCreditCard(user, this, orderCode,
                Const.StatusRefused,
                informationResponse, completeResponse);
        }

        /// <summary>
        /// Affectation du message si la carte est invalide
        /// </summary>
        /// <param name="unvalidMsg">Message</param>
        public void SetUnValidMsg(UserInfo user, string unvalidMsg)
        {
            this.UnValidMsg = unvalidMsg;
            // Clean message by removing exception tag
            string cleanUnvalidMessage = CCEExceptionUtil.CleanMessage(unvalidMsg);
            // On va tracer ce rejet dans la table des traces des carte rejetées
            Services.LogRejectedCreditCard(user, this, string.Empty,
                Const.StatusRefused,
                cleanUnvalidMessage, cleanUnvalidMessage);
        }

        /// <summary>
        /// Retourne le message si la carte est invalide
        /// </summary>
        /// <returns>Raison d'invalidité de la carte</returns>
        public string GetUnValidMsg()
        {
            return UnValidMsg;
        }

        /// <summary>
        /// Retourne le message si la carte est invalide
        /// On retire du messages les éventuelles balises
        /// d'erreur
        /// </summary>
        /// <returns>Raison d'invalidité de la carte</returns>
        public string GetCleanedUnValidMsg()
        {
            if (GetUnValidMsg() == null) return null;

            if (GetUnValidMsg().StartsWith(CCEExceptionUtil.EXCEPTION_TAG_OPEN))
            {
                return CCEExceptionUtil.GetExceptionOnlyMessage(GetUnValidMsg());
            }
            return GetUnValidMsg();
        }



        //--> EGE-62034 : Revamp - CCE - Change Financial flow update
        //public void SetNavisionCardLabel(string navisionCardLabel)
        // {
        //     this.NavisionCardLabel = navisionCardLabel;
        //}
        /// <summary>
        /// Affectation du label dans Navision
        /// </summary>
        /// <param name="navisionCardLabel">Label</param>
        public void SetNavisionFinancialFlow(string navisionFinancialFlow)
        {
            this.NavisionFinancialFlow = navisionFinancialFlow;
        }
        /// <summary>
        /// Affectation du label dans Navision
        /// </summary>
        /// <param name="navisionCardLabel">Label</param>
        public void SetNavisionEnhancedFlow(string navisionEnhancedFlow)
        {
            this.NavisionEnhancedFlow = navisionEnhancedFlow;
        }
        //<-- EGE-62034 : Revamp - CCE - Change Financial flow update


        //>>EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check  
        /// <summary>
        /// Set wheter or not we need to do online validation
        /// </summary>
        /// <param name="value">CardOnlineValidations</param>
        public void SetOnlineValidation(string value)
        {
            switch (value)
            {
                case CARD_ONLINE_ZERO_VALIDATION:
                    this.OnlineValidation = CardOnlineValidations.ZERO_AMOUNT;
                    break;
                case CARD_ONLINE_ONE_VALIDATION:
                    this.OnlineValidation = CardOnlineValidations.ONE_AMOUNT;
                    break;
                default:
                    this.OnlineValidation = CardOnlineValidations.NO_VALIDATION;
                    break;
            }

        }

        /// <summary>
        /// Returns true is we need to do online validation
        /// for this card
        /// </summary>
        /// <returns>CardOnlineValidations</returns>
        public CardOnlineValidations GetOnlineValidation()
        {
            return this.OnlineValidation;
        }

        //<<EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check 


        //--> EGE-62034 : Revamp - CCE - Change Financial flow update
        //public string GetNavisionCardLabel()
        //{
        //    return NavisionCardLabel;
        //}
        /// <summary>
        /// Retourne le label de la carte dans Navision
        /// </summary>
        /// <returns>Label</returns>
        public string GetNavisionFinancialFlow()
        {
            return NavisionFinancialFlow;
        }

        /// <summary>
        /// Retourne le label de la carte dans Navision
        /// </summary>
        /// <returns>Label</returns>
        public string GetNavisionEnhancedFlow()
        {
            return NavisionEnhancedFlow;
        }
        //<-- EGE-62034 : Revamp - CCE - Change Financial flow update

        /// <summary>
        /// Affectation CVC
        /// </summary>
        /// <param name="cvc">CVC</param>
        public void SetCvc(string cvc)
        {
            this.Cvc = cvc;
        }

        /// <summary>
        /// Retourne le CVC
        /// </summary>
        /// <returns>CVC</returns>
        public string GetCvc()
        {
            return Cvc;
        }

        /// <summary>
        /// Affectation du paiement de la carte en BTA
        /// </summary>
        public void SetNavisionPaymentBTA()
        {
            this.NavisionPaymentBTA = 1;
        }
        /// <summary>
        /// Retourne 1 si la carte va
        /// être utilisée pour les paiement BTA
        /// </summary>
        /// <returns>1 ou 0</returns>
        public int GetNavisionPaymentBTA()
        {
            return NavisionPaymentBTA;
        }
        /// <summary>
        /// Affectation du paiement de la carte en DINERS
        /// </summary>
        public void SetNavisionLodgedCard()
        {
            this.NavisionLodgedCard = 1;
        }
        /// <summary>
        /// Retourne 1 si la carte va
        /// être utilisée pour les paiement DINERS
        /// </summary>
        /// <returns>1 ou 0</returns>
        public int GetNavisionLodgedCard()
        {
            return NavisionLodgedCard;
        }

        /// <summary>
        /// Affectation du paiement de la carte en AIRPLUS
        /// </summary>
        public void SetNavisionPaymentAirPlus()
        {
            this.NavisionPaymentAirPlus = 1;
        }

        /// <summary>
        /// Retourne 1 si la carte va
        /// être utilisée pour les paiement AIRPLUS
        /// </summary>
        /// <returns>1 ou 0</returns>
        public int GetNavisionPaymentAirPlus()
        {
            return NavisionPaymentAirPlus;
        }

        /// <summary>
        /// Retourne la date d'expiration
        /// de la carte
        /// </summary>
        /// <returns>Date d'expiration</returns>
        public DateTime GetExpirationDate()
        {
            return ExpirationDate;
        }

        /// <summary>
        /// Retourne TRUE si la date d'expiration de la carte
        /// a été renseignée
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public bool isExpirationDateProvided()
        {
            return !Util.IsEmptyDate(GetExpirationDate());
        }
        /// <summary>
        /// Retourne information sur fictive BTA
        /// Si la carte est fictive BTA alors
        /// la valeur 1 sera retournée autrement 0
        /// </summary>
        /// <returns>1 si la carte est fictive BTA, à sinon</returns>
        public int GetNavisionFictiveBTA()
        {
            return this.NavisionFictiveBTA;
        }

        /// <summary>
        /// Affectation carte Fictive BTA
        /// </summary>
        public void SetNavisionFictiveBTA()
        {
            this.NavisionFictiveBTA = 1;
        }

        //>>EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check  
        /// <summary>
        /// Returne TRUE si la carte 
        /// est une carte bancaire BIBIT
        /// MasterCard
        /// VISA
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        /*public bool isBIBIT()
        {
            return (GetNavisionCardType() == CreditCardVerifier.NavisionCardTypeVisa
                   || GetNavisionCardType() == CreditCardVerifier.NavisionCardTypeEurocardMastercard);
        }*/
        //<<EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check


        /// <summary>
        /// Retourne 
        /// </summary>
        /// <returns></returns>
        public string GetCardNumber()
        {
            return this.CardNumber;
        }

        /// <summary>
        /// Affectation du numéro de carte
        /// </summary>
        /// <param name="cardNumber">Numéro de carte</param>
        public void SetCardNumber(string cardNumber)
        {
            this.CardNumber = cardNumber;
            SetTruncatedPAN(CreditCardVerifier.TruncatePan(cardNumber));
        }

        /// <summary>
        /// Affectation du warning pour les
        /// cartes BIBIT
        /// </summary>
        /// <param name="value">Warning</param>
        public void SetInformationCode(string value)
        {
            this.InformationCode = value;
        }


        /// <summary>
        /// Affectation du warning pour les
        /// cartes BIBIT
        /// </summary>
        /// <param name="value">Warning</param>
        public void SetInformationMessage(string value)
        {
            this.InformationMessage = value;
        }

        /// <summary>
        /// Retourne le warning
        /// sur la validation des cartes BIBIT
        /// </summary>
        /// <returns>Warning</returns>
        public string GetInformationCode()
        {
            return this.InformationCode;
        }

        /// <summary>
        /// Retourne le warning
        /// sur la validation des cartes BIBIT
        /// </summary>
        /// <returns>Warning</returns>
        public string GetInformationMessage()
        {
            return this.InformationMessage;
        }

        /// <summary>
        /// Affectation du pos
        /// </summary>
        /// <param name="value">Pos</param>
        public void SetPOS(string value)
        {
            this.Pos = value;
        }

        /// <summary>
        /// Retourne le Pos
        /// </summary>
        /// <returns>Pos</returns>
        public string GetPOS()
        {
            return this.Pos;
        }


        /// <summary>
        /// Affectation du service
        /// </summary>
        /// <param name="value">service</param>
        public void SetService(string value)
        {
            this.Service = value;
        }

        /// <summary>
        /// Retourne le service
        /// </summary>
        /// <returns>service</returns>
        public string GetService()
        {
            return this.Service;
        }


        /// <summary>
        /// Affectation du code client
        /// </summary>
        /// <param name="value">Code client</param>
        public void SetCustomerCode(string value)
        {
            this.CustomerCode = value;
        }

        /// <summary>
        /// Retourne le code du client
        /// </summary>
        /// <returns>Code client</returns>
        public string GetCustomerCode()
        {
            return this.CustomerCode;
        }


        /// <summary>
        /// Affectation du code voyageur
        /// </summary>
        /// <param name="value">Code voyageur</param>
        public void SetTravellerCode(string value)
        {
            this.Travellercode = value;
        }

        /// <summary>
        /// Retourne le code du voyageur
        /// </summary>
        /// <returns>Code voyageur</returns>
        public string GetTravellerCode()
        {
            return this.Travellercode;
        }

        /// <summary>
        /// Affectation du HolderName
        /// </summary>
        /// <param name="value">HolderName</param>
        public void SetHolderName(string value)
        {
            this.HolderName = value;
        }

        /// <summary>
        /// Retourne le HolderName
        /// </summary>
        /// <returns>HolderName</returns>
        public string GetHolderName()
        {
            return this.HolderName;
        }

        /// <summary>
        /// Affectation du Token
        /// </summary>
        /// <param name="value">Token</param>
        public void SetToken(string value)
        {
            this.Token = String.IsNullOrEmpty(value) ? string.Empty : value.ToUpper();
        }

        /// <summary>
        /// Retourne le TokenFO
        /// </summary>
        /// <returns>TokenFO</returns>
        public string GetToken()
        {
            return this.Token;
        }

        /// <summary>
        /// Activation de la vérification en ligne
        /// via le service RBS
        /// uniquement pour BIBIT
        /// </summary>
        /// <param name="value"></param>
        public void SetOnlineCheckRequested(bool value)
        {
            this.OnlineCheckRequested = value;
        }

        /// <summary>
        /// Retourne TRUE si la vérification
        /// est activée via le service RBS
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public bool IsOnlineCheckRequested()
        {
            return this.OnlineCheckRequested;
        }

        /// <summary>
        /// Retourne TRUE la vérification de la carte
        /// via le service BIBIT a été demandé
        /// ET s'il s'agit d'une carte BIBIT
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <returns>TRUE ou FALSE</returns>
        public bool IsBibitCheck(UserInfo user)
        {

            return (
                IsRBSRequestActivate()                      // La vérification en ligne est activée pour toute l'application
                && IsOnlineCheckRequested()                 // La verification en ligne a été demandée par l'appelant
                && GetOnlineValidation() != CardOnlineValidations.NO_VALIDATION             // BackOffice system request validation for this card
              );
        }



        /// <summary>
        /// Indique si la vérification en ligne
        /// de la carte est activée
        /// </summary>
        /// <returns>Retourne Vrai ou faux</returns>
        private bool IsRBSRequestActivate()
        {
            return BibitVerifier.RBSRequestOn;
        }



        /// <summary>
        /// Retourne le numéro de carte crypté via SafeNet
        /// </summary>
        /// <returns>Numéro de carte crypté</returns>
        public string GetEncryptedPan()
        {
            return this.EncryptedPan;
        }

        /// <summary>
        /// Indique si le numéro de carte a été crypté
        /// </summary>
        /// <returns>True ou False</returns>
        public bool IsEncryptedPan()
        {
            return (this.EncryptedPan != null);
        }

        /// <summary>
        /// Affectatio, du numéro de carte crypté
        /// </summary>
        /// <param name="value">Cryptogramme</param>
        public void SetEncryptedPan(string value)
        {
            this.EncryptedPan = value;
        }

        public void SetBibitValidFromCache(bool value)
        {
            this.BibitValidFromCache = value;
        }
        public bool IsBibitValidFromCache()
        {
            return this.BibitValidFromCache;
        }

        public void SetBibitValidFromCacheDate(DateTime date)
        {
            this.BibitValidFromCacheDate = date;
        }

        /// <summary>
        /// Récupération de la date de dernière
        /// mise en cache pour une carte
        /// </summary>
        /// <returns>Date de dernière mise en cache</returns>
        public DateTime GetBibitValidFromCacheDate()
        {
            return this.BibitValidFromCacheDate;
        }


        /// <summary>
        /// Indicate if we need to validate the credit card
        /// in a remote API
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>True of False</returns>
        public bool IsCallOnLineValidationForSure(UserInfo user)
        {
            return (IsBibitCheck(user));
        }


    }
}