using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crawler.Lib
{
    public class UnicodeUtility
    {
        private const string uniChars =
            "àáảãạâầấẩẫậăằắẳẵặèéẻẽẹêềếểễệđìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵÀÁẢÃẠÂẦẤẨẪẬĂẰẮẲẴẶÈÉẺẼẸÊỀẾỂỄỆĐÌÍỈĨỊÒÓỎÕỌÔỒỐỔỖỘƠỜỚỞỠỢÙÚỦŨỤƯỪỨỬỮỰỲÝỶỸỴÂĂĐÔƠƯ";

        private const string KoDauChars =
            "aaaaaaaaaaaaaaaaaeeeeeeeeeeediiiiiooooooooooooooooouuuuuuuuuuuyyyyyAAAAAAAAAAAAAAAAAEEEEEEEEEEEDIIIOOOOOOOOOOOOOOOOOOOUUUUUUUUUUUYYYYYAADOOU";

        public static int UnicodeToUTF8(byte[] dest, int maxDestBytes, string source, int sourceChars)
        {
            int i, count;
            int c, result;

            result = 0;
            if ((source != null && source.Length == 0))
                return result;
            count = 0;
            i = 0;
            if (dest != null)
            {
                while ((i < sourceChars) && (count < maxDestBytes))
                {
                    c = (int)source[i++];
                    if (c <= 0x7F)
                        dest[count++] = (byte)c;
                    else if (c > 0x7FF)
                    {
                        if ((count + 3) > maxDestBytes)
                            break;
                        dest[count++] = (byte)(0xE0 | (c >> 12));
                        dest[count++] = (byte)(0x80 | ((c >> 6) & 0x3F));
                        dest[count++] = (byte)(0x80 | (c & 0x3F));
                    }
                    else
                    {
                        //  0x7F < source[i] <= 0x7FF
                        if ((count + 2) > maxDestBytes)
                            break;
                        dest[count++] = (byte)(0xC0 | (c >> 6));
                        dest[count++] = (byte)(0x80 | (c & 0x3F));
                    }
                }
                if (count >= maxDestBytes)
                    count = maxDestBytes - 1;
                dest[count] = (byte)(0);
            }
            else
            {
                while (i < sourceChars)
                {
                    c = (int)(source[i++]);
                    if (c > 0x7F)
                    {
                        if (c > 0x7FF)
                            count++;
                        count++;
                    }
                    count++;
                }
            }
            result = count + 1;
            return result;
        }


        public static int UTF8ToUnicode(char[] dest, int maxDestChars, byte[] source, int sourceBytes)
        {
            int i, count;
            int c, result;
            int wc;

            if (source == null)
            {
                result = 0;
                return result;
            }
            result = (int)(-1);
            count = 0;
            i = 0;
            if (dest != null)
            {
                while ((i < sourceBytes) && (count < maxDestChars))
                {
                    wc = (int)(source[i++]);
                    if ((wc & 0x80) != 0)
                    {
                        if (i >= sourceBytes)
                            return result;
                        wc = wc & 0x3F;
                        if ((wc & 0x20) != 0)
                        {
                            c = (byte)(source[i++]);
                            if ((c & 0xC0) != 0x80)
                                return result;
                            if (i >= sourceBytes)
                                return result;
                            wc = (wc << 6) | (c & 0x3F);
                        }
                        c = (byte)(source[i++]);
                        if ((c & 0xC0) != 0x80)
                            return result;
                        dest[count] = (char)((wc << 6) | (c & 0x3F));
                    }
                    else
                        dest[count] = (char)wc;
                    count++;
                }
                if (count > maxDestChars)
                    count = maxDestChars - 1;
                dest[count] = (char)(0);
            }
            else
            {
                while (i < sourceBytes)
                {
                    c = (byte)(source[i++]);
                    if ((c & 0x80) != 0)
                    {
                        if (i >= sourceBytes)
                            return result;
                        c = c & 0x3F;
                        if ((c & 0x20) != 0)
                        {
                            c = (byte)(source[i++]);
                            if ((c & 0xC0) != 0x80)
                                return result;
                            if (i >= sourceBytes)
                                return result;
                        }
                        c = (byte)(source[i++]);
                        if ((c & 0xC0) != 0x80)
                            return result;
                    }
                    count++;
                }
            }
            result = count + 1;
            return result;
        }


        public static byte[] UTF8Encode(string ws)
        {
            int l;
            byte[] temp, result;

            result = null;
            if ((ws != null && ws.Length == 0))
                return result;
            temp = new byte[ws.Length * 3];
            l = UnicodeToUTF8(temp, temp.Length + 1, ws, ws.Length);
            if (l > 0)
            {
                result = new byte[l - 1];
                Array.Copy(temp, 0, result, 0, l - 1);
            }
            else
            {
                result = new byte[ws.Length];
                for (int i = 0; i < result.Length; i++)
                    result[i] = (byte)(ws[i]);
            }
            return result;
        }


        public static string UTF8Decode(byte[] s)
        {
            int l;
            char[] temp;
            string result;

            result = String.Empty;
            if (s == null)
                return result;
            temp = new char[s.Length + 1];
            l = UTF8ToUnicode(temp, temp.Length, s, s.Length);
            if (l > 0)
            {
                result = "";
                for (int i = 0; i < l - 1; i++)
                    result += temp[i];
            }
            else
            {
                result = "";
                for (int i = 0; i < s.Length; i++)
                    result += (char)(s[i]);
            }
            return result;
        }

        public static string RemoveSpecialCharacter(string orig)
        {
            string rv;

            // replacing with space allows the camelcase to work a little better in most cases.
            rv = orig.Replace("\\", " ");
            rv = rv.Replace("(", " ");
            rv = rv.Replace(")", " ");
            rv = rv.Replace("/", " ");
            rv = rv.Replace("-", " ");
            rv = rv.Replace(",", " ");
            rv = rv.Replace(">", " ");
            rv = rv.Replace("<", " ");
            rv = rv.Replace("&", " ");
            rv = rv.Replace("!", " ");
            rv = rv.Replace("@", " ");
            rv = rv.Replace("#", " ");
            rv = rv.Replace("$", " ");
            rv = rv.Replace("%", " ");
            rv = rv.Replace("^", " ");
            rv = rv.Replace("*", " ");
            rv = rv.Replace("+", " ");
            rv = rv.Replace("|", " ");
            rv = rv.Replace("[", " ");
            rv = rv.Replace("]", " ");
            rv = rv.Replace("{", " ");
            rv = rv.Replace("}", " ");
            rv = rv.Replace(":", " ");
            rv = rv.Replace(";", " ");
            rv = rv.Replace("?", " ");
            rv = rv.Replace("~", " ");
            rv = rv.Replace(",", " ");
            //rv = rv.Replace(".", " ");
            rv = rv.Replace("\"", "");
            // single quotes shouldn't result in CamelCase variables like Patient's -> PatientS
            // "smart" forward quote
            rv = rv.Replace("'", "");

            // make sure to get rid of double spaces.
            rv = rv.Replace("   ", " ");
            rv = rv.Replace("  ", " ");

            rv = rv.Trim(' '); // Remove leading and trailing spaces.

            return (rv);
        }
        public static string UnicodeToKoDau(string s)
        {
            string retVal = String.Empty;
            if (s == null)
                return retVal;
            int pos;
            for (int i = 0; i < s.Length; i++)
            {
                pos = uniChars.IndexOf(s[i].ToString());
                if (pos >= 0)
                    retVal += KoDauChars[pos];
                else
                    retVal += s[i];
            }
            return retVal;
        }

        public static string UnicodeToWindows1252(string s)
        {
            string retVal = String.Empty;
            for (int i = 0; i < s.Length; i++)
            {
                int ord = (int)s[i];
                if (ord > 191)
                    retVal += "&#" + ord.ToString() + ";";
                else
                    retVal += s[i];
            }
            return retVal;
        }


        public static string NCRToUnicode(string strInput)
        {
            string TCVN = "&#225;,&#224;,&#7841;,&#7843;,&#227;,&#226;,&#7845;,&#7847;,&#7853;,&#7849;,&#7851;,&#259;,&#7855;,&#7857;,&#7863;,&#7859;,&#7861;,&#é;,&#232;,&#7865;,&#7867;,&#7869;,&#234;,&#7871;,&#7873;,&#7879;,&#7875;,&#7877;,&#243;,&#242;,&#7885;,&#7887;,&#245;,&#244;,&#7889;,&#7891;,&#7897;,&#7893;,&#7895;,&#417;,&#7899;,&#7901;,&#7907;,&#7903;,&#7905;,&#250;,&#249;,&#7909;,&#7911;,&#361;,&#432;,&#7913;,&#7915;,&#7921;,&#7917;,&#7919;,&#237;,&#236;,&#7883;,&#7881;,&#297;,&#273;,&#253;,&#7923;,&#7925;,&#7927;,&#7929;";
            TCVN += "&#193;,&#192;,&#7840;,&#7842;,&#195;,&#194;,&#7844;,&#7846;,&#7852;,&#7848;,&#7850;,&#258;,&#7854;,&#7856;,&#7862;,&#7858;,&#7860;,&#200;,&#7864;,&#7866;,&#7868;,&#7870;,&#7872;,&#7878;,&#7874;,&#7876;,&#211;,&#210;,&#7884;,&#7886;,&#213;,&#212;,&#7888;,&#7890;,&#7896;,&#7892;,&#7894;,&#416;,&#7898;,&#7900;,&#7906;,&#7902;,&#7904;,&#218;,&#217;,&#7908;,&#7910;,&#360;,&#431;,&#7912;,&#7914;,&#7920;,&#7916;,&#7918;,&#272;,&#221;,&#7922;,&#7924;,&#7926;,&#7928;";
            string UNICODE = "á,à,ạ,ả,ã,â,ấ,ầ,ậ,ẩ,ẫ,ă,ắ,ằ,ặ,ẳ,ẵ,é,è,ẹ,ẻ,ẽ,ê,ế,ề,ệ,ể,ễ,ó,ò,ọ,ỏ,õ,ô,ố,ồ,ộ,ổ,ỗ,ơ,ớ,ờ,ợ,ở,ỡ,ú,ù,ụ,ủ,ũ,ư,ứ,ừ,ự,ử,ữ,í,ì,ị,ỉ,ĩ,đ,ý,ỳ,ỵ,ỷ,ỹ";
            UNICODE += "Á,À,Ạ,Ả,Ã,Â,Ấ,Ầ,Ậ,Ẩ,Ẫ,Ă,Ắ,Ằ,Ặ,Ẳ,Ẵ,È,Ẹ,Ẻ,Ẽ,Ế,Ề,Ệ,Ể,Ễ,Ó,Ò,Ọ,Ỏ,Õ,Ô,Ố,Ồ,Ộ,Ổ,Ỗ,Ơ,Ớ,Ờ,Ợ,Ở,Ỡ,Ú,Ù,Ụ,Ủ,Ũ,Ư,Ứ,Ừ,Ự,Ử,Ữ,Đ,Ý,Ỳ,Ỵ,Ỷ,Ỹ";
            string[] str = TCVN.Split(new Char[] { ',' });
            string[] str1 = UNICODE.Split(new Char[] { ',' });
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != "")
                {
                    strInput = strInput.Replace(str[i], str1[i]);
                }
            }
            return strInput;
        }

        public static string UnicodeToISO8859(string src)
        {
            Encoding iso = Encoding.GetEncoding("iso8859-1");
            Encoding unicode = Encoding.UTF8;
            byte[] unicodeBytes = unicode.GetBytes(src);
            return iso.GetString(unicodeBytes);
        }

        public static string ISO8859ToUnicode(string src)
        {
            Encoding iso = Encoding.GetEncoding("iso8859-1");
            Encoding unicode = Encoding.UTF8;
            byte[] isoBytes = iso.GetBytes(src);
            return unicode.GetString(isoBytes);
        }
    }
}
