using Eede.Domain.Palettes;
using Eede.Domain.Pictures;
using Eede.Domain.SharedKernel;
using System;
using System.Collections.Generic;

namespace Eede.Domain.ImageEditing.DrawingTools;

public class Drawer(Picture drawingPicture, PenStyle penCase)
{
    public readonly Picture DrawingPicture = drawingPicture;

    private readonly PenStyle PenStyle = penCase;

    public Picture DrawPoint(Position position)
    {
        int x = position.X;
        int y = position.Y;
        return DrawingPicture.Draw(dest =>
        {
            byte[] imageData = DrawPointRoutine(dest, dest.CloneImage(), position);
            return Picture.Create(dest.Size, imageData);
        }, PenStyle.Blender);
    }

    private byte[] DrawPointRoutine(Picture dest, byte[] imageData, Position position)
    {
        if (PenStyle.Width == 1)
        {
            return DrawFillEllipseRoutine(dest, imageData, position, position);
        }
        int d = PenStyle.Width / 2;
        int dd = PenStyle.Width - d;
        Position fromPosition = new(position.X - d, position.Y - d);
        Position toPosition = new(position.X + dd, position.Y + dd);
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
            int dx = x2 > x1 ? x2 - x1 : x1 - x2;
            int dy = y2 > y1 ? y2 - y1 : y1 - y2;

            /* 二点の方向 */
            int sx = x2 > x1 ? 1 : -1;
            int sy = y2 > y1 ? 1 : -1;

            int cx = x1;
            int cy = y1;

            int dx2 = dx * 2;
            int dy2 = dy * 2;

            ArgbColor color = PenStyle.Color;
            /* 傾きが1より小さい場合 */
            if (dx > dy)
            {
                int E = -dx;
                for (int i = 0; i <= dx; i++)
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
                for (int i = 0; i <= dy; i++)
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
            ArgbColor color = PenStyle.Color;

            (int fx, int fy, int tx, int ty) = NormalizePosition(from, to);

            int width = tx - fx;
            int height = ty - fy;
            int stride = dest.Stride;
            if (width == 0 && height == 0)
            {
                if (fx >= 0 && fx < dest.Width && fy >= 0 && fy < dest.Height)
                {
                    DrawPixel(imageData, stride, fx, fy, color);
                }
                return Picture.Create(dest.Size, imageData);
            }
            int centerX = fx + (width / 2);
            int centerY = fy + (height / 2);
            int diameter = Math.Min(width, height);
            int mirrorX = centerX;
            int mirrorY = centerY;
            if ((diameter & 1) == 1)
            {
                // 奇数直径対応時にいい感じの位置にするため右に1px、下に1px拡張している
                centerX++;
                centerY++;
            }

            int x = 0;
            int y = (diameter / 2) + 1;
            int d = (-diameter * diameter) + (4 * y * y) - (4 * y) + 2;
            int dx = 4;
            int dy = (-8 * y) + 8;
            while (x <= y)
            {
                if (d > 0)
                {
                    d += dy;
                    dy += 8;
                    y--;
                }
                int cx = centerX + x;
                int cy = centerY + y;
                int mx = mirrorX - x;
                int my = mirrorY - y;
                if (/*cx >= 0 && */cx < dest.Width)
                {
                    if (/*cy >= 0 &&*/ cy < dest.Height)
                    {
                        DrawPixel(imageData, stride, cx, cy, color);
                    }
                    if (my >= 0 /*&& my < dest.Height*/)
                    {
                        DrawPixel(imageData, stride, cx, my, color);
                    }
                }
                if (mx >= 0 /*&& mx < dest.Width*/)
                {
                    if (/*cy >= 0 &&*/ cy < dest.Height)
                    {
                        DrawPixel(imageData, stride, mx, cy, color);
                    }
                    if (my >= 0 /*&& my < dest.Height*/)
                    {
                        DrawPixel(imageData, stride, mx, my, color);
                    }
                }
                int cxy = centerX + y;
                int cyx = centerY + x;
                int mxy = mirrorX - y;
                int myx = mirrorY - x;
                if (/*cxy >= 0 &&*/ cxy < dest.Width)
                {
                    if (/*cyx >= 0 &&*/ cyx < dest.Height)
                    {
                        DrawPixel(imageData, stride, cxy, cyx, color);
                    }
                    if (myx >= 0 /*&& myx < dest.Height*/)
                    {
                        DrawPixel(imageData, stride, cxy, myx, color);
                    }
                }
                if (mxy >= 0 /*&& mxy < dest.Width*/)
                {
                    if (/*cyx >= 0 &&*/ cyx < dest.Height)
                    {
                        DrawPixel(imageData, stride, mxy, cyx, color);
                    }
                    if (myx >= 0 /*&& myx < dest.Height*/)
                    {
                        DrawPixel(imageData, stride, mxy, myx, color);
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
            byte[] imageData = DrawFillEllipseRoutine(dest, dest.CloneImage(), from, to);
            return Picture.Create(dest.Size, imageData);
        }, PenStyle.Blender);
    }

    private byte[] DrawFillEllipseRoutine(Picture dest, byte[] imageData, Position from, Position to)
    {
        ArgbColor color = PenStyle.Color;

        (int fx, int fy, int tx, int ty) = NormalizePosition(from, to);

        int width = tx - fx;
        int height = ty - fy;
        int stride = dest.Stride;
        if (width == 0 && height == 0)
        {
            if (fx >= 0 && fx < dest.Width && fy >= 0 && fy < dest.Height)
            {
                DrawPixel(imageData, stride, fx, fy, color);
            }
            return imageData;
        }
        int centerX = fx + (width / 2);
        int centerY = fy + (height / 2);
        int diameter = Math.Min(width, height);
        int mirrorX = centerX;
        int mirrorY = centerY;
        if ((diameter & 1) == 1)
        {
            // 奇数直径対応時にいい感じの位置にするため右に1px、下に1px拡張している
            centerX++;
            centerY++;
        }

        int x = 0;
        int y = (diameter / 2) + 1;
        int d = (-diameter * diameter) + (4 * y * y) - (4 * y) + 2;
        int dx = 4;
        int dy = (-8 * y) + 8;
        while (x <= y)
        {
            if (d > 0)
            {
                d += dy;
                dy += 8;
                y--;
            }
            int cx = Math.Min(centerX + x, dest.Width - 1);
            int cy = centerY + y;
            int mx = Math.Max(mirrorX - x, 0);
            int my = mirrorY - y;
            if (cy >= 0 && cy < dest.Height)
            {
                DrawScanLine(imageData, stride, mx, cy, cx, color);
            }
            if (my >= 0 && my < dest.Height)
            {
                DrawScanLine(imageData, stride, mx, my, cx, color);
            }
            int cxy = Math.Min(centerX + y, dest.Width - 1);
            int cyx = centerY + x;
            int mxy = Math.Max(mirrorX - y, 0);
            int myx = mirrorY - x;

            if (cyx >= 0 && cyx < dest.Height)
            {
                DrawScanLine(imageData, stride, mxy, cyx, cxy, color);
            }
            if (myx >= 0 && myx < dest.Height)
            {
                DrawScanLine(imageData, stride, mxy, myx, cxy, color);
            }

            d += dx;
            dx += 8;
            x++;
        }

        return imageData;
    }

    public Picture DrawRectangle(Position from, Position to)
    {
        return DrawingPicture.Draw(dest =>
        {
            byte[] imageData = dest.CloneImage();
            ArgbColor color = PenStyle.Color;
            int stride = dest.Stride;
            (int fx, int fy, int tx, int ty) = NormalizePosition(from, to);
            DrawScanLine(imageData, stride, fx, fy, tx, color);
            for (int i = fy; i < ty; i++)
            {
                DrawPixel(imageData, stride, fx, i, color);
                DrawPixel(imageData, stride, tx, i, color);
            }
            DrawScanLine(imageData, stride, fx, ty, tx, color);
            return Picture.Create(dest.Size, imageData);
        }, PenStyle.Blender);
    }

    public Picture DrawFillRectangle(Position from, Position to)
    {
        return DrawingPicture.Draw(dest =>
        {
            byte[] imageData = dest.CloneImage();
            ArgbColor color = PenStyle.Color;
            int stride = dest.Stride;
            (int fx, int fy, int tx, int ty) = NormalizePosition(from, to);

            for (int i = fy; i <= ty; i++)
            {
                DrawScanLine(imageData, stride, fx, i, tx, color);
            }
            return Picture.Create(dest.Size, imageData);
        }, PenStyle.Blender);
    }

    private static (int fx, int fy, int tx, int ty) NormalizePosition(Position from, Position to)
    {
        int fx = from.X;
        int fy = from.Y;
        int tx = to.X;
        int ty = to.Y;
        if (tx < fx)
        {
            (fx, tx) = (tx, fx);
        }
        if (ty < fy)
        {
            (fy, ty) = (ty, fy);
        }
        return (fx, fy, tx, ty);
    }


    private static void DrawPixel(byte[] imageArray, int stride, int x, int y, ArgbColor color)
    {
        int index = (x * 4) + (stride * y);
        imageArray[index] = color.Blue;
        imageArray[index + 1] = color.Green;
        imageArray[index + 2] = color.Red;
        imageArray[index + 3] = color.Alpha;
    }

    private static void DrawScanLine(byte[] imageArray, int stride, int x, int y, int toX, ArgbColor color)
    {
        int index = (x * 4) + (stride * y);
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
            ArgbColor color = PenStyle.Color;

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
                int index = (point.X * 4) + sy;

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
                    int leftIndex = ((leftX - 1) * 4) + sy;
                    if (!baseCol.EqualsArgb(PickColor(image, leftIndex)))
                    {
                        break;
                    }
                }
                /* search right point */
                for (; rightX < cWidth - 1; rightX++)
                {
                    int rightIndex = ((rightX + 1) * 4) + sy;
                    if (!baseCol.EqualsArgb(PickColor(image, rightIndex)))
                    {
                        break;
                    }
                }
                /* paint from leftX to rightX */
                if (leftX == rightX)
                {
                    DrawPixel(image, dest.Stride, leftX, point.Y, color);
                }
                else
                {
                    DrawScanLine(image, dest.Stride, leftX, point.Y, rightX, color);
                }
                /* search next lines */
                if (point.Y + 1 < cHeight)
                {
                    ScanLine(image, leftX, rightX, point.Y + 1, dest.Stride, buffer, baseCol);
                }
                if (point.Y - 1 >= 0)
                {
                    ScanLine(image, leftX, rightX, point.Y - 1, dest.Stride, buffer, baseCol);
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

    private static void ScanLine(byte[] image, int leftX, int rightX, int y, int stride, Stack<Position> buffer, ArgbColor baseCol)
    {
        int sy = stride * y;
        while (leftX <= rightX)
        {
            for (; leftX <= rightX; leftX++)
            {
                int index = (leftX * 4) + sy;
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
                int index = (leftX * 4) + sy;
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