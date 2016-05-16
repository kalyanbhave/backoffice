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
using System.Configuration;
using System.Net.Mail;
using System.Web;
using SafeNetWS.login;

namespace SafeNetWS.utils
{
    public class MailSender
    {
        public const char MailToSeparator = ';';
        public enum AddressType { To, CC, Bcc };
        public const string MailSubject = "CEE - Error";


        /// <summary>
        /// Sends mail using SMTP client, it can use the IP of Server also
        /// </summary>
        /// <param name="mail">The SMTP server (MailServer) as String</param>
        private static void SendMail(string Smtp, System.Net.Mail.MailMessage MailMessage)
        {

            // Instantiate a new instance of SmtpClient
            SmtpClient mSmtpClient = new SmtpClient();
            mSmtpClient.Host = Smtp;

            // Send the mail message
            mSmtpClient.Send(MailMessage);


        } //  End SendMail


        /// <summary>
        /// Sends an mail message
        /// </summary>
        /// <param name="user">User information</param>
        /// <param name="body">Body of mail message</param>
        public static void SendMail(UserInfo user, string body)
        {
            string Sender = @ConfigurationManager.AppSettings["MailErrorSender"];
            string Receiver = @ConfigurationManager.AppSettings["MailErrorReceiver"];

            if (String.IsNullOrEmpty(Sender) || String.IsNullOrEmpty(Receiver))
            {
                // Mail receiver is missing
                return;
            }

            string Smtp = @ConfigurationManager.AppSettings["MailSmtp"];
            if (String.IsNullOrEmpty(Smtp))
            {
                // SMTP server is missing
                return;
            }

            // Instantiate a new instance of MailMessage
            MailMessage MailMessage = new MailMessage();
            try
            {
                // Set the sender address of the mail message
                MailMessage.From = new MailAddress(Sender);

                // Set the recepient address of the mail message
                AddAddress(MailMessage, AddressType.To, Receiver);

                // Set the subject of the mail message
                MailMessage.Subject = MailSubject;

                // Set the priority of the mail message to high
                MailMessage.Priority = MailPriority.High;

                // Add Egencia picture
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                LinkedResource imagelink = new LinkedResource(HttpContext.Current.Server.MapPath("images/egencia.jpg"), "image/jpeg");
                imagelink.ContentId = "egenciaImage";
                imagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                // adding the imaged linked to htmlView...
                htmlView.LinkedResources.Add(imagelink);
                // add the view
                MailMessage.AlternateViews.Add(htmlView);

                // Send the mail message
                SendMail(Smtp, MailMessage);
               
            }
            catch (InvalidOperationException i)
            {
                throw new Exception(user.GetMessages().GetString("MailSender.Error.SendingMail", i.Message,true));
            }
            catch (SmtpException s)
            {
                throw new Exception(user.GetMessages().GetString("MailSender.Error.SMTPConnectionError", s.Message, true));
            }
            catch (Exception e)
            {
                throw new Exception(user.GetMessages().GetString("MailSender.Error", e.Message, true));
            }
            finally
            {
                MailMessage.Dispose();
            }
        }

        /// <summary>
        /// Ajout des destinataire au courriel
        /// </summary>
        /// <param name="message">MailMessage</param>
        /// <param name="type">Type d'adresse (To, Bcc, Cc)</param>
        /// <param name="address">Adresse</param>
        private static void AddAddress(MailMessage message, AddressType type, string address)
        {
            // Set the address of the mail message
            string[] mailTos = address.Split(MailToSeparator);
            int nr = mailTos.Length;
            for (int i = 0; i < nr; i++)
            {
                if (!String.IsNullOrEmpty(mailTos[i]))
                {
                    switch (type)
                    {
                        case AddressType.To:
                            message.To.Add(new MailAddress(mailTos[i]));
                            break;
                        case AddressType.CC:
                            message.CC.Add(new MailAddress(mailTos[i]));
                            break;
                        default:
                            message.Bcc.Add(new MailAddress(mailTos[i]));
                            break;
                    }
                }
            }
        }
    }
}
