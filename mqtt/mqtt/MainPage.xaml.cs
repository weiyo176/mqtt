using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using M2Mqtt;
using M2Mqtt.Messages;

namespace mqtt
{
    public partial class MainPage : ContentPage
    {
        private Polyline polyline;
        private Polyline user1;
        private Polyline user2;
        private List<Xamarin.Forms.Maps.Position> positions1 =new List<Position>();
        private List<Xamarin.Forms.Maps.Position> positions2 = new List<Position>();
        private Xamarin.Forms.Maps.Position lastPosition;
        int i = 0;
        MqttClient MClient;
        Label m;
        string topic1 = "123";
        string topic2 = "456";

        Pin pin1= new Pin() { Label = "null"};
        Pin pin2 = new Pin() { Label = "null" };
        public MainPage()
        {
            InitializeComponent();
            //MClient = new MqttClient("broker.MQTTGO.io");
            //MClient.Connect("MQTTGO-3113472595");
            //if (MClient.IsConnected)
            //{
            string[] topics = new string[2];
            topics[0] = topic1;
            topics[1] = topic2;
            byte[] msgs = new byte[2];
            msgs[0] = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE;
            msgs[1] = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE;
            //    _ = MClient.Subscribe(topics, msgs);
            //    MClient.MqttMsgPublishReceived += MClient_MqttMsgPublishReceived;
            //}
            polyline = new Polyline
            {
                StrokeColor = Color.Blue,
                StrokeWidth = 12
            };
            user1 = new Polyline
            {
                StrokeColor = Color.Blue,
                StrokeWidth = 12
            };
            user2 = new Polyline
            {
                StrokeColor = Color.Green,
                StrokeWidth = 12
            };
            map.MapElements.Add(polyline);
            map.MapElements.Add(user1);
            map.MapElements.Add(user2);
            map.Pins.Add(pin1 );
            map.Pins.Add(pin2 );
           
            lastPosition = new Position(23.9494, 120.9377); // 初始位置
            map.MoveToRegion(MapSpan.FromCenterAndRadius(lastPosition, Distance.FromKilometers(1)));
            //AddPin();
            StartLocationUpdate();
            //m = mqtt1;
            // create client instance --------------------------------------------
            MqttClient client = new MqttClient("broker.MQTTGO.io");
            //
            // register to message received 
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);

            // subscribe to the topic "/home/temperature" with QoS 2 
            client.Subscribe(topics, msgs);

            //get random number
            int n = Get_Random(1, 500);
        }
        public static int Get_Random(int min, int max)
        {
            //Random rnd = new Random(Guid.NewGuid().GetHashCode());
            Random rnd = new Random();
            int result = rnd.Next(min, max);
            return result;
        }


        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string str = System.Text.Encoding.Default.GetString(e.Message);
            //if topic is 123 
            

            //string location = "24.13333 120.68333";
            //change location into Location
            //try cover string to double
            try
            {
                string[] loc = str.Split(' ');
                double lat = Convert.ToDouble(loc[0]);
                double lon = Convert.ToDouble(loc[1]);
                Xamarin.Forms.Maps.Position position = new Xamarin.Forms.Maps.Position(lat, lon);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (e.Topic == topic1)
                    {
                        
                        AddPin(position, topic1);
                        AddLine(position ,topic1);
                        CountDistance(user1.Geopath, topic1);
                    }
                    else if (e.Topic == topic2)
                    {
                        AddPin(position, topic2);
                        AddLine(position, topic2);
                        CountDistance(user2.Geopath, topic2);
                    }
                    
                });
            }
            catch (Exception ex)
            {
                //MainThread.BeginInvokeOnMainThread(() =>
                //{
                //    if (e.Topic == topic1)
                //    {
                //        mqtt2.Text = str;
                //    }
                //    else if (e.Topic == topic2)
                //    {
                //        mqtt3.Text = str;
                //    }
                //});
            }
            //double lat = Convert.ToDouble(loc[0]);
            //double lon = Convert.ToDouble(loc[1]);

            // handle message received 
        }
        public double DistanceOfTwoPoints(double lat1, double lng1, double lat2, double lng2)
        {
            double radLng1 = lng1 * Math.PI / 180.0;
            double radLng2 = lng2 * Math.PI / 180.0;
            double a = radLng1 - radLng2;
            double b = (lat1 - lat2) * Math.PI / 180.0;
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
                Math.Cos(radLng1) * Math.Cos(radLng2) * Math.Pow(Math.Sin(b / 2), 2))) * 6378.137;
            s = Math.Round(s * 10000) / 10000;



            return s;
        }
        private void CountDistance(IList<Position> geopath, string topic)
        {
            //count distance
            double distance = 0;
            for (int i = 0; i < geopath.Count - 1; i++)
            {
                distance += DistanceOfTwoPoints(geopath[i].Latitude, geopath[i].Longitude, geopath[i + 1].Latitude, geopath[i + 1].Longitude);
            }
            if (topic == topic1) 
                mqtt2.Text = distance.ToString() + " km";
            else if (topic == topic2)
                mqtt3.Text = distance.ToString() + " km";
            else if (topic == "self")
                mqtt1.Text = distance.ToString() + " km";
        }

        private void AddLine(Xamarin.Forms.Maps.Position position, string topic)
        {
            if (topic == topic1 && positions1.Count == 0)
            {
                
                positions1.Add(position);
                user1.Geopath.Add(position);
            }
            else if (topic == topic2 && positions2.Count == 0)
            {
                positions2.Add(position);
                user2.Geopath.Add(position);
            }
            else if (topic == topic2)
            {
                if (positions2[positions2.Count - 1] != position)
                {
                    positions2.Add(position);
                    user2.Geopath.Add(position);
                }
            }
            else if (topic == topic1)
            {
                if (positions1[positions1.Count - 1] != position)
                {
                    positions1.Add(position);
                    user1.Geopath.Add(position);
                }
            }

        }

        //static void MClient_MqttMsgPublishReceived(object sender,MqttMsgConnectEventArgs e)
        //{
        //    string topic = e.Topic;
        //    string value = Encoding.UTF8.GetString(e.Message);
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        if (topic == "123") mqtt.Text = value;
        //        if (topic == "456") mqtt.Text = value;
        //    });
        //}
        public void AddPin(Xamarin.Forms.Maps.Position position, string topic)
        {
            if (topic == topic1)
            {
                pin1.Position = position;
                pin1.Address = position.ToString();
                pin1.Type = PinType.Place;
                pin1.Label = "user1";
            }
            else if (topic == topic2)
            {
                pin2.Position = position;
                pin2.Address = position.ToString();
                pin2.Type = PinType.Place;
                pin2.Label = "user2";
            }
            
        }
        private bool StartLocationUpdate()
        {
            // 設定每5秒更新一次位置
            Device.StartTimer(TimeSpan.FromSeconds(3), () =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await GetAndDisplayLocation();
                    CountDistance(polyline.Geopath, "self");
                });

                // 返回 true，以繼續定時器的執行；返回 false，以停止定時器。
                return true;
            });

            return true;
        }
        public async Task GetAndDisplayLocation()
        {
            try
            {
                var location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(30)
                });
                Position loc = new Position(location.Latitude, location.Longitude);
                if (lastPosition != loc)
                {
                    // 添加新的位置到 Polyline
                    polyline.Geopath.Add(loc);
                    lastPosition = loc;
                }

            }
            catch (Exception ex)
            {
                return;
                // 处理异常
            }
        }
        public void AddPin()
        {
            Xamarin.Forms.Maps.Position position = new Xamarin.Forms.Maps.Position(24.13333, 120.68333);
            // Instantiate a Circle
            Circle circle = new Circle
            {
                Center = position,
                Radius = new Distance(250),
                StrokeColor = Color.Red,
                StrokeWidth = 8,
                FillColor = new Color(1, 0, 0, 0.2)
            };
            Pin pin = new Pin
            {
                Label = "Santa Cruz",
                Address = "The city with a boardwalk",
                Type = PinType.Place,
                Position = position
            };
            map.Pins.Add(pin);
            // Add the Circle to the map's MapElements collection
            map.MapElements.Add(circle);

            // Initialize the Polyline
            polyline = new Polyline
            {
                StrokeColor = Color.Blue,
                StrokeWidth = 12
            };
            map.MapElements.Add(polyline);
        }

        private void OnLabelTapped(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            if (label == mqtt1)
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(lastPosition, Distance.FromKilometers(1)));
            }
            else if (label == mqtt2)
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(pin1.Position, Distance.FromKilometers(1)));
            }
            else if (label == mqtt3)
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(pin2.Position, Distance.FromKilometers(1)));
            }
        }
        private void reset(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            if (clickedButton == re1)
            {
                // clear polyline
                polyline.Geopath.Clear();

            }
            else if (clickedButton == re2)
            {
                // clear polyline
                user1.Geopath.Clear();
                mqtt2.Text = "0km";
            }
            else if (clickedButton == re3)
            {
                user2.Geopath.Clear();
                mqtt3.Text =  "0km";
                // clear polyline

            }


        }
     }
}
