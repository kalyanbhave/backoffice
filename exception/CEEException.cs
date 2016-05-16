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
    public class CEEException
    {
        private CCEExceptionInfo ExceptionInfo;
        private string ExceptionMessage;

        public CEEException(string value)
        {
            SetExceptionFrom(value);
        }

        public CEEException(string code, string value)
        {
            this.SetExceptionInfo(code, CCEExceptionMap.EXCEPTION_TYPE_UNKNWON,
                CCEExceptionMap.EXCEPTION_SEVERITY_ERROR);
            this.SetExceptionMessage(value);
        }

        public string GetCompleteExceptionMessage()
        {
            return "<ex><code>" + GetExceptionCode() + "</code>"
               + "<severity>" + GetExceptionSeverity() + "</severity>"
               + "<type>" + GetExceptionType() + "</type></ex>"
               + GetExceptionMessage();
        }

        public void SetExceptionInfo(CCEExceptionInfo value)
        {
            this.ExceptionInfo = value;
        }

        public void SetExceptionInfo(string code, string type, string severity)
        {
            SetExceptionInfo(new CCEExceptionInfo(code, type, severity));
        }

        public CCEExceptionInfo GetExceptionInfo()
        {
            return this.ExceptionInfo;
        }

        public string GetExceptionCode()
        {
            return GetExceptionInfo().GetInfoCode();
        }

        public string GetExceptionType()
        {
            return GetExceptionInfo().GetInfoType();
        }

        public string GetExceptionSeverity()
        {
            return GetExceptionInfo().GetInfoSeverity();
        }


        public string GetExceptionMessage()
        {
            return this.ExceptionMessage;
        }

        public void SetExceptionMessage(string value)
        {
            this.ExceptionMessage = value;
        }


        /// <summary>
        /// Décomposition de l'exception si cette dernière est enrichie
        /// On va extraire le code de l'exception
        /// le degré de sévérité de l'exception
        /// le type d'exception
        /// </summary>
        private void SetExceptionFrom(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                if (value.StartsWith(CCEExceptionUtil.EXCEPTION_TAG_OPEN))
                {
                    // Ce message est enrichi
                    // par le code, le type et la sévérité du message
                    SetExceptionInfo(new CCEExceptionInfo(CCEExceptionUtil.GetExceptionCode(value),
                        CCEExceptionUtil.GetExceptionType(value),
                        CCEExceptionUtil.GetExceptionSeverity(value)));
                    SetExceptionMessage(CCEExceptionUtil.GetExceptionOnlyMessage(value));
                }
                else
                {
                    // Cette exception n'est pas enrichie
                    // On va mettre les valeurs par défaut
                    SetExceptionInfo(new CCEExceptionInfo(CCEExceptionMap.EXCEPTION_CODE_DEFAULT,
                         CCEExceptionMap.EXCEPTION_TYPE_SYSTEM,
                          CCEExceptionMap.EXCEPTION_SEVERITY_DEFAULT));
                }
            }
        }

    }
}
