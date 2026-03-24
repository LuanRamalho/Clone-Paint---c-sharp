using System;
using System.Collections.Generic;
using System.Drawing;

namespace Win11PaintClone
{
    public static class DrawingHelper
    {
        // Algoritmo de Flood Fill (Balde de Tinta)
        public static void FloodFill(Bitmap bitmap, Point pt, Color targetColor, Color replacementColor)
        {
            if (targetColor.ToArgb() == replacementColor.ToArgb()) return;

            Stack<Point> pixels = new Stack<Point>();
            pixels.Push(pt);

            while (pixels.Count > 0)
            {
                Point a = pixels.Pop();
                if (a.X < bitmap.Width && a.X >= 0 && a.Y < bitmap.Height && a.Y >= 0)
                {
                    if (bitmap.GetPixel(a.X, a.Y) == targetColor)
                    {
                        bitmap.SetPixel(a.X, a.Y, replacementColor);
                        pixels.Push(new Point(a.X - 1, a.Y));
                        pixels.Push(new Point(a.X + 1, a.Y));
                        pixels.Push(new Point(a.X, a.Y - 1));
                        pixels.Push(new Point(a.X, a.Y + 1));
                    }
                }
            }
        }

        // Desenha uma Estrela de 5 pontas
        public static void DrawStar(Graphics g, Pen pen, Rectangle rect)
        {
            PointF[] points = new PointF[10];
            double angle = -Math.PI / 2;
            double decr = Math.PI / 5;
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            float rx = rect.Width / 2f;
            float ry = rect.Height / 2f;

            for (int i = 0; i < 10; i++)
            {
                float r = (i % 2 == 0) ? rx : rx * 0.4f;
                points[i] = new PointF(cx + (float)(r * Math.Cos(angle)), cy + (float)(r * Math.Sin(angle)));
                angle += decr;
            }
            g.DrawPolygon(pen, points);
        }

        // Desenha uma Seta
        public static void DrawArrow(Graphics g, Pen pen, Point start, Point end)
        {
            float headLength = 15f;
            double angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
            g.DrawLine(pen, start, end);
            g.DrawLine(pen, end, new Point((int)(end.X - headLength * Math.Cos(angle - Math.PI / 6)), (int)(end.Y - headLength * Math.Sin(angle - Math.PI / 6))));
            g.DrawLine(pen, end, new Point((int)(end.X - headLength * Math.Cos(angle + Math.PI / 6)), (int)(end.Y - headLength * Math.Sin(angle + Math.PI / 6))));
        }
    }
}