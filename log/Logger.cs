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
using SafeNetWS.utils;
using SafeNetWS.exception;
using SafeNetWS.utils.cache;

namespace SafeNetWS.log
{
 
    /// <summary>
    /// Cette classe permet l'écriture de traces
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class Logger
    {

        public const int LogLevelEmergency = 0;
        public const int LogLevelAlert = 1;
        public const int LogLevelCritical = 2;
        public const int LogLevelError = 3;
        public const int LogLevelWarning = 4;
        public const int LogLevelNotice = 5;
        public const int LogLevelInformation = 6;
        public const int LogLevelDebug = 7;

        public const int LogFacilityUser = 1;

        // This is the log files deletion limit
        // if the log file is older than X months, it will be automatically removed
        private static int LogFilesDeletionLimit = Util.ConvertStringToInt(ConfigurationManager.AppSettings["LogFilesDeletionMonthsLimit"]);

        /// <summary>
        /// Ecriture d'un message warning vers Syslog
        /// </summary>
        /// <param name="message">Message à écrire</param>
        public static void WriteWarningToLog(string message)
        {
            WriteToLog(message, Logger.LogFacilityUser, Logger.LogLevelWarning);
        }


        /// <summary>
        /// Ecriture d'un message information vers Syslog
        /// </summary>
        public static void WriteInformationToLog(string message)
        {
            WriteToLog(message, Logger.LogFacilityUser, Logger.LogLevelInformation);
        }


        /// <summary>
        /// Ecriture d'un message warning vers Syslog
        /// </summary>
        /// <param name="message">Message à écrire</param>
        public static void WriteErrorToLog(string message)
        {
            WriteToLog(message, Logger.LogFacilityUser, Logger.LogLevelError);
        }

        /// <summary>
        /// Ecriture des informations vers les systemes de trace
        /// Syslog
        /// Log fichier (optionnel)
        /// </summary>
        /// <param name="message">Message à tracer</param>
        /// <param name="facility">Facilité (Syslog)</param>
        /// <param name="level">Niveau (Syslog)</param>
        private static void WriteToLog(string message, int facility, int level)
        {
            string CleanedMessage = CCEExceptionUtil.CleanMessage(message, facility, level);

            // Ecriture du message dans le fichier de trace
            WriteToLogFile(CleanedMessage);
        }


        /// <summary>
        /// Clean message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns></returns>
        public static string CleanMessage(string message)
        {
            return CCEExceptionUtil.CleanMessage(message, Logger.LogFacilityUser, Logger.LogLevelError);
        }


        /// <summary>
        /// Ecriture d'information vers le fichier de trace
        /// </summary>
        /// <param name="message">Message à écrire</param>
        private static void WriteToLogFile(string message)
        {
            if (String.IsNullOrEmpty(message))
            {
                // Pas de message...on ne va pas plus loin
                return;
            }
                
            // Vérification trace dans fichiers
            if (!Filelog.IsUseLogFile())
            {
                // L'écriture dans les fichiers log n'est pas activée
                return;
            }

            if (Global.GetFileLog() == null)
            {
                // on va ouvrir un nouveau fichier de trace
                Global.InitFileLog();
            }
            if (DateTime.Today.Day > Global.GetFileLog().GetCreationDate().Day)
            {
                // Le fichier est vieux d'un jour
                // on va le fermer et en ouvrir un nouveau
                Global.InitFileLog();
            }

            // L'écriture dans les fichiers trace
            // est activée
            // On va écrire le message dans le fichier trace
            Global.GetFileLog().WriteLine(message);   
        }

        /// <summary>
        /// Delete old files
        /// </summary>
        public static void DeleteOldFiles()
        {
            if (LogFilesDeletionLimit <= 0) return;

            Global.GetFileLog().WriteLine("================== Started old log files deletion ==================");
            Global.GetFileLog().WriteLine(String.Format("Removing all files old than {0} months ...", LogFilesDeletionLimit)); 

            // Let's calculate the limit date
            // All files that were created before that date must be deleted
            DateTime LimitDate = DateTime.Now.AddMonths(-LogFilesDeletionLimit);
          
            try
            {
                // Get all lof files
                string[] files = Directory.GetFiles(Filelog.LogFolder);
                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    // Fetch files and check if we need to delete it
                    if (fi.LastAccessTime < LimitDate)
                    {
                        // It's an older file, we need to delete it
                        //File.Delete(file);
                        // File was successfully removed
                        Global.GetFileLog().WriteLine(String.Format("Log file [{0}] was deleted.", file.ToString()));  
                    }
                }
            }
            catch (Exception e)
            {
                // Error while procesing log files
                Global.GetFileLog().WriteLine(String.Format("Error while deleting log files {0}", e.Message));
            }
            Global.GetFileLog().WriteLine("================== Ended old log files deletion ===================="); 
        }
    }
}
