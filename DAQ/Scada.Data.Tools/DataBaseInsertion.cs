using MySql.Data.MySqlClient;
using Scada.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Scada.Data.Tools
{
    class DataBaseInsertion
    {
        private string connectionString = null;

        private int v = 0;

        private string device;

        private static DateTime beginTime = default(DateTime);

        public DataBaseInsertion(string device)
        {
            this.device = device;
        }

        private int Interval
        {
            get { return 30; }
        }

        public int RecordInterval
        {
            get;
            set;
        }


        internal void Execute()
        {
            this.connectionString = new DBConnectionString().ToString();
            using (var connToMySql = new MySqlConnection(connectionString))
            {
                connToMySql.Open();

                using (MySqlCommand cmd = connToMySql.CreateCommand())
                {
                    DateTime t = GetBaseTime(DateTime.Now);
                    while (true)
                    {
                        if (this.device != "nai")
                        {
                            t = t.AddSeconds(this.Interval);
                        }
                        else
                        {
                            t = t.AddSeconds(60 * 5);
                        }
                        ExecuteSQL(cmd, t);
                        Thread.Sleep(this.RecordInterval * 1000);
                    }
                }
            }
        }

        private DateTime GetBaseTime(DateTime startTime)
        {
            // 目前只支持30秒 和 5分钟两种间隔
            // Debug.Assert(this.Interval == 30 || this.Interval == 60 * 5 || this.Interval == 0);

            DateTime baseTime = default(DateTime);
            if (this.Interval == 30)
            {
                int second = startTime.Second / 30 * 30;
                baseTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, second);
            }
            else if (this.Interval == 60 * 5)
            {
                int min = startTime.Minute / 5 * 5;
                baseTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, min, 0);
            }
            return baseTime;
        }

        internal void Execute(string content)
        {
            
        }

        internal void ExecuteSQL(MySqlCommand cmd, DateTime t)
        {
            if (this.device.ToLower() == "hpic")
            {
                cmd.CommandText = "insert into HPIC_rec(time, doserate, highvoltage, battery, temperature, alarm) values(@1, @2, @3, 123, 24, 1)";
                cmd.Parameters.AddWithValue("@1", t);

                v = (v + 1) % 5;
                long tick = DateTime.Now.Ticks;
                Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
                v = ran.Next(80, 120);
                double d = v;
                cmd.Parameters.AddWithValue("@2", d);

                v = ran.Next(100, 103);
                cmd.Parameters.AddWithValue("@3", v * 4);


                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            else if (this.device.ToLower() == "weather")
            {
                cmd.CommandText = "insert into weather(time, Windspeed, Direction, Temperature, Humidity, Pressure, Raingauge, Rainspeed, Dewpoint, IfRain, alarm) " 
                                    + "values(@1, 0, 360, @2, @3, 1000.1, 1, 1, 0, 1, 0)";
                cmd.Parameters.AddWithValue("@1", t);

                v = (v + 1) % 5;
                double d = double.Parse("1." + v);
                cmd.Parameters.AddWithValue("@2", d);

                double h = double.Parse("20." + v);
                cmd.Parameters.AddWithValue("@3", h);

                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            else if (this.device.ToLower() == "shelter")
            {
                cmd.CommandText = "insert into environment_rec(time, Temperature, Humidity, IfMainPowerOff, BatteryHours, IfSmoke, IfWater, IfDoorOpen, Alarm)" 
                                    + " values(@1, @2, @3, 0, 4, 0, 0, 0, 0)";
                cmd.Parameters.AddWithValue("@1", t);

                v = (v + 1) % 5;
                double d = double.Parse("2" + v);
                cmd.Parameters.AddWithValue("@2", d);

                double h = double.Parse("3" + v);
                cmd.Parameters.AddWithValue("@3", h);

                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            else if (this.device.ToLower() == "nai")
            {
                t = new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0);
                cmd.CommandText = "insert into nai_rec(time, StartTime, EndTime, Coefficients, ChannelData, DoseRate, Temperature,HighVoltage,NuclideFound,EnergyFromPosition) values(@1, @2, @3, @4, @5, @6, 24, 400.1, 1, 1460.83)";
                cmd.Parameters.AddWithValue("@1", t);
                cmd.Parameters.AddWithValue("@2", t.AddMinutes(-5));
                cmd.Parameters.AddWithValue("@3", t);

                cmd.Parameters.AddWithValue("@4", "-4.94022E+00 1.37924E+00 9.68201E-05");
                cmd.Parameters.AddWithValue("@5", channcelData);

                long tick = DateTime.Now.Ticks;
                Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));

                v = ran.Next(60, 80);
                cmd.Parameters.AddWithValue("@6", v);

                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

                string indication = "100";
                if (v < 65)
                    indication = "75";
                string channel = "993";
                int d = ran.Next(118, 140);
                string doserate = ((double)d / 10.0).ToString();
                string energy = "1460.74";
                this.AddNuclideData(cmd, t, "K-40", "1.11E+01 4.00E+00", indication, doserate, channel, energy);

            }
            else if (this.device.ToLower() == "mds")
            {
                this.insertMdsAisData("hvsampler_rec", t, cmd);
            }
            else if (this.device.ToLower() == "ais")
            {
                this.insertMdsAisData("isampler_rec",t, cmd);
            }
            else if (this.device.ToLower() == "dwd")
            {
                /*
                 Time, IfRain, Barrel, Alarm, IsLidOpen, CurrentRainTime      
                 */
                t = new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0);
                cmd.CommandText = 
                    "insert into rdsampler_rec(time, IfRain, Barrel, Alarm, IsLidOpen, CurrentRainTime) " + 
                    "values(@1, @2, @3, @4, @5, @6)";
                cmd.Parameters.AddWithValue("@1", t);
                cmd.Parameters.AddWithValue("@2", 1);
                cmd.Parameters.AddWithValue("@3", "0");

                v += 5;
                int a = v / 10 * 10;

                cmd.Parameters.AddWithValue("@4", 0);
                cmd.Parameters.AddWithValue("@5", 0);
                cmd.Parameters.AddWithValue("@6", a.ToString());

                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }

        }

        private void AddNuclideData(MySqlCommand cmd, DateTime t, string name, string activity, string indication, string doserate, string channel, string energy)
        {
            cmd.CommandText = "insert into nainuclide_rec(time, Name, Activity, indication, doserate, channel, energy)"+
                " values(@1, @2, @3, @4, @5, @6, @7)";
            cmd.Parameters.AddWithValue("@1", t);
            cmd.Parameters.AddWithValue("@2", name);
            cmd.Parameters.AddWithValue("@3", activity);

            cmd.Parameters.AddWithValue("@4", indication);
            cmd.Parameters.AddWithValue("@5", doserate);
            cmd.Parameters.AddWithValue("@6", channel);
            cmd.Parameters.AddWithValue("@7", energy);

            cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();




        }

        /*
  CREATE TABLE `isampler_rec` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Sid` int(11) NOT NULL,
  `Time` datetime DEFAULT NULL,
  `Volume` varchar(8) DEFAULT NULL,
  `Flow` varchar(8) DEFAULT NULL,
  `Hours` varchar(8) DEFAULT NULL,
  `Status` bit(1) DEFAULT NULL,
  `BeginTime` datetime DEFAULT NULL,
  `EndTime` datetime DEFAULT NULL,
  `Alarm1` bit(1) DEFAULT NULL,
  `Alarm2` bit(1) DEFAULT NULL,
  `Alarm3` bit(1) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

         */
        private void insertMdsAisData(string tableName, DateTime t, MySqlCommand cmd)
        {
            t = new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0);

            if (beginTime == default(DateTime))
            {
                beginTime = t;
            }

            var c = string.Format("insert into {0}(Sid, time, Volume, Flow, Hours, Status, BeginTime, EndTime, Alarm1, Alarm2, Alarm3) values(@1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11)", tableName);
            cmd.CommandText = c;

            cmd.Parameters.AddWithValue("@1", "29");
            cmd.Parameters.AddWithValue("@2", t);
            cmd.Parameters.AddWithValue("@3", "200");
            cmd.Parameters.AddWithValue("@4", "42");
            cmd.Parameters.AddWithValue("@5", "4");
            cmd.Parameters.AddWithValue("@6", 1);

            cmd.Parameters.AddWithValue("@7", beginTime);
            cmd.Parameters.AddWithValue("@8", "0000-00-00 00:00:00");
            cmd.Parameters.AddWithValue("@9",  0);
            cmd.Parameters.AddWithValue("@10", 0);
            cmd.Parameters.AddWithValue("@11", 0);

            cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();

        }




        private static string channcelData = @"0 0 0 0 0 0 0 0 0 0 0 0 10 43 129 171 206 227 218 244 264 238 266 273 280 257 309 299 355 347 340 349 317 363 393 406 374 414 442 451 538 544 573 587 623 654 685 789 854 867 930 890 979 1063 1101 1065 1138 1238 1278 1253 1339 1280 1369 1402 1368 1393 1449 1464 1343 1405 1436 1375 1408 1422 1415 1427 1417 1425 1401 1425 1393 1443 1313 1401 1370 1385 1301 1311 1339 1309 1292 1277 1259 1306 1209 1241 1260 1236 1231 1278 1163 1255 1169 1209 1123 1111 1075 1078 1136 1074 1070 1004 1073 1021 1023 1017 1054 1002 886 964 915 899 928 875 938 869 861 847 822 807 780 848 775 774 751 780 740 730 747 757 744 783 740 741 713 711 736 692 735 692 665 645 667 638 616 630 642 621 598 607 653 617 608 583 589 540 582 534 568 540 539 565 583 596 557 557 595 552 556 540 587 525 586 531 560 528 540 517 471 487 481 453 498 434 407 440 435 372 402 383 386 354 398 392 397 349 371 369 351 339 330 365 357 342 353 324 338 327 337 343 306 304 326 346 330 316 333 269 298 304 264 291 281 278 257 271 254 265 278 270 278 252 247 269 283 245 249 291 248 276 279 253 262 245 250 256 258 264 240 251 232 253 230 250 252 253 269 214 246 241 208 214 232 190 196 202 198 202 214 188 179 182 173 177 185 158 190 174 192 155 153 174 184 160 171 173 154 165 165 155 153 176 145 143 128 143 164 172 152 148 146 150 146 145 153 147 138 141 134 124 134 148 135 127 154 132 112 130 113 125 112 116 108 118 134 111 142 131 139 112 123 120 124 133 121 112 124 131 133 109 123 107 133 105 111 111 126 123 126 120 107 109 102 131 120 120 112 113 125 87 119 113 121 131 122 116 118 116 112 105 112 117 116 113 91 106 104 110 102 97 109 121 102 114 94 98 94 107 103 99 91 91 94 102 85 102 84 79 109 98 92 86 88 81 103 83 89 101 105 85 100 91 94 122 100 99 105 112 106 115 110 102 98 119 133 123 102 102 95 105 109 103 95 110 93 83 93 110 126 118 101 95 100 91 88 97 91 103 86 100 77 81 87 76 80 82 59 78 63 74 74 66 66 66 75 73 58 68 57 69 65 58 71 55 57 55 69 69 64 74 67 58 49 73 64 55 52 65 54 47 55 65 58 63 48 57 58 56 68 61 53 51 64 72 38 52 56 63 47 62 49 44 61 52 49 63 71 72 54 54 61 55 51 53 60 54 59 68 72 58 60 47 61 62 42 69 54 53 42 61 49 64 62 65 47 65 51 54 50 46 66 47 56 56 61 49 48 56 53 39 50 39 46 45 45 49 55 44 50 55 48 39 37 46 42 52 36 40 50 35 56 37 42 57 45 42 52 49 49 44 41 44 53 45 59 50 48 52 57 59 33 42 43 41 42 43 39 43 36 53 45 51 45 43 49 49 49 38 40 49 46 33 45 49 50 50 50 47 76 34 46 47 45 61 49 53 54 53 56 43 52 49 59 52 54 56 55 46 60 50 45 57 65 54 52 58 51 49 46 40 47 53 53 36 47 52 48 56 45 48 53 50 37 40 48 43 61 46 38 55 43 45 52 49 62 34 55 42 41 44 37 38 28 34 50 52 46 37 25 36 27 33 34 29 42 39 34 31 27 28 33 36 33 26 31 38 27 38 36 30 28 27 31 28 30 29 22 27 24 36 22 34 31 28 32 26 34 29 27 29 31 28 28 31 42 28 25 30 30 25 24 31 28 32 27 39 23 32 44 29 36 30 36 37 28 33 41 38 29 35 39 31 40 29 41 39 28 44 30 44 31 29 39 32 28 37 26 27 46 31 31 27 36 33 37 23 29 30 31 30 41 40 27 29 28 28 24 30 14 24 28 28 29 21 25 26 30 26 30 29 24 23 35 25 28 28 22 18 20 27 28 21 22 30 19 27 21 28 22 25 31 20 25 21 16 20 17 26 25 29 21 20 15 19 18 19 19 29 22 23 24 14 23 22 25 20 22 20 26 9 22 16 19 16 12 12 20 20 13 15 21 19 21 22 14 16 17 18 21 10 13 16 21 24 17 9 12 14 17 21 11 11 11 16 14 18 10 17 17 13 16 12 10 13 14 14 13 14 20 17 14 12 17 20 23 16 18 22 16 9 16 18 13 20 21 20 19 14 18 14 21 26 23 20 25 21 19 25 21 31 25 41 33 33 33 43 47 37 31 43 40 47 45 47 50 65 65 57 58 65 63 68 55 63 70 76 55 79 57 64 75 64 82 70 74 71 75 64 72 69 74 67 70 59 61 66 53 57 63 64 49 48 35 50 43 47 37 43 45 48 36 29 31 28 31 36 28 15 15 16 21 15 30 17 17 22 15 14 16 17 17 19 11 12 10 14 8 12 10 12 13 15 8 9 9 9 22 17 6 8 7 6 9 7 2 12 7 6 6 7 7 5 11 11 7 9 9 9 5 8 9 5 14 8 8 9 8 7 6 7 9 7 7 6 6 12 9 11 7 7 5 3 4 11 8 6 5 8 5 5 11 7 8 8 5 1 4 4 3 8 5 4 6 7 6 7 4 5 4 6 7 6 10 5 7 4 5 5 12 4 5 5 5 6 6 9 11 9 6 7 11 6 5 10 5 8 5 14 9 8 7 8 7 12 11 6 10 9 8 14 11 9 9 17 10 10 6 9 10 9 3 9 4 9 14 10 5 10 5 5 5 2 11 4 5 9 7 8 7 6 7 5 5 5 1 4 5 3 6 11 4 8 2 6 2 4 4 5 2 3 8 4 6 5 6 8 3 2 7 3 7 6 8 4 3 5 6 3 7 5 5 2 5 5 4 6 2 9 3 7 3 3 3 1 4 6 4 3 2 3 4 4 1 2 1 0 1 2 3 4 5 1 4 1 2 1 1 6 0 4 5 1 5 4 4 3 3 4 3 4 4 2 0 3 6 5 4 3 6 2 3 2 2 6 5 4 1 5 2 3 4 0 1 3 3 3 3 3 3 2 3 3 3 2 5 4 3 3 2 4 3 5 3 5 6 3 4 4 1 4 3 5 5 1 5 8 3 6 4 6 5 9 5 4 7 4 7 2 5 5 6 6 7 6 4 3 9 5 3 6 4 9 3 4 13 5 7 8 10 11 2 5 7 5 6 5 4 4 4 4 8 6 3 3 4 5 7 4 5 5 6 6 5 4 7 1 7 3 7 6 9 3 5 7 8 5 4 3 7 2 2 6 2 3 5 3 3 3 7 8 6 5 4 7 3 3 9 3 7 4 6 4 5 3 2 4 6 4 1 6 4 3 5 7 3 4 5 2 5 4 4 4 1 5 3 2 4 1 3 5 1 4 2 6 2 5 6 1 6 1 1 2 0 5 2 1 2 2 3 4 6 3 2 3 5 6 2 2 1 4 2 2 2 2 2 3 2 7 0 3 6 4 3 6 1 2 0 1 5 1 7 2 2 2 0 1 3 2 2 1 2 3 2 0 1 1 0 2 1 0 3 6 3 3 1 1 0 3 3 1 2 1 2 1 3 1 2 2 1 0 4 0 6 3 1 1 1 2 1 3 1 2 1 0 0 1 0 1 1 2 1 4 0 2 4 5 1 4 2 2 2 2 3 3 5 2 2 5 2 3 5 5 1 2 4 5 4 3 0 5 0 5 8 2 4 6 12 7 5 6 8 6 6 6 5 12 3 9 15 6 5 7 5 4 12 11 15 13 8 8 8 15 5 4 9 7 10 5 7 12 9 6 4 5 10 11 5 6 8 11 4 5 2 6 5 8 6 5 9 5 4 1 2 7 7 4 3 2 6 1 2 1 2 2 1 5 3 3 3 3 2 2 1 1 0 3 3 3 0 1 2 0 0 0 0 0 1 1 1 0 2 0 0 0 0 1 1 0 0 2 0 1 1 0 0 0 0 0 0 0 1 0 0 0 1 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 ";
        
    }
}
