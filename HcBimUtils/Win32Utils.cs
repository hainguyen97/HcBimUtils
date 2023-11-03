using System.Diagnostics ;
using System.Runtime.InteropServices ;
using System.Text ;
using HWND = System.IntPtr;

namespace HcBimUtils
{
   public struct RECT
   {
      public int Left;

      public int Top;

      public int Right;

      public int Bottom;
   }

   public class Win32Utils
   {
      private const int GW_HWNDNEXT = 2;
      private const int GW_HWNDPREV = 3;

      [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
      private static extern bool SetDefaultPrinter(string Name);

      [DllImport("user32.dll", SetLastError = true)]
      private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

      /// <summary>
      /// Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.
      /// </summary>
      [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
      private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

      private const UInt32 WM_CLOSE = 0x0010;

      [DllImport("user32.dll")]
      private static extern IntPtr GetForegroundWindow();

      [DllImport("user32.dll", SetLastError = true)]
      public static extern bool SetForegroundWindow(IntPtr hWnd);

      [DllImport("user32.dll")]
      private static extern IntPtr GetTopWindow(IntPtr hWnd);

      [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindow", SetLastError = true)]
      private static extern IntPtr GetNextWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.U4)] int wFlag);

      [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
      public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

      [DllImport("user32.dll", SetLastError = true)]
      public static extern bool IsWindowVisible(IntPtr hWnd);

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

      [DllImport("user32.DLL")]
      private static extern int GetWindowTextLength(HWND hWnd);

      [DllImport("user32.dll")]
      public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

      [DllImport("user32.dll", SetLastError = true)]
      public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

      [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
      public static extern IntPtr GetParent(IntPtr hWnd);

      public delegate bool EnumWindowsProc(HWND hWnd, int lParam);

      [DllImport("user32.DLL")]
      public static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

      [DllImport("user32.dll", SetLastError = true)]
      private static extern bool BringWindowToTop(IntPtr hWnd);

      [DllImport("user32.dll", SetLastError = true)]
      private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

      [DllImport("user32.dll", SetLastError = true)]
      private static extern bool AllowSetForegroundWindow(int dwProcessId);

      private const int SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000;
      private const int SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;
      private const int SPIF_SENDCHANGE = 2;

      [DllImport("user32.dll", SetLastError = true)]
      private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

      [DllImport("user32.dll", EntryPoint = "ShowWindow")]
      public static extern IntPtr ShowWindow(IntPtr hWnd, int nCmdShow);

      [DllImport("user32.dll", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

      [DllImport("user32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool GetWindowRect(IntPtr intPtr, out RECT rect);

      [DllImport("user32.dll", EntryPoint = "GetClassName")]
      public static extern int GetClassName(IntPtr hWnd, StringBuilder className, int nMaxCount);

      [DllImport("user32.DLL")]
      public static extern IntPtr GetShellWindow();

      private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

      [DllImport("user32")]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

      private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

      [DllImport("user32.dll")]
      private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

      //===================================================================
      //===================================================================
      //===================================================================
      //===================================================================
      //===================================================================
      //===================================================================
      public static void SetWindowDefaultPrinter(string printerName)
      {
         SetDefaultPrinter(printerName);
      }

      public static string FindWindowClass(IntPtr intPtr)
      {
         string value = string.Empty;
         if (intPtr != null)
         {
            StringBuilder stringBuilder = new StringBuilder(100);
            GetClassName(intPtr, stringBuilder, 100);
            value = stringBuilder.ToString();
         }
         return value;
      }

      public static string FindWindowText(IntPtr intPtr)
      {
         string value = string.Empty;
         if (intPtr != null)
         {
            StringBuilder stringBuilder = new StringBuilder(100);
            GetWindowText(intPtr, stringBuilder, 100);
            value = stringBuilder.ToString();
         }
         return value;
      }

      public static void GetWindow(IntPtr intPtr)
      {
         ShowWindow(intPtr, 1);
      }

      public static void GetListWindow(IntPtr intPtrInput, out List<IntPtr> intPtrList, out List<string> stringList, out List<string> classNameList, out List<string> rectList)
      {
         intPtrList = new List<IntPtr>();
         stringList = new List<string>();
         classNameList = new List<string>();
         rectList = new List<string>();
         IntPtr intPtr = FindWindowEx(intPtrInput, IntPtr.Zero, null, null);
         while (intPtr != IntPtr.Zero)
         {
            StringBuilder stringBuilder = new StringBuilder(256);
            GetWindowText(intPtr, stringBuilder, stringBuilder.Capacity);
            string item = stringBuilder.ToString();
            StringBuilder classNameBuffer = new StringBuilder(100);
            int className = GetClassName(intPtr, classNameBuffer, 100);

            RECT rect;
            GetWindowRect(intPtr, out rect);
            rectList.Add(string.Format("{0},{1},{2},{3}", rect.Left, rect.Right, rect.Top, rect.Bottom));

            stringList.Add(item);
            intPtrList.Add(intPtr);
            classNameList.Add(classNameBuffer.ToString());
            intPtr = FindWindowEx(intPtrInput, intPtr, null, null);
         }
      }

      public static void SetWindow(IntPtr intPtr, int x, int y, int cx, int cy)
      {
         SetWindowPos(intPtr, IntPtr.Zero, x, y, cx, cy, 4);
      }

      public void SoftAltTab(IntPtr hwnd)
      {
         var list = Windows();
         RemoveBeforeForeground(list);

         int i = 0;
         foreach (IntPtr _hwnd in list)
         {
            if (IsWindowVisible(_hwnd))
            {
               string title = GetText(_hwnd);
               Process proc = GetProcess(_hwnd);
               if ((Process.GetCurrentProcess().Id == proc.Id) ||
                     (title == "Program Manager" && proc.ProcessName == "explorer") ||
                     (title == "" && proc.ProcessName == "explorer"))
               {
                  Console.Write("black list: ");
                  dump(_hwnd);
                  continue;
               }
               i++;
               Console.Write("index={0}, ", i);
               dump(_hwnd);
               if (i == 2)
               {
                  SetForeground(hwnd, _hwnd);
                  break;
               }
            }
            else
            {
               Console.Write("invisible: ");
               dump(_hwnd);
            }
         }
      }

      private void RemoveBeforeForeground(List<IntPtr> list)
      {
         IntPtr fore = GetForegroundWindow();
         int pos = list.IndexOf(fore);
         if (pos > -1)
         {
            list.RemoveRange(0, pos);
         }
      }

      private List<IntPtr> Windows()
      {
         List<IntPtr> list = new List<IntPtr>();
         IntPtr hwnd = GetTopWindow((IntPtr)null);
         while (hwnd != IntPtr.Zero)
         {
            list.Add(hwnd);
            hwnd = GetNextWindow(hwnd, GW_HWNDNEXT);
         }
         return list;
      }

      [DllImport("user32.dll")]
      private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

      private const int SWP_NOMOVE = 0x0002;
      private const int SWP_NOSIZE = 0x0001;
      private const int HWND_TOP = 0;

      private void SetForeground(IntPtr hwnd, IntPtr _hwnd)
      {
         IntPtr spTime = IntPtr.Zero;
         uint foreThread = GetThreadId(GetForegroundWindow());
         uint thisThread = GetThreadId(hwnd);
         bool ret = true;

         if (foreThread != thisThread)
         {
            ret = AttachThreadInput(thisThread, foreThread, true);
            Console.WriteLine("Attach Foreground Thread->{0}", ret);
            if (ret == false) DumpWin32ErrorCode();

            ret = SystemParametersInfo(SPI_GETFOREGROUNDLOCKTIMEOUT, 0, spTime, 0);
            Console.WriteLine("SystemParametersInfo->{0}", ret);
            if (ret == false) DumpWin32ErrorCode();

            ret = SystemParametersInfo(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, IntPtr.Zero, SPIF_SENDCHANGE);
            Console.WriteLine("SystemParametersInfo->{0}", ret);
            if (ret == false) DumpWin32ErrorCode();

            ret = AllowSetForegroundWindow(GetProcessId(_hwnd));
            Console.WriteLine("AllowSetForegroundWindow->{0}", ret);
            if (ret == false) DumpWin32ErrorCode();
         }

         ret = BringWindowToTop(_hwnd);
         Console.WriteLine("BringWindowToTop->{0}", ret);
         if (ret == false) DumpWin32ErrorCode();

         ret = SetWindowPos(_hwnd, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
         Console.WriteLine("SetWindowPos->{0}", ret);
         if (ret == false) DumpWin32ErrorCode();

         ret = SetForegroundWindow(_hwnd);
         Console.WriteLine("SetForegroundWindow->{0}", ret);
         if (ret == false) DumpWin32ErrorCode();

         if (foreThread != thisThread)
         {
            ret = SystemParametersInfo(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, spTime, SPIF_SENDCHANGE);
            Console.WriteLine("SystemParametersInfo->{0}", ret);
            if (ret == false) DumpWin32ErrorCode();

            ret = AttachThreadInput(thisThread, foreThread, false);
            Console.WriteLine("Dettach Foreground Thread->{0}", ret);
            if (ret == false) DumpWin32ErrorCode();
         }
      }

      private void DumpWin32ErrorCode()
      {
         Console.WriteLine("ErrorCode: {0}", Marshal.GetLastWin32Error());
      }

      private string GetText(IntPtr hwnd)
      {
         StringBuilder sb = new StringBuilder(0x1024);
         GetWindowText(hwnd, sb, sb.Capacity);
         return sb.ToString();
      }

      private uint GetThreadId(IntPtr hwnd)
      {
         int pid;
         return GetWindowThreadProcessId(hwnd, out pid);
      }

      private int GetProcessId(IntPtr hwnd)
      {
         int pid;
         GetWindowThreadProcessId(hwnd, out pid);
         return pid;
      }

      private Process GetProcess(IntPtr hwnd)
      {
         return Process.GetProcessById(GetProcessId(hwnd));
      }

      private void dump(IntPtr hwnd)
      {
         Process p = GetProcess(hwnd);
         Console.WriteLine("hwnd={0}, title={1}, id={2}, name={3}", hwnd, GetText(hwnd), p.Id, p.ProcessName);
      }

      public static bool GetCurrentViewRect(out IntPtr ptrOut, out int width, out int height)
      {
         ptrOut = IntPtr.Zero;
         width = (height = 0);
         IntPtr intPtr = Process.GetCurrentProcess().MainWindowHandle;
         if (intPtr == IntPtr.Zero)
         {
            return false;
         }
         ptrOut = FindWindowEx(intPtr, IntPtr.Zero, "MDIClient", "");
         RECT rect;
         if (!GetWindowRect(ptrOut, out rect))
         {
            return false;
         }
         width = rect.Right - rect.Left;
         height = rect.Bottom - rect.Top;
         return true;
      }

      public static bool TileWindows()
      {
         IntPtr intPtr;
         int width;
         int height;
         if (!GetCurrentViewRect(out intPtr, out width, out height))
         {
            return false;
         }
         List<IntPtr> list;
         List<string> list2;
         List<string> classnamelist;
         List<string> rectList;
         GetListWindow(intPtr, out list, out list2, out classnamelist, out rectList);
         if (list.Count < 2)
         {
            return false;
         }
         GetWindow(list[0]);
         GetWindow(list[1]);
         SetWindow(list[1], 0, 0, width / 2, height);
         SetWindow(list[0], width / 2, 0, width / 2, height);

         return true;
      }

      public static IntPtr GetMainWindow(int pid)
      {
         HWND shellWindow = GetShellWindow();
         List<HWND> windowsForPid = new List<IntPtr>();

         try
         {
            EnumWindows(
            // EnumWindowsProc Function, does the work
            // on each window.
            delegate (HWND hWnd, int lParam)
            {
               if (hWnd == shellWindow) return true;
               if (!IsWindowVisible(hWnd)) return true;

               uint windowPid = 0;
               GetWindowThreadProcessId(hWnd, out windowPid);

               // if window is from Pid of interest,
               // see if it's the Main Window
               if (windowPid == pid)
               {
                  // By default Main Window has a
                  // Parent Window of Zero, no parent.
                  HWND parentHwnd = GetParent(hWnd);
                  if (parentHwnd == IntPtr.Zero)
                     windowsForPid.Add(hWnd);
               }

               return true;
            }
            // lParam, nothing, null...
            , 0);
         }
         catch (Exception)
         { }

         return DetermineMainWindow(windowsForPid);
      }

      public static IntPtr DetermineMainWindow(List<HWND> handles)
      {
         // Safty conditions, bail if not met.
         if (handles == null || handles.Count <= 0)
            return IntPtr.Zero;

         // default Null handel
         HWND mainWindow = IntPtr.Zero;

         // only one window so return it,
         // must be the Main Window??
         if (handles.Count == 1)
         {
            mainWindow = handles[0];
         }
         // more than one window
         else
         {
            // more than one candidate for Main Window
            // so find the Main Window by its Title, it
            // will contain "Autodesk Revit"
            foreach (var hWnd in handles)
            {
               int length = GetWindowTextLength(hWnd);
               if (length == 0) continue;

               StringBuilder builder = new StringBuilder(
                  length);

               GetWindowText(hWnd, builder, length + 1);

               // Depending on the Title of the Main Window
               // to have "Autodesk Revit" in it.
               if (builder.ToString().ToLower().Contains(
                  "autodesk revit"))
               {
                  mainWindow = hWnd;
                  break; // found Main Window stop and return it.
               }
            }
         }
         return mainWindow;
      }

      //public static List<RevitWindow> GetRevitWindow(int processId)
      //{
      //    var handles = new List<RevitWindow>();

      //    foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
      //        EnumThreadWindows(
      //            thread.Id,
      //            (hWnd, lParam) =>
      //            {
      //                handles.Add(new RevitWindow(hWnd));
      //                return true;
      //            },
      //            IntPtr.Zero);
      //    return handles;
      //}
      public static void CloseWindow(IntPtr intPtr)
      {
         try
         {
            SendMessage(intPtr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
         }
         catch
         {
         }
      }
   }
}