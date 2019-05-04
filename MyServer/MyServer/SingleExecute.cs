using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyServer {

    /// <summary>
    /// 一个要执行的方法，委托
    /// </summary>
    public delegate void ExecuteDelegate();

    /// <summary>
    /// 单线程执行
    /// </summary>
    public class SingleExecute {

        // 单例
        private static object objLock1 = new object();
        private static SingleExecute instance = null;
        public static SingleExecute Instance {
            get {
                lock (objLock1) { // 加锁
                    if (instance == null) {
                        instance = new SingleExecute();
                    }
                    return instance;
                }
            }
        }

        private static object objLock2 = new object();
        /// <summary>
        /// 互斥锁
        /// </summary>
        private Mutex mutex;

        public SingleExecute() {
            mutex = new Mutex();
        }

        /// <summary>
        /// 单线程执行逻辑
        /// </summary>
        /// <param name="executeDelegate"></param>
        public void Exeecute(ExecuteDelegate executeDelegate) {
            lock (objLock2) {
                mutex.WaitOne(); // 一直等待，直到线程执行完
                executeDelegate();
                mutex.ReleaseMutex(); // 释放
            }
        }
    }
}
