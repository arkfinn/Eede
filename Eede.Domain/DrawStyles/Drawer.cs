using Eede.Domain.Colors;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Eede.Domain.DrawStyles
{
    public class Drawer
    {
        public readonly Picture DrawingPicture;

        private readonly PenStyle PenStyle;

        public Drawer(Picture drawingPicture, PenStyle penCase)
        {
            DrawingPicture = drawingPicture;
            PenStyle = penCase;
        }

        public Picture DrawPoint(Position position)
        {
            int x = position.X;
            int y = position.Y;
            return DrawingPicture.Draw(dest =>
            {
                var imageData = DrawPointRoutine(dest, dest.CloneImage(), position);
                return Picture.Create(dest.Size, imageData);
            }, PenStyle.Blender);
        }

        private byte[] DrawPointRoutine(Picture dest, byte[] imageData, Position position)
        {
            if (PenStyle.Width == 1)
            {
                return DrawFillEllipseRoutine(dest, imageData, position, position);
            }
            var d = PenStyle.Width / 2;
            var dd = PenStyle.Width - d;
            var fromPosition = new Position(position.X - d, position.Y - d);
            var toPosition = new Position(position.X + dd, position.Y + dd);
            return DrawFillEllipseRoutine(dest, imageData, fromPosition, toPosition);
        }

        public Picture DrawLine(Position from, Position to)
        {
            return DrawingPicture.Draw(dest =>
            {
                byte[] imageData = dest.CloneImage();
                int width = dest.Width;
                int x1 = from.X;
                int y1 = from.Y;
                int x2 = to.X;
                int y2 = to.Y;
                /* 二点間の距離 */
                var dx = (x2 > x1) ? x2 - x1 : x1 - x2;
                var dy = (y2 > y1) ? y2 - y1 : y1 - y2;

                /* 二点の方向 */
                var sx = (x2 > x1) ? 1 : -1;
                var sy = (y2 > y1) ? 1 : -1;

                var cx = x1;
                var cy = y1;

                var dx2 = dx * 2;
                var dy2 = dy * 2;

                var color = PenStyle.Color;
                /* 傾きが1より小さい場合 */
                if (dx > dy)
                {
                    int E = -dx;
                    for (var i = 0; i <= dx; i++)
                    {
                        if (cy >= 0 && cy < dest.Height && cx >= 0 && cx < dest.Width)
                        {
                            imageData = DrawPointRoutine(dest, imageData, new Position(cx, cy));
                            //int index = cx * 4 + dest.Stride * cy;
                            //imageData[index + 0] = color.B;
                            //imageData[index + 1] = color.G;
                            //imageData[index + 2] = color.R;
                            //imageData[index + 3] = color.A;
                        }
                        cx += sx;
                        E += dy2;
                        if (E > 0)
                        {
                            cy += sy;
                            E -= dx2;
                        }

                    }
                }
                /* 傾きが1以上の場合 */
                else
                {
                    int E = -dy;
                    for (var i = 0; i <= dy; i++)
                    {
                        if (cy >= 0 && cy < dest.Height && cx >= 0 && cx < dest.Width)
                        {
                            imageData = DrawPointRoutine(dest, imageData, new Position(cx, cy));
                            //int index = cx * 4 + dest.Stride * cy;
                            //imageData[index + 0] = color.B;
                            //imageData[index + 1] = color.G;
                            //imageData[index + 2] = color.R;
                            //imageData[index + 3] = color.A;
                        }
                        cy += sy;
                        E += dx2;
                        if (E > 0)
                        {
                            cx += sx;
                            E -= dy2;
                        }
                    }
                }

                return Picture.Create(dest.Size, imageData);
            }, PenStyle.Blender);
        }

        public Picture DrawEllipse(Position from, Position to)
        {
            return DrawingPicture.Draw(dest =>
            {
                byte[] imageData = dest.CloneImage();
                var color = PenStyle.Color;

                var fx = from.X;
                var fy = from.Y;
                var tx = to.X;
                var ty = to.Y;
                if (tx > fx)
                {
                    (fx, tx) = (tx, fx);
                }
                if (ty > fy)
                {
                    (fy, ty) = (ty, fy);
                }

                int width = fx - tx;
                int height = fy - ty;
                var stride = dest.Stride;
                if (width == 0 && height == 0)
                {
                    if (fx >= 0 && fx < dest.Width && fy >= 0 && fy < dest.Height)
                    {
                        drawPixel(imageData, stride, fx, fy, color);
                    }
                    return Picture.Create(dest.Size, imageData);
                }
                int centerX = fx + width / 2;
                int centerY = fy + height / 2;
                int diameter = Math.Min(width, height);
                var mirrorX = centerX;
                var mirrorY = centerY;
                if ((diameter & 1) == 0)
                {
                    mirrorX++;
                    mirrorY++;
                }

                int x = 0;
                int y = diameter / 2 + 1;
                int d = -diameter * diameter + 4 * y * y - 4 * y + 2;
                var dx = 4;
                var dy = -8 * y + 8;
                while (x <= y)
                {
                    if (d > 0)
                    {
                        d += dy;
                        dy += 8;
                        y--;
                    }
                    var cx = centerX + x;
                    var cy = centerY + y;
                    var mx = mirrorX - x;
                    var my = mirrorY - y;
                    if (/*cx >= 0 && */cx < dest.Width)
                    {
                        if (/*cy >= 0 &&*/ cy < dest.Height)
                        {
                            drawPixel(imageData, stride, cx, cy, color);
                        }
                        if (my >= 0 /*&& my < dest.Height*/)
                        {
                            drawPixel(imageData, stride, cx, my, color);
                        }
                    }
                    if (mx >= 0 /*&& mx < dest.Width*/)
                    {
                        if (/*cy >= 0 &&*/ cy < dest.Height)
                        {
                            drawPixel(imageData, stride, mx, cy, color);
                        }
                        if (my >= 0 /*&& my < dest.Height*/)
                        {
                            drawPixel(imageData, stride, mx, my, color);
                        }
                    }
                    var cxy = centerX + y;
                    var cyx = centerY + x;
                    var mxy = mirrorX - y;
                    var myx = mirrorY - x;
                    if (/*cxy >= 0 &&*/ cxy < dest.Width)
                    {
                        if (/*cyx >= 0 &&*/ cyx < dest.Height)
                        {
                            drawPixel(imageData, stride, cxy, cyx, color);
                        }
                        if (myx >= 0 /*&& myx < dest.Height*/)
                        {
                            drawPixel(imageData, stride, cxy, myx, color);
                        }
                    }
                    if (mxy >= 0 /*&& mxy < dest.Width*/)
                    {
                        if (/*cyx >= 0 &&*/ cyx < dest.Height)
                        {
                            drawPixel(imageData, stride, mxy, cyx, color);
                        }
                        if (myx >= 0 /*&& myx < dest.Height*/)
                        {
                            drawPixel(imageData, stride, mxy, myx, color);
                        }
                    }
                    d += dx;
                    dx += 8;
                    x++;
                }

                return Picture.Create(dest.Size, imageData);
            }, PenStyle.Blender);
        }

        public Picture DrawFillEllipse(Position from, Position to)
        {
            return DrawingPicture.Draw(dest =>
            {
                var imageData = DrawFillEllipseRoutine(dest, dest.CloneImage(), from, to);
                return Picture.Create(dest.Size, imageData);
            }, PenStyle.Blender);
        }

        private byte[] DrawFillEllipseRoutine(Picture dest, byte[] imageData, Position from, Position to)
        {
            var color = PenStyle.Color;

            var fx = from.X;
            var fy = from.Y;
            var tx = to.X;
            var ty = to.Y;
            if (tx < fx)
            {
                (fx, tx) = (tx, fx);
            }
            if (ty < fy)
            {
                (fy, ty) = (ty, fy);
            }

            int width = tx - fx;
            int height = ty - fy;
            var stride = dest.Stride;
            if (width == 0 && height == 0)
            {
                if (fx >= 0 && fx < dest.Width && fy >= 0 && fy < dest.Height)
                {
                    drawPixel(imageData, stride, fx, fy, color);
                }
                return imageData;
            }
            int centerX = fx + width / 2;
            int centerY = fy + height / 2;
            int diameter = Math.Min(width, height);
            var mirrorX = centerX;
            var mirrorY = centerY;
            if ((diameter & 1) == 0)
            {
                // 偶数直径対応時にいい感じの位置にするため右に1px、上に1px拡張している
                mirrorX++;
                //mirrorY++;
                centerY--;
            }

            int x = 0;
            int y = diameter / 2 + 1;
            int d = -diameter * diameter + 4 * y * y - 4 * y + 2;
            var dx = 4;
            var dy = -8 * y + 8;
            while (x <= y)
            {
                if (d > 0)
                {
                    d += dy;
                    dy += 8;
                    y--;
                }
                var cx = Math.Min(centerX + x, dest.Width - 1);
                var cy = centerY + y;
                var mx = Math.Max(mirrorX - x, 0);
                var my = mirrorY - y;
                if (cy >= 0 && cy < dest.Height)
                {
                    drawScanLine(imageData, stride, mx, cy, cx, color);
                }
                if (my >= 0 && my < dest.Height)
                {
                    drawScanLine(imageData, stride, mx, my, cx, color);
                }
                var cxy = Math.Min(centerX + y, dest.Width - 1);
                var cyx = centerY + x;
                var mxy = Math.Max(mirrorX - y, 0);
                var myx = mirrorY - x;

                if (cyx >= 0 && cyx < dest.Height)
                {
                    drawScanLine(imageData, stride, mxy, cyx, cxy, color);
                }
                if (myx >= 0 && myx < dest.Height)
                {
                    drawScanLine(imageData, stride, mxy, myx, cxy, color);
                }

                d += dx;
                dx += 8;
                x++;
            }

            return imageData;
        }

        private static void drawPixel(byte[] imageArray, int stride, int x, int y, ArgbColor color)
        {
            int index = x * 4 + stride * y;
            imageArray[index] = color.Blue;
            imageArray[index + 1] = color.Green;
            imageArray[index + 2] = color.Red;
            imageArray[index + 3] = color.Alpha;
        }

        private static void drawScanLine(byte[] imageArray, int stride, int x, int y, int toX, ArgbColor color)
        {
            int index = x * 4 + stride * y;
            for (int i = x; i <= toX; i++)
            {
                imageArray[index] = color.Blue;
                imageArray[index + 1] = color.Green;
                imageArray[index + 2] = color.Red;
                imageArray[index + 3] = color.Alpha;
                index += 4;
            }
        }

        public Picture Fill(Position from)
        {
            return DrawingPicture.Draw(dest =>
            {
                int cWidth = dest.Width;
                int cHeight = dest.Height;
                var color = PenStyle.Color;

                ArgbColor baseCol = dest.PickColor(from);

                if (color.EqualsArgb(baseCol))
                {
                    return dest;
                }
                byte[] image = dest.CloneImage();
                Stack<Position> buffer = new();
                buffer.Push(from);
                //Pen p = new Pen(col);
                while (buffer.Count > 0)
                {
                    Position point = buffer.Pop();
                    int sy = dest.Stride * point.Y;
                    int index = point.X * 4 + sy;

                    /* skip already painted */
                    if (color.EqualsArgb(PickColor(image, index)))
                    {
                        continue;
                    }

                    int leftX = point.X;
                    int rightX = point.X;
                    /* search left point */
                    for (; 0 < leftX; leftX--)
                    {
                        int leftIndex = (leftX - 1) * 4 + sy;
                        if (!baseCol.EqualsArgb(PickColor(image, leftIndex)))
                        {
                            break;
                        }
                    }
                    /* search right point */
                    for (; rightX < cWidth - 1; rightX++)
                    {
                        int rightIndex = (rightX + 1) * 4 + sy;
                        if (!baseCol.EqualsArgb(PickColor(image, rightIndex)))
                        {
                            break;
                        }
                    }
                    /* paint from leftX to rightX */
                    if (leftX == rightX)
                    {
                        drawPixel(image, dest.Stride, leftX, point.Y, color);
                    }
                    else
                    {
                        drawScanLine(image, dest.Stride, leftX, point.Y, rightX, color);
                    }
                    /* search next lines */
                    if (point.Y + 1 < cHeight)
                    {
                        scanLine(image, leftX, rightX, point.Y + 1, dest.Stride, buffer, baseCol);
                    }
                    if (point.Y - 1 >= 0)
                    {
                        scanLine(image, leftX, rightX, point.Y - 1, dest.Stride, buffer, baseCol);
                    }
                }
                return Picture.Create(dest.Size, image);
            }, PenStyle.Blender);
        }

        private static ArgbColor PickColor(byte[] image, int index)
        {
            return new ArgbColor(
                image[index + 3],
                image[index + 2],
                image[index + 1],
                image[index]);
        }

        private void scanLine(byte[] image, int leftX, int rightX, int y, int stride, Stack<Position> buffer, ArgbColor baseCol)
        {
            int sy = stride * y;
            while (leftX <= rightX)
            {
                for (; leftX <= rightX; leftX++)
                {
                    int index = leftX * 4 + sy;
                    if (baseCol.EqualsArgb(PickColor(image, index)))
                    {
                        break;
                    }
                }
                if (rightX < leftX)
                {
                    break;
                }
                for (; leftX <= rightX; leftX++)
                {
                    int index = leftX * 4 + sy;
                    if (!baseCol.EqualsArgb(PickColor(image, index)))
                    {
                        break;
                    }
                }
                buffer.Push(new Position(leftX - 1, y));
            }
        }

        public bool Contains(Position position)
        {
            return DrawingPicture.Contains(position);
        }
    }
}