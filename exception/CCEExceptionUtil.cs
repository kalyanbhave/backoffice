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
using SafeNetWS.log;

namespace SafeNetWS.exception
{
    /// <summary>
    /// Enrichissement d'une exception
    /// <ex>
    ///   <code></code>
    ///   <severity></severity>
    ///   <type></type>
    /// </ex>
    /// </summary>
    public class CCEExceptionUtil
    {

        public const string EXCEPTION_TAG_OPEN = "<ex>";
        public const string EXCEPTION_TAG_CLOSE = "</ex>";
        public const string EXCEPTION_CODE_TAG_OPEN = "<ex><code>";
        public const string EXCEPTION_CODE_TAG_CLOSE = "</code>";
        public const string EXCEPTION_SEVERITY_TAG_OPEN = "<severity>";
        public const string EXCEPTION_SEVERITY_TAG_CLOSE = "</severity>";
        public const string EXCEPTION_TYPE_TAG_OPEN = "<type>";
        public const string EXCEPTION_TYPE_TAG_CLOSE = "</type>";

        /// <summary>
        /// Enrichissement des exceptions
        /// On va ajouter 
        /// le code de l'exception
        /// le degré de sévérité
        /// le type d'exception
        /// </summary>
        /// <param name="key">clé du message de translation</param>
        /// <param name="messageOld">le message complet</param>
        /// <param name="newMessage">le message complet</param>
        /// <returns>Message enrichi</returns>
        public static string GetEnhancedMessage(string key, string messageOld, string newMessage)
        {
            if (messageOld.StartsWith(EXCEPTION_TAG_OPEN))
            {
                // Ce message est déjà enrichi
                // par le code, le type et la sévérité du message
                // donc pas besoin de lui ajouter les informations
                return messageOld;
            }
            // Ce message n'est pas enrichi
            // On va donc lui ajouter les informations qui sont fonction de la clé

            CCEExceptionInfo info = CCEExceptionMap.GetExceptionInfo(key);
            
            return String.Format("<ex><code>{0}</code><severity>{1}</severity><type>{2}</type></ex>{3}",
            info.GetInfoCode(), info.GetInfoSeverity(), info.GetInfoType(), newMessage);
        }

        /// <summary>
        /// Extraction d'une information depuis le message de l'exception
        /// </summary>
        /// <param name="message">Message de l'exception</param>
        /// <param name="tagStart">Tag d'ouverture</param>
        /// <param name="tagEnd">Tag de fermeture</param>
        /// <returns>Information exception</returns>
        private static string GetExceptionValue(string message, string tagStart, string tagEnd)
        {
            string retval = null;
            if (tagStart != null)
            {
                int indexOfStart = message.IndexOf(tagStart);
                if (indexOfStart > -1)
                {
                    int indexOfEnd = message.IndexOf(tagEnd);
                    if (indexOfEnd > -1)
                    {
                        indexOfStart += tagStart.Length;
                        retval = message.Substring(indexOfStart, indexOfEnd - indexOfStart);
                    }
                }
            }
            else
            {
                // Mise à jour du message de l'exception
                int indexOfStart = message.IndexOf(tagEnd);
                if (indexOfStart > -1)
                {
                    indexOfStart += tagEnd.Length;
                    retval = message.Substring(indexOfStart, message.Length - indexOfStart);
                }
            }
            return retval;
        }


        /// <summary>
        /// Extraction du code l'exception
        /// </summary>
        /// <param name="message">Message de l'exception</param>
        /// <returns>Code de l'exception</returns>
        public static string GetExceptionCode(string message)
        {
            return GetExceptionValue(message, EXCEPTION_CODE_TAG_OPEN, EXCEPTION_CODE_TAG_CLOSE);
        }

        /// <summary>
        /// Extraction de de la sévérité l'exception
        /// </summary>
        /// <param name="message">Message de l'exception</param>
        /// <returns>Code de l'exception</returns>
        public static string GetExceptionSeverity(string message)
        {
            return GetExceptionValue(message, EXCEPTION_SEVERITY_TAG_OPEN, EXCEPTION_SEVERITY_TAG_CLOSE);
        }

        /// <summary>
        /// Extraction du type l'exception
        /// </summary>
        /// <param name="message">Message de l'exception</param>
        /// <returns>Code de l'exception</returns>
        public static string GetExceptionType(string message)
        {
            return GetExceptionValue(message, EXCEPTION_TYPE_TAG_OPEN, EXCEPTION_TYPE_TAG_CLOSE);
        }
        /// <summary>
        /// Extraction du message de l'exception
        /// </summary>
        /// <param name="message">Message de l'exception</param>
        /// <returns>Code de l'exception</returns>
        public static string GetExceptionOnlyMessage(string message)
        {
            return GetExceptionValue(message, null, EXCEPTION_TAG_CLOSE);
        }

        /// <summary>
        /// Création d'une nouvelle exception
        /// </summary>
        /// <param name="code">Code exception</param>
        /// <param name="type">Type exception</param>
        /// <param name="severity">Severité</param>
        /// <param name="message">Message</param>
        /// <returns>Exception</returns>
        public static CEEException BuildCCEException(string code, string type, string severity, string message)
        {
            CEEException value = new CEEException(null);
            value.SetExceptionInfo(code, type, severity);
            value.SetExceptionMessage(message);
            return value;
        }


        /// <summary>
        /// Clean message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="facility">facility</param>
        /// <param name="level">level</param>
        /// <returns></returns>
        public static string CleanMessage(string message, int facility, int level)
        {
            string CleanedMessage = message;
            if (level == Logger.LogLevelError || level == Logger.LogLevelWarning)
            {
                // Le message est un message d'erreur
                // il contient donc des tags
                // correspondant au code, type et sévérité de l'erreur
                // avant de tracer ce message, il faut enlever ces informations
                // et retourner uniquement le message d'erreur
                CleanedMessage = GetExceptionMessage(message);
            }
            return CleanedMessage;
        }


        /// <summary>
        /// Clean message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns></returns>
        public static string CleanMessage(string message)
        {
            return CleanMessage(message, Logger.LogFacilityUser, Logger.LogLevelError);
        }


        /// <summary>
        /// Traitement de l'exception si elle est enrichie
        /// </summary>
        /// <param name="message">Exception enrichie</param>
        /// <returns>Message d'erreur uniquement</returns>
        public static string GetExceptionMessage(string message)
        {
            if (message == null) return null;
            if (message.StartsWith(CCEExceptionUtil.EXCEPTION_TAG_OPEN))
            {
                // Ce message est enrichi
                // par le code, le type et la sévérité du message
                return CCEExceptionUtil.GetExceptionOnlyMessage(message);
            }
            else
            {
                // Cette exception n'est pas enrichie
                return message;
            }
        }
    }
}
