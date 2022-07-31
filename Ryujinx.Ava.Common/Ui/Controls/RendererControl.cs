using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Ryujinx.Common.Configuration;
using SPB.Windowing;
using System;

namespace Ryujinx.Ava.Common.Ui.Controls
{
    public abstract class RendererControl : Control
    {
        protected object _image;

        static RendererControl()
        {
            AffectsRender<RendererControl>(ImageProperty);
        }

        public readonly static StyledProperty<object> ImageProperty =
            AvaloniaProperty.Register<RendererControl, object>(
                nameof(Image),
                0,
                inherits: true,
                defaultBindingMode: BindingMode.TwoWay);

        protected object Image
        {
            get => _image;
            set => SetAndRaise(ImageProperty, ref _image, value);
        }

        public event EventHandler<EventArgs> RendererInitialized;
        public event EventHandler<Size> SizeChanged;

        protected Size RenderSize { get; private set; }
        public bool IsStarted { get; private set; }

        public GraphicsDebugLevel DebugLevel { get; }

        private bool _isInitialized;

        protected ICustomDrawOperation DrawOperation { get; private set; }

        public RendererControl(GraphicsDebugLevel graphicsDebugLevel)
        {
            DebugLevel = graphicsDebugLevel;
            IObservable<Rect> resizeObservable = this.GetObservable(BoundsProperty);

            resizeObservable.Subscribe(Resized);

            Focusable = true;
        }

        protected void Resized(Rect rect)
        {
            SizeChanged?.Invoke(this, rect.Size);

            if (!rect.IsEmpty)
            {
                RenderSize = rect.Size * VisualRoot.RenderScaling;

                DrawOperation?.Dispose();
                DrawOperation = CreateDrawOperation();
            }
        }

        protected abstract ICustomDrawOperation CreateDrawOperation();
        protected abstract void CreateWindow();

        public override void Render(DrawingContext context)
        {
            if (!_isInitialized)
            {
                CreateWindow();

                OnRendererInitialized();
                _isInitialized = true;
            }

            if (!IsStarted || Image == null)
            {
                return;
            }

            if (DrawOperation != null)
            {
                context.Custom(DrawOperation);
            }

            base.Render(context);
        }

        protected void OnRendererInitialized()
        {
            RendererInitialized?.Invoke(this, EventArgs.Empty);
        }

        public void QueueRender()
        {
            AppConfig.RenderTimer.TickNow();
        }

        public abstract void Present(object image);

        public void Start()
        {
            IsStarted = true;
            QueueRender();
        }

        public void Stop()
        {
            IsStarted = false;
        }

        public abstract void DestroyBackgroundContext();
        public abstract void MakeCurrent();
        public abstract void MakeCurrent(SwappableNativeWindowBase window);
    }
}