using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCountDemo
{
    class packConfig
    {
        /// <summary>
        /// 自动计数器模块的IP地址
        /// </summary>
        public string ip = "";
        /// <summary>
        /// 包装线名
        /// </summary>
        public string PackNO = "";
        /// <summary>
        /// 每袋包装上线个数
        /// </summary>
        public int StandardPCSUpperLimit = 0;
        /// <summary>
        /// 串口号
        /// </summary>
        public string PortName = "";
        /// <summary>
        /// 称重总数量的公差
        /// </summary>
        public float LotSNAmount = 0;
    }
}
