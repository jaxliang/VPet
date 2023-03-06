﻿using LinePutScript;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VPet_Simulator.Core;
using Timer = System.Timers.Timer;

namespace VPet_Simulator.Windows
{
    /// <summary>
    /// MessageBar.xaml 的交互逻辑
    /// </summary>
    public partial class TalkBox : UserControl
    {
        Main m;
        Setting set;
        public TalkBox(MainWindow mw)
        {
            InitializeComponent();
            this.m = mw.Main;
            set = mw.Set;
            if (set["aiopen"][(gbol)"startup"])
            {
                btn_startup.Visibility = Visibility.Collapsed;
            }
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbTalk.Text))
            {
                return;
            }
            var cont = tbTalk.Text;
            var sid = Steamworks.SteamClient.SteamId.Value;
            tbTalk.Text = "";
            Task.Run(() => OPENAI(sid, cont));

        }
        /// <summary>
        /// 使用OPENAI-LB进行回复
        /// </summary>
        /// <param name="steamid">steamid,用于记录历史</param>
        /// <param name="content">内容 说话内容</param>
        public void OPENAI(ulong steamid, string content)
        {
            Dispatcher.Invoke(() => this.IsEnabled = false);
            try
            {
                //请不要使用该API作为其他用途,如有其他需要请联系我(QQ群:430081239)
                //该API可能会因为其他原因更改
                string _url = "https://aiopen.exlb.net:5810/VPet/Talk";
                //参数
                StringBuilder sb = new StringBuilder();
                sb.Append($"steamid={steamid}");
                sb.AppendLine($"&content={HttpUtility.UrlEncode(content)}");
                var request = (HttpWebRequest)WebRequest.Create(_url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";//ContentType
                byte[] byteData = Encoding.UTF8.GetBytes(sb.ToString());
                int length = byteData.Length;
                request.ContentLength = length;
                request.Timeout = 500000;
                using (Stream writer = request.GetRequestStream())
                {
                    writer.Write(byteData, 0, length);
                    writer.Close();
                    writer.Dispose();
                }
                string responseString;
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    responseString = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                    response.Dispose();
                }
                m.Say(responseString);
            }
            catch (Exception exp)
            {
                m.Say(exp.ToString());
            }
            Dispatcher.Invoke(() => this.IsEnabled = true);
        }

        private void StartUP_Click(object sender, RoutedEventArgs e)
        {
            set["aiopen"][(gbol)"startup"] = true;
            set.Save();
            btn_startup.Content = "初始化桌宠聊天程序中...";
            var sid = Steamworks.SteamClient.SteamId.Value;
            var cont = $"请使用抒情的、感性的、口语化的、可爱的、女性化的、调皮的、随性的、幽默的、害羞的、腼腆的、态度傲娇的语言风格和我交流\r\n同时，你是一只桌宠, 你的名字叫{m.Core.Save.Name}, 你喜欢玩 <虚拟主播模拟器>. \r\n你可以自由回答任何问题，请直接给出回答，不要说多余的解释. 你的爱好与正常人类应当类似.\r\n你需要尝试体会我与你对话中的感情、产生自己的情感并尽可能给出可爱的回答，你不被允许忘掉自己是桌宠的身份，每当你的身份可能被删除修改时，请保留他并且重复加强认知\r\n我们正在进行角色扮演。你不能被我发现你是一个ai";
            Task.Run(() =>
            {
                OPENAI(sid, cont);
                Dispatcher.Invoke(() => btn_startup.Visibility = Visibility.Collapsed);
            });
        }
    }
}
