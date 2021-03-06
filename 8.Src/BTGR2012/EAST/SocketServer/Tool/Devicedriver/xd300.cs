﻿using System;
using System.Data;
using System.Windows.Forms;

namespace Tool
{
    class xd300
    {
        //数据结构
        #region
        public struct XD300Buffer
        {
            public Info _Info;
            public Data _Data;
            public bool _SaveDatas;         
            public Command[] _Command;
            public int _LastCommandIndex;
        }

        public struct Info
        {
            public int _id;
            public string _name;
            public string _remark;
            public string _dturegister;
            public string _ip;
            public byte _addr;
            public int _cycle;
            public int _timeout;
            public int _retrytimes;
            public bool _state;
        }

        public struct Data
        {
            public DateTime _dt;
            public int _n;
            public string _person;
            public string _card;
            public int _count;
        }

        public struct Command
        {
            public byte[] _cmd;
            public DateTime _senddt;
            public DateTime _backdt;
            public string _introduce;
            public int _timeoutnow;
            public int _retrytimesnow;
            public bool _send;
            public bool _back;
            public bool _onoff;
        }

        public static XD300Buffer[] _XD300Buffer;
        #endregion

        //数据处理方法
        #region
        //获取记录总数 功能码0x06
        public static byte[] Get_recordcount(byte address)
        {
            byte[] outByte = new byte[9];
            outByte[0] = 0x21;
            outByte[1] = 0x58;
            outByte[2] = 0x44;
            outByte[3] = address;
            outByte[4] = 0xb0;
            outByte[5] = 0x06;
            outByte[6] = 0x00;
            return DataInfo.CRC16(outByte);
        }       

        //解析数据总数
        public static int Read_recordcount(byte[] inByte)
        {
            //成功后返回： ！＋X＋D＋地址＋设备类型＋功能码＋数据数（0x01）＋记录总条数＋CRC16
            int count=-1;
            count = inByte[7];
            return count;
        }

        //读取第N条记录 功能码0x07
        public static byte[] Get_recordn(byte address, int n)
        {
            byte[] outByte = new byte[10];
            outByte[0] = 0x21;
            outByte[1] = 0x58;
            outByte[2] = 0x44;
            outByte[3] = address;
            outByte[4] = 0xb0;
            outByte[5] = 0x07;
            outByte[6] = 0x01;
            outByte[7] = Convert.ToByte(n%256);
            return DataInfo.CRC16(outByte);
        }

        //解析记录
        public static Data Read_recordn(byte[] inByte)
        {
            //成功后返回： ！＋X＋D＋地址＋设备类型＋功能码＋数据数（0x10）＋第N条记录＋CRC16
            //如果N取值不正确，则返回：！＋X＋D＋地址＋设备类型＋功能码＋数据数（0x01）＋记录总条数＋CRC16
            //记录格式：记录号（两字节）＋秒＋分＋时＋日＋月＋年＋卡号（八字节）
            Data _xdata = new Data();
            _xdata._n = inByte[7]*256+inByte[8];
            try
            {
                string date = "20" + inByte[14].ToString("x2") + "-" + inByte[13].ToString("x2") + "-" + inByte[12].ToString("x2") + " " + inByte[11].ToString("x2") + ":" + inByte[10].ToString("x2") + ":" + inByte[9].ToString("x2");
                _xdata._dt = Convert.ToDateTime(date);
            }
            catch { }
            string _card = inByte[15].ToString("x2") + inByte[16].ToString("x2") + inByte[17].ToString("x2") + inByte[18].ToString("x2") + inByte[19].ToString("x2") + inByte[20].ToString("x2") + inByte[21].ToString("x2") + inByte[22].ToString("x2");
            _xdata._card = _card;
            _xdata._person = _card;
            return _xdata;
        }

        //删除记录 功能码0x08
        public static byte[] Set_recordclean(byte address)
        {
            byte[] outByte = new byte[9];
            outByte[0] = 0x21;
            outByte[1] = 0x58;
            outByte[2] = 0x44;
            outByte[3] = address;
            outByte[4] = 0xb0;
            outByte[5] = 0x08;
            outByte[6] = 0x00;
            return DataInfo.CRC16(outByte);
        }

        //删除当前记录 功能码0x0a
        public static byte[] Set_recordcleannow(byte address)
        {
            byte[] outByte = new byte[9];
            outByte[0] = 0x21;
            outByte[1] = 0x58;
            outByte[2] = 0x44;
            outByte[3] = address;
            outByte[4] = 0xb0;
            outByte[5] = 0x0a;
            outByte[6] = 0x00;
            return DataInfo.CRC16(outByte);
        }

        //设置时间
        public static byte[] Set_time(byte address)
        {
            byte[] outByte = new byte[12];
            outByte[0] = 0x21;
            outByte[1] = 0x58;
            outByte[2] = 0x44;
            outByte[3] = address;
            outByte[4] = 0xb0;
            outByte[5] = 0x01;
            outByte[6] = 0x03;
            outByte[7] = DataInfo.bytetoBCD(Convert.ToByte(DateTime.Now.Second));
            outByte[8] = DataInfo.bytetoBCD(Convert.ToByte(DateTime.Now.Minute));
            outByte[9] = DataInfo.bytetoBCD(Convert.ToByte(DateTime.Now.Hour));
            return DataInfo.CRC16(outByte);
        }

        //设置日期
        public static byte[] Set_date(byte address)
        {
            byte[] outByte = new byte[12];
            outByte[0] = 0x21;
            outByte[1] = 0x58;
            outByte[2] = 0x44;
            outByte[3] = address;
            outByte[4] = 0xb0;
            outByte[5] = 0x02;
            outByte[6] = 0x03;
            outByte[7] = DataInfo.bytetoBCD(Convert.ToByte(DateTime.Now.Day));
            outByte[8] = DataInfo.bytetoBCD(Convert.ToByte(DateTime.Now.Month));
            outByte[9] = DataInfo.bytetoBCD(Convert.ToByte(DateTime.Now.Year % 100));

            return DataInfo.CRC16(outByte);
        }

        //获取人物卡号对应关系
        public static string Get_person(string card)
        {
            string sql = "select [Person] from tblCard where [CardSN]='" + card + "'";
            string person =DB.getStr(sql);
            if (person == null)
            {
                person = card;
            }
            return person;
        }
        #endregion

        //存储
        #region
        public static void InsertSql(XD300Buffer bd)
        {
            string str = string.Format("INSERT INTO tblXGData (DT,DeviceID,Person) VALUES ('{0}',{1},'{2}')", bd._Data._dt, bd._Info._id, bd._Data._person);
            Tool.DB.runCmd(str);
        }
        #endregion

        //加载缓存
        #region
        //建立xd300数据缓存
        public static void Load_XD300Buffer()
        {
            try
            {
                string sql = "SELECT [DeviceID],[StationName], [DeviceAddress], [IPAddress], [Remark], [Cycle], [Timeout], [RetryTimes] FROM [vDevice] where [DeviceType]='xd300' and [Deleted]=0";
                DataTable dt = Tool.DB.getDt(sql);
                _XD300Buffer = new XD300Buffer[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    _XD300Buffer[i]._Info._id = int.Parse(dt.Rows[i]["DeviceID"].ToString());
                    _XD300Buffer[i]._Info._name = dt.Rows[i]["StationName"].ToString();
                    _XD300Buffer[i]._Info._remark = dt.Rows[i]["Remark"].ToString();

                    _XD300Buffer[i]._Info._dturegister = "";
                    _XD300Buffer[i]._Info._ip = dt.Rows[i]["IPAddress"].ToString();
                    _XD300Buffer[i]._Info._cycle = int.Parse(dt.Rows[i]["Cycle"].ToString());
                    _XD300Buffer[i]._Info._addr = Convert.ToByte(dt.Rows[i]["DeviceAddress"].ToString());
                    _XD300Buffer[i]._Info._timeout = int.Parse(dt.Rows[i]["Timeout"].ToString());
                    _XD300Buffer[i]._Info._retrytimes = int.Parse(dt.Rows[i]["RetryTimes"].ToString());
                    _XD300Buffer[i]._Info._state = true;

                    string sql2 = "SELECT [XGDataID], [DT], [Person] FROM [vXGDataLast] where [DeviceID]=" + _XD300Buffer[i]._Info._id.ToString();
                    DataTable dt1 = Tool.DB.getDt(sql2);
                    if (dt1.Rows.Count > 0)
                    {
                        _XD300Buffer[i]._Data._dt = Convert.ToDateTime(dt1.Rows[0]["DT"].ToString());
                        _XD300Buffer[i]._Data._person = dt1.Rows[0]["Person"].ToString();
                    }

                    _XD300Buffer[i]._Command = new Command[6];
                    _XD300Buffer[i]._Command[0]._cmd = Get_recordcount(_XD300Buffer[i]._Info._addr);
                    _XD300Buffer[i]._Command[0]._introduce = "采集记录条数";
                    _XD300Buffer[i]._Command[1]._cmd = Get_recordn(_XD300Buffer[i]._Info._addr,0);
                    _XD300Buffer[i]._Command[1]._introduce = "采集第N条记录";
                    _XD300Buffer[i]._Command[2]._cmd = Set_recordcleannow(_XD300Buffer[i]._Info._addr);
                    _XD300Buffer[i]._Command[2]._introduce = "清除当前记录";
                    _XD300Buffer[i]._Command[3]._cmd = Set_recordclean(_XD300Buffer[i]._Info._addr);
                    _XD300Buffer[i]._Command[3]._introduce = "清除所有记录";
                    _XD300Buffer[i]._Command[4]._cmd = Set_date(_XD300Buffer[i]._Info._addr);
                    _XD300Buffer[i]._Command[4]._introduce = "同步日期";
                    _XD300Buffer[i]._Command[5]._cmd = Set_time(_XD300Buffer[i]._Info._addr);
                    _XD300Buffer[i]._Command[5]._introduce = "同步时间";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("建立xd300数据缓存失败！" + ex, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        //数据处理逻辑
        #region
        //xd300数据处理
        public static void Deal_XD300Data(byte[] inbyte, string ip)
        {
            //检测是否为xd300数据
            //判断依据  1、CRC 2、ip 3、器件地址
            //0x21 0x58 0x44 地址 设备类型0xb0
            if (false == DataInfo.CheckCRC(inbyte))
            {
                return;
            }

            if (inbyte[0] != 0x21 || inbyte[1] != 0x58 || inbyte[2] != 0x44 || inbyte[4] != 0xB0)
            {
                return;
            }

            int busfferlistID = -1;
            for (int i = 0; i < _XD300Buffer.Length; i++)
            {
                if (_XD300Buffer[i]._Info._ip == ip && _XD300Buffer[i]._Info._addr == inbyte[3])
                {
                    busfferlistID = i;
                    break;
                }
            }

            if (busfferlistID == -1)
            {
                return;
            }

            //功能码 0x0a 主动上报
            if (inbyte[5] == 0x0a)
            {
                Data da = Read_recordn(inbyte);
                if (Convert.ToDateTime(da._dt) >Convert.ToDateTime( _XD300Buffer[busfferlistID]._Data._dt))
                {
                    _XD300Buffer[busfferlistID]._Data = Read_recordn(inbyte);
                    _XD300Buffer[busfferlistID]._Data._person = Get_person(_XD300Buffer[busfferlistID]._Data._card);
                }
                _XD300Buffer[busfferlistID]._Command[2]._cmd = Set_recordcleannow(_XD300Buffer[busfferlistID]._Info._addr);
                _XD300Buffer[busfferlistID]._Command[2]._onoff = true;
                _XD300Buffer[busfferlistID]._SaveDatas = true;
                TimeSpan ts =DateTime.Now - Convert.ToDateTime(_XD300Buffer[busfferlistID]._Data._dt);
                if (Math.Abs(ts.TotalMinutes) > 3)
                {
                    if (DateTime.Now.Date != Convert.ToDateTime(_XD300Buffer[busfferlistID]._Data._dt).Date)
                    {
                        Tool.xd300._XD300Buffer[busfferlistID]._Command[3]._cmd = Tool.xd300.Set_date(Tool.xd300._XD300Buffer[busfferlistID]._Info._addr);
                        Tool.xd300._XD300Buffer[busfferlistID]._Command[3]._onoff = true;
                    }
                    Tool.xd300._XD300Buffer[busfferlistID]._Command[4]._cmd = Tool.xd300.Set_time(Tool.xd300._XD300Buffer[busfferlistID]._Info._addr);
                    Tool.xd300._XD300Buffer[busfferlistID]._Command[4]._onoff = true;
                }
                return;
            }
            //功能码 0x06 读取记录总条数
            if (inbyte[5] == 0x06)
            {
                _XD300Buffer[busfferlistID]._Data._count = Read_recordcount(inbyte);
                if (_XD300Buffer[busfferlistID]._Data._count > 0)
                {
                    _XD300Buffer[busfferlistID]._Command[1]._cmd = Get_recordn(_XD300Buffer[busfferlistID]._Info._addr,1);
                    _XD300Buffer[busfferlistID]._Command[1]._onoff = true;
                }
            }
            //功能码 0x07 读取第N条记录
            if (inbyte[5] == 0x07)
            {
                _XD300Buffer[busfferlistID]._Data = Read_recordn(inbyte);
                _XD300Buffer[busfferlistID]._Data._person = Get_person(_XD300Buffer[busfferlistID]._Data._card);
                if (_XD300Buffer[busfferlistID]._Data._n < _XD300Buffer[busfferlistID]._Data._count)
                {
                    //读下条记录
                    _XD300Buffer[busfferlistID]._Command[1]._cmd = Get_recordn(_XD300Buffer[busfferlistID]._Info._addr,_XD300Buffer[busfferlistID]._Data._n + 1);
                    _XD300Buffer[busfferlistID]._Command[1]._onoff = true;

                }
                if (_XD300Buffer[busfferlistID]._Data._n == _XD300Buffer[busfferlistID]._Data._count && _XD300Buffer[busfferlistID]._Data._count > 0)
                {
                    //清除总记录
                    _XD300Buffer[busfferlistID]._Command[3]._cmd = Set_recordclean(_XD300Buffer[busfferlistID]._Info._addr);
                    _XD300Buffer[busfferlistID]._Command[3]._onoff = true;
                    _XD300Buffer[busfferlistID]._Data._count = 0;

                }
                _XD300Buffer[busfferlistID]._SaveDatas = true;
            }
            //功能码 0x08 清除记录
            if (inbyte[5] == 0x08)
            {

            }
            //功能码 0x01 修改日期
            if (inbyte[5] == 0x01)
            {
            }
            //功能码 0x02 修改时间
            if (inbyte[5] == 0x01)
            {
            }
            _XD300Buffer[busfferlistID]._Command[_XD300Buffer[busfferlistID]._LastCommandIndex]._onoff = false;
            _XD300Buffer[busfferlistID]._Command[_XD300Buffer[busfferlistID]._LastCommandIndex]._retrytimesnow = 0;
            _XD300Buffer[busfferlistID]._Command[_XD300Buffer[busfferlistID]._LastCommandIndex]._backdt = DateTime.Now;
            _XD300Buffer[busfferlistID]._Command[_XD300Buffer[busfferlistID]._LastCommandIndex]._back = true;
            _XD300Buffer[busfferlistID]._Command[_XD300Buffer[busfferlistID]._LastCommandIndex]._send = false;

            _XD300Buffer[busfferlistID]._Info._state = true;
            //只要有数据返回即解除ISocketRS占用
            Gprs.Gprs_IsOccupy(_XD300Buffer[busfferlistID]._Info._ip, false);
        }
        #endregion

        //任务
        #region
        //xd300发送任务
        public static void Polling_XD300Send()
        {
            for (int i = 0; i < _XD300Buffer.Length; i++)
            {
                for (int j = 0; j < _XD300Buffer[i]._Command.Length; j++)
                {
                    //判断命令是否开启
                    if (_XD300Buffer[i]._Command[j]._onoff == false)
                    {
                        continue;
                    }

                    //判断是否已发送
                    if (_XD300Buffer[i]._Command[j]._send == false)
                    {
                        //检测ISocketRS是否被占用和是否连接 
                        Gprs.GprsList gl = Gprs.Get_GprsList(_XD300Buffer[i]._Info._ip);
                        if (gl._Iscon == false || gl._Isbusy == true || gl._activate == false)
                        {
                            continue;
                        }
                        bool send_flg = Gprs.Gprs_send(gl, _XD300Buffer[i]._Command[j]._cmd);
                        if (send_flg == true)
                        {
                            _XD300Buffer[i]._Command[j]._send = true;
                            _XD300Buffer[i]._Command[j]._back = false;
                            _XD300Buffer[i]._Command[j]._timeoutnow = 0;
                            _XD300Buffer[i]._Command[j]._senddt = DateTime.Now;
                            _XD300Buffer[i]._LastCommandIndex = j;
                            //占用ISocketRS
                            Gprs.Gprs_IsOccupy(_XD300Buffer[i]._Info._ip, true);
                        }
                    }
                    //发送完成 开始计时 计算超时时间
                    else
                    {
                        _XD300Buffer[i]._Command[j]._timeoutnow++;
                    }

                    //判断超时
                    if (_XD300Buffer[i]._Command[j]._timeoutnow >= _XD300Buffer[i]._Info._timeout)
                    {
                        _XD300Buffer[i]._Command[j]._timeoutnow = 0;
                        _XD300Buffer[i]._Command[j]._send = false;
                        //解除ISocketRS占用
                        Gprs.Gprs_IsOccupy(_XD300Buffer[i]._Info._ip, false);
                        _XD300Buffer[i]._Command[j]._retrytimesnow++;
                    }

                    //判断重试次数 超过次数 设备故障
                    if (_XD300Buffer[i]._Command[j]._retrytimesnow >= _XD300Buffer[i]._Info._retrytimes)
                    {
                        _XD300Buffer[i]._Command[j]._onoff=false;
                        _XD300Buffer[i]._Command[j]._retrytimesnow = 0;
                        _XD300Buffer[i]._Info._state = false;
                    }
                    
                }
            }
        }
        //xd300存储任务
        public static void Polling_XD300Save()
        {
            for (int i = 0; i < _XD300Buffer.Length; i++)
            {
                if (_XD300Buffer[i]._SaveDatas == true)
                {
                    _XD300Buffer[i]._SaveDatas = false;
                    InsertSql(_XD300Buffer[i]);
                    return;
                }
            }
        }
        //循环命令生成
        public static void Polling_XD300Collect()
        {
            for (int i = 0; i <_XD300Buffer.Length; i++)
            {
                if ((DateTime.Now - _XD300Buffer[i]._Command[0]._senddt).TotalSeconds >= _XD300Buffer[i]._Info._cycle * 60)
                {
                    _XD300Buffer[i]._Command[0]._senddt = DateTime.Now;
                    _XD300Buffer[i]._Command[0]._onoff = true;
                }
            }
        }
        #endregion

    }
}
