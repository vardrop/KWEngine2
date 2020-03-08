using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Helper
{
    internal static class HelperGLLoader
    {

        internal static List<LoadPackage> LoadList = new List<LoadPackage>();

        internal static void AddCall(object receiver, Action a)
        {
            
            LoadPackage lp = new LoadPackage();
            lp.Receiver = receiver;
            lp.Action = a;
            lp.ReceiverType = receiver.GetType();
            lock (LoadList)
            {
                LoadList.Add(lp);
            }
        }

    }
}
