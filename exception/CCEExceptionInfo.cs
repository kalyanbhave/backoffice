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

namespace SafeNetWS.exception
{

    public class CCEExceptionInfo
    {
        private string Code;
        private string Type;
        private string Severity;

        /// <summary>
        /// Enrichissement d'une exception
        /// </summary>
        /// <param name="code">Code de l 'exception</param>
        /// <param name="type">Type de l'exception</param>
        /// <param name="severity">Sévértité d'une exception</param>
        public CCEExceptionInfo(string code, string type, string severity)
        {
            this.Code = code;
            this.Type = type;
            this.Severity = severity;
        }

        /// <summary>
        /// Retourne le code de l'exception
        /// </summary>
        /// <returns>Code exception</returns>
        public string GetInfoCode()
        {
            return this.Code;
        }

        /// <summary>
        /// Retourne le type de l'exception
        /// </summary>
        /// <returns>Type de l'exception</returns>
        public string GetInfoType()
        {
            return this.Type;
        }

        /// <summary>
        /// Retourne le degré de sévérité de l'exception
        /// </summary>
        /// <returns>Sévérité de l'exception</returns>
        public string GetInfoSeverity()
        {
            return this.Severity;
        }

      /*  public string GetExceptionToString()
        {
            return CCEExceptionUtil.EXCEPTION_CODE_TAG_OPEN + GetInfoCode() + CCEExceptionUtil.EXCEPTION_CODE_TAG_CLOSE
                + CCEExceptionUtil.EXCEPTION_TYPE_TAG_OPEN + GetInfoType() + CCEExceptionUtil.EXCEPTION_TYPE_TAG_CLOSE
                + CCEExceptionUtil.EXCEPTION_SEVERITY_TAG_OPEN + GetInfoSeverity() + CCEExceptionUtil.EXCEPTION_SEVERITY_TAG_CLOSE
                + CCEExceptionUtil.EXCEPTION_MeSEVERITY_TAG_OPEN + GetInfoSeverity() + CCEExceptionUtil.EXCEPTION_SEVERITY_TAG_CLOSE
                + CCEExceptionUtil.EXCEPTION_TAG_CLOSE;
        }
    */
    }



}
