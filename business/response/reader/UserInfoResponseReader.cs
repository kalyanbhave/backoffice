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
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Xml;
using System.IO;
using SafeNetWS.utils;

namespace SafeNetWS.business.response.reader
{
    /**
     * Cette classe permet de lire la réponse apportée
     * par la méthode qui se connecte à l'AD re récupère les informations sur un user
     * La réponse est structurée de la manière suivante :
     * L'entrée réponse est structurée de la manière suivante :
     * <?xml version="1.0" encoding="ISO-8859-1"?>
     * <Response>
     *   <Duration>Valeur de retour</Duration>
     *   <Value>
     *       <Login>Valeur de retour</Login>
     *       <Lang>Valeur de retour</Lang>
     *       <ClientIP>Valeur de retour</ClientIP>
     *       <DisplayName>Valeur de retour</DisplayName>
     *       <DisplayCardsCount>Valeur de retour</DisplayCardsCount>
     *       <LoginDate>Valeur de retour</LoginDate>
     *       <DisplayACardInLookupTool>Valeur de retour</DisplayACardInLookupTool>
     *       <ProcessALookupInLookupTool>Valeur de retour</ProcessALookupInLookupTool>
     *       <ProcessAResverseLookup>Valeur de retour</ProcessAResverseLookup>
     *       <CreateATransactionalCard>Valeur de retour</CreateATransactionalCard>
     *       <CreateAProfilCard>Valeur de retour</CreateAProfilCard>
     *       <EncryptACard>Valeur de retour</EncryptACard>
     *       <EncryptAFOCard>Valeur de retour</EncryptAFOCard>
     *       <CanUpdateTokenAfterKeyRotation>Valeur de retour</CanUpdateTokenAfterKeyRotation>  
     *       <IsARobot>Valeur de retour</IsARobot>   
     *   </Value>
     *   <Exception>
     *       <Count>0</Count>
     *       <Message></Message>
     *  </Exception>
     * </Response>
     * 
     * Le client doit parser cet XML et extraire en premier le tag "Exception/Count"
     * 
     * Date : 13/10/2009
     * Auteur : Samatar HASSAN
     * 
     * 
     */
    public class UserInfoResponseReader
    {
        private const string Xml_Response_Login_TagName = "Login";
        // Value Lang to return (serialized into string)
        private const string Xml_Response_Lang_TagName = "Lang";
        // Value ClientIP to return (serialized into string)
        private const string Xml_Response_ClientIP_TagName = "ClientIP";
        // Value DisplayName to return (serialized into string)
        private const string Xml_Response_DisplayName_TagName = "DisplayName";
        // Value displayCardsCount to return (serialized into string)
        private const string Xml_Response_DisplayCardsCount_TagName = "DisplayCardsCount";
        // Value loginDate to return (serialized into string)
        private const string Xml_Response_LoginDate_TagName = "LoginDate";
        // Value DisplayACardInLookupTool to return (serialized into string)
        private const string Xml_Response_DisplayACardInLookupTool_TagName = "DisplayACardInLookupTool";
        // Value ProcessALookupInLookupTool to return (serialized into string)
        private const string Xml_Response_ProcessALookupInLookupTool_TagName = "ProcessALookupInLookupTool";
        // Value ProcessAResverseLookup to return (serialized into string)
        private const string Xml_Response_ProcessAResverseLookup_TagName = "ProcessAResverseLookup";
        // Value CreateATransactionalCard to return (serialized into string)
        private const string Xml_Response_CreateATransactionalCard_TagName = "CreateATransactionalCard";
        // Value CreateAProfilCard to return (serialized into string)
        private const string Xml_Response_CreateAProfilCard_TagName = "CreateAProfilCard";
        // Value EncryptACard to return (serialized into string)
        private const string Xml_Response_EncryptACard_TagName = "EncryptACard";
        // Value EncryptAFOCard to return (serialized into string)
        private const string Xml_Response_EncryptAFOCard_TagName = "EncryptAFOCard";
        // Value UpdateTokenAfterKeyRotation to return (serialized into string)
        private const string Xml_Response_UpdateTokenAfterKeyRotation_TagName = "UpdateTokenAfterKeyRotation";
        // Value IsARobot to return (serialized into string)
        private const string Xml_Response_IsARobot_TagName = "IsARobot";

        // Value Duration In Milliseconds to return (serialized into string)
        private const string Xml_Response_Duration_TagName = "Duration";



        private string InputResponse;
        // Valeurs de retour
        // Login du client
        private string Login;
        // Langue du client
        private string Lang;
        // L'adresse IP du client
        private string ClientIP;
        // Le nom de l'utilisateur récupére de l'AD
        private string DisplayName;
        // nombre de visualisation de cartes
        private int DisplayCardsCount;
        // Date de connexion
        private DateTime LoginDate;
        // Gestion des droits
        private bool DisplayACardInLookupTool;
        private bool ProcessALookupInLookupTool;
        private bool ProcessAResverseLookup;
        private bool CreateATransactionalCard;
        private bool CreateAProfilCard;
        private bool EncryptACard;
        private bool EncryptAFOCard;
        private bool UpdateTokenAfterKeyRotation;
        private bool Is_A_Robot;

        private double Duration;

        private int ExceptionCount;
        private string ExceptionMessage;

        // Exception 
        // Exception code (0 = no error otherwise 1)
        private const string Xml_Response_Exception_Code_TagName = "Count";
        // Exception message
        private const string Xml_Response_Exception_Message_TagName = "Message";


        public UserInfoResponseReader(string inputResponse)
        {
            this.InputResponse = inputResponse;
            this.Lang = "en";
            this.LoginDate=DateTime.Now;

            // On va lire l'entrée
            ParseResponse();
           
        }

        private void ParseResponse()
        {
            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                //The XmlResolver property is set to null. External resources are not resolved.
                doc.XmlResolver = null;

                // On charge la réponse
                doc.Load(new StringReader(InputResponse));
                   
                try
                {
                    // On récupère en premier l'état d'exception
                    this.ExceptionCount = Util.ConvertStringToInt(doc.GetElementsByTagName(Xml_Response_Exception_Code_TagName)[0].InnerXml);

                    if (this.ExceptionCount > 0)
                    {
                        this.ExceptionMessage = doc.GetElementsByTagName(Xml_Response_Exception_Message_TagName)[0].InnerXml;
                    }
                }
                catch (Exception)
                {
                    // Visiblement pas d'exception
                    // 
                }
                
                // Pas d'exception, on récupère les valeurs
                if (this.ExceptionCount == 0)
                {
                    // On récupère directement les différentes valeurs dans la node <Value></Value>
                    // Il ne doit y avoir qu'une node de chaque type
                    this.Login = doc.GetElementsByTagName(Xml_Response_Login_TagName)[0].InnerXml;
                    this.Lang = doc.GetElementsByTagName(Xml_Response_Lang_TagName)[0].InnerXml;
                    this.ClientIP = doc.GetElementsByTagName(Xml_Response_ClientIP_TagName)[0].InnerXml;
                    this.DisplayName = doc.GetElementsByTagName(Xml_Response_DisplayName_TagName)[0].InnerXml;
                    this.DisplayCardsCount = Util.ConvertStringToInt(doc.GetElementsByTagName(Xml_Response_DisplayCardsCount_TagName)[0].InnerXml);
                    this.LoginDate = Util.ConvertStringToDate(doc.GetElementsByTagName(Xml_Response_LoginDate_TagName)[0].InnerXml, Const.DateFormat_ddMMyyyyHHmmss);
                    this.DisplayACardInLookupTool = ConvertIntToBool(doc.GetElementsByTagName(Xml_Response_DisplayACardInLookupTool_TagName)[0].InnerXml);
                    this.ProcessALookupInLookupTool = ConvertIntToBool(doc.GetElementsByTagName(Xml_Response_ProcessALookupInLookupTool_TagName)[0].InnerXml);
                    this.ProcessAResverseLookup = ConvertIntToBool(doc.GetElementsByTagName(Xml_Response_ProcessAResverseLookup_TagName)[0].InnerXml);
                    this.CreateATransactionalCard = ConvertIntToBool(doc.GetElementsByTagName(Xml_Response_CreateATransactionalCard_TagName)[0].InnerXml);
                    this.CreateAProfilCard = ConvertIntToBool(doc.GetElementsByTagName(Xml_Response_CreateAProfilCard_TagName)[0].InnerXml);
                    this.UpdateTokenAfterKeyRotation = ConvertIntToBool(doc.GetElementsByTagName(Xml_Response_UpdateTokenAfterKeyRotation_TagName)[0].InnerXml);
                    this.EncryptACard = ConvertIntToBool(doc.GetElementsByTagName(Xml_Response_EncryptACard_TagName)[0].InnerXml);
                    this.EncryptAFOCard = ConvertIntToBool(doc.GetElementsByTagName(Xml_Response_EncryptAFOCard_TagName)[0].InnerXml);
                    this.Is_A_Robot = ConvertIntToBool(doc.GetElementsByTagName(Xml_Response_IsARobot_TagName)[0].InnerXml);
                    
                    this.Duration = Util.ConvertStringToDouble(doc.GetElementsByTagName(Xml_Response_Duration_TagName)[0].InnerXml);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erreur lors de la lecture de la réponse! Erreur :" + e.Message);
            }
            
        }
        private bool ConvertIntToBool(String value)
        {
            return value.Equals("1") ? true : false;
        }
  
       public string GetLogin()
       {
           return this.Login;
       }
       public string GetClientIP()
       {
           return this.ClientIP;
       }
       public string GetDisplayName()
       {
           return this.DisplayName;
       }
       public int GetDisplayCardsCount()
       {
           return this.DisplayCardsCount;
       }
       public DateTime GetLoginDate()
       {
           return this.LoginDate;
       }
       public bool GetDisplayACardInLookupTool()
       {
           return this.DisplayACardInLookupTool;
       }

       public bool GetProcessALookupInLookupTool()
       {
           return this.ProcessALookupInLookupTool;
       }
       public bool GetProcessAResverseLookup()
       {
           return this.ProcessAResverseLookup;
       }
       public bool GetCreateAProfilCard()
       {
           return this.CreateAProfilCard;
       }
       public bool GetCreateATransactionalCard()
       {
           return this.CreateATransactionalCard;
       }
       public bool GetUpdateTokenAfterKeyRotation()
       {
           return UpdateTokenAfterKeyRotation;
       }
       public bool GetEncryptACard()
       {
           return EncryptACard;
       }
       public bool GetEncryptAFOCard()
       {
           return EncryptAFOCard;
       }
       public bool IsARobot()
       {
           return Is_A_Robot;
       }
       public string getExceptionMessage()
       {
           return this.ExceptionMessage;
       }
       public bool IsError()
       {
           return (this.ExceptionCount > 0);
       }
       public double GetDuration()
       {
           return this.Duration;
       }
    }
}
