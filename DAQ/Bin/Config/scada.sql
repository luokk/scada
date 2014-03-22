
#
# ReadMe: Run Scada.DAQ.Installer.exe --init-database
# To Create Database and tables.
#


DROP TABLE IF EXISTS `weather`;
CREATE TABLE `weather` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Time` datetime DEFAULT NULL,
  `Windspeed` varchar(8) DEFAULT NULL,
  `Direction` varchar(8) DEFAULT NULL,
  `Temperature` varchar(8) DEFAULT NULL,
  `Humidity` varchar(8) DEFAULT NULL,
  `Pressure` varchar(8) DEFAULT NULL,
  `Raingauge` varchar(8) DEFAULT NULL,
  `Rainspeed` varchar(8) DEFAULT NULL,
  `Dewpoint` varchar(8) DEFAULT NULL,
  `IfRain` bit(1) DEFAULT NULL,
  `Alarm` bit(1) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;


DROP TABLE IF EXISTS `hpic_rec`;
CREATE TABLE `hpic_rec` (
`Id` int(11) NOT NULL AUTO_INCREMENT, /*采样ID,唯一号*/
`Time` datetime DEFAULT NULL,
`Doserate` varchar(18)  DEFAULT NULL, /*剂量率值，单位：nGy/h，数据格式：N14.2*/
`Highvoltage` varchar(8)  DEFAULT NULL, /*高压值，单位：V，数据格式：N4.2*/
`Battery` varchar(8), /*电池电压，单位：V，数据格式：N4.2*/
`Temperature` varchar(8), /*探测器温度，单位：℃，数据格式：N4.2*/
`Alarm` bit(1), /*报警，单位：无；数据格式：0、1、2,，代表不同的报警类型，保留字段*/
PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;


DROP TABLE IF EXISTS `HVSampler_rec`;
CREATE TABLE `HVSampler_rec` (
`Id` int(11) NOT NULL AUTO_INCREMENT,
`Sid` int(11) NOT NULL, /*采样ID,唯一号*/
`Time` datetime DEFAULT NULL,
`Volume` varchar(8), /*真空泵开关状态，单位：无；数据格式：0或1表示开关*/
`Flow` varchar(8), /*报警，单位：无；数据格式：0、1、2,，代表不同的报警类型，保留字段*/
`Hours` varchar(8),
`Status` bit,
`BeginTime` datetime,
`EndTime` datetime,
`Alarm1` bit, /*滤纸*/
`Alarm2` bit, /*流量*/
`Alarm3` bit, /*主电源*/
PRIMARY KEY (`Id`)
)ENGINE=MyISAM DEFAULT CHARSET=utf8;


DROP TABLE IF EXISTS `ISampler_rec`;
CREATE TABLE `ISampler_rec` (
`Id` int(11) NOT NULL AUTO_INCREMENT,
`Sid` int(11) NOT NULL, /*采样ID,唯一号*/
`Time` datetime DEFAULT NULL,
`Volume` varchar(8), /*真空泵开关状态，单位：无；数据格式：0或1表示开关*/
`Flow` varchar(8), /*报警，单位：无；数据格式：0、1、2,，代表不同的报警类型，保留字段*/
`Hours` varchar(8),
`Status` bit,
`BeginTime` datetime,
`EndTime` datetime,
`Alarm1` bit,
`Alarm2` bit,
`Alarm3` bit,
PRIMARY KEY (`Id`)
)ENGINE=MyISAM DEFAULT CHARSET=utf8;


DROP TABLE IF EXISTS `RDSampler_rec`;
CREATE TABLE `RDSampler_rec` (
`Id` int(11) NOT NULL AUTO_INCREMENT, /*采样ID,唯一号*/
`Time` datetime DEFAULT NULL,
`IfRain` bit, /*感雨，单位：无，数据格式：0或1表示是否下雨*/
`Barrel` int(4), /*桶状态，单位：无，数据格式：0、1、2表示哪个桶正在使用*/
`Alarm` bit,/*报警，单位：无；数据格式：0、1、2,，代表不同的报警类型，保留字段*/
`IsLidOpen` bit,
`CurrentRainTime` varchar(10) ,
`BeginTime` datetime DEFAULT NULL,/*开始时间，保留字段*/
`Endtime` datetime DEFAULT NULL,/*开始时间，保留字段*/
PRIMARY KEY (`Id`)
)ENGINE=MyISAM DEFAULT CHARSET=utf8;



DROP TABLE IF EXISTS `Environment_rec`;
CREATE TABLE `Environment_rec` (
`Id` int(11) NOT NULL AUTO_INCREMENT, /*采样ID,唯一号*/
`Time` datetime DEFAULT NULL,
`Temperature` varchar(8), /*温度，单位：℃，数据格式：N8*/
`Humidity` varchar(8), /*湿度，单位：%，数据格式：N8*/
`IfMainPowerOff` bit,
`BatteryHours` varchar(10),
`IfSmoke` bit, /*烟感报警，单位：无，数据格式：0或1表示是否报警*/
`IfWater` bit, /*浸水报警，单位：无，数据格式：0或1表示是否报警*/
`IfDoorOpen` bit, /*门禁报警，单位：无，数据格式：0或1表示是否报警*/
`Alarm` bit, /*报警，单位：无；数据格式：0、1、2,，代表不同的报警类型，保留字段*/
PRIMARY KEY (`Id`)
)ENGINE=MyISAM DEFAULT CHARSET=utf8;

CREATE TABLE `environment_door_rec` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Time` datetime DEFAULT NULL,
  `IfDoorOpen` bit(1) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM AUTO_INCREMENT=5 DEFAULT CHARSET=utf8;




DROP TABLE IF EXISTS `NaI_Rec`;
CREATE TABLE `NaI_Rec` (
`Id` int(11) NOT NULL AUTO_INCREMENT, /*采样ID,唯一号*/
`Time` datetime, 
`StartTime` datetime, 
`EndTime` datetime, 
`Coefficients` varchar(48), 
`ChannelData` varchar(10000), 
`DoseRate` varchar(16), 
`Temperature` varchar(16), 
`HighVoltage` varchar(16),
`NuclideFound` bit,
`EnergyFromPosition` varchar(16),
PRIMARY KEY (`Id`)
)ENGINE=MyISAM DEFAULT CHARSET=utf8;


DROP TABLE IF EXISTS `NaINuclide_Rec`;
CREATE TABLE `NaINuclide_Rec` (
`Id` int(11) NOT NULL AUTO_INCREMENT, /*采样ID,唯一号*/
`Time` datetime,
`Name` varchar(16),
`Activity` varchar(32),
`Indication` varchar(16),
`DoseRate` varchar(16),
`Channel` varchar(16), 
`Energy` varchar(16),
PRIMARY KEY (`Id`)
)ENGINE=MyISAM DEFAULT CHARSET=utf8;