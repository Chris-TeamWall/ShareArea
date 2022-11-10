using System.Collections.Generic;
using System.Windows.Forms;

namespace ShareArea.UI
{
  /// <summary>
  /// Snippet from https://mycsharp.de/forum/threads/36155/globale-hotkeys-tastenkombinationen?page=1
  /// </summary>
  public class HotKeyManager : IMessageFilter
  {
    public class HotKeyObject
    {
      public Keys HotKey { get; set; }

      public MODKEY Modifier { get; set; }

      public string HotKeyID { get; set; }

      public short AtomID { get; set; }

      public HotKeyObject(Keys NewHotKey, MODKEY NewModifier, string NewHotKeyID)
      {
        HotKey = NewHotKey;
        Modifier = NewModifier;
        HotKeyID = NewHotKeyID;
      }
    }

    public Form OwnerForm { get; set; }

    private Dictionary<short, HotKeyObject> mHotKeyList = new Dictionary<short, HotKeyObject>();
    private Dictionary<string, short> mHotKeyIDList = new Dictionary<string, short>();

    /// <summary>
    /// Diesem Event wird immer die zugewiesene HotKeyID übergeben wenn eine HotKey Kombination gedrückt wurde.
    /// </summary>
    public event HotKeyPressedEventHandler HotKeyPressed;
    public delegate void HotKeyPressedEventHandler(string HotKeyID);

    public enum MODKEY : int
    {
      MOD_ALT = 1,
      MOD_CONTROL = 2,
      MOD_SHIFT = 4,
      MOD_WIN = 8
    }

    public HotKeyManager(Form ownerForm)
    {
      OwnerForm = ownerForm;
      Application.AddMessageFilter(this);
    }

    public HotKeyManager()
    {
    }

    /// <summary>
    /// Diese Funktion fügt einen Hotkey hinzu und registriert ihn auch sofort
    /// </summary>
    /// <param name="KeyCode">Den KeyCode für die Taste</param>
    /// <param name="Modifiers">Die Zusatztasten wie z.B. Strg oder Alt, diese können auch mit OR kombiniert werden</param>
    /// <param name="HotKeyID">Die ID die der Hotkey bekommen soll um diesen zu identifizieren</param>
    public void AddHotKey(Keys KeyCode, MODKEY Modifiers, string HotKeyID)
    {
      if (mHotKeyIDList.ContainsKey(HotKeyID) == true) return; // TODO: might not be correct. Was : Exit Sub

      short ID = NativeFunctions.GlobalAddAtom(HotKeyID);
      mHotKeyIDList.Add(HotKeyID, ID);
      mHotKeyList.Add(ID, new HotKeyObject(KeyCode, Modifiers, HotKeyID));
      NativeFunctions.RegisterHotKey(OwnerForm.Handle, (int)ID, (int)mHotKeyList[ID].Modifier, (int)mHotKeyList[ID].HotKey);
    }

    /// <summary>
    /// Diese Funktion entfernt einen Hotkey und deregistriert ihn auch sofort
    /// </summary>
    /// <param name="HotKeyID">Gibt die HotkeyID an welche entfernt werden soll</param>
    public void RemoveHotKey(string HotKeyID)
    {
      if (mHotKeyIDList.ContainsKey(HotKeyID) == false) return; // TODO: might not be correct. Was : Exit Sub

      short ID = mHotKeyIDList[HotKeyID];
      mHotKeyIDList.Remove(HotKeyID);
      mHotKeyList.Remove(ID);
      NativeFunctions.UnregisterHotKey(OwnerForm.Handle, (int)ID);
      NativeFunctions.GlobalDeleteAtom(ID);
    }

    /// <summary>
    /// Diese Routine entfernt und Deregistriert alle Hotkeys
    /// </summary>
    public void RemoveAllHotKeys()
    {
      List<string> IDList = new List<string>();
      foreach (KeyValuePair<string, short> KVP in mHotKeyIDList)
      {
        IDList.Add(KVP.Key);
      }

      for (int i = 0; i <= IDList.Count - 1; i++)
      {
        RemoveHotKey(IDList[i]);
      }
    }

    public bool PreFilterMessage(ref Message m)
    {
      if (m.Msg == NativeFunctions.WM_HOTKEY)
      {
        if (HotKeyPressed != null)
        {
          HotKeyPressed(mHotKeyList[(short)m.WParam].HotKeyID);
        }
      }
      return false;
    }
  }
}
