using Eede.Domain.Pictures;
using Eede.Domain.Pictures.Actions;
using System;

namespace Eede.Presentation.Common.Pictures.Actions
{
    public enum PictureActions
    {
        ShiftUp,
        ShiftDown,
        ShiftLeft,
        ShiftRight,
        FlipVertical,
        FlipHorizontal,
        RotateRight,
        RotateLeft
    }
    public static class PictureActionsExtension
    {

        public static Picture Execute(this PictureActions actionType, Picture previous)
        {
            return actionType switch
            {
                PictureActions.ShiftUp => new ShiftUpAction(previous).Execute(),
                PictureActions.ShiftDown => new ShiftDownAction(previous).Execute(),
                PictureActions.ShiftLeft => new ShiftLeftAction(previous).Execute(),
                PictureActions.ShiftRight => new ShiftRightAction(previous).Execute(),
                PictureActions.FlipHorizontal => new FlipHorizontalAction(previous).Execute(),
                PictureActions.FlipVertical => new FlipVerticalAction(previous).Execute(),
                //case PictureActions.RotateLeft:
                //    return;
                PictureActions.RotateRight => new RotateRightAction(previous).Execute(),
                _ => throw new ArgumentException(null, nameof(actionType)),
            };
        }
    }
}
