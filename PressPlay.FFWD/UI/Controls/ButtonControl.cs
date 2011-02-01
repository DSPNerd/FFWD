using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PressPlay.FFWD;
using PressPlay.FFWD.ScreenManager;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Input.Touch;
#endif
 
#if WINDOWS
using Microsoft.Xna.Framework.Input;
#endif

namespace PressPlay.FFWD.UI.Controls
{

    public class ButtonControlEventArgs : EventArgs
    {
        string _link;

        public ButtonControlEventArgs(string link)
        {
            this._link = link;
        }

        /// <summary>
        /// Gets the index of the player who triggered this event.
        /// </summary>
        public string link
        {
            get { return _link; }
        }


    }

    public enum ButtonControlStates
    {
        hover,
        normal,
        pressed,
        disabled
    }

    public class ButtonControl : Control
    {
        public TextControl textControl;
        public AudioClip buttonSound;
        private ImageControl background;
        private ButtonStyle buttonStyle;

        private ButtonControlStates previousState = ButtonControlStates.normal;
        private ButtonControlStates _state = ButtonControlStates.normal;
        public ButtonControlStates state
        {
            get
            {
                return _state;
            }
            set
            {

            }
        }

        private string link;
        private bool useCustomClickRect = false;
        private Rectangle _clickRect;
        public Rectangle clickRect
        {
            get
            {
                return _clickRect;
            }
            set
            {
                useCustomClickRect = (clickRect != null) ? true : false;
                _clickRect = value;
            }
        }

        /// <summary>
        /// Event raised when the menu entry is selected.
        /// </summary>
        public event EventHandler<EventArgs> OnClickEvent;

        /// <summary>
        /// Method for raising the Selected event.
        /// </summary>
        protected virtual void OnClickMethod()
        {            
            if (OnClickEvent != null)
            {
                OnClickEvent(this, new ButtonControlEventArgs(link));
            }
        }

        public ButtonControl(ButtonStyle buttonStyle, string link)
        {
            gameObject.name = "ButtonControl";

            this.buttonStyle = buttonStyle;
            this.link = link;

            background = new ImageControl(buttonStyle.texture, buttonStyle[ButtonControlStates.normal]);
            AddChild(background);

            textControl = new TextControl();
            AddChild(textControl);
        }

        private void ChangeState(ButtonControlStates newState)
        {
            switch(newState){
                case ButtonControlStates.normal:
                    
                    break;
                case ButtonControlStates.pressed:
                    break;
                case ButtonControlStates.hover:
                    break;
                case ButtonControlStates.disabled:
                    break;
            }

            // we get and set the correct texture
            ButtonTexture bt = buttonStyle.GetButtonTexture(newState);
            
            ((UISpriteRenderer)background.renderer).texture = bt.texture;
            ((UISpriteRenderer)background.renderer).sourceRect = bt.sourceRect;

            previousState = _state;
            _state = newState;
        }

        public void ScaleTextToFit()
        {
            // TODO This needs to able to scale the text to fit
            // Something is broken with the size / bounds logic after scaling
            textControl.transform.localScale = new Vector3(background.size.x / textControl.size.x);
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);

            if (isMouseWithinBounds(input))
            {
                if (input.isMouseDown)
                {
                    if (state != ButtonControlStates.pressed)
                    {
                        ChangeState(ButtonControlStates.pressed);
                    }
                }
                else if(input.isMouseUp)
                {
                    if (state == ButtonControlStates.pressed)
                    {
                        OnClickMethod();
                        ChangeState(ButtonControlStates.normal);
                    }
                }
                else if (state != ButtonControlStates.pressed && state != ButtonControlStates.hover)
                {
                    ChangeState(ButtonControlStates.hover);
                }
            }
            else
            {
                if (state == ButtonControlStates.hover || state == ButtonControlStates.pressed)
                {
                    ChangeState(ButtonControlStates.normal);
                }
            }

            //if (input.isMouseDown && IsMouseClickWithinBounds(new Point(ms.X, ms.Y)))
            //{

            //    if (onClickSourceRect != Rectangle.Empty)
            //    {
            //        background.sourceRect = onClickSourceRect;
            //    }

            //    isPressed = true;
            //}

            //if (input.isMouseUp && isPressed)
            //{

            //    if (IsMouseClickWithinBounds(new Point(ms.X, ms.Y)))
            //    {
            //        OnClickMethod();
            //    }
            //    else
            //    {
            //        background.sourceRect = sourceRect;
            //    }

            //    isPressed = false;
            //}
        }

        protected override bool isMouseWithinBounds(InputState input)
        {
            if (useCustomClickRect)
            {
                return isMouseWithinBounds(input, clickRect);
            }
            else
            {
                return base.isMouseWithinBounds(input);
            }
        }
    }
}
