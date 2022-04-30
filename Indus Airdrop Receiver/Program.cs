using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Indus_Airdrop_Receiver
{
    internal class Program
    {
        //1. 로그인 페이지 들어가기
        //2. 개발자 도구 열기
        //2-1. 로그인 버튼 찾기
        //3. 자바스크립트의 login함수 찾기
        //login_url = phps/login.php
        //parameters = email, password

        static void Main(string[] args)
        {
            new Program();
        }

        string loginUrl = "https://www.indus-io.com/phps/login.php";
        string airdropReceiveUrl = "https://www.indus-io.com/phps/airdropReceive.php";

        CookieContainer cookieContainer = new CookieContainer();

        string loginDataPath = "login.dat";
        
        public Program()
        {
            //test
            string email, password;

            if(!File.Exists(loginDataPath))
            {
                Console.WriteLine("enter the email.");
                email = SHA256(Console.ReadLine());

                Console.WriteLine("enter the password.");
                password = SHA256(Console.ReadLine());

                File.WriteAllLines(loginDataPath, new string[2] { email, password });
                Console.WriteLine("initialized login data.");
            }

            string[] dat = File.ReadAllLines(loginDataPath);
            email = dat[0];
            password = dat[1];

            byte[] parameters = Encoding.UTF8.GetBytes(string.Format("email={0}&password={1}&encrypted", email, password.Replace("&", "%26")));

            HttpWebRequest hwr = InitializeHttpWebRequest(loginUrl);
            hwr.Method = "POST";
            hwr.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            hwr.ContentLength = parameters.Length;

            ApplyPostData(hwr, parameters);

            string loginStatus = ReadToEnd(hwr);

            if (loginStatus.Equals("success"))
            {
                hwr = InitializeHttpWebRequest(airdropReceiveUrl);
                string data = ReadToEnd(hwr);
                if (int.TryParse(data, out int amount))
                {
                    Console.WriteLine("Received " + amount + " INDUS");
                }
                else
                {
                    Console.WriteLine(data);
                }
            }
            else
            {
                File.Delete(loginDataPath);
                Console.WriteLine("Login Failure");
            }

            Console.ReadLine();
        }

        HttpWebRequest InitializeHttpWebRequest(string url)
        {
            HttpWebRequest hwr = WebRequest.Create(url) as HttpWebRequest;
            hwr.CookieContainer = cookieContainer;
            hwr.KeepAlive = true;

            return hwr;
        }

        void ApplyPostData(HttpWebRequest hwr, byte[] data)
        {
            using (Stream rs = hwr.GetRequestStream())
            {
                rs.Write(data, 0, data.Length);
            }
        }

        string ReadToEnd(HttpWebRequest hwr)
        {
            using (StreamReader sr = new StreamReader(hwr.GetResponse().GetResponseStream(), Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        static string SHA256(string data)
        {
            SHA256 sha = new SHA256Managed();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
                stringBuilder.AppendFormat("{0:x2}", b);

            return stringBuilder.ToString();
        }
    }
}
