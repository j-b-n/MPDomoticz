using System;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;


namespace MP_Domoticz
{    
 /// <summary>
 /// Summary description for MusicSort.
 /// </summary>
 public class DeviceSort : IComparer<GUIListItem>
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
 
   public int Compare(GUIListItem item1, GUIListItem item2)
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
     if (item1.IsFolder && item1.Label == "..")
     {
       return -1;
     }
     if (item2.IsFolder && item2.Label == "..")
     {
       return -1;
     }
     if (item1.IsFolder && !item2.IsFolder)
     {
       return -1;
     }
     else if (!item1.IsFolder && item2.IsFolder)
     {
       return 1;
     }

     DomoticzServer.Device device1 = (DomoticzServer.Device)item1.MusicTag;
     DomoticzServer.Device device2 = (DomoticzServer.Device)item2.MusicTag;
 
 
     SortMethod method = currentSortMethod;
     bool bAscending = sortAscending;

     switch (method)
     {
         case SortMethod.Name:
             if (bAscending)
             {                 
                 return MediaPortal.Util.StringLogicalComparer.Compare(item1.Label, item2.Label);
             }
             else
             {
                 return MediaPortal.Util.StringLogicalComparer.Compare(item2.Label, item1.Label);
             }


         case SortMethod.LastSeen:

             DateTime time1 = DateTime.MinValue;
             DateTime time2 = DateTime.MinValue;
             if (device1 != null)
             {
                 time1 = Convert.ToDateTime(device1.LastUpdate);
             }
             if (device2 != null)
             {
                 time2 = Convert.ToDateTime(device2.LastUpdate);
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
  