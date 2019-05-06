using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServer {
    /// <summary>
    /// 线程安全的整型
    /// </summary>
    public class ThreadSafeInt {

        private int value;

        public ThreadSafeInt(int value) {
            this.value = value;
        }

        /// <summary>
        /// 增加并获取
        /// </summary>
        /// <returns></returns>
        public int AddGet() {
            lock (this) {
                value++;
                return value;
            }
        }

        /// <summary>
        /// 减少并获取
        /// </summary>
        /// <returns></returns>
        public int ReduceGet() {
            lock (this) {
                value--;
                return value;
            }
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <returns></returns>
        public int Get() {
            lock (this) {
                return value;
            }
        }
    }
}
