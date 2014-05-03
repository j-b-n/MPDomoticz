using System;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;


namespace MP_Domoticz
{    
 /// <summary>
 /// Sort list based on Device information!
 /// </summary>
 public class DeviceSort : IComparer<DomoticzServer.Device>
 {
   private SortMethod currentSortMethod;
   private bool sortAscending = true;
 
   public DeviceSort(SortMethod method, bool ascending)
   {
     currentSortMethod = method;
     sortAscending = ascending;
   }
 
   public enum SortMethod
   {
     Name = 0,
     LastSeen = 1, // LastSeen     
   }
 
   public int Compare(DomoticzServer.Device item1, DomoticzServer.Device item2)
   {
     if (item1 == item2)
     {
       return 0;
     }
     if (item1 == null)
     {
       return -1;
     }
     if (item2 == null)
     {
       return -1;
     }
   
 
     SortMethod method = currentSortMethod;
     bool bAscending = sortAscending;

     switch (method)
     {
         case SortMethod.Name:
             if (bAscending)
             {
                 return MediaPortal.Util.StringLogicalComparer.Compare(item1.Name, item2.Name);
             }
             else
             {
                 return MediaPortal.Util.StringLogicalComparer.Compare(item2.Name, item1.Name);
             }


         case SortMethod.LastSeen:

             DateTime time1 = DateTime.MinValue;
             DateTime time2 = DateTime.MinValue;
             if (item1 != null)
             {
                 time1 = Convert.ToDateTime(item1.LastUpdate);
             }
             if (item2 != null)
             {
                 time2 = Convert.ToDateTime(item2.LastUpdate);
             }

             if (bAscending)
             {
                 return DateTime.Compare(time1, time2);
             }
             else
             {
                 return DateTime.Compare(time2, time1);
             }
     }
     return 0;
   }
 }
}
  