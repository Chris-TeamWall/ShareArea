using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;

namespace ShareArea.WindowsUtils
{
  /// <summary>
  /// UAC Utils.
  /// </summary>
  public static class UACUtils
  {
    #region Structures

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
      public UInt32 LowPart;
      public Int32 HighPart;
    }

    public struct TOKEN_PRIVILEGES
    {
      public UInt32 PrivilegeCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
      public LUID_AND_ATTRIBUTES[] Privileges;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID_AND_ATTRIBUTES
    {
      public LUID Luid;
      public UInt32 Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
      public int Length;
      public IntPtr lpSecurityDescriptor;
      public bool bInheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STARTUPINFO
    {
      public int cb;
      public String lpReserved;
      public String lpDesktop;
      public String lpTitle;
      public uint dwX;
      public uint dwY;
      public uint dwXSize;
      public uint dwYSize;
      public uint dwXCountChars;
      public uint dwYCountChars;
      public uint dwFillAttribute;
      public uint dwFlags;
      public short wShowWindow;
      public short cbReserved2;
      public IntPtr lpReserved2;
      public IntPtr hStdInput;
      public IntPtr hStdOutput;
      public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION
    {
      public IntPtr hProcess;
      public IntPtr hThread;
      public uint dwProcessId;
      public uint dwThreadId;
    }

    #endregion

    #region Enumerations

    public enum RegistrySecurity
    {
      KEY_ALL_ACCESS = 0xF003F,
      KEY_CREATE_LINK = 0x0020,
      KEY_CREATE_SUB_KEY = 0x0004,
      KEY_ENUMERATE_SUB_KEYS = 0x0008,
      KEY_EXECUTE = 0x20019,
      KEY_NOTIFY = 0x0010,
      KEY_QUERY_VALUE = 0x0001,
      KEY_READ = 0x20019,
      KEY_SET_VALUE = 0x0002,
      KEY_WOW64_32KEY = 0x0200,
      KEY_WOW64_64KEY = 0x0100,
      KEY_WRITE = 0x20006,
    }


    enum TOKEN_TYPE : int
    {
      TokenPrimary = 1,
      TokenImpersonation = 2
    }

    enum SECURITY_IMPERSONATION_LEVEL : int
    {
      SecurityAnonymous = 0,
      SecurityIdentification = 1,
      SecurityImpersonation = 2,
      SecurityDelegation = 3,
    }

    #endregion

    #region Constants

    public const UInt32 SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
    public const UInt32 SE_PRIVILEGE_ENABLED = 0x00000002;
    public const UInt32 SE_PRIVILEGE_REMOVED = 0x00000004;
    public const UInt32 SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;

    public const int SERVICE_ALL_ACCESS = 0xF01FF;
    public const int SERVICE_QUERY_CONFIG = 0x0001;
    public const int SERVICE_CONFIG_FAILURE_ACTIONS = 0x2;
    public const int ERROR_ACCESS_DENIED = 5;
    public const int SERVICE_CONFIG_FAILURE_ACTIONS_FLAG = 0x4;

    public const string SE_ASSIGNPRIMARYTOKEN_NAME = "SeAssignPrimaryTokenPrivilege";

    public const string SE_AUDIT_NAME = "SeAuditPrivilege";

    public const string SE_BACKUP_NAME = "SeBackupPrivilege";

    public const string SE_CHANGE_NOTIFY_NAME = "SeChangeNotifyPrivilege";

    public const string SE_CREATE_GLOBAL_NAME = "SeCreateGlobalPrivilege";

    public const string SE_CREATE_PAGEFILE_NAME = "SeCreatePagefilePrivilege";

    public const string SE_CREATE_PERMANENT_NAME = "SeCreatePermanentPrivilege";

    public const string SE_CREATE_SYMBOLIC_LINK_NAME = "SeCreateSymbolicLinkPrivilege";

    public const string SE_CREATE_TOKEN_NAME = "SeCreateTokenPrivilege";

    public const string SE_DEBUG_NAME = "SeDebugPrivilege";

    public const string SE_ENABLE_DELEGATION_NAME = "SeEnableDelegationPrivilege";

    public const string SE_IMPERSONATE_NAME = "SeImpersonatePrivilege";

    public const string SE_INC_BASE_PRIORITY_NAME = "SeIncreaseBasePriorityPrivilege";

    public const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";

    public const string SE_INC_WORKING_SET_NAME = "SeIncreaseWorkingSetPrivilege";

    public const string SE_LOAD_DRIVER_NAME = "SeLoadDriverPrivilege";

    public const string SE_LOCK_MEMORY_NAME = "SeLockMemoryPrivilege";

    public const string SE_MACHINE_ACCOUNT_NAME = "SeMachineAccountPrivilege";

    public const string SE_MANAGE_VOLUME_NAME = "SeManageVolumePrivilege";

    public const string SE_PROF_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";

    public const string SE_RELABEL_NAME = "SeRelabelPrivilege";

    public const string SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";

    public const string SE_RESTORE_NAME = "SeRestorePrivilege";

    public const string SE_SECURITY_NAME = "SeSecurityPrivilege";

    public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

    public const string SE_SYNC_AGENT_NAME = "SeSyncAgentPrivilege";

    public const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";

    public const string SE_SYSTEM_PROFILE_NAME = "SeSystemProfilePrivilege";

    public const string SE_SYSTEMTIME_NAME = "SeSystemtimePrivilege";

    public const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";

    public const string SE_TCB_NAME = "SeTcbPrivilege";

    public const string SE_TIME_ZONE_NAME = "SeTimeZonePrivilege";

    public const string SE_TRUSTED_CREDMAN_ACCESS_NAME = "SeTrustedCredManAccessPrivilege";

    public const string SE_UNDOCK_NAME = "SeUndockPrivilege";

    public const string SE_UNSOLICITED_INPUT_NAME = "SeUnsolicitedInputPrivilege";

    public const uint MAXIMUM_ALLOWED = 0x2000000;
    public const int CREATE_NEW_CONSOLE = 0x00000010;

    public const int IDLE_PRIORITY_CLASS = 0x40;
    public const int NORMAL_PRIORITY_CLASS = 0x20;
    public const int HIGH_PRIORITY_CLASS = 0x80;
    public const int REALTIME_PRIORITY_CLASS = 0x100;

    private static uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
    private static uint STANDARD_RIGHTS_READ = 0x00020000;
    private static uint TOKEN_ASSIGN_PRIMARY = 0x0001;
    private static uint TOKEN_DUPLICATE = 0x0002;
    private static uint TOKEN_IMPERSONATE = 0x0004;
    public static uint TOKEN_QUERY = 0x0008;
    private static uint TOKEN_QUERY_SOURCE = 0x0010;
    public static uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
    private static uint TOKEN_ADJUST_GROUPS = 0x0040;
    private static uint TOKEN_ADJUST_DEFAULT = 0x0080;
    private static uint TOKEN_ADJUST_SESSIONID = 0x0100;
    private static uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
    private static uint TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
        TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
        TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
        TOKEN_ADJUST_SESSIONID);

    #endregion

    #region Win32 API Imports

    [DllImport("user32.dll")]
    public static extern bool AllowSetForegroundWindow(uint dwProcessId);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
       [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
       ref TOKEN_PRIVILEGES NewState,
       UInt32 Zero,
       IntPtr Null1,
       IntPtr Null2);


    [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
    public static extern int RegOpenCurrentUser(int samDesired, out IntPtr phkResult);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hSnapshot);

    [DllImport("kernel32.dll")]
    public static extern uint WTSGetActiveConsoleSessionId();

    [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool WTSQueryUserToken(uint sessionId, out IntPtr tokenHandle);

    [DllImport("wtsapi32.dll")]
    static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] String pServerName);

    [DllImport("wtsapi32.dll")]
    static extern void WTSCloseServer(IntPtr hServer);

    [DllImport("Wtsapi32.dll")]
    public static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass,
        out System.IntPtr ppBuffer, out uint pBytesReturned);

    [DllImport("wtsapi32.dll")]
    static extern Int32 WTSEnumerateSessions(IntPtr hServer, [MarshalAs(UnmanagedType.U4)] Int32 Reserved,
        [MarshalAs(UnmanagedType.U4)] Int32 Version, ref IntPtr ppSessionInfo, [MarshalAs(UnmanagedType.U4)] ref Int32 pCount);

    [DllImport("wtsapi32.dll")]
    static extern void WTSFreeMemory(IntPtr pMemory);

    [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public extern static bool CreateProcessAsUser(IntPtr hToken, String lpApplicationName, String lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
        ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandle, int dwCreationFlags, IntPtr lpEnvironment,
        String lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

    [DllImport("kernel32.dll")]
    static extern bool ProcessIdToSessionId(uint dwProcessId, ref uint pSessionId);

    [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
    public extern static bool DuplicateTokenEx(IntPtr ExistingTokenHandle, uint dwDesiredAccess,
        ref SECURITY_ATTRIBUTES lpThreadAttributes, int TokenType,
        int ImpersonationLevel, ref IntPtr DuplicateTokenHandle);

    [DllImport("kernel32.dll")]
    static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport("advapi32", SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]
    public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, ref IntPtr TokenHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetCurrentProcess();

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName,
        out LUID lpLuid);

    #endregion

    /// <summary>
    /// Launches the given application with full admin rights, and in addition bypasses the Vista UAC prompt
    /// </summary>
    /// <param name="applicationName">The name of the application to launch</param>
    /// <param name="procInfo">Process information regarding the launched application that gets returned to the caller</param>
    /// <returns></returns>
    public static void StartProcessAndBypassUAC(int sessionId, String applicationName, out PROCESS_INFORMATION procInfo)
    {
      uint winlogonPid = 0;
      IntPtr hUserTokenDup = IntPtr.Zero, hPToken = IntPtr.Zero, hProcess = IntPtr.Zero;
      procInfo = new PROCESS_INFORMATION();

      // obtain the currently active session id; every logged on user in the system has a unique session id
      uint dwSessionId = (uint)sessionId;// WTSGetActiveConsoleSessionId();

      // obtain the process id of the winlogon process that is running within the currently active session
      Process[] processes = Process.GetProcessesByName("winlogon");
      foreach (Process p in processes)
      {
        if ((uint)p.SessionId == dwSessionId)
        {
          winlogonPid = (uint)p.Id;
        }
      }

      // obtain a handle to the winlogon process
      hProcess = OpenProcess(MAXIMUM_ALLOWED, false, winlogonPid);

      // obtain a handle to the access token of the winlogon process
      if (!OpenProcessToken(hProcess, TOKEN_DUPLICATE, ref hPToken))
      {
        int dummy_i = Marshal.GetLastWin32Error();
        CloseHandle(hProcess);
        throw new InvalidOperationException($"Could not open process token. Reason: {dummy_i}");
      }

      // Security attibute structure used in DuplicateTokenEx and CreateProcessAsUser
      // I would prefer to not have to use a security attribute variable and to just 
      // simply pass null and inherit (by default) the security attributes
      // of the existing token. However, in C# structures are value types and therefore
      // cannot be assigned the null value.
      SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
      sa.Length = Marshal.SizeOf(sa);
      
      // copy the access token of the winlogon process; the newly created token will be a primary token
      if (!DuplicateTokenEx(hPToken, MAXIMUM_ALLOWED, ref sa, (int)SECURITY_IMPERSONATION_LEVEL.SecurityIdentification, (int)TOKEN_TYPE.TokenPrimary, ref hUserTokenDup))
      {
        int dummy_i = Marshal.GetLastWin32Error();
        CloseHandle(hProcess);
        CloseHandle(hPToken);
        throw new InvalidOperationException($"Could not duplicate token. Reason: {dummy_i}");
      }

      // By default CreateProcessAsUser creates a process on a non-interactive window station, meaning
      // the window station has a desktop that is invisible and the process is incapable of receiving
      // user input. To remedy this we set the lpDesktop parameter to indicate we want to enable user 
      // interaction with the new process.
      STARTUPINFO si = new STARTUPINFO();
      si.cb = (int)Marshal.SizeOf(si);
      //si.lpDesktop = @"winsta0\winlogon"; // interactive window station parameter; basically this indicates that the process created can display a GUI on the desktop
      si.lpDesktop = @"winsta0\default"; // interactive window station parameter; basically this indicates that the process created can display a GUI on the desktop

      // flags that specify the priority and creation method of the process
      int dwCreationFlags = NORMAL_PRIORITY_CLASS | CREATE_NEW_CONSOLE;
      // create a new process in the current user's logon session
      bool result = CreateProcessAsUser(hUserTokenDup,        // client's access token
                                      null,                   // file to execute
                                      applicationName,        // command line
                                      ref sa,                 // pointer to process SECURITY_ATTRIBUTES
                                      ref sa,                 // pointer to thread SECURITY_ATTRIBUTES
                                      false,                  // handles are not inheritable
                                      dwCreationFlags,        // creation flags
                                      IntPtr.Zero,            // pointer to new environment block 
                                      Path.GetDirectoryName(applicationName),// name of current directory 
                                      ref si,                 // pointer to STARTUPINFO structure
                                      out procInfo            // receives information about new process
                                      );
      int? lastError = null;
      
      if (result == false)
      {
        lastError = Marshal.GetLastWin32Error();
      }
      // invalidate the handles
      CloseHandle(hProcess);
      CloseHandle(hPToken);
      CloseHandle(hUserTokenDup);
      if (lastError.HasValue)
      {
        throw new InvalidOperationException($"Could not start process. Reason {lastError.Value}");
      }
    }

    public static void AdjustProcess(uint dwProcessId)
    {
      AdjustProcess(dwProcessId, "SeChangeNotifyPrivilege");
      AdjustProcess(dwProcessId, "SeSecurityPrivilege");
      AdjustProcess(dwProcessId, "SeBackupPrivilege");
      AdjustProcess(dwProcessId, "SeRestorePrivilege");
      AdjustProcess(dwProcessId, "SeSystemtimePrivilege");
      AdjustProcess(dwProcessId, "SeShutdownPrivilege");
      AdjustProcess(dwProcessId, "SeRemoteShutdownPrivilege");
      AdjustProcess(dwProcessId, "SeTakeOwnershipPrivilege");
      AdjustProcess(dwProcessId, "SeDebugPrivilege");
      AdjustProcess(dwProcessId, "SeSystemEnvironmentPrivilege");
      AdjustProcess(dwProcessId, "SeSystemProfilePrivilege");
      AdjustProcess(dwProcessId, "SeProfileSingleProcessPrivilege");
      AdjustProcess(dwProcessId, "SeIncreaseBasePriorityPrivilege");
      AdjustProcess(dwProcessId, "SeLoadDriverPrivilege");
      AdjustProcess(dwProcessId, "SeCreatePagefilePrivilege");
      AdjustProcess(dwProcessId, "SeIncreaseQuotaPrivilege");
      AdjustProcess(dwProcessId, "SeUndockPrivilege");
      AdjustProcess(dwProcessId, "SeManageVolumePrivilege");
      AdjustProcess(dwProcessId, "SeAssignPrimaryTokenPrivilege");
      AdjustProcess(dwProcessId, "SeAuditPrivilege");
      AdjustProcess(dwProcessId, "SeCreateGlobalPrivilege");
      AdjustProcess(dwProcessId, "SeCreatePermanentPrivilege");
      AdjustProcess(dwProcessId, "SeCreateSymbolicLinkPrivilege");
      AdjustProcess(dwProcessId, "SeCreateTokenPrivilege");
      AdjustProcess(dwProcessId, "SeEnableDelegationPrivilege");
      AdjustProcess(dwProcessId, "SeImpersonatePrivilege");
      AdjustProcess(dwProcessId, "SeIncreaseWorkingAdjustProcess");
      AdjustProcess(dwProcessId, "SeLockMemoryPrivilege");
      AdjustProcess(dwProcessId, "SeMachineAccountPrivilege");
      AdjustProcess(dwProcessId, "SeRelabelPrivilege");
      AdjustProcess(dwProcessId, "SeSecurityPrivilege");
      AdjustProcess(dwProcessId, "SeSyncAgentPrivilege");
      AdjustProcess(dwProcessId, "SeTcbPrivilege");
      AdjustProcess(dwProcessId, "SeTimeZonePrivilege");
      AdjustProcess(dwProcessId, "SeTrustedCredManAccessPrivilege");
      AdjustProcess(dwProcessId, "SeUnsolicitedInputPrivilege");
    }

    private static void AdjustProcess(uint dwProcessId, string privileg)
    {
      var token_PRIVILEGES = new TOKEN_PRIVILEGES();
      LUID luid;
      var hProcess = OpenProcess(MAXIMUM_ALLOWED, false, dwProcessId);
      IntPtr hRefToken = IntPtr.Zero;
      bool i2 = OpenProcessToken(hProcess, 40, ref hRefToken);
      if (i2 == false)
      {
        CloseHandle(hProcess);
        return;
      }
      i2 = LookupPrivilegeValue(null, privileg, out luid);
      if (i2 == false)
      {
        CloseHandle(hRefToken);
        CloseHandle(hProcess);
        return;
      }
      token_PRIVILEGES.PrivilegeCount = 1;
      token_PRIVILEGES.Privileges = new LUID_AND_ATTRIBUTES[1];
      token_PRIVILEGES.Privileges[0] = new LUID_AND_ATTRIBUTES();
      token_PRIVILEGES.Privileges[0] = new LUID_AND_ATTRIBUTES() { Attributes = 2, Luid = luid };
      var resx = AdjustTokenPrivileges(hRefToken, false, ref token_PRIVILEGES, 0, IntPtr.Zero, IntPtr.Zero);
      CloseHandle(hRefToken);
      CloseHandle(hProcess);
    }

    private static IntPtr OpenServer(string Name)
    {
      IntPtr server = WTSOpenServer(Name);
      return server;
    }

    private static void CloseServer(IntPtr ServerHandle)
    {
      WTSCloseServer(ServerHandle);
    }

    public static string GetUserOfActiveConsoleSession()
    {
      var activeConsoleSession = WTSGetActiveConsoleSessionId();
      if (activeConsoleSession == 0xffffffff)
      {
        return string.Empty;
      }
      return GetUserNameFromSession(null, activeConsoleSession);
    }

    public static string GetUserNameFromSession(string serverName, uint sessionId)
    {
      IntPtr server = IntPtr.Zero;
      if (string.IsNullOrEmpty(serverName) == false)
      {
        server = OpenServer(serverName);
      }
      System.IntPtr buffer = IntPtr.Zero;
      uint bytesReturned;
      try
      {
        bool worked = WTSQuerySessionInformation(server, (int)sessionId,
            WTS_INFO_CLASS.UserName, out buffer, out bytesReturned);
        var strData = Marshal.PtrToStringAnsi(buffer);
        return strData;
      }
      finally
      {
        if (buffer != IntPtr.Zero)
        {
          WTSFreeMemory(buffer);
          buffer = IntPtr.Zero;
        }
        if (server != IntPtr.Zero)
        {
          CloseServer(server);
        }
      }
    }

    public static List<TerminalSessionData> ListSessions(string ServerName)
    {
      IntPtr server = IntPtr.Zero;
      List<TerminalSessionData> ret = new List<TerminalSessionData>();
      server = OpenServer(ServerName);

      try
      {
        IntPtr ppSessionInfo = IntPtr.Zero;

        Int32 count = 0;
        Int32 retval = WTSEnumerateSessions(server, 0, 1, ref ppSessionInfo, ref count);
        Int32 dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));

        Int64 current = (int)ppSessionInfo;

        if (retval != 0)
        {
          for (int i = 0; i < count; i++)
          {
            WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure((System.IntPtr)current, typeof(WTS_SESSION_INFO));
            current += dataSize;

            ret.Add(new TerminalSessionData(si.SessionID, si.State, si.pWinStationName));
          }

          WTSFreeMemory(ppSessionInfo);
        }
      }
      finally
      {
        CloseServer(server);
      }

      return ret;
    }

    public static TerminalSessionInfo GetSessionInfo(string ServerName, int SessionId)
    {
      IntPtr server = IntPtr.Zero;
      if (string.IsNullOrEmpty(ServerName) == false)
      {
        server = OpenServer(ServerName);
      }
      System.IntPtr buffer = IntPtr.Zero;
      uint bytesReturned;
      TerminalSessionInfo data = new TerminalSessionInfo();

      try
      {
        bool worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.ApplicationName, out buffer, out bytesReturned);

        if (!worked)
          return data;

        string strData = Marshal.PtrToStringAnsi(buffer);
        data.ApplicationName = strData;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.ClientAddress, out buffer, out bytesReturned);

        if (!worked)
          return data;

        WTS_CLIENT_ADDRESS si = (WTS_CLIENT_ADDRESS)Marshal.PtrToStructure((System.IntPtr)buffer, typeof(WTS_CLIENT_ADDRESS));
        data.ClientAddress = si;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.ClientBuildNumber, out buffer, out bytesReturned);

        if (!worked)
          return data;

        int lData = Marshal.ReadInt32(buffer);
        data.ClientBuildNumber = lData;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.ClientDirectory, out buffer, out bytesReturned);

        if (!worked)
          return data;

        strData = Marshal.PtrToStringAnsi(buffer);
        data.ClientDirectory = strData;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.ClientDisplay, out buffer, out bytesReturned);

        if (!worked)
          return data;

        WTS_CLIENT_DISPLAY cd = (WTS_CLIENT_DISPLAY)Marshal.PtrToStructure((System.IntPtr)buffer, typeof(WTS_CLIENT_DISPLAY));
        data.ClientDisplay = cd;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.ClientHardwareId, out buffer, out bytesReturned);

        if (!worked)
          return data;

        lData = Marshal.ReadInt32(buffer);
        data.ClientHardwareId = lData;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.ClientName, out buffer, out bytesReturned);
        strData = Marshal.PtrToStringAnsi(buffer);
        data.ClientName = strData;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.ClientProductId, out buffer, out bytesReturned);
        Int16 intData = Marshal.ReadInt16(buffer);
        data.ClientProductId = intData;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.ClientProtocolType, out buffer, out bytesReturned);
        intData = Marshal.ReadInt16(buffer);
        data.ClientProtocolType = intData;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.ConnectState, out buffer, out bytesReturned);
        lData = Marshal.ReadInt32(buffer);
        data.ConnectState = (WTS_CONNECTSTATE_CLASS)Enum.ToObject(typeof(WTS_CONNECTSTATE_CLASS), lData);

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.DomainName, out buffer, out bytesReturned);
        strData = Marshal.PtrToStringAnsi(buffer);
        data.DomainName = strData;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.InitialProgram, out buffer, out bytesReturned);
        strData = Marshal.PtrToStringAnsi(buffer);
        data.InitialProgram = strData;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.OEMId, out buffer, out bytesReturned);
        strData = Marshal.PtrToStringAnsi(buffer);
        data.OEMId = strData;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.SessionId, out buffer, out bytesReturned);
        lData = Marshal.ReadInt32(buffer);
        data.SessionId = lData;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.UserName, out buffer, out bytesReturned);
        strData = Marshal.PtrToStringAnsi(buffer);
        data.UserName = strData;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.WinStationName, out buffer, out bytesReturned);
        strData = Marshal.PtrToStringAnsi(buffer);
        data.WinStationName = strData;

        worked = WTSQuerySessionInformation(server, SessionId,
            WTS_INFO_CLASS.WorkingDirectory, out buffer, out bytesReturned);
        strData = Marshal.PtrToStringAnsi(buffer);
        data.WorkingDirectory = strData;
      }
      finally
      {
        WTSFreeMemory(buffer);
        buffer = IntPtr.Zero;
        CloseServer(server);
      }

      return data;
    }

    /// <summary>
    /// Führt die Action im Context des aktuell an der Console angemeldeten Benutzers aus
    /// </summary>
    /// <param name="action"></param>
    public static void ExecuteAsConsoleUser(Action action)
    {
      IntPtr currentToken;
      var sessionId = WTSGetActiveConsoleSessionId();
      string userName = GetUserNameFromSession(string.Empty, sessionId);
      if (string.IsNullOrEmpty(userName))
      {
        throw new InvalidOperationException("UAC: There is no user connected to the Windows Console Session!");
      }
      string currentUser = WindowsIdentity.GetCurrent().Name;
      if (currentUser.IndexOf('\\') > 0)
      {
        currentUser = currentUser.Split('\\')[1];
      }
      if (string.Compare(currentUser, userName, StringComparison.OrdinalIgnoreCase) == 0)
      {
        action();
      }
      else
      {
        bool bRet = WTSQueryUserToken(sessionId, out currentToken);
        if (bRet == false)
        {
          var error = Marshal.GetLastWin32Error();
          CloseHandle(currentToken);
          throw new InvalidOperationException($"Could not query user Token. Reason: {error}");
        }
        using (WindowsIdentity newId = new WindowsIdentity(currentToken))
        using (WindowsImpersonationContext impersonatedUser = newId.Impersonate())
        {
          action();
        }
        CloseHandle(currentToken);
      }
    }

    public static void ExecuteAsSessionUser(int sessionId, Action action)
    {
      IntPtr currentToken;
      string userName = GetUserNameFromSession(string.Empty, (uint)sessionId);
      if (string.IsNullOrEmpty(userName))
      {
        throw new InvalidOperationException("UAC: There is no user connected to the given Session!");
      }
      string currentUser = WindowsIdentity.GetCurrent().Name;
      if (currentUser.IndexOf('\\') > 0)
      {
        currentUser = currentUser.Split('\\')[1];
      }
      if (string.Compare(currentUser, userName, StringComparison.OrdinalIgnoreCase) == 0)
      {
        action();
      }
      else
      {
        bool bRet = WTSQueryUserToken((uint)sessionId, out currentToken);
        if (bRet == false)
        {
          var error = Marshal.GetLastWin32Error();
          CloseHandle(currentToken);
          throw new InvalidOperationException($"Could not query user Token. Reason: {error}");
        }
        using (WindowsIdentity newId = new WindowsIdentity(currentToken))
        using (WindowsImpersonationContext impersonatedUser = newId.Impersonate())
        {
          action();
        }
        CloseHandle(currentToken);
      }
    }

    private static IntPtr GetImpersonateUserRegistryHandle(RegistrySecurity _access)
    {
      IntPtr safeHandle = new IntPtr();
      int result = RegOpenCurrentUser((int)_access, out safeHandle);

      return safeHandle;
    }

    /// <summary>
    /// Stellt der übergebenen Aktion den RegistryKey HKEY_CURRENT_USER des aktuellen Benutzer Contextes
    /// </summary>
    /// <param name="action"></param>
    public static void ImpersonatedCurrentUser(Action<RegistryKey> action)
    {
      using (SafeRegistryHandle safeHandle = new SafeRegistryHandle(UACUtils.GetImpersonateUserRegistryHandle(UACUtils.RegistrySecurity.KEY_ALL_ACCESS), true))
      {
        using (RegistryKey impersonatedUserHkcu = RegistryKey.FromHandle(safeHandle, RegistryView.Default))
        {
          action(impersonatedUserHkcu);
        }
      }

    }


    // Win32 function to open the service control manager.
    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern SafeServiceHandle OpenSCManager(
        string lpMachineName,
        string lpDatabaseName,
        int dwDesiredAccess);

    // Win32 function to open a service instance.
    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern SafeServiceHandle OpenService(
        SafeServiceHandle hSCManager,
        string lpServiceName,
        int dwDesiredAccess);

    // Win32 function to close a service related handle.
    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseServiceHandle(IntPtr hSCObject);

    // Win32 function to change the service config for the failure actions.
    // If the service controller handles the SC_ACTION_REBOOT action, 
    // the caller must have the SE_SHUTDOWN_NAME privilege.
    [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2",
        CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ChangeServiceFailureActions(
        SafeServiceHandle hService,
        int dwInfoLevel,
        [MarshalAs(UnmanagedType.Struct)]
            ref SERVICE_FAILURE_ACTIONS lpInfo);

    // This setting is ignored unless the service has configured failure actions.
    [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2",
        CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FailureActionsOnNonCrashFailures(
        SafeServiceHandle hService,
        int dwInfoLevel,
        [MarshalAs(UnmanagedType.Struct)]
            ref SERVICE_FAILURE_ACTIONS_FLAG lpInfo);

  }


  public class TerminalSessionData
  {
    public int SessionId;
    public WTS_CONNECTSTATE_CLASS ConnectionState;
    public string StationName;

    public TerminalSessionData(int sessionId, WTS_CONNECTSTATE_CLASS connState, string stationName)
    {
      SessionId = sessionId;
      ConnectionState = connState;
      StationName = stationName;
    }

    public override string ToString()
    {
      return String.Format("{0} {1} {2}", SessionId, ConnectionState, StationName);
    }
  }

  public class TerminalSessionInfo
  {
    public string InitialProgram;
    public string ApplicationName;
    public string WorkingDirectory;
    public string OEMId;
    public int SessionId;
    public string UserName;
    public string WinStationName;
    public string DomainName;
    public WTS_CONNECTSTATE_CLASS ConnectState;
    public int ClientBuildNumber;
    public string ClientName;
    public string ClientDirectory;
    public int ClientProductId;
    public int ClientHardwareId;
    public WTS_CLIENT_ADDRESS ClientAddress;
    public WTS_CLIENT_DISPLAY ClientDisplay;
    public int ClientProtocolType;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct WTS_SESSION_INFO
  {
    public Int32 SessionID;
    [MarshalAs(UnmanagedType.LPStr)]
    public String pWinStationName;
    public WTS_CONNECTSTATE_CLASS State;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct WTS_CLIENT_ADDRESS
  {
    public uint AddressFamily;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] Address;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct WTS_CLIENT_DISPLAY
  {
    public uint HorizontalResolution;
    public uint VerticalResolution;
    public uint ColorDepth;
  }

  public enum WTS_CONNECTSTATE_CLASS
  {
    Active,
    Connected,
    ConnectQuery,
    Shadow,
    Disconnected,
    Idle,
    Listen,
    Reset,
    Down,
    Init
  }

  public enum WTS_INFO_CLASS
  {
    InitialProgram = 0,
    ApplicationName = 1,
    WorkingDirectory = 2,
    OEMId = 3,
    SessionId = 4,
    UserName = 5,
    WinStationName = 6,
    DomainName = 7,
    ConnectState = 8,
    ClientBuildNumber = 9,
    ClientName = 10,
    ClientDirectory = 11,
    ClientProductId = 12,
    ClientHardwareId = 13,
    ClientAddress = 14,
    ClientDisplay = 15,
    ClientProtocolType = 16
  }

  // Enumeration for SC_ACTION
  // The SC_ACTION_TYPE enumeration specifies the actions that the SCM can perform.
  internal enum SC_ACTION_TYPE
  {
    None = 0,
    RestartService = 1,
    RebootComputer = 2,
    Run_Command = 3
  }

  // Struct for SERVICE_FAILURE_ACTIONS
  // Represents an action that the service control manager can perform.
  [StructLayout(LayoutKind.Sequential)]
  public struct SC_ACTION
  {
    public int Type;
    public int Delay;
  }

  // Struct for ChangeServiceFailureActions
  // Represents the action the service controller should take on each failure of a 
  // service.
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  public struct SERVICE_FAILURE_ACTIONS
  {
    public int dwResetPeriod;
    public string lpRebootMsg;
    public string lpCommand;
    public int cActions;
    // A pointer to an array of SC_ACTION structures
    public IntPtr lpsaActions;
  }

  // Struct for FailureActionsOnNonCrashFailures
  // Contains the failure actions flag setting of a service.
  [StructLayout(LayoutKind.Sequential)]
  public struct SERVICE_FAILURE_ACTIONS_FLAG
  {
    public bool fFailureActionsOnNonCrashFailures;
  }
  /// <summary>
  /// Represents a wrapper class for a service handle.
  /// </summary>
  [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
  [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
  public class SafeServiceHandle : SafeHandleZeroOrMinusOneIsInvalid
  {
    internal SafeServiceHandle()
        : base(true)
    {
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    protected override bool ReleaseHandle()
    {
      return UACUtils.CloseServiceHandle(base.handle);
    }
  }

}
