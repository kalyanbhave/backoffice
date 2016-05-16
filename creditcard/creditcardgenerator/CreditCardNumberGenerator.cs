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
using System.Collections.Generic;
using System.Threading;
using SafeNetWS.utils;

namespace SafeNetWS.creditcard.creditcardgenerator
{
    /// <summary>
    /// Cette classe permet de générer des numéros de cartes
    /// valides aléatoirement
    /// ATTENTION :
    /// cette classe ne doit être utilisée uniquement que pour
    /// effectuer des tests
    /// JAMAIS dans le système de production
    /// </summary>
    public class RandomCreditCardNumberGenerator
    {
        public const string CARD_TYPE_AMEX = "AMERICAN EXPRESS";
        public const string CARD_TYPE_AMEX2 = "AMEX";
        public const string CARD_TYPE_AMEX_19_INDIA = "AMERICAN EXPRESS INDIA 19";
        public const string CARD_TYPE_AMEX_19_INDIA2 = "AMEX INDIA 19";
        public const string CARD_TYPE_AMEX_41_INDIA = "AMEX INDIA 41";
        public const string CARD_TYPE_AMEX_41_INDIA2 = "AMERICAN EXPRESS INDIA 41";
        
        public const string CARD_TYPE_DINERS = "DINERS CLUB";
        public const string CARD_TYPE_DINERS2 = "DINERS";
        public const string CARD_TYPE_DISCOVER = "DISCOVER";
        public const string CARD_TYPE_ENROUTE = "ENROUTE";
        public const string CARD_TYPE_ENROUTE2 = "EN ROUTE";
        public const string CARD_TYPE_JCB_15 = "JCB15";
        public const string CARD_TYPE_JCB_152 = "JCB 15";
        public const string CARD_TYPE_JCB_153 = "JCB1";
        public const string CARD_TYPE_JCB_16 = "JCB16";
        public const string CARD_TYPE_JCB_162 = "JCB 16";
        public const string CARD_TYPE_JCB_163 = "JCB2";
        public const string CARD_TYPE_MASTERCARD = "MASTERCARD";
        public const string CARD_TYPE_VISA = "VISA";
        public const string CARD_TYPE_VOYAGER = "VOYAGER";
        public const string CARD_TYPE_AIRPLUS = "AIRPLUS";
        public const string CARD_TYPE_BANKCARD = "BANKCARD";
        public const string CARD_TYPE_MAESTRO = "MAESTRO";
        public const string CARD_TYPE_SOLO = "SOLO";
        public const string CARD_TYPE_SWITCH = "SWITCH";
        public const string CARD_TYPE_LASER = "LASER";

        private static String[] AMEX_PREFIX_LIST = new[] { "34", "37" };

        private static String[] AMEX_INDIA_19_PREFIX_LIST = new[] { "376919"};

        private static String[] AMEX_INDIA_41_PREFIX_LIST = new[] { "376941" };

        private static String[] DINERS_PREFIX_LIST = new[] { "300", "301", "302", "303", "36", "38" };

        private static String[] DISCOVER_PREFIX_LIST = new[] { "6011" };

        private static String[] ENROUTE_PREFIX_LIST = new[] { "2014", "2149" };

        private static String[] JCB_15_PREFIX_LIST = new[] { "2100", "1800" };

        private static String[] JCB_16_PREFIX_LIST = new[] { "3088", "3096", "3112", "3158", "3337", "3528" };

        private static String[] MASTERCARD_PREFIX_LIST = new[] { "51", "52", "53", "54", "55" };

        private static String[] VISA_PREFIX_LIST = new[] { "4539", "4556", "4916", "4532", "4929", "40240071", "4485", "4716", "4" };

        private static String[] VOYAGER_PREFIX_LIST = new[] { "8699" };

        private static String[] AIRPLUS_PREFIX_LIST = new[] { "192", "122" };

        private static String[] BANKCARD_PREFIX_LIST = new[] { "56" };

        private static String[] MAESTRO_PREFIX_LIST = new[] { "5020", "6" };

        private static String[] SOLO_PREFIX_LIST = new[] { "6334", "6767" };

        private static String[] SWITCH_PREFIX_LIST = new[] { "4903", "4905" , "4911", "4936", "564182", "633110", "6333", "6759"};

        private static String[] LASER_PREFIX_LIST = new[] { "6304", "6706", "6771", "6709" };

        private static String Strrev(SecurePAN str)
        {
            if (str == null) return "";
            String revstr = "";
            int NrStr = str.GetPAN().Length;
            for (int i = NrStr - 1; i >= 0; i--)
            {
                revstr += str.GetPAN()[i];
            }
            return revstr;
        }

        /// <summary>
        /// Returns a credit card number
        /// 
        /// </summary>
        /// <param name="prefix">is the start of the CC number as a string, any number of digits.</param>
        /// <param name="length">is the length of the CC number to generate. Typically 13 or 16</param>
        /// <returns>Card number</returns>
        private static String CompletedNumber(String prefix, int length)
        {
            String ccnumber = prefix;
            // generate digits
            while (ccnumber.Length < (length - 1))
            {
                double rnd = (new Random().NextDouble() * 1.0f - 0f);
                ccnumber += Math.Floor(rnd * 10);
                Thread.Sleep(20);
            }
            // reverse number and convert to int
            String reversedCCnumberString = Strrev(new SecurePAN(ccnumber));
            var reversedCCnumberList = new List<int>();
            for (int i = 0; i < reversedCCnumberString.Length; i++)
            {
                reversedCCnumberList.Add(Convert.ToInt32(reversedCCnumberString[i].ToString()));
            }
            // calculate sum
            int sum = 0;
            int pos = 0;
            int[] reversedCCnumber = reversedCCnumberList.ToArray();
            while (pos < length - 1)
            {
                int odd = reversedCCnumber[pos] * 2;
                if (odd > 9)
                {
                    odd -= 9;
                }
                sum += odd;
                if (pos != (length - 2))
                {
                    sum += reversedCCnumber[pos + 1];
                }
                pos += 2;
            }
            // calculate check digit
            int checkdigit =  Convert.ToInt32((Math.Floor((decimal)sum / 10) + 1) * 10 - sum) % 10;
            ccnumber += checkdigit;
            return ccnumber;
        }

        /// <summary>
        /// Génération d'un tableau de numéros de cartes
        /// valides et aléatoires
        /// </summary>
        /// <param name="prefixList">Préfixes du type de carte</param>
        /// <param name="length">Taille du numéro</param>
        /// <param name="howMany">Nombre de numéros de carte</param>
        /// <returns>Tableau de numéros de carte</returns>
        private static String[] CreditCardNumber(String[] prefixList, int length, int howMany)
        {
            int plen = prefixList.Length;
            var result = new Stack<String>();
            for (int i = 0; i < howMany; i++)
            {
                int next = new Random().Next(0, plen - 1);
                String ccnumber = prefixList[next];
                result.Push(CompletedNumber(ccnumber, length));
            }
            return result.ToArray();
        }

        /// <summary>
        /// Génération d'un tableau de numéros de carte
        /// valides et aléatoires
        /// </summary>
        /// <param name="CardType">Type de carte (amex, airplus, ...)</param>
        /// <param name="size">Taille du numéro</param>
        /// <param name="howMany">Nombre de numéros</param>
        /// <returns>Tableau de numéros de carte</returns>
        public static string[] GenerateCreditCardNumbers(String cardType, int size, int howMany)
        {
            string[] cards = null;
            switch (cardType)
            {
                case CARD_TYPE_MASTERCARD:
                    cards= CreditCardNumber(MASTERCARD_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_AMEX:
                case CARD_TYPE_AMEX2:
                    cards= CreditCardNumber(AMEX_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_AMEX_19_INDIA:
                case CARD_TYPE_AMEX_19_INDIA2:
                    cards = CreditCardNumber(AMEX_INDIA_19_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_AMEX_41_INDIA:
                case CARD_TYPE_AMEX_41_INDIA2:
                    cards = CreditCardNumber(AMEX_INDIA_41_PREFIX_LIST, size, howMany);
                    break; 
                case CARD_TYPE_DINERS:
                case CARD_TYPE_DINERS2:
                    cards= CreditCardNumber(DINERS_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_DISCOVER:
                    cards = CreditCardNumber(DISCOVER_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_ENROUTE:
                case CARD_TYPE_ENROUTE2: 
                    cards= CreditCardNumber(ENROUTE_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_JCB_15:
                case CARD_TYPE_JCB_152:
                case CARD_TYPE_JCB_153:
                    cards = CreditCardNumber(JCB_15_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_JCB_16:
                case CARD_TYPE_JCB_162:
                case CARD_TYPE_JCB_163:
                    cards= CreditCardNumber(JCB_16_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_VISA:
                    cards=  CreditCardNumber(VISA_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_VOYAGER:
                    cards =CreditCardNumber(VOYAGER_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_AIRPLUS:
                    cards = CreditCardNumber(AIRPLUS_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_BANKCARD:
                    cards = CreditCardNumber(BANKCARD_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_MAESTRO:
                    cards = CreditCardNumber(MAESTRO_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_SOLO:
                    cards = CreditCardNumber(SOLO_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_SWITCH:
                    cards = CreditCardNumber(SWITCH_PREFIX_LIST, size, howMany);
                    break;
                case CARD_TYPE_LASER:
                    cards = CreditCardNumber(LASER_PREFIX_LIST, size, howMany);
                    break;
                default:
                    break;
            }
            if (cards == null) throw new Exception("Card type [" + cardType + "] is unkown!");
            return cards;
        }

    }
}
