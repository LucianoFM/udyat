using ScreenshotCaptureWithMouse.ScreenCapture;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Udyat.Class;

namespace CodeReflection.ScreenCapturingDemo
{

    /// <summary>
    /// Provides methods for capturing windows, and window icons as bitmap images
    /// </summary>
    public class ScreenCapturing
	{
        //private static int footerHeight = 70;
        public static string Version;
        public static Bitmap BitmapLogo;
        public static Font LegendFont;
        public static Font LegendDateFont;
        public static Color LegendColor;
        public static DataImage LegendData;
        public static Color FooterBackgroundColor;        

        static Bitmap CaptureCursor(ref int x, ref int y)
        {
            Bitmap bmp;
            IntPtr hicon;
            Win32Stuff.CURSORINFO ci = new Win32Stuff.CURSORINFO();
            Win32Stuff.ICONINFO icInfo;
            ci.cbSize = Marshal.SizeOf(ci);
            if (Win32Stuff.GetCursorInfo(out ci))
            {
                if (ci.flags == Win32Stuff.CURSOR_SHOWING)
                {
                    hicon = Win32Stuff.CopyIcon(ci.hCursor);
                    if (Win32Stuff.GetIconInfo(hicon, out icInfo))
                    {
                        x = ci.ptScreenPos.x - ((int)icInfo.xHotspot);
                        y = ci.ptScreenPos.y - ((int)icInfo.yHotspot);
                        Icon ic = Icon.FromHandle(hicon);
                        bmp = ic.ToBitmap();

                        return bmp;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a large icon from the window as a Bitmap 
        /// </summary>
        public static Bitmap GetWindowLargeIconAsBitmap(int handle)
		{	
			try
			{
				int result;		
				IntPtr hWnd = new IntPtr(handle);
				Win32.SendMessageTimeout(hWnd, Win32.WM_GETICON, Win32.ICON_BIG, 0, Win32.SMTO_ABORTIFHUNG, 1000, out result);
			
				IntPtr hIcon = new IntPtr(result);

				if (hIcon == IntPtr.Zero)
				{
					result = Win32.GetClassLong(hWnd, Win32.GCL_HICON);
					hIcon = new IntPtr(result);
				}
				
				if (hIcon == IntPtr.Zero)
				{
					Win32.SendMessageTimeout(hWnd, Win32.WM_QUERYDRAGICON, 0, 0, Win32.SMTO_ABORTIFHUNG, 1000, out result);
					hIcon = new IntPtr(result);
				}
				
				if (hIcon == IntPtr.Zero)
					return null;
				else
					return Bitmap.FromHicon(hIcon);	
			}
			catch(System.Exception)
			{
				//				System.Diagnostics.Trace.WriteLine(systemException);
			}
			return null;
		}

		/// <summary>
		/// Gets a small icon from the window as a Bitmap
		/// </summary>
		public static Bitmap GetWindowSmallIconAsBitmap(int handle)
		{			
			try
			{
				int result;
				IntPtr hWnd = new IntPtr(handle);	
				Win32.SendMessageTimeout(hWnd, Win32.WM_GETICON, Win32.ICON_SMALL, 0, Win32.SMTO_ABORTIFHUNG, 1000, out result);
			
				IntPtr hIcon = new IntPtr(result);

				if (hIcon == IntPtr.Zero)
				{
					result = Win32.GetClassLong(hWnd, Win32.GCL_HICONSM);
					hIcon = new IntPtr(result);
				}
				
				if (hIcon == IntPtr.Zero)
				{
					Win32.SendMessageTimeout(hWnd, Win32.WM_QUERYDRAGICON, 0, 0, Win32.SMTO_ABORTIFHUNG, 1000, out result);
					hIcon = new IntPtr(result);
				}

				if (hIcon == IntPtr.Zero)
					return null;
				else
					return Bitmap.FromHicon(hIcon);
			}
			catch(System.Exception)
			{
				//				System.Diagnostics.Trace.WriteLine(systemException);
			}
			return null;
		}

		/// <summary>
		/// Gets a Bitmap of the window (aka. screen capture)
		/// </summary>
		public static Bitmap GetPrimaryDesktopWindowCaptureAsBitmap()
		{			
			// create a graphics object from the window handle
			Graphics gfxWindow = Graphics.FromHwnd(IntPtr.Zero);

			// create a bitmap from the visible clipping bounds of the graphics object from the window
			Bitmap bitmap = new Bitmap((int)gfxWindow.VisibleClipBounds.Width, (int)gfxWindow.VisibleClipBounds.Height, gfxWindow);
				
			// create a graphics object from the bitmap
			Graphics gfxBitmap = Graphics.FromImage(bitmap);
			
			// get a device context for the window
			IntPtr hdcWindow = gfxWindow.GetHdc();
			
			// get a device context for the bitmap
			IntPtr hdcBitmap = gfxBitmap.GetHdc();
			
			// bitblt the window to the bitmap
			Win32.BitBlt(hdcBitmap, 0, 0, bitmap.Width, bitmap.Height, hdcWindow, 0, 0, (int)Win32.TernaryRasterOperations.SRCCOPY);
			
			// release the bitmap's device context
			gfxBitmap.ReleaseHdc(hdcBitmap);
			
			// release the window's device context
			gfxWindow.ReleaseHdc(hdcWindow);

			// dispose of the bitmap's graphics object
			gfxBitmap.Dispose();

			// dispose of the window's graphics object
			gfxWindow.Dispose();

			// return the bitmap of the window
			return bitmap;			
		}

        public static Bitmap CreateLegend(Bitmap pBackBitmap, Graphics g)
        {
            if (pBackBitmap != null)
            {
                Bitmap bitmapImg = new Bitmap(pBackBitmap);// Original Image
                Bitmap bitmapComment = new Bitmap(pBackBitmap.Width, LegendData.FooterHeight);// Footer
                Bitmap bitmapNewImage = new Bitmap(pBackBitmap.Width, pBackBitmap.Height + LegendData.FooterHeight);//New Image
                Graphics graphicImage = Graphics.FromImage(bitmapNewImage);
                graphicImage.Clear(FooterBackgroundColor);
                graphicImage.DrawImage(bitmapImg, new Point(0, 0));
                graphicImage.DrawImage(bitmapComment, new Point(bitmapComment.Width, 0));
                // Adiciona o logo
                graphicImage.DrawImage(BitmapLogo, new Point(40, bitmapImg.Height - 60));
                // Versão
                graphicImage.DrawString(Version, LegendFont, new SolidBrush(Color.Gray), 30, bitmapImg.Height + 20);// + footerHeight / 2);
                // Posição inicial da legenda
                int mLeft = 160;
                int mTop = bitmapImg.Height - 50;
                // Nome do Usuário
                graphicImage.DrawString(LegendData.ComputerName, LegendFont, new SolidBrush(LegendColor), mLeft, mTop);
                // Nome do Computador
                mTop += 30;
                graphicImage.DrawString(LegendData.IP, LegendFont, new SolidBrush(LegendColor), mLeft, mTop);
                // Data e hora da captura
                mTop += 30;
                graphicImage.DrawString(LegendData.DateTimeCapture, LegendDateFont, new SolidBrush(LegendColor), mLeft+160, mTop);
                // 
                //mLeft = 800;
                //mTop = bitmapImg.Height - 40;
                //graphicImage.DrawString(LegendData.DateTimeCapture, LegendDateFont, new SolidBrush(LegendColor), mLeft, mTop);

                //graphicImage.DrawRectangle(new Pen(Brushes.Red, 5), new Rectangle(0, bitmapNewImage.Height - footerHeight - 15, 5/*bitmapImg.Width*/, bitmapImg.Height));

                return bitmapNewImage;
               

            }
            return pBackBitmap;
        }

        public static Bitmap[] GetDesktopWindowCaptureAsBitmaps()
        {
            Screen[] screens = Screen.AllScreens;
            Bitmap[] result = new Bitmap[screens.Length];
            int[] xStartPosition = new int[screens.Length];
            Screen screen;
            Rectangle rcScreen = Rectangle.Empty;
            int cursorX = 0;
            int cursorY = 0;
            for (int idx=0; idx < screens.Length; idx++)
            {
                screen = screens[idx];
                // Guarda a posição lateral (X) inicial do monitor em questão
                xStartPosition[idx] = idx == 0 ? 0 : screens[idx - 1].Bounds.Width + xStartPosition[idx - 1];
                // Tamanho da tela
                rcScreen = new Rectangle(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
                IntPtr hdcSource = Win32.CreateDC(IntPtr.Zero, screen.DeviceName, IntPtr.Zero, IntPtr.Zero);
                // Blt the source directly to the composite destination...
                int xDest = screen.Bounds.X - rcScreen.X;
                int yDest = screen.Bounds.Y - rcScreen.Y;
                // Create a composite bitmap of the size of all screens...
                result[idx] = new Bitmap(screen.Bounds.Width, screen.Bounds.Height + LegendData.FooterHeight);
                // Get a graphics object for the composite bitmap and initialize it...
                Graphics g = Graphics.FromImage(result[idx]);
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.FillRectangle(SystemBrushes.Desktop, 0, 0, rcScreen.Width - rcScreen.X, rcScreen.Height - rcScreen.Y);
                // Get an HDC for the composite area...
                IntPtr hdcDestination = g.GetHdc();
                bool success = Win32.StretchBlt(hdcDestination, xDest, yDest, screen.Bounds.Width, screen.Bounds.Height, hdcSource, 0, 0, screen.Bounds.Width, screen.Bounds.Height, (int)Win32.TernaryRasterOperations.SRCCOPY);
                if (!success)
                {
                    System.ComponentModel.Win32Exception win32Exception = new System.ComponentModel.Win32Exception();
                    System.Diagnostics.Trace.WriteLine(win32Exception);
                }
                
                // Cleanup source HDC...
                Win32.DeleteDC(hdcSource);
                g.ReleaseHdc(hdcDestination);
                g.Dispose();
                // Adiciona a legenda
                if (LegendData.UseLegend)
                {
                    result[idx] = CreateLegend(result[idx], g);
                }


                // Adiciona o cursor
                cursorX = 0;
                cursorY = 0;
                Bitmap cursorBMP = CaptureCursor(ref cursorX, ref cursorY);
                if (result[idx] != null)
                {
                    if (cursorBMP != null)
                    {
                        cursorX = cursorX - xStartPosition[idx];
                        cursorY = cursorY + LegendData.FooterHeight;
                        Rectangle r = new Rectangle(cursorX, cursorY, cursorBMP.Width, cursorBMP.Height);
                        g = Graphics.FromImage(result[idx]);
                        g.DrawImage(cursorBMP, cursorX, cursorY);
                        g.Flush();
                    }
                }

                // Carimbo de sistema TRIAL
                if (LegendData.IsTrialVersion)
                {
                    using (Graphics gr = Graphics.FromImage(result[idx]))
                    {
                        float w = result[idx].Width / 2f;
                        float h = result[idx].Height / 2f;
                        gr.TranslateTransform(w, h);
                        gr.RotateTransform(45);
                        SizeF size = gr.MeasureString("Versão TRIAL de uso temporário", new Font("Tahoma", 30));
                        PointF drawPoint = new PointF(-size.Width / 2f, -size.Height / 2f);
                        gr.DrawString("Versão TRIAL de uso temporário", new Font("Tahoma", 30), Brushes.Yellow, drawPoint);
                    }
                }

            }
            
            return result;
        }

        public static Bitmap GetDesktopWindowCaptureAsBitmap()
		{
			Rectangle rcScreen = Rectangle.Empty;
			Screen[] screens = Screen.AllScreens;
            int[] xStartPosition = new int[screens.Length];
            int idx = 0;
            int cursorX = 0;
            int cursorY = 0;

            // Create a rectangle encompassing all screens...
            foreach (Screen screen in screens)
				rcScreen = Rectangle.Union(rcScreen, screen.Bounds);
			//			System.Diagnostics.Trace.WriteLine(rcScreen);

			// Create a composite bitmap of the size of all screens...
			Bitmap finalBitmap = new Bitmap(rcScreen.Width, rcScreen.Height + LegendData.FooterHeight);

			// Get a graphics object for the composite bitmap and initialize it...
			Graphics g = Graphics.FromImage(finalBitmap);
			g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
			g.FillRectangle(SystemBrushes.Desktop, 0, 0, rcScreen.Width - rcScreen.X, rcScreen.Height - rcScreen.Y);
            // Get an HDC for the composite area...
            IntPtr hdcDestination = g.GetHdc();

			// Now, loop through screens, BitBlting each to the composite HDC created above...
			foreach(Screen screen in screens)
			{
                // Guarda a posição lateral (X) inicial do monitor em questão
                xStartPosition[idx] = idx == 0 ? 0 : screens[idx - 1].Bounds.Width + xStartPosition[idx - 1];
                // Create DC for each source monitor...
                IntPtr hdcSource = Win32.CreateDC(IntPtr.Zero, screen.DeviceName, IntPtr.Zero, IntPtr.Zero);
				// Blt the source directly to the composite destination...
				int xDest = screen.Bounds.X - rcScreen.X;
				int yDest = screen.Bounds.Y - rcScreen.Y;
				//				bool success = BitBlt(hdcDestination, xDest, yDest, screen.Bounds.Width, screen.Bounds.Height, hdcSource, 0, 0, (int)TernaryRasterOperations.SRCCOPY);
				bool success = Win32.StretchBlt(hdcDestination, xDest, yDest, screen.Bounds.Width, screen.Bounds.Height, hdcSource, 0, 0, screen.Bounds.Width, screen.Bounds.Height, (int)Win32.TernaryRasterOperations.SRCCOPY);
				//				System.Diagnostics.Trace.WriteLine(screen.Bounds);
				if (!success)
				{
					System.ComponentModel.Win32Exception win32Exception = new System.ComponentModel.Win32Exception();					
					System.Diagnostics.Trace.WriteLine(win32Exception);
				}
				// Cleanup source HDC...
				Win32.DeleteDC(hdcSource);				
			}
			// Cleanup destination HDC and Graphics...
			g.ReleaseHdc(hdcDestination);
			g.Dispose();
            // Adiciona Legenda
            if (LegendData.UseLegend)
            {
                finalBitmap = CreateLegend(finalBitmap, g);
            }            
            // Adiciona o cursor
            cursorX = 0;
            cursorY = 0;
            Bitmap cursorBMP = CaptureCursor(ref cursorX, ref cursorY);
            if (finalBitmap != null)
            {
                if (cursorBMP != null)
                {
                    cursorX = cursorX - xStartPosition[idx];
                    cursorY = cursorY + LegendData.FooterHeight;
                    Rectangle r = new Rectangle(cursorX, cursorY, cursorBMP.Width, cursorBMP.Height);
                    g = Graphics.FromImage(finalBitmap);
                    g.DrawImage(cursorBMP, r);
                    g.Flush();
                    return finalBitmap;
                }
                else
                    return finalBitmap;
            }
            idx += 1;
            return finalBitmap;
		}

		/// <summary>
		/// Gets a Bitmap of the window (aka. screen capture)
		/// </summary>
		public static Bitmap GetWindowCaptureAsBitmap(int handle)
		{
			IntPtr hWnd = new IntPtr(handle);
			Win32.Rect rc = new Win32.Rect();
			if (!Win32.GetWindowRect(hWnd, ref rc))
				return null;

//			Win32.WindowInfo wi = new Win32.WindowInfo();
//			wi.size = Marshal.SizeOf(wi);
//			if (!Win32.GetWindowInfo(hWnd, ref wi))
//				return null;
						
			// create a bitmap from the visible clipping bounds of the graphics object from the window
			Bitmap bitmap = new Bitmap(rc.Width, rc.Height);
			
			// create a graphics object from the bitmap
			Graphics gfxBitmap = Graphics.FromImage(bitmap);
			
			// get a device context for the bitmap
			IntPtr hdcBitmap = gfxBitmap.GetHdc();

			// get a device context for the window
			IntPtr hdcWindow = Win32.GetWindowDC(hWnd);
						
			// bitblt the window to the bitmap
			Win32.BitBlt(hdcBitmap, 0, 0, rc.Width, rc.Height, hdcWindow, 0, 0, (int)Win32.TernaryRasterOperations.SRCCOPY);
			
			// release the bitmap's device context
			gfxBitmap.ReleaseHdc(hdcBitmap);					

			Win32.ReleaseDC(hWnd, hdcWindow);

			// dispose of the bitmap's graphics object
			gfxBitmap.Dispose();		

			// return the bitmap of the window
			return bitmap;			
		}

		/// <summary>
		/// Convert a bitmap to a byte array
		/// </summary>
		/// <param name="bmp"></param>
		/// <returns></returns>
		public static byte[] GetBytes(Bitmap bmp)
		{
			if (bmp == null)
				return new byte[0];

			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
			// maybe 
			// bmp.Dispose();
						
			return stream.GetBuffer();
		}

		public static byte[] GetBytes(Icon icon)
		{
			if (icon == null)
				return new byte[0];

			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			icon.Save(stream);
			return stream.GetBuffer();
		}

		public static Icon GetIcon(byte[] bytes)
		{
			if (bytes == null)
				return null;

			if (bytes.Length == 0)
				return null;

			System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
			Icon icon = new Icon(stream);
			stream.Close();
			try
			{
				return (Icon)icon.Clone();
			}
			catch(Exception ex)
			{
				Trace.WriteLine(ex);
			}
			finally
			{
				icon.Dispose();
			}
			return null;
		}

		/// <summary>
		/// Converts a byte array to a bitmap
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static Bitmap GetBitmap(byte[] bytes)
		{
			if (bytes == null)
				return null;
			
			if (bytes.Length == 0)
				return null;

			System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
			Bitmap b = new Bitmap(stream);
			stream.Close();
			try
			{
				return (Bitmap)b.Clone();
			}
			catch(Exception ex)
			{
				Trace.WriteLine(ex);
			}
			finally
			{
				b.Dispose();
			}
			return null;
		}

		/// <summary>
		/// Gets a byte array of a bitmap for the large icon for the window specified by the handle 
		/// </summary>
		/// <param name="hWnd"></param>
		/// <returns></returns>
		public static byte[] GetWindowLargeIconAsByteArray(int handle)
		{
			return ScreenCapturing.GetBytes(ScreenCapturing.GetWindowLargeIconAsBitmap(handle));
		}

		/// <summary>
		/// Gets a byte array of a bitmap for the small icon for the window specified by the handle 
		/// </summary>
		/// <param name="hWnd"></param>
		/// <returns></returns>
		public static byte[] GetWindowSmallIconAsByteArray(int handle)
		{
			return ScreenCapturing.GetBytes(ScreenCapturing.GetWindowSmallIconAsBitmap(handle));
		}

		/// <summary>
		/// Gets a byte array of a bitmap for the desktop window
		/// </summary>
		/// <returns></returns>
		public static byte[] GetDesktopWindowCaptureAsByteArray()
		{
			return ScreenCapturing.GetBytes(ScreenCapturing.GetDesktopWindowCaptureAsBitmap());
		}
		
		/// <summary>
		/// Gets a byte array of a bitmap for the window specified by the handle 
		/// </summary>
		/// <param name="hWnd"></param>
		/// <returns></returns>
		public static byte[] GetWindowCaptureAsByteArray(int handle)
		{
			return ScreenCapturing.GetBytes(ScreenCapturing.GetWindowCaptureAsBitmap(handle));
		}	

		/// <summary>
		/// Calculates the total screen size, for all attached monitors, by unioning each monitor's bounds together
		/// </summary>
		/// <returns></returns>
		public static Rectangle CalculateTotalScreenSize()
		{
			Rectangle rcScreen = Rectangle.Empty;
			Screen[] screens = Screen.AllScreens;

			// Create a rectangle encompassing all screens...
			foreach(Screen screen in screens)			
				rcScreen = Rectangle.Union(rcScreen, screen.Bounds);

			return rcScreen;
		}
	}

}
