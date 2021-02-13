using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Widget;
using BilibiliDM_PluginFramework;
using BiliDMLib;
using Bililive_dm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace soBiliDanmaku
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private List<String> dmkData = new List<string>(); 
        //private string[] dmkData;
        private ArrayAdapter adapter;
        
        private int TTS_DATA_CHECK_CODE = 0;
        private int RESULT_TALK_CODE = 1;
        //private TextToSpeech myTTS;
        //private Context context;
        private bool switchDanmaku = true;
        //private DanmakuServer dmkServer;
        //private SharedPreferences sp;
        private EditText iptRoomId;
        private Button btnConnect;
        private Button btnDisconnect;
        private Button btnNotice;
        private Button btnMute;
        private Button btnRandom;
        private CheckBox cbRead;
        private CheckBox cbGift;
        private CheckBox cbOverRead;
        private CheckBox cbReconnect;
        private CheckBox cbVibrate;
        private ListView dmkList;
        private bool WelcomeFlag = false;

        private readonly DanmakuLoader b = new DanmakuLoader();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            RegisterWidget();
            RegisterEvent();
            Welcome();
        }
        private void RegisterWidget()
        {
            //注册界面组件 
            iptRoomId = FindViewById<EditText>(Resource.Id.iptRoomId);
            btnConnect = FindViewById<Button>(Resource.Id.btnConnect);
            btnDisconnect = FindViewById<Button>(Resource.Id.btnDisconnect);
            btnNotice = FindViewById<Button>(Resource.Id.btnNotice);
            btnMute = FindViewById<Button>(Resource.Id.btnMute);
            btnRandom = FindViewById<Button>(Resource.Id.btnRandom);
            cbReconnect = FindViewById<CheckBox>(Resource.Id.cbReconnect);
            cbRead = FindViewById<CheckBox>(Resource.Id.cbRead);
            cbGift = FindViewById<CheckBox>(Resource.Id.cbGift);
            cbOverRead = FindViewById<CheckBox>(Resource.Id.cbOverRead);
            cbVibrate = FindViewById<CheckBox>(Resource.Id.cbVibrate);
            dmkList = FindViewById<ListView>(Resource.Id.dmkList);


            adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, dmkData);
            dmkList.Adapter = adapter;


            btnDisconnect.Background.SetAlpha(128);
            btnDisconnect.Enabled = false;
        }
        private void RegisterEvent()
        {
            b.Disconnected += b_Disconnected;
            b.ReceivedDanmaku += b_ReceivedDanmaku;
            b.ReceivedRoomCount += b_ReceivedRoomCount;
            //注册事件
            btnConnect.Click += BtnConnect_Click;
            btnDisconnect.Click += BtnDisconnect_Click;
            btnMute.Click += BtnMute_Click;
            btnNotice.Click += BtnNotice_Click;
            btnRandom.Click += BtnRandom_Click;
            //btnDisconnect.setOnClickListener(this);
            //btnNotice.setOnClickListener(this);
            //btnMute.setOnClickListener(this);
            //btnRandom.setOnClickListener(this);
            //cbReconnect.setOnClickListener(this);
            //cbRead.setOnClickListener(this);
            //cbGift.setOnClickListener(this);
            //cbOverRead.setOnClickListener(this);
        }

        private void UIConnected()
        {
            iptRoomId.Background.SetAlpha(128);
            btnDisconnect.Background.SetAlpha(255);
            btnConnect.Background.SetAlpha(128);

            iptRoomId.Enabled = false;
            btnConnect.Enabled = false;
            btnDisconnect.Enabled = true;
        }

        private void UIDisconnected()
        {
            iptRoomId.Background.SetAlpha(255);
            btnDisconnect.Background.SetAlpha(128);
            btnConnect.Background.SetAlpha(255);

            iptRoomId.Enabled = true;
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
        }


        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            int roomId;
            try
            {
                roomId = Convert.ToInt32(iptRoomId.Text.Trim());
            }
            catch (Exception)
            {
                adapter.Add("请输入房间号 房间号是数字");
                return;
            }
            if (roomId > 0)
            {
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = false;
                var connectresult = false;
                var trytime = 0;
                adapter.Add("正在连接");


                connectresult = await b.ConnectAsync(roomId);

                if (!connectresult && b.Error != null)// 如果连接不成功并且出错了
                {
                    adapter.Add(b.Error.ToString());
                    UIDisconnected();
                }

                while (!connectresult && sender == null && cbReconnect.Checked == true)
                {
                    if (trytime > 5)
                        break;
                    else
                        trytime++;

                    await Task.Delay(1000); // 稍等一下
                    adapter.Add("正在连接");
                    connectresult = await b.ConnectAsync(roomId);
                }

                if (connectresult)
                {
                    adapter.Add("连接成功");
                    UIConnected();
                }
                else
                {
                    adapter.Add("连接失败");
                    UIDisconnected();
                }
            }
            else
            {
                adapter.Add("房间号ID非法");
                UIDisconnected();
            }
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            b.Disconnect();
            UIDisconnected();
            adapter.Add("连接已关闭");
        }


        private void BtnMute_Click(object sender, EventArgs e)
        {
            CancelSpeech();
        }


        private void BtnRandom_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            int card = rnd.Next(2);
            string result = "";
            if (card == 0)
                result = "小";
            else
                result = "大";
            adapter.Add("结果是" + result);
            if (cbRead.Checked == true)
                readDamaku("结果是" + result);
        }

        private void vibrate()
        {
            if (cbVibrate.Checked == true)
            {
                try
                {
                    // Use default vibration length
                    // Vibration.Vibrate();

                    // Or use specified time
                    var duration = TimeSpan.FromSeconds(0.05);
                    Vibration.Vibrate(duration);
                }
                catch (FeatureNotSupportedException ex)
                {
                    // Feature not supported on device
                }
                catch (Exception ex)
                {
                    // Other error has occurred.
                }
            }
        }


        private void BtnNotice_Click(object sender, EventArgs e)
        {
            adapter.Add("测试123");
            readDamaku("测试123");
            vibrate();

        }

        private void b_Disconnected(object sender, DisconnectEvtArgs args)
        {
            adapter.Add("连接被断开");
            MainThread.BeginInvokeOnMainThread(new Action(() =>
            {
                if (cbReconnect.Checked == true && args.Error != null)
                {
                    adapter.Add("正在自动重连");
                    BtnConnect_Click(null, null);
                }
                else
                {
                    btnConnect.Enabled = true;
                }
            }));

        }


        CancellationTokenSource cts;
        public async Task readDamaku(string str_danmaku)
        {
            if (cbRead.Checked == true)
            {
                cts = new CancellationTokenSource();
                await TextToSpeech.SpeakAsync(str_danmaku, cancelToken: cts.Token);
            }
            // This method will block until utterance finishes.
        }

        // Cancel speech if a cancellation token exists & hasn't been already requested.
        public void CancelSpeech()
        {
            if (cts?.IsCancellationRequested ?? true)
                return;

            cts.Cancel();
        }

        private void b_ReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            string strSend = "";
            if (e.Danmaku.MsgType == MsgTypeEnum.Comment)
            {
                strSend = e.Danmaku.UserName + "[" + e.Danmaku.UserID + "]:" + e.Danmaku.CommentText;
                adapter.Add(strSend);
                vibrate();
                readDamaku(e.Danmaku.CommentText);                
            }

            if (e.Danmaku.MsgType == MsgTypeEnum.GiftSend)
            {
                strSend = e.Danmaku.UserName + "送了" + e.Danmaku.GiftCount + "个" + e.Danmaku.GiftName;
                adapter.Add(strSend);
                readDamaku(strSend);
            }
        }
        private void b_ReceivedRoomCount(object sender, ReceivedRoomCountArgs e)
        {
            MainThread.BeginInvokeOnMainThread(new Action(() => 
            {                
                this.Title = "在线人数：" + e.UserCount;
            }));            
        }
        private void Welcome()
        {
            if (!WelcomeFlag)
            {
                adapter.Add("sosoB站手机弹幕姬0.0.7版 by Sofronio");                
                adapter.Add("B站直播间20767 BuildDate 2020-11-14");                
                adapter.Add("感谢原B站手机弹幕姬作者 by 姪乃浜梢");
                adapter.Add("B站直播间59379");
                adapter.Add("感谢原C#Bilibili弹幕姬作者 by CopyLiu");
                adapter.Add("B站直播间5051");
                //adapter.NotifyDataSetChanged();
            }
            WelcomeFlag = true;
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}