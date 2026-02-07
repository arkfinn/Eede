using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using System;
using System.Collections.Generic;

namespace Eede.Domain.ImageEditing.DrawingTools;

public class Drawer(Picture drawingPicture, PenStyle penCase)
{
    public readonly Picture DrawingPicture = drawingPicture;

    private readonly PenStyle PenStyle = penCase;

    public (Picture Picture, PictureArea Area) DrawPoint(CanvasCoordinate coordinate)
    {
        return DrawPoint(coordinate.ToPosition());
    }

    public (Picture Picture, PictureArea Area) DrawPoint(Position position)
    {
        PictureArea area = GetPointArea(position);
        Picture result = DrawingPicture.Draw(dest =>
        {
            byte[] imageData = DrawPointRoutine(dest, dest.CloneImage(), position);
            return Picture.Create(dest.Size, imageData);
        }, PenStyle.Blender);
        return (result, area);
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

    private PictureArea GetPointArea(Position position)
    {
        if (PenStyle.Width == 1)
        {
            return new PictureArea(position, new PictureSize(1, 1));
        }
        int d = PenStyle.Width / 2;
        int dd = PenStyle.Width - d;
        Position fromPosition = new(position.X - d, position.Y - d);
        return new PictureArea(fromPosition, new PictureSize(PenStyle.Width, PenStyle.Width));
    }

    public (Picture Picture, PictureArea Area) DrawLine(Position from, Position to)
    {
        (int fx, int fy, int tx, int ty) = NormalizePosition(from, to);
        int d = PenStyle.Width / 2;
        int dd = PenStyle.Width - d;
        PictureArea area = new(new Position(fx - d, fy - d), new PictureSize(tx - fx + PenStyle.Width, ty - fy + PenStyle.Width));

        Picture result = DrawingPicture.Draw(dest =>
        {
            byte[] imageData = dest.CloneImage();
            int width = dest.Width;
            int height = dest.Height;
            int stride = dest.Stride;
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
                    if (cy >= 0 && cy < height && cx >= 0 && cx < width)
                    {
                        imageData = DrawPointRoutine(dest, imageData, new Position(cx, cy));
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
                    if (cy >= 0 && cy < height && cx >= 0 && cx < width)
                    {
                        imageData = DrawPointRoutine(dest, imageData, new Position(cx, cy));
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
        return (result, area);
    }

    public (Picture Picture, PictureArea Area) DrawEllipse(Position from, Position to)
    {
        (int fx, int fy, int tx, int ty) = NormalizePosition(from, to);
        PictureArea area = new(new Position(fx, fy), new PictureSize(tx - fx + 1, ty - fy + 1));

        Picture result = DrawingPicture.Draw(dest =>
        {
            byte[] imageData = dest.CloneImage();
            ArgbColor color = PenStyle.Color;

            int width = tx - fx;
            int height = ty - fy;
            int stride = dest.Stride;
            int imgWidth = dest.Width;
            int imgHeight = dest.Height;
            if (width == 0 && height == 0)
            {
                DrawPixel(imageData, stride, imgWidth, imgHeight, fx, fy, color);
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

                DrawPixel(imageData, stride, imgWidth, imgHeight, cx, cy, color);
                DrawPixel(imageData, stride, imgWidth, imgHeight, cx, my, color);
                DrawPixel(imageData, stride, imgWidth, imgHeight, mx, cy, color);
                DrawPixel(imageData, stride, imgWidth, imgHeight, mx, my, color);

                int cxy = centerX + y;
                int cyx = centerY + x;
                int mxy = mirrorX - y;
                int myx = mirrorY - x;

                DrawPixel(imageData, stride, imgWidth, imgHeight, cxy, cyx, color);
                DrawPixel(imageData, stride, imgWidth, imgHeight, cxy, myx, color);
                DrawPixel(imageData, stride, imgWidth, imgHeight, mxy, cyx, color);
                DrawPixel(imageData, stride, imgWidth, imgHeight, mxy, myx, color);

                d += dx;
                dx += 8;
                x++;
            }

            return Picture.Create(dest.Size, imageData);
        }, PenStyle.Blender);
        return (result, area);
    }



    public (Picture Picture, PictureArea Area) DrawFillEllipse(Position from, Position to)
    {
        (int fx, int fy, int tx, int ty) = NormalizePosition(from, to);
        PictureArea area = new(new Position(fx, fy), new PictureSize(tx - fx + 1, ty - fy + 1));

        Picture result = DrawingPicture.Draw(dest =>
        {
            byte[] imageData = DrawFillEllipseRoutine(dest, dest.CloneImage(), from, to);
            return Picture.Create(dest.Size, imageData);
        }, PenStyle.Blender);
        return (result, area);
    }

    private byte[] DrawFillEllipseRoutine(Picture dest, byte[] imageData, Position from, Position to)
    {
        ArgbColor color = PenStyle.Color;

        (int fx, int fy, int tx, int ty) = NormalizePosition(from, to);

        int width = tx - fx;
        int height = ty - fy;
        int stride = dest.Stride;
        int imgWidth = dest.Width;
        int imgHeight = dest.Height;
        if (width == 0 && height == 0)
        {
            DrawPixel(imageData, stride, imgWidth, imgHeight, fx, fy, color);
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
            int cx = centerX + x;
            int cy = centerY + y;
            int mx = mirrorX - x;
            int my = mirrorY - y;

            DrawScanLine(imageData, stride, imgWidth, imgHeight, mx, cy, cx, color);
            DrawScanLine(imageData, stride, imgWidth, imgHeight, mx, my, cx, color);

            int cxy = centerX + y;
            int cyx = centerY + x;
            int mxy = mirrorX - y;
            int myx = mirrorY - x;

            DrawScanLine(imageData, stride, imgWidth, imgHeight, mxy, cyx, cxy, color);
            DrawScanLine(imageData, stride, imgWidth, imgHeight, mxy, myx, cxy, color);

            d += dx;
            dx += 8;
            x++;
        }

        return imageData;
    }

    public (Picture Picture, PictureArea Area) DrawRectangle(Position from, Position to)
    {
        (int fx, int fy, int tx, int ty) = NormalizePosition(from, to);
        PictureArea area = new(new Position(fx, fy), new PictureSize(tx - fx + 1, ty - fy + 1));

        Picture result = DrawingPicture.Draw(dest =>
        {
            byte[] imageData = dest.CloneImage();
            ArgbColor color = PenStyle.Color;
            int stride = dest.Stride;
            int width = dest.Width;
            int height = dest.Height;
            DrawScanLine(imageData, stride, width, height, fx, fy, tx, color);
            for (int i = fy; i < ty; i++)
            {
                DrawPixel(imageData, stride, width, height, fx, i, color);
                DrawPixel(imageData, stride, width, height, tx, i, color);
            }
            DrawScanLine(imageData, stride, width, height, fx, ty, tx, color);
            return Picture.Create(dest.Size, imageData);
        }, PenStyle.Blender);
        return (result, area);
    }

    public (Picture Picture, PictureArea Area) DrawFillRectangle(Position from, Position to)
    {
        (int fx, int fy, int tx, int ty) = NormalizePosition(from, to);
        PictureArea area = new(new Position(fx, fy), new PictureSize(tx - fx + 1, ty - fy + 1));

        Picture result = DrawingPicture.Draw(dest =>
        {
            byte[] imageData = dest.CloneImage();
            ArgbColor color = PenStyle.Color;
            int stride = dest.Stride;
            int width = dest.Width;
            int height = dest.Height;

            for (int i = fy; i <= ty; i++)
            {
                DrawScanLine(imageData, stride, width, height, fx, i, tx, color);
            }
            return Picture.Create(dest.Size, imageData);
        }, PenStyle.Blender);
        return (result, area);
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


    private static void DrawPixel(byte[] imageArray, int stride, int width, int height, int x, int y, ArgbColor color)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return;
        }
        int index = (x * 4) + (stride * y);
        imageArray[index] = color.Blue;
        imageArray[index + 1] = color.Green;
        imageArray[index + 2] = color.Red;
        imageArray[index + 3] = color.Alpha;
    }

    private static void DrawScanLine(byte[] imageArray, int stride, int width, int height, int x, int y, int toX, ArgbColor color)
    {
        if (y < 0 || y >= height)
        {
            return;
        }
        int startX = Math.Max(0, x);
        int endX = Math.Min(width - 1, toX);
        if (startX > endX)
        {
            return;
        }

        int index = (startX * 4) + (stride * y);
        for (int i = startX; i <= endX; i++)
        {
            imageArray[index] = color.Blue;
            imageArray[index + 1] = color.Green;
            imageArray[index + 2] = color.Red;
            imageArray[index + 3] = color.Alpha;
            index += 4;
        }
    }

    public (Picture Picture, PictureArea Area) Fill(CanvasCoordinate from)
    {
        return Fill(from.ToPosition());
    }

    public (Picture Picture, PictureArea Area) Fill(Position from)
    {
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        bool anyFilled = false;

        Picture result = DrawingPicture.Draw(dest =>
        {
            int cWidth = dest.Width;
            int cHeight = dest.Height;
            byte[] imageData = dest.CloneImage();
            ArgbColor targetColor = dest.PickColor(from);
            ArgbColor color = PenStyle.Color;
            if (targetColor == color)
            {
                return Picture.Create(dest.Size, imageData);
            }

            int stride = dest.Stride;

            bool IsTargetColor(int x, int y)
            {
                if (x < 0 || x >= cWidth || y < 0 || y >= cHeight)
                {
                    return false;
                }
                int index = (x * 4) + (stride * y);
                return imageData[index] == targetColor.Blue &&
                       imageData[index + 1] == targetColor.Green &&
                       imageData[index + 2] == targetColor.Red &&
                       imageData[index + 3] == targetColor.Alpha;
            }

            void UpdateBounds(int x, int y)
            {
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
                anyFilled = true;
            }

            Stack<Position> stack = new();
            stack.Push(from);
            while (stack.Count > 0)
            {
                Position p = stack.Pop();
                int x = p.X;
                int y = p.Y;
                while (x >= 0 && IsTargetColor(x, y))
                {
                    x--;
                }
                x++;
                bool spanAbove = false;
                bool spanBelow = false;
                while (x < cWidth && IsTargetColor(x, y))
                {
                    DrawPixel(imageData, stride, cWidth, cHeight, x, y, color);
                    UpdateBounds(x, y);
                    if (!spanAbove && y > 0 && IsTargetColor(x, y - 1))
                    {
                        stack.Push(new Position(x, y - 1));
                        spanAbove = true;
                    }
                    else if (spanAbove && y > 0 && !IsTargetColor(x, y - 1))
                    {
                        spanAbove = false;
                    }

                    if (!spanBelow && y < cHeight - 1 && IsTargetColor(x, y + 1))
                    {
                        stack.Push(new Position(x, y + 1));
                        spanBelow = true;
                    }
                    else if (spanBelow && y < cHeight - 1 && !IsTargetColor(x, y + 1))
                    {
                        spanBelow = false;
                    }
                    x++;
                }
            }
            return Picture.Create(dest.Size, imageData);
        }, PenStyle.Blender);

        PictureArea area = anyFilled
            ? new PictureArea(new Position(minX, minY), new PictureSize(maxX - minX + 1, maxY - minY + 1))
            : new PictureArea(new Position(0, 0), new PictureSize(0, 0));
        return (result, area);
    }

    public bool Contains(Position position)
    {
        return DrawingPicture.Contains(position);
    }
}