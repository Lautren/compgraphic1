using lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;


namespace DX2
{
    public class NativeWindow : HwndHost
    {

        public new IntPtr Handle { get; private set; }
        Procedure procedure;
        Scene scene; // Объект класса Scene для рисования
        bool selectpolystate = false;
        
        enum SELECT_POINT_STATES { SPS_NOP,SPS_SELECT,SPS_MOVE};
        SELECT_POINT_STATES selectpointstate = SELECT_POINT_STATES.SPS_NOP;
        const int WM_PAINT = 0x000F;
        const int WM_SIZE = 0x0005;
        const int WM_LBUTTONUP = 0x0202;

        [StructLayout(LayoutKind.Sequential)]
        struct WindowClass
        {
            public uint Style;
            public IntPtr Callback;
            public int ClassExtra;
            public int WindowExtra;
            public IntPtr Instance;
            public IntPtr Icon;
            public IntPtr Cursor;
            public IntPtr Background;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Menu;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Class;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Paint
        {
            public IntPtr Context;
            public bool Erase;
            public Rect Area;
            public bool Restore;
            public bool Update;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] Reserved;
        }

        delegate IntPtr Procedure
            (IntPtr handle,
            uint message,
            IntPtr wparam,
            IntPtr lparam);

        [DllImport("user32.dll")]
        static extern IntPtr CreateWindowEx
            (uint extended,
            [MarshalAs(UnmanagedType.LPWStr)] 
        string name,
            [MarshalAs(UnmanagedType.LPWStr)]
        string caption,
            uint style,
            int x,
            int y,
            int width,
            int height,
            IntPtr parent,
            IntPtr menu,
            IntPtr instance,
            IntPtr param);

        [DllImport("user32.dll")]
        static extern IntPtr LoadCursor
            (IntPtr instance,
            int name);

        [DllImport("user32.dll")]
        static extern IntPtr DefWindowProc
            (IntPtr handle,
            uint message,
            IntPtr wparam,
            IntPtr lparam);

        [DllImport("user32.dll")]
        static extern ushort RegisterClass
            ([In] 
        ref WindowClass register);

        [DllImport("user32.dll")]
        static extern int InvalidateRect
            ([In] 
        IntPtr handle, IntPtr rect,int Erase);


        [DllImport("user32.dll")]
        static extern bool DestroyWindow
            (IntPtr handle);

        [DllImport("user32.dll")]
        static extern IntPtr BeginPaint
            (IntPtr handle,
            out Paint paint);

        [DllImport("user32.dll")]
        static extern bool EndPaint
            (IntPtr handle,
            [In] ref Paint paint);

        protected override HandleRef BuildWindowCore(HandleRef parent)
        {
            var callback = Marshal.GetFunctionPointerForDelegate(procedure = WndProc);
            var width = Convert.ToInt32(ActualWidth);
            var height = Convert.ToInt32(ActualHeight);
            var cursor = LoadCursor(IntPtr.Zero, 32512);
            var menu = string.Empty;
            var background = new IntPtr(1);
            var zero = IntPtr.Zero;
            var caption = string.Empty;
            var style = 3u;
            var extra = 0;
            var extended = 0u;
            var window = 0x50000000u;
            var point = 0;
            var name = "Win32";

            var wnd = new WindowClass
            {
                Style = style,
                Callback = callback,
                ClassExtra = extra,
                WindowExtra = extra,
                Instance = zero,
                Icon = zero,
                Cursor = cursor,
                Background = background,
                Menu = menu,
                Class = name
            };

            RegisterClass(ref wnd);
            Handle = CreateWindowEx(extended, name, caption,
                window, point, point, width, height,
                parent.Handle, zero, zero, zero);
            
            scene = new Scene(Handle); // Создание нового объекта Scene

            return new HandleRef(this, Handle);
        }

        protected override void DestroyWindowCore(HandleRef handle)
        {
            DestroyWindow(handle.Handle);
        }

        protected override IntPtr WndProc(IntPtr handle, int message, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            try
            {
                switch (message)
                {
                    case WM_PAINT:
                    {
                    Paint paint;
                    BeginPaint(handle, out paint);
                    scene.Draw(); // Перерисовка содержимого
                    EndPaint(handle, ref paint);
                    handled = true;
                    }
                    break;
                    case WM_SIZE:
                           scene.Resize(handle); // Обработка изменения размеров
                            handled = true;
                            break;
                    case WM_LBUTTONUP:
                        {
                            int x = (int)lparam;
                            // выбор линии
                            if (selectpolystate)
                            {
                                selectpolystate = false;
                                scene.ChangePoly(x & 0xffff, x >> 16);
                                InvalidateRect(handle, (IntPtr)0, 1);
                                RestorePolyColor();
                            }
                            else
                            // выбор точки
                            if (selectpointstate == SELECT_POINT_STATES.SPS_SELECT)
                            {
                                selectpointstate = SELECT_POINT_STATES.SPS_MOVE;
                                scene.ChangePoint(x & 0xffff, x >> 16);
                                InvalidateRect(handle, (IntPtr)0, 1);
                                
                            }
                            else
                            // перенос точки
                            if (selectpointstate == SELECT_POINT_STATES.SPS_MOVE)
                            {
                                selectpointstate = SELECT_POINT_STATES.SPS_NOP;
                                scene.ChangePoint(x & 0xffff, x >> 16);
                                InvalidateRect(handle, (IntPtr)0, 1);
                            }
                            // построение ломаной
                            else
                            {
                                scene.AddPoint(x & 0xffff, x >> 16);
                                InvalidateRect(handle, (IntPtr)0, 1);
                            }
                        }
                            break;

                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return base.WndProc(handle, message, wparam, lparam, ref handled);
        }
        static IntPtr WndProc(IntPtr handle, uint message, IntPtr wparam, IntPtr lparam)
        {
            return DefWindowProc(handle, message, wparam, lparam);
        }
        static public NativeWindow self;
        public NativeWindow() { self = this; }

        public void SelectPoly()
        {
            selectpolystate = true;
        }
        public void SelectPoint()
        {
           selectpointstate = SELECT_POINT_STATES.SPS_SELECT;
        }

        public void RestorePolyColor()
        {
            Task.Delay(500).ContinueWith(_ =>
            {
                scene.RestorePolyColor();
                InvalidateRect(Handle, (IntPtr)0, 1);
            }
            );
        }

        public void SetGradient(uint color1, uint color2, uint style)
        {
            scene.SetGradient(color1, color2, style);
            InvalidateRect(Handle, (IntPtr)0, 1);
        }
        public void SetColor(uint color)
        {
            scene.SetColor(color);
            InvalidateRect(Handle, (IntPtr)0, 1);
        }
        public void DeletePoly()
        {
            scene.DeletePoly();
            InvalidateRect(Handle, (IntPtr)0, 1);
        }
        public void DeletePoint()
        {
            scene.DeletePoint();
            InvalidateRect(Handle, (IntPtr)0, 1);
        }
        public void NewPoly()
        {
            scene.NewPoly();
        }

    }
}
