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
using SafeNetWS.utils;
using SafeNetWS.database.result;
using SafeNetWS.login;
using SafeNetWS.exception;


namespace SafeNetWS.business.response.writer
{
   /// <summary>
   /// Cette classe permet de construire la réponse apportée
   /// par la méthode qui teste tous les composants du service web
   /// La réponse est structurée de la manière suivante :
   /// 
   ///<?xml version="1.0" encoding="ISO-8859-1"?>
   ///  <Response>
   ///    <Duration>Valeur de retour</Duration>
   ///    <Version>3.3.3  build date : 20140603 </Version>   
   ///    <GlobalStatus>OK</GlobalStatus>
   ///    <NavisionDatabase>
   ///       <Status>OK</Status>
   ///       <Error></Error>
   ///   </NavisionDatabase>
   ///   <EncryptedBODatabase>
   ///       <Status>OK</Status>
   ///       <Error></Error>
   ///   </EncryptedBODatabase>
   ///   <EncryptedFODatabase>
   ///       <Status>OK</Status>
   ///       <Error></Error>
   ///   </EncryptedFODatabase>
   ///   <CreditCardLogDatabase>
   ///       <Status>OK</Status>
   ///       <Error></Error>
   ///   </CreditCardLogDatabase>
   ///   <VCardLogDatabase>
   ///       <Status>OK</Status>
   ///       <Error></Error>
   ///   </VCardLogDatabase>
   ///   <SafeNet>
   ///       <Status>OK</Status>
   ///       <Error></Error>
   ///   </SafeNet>
   ///   <ActiveDirectory>
   ///       <Status>OK</Status>
   ///       <Error></Error>
   ///   </ActiveDirectory>
   ///   <Syslog>
   ///       <Status>OK</Status>
   ///       <Error></Error>
   ///   </Syslog>
   ///   <RBSWss>
   ///       <Status>OK</Status>
   ///       <Error></Error>
   ///   </RBSWss>
   ///   <ENettWss>
   ///       <Status>OK</Status>
   ///       <Error></Error>
   ///   </ENettWss>
   ///  </Response>
   ///  
   ///  
   ///  Date : 01/03/2012
   ///  Auteur : Samatar HASSAN
   ///  
   /// </summary> 
    public class TestAllComponentsResponse
    {
        
        private const string Xml_Response_Open_Tag="<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";

        // Version
        private const string Xml_Response_Version_Open_Tag = "<Version>";
        private const string Xml_Response_Version_Close_Tag = "</Version>";
        // Global status
        private const string Xml_Response_Global_Status_Open_Tag = "<GlobalStatus>";
        private const string Xml_Response_Global_Status_Close_Tag = "</GlobalStatus>";
        // NavisionDatabase 
        private const string Xml_Response_NavisionDatabase_Open_Tag = "<NavisionDatabase>";
        private const string Xml_Response_NavisionDatabase_Close_Tag = "</NavisionDatabase>";
        // NavisionDatabase_Status
        private const string Xml_Response_NavisionDatabase_Status_Open_Tag = "<NavisionDatabase_Status>";
        private const string Xml_Response_NavisionDatabase_Status_Close_Tag = "</NavisionDatabase_Status>";
        // NavisionDatabase_Error
        private const string Xml_Response_NavisionDatabase_Error_Open_Tag = "<NavisionDatabase_Error>";
        private const string Xml_Response_NavisionDatabase_Error_Close_Tag = "</NavisionDatabase_Error>";

        // EncryptedBODatabase
        private const string Xml_Response_EncryptedBODatabase_Open_Tag = "<EncryptedBODatabase>";
        private const string Xml_Response_EncryptedBODatabase_Close_Tag = "</EncryptedBODatabase>";
        // EncryptedBODatabase_Status
        private const string Xml_Response_EncryptedBODatabase_Status_Open_Tag = "<EncryptedBODatabase_Status>";
        private const string Xml_Response_EncryptedBODatabase_Status_Close_Tag = "</EncryptedBODatabase_Status>";
        // EncryptedBODatabase_Error
        private const string Xml_Response_EncryptedBODatabase_Error_Open_Tag = "<EncryptedBODatabase_Error>";
        private const string Xml_Response_EncryptedBODatabase_Error_Close_Tag = "</EncryptedBODatabase_Error>";

        // EncryptedFODatabase
        private const string Xml_Response_EncryptedFODatabase_Open_Tag = "<EncryptedFODatabase>";
        private const string Xml_Response_EncryptedFODatabase_Close_Tag = "</EncryptedFODatabase>";
        // EncryptedFODatabase_Status
        private const string Xml_Response_EncryptedFODatabase_Status_Open_Tag = "<EncryptedFODatabase_Status>";
        private const string Xml_Response_EncryptedFODatabase_Status_Close_Tag = "</EncryptedFODatabase_Status>";
        // EncryptedFODatabase_Error
        private const string Xml_Response_EncryptedFODatabase_Error_Open_Tag = "<EncryptedFODatabase_Error>";
        private const string Xml_Response_EncryptedFODatabase_Error_Close_Tag = "</EncryptedFODatabase_Error>";

        // CreditCardDatabase
        private const string Xml_Response_CreditCardLogDatabase_Open_Tag = "<CreditCardLogDatabase>";
        private const string Xml_Response_CreditCardLogDatabase_Close_Tag = "</CreditCardLogDatabase>";
        // CreditCardDatabase_Status
        private const string Xml_Response_CreditCardLogDatabase_Status_Open_Tag = "<CreditCardLogDatabase_Status>";
        private const string Xml_Response_CreditCardLogDatabase_Status_Close_Tag = "</CreditCardLogDatabase_Status>";
        // CreditCardDatabase_Error
        private const string Xml_Response_CreditCardLogDatabase_Error_Open_Tag = "<CreditCardLogDatabase_Error>";
        private const string Xml_Response_CreditCardLogDatabase_Error_Close_Tag = "</CreditCardLogDatabase_Error>";


        // VCardDatabase
        private const string Xml_Response_VCardLogDatabase_Open_Tag = "<VCardLogDatabase>";
        private const string Xml_Response_VCardLogDatabase_Close_Tag = "</VCardLogDatabase>";
        // VCardDatabase_Status
        private const string Xml_Response_VCardLogDatabase_Status_Open_Tag = "<VCardLogDatabase_Status>";
        private const string Xml_Response_VCardLogDatabase_Status_Close_Tag = "</VCardLogDatabase_Status>";
        // VCardDatabase_Error
        private const string Xml_Response_VCardLogDatabase_Error_Open_Tag = "<VCardLogDatabase_Error>";
        private const string Xml_Response_VCardLogDatabase_Error_Close_Tag = "</VCardLogDatabase_Error>";


        // SafeNet
        private const string Xml_Response_SafeNet_Open_Tag = "<SafeNet>";
        private const string Xml_Response_SafeNet_Close_Tag = "</SafeNet>";
        // SafeNet_Status
        private const string Xml_Response_SafeNet_Status_Open_Tag = "<SafeNet_Status>";
        private const string Xml_Response_SafeNet_Status_Close_Tag = "</SafeNet_Status>";
        // SafeNet_Error
        private const string Xml_Response_SafeNet_Error_Open_Tag = "<SafeNet_Error>";
        private const string Xml_Response_SafeNet_Error_Close_Tag = "</SafeNet_Error>";


        // ActiveDirectory
        private const string Xml_Response_ActiveDirectory_Open_Tag = "<ActiveDirectory>";
        private const string Xml_Response_ActiveDirectory_Close_Tag = "</ActiveDirectory>";
        // ActiveDirectory_Status
        private const string Xml_Response_ActiveDirectory_Status_Open_Tag = "<ActiveDirectory_Status>";
        private const string Xml_Response_ActiveDirectory_Status_Close_Tag = "</ActiveDirectory_Status>";
        // ActiveDirectory_Error
        private const string Xml_Response_ActiveDirectory_Error_Open_Tag = "<ActiveDirectory_Error>";
        private const string Xml_Response_ActiveDirectory_Error_Close_Tag = "</ActiveDirectory_Error>";


        // Syslog
        private const string Xml_Response_Syslog_Open_Tag = "<Syslog>";
        private const string Xml_Response_Syslog_Close_Tag = "</Syslog>";
        // Syslog_Status
        private const string Xml_Response_Syslog_Status_Open_Tag = "<Syslog_Status>";
        private const string Xml_Response_Syslog_Status_Close_Tag = "</Syslog_Status>";
        // Syslog_Error
        private const string Xml_Response_Syslog_Error_Open_Tag = "<Syslog_Error>";
        private const string Xml_Response_Syslog_Error_Close_Tag = "</Syslog_Error>";

        // RBSWss
        private const string Xml_Response_RBSWss_Open_Tag = "<RBSWss>";
        private const string Xml_Response_RBSWss_Close_Tag = "</RBSWss>";
        // RBSWss_Status
        private const string Xml_Response_RBSWss_Status_Open_Tag = "<RBSWss_Status>";
        private const string Xml_Response_RBSWss_Status_Close_Tag = "</RBSWss_Status>";
        // RBSWss_Error
        private const string Xml_Response_RBSWss_Error_Open_Tag = "<RBSWss_Error>";
        private const string Xml_Response_RBSWss_Error_Close_Tag = "</RBSWss_Error>";
        // ENett
        private const string Xml_Response_EnettWss_Open_Tag = "<ENettWss>";
        private const string Xml_Response_ENettWss_Close_Tag = "</ENettWss>";
        private const string Xml_Response_ENettWss_Status_Open_Tag = "<ENettWss_Status>";
        private const string Xml_Response_ENettWss_Status_Close_Tag = "</ENettWss_Status>";
        private const string Xml_Response_ENettWss_Error_Open_Tag = "<ENettWss_Error>";
        private const string Xml_Response_ENettWss_Error_Close_Tag = "</ENettWss_Error>";
        // Navision Wss
        // Navision
        private const string Xml_Response_NavisionWss_Open_Tag = "<NavisionWss>";
        private const string Xml_Response_NavisionWss_Close_Tag = "</NavisionWss>";
        private const string Xml_Response_NavisionWss_Status_Open_Tag = "<NavisionWss_Status>";
        private const string Xml_Response_NavisionWss_Status_Close_Tag = "</NavisionWss_Status>";
        private const string Xml_Response_NavisionWss_Error_Open_Tag = "<NavisionWss_Error>";
        private const string Xml_Response_NavisionWss_Error_Close_Tag = "</NavisionWss_Error>";

        // Value Duration In Milliseconds to return (serialized into string)
        private const string Xml_Response_Duration_Open_Tag = "<Duration>";
        private const string Xml_Response_Duration_Close_Tag = "</Duration>";




        private UserInfo User;
        private DateTime StartDate;
        private string NavisionDatabaseStatus;
        private string NavisionDatabaseError;
        private string EncryptedBODatabaseStatus;
        private string EncryptedBODatabaseError;
        private string EncryptedFODatabaseStatus;
        private string EncryptedFODatabaseError;
        private string CreditCardLogDatabaseStatus;
        private string CreditCardLogDatabaseError;
        private string VCardLogDatabaseStatus;
        private string VCardLogDatabaseError;
        private string SafeNetStatus;
        private string SafeNetError;
        private string ActiveDirectoryStatus;
        private string ActiveDirectoryError;
        private string SyslogStatus;
        private string SyslogError;
        private string RBSWssStatus;
        private string RBSWssError;
        private string ENettWssStatus;
        private string ENettWssError;
        private string NavisionWssStatus;
        private string NavisionWssError;

        private string GlobalStatus;



        public TestAllComponentsResponse()
        {
            // Initialisation
            this.StartDate = DateTime.Now;
            this.NavisionDatabaseStatus = Const.Success;
            this.EncryptedBODatabaseStatus = Const.Success;
            this.EncryptedFODatabaseError = Const.Success;
            this.SafeNetStatus = Const.Success;
            this.SyslogStatus = Const.Success;
            this.EncryptedFODatabaseStatus = Const.Success;
            this.ActiveDirectoryStatus = Const.Success;
            this.CreditCardLogDatabaseStatus = Const.Success;
            this.VCardLogDatabaseStatus = Const.Success;
            this.GlobalStatus = Const.Success;
            this.RBSWssStatus = Const.Success;
            this.ENettWssStatus = Const.Success;
            this.NavisionWssStatus = Const.Success;
        }

        public void SetGlobalStatusFailed()
        {
            this.GlobalStatus = Const.Failed;
        }

        public void SetNavisionDababaseFailed(Exception e)
        {
            this.NavisionDatabaseStatus = Const.Failed;
            this.NavisionDatabaseError = CCEExceptionUtil.GetExceptionMessage(e.Message);
            SetGlobalStatusFailed();
        }

        public void SetEncryptedBODatabaseFailed(Exception e)
        {
            this.EncryptedBODatabaseStatus = Const.Failed;
            this.EncryptedBODatabaseError = CCEExceptionUtil.GetExceptionMessage(e.Message);
            SetGlobalStatusFailed();
        }

        public void SetEncryptedFODatabaseFailed(Exception e)
        {
            this.EncryptedFODatabaseStatus = Const.Failed;
            this.EncryptedFODatabaseError = CCEExceptionUtil.GetExceptionMessage(e.Message);
            SetGlobalStatusFailed();
        }

        public void SetSafeNetFailed(Exception e)
        {
            this.SafeNetStatus = Const.Failed;
            this.SafeNetError = CCEExceptionUtil.GetExceptionMessage(e.Message);
            SetGlobalStatusFailed();
        }
        public void SetActiveDirectoryFailed(Exception e)
        {
            this.ActiveDirectoryStatus = Const.Failed;
            this.ActiveDirectoryError = CCEExceptionUtil.GetExceptionMessage(e.Message);
            SetGlobalStatusFailed();
        }

        public void SetSyslogFailed(Exception e)
        {
            this.SyslogStatus = Const.Failed;
            this.SyslogError = CCEExceptionUtil.GetExceptionMessage(e.Message);
            SetGlobalStatusFailed();
        }
        public void SetCredicardLogDatabaseFailed(Exception e)
        {
            this.CreditCardLogDatabaseStatus = Const.Failed;
            this.CreditCardLogDatabaseError = CCEExceptionUtil.GetExceptionMessage(e.Message);
            SetGlobalStatusFailed();
        }
        public void SetVCardLogDatabaseFailed(Exception e)
        {
            this.VCardLogDatabaseStatus = Const.Failed;
            this.VCardLogDatabaseError = CCEExceptionUtil.GetExceptionMessage(e.Message);
            SetGlobalStatusFailed();
        }

        public void SetRBSWssFailed(Exception e)
        {
            this.RBSWssStatus = Const.Failed;
            this.RBSWssError = CCEExceptionUtil.GetExceptionMessage(e.Message);
            SetGlobalStatusFailed();
        }
        public void SetENettWssFailed(Exception e)
        {
            this.ENettWssStatus = Const.Failed;
            this.ENettWssError = CCEExceptionUtil.GetExceptionMessage(e.Message);
            SetGlobalStatusFailed();
        }
        public void SetNavisionWssFailed(Exception e)
        {
            this.NavisionWssStatus = Const.Failed;
            this.NavisionWssError = CCEExceptionUtil.GetExceptionMessage(e.Message);
            SetGlobalStatusFailed();
        }
        public void SetUser(UserInfo useri)
        {
            this.User = useri;
        }
        public UserInfo GetUser()
        {
            return this.User;
        }

       
        private DateTime GetStartDate()
        {
            return this.StartDate;
        }

        /// <summary>
        /// Return global status
        /// </summary>
        /// <returns>Global status</returns>
        private string GetGlobalStatus()
        {
            return this.GlobalStatus;
        }

        /// <summary>
        /// Retourne la durée du traitement
        /// en ms
        /// </summary>
        /// <returns>Durée (ms)</returns>
        private string GetDuration()
        {
            return Util.GetDuration(this.StartDate).ToString();
        }

        /// <summary>
        /// Retour de la réponse structurée en XML
        /// </summary>
        /// <returns>Réponse (XML)</returns>
        public string GetResponse()
        {
            // On trace la demande
            LogResponse();
            // Ok, on a construire la réponse
            string strData =
            Const.XmlHeader
            + Xml_Response_Open_Tag
                + Xml_Response_Duration_Open_Tag
                    + GetDuration()
                + Xml_Response_Duration_Close_Tag
                + Xml_Response_Version_Open_Tag
                   +  Const.GetApplicationName()
                + Xml_Response_Version_Close_Tag
                + Xml_Response_Global_Status_Open_Tag
                   + GetGlobalStatus()
               + Xml_Response_Global_Status_Close_Tag
               + Xml_Response_NavisionDatabase_Open_Tag
               + Xml_Response_NavisionDatabase_Status_Open_Tag
                    + GetNavisionDatabaseStatus()
               + Xml_Response_NavisionDatabase_Status_Close_Tag;
                if (!Util.ConvertStringToBool(GetNavisionDatabaseStatus()))
                {
                    strData+=Xml_Response_NavisionDatabase_Error_Open_Tag
                         + GetNavisionDatabaseError()
                    + Xml_Response_NavisionDatabase_Error_Close_Tag;
                }
               strData += Xml_Response_NavisionDatabase_Close_Tag
              + Xml_Response_EncryptedBODatabase_Open_Tag
                + Xml_Response_EncryptedBODatabase_Status_Open_Tag
                     + GetEncryptedBODatabaseStatus()
                + Xml_Response_EncryptedBODatabase_Status_Close_Tag;
                if (!Util.ConvertStringToBool(GetEncryptedBODatabaseStatus()))
                {
                    strData += Xml_Response_EncryptedBODatabase_Error_Open_Tag
                         + GetEncryptedBODatabaseError()
                    + Xml_Response_EncryptedBODatabase_Error_Close_Tag;
                }
                strData += Xml_Response_EncryptedBODatabase_Close_Tag
                + Xml_Response_EncryptedFODatabase_Open_Tag
                 + Xml_Response_EncryptedFODatabase_Status_Open_Tag
                      + GetEncryptedFODatabaseStatus()
                 + Xml_Response_EncryptedFODatabase_Status_Close_Tag;

                if (!Util.ConvertStringToBool(GetEncryptedFODatabaseStatus()))
                {
                    strData += Xml_Response_EncryptedFODatabase_Error_Open_Tag
                         + GetEncryptedFODatabaseError()
                    + Xml_Response_EncryptedFODatabase_Error_Close_Tag;
                }
                strData += Xml_Response_EncryptedFODatabase_Close_Tag
                + Xml_Response_CreditCardLogDatabase_Open_Tag
                     + Xml_Response_CreditCardLogDatabase_Status_Open_Tag
                          + GetCreditCardLogDatabaseStatus()
                     + Xml_Response_CreditCardLogDatabase_Status_Close_Tag;
                    if (!Util.ConvertStringToBool(GetCreditCardLogDatabaseStatus()))
                    {
                        strData += Xml_Response_CreditCardLogDatabase_Error_Open_Tag
                             + GetCreditCardLogDatabaseError()
                        + Xml_Response_CreditCardLogDatabase_Error_Close_Tag;
                    }
                 strData += Xml_Response_CreditCardLogDatabase_Close_Tag
                  + Xml_Response_VCardLogDatabase_Open_Tag
                     + Xml_Response_VCardLogDatabase_Status_Open_Tag
                          + GetVCardLogDatabaseStatus()
                     + Xml_Response_VCardLogDatabase_Status_Close_Tag;
                 if (!Util.ConvertStringToBool(GetVCardLogDatabaseStatus()))
                 {
                     strData += Xml_Response_VCardLogDatabase_Error_Open_Tag
                          + GetVCardLogDatabaseError()
                     + Xml_Response_VCardLogDatabase_Error_Close_Tag;
                 }
                 strData += Xml_Response_VCardLogDatabase_Close_Tag
                   + Xml_Response_SafeNet_Open_Tag
                   + Xml_Response_SafeNet_Status_Open_Tag
                        + GetSafeNetStatus()
                   + Xml_Response_SafeNet_Status_Close_Tag;
                    if (!Util.ConvertStringToBool(GetSafeNetStatus()))
                    {
                        strData += Xml_Response_SafeNet_Error_Open_Tag
                             + GetSafeNetError()
                        + Xml_Response_SafeNet_Error_Close_Tag;
                    }
                    strData += Xml_Response_SafeNet_Close_Tag
                    + Xml_Response_ActiveDirectory_Open_Tag
                   + Xml_Response_ActiveDirectory_Status_Open_Tag
                        + GetActiveDirectoryStatus()
                   + Xml_Response_ActiveDirectory_Status_Close_Tag;
                    if (!Util.ConvertStringToBool(GetActiveDirectoryStatus()))
                    {
                        strData += Xml_Response_ActiveDirectory_Error_Open_Tag
                             + GetActiveDirectoryError()
                        + Xml_Response_ActiveDirectory_Error_Close_Tag;
                    }

                    strData += Xml_Response_ActiveDirectory_Close_Tag
               + Xml_Response_Syslog_Open_Tag
                   + Xml_Response_Syslog_Status_Open_Tag
                        + GetSyslogStatus()
                   + Xml_Response_Syslog_Status_Close_Tag;
                    if (!Util.ConvertStringToBool(GetSyslogStatus()))
                    {
                        strData += Xml_Response_Syslog_Error_Open_Tag
                             + GetSyslogError()
                        + Xml_Response_Syslog_Error_Close_Tag;
                    }

                    strData += Xml_Response_Syslog_Close_Tag
                  + Xml_Response_RBSWss_Open_Tag
                   + Xml_Response_RBSWss_Status_Open_Tag
                        + GetRBSWssStatus()
                   + Xml_Response_RBSWss_Status_Close_Tag;
                    if (!Util.ConvertStringToBool(GetRBSWssStatus()))
                    {
                        strData += Xml_Response_RBSWss_Error_Open_Tag
                             + GetRBSWssError()
                        + Xml_Response_RBSWss_Error_Close_Tag;
                    }
                    strData += Xml_Response_RBSWss_Close_Tag
                   + Xml_Response_EnettWss_Open_Tag
                    + Xml_Response_ENettWss_Status_Open_Tag
                              + GetENettWssStatus()
                    + Xml_Response_ENettWss_Status_Close_Tag;
                    if (!Util.ConvertStringToBool(GetENettWssStatus()))
                    {
                        strData += Xml_Response_ENettWss_Error_Open_Tag
                             + GetENettWssError()
                        + Xml_Response_ENettWss_Error_Close_Tag;
                    }

                    strData += Xml_Response_ENettWss_Close_Tag

                   + Xml_Response_NavisionWss_Open_Tag
                    + Xml_Response_NavisionWss_Status_Open_Tag
                              + GetNavisionWssStatus()
                    + Xml_Response_NavisionWss_Status_Close_Tag;
                    if (!Util.ConvertStringToBool(GetNavisionWssStatus()))
                    {
                        strData += Xml_Response_NavisionWss_Error_Open_Tag
                             + GetNavisionWssError()
                        + Xml_Response_NavisionWss_Error_Close_Tag;
                    }

                    strData += Xml_Response_NavisionWss_Close_Tag

            
               + Xml_Response_Close_Tag;
            return strData;
       }

       private string GetNavisionDatabaseError()
       {
           return this.NavisionDatabaseError == null ? string.Empty : NavisionDatabaseError;
       }
       private string GetNavisionDatabaseStatus()
       {
           return this.NavisionDatabaseStatus;
       }
       private string GetEncryptedBODatabaseError()
       {
           return this.EncryptedBODatabaseError == null ? string.Empty : EncryptedBODatabaseError;
       }
       private string GetEncryptedBODatabaseStatus()
       {
           return this.EncryptedBODatabaseStatus;
       }
       private string GetEncryptedFODatabaseError()
       {
           return this.EncryptedFODatabaseError == null ? string.Empty : EncryptedFODatabaseError;
       }

       private string GetEncryptedFODatabaseStatus()
       {
           return this.EncryptedFODatabaseStatus;
       }
       private string GetCreditCardLogDatabaseError()
       {
           return this.CreditCardLogDatabaseError == null ? string.Empty : CreditCardLogDatabaseError;
       }
       private string GetVCardLogDatabaseError()
       {
           return this.VCardLogDatabaseError == null ? string.Empty : VCardLogDatabaseError;
       }
       private string GetVCardLogDatabaseStatus()
       {
           return this.VCardLogDatabaseStatus;
       }
       private string GetCreditCardLogDatabaseStatus()
       {
           return this.CreditCardLogDatabaseStatus;
       }
       private string GetEncryptedActiveDirectoryError()
       {
           return this.ActiveDirectoryError == null ? string.Empty : ActiveDirectoryError;
       }
       private string GetActiveDirectoryStatus()
       {
           return this.ActiveDirectoryStatus;
       }
       private string GetActiveDirectoryError()
       {
           return this.ActiveDirectoryError == null ? string.Empty : ActiveDirectoryError;
       }
       private string GetSafeNetErrorStatus()
       {
           return this.ActiveDirectoryStatus;
       }
       private string GetSafeNetError()
       {
           return this.SafeNetError == null ? string.Empty : SafeNetError;
       }
       private string GetSafeNetStatus()
       {
           return this.SafeNetStatus;
       }
       private string GetSyslogStatus()
       {
           return this.SyslogStatus;
       }
       private string GetSyslogError()
       {
           return this.SyslogError == null ? string.Empty : SyslogError;
       }

       private string GetRBSWssStatus()
       {
           return this.RBSWssStatus;
       }
       private string GetRBSWssError()
       {
           return this.RBSWssError == null ? string.Empty : RBSWssError;
       }
       private string GetENettWssStatus()
       {
           return this.ENettWssStatus;
       }
       private string GetENettWssError()
       {
           return this.ENettWssError == null ? string.Empty : ENettWssError;
       }

       private string GetNavisionWssStatus()
       {
           return this.NavisionWssStatus;
       }
       private string GetNavisionWssError()
       {
           return this.NavisionWssError == null ? string.Empty : NavisionWssError;
       }

       private string GetValueMessage()
       {
           return String.Format("Global status ={0}.", GetGlobalStatus());
       }

       /// <summary>
       /// On va répondre au client
       /// mais avant, nous devons tracer cette demande
       /// en informant Syslog
       /// </summary>
       private void LogResponse()
       {
           Services.WriteOperationStatusToLog(GetUser(),
             String.Format(" and requested components status"),
             String.Format(".The following values were returned to user : {0}", GetValueMessage()),
             null,
             false,
             GetDuration());
       }
    }
}