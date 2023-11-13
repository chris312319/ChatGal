using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.IO;

public class TablePetBack : MonoBehaviour
{
    public string strProduct;//项目名称
    public int windowWidth;//窗口宽度
    public int windowHeight;//窗口高度
    private int currentX;
    private int currentY;
    #region Win函数常量
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll")]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
    [DllImport("user32.dll")]
    static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, int bAlpha, int dwFlags);
    [DllImport("Dwmapi.dll")]
    static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    private const int GWL_EXSTYLE = -20;
    private const int GWL_STYLE = -16;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_BORDER = 0x00800000;
    private const int WS_CAPTION = 0x00C00000;
    private const int SWP_SHOWWINDOW = 0x0040;
    private const int LWA_COLORKEY = 0x00000001;
    private const int LWA_ALPHA = 0x00000002;
    private const int WS_EX_TRANSPARENT = 0x20;
    private const int ULW_COLORKEY = 0x00000001;
    private const int ULW_ALPHA = 0x00000002;
    private const int ULW_OPAQUE = 0x00000004;
    private const int ULW_EX_NORESIZE = 0x00000008;
    #endregion
    IntPtr hwnd;
    // Use this for initialization
    void Awake()
    {
        Screen.fullScreen = false;
#if UNITY_EDITOR
        print("编辑模式不更改窗体");
#else
        hwnd = FindWindow(null, strProduct);
        //去边框并且透明
        SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_LAYERED);
        int intExTemp = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_BORDER & ~WS_CAPTION);

        //保持右下角间位置
        currentX = Screen.currentResolution.width - windowWidth;
        currentY = Screen.currentResolution.height - windowHeight;
        SetWindowPos(hwnd, -1, currentX, currentY, windowWidth, windowHeight, SWP_SHOWWINDOW);
        var margins = new MARGINS() { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hwnd, ref margins);
#endif
    }

    void OnApplicationQuit()
    {
        SetWindowPos(hwnd, -1, currentX, currentY, 0, 0, SWP_SHOWWINDOW);
    }
}

