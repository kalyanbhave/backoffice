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
using System.Configuration;
using SafeNetWS.utils;
using SafeNetWS.messages;
using SafeNetWS.login.ldap;
using SafeNetWS.business;
using SafeNetWS.www;
using SafeNetWS.log;

namespace SafeNetWS.login
{
    /// <summary>
    /// Cette classe permet de se connecter au serveur LDAP
    /// et vérifier le compte utilisateur
    /// elle gère en outre les droits d'exécution des méthodes
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class UserInfo
    {
        // Owner d'application
        public const int APPLICATION_OWNER_CLASSIC= 0;
        public const int APPLICATION_OWNER_LEGACY = 2;

        // Application
        public const int APPLICATION_NONE = 0;
        public const int APPLICATION_CARD_INSERT = 1;
        public const int APPLICATION_TRANSACTIONAL_CARD_INSERT = 2;
        public const int APPLICATION_PAN_LOOKUP = 3;
        public const int APPLICATION_TOKEN_LOOKUP = 4;
        public const int APPLICATION_TOKENIZATION = 5;
        public const int APPLICATION_UNTOKENIZATION = 6;
        public const int APPLICATION_FO_TOKENIZATION = 7;
        public const int APPLICATION_FO_UNTOKENIZATION = 8;
        public const int APPLICATION_MID_OFFICE_ROBOTIC_TOOL = 9;
        public const int APPLICATION_KEY_ROTATION = 10;
        public const int APPLICATION_CARD_VALIDATION = 11;
        public const int APPLICATION_CARD_DELETION = 12;
        public const int APPLICATION_PAYMENT_MEANS_LOOKUP = 13;

        // VPayment
        public const int APPLICATION_VPAYMENT_ID_GENERATION = 14;
        public const int APPLICATION_VPAYMENT_ID_VALIDATION = 15;

        // VNett
        public const int APPLICATION_VNETT_VAN_RETRIEVAL = 16;
        public const int APPLICATION_VNETT_VAN_AMEND = 17;
        public const int APPLICATION_VNETT_VAN_DETAILS_LOOKUP = 18;
        public const int APPLICATION_VNETT_VAN_CANCEL = 19;
        //Lodged card references
        public const int APPLICATION_LODGED_CARD_REFERENCES = 20;
        public const int APPLICATION_TOKEN_VALIDATION = 21;
        // Check
        public const int APPLICATION_AUTO_TEST = 22;
        // Encryption/Decryption
        public const int APPLICATION_ENCRYPT_STRING = 23;
        public const int APPLICATION_DECRYPT_STRING = 24;
        public const int APPLICATION_FORM_OF_PAYMENT_LOOKUP = 25;
        public const int APPLICATION_CARD_INSERT_FROM_TOKEN = 26;

        private static string[] ApplicationNames = 
        {   
            string.Empty,
            "CARD_INSERT",
            "TRANSACTIONAL_CARD_INSERT",
            "PAN_LOOKUP",
            "TOKEN_LOOKUP",
            "TOKENIZATION",
            "UNTOKENIZATION",
            "FO_TOKENIZATION",
            "FO_UNTOKENIZATION",
            "MID_OFFICE_ROBOTIC_TOOL",
            "KEY_ROTATION",
            "CARD_VALIDATION",
            "CARD_DELETION",
            "PAYMENT_MEANS_LOOKUP",
            "VPAYMENT_ID_GENERATION",
            "VPAYMENT_ID_VALIDATION",
            "VNETT_VAN_RETRIEVAL",
            "VNETT_VAN_AMEND",
            "VNETT_VAN_DETAILS_LOOKUP",
            "VNETT_VAN_CANCEL",
            "LODGED_CARD_REFERENCES",
            "TOKEN_VALIDATION",
            "AUTO_TEST",
            "ENCRYPT_STRING",
            "DECRYPT_STRING",
            "FORM_OF_PAYMENT_LOOKUP",
            "CARD_INSERT_FROM_TOKEN"
         };

        private int Application;

        // Action
        public const int ACTION_NONE = 0;
        public const int ACTION_ENCRYPT = 1;
        public const int ACTION_DECRYPT = 2;
        public const int ACTION_DECRYPT_ENCRYPT = 3;


        private static string[] ActionNames = 
        {   
            string.Empty , 
            "ENCRYPT", 
            "DECRYPT", 
            "ACTION_DECRYPT_ENCRYPT",
         };

        private int Action;

        // Gestion des droits
        public const int RightNA = -1;
        public const int RightDisplayACardInLookupTool = 1;
        public const int RightProcessALookupInLookupTool = 2;
        public const int RightProcessAReverseLookup = 3;
        public const int RightCreateATransactionalCard = 4;
        public const int RightCreateAProfilCard = 5;
        public const int RightUpdateTokenAfterKeyRotation = 6;
        // Droit d'insertion des cartes dans la base des cartes enryptées
        public const int RightEncryptCard = 7;
        // Droit d'insertion des cartes hébergées par FrontOffice
        public const int RightEncryptFOCard = 8;

        // Droit nécéssaire pour effectuer
        // l'action souhaitée par l'application
        private int RequiredRight;

        // Langue de l'utilisateur
        private Messages Messages;

        // Login du client
        private string Login;
        // Vérification du mot de passe
        private bool CheckPassword;
        // Mot de passe du client
        private string Password;
        // L'adresse IP, le nom de l'hote client
        private RemoteHost RemoteHost;
        // Le nom de l'utilisateur récupére de l'AD
        private string DisplayName;
        // nombre de visualisation de cartes
        private int DisplayCardsCount;
        // Date de connexion
        private DateTime LoginDate;
        // Nombre de fois que cet utilisateur
        // a visualiser des cartes
        // On récupère ces infos depuis un fichier
        private string UserFilePath;
        // Gestion des droits
        private bool DisplayACardInLookupTool;
        private bool ProcessALookupInLookupTool;
        private bool ProcessAResverseLookup;
        private bool CreateATransactionalCard;
        private bool CreateAProfilCard;
        private bool IsARobot;
        private bool UpdateTokenAfterKeyRotation;
        private bool EncryptCard;
        private bool EncryptFOCard;

        // Owner de l'application
        private int ApplicationOwner;

        public UserInfo(string local, string logini, RemoteHost remoteHost)
        {
            // On récupère la langue avec laquelle on
            // va répondre au client
            // par défault c'est l'anglais
            string Lang = Util.Nvl(local, Const.LangEN);
            // On défini la langue de l'utilisateur
            SetMessages(Lang);

            SetLogin(logini);
            SetLoginDate();
            SetPassword(null);

            // On indique l'hôte distant qui fait l'appel
            SetRemoteHost(remoteHost);
            // également l'application par laquelle le client est passé
            SetApplication(UserInfo.APPLICATION_NONE);
            // enfin l'action (Encryption ou Décryptage)
            SetAction(UserInfo.ACTION_NONE);
        }

        public UserInfo(string local, string logini, RemoteHost remoteHost,
           int application, int action)
        {

            // On récupère la langue avec laquelle on
            // va répondre au client
            // par défault c'est l'anglais
            string Lang = Util.Nvl(local, Const.LangEN);
            // On défini la langue de l'utilisateur
            SetMessages(Lang);

            SetLogin(logini);
            SetLoginDate();
            SetPassword(null);

            // On indique l'hôte distant qui fait l'appel
            SetRemoteHost(remoteHost);
            // également l'application par laquelle le client est passé
            SetApplication(application);
            // enfin l'action (Encryption ou Décryptage)
            SetAction(action);
        }
       
        public UserInfo(string local, string logini, string passwordin, bool checkPassword, RemoteHost remoteHost,
            int application, int action)
        {
            // On récupère la langue avec laquelle on
            // va répondre au client
            // par défault c'est l'anglais
            string Lang = Util.Nvl(local, Const.LangEN);
            // On défini la langue de l'utilisateur
            SetMessages(Lang);

            SetLogin(logini);
            SetLoginDate();
            SetCheckPassword(checkPassword);
            SetPassword(checkPassword ? passwordin : null);

            // On indique l'hôte distant qui fait l'appel
            SetRemoteHost(remoteHost);
            // également l'application par laquelle le client est passé
            SetApplication(application);
            // enfin l'action (Encryption ou Décryptage)
            SetAction(action);
        }



        public UserInfo(UserInfo user)
        {
            // On reprend les informations depuis
            // le compte passé en paramètre
            SetMessages(user.GetMessages());
            // On va vérifier le compte utilisateur client
            if (String.IsNullOrEmpty(user.GetLogin()))
            {
                // Aucun compte utilisateur 
                // On ne sait pas qui sollicite ce service
                throw new Exception(GetMessages().GetString("EmptyLogin", true));
            }
            SetLogin(user.GetLogin());
            SetLoginDate(user.GetLoginDate());

            if (user.IsCheckPassword())
            {
                // Nous devons vérifier le mot de passe
                if (String.IsNullOrEmpty(user.GetPassword())) throw new Exception(GetMessages().GetString("EmptyPassword", true));
            }
            SetPassword(user.GetPassword());

            SetRemoteHost(user.GetRemoteHost());
            // On met à jour l'application mais pas le droit exigé
            // car ce dernier est transmis par le compte utilisateur
            SetApplication(user.GetApplication(), false);
            // On transmet le droit exigé par l'application
            SetRequiredRigth(user.GetRequiredRigth());
            // On transmet l'owner de l'application
            SetApplicationOwner(user.GetApplicationOwner());
            // et l'action que le compte souhaite effectuee
            SetAction(user.GetAction());
       
            // On va charger le nombre de fois que cet utilisateur
            // a visualiser des cartes
            // On récupère ces infos depuis un fichier
            SetUserFilePath();

            // On récupère les infos
            // sur le nombre de cartes que cet utilisateur a déjà visualiser dans la journée
            SetDisplayCardsCount();


            // Connection au LDAP
            // et récupération des information sur le client
            // nom, groupes
            ConnectToLDAP();

            // On va supprimer les fichiers qui stoquent les informations
            // sur le nombre de visualisation de PAN
            if (!IsRobot())
            {
                // On ne vas pas embêter les robots avec ce traitement
                // car ce dernier ne concerne que les utilisateurs
                // c'est donc lors d'un appel d'un utilisateur non robot
                // que le nettoyage sera effectué
                Services.RemoveDisplayPANCount(this);
            }
     
        }


        /// <summary>
        /// Connexion à l'active Directory
        /// pour vérifier le compte utilisateur
        /// et reourner la liste des groupes auxquels
        /// cet dernier
        /// </summary>
        private void ConnectToLDAP()
        {
            LdapAuthentication adAuth = null;
            try
            {
                // Connection au LDAP pour vérifier le compte user
                adAuth = new LdapAuthentication(GetMessages());
                if (!adAuth.UserExists(GetLogin(), GetPassword()))
                {
                    // Le compte est introuvable 
                    // ou le login/mot de passe est erroné
                    if(GetPassword()==null) 
                    {
                        throw new Exception(GetMessages().GetString("LDAPUnknownUser", GetLogin(), true));
                    }
                    else
                    {
                        throw new Exception(GetMessages().GetString("LDAPUnknownUserOrWrongPassword", GetLogin(), true));
                    }
                }
                // On a trouvé l'utilisation sur le serveur LDAP 
                // On récupère son nom
                this.DisplayName = adAuth.GetDisplayName();

                if (GetRequiredRigth() != UserInfo.RightNA)
                {
                    // We need to check against a specific AD group for this application
                    // First, let's extract all group
                    Hashtable LDAPGroups = adAuth.GetGroups();

                    this.DisplayACardInLookupTool = LDAPGroups.ContainsValue(ConfigurationManager.AppSettings["CanDisplayACardInLookupTool"]);
                    this.ProcessALookupInLookupTool = LDAPGroups.ContainsValue(ConfigurationManager.AppSettings["CanProcessALookupInLookupTool"]);
                    this.ProcessAResverseLookup = LDAPGroups.ContainsValue(ConfigurationManager.AppSettings["CanProcessAResverseLookup"]);
                    this.CreateATransactionalCard = LDAPGroups.ContainsValue(ConfigurationManager.AppSettings["CanCreateATransactionalCard"]);
                    this.CreateAProfilCard = LDAPGroups.ContainsValue(ConfigurationManager.AppSettings["CanCreateAProfilCard"]);
                    this.UpdateTokenAfterKeyRotation = LDAPGroups.ContainsValue(ConfigurationManager.AppSettings["CanUpdateTokenAfterKeyRotation"]);
                    this.IsARobot = LDAPGroups.ContainsValue(ConfigurationManager.AppSettings["IsARobot"]);
                    this.EncryptCard = LDAPGroups.ContainsValue(ConfigurationManager.AppSettings["CanEncryptCard"]);
                    this.EncryptFOCard = LDAPGroups.ContainsValue(ConfigurationManager.AppSettings["CanEncryptFOCard"]);
                   
                }  
            }
            catch (Exception e)
            {
                // Erreur lors de la connexion au serveur LDAP
                throw new Exception(GetMessages().GetString("LDAPConnectionError", GetLogin(), e.Message, true)); 

            }
            finally
            {
                // On va fermer proprement la connexion
                // au serveur LDAP
                if (adAuth != null)
                {
                    try
                    {
                        adAuth.Disconnect();
                    }
                    catch (Exception) { } // On ignore cette erreur  
                }
            }

        }

        /// <summary>
        /// Retourner le compte utilisateur
        /// </summary>
        /// <returns>Compte utilisateur</returns>
        public string GetLogin()
        {
            return this.Login;
        }
      
        /// <summary>
        /// Date de connexion à l'application
        /// </summary>
        /// <returns>Date de connexion</returns>
        public DateTime GetLoginDate()
        {
            return this.LoginDate;
        }

        /// <summary>
        /// Retourne le nombre de visualisations de cartes
        /// </summary>
        /// <returns>Nombre de visualisations de cartes</returns>
        public int GetDisplayCardsCount()
        {
            return this.DisplayCardsCount;
        }

        /// <summary>
        /// Affectation du nombre de visualisations de cartes
        /// </summary>
        private void SetDisplayCardsCount()
        {
            try
            {
                // On récupère les infos
                // sur le nombre de cartes que cet utilisateur a déjà visualiser dans la journée
                this.DisplayCardsCount = Util.ConvertStringToInt(Util.Nvl(Util.GetContentDispayCardsFilesFolder(GetUserFilePath(), true), "0"));
            }
            catch (Exception)
            {
                // On ignore cette exception
            }
        }
        
        /// <summary>
        /// Incrémente le nombre de visualisations de cartes
        /// </summary>
        public void IncrDisplayCardsCount()
        {
            if (!IsRobot())
            {
                this.DisplayCardsCount++;
                // On incrémente le nombre de fois que ce utilisateur a consulté une carte
                // et on sauvegarde cette information pour l'utiliser la prochaine fois
                Util.SaveContentToFile(GetUserFilePath(), this.DisplayCardsCount.ToString());
            }
        }

        /// <summary>
        /// Affectation du fichier du nombre de visualisations des cartes
        /// </summary>
        private void SetUserFilePath()
        {
            this.UserFilePath =  GetLogin() + DateTime.Now.ToString(Const.DateFormat_ddMMyyyy);
        }

        /// <summary>
        /// Retourne le fichier du nombre de visualisations des cartes
        /// </summary>
        /// <returns></returns>
        private String GetUserFilePath()
        {
            return this.UserFilePath;
        }
        /// <summary>
        /// Retourne VRAI si la limite de visualisation de cartes
        /// n'est pas atteinte.
        /// </summary>
        /// <returns>VRAI si limite non atteinte</returns>
        public bool IsDisplayCardsCountLimitNotReached()
        {
            return (this.DisplayCardsCount < int.Parse(ConfigurationManager.AppSettings["DisplayCardsLimit"]));
        }
        
        /// <summary>
        /// Retourne le droit de visualiser une carte
        /// </summary>
        /// <returns>VRAI si l'utilisateur à le droit de visualiser des cartes</returns>
        public bool CanDisplayACardInLookupTool()
        {
            return this.DisplayACardInLookupTool;
        }

        /// <summary>
        /// Retourne le droit de visualiser
        /// plusieurs cartes à la suite
        /// </summary>
        /// <returns>VRAI si l'utilisateur à le droit de visualiser des cartes</returns>
        public bool CanProcessALookupInLookupTool()
        {
            return this.ProcessALookupInLookupTool;
        }
        
        /// <summary>
        /// Retourne le droit de visualiser le Token
        /// depuis le numéro de carte
        /// </summary>
        /// <returns>VRAI si l'utilisateur à le droit de voir le Token depuis la carte</returns>
        public bool CanProcessAResverseLookup()
        {
            return this.ProcessAResverseLookup;
        }
       
        /// <summary>
        /// Retourne le droit de créer une carte transactionnelle
        /// </summary>
        /// <returns>VRAI si l'utilisateur a le droit de créer des cartes transactionnelles</returns>
        public bool CanCreateATransactionalCard()
        {
            return this.CreateATransactionalCard;
        }
        
        /// <summary>
        /// Retourne le droit d'encrypter des numéros de cartes
        /// </summary>
        /// <returns>VRAI si l'utilisateur a le droit d'encrypter des cartes</returns>
        public bool CanEncryptCard()
        {
            return (this.EncryptCard || this.CreateAProfilCard || this.CreateATransactionalCard);
        }

        /// <summary>
        /// Retourne le droit d'encrypter des numéros de cartes
        /// hébergées côté FrontOffice
        /// </summary>
        /// <returns>VRAI si l'utilisateur a le droit d'encrypter des cartes</returns>
        public bool CanEncryptFOCard()
        {
            return this.EncryptFOCard;
        }


        /// <summary>
        /// Retourne le droit de créer une carte profiles
        /// </summary>
        /// <returns>VRAI si l'utilisateur a le droit de créer des cartes profiles</returns>
        public bool CanCreateAProfilCard()
        {
            return this.CreateAProfilCard;
        }
        
        /// <summary>
        /// Retourne le droit de mettre à jour les cryptogrammes
        /// après une rotation de clé
        /// </summary>
        /// <returns>VRAI si l'utilisateur a le droit de mettre à jour les cryptogrammes</returns>
        public bool CanUpdateTokenAfterKeyRotation()
        {
            return this.UpdateTokenAfterKeyRotation;
        }
        
        /// <summary>
        /// Retourne VRAI si l'utilisateur est un robot
        /// </summary>
        /// <returns>VRAI si l'utilisateur est un robot</returns>
        public bool IsRobot()
        {
            return IsARobot;
        }
       
        /// <summary>
        /// Mise à jour de l'adresse IP
        /// </summary>
        /// <param name="host">Hote distant</param>
        private void SetRemoteHost(RemoteHost host)
        {
            this.RemoteHost = host;
        }
        
        /// <summary>
        /// Retourne l'hôte distant
        /// </summary>
        /// <returns>Hôte distant</returns>
        public RemoteHost GetRemoteHost()
        {
            return this.RemoteHost;
        }

        /// <summary>
        /// Retourne l'adresse IP du client
        /// </summary>
        /// <returns>Adresse IP</returns>
        public string GetClientIP()
        {
            return GetRemoteHost().GetAddr();
        }
        
        /// <summary>
        /// Retourne le nom de l'utilisateur
        /// </summary>
        /// <returns>Nom utilisateur</returns>
        public string GetDisplayName()
        {
            return this.DisplayName;
        }

        /// <summary>
        /// Retourne le pointeur
        /// sur la langue souhaitée par l'utilisateur
        /// </summary>
        /// <returns>Pointeur langue</returns>
        public Messages GetMessages()
        {
            return this.Messages;
        }

        /// <summary>
        /// Retourne le message
        /// préfixe envoyé à Syslog
        /// </summary>
        /// <returns>Message préfixe</returns>
        public string GetLogMessageBeforeReply()
        {
            return String.Format("(login = {0}, IP = {1}) called {2} - {3}",
                GetLogin(), GetClientIP(), GetApplicationName(GetApplication()), GetActionName(GetAction()));
        }

        /// <summary>
        /// Mise à jour de l'application
        /// induite par l'appel du client
        /// </summary>
        /// <param name="application">Application</param>
        /// <param name="updateRequiredRight">Mise à jour des droits</param>
        private void SetApplication(int application, bool updateRequiredRight)
        {
            this.Application = application;
            if (updateRequiredRight)
            {
                SetRequiredRigth();
                SetApplicationOwner();
            }
        }


        /// <summary>
        /// Mise à jour de l'application
        /// induite par l'appel du client
        /// </summary>
        /// <param name="application">Application</param>
        private void SetApplication(int application)
        {
            SetApplication(application, true);
        }

        /// <summary>
        /// Retourne l'application
        /// induite par l'appel du client
        /// </summary>
        /// <returns>Application</returns>
        public int GetApplication()
        {
           return this.Application;
        }

        /// <summary>
        /// Mise à jour de l'action
        /// induite par l'appel du client
        /// </summary>
        /// <param name="action">Action</param>
        private void SetAction(int action)
        {
            this.Action = action;
        }
        /// <summary>
        /// Retourne l'action
        /// induite par l'appel du client
        /// </summary>
        /// <returns>Action</returns>
        public int GetAction()
        {
            return this.Action;
        }
        
        /// <summary>
        /// Retourne le nom de l'application
        /// depuis l'id
        /// </summary>
        /// <param name="application">Nom de l'aaplication</param>
        /// <returns></returns>
        public static string GetApplicationName(int application)
        {
            return (application > -1 && application < ApplicationNames.Length ? ApplicationNames[application] : null);
        }
        
        /// <summary>
        /// Retourne le nom de l'action
        /// depuis l'id
        /// </summary>
        /// <param name="action">Nom de l'action</param>
        /// <returns></returns>
        public static string GetActionName(int action)
        {
            return (action > -1 && action < ActionNames.Length ? ActionNames[action] : null);
        }

        /// <summary>
        /// Cette fonction permet de définir
        /// le droit requis afin de répondre au client
        /// Ce droit requis est fonction de l'application appelée
        /// <param name="right">Droit</param>
        /// </summary>
        private void SetRequiredRigth(int right)
        {
            this.RequiredRight = right;
        }

        /// <summary>
        /// Cette fonction permet de définir
        /// le droit requis afin de répondre au client
        /// Ce droit requis est fonction de l'application appelée
        /// </summary>
        private void SetRequiredRigth()
        {
            switch (GetApplication())
            {
                case APPLICATION_KEY_ROTATION:
                    this.RequiredRight= RightUpdateTokenAfterKeyRotation;
                    break;
                case APPLICATION_MID_OFFICE_ROBOTIC_TOOL:
                    this.RequiredRight = RightDisplayACardInLookupTool; 
                    break;
                case APPLICATION_CARD_DELETION:
                    this.RequiredRight = RightCreateAProfilCard;
                    break;
                case APPLICATION_FO_TOKENIZATION:
                case APPLICATION_FO_UNTOKENIZATION:
                case APPLICATION_VNETT_VAN_RETRIEVAL:
                case APPLICATION_VNETT_VAN_AMEND:
                case APPLICATION_LODGED_CARD_REFERENCES:
                    this.RequiredRight = RightEncryptFOCard;
                    break;
                case APPLICATION_TRANSACTIONAL_CARD_INSERT:
                    this.RequiredRight = RightCreateATransactionalCard;
                    break;
                case APPLICATION_TOKEN_LOOKUP:
                    this.RequiredRight = RightProcessAReverseLookup;
                    break;
                case APPLICATION_PAN_LOOKUP:
                case APPLICATION_UNTOKENIZATION:
                case APPLICATION_FORM_OF_PAYMENT_LOOKUP:
                    this.RequiredRight = RightProcessALookupInLookupTool;
                    break;
                case APPLICATION_TOKENIZATION:
                    this.RequiredRight = RightEncryptCard;
                    break;
                default:
                    this.RequiredRight = RightNA;
                    break;
            }
        }

        /// <summary>
        /// Cette fonction permet de définir
        /// l'owner d'une application
        /// </summary>
        /// <param name="value">Owner de l'application</param>
        private void SetApplicationOwner(int value)
        {
            this.ApplicationOwner = value;
        }

        /// <summary>
        /// Cette fonction permet de définir
        /// l'owner d'une application
        /// </summary>
        private void SetApplicationOwner()
        {
            switch (GetApplication())
            {
                case APPLICATION_FO_TOKENIZATION:
                case APPLICATION_FO_UNTOKENIZATION:
                    this.ApplicationOwner = APPLICATION_OWNER_LEGACY;
                    break;            
                default:
                    this.ApplicationOwner = APPLICATION_OWNER_CLASSIC;
                    break;
            }
        }

        /// <summary>
        /// Retourne l'owner de l'application
        /// </summary>
        /// <returns>Owner de l'application</returns>
        private int GetApplicationOwner()
        {
            return this.ApplicationOwner;
        }


        /// <summary>
        /// Returns TRUE is the application is a legacy
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public Boolean IsLegacyApplication()
        {
            return (GetApplicationOwner() == APPLICATION_OWNER_LEGACY);
        }


        /// <summary>
        /// Retourne le droit exigé
        /// pour l'application que le client sollicite
        /// </summary>
        /// <returns></returns>
        public int GetRequiredRigth()
        {
            return this.RequiredRight;
        }

        /// <summary>
        /// Récupération du mot de passe
        /// </summary>
        /// <returns>Mot de passe</returns>
        private string GetPassword()
        {
            return this.Password;
        }

        /// <summary>
        /// Affectation du mot de passe
        /// </summary>
        /// <param name="passwordin">Mot de passe</param>
        private void SetPassword(string passwordin)
        {
            this.Password = passwordin;
        }

        /// <summary>
        /// Vérification du mot de passe
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        private bool IsCheckPassword()
        {
            return this.CheckPassword;
        }


        /// <summary>
        /// Affectation vérification du mot de passe
        /// </summary>
        /// <param name="value">TRUE ou FALSE</param>
        private void SetCheckPassword(bool value)
        {
            this.CheckPassword = value;
        }

        /// <summary>
        /// Affectation du compte utilisateur
        /// </summary>
        /// <param name="loginin">Compte utilisateur</param>
        private void SetLogin(string loginin)
        {
            this.Login = loginin;
            this.DisplayName = string.Empty;
        }
        /// <summary>
        /// Affectation de la date de connexion
        /// </summary>
        private void SetLoginDate()
        {
            this.LoginDate = DateTime.Now;
        }

       
        /// <summary>
        /// Affectation de la date de connexion
        /// </summary>
        /// <param name="date">Date</param>
        private void SetLoginDate(DateTime date)
        {
            this.LoginDate = date;
        }


        /// <summary>
        /// Affectation des messages
        /// suivant la langue de traduction
        /// </summary>
        /// <param name="language">Langue</param>
        private void SetMessages(string language)
        {
            this.Messages = new Messages(language);
        }

        /// <summary>
        /// Affectation des messages
        /// </summary>
        /// <param name="message">Messages</param>
        private void SetMessages(Messages message)
        {
            this.Messages = message;
        }

    }
}