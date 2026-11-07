using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ThePixelRealms
{
    /// <summary>
    /// Streams static world visuals around the current camera.
    /// NPCs, enemies, collision platforms and gameplay systems are deliberately not managed here.
    /// </summary>
    public sealed class WorldStreamingSystem
    {
        private sealed class StreamedElement
        {
            public FrameworkElement Element { get; init; }
            public double WorldX { get; init; }
            public double Width { get; init; }
            public double ParallaxFactor { get; init; }
            public int Order { get; init; }
            public bool IsAttached { get; set; }
        }

        private readonly Canvas canvas;
        private readonly List<StreamedElement> elements = new();
        private readonly double preloadDistance;
        private int nextOrder;

        public WorldStreamingSystem(Canvas canvas, double preloadDistance = 1800)
        {
            this.canvas = canvas;
            this.preloadDistance = preloadDistance;
        }

        /// <summary>
        /// Replaces one large tiled Rectangle with one streamable Rectangle per complete tile.
        /// The template Width must be an exact multiple of tileWidth.
        /// </summary>
        public void RegisterTiledRectangle(Rectangle template, double tileWidth)
        {
            if (template == null || tileWidth <= 0)
                return;

            double worldX = Canvas.GetLeft(template);
            if (double.IsNaN(worldX)) worldX = 0;

            double rawCount = template.Width / tileWidth;
            int tileCount = (int)Math.Round(rawCount);

            if (tileCount <= 0 || Math.Abs(rawCount - tileCount) > 0.0001)
                throw new InvalidOperationException(
                    $"{template.Name}: Width ({template.Width}) must be an exact multiple of tile width ({tileWidth}).");

            double top = Canvas.GetTop(template);
            int zIndex = Panel.GetZIndex(template);
            double factor = ParallaxSystem.GetFactor(template);

            Brush fill = template.Fill;
            if (fill is Freezable freezable && freezable.CanFreeze && !freezable.IsFrozen)
                freezable.Freeze();

            canvas.Children.Remove(template);

            for (int i = 0; i < tileCount; i++)
            {
                var chunk = new Rectangle
                {
                    Width = tileWidth,
                    Height = template.Height,
                    Fill = fill,
                    SnapsToDevicePixels = template.SnapsToDevicePixels
                };

                RenderOptions.SetBitmapScalingMode(chunk, RenderOptions.GetBitmapScalingMode(template));
                Panel.SetZIndex(chunk, zIndex);
                Canvas.SetTop(chunk, top);

                double chunkWorldX = worldX + (i * tileWidth);
                Canvas.SetLeft(chunk, chunkWorldX);
                chunk.Tag = chunkWorldX;
                ParallaxSystem.SetFactor(chunk, factor);

                Register(chunk, chunkWorldX, tileWidth, factor, attachNow: false);
            }
        }

        /// <summary>Registers a single static visual as a streamable world element.</summary>
        public void Register(FrameworkElement element)
        {
            if (element == null)
                return;

            double worldX = Canvas.GetLeft(element);
            if (double.IsNaN(worldX)) worldX = 0;

            double width = element.Width;
            if (double.IsNaN(width) || width <= 0)
                width = element.ActualWidth;

            double factor = ParallaxSystem.GetFactor(element);
            element.Tag = worldX;

            Register(element, worldX, width, factor, canvas.Children.Contains(element));
        }

        private void Register(FrameworkElement element, double worldX, double width, double factor, bool attachNow)
        {
            elements.Add(new StreamedElement
            {
                Element = element,
                WorldX = worldX,
                Width = width,
                ParallaxFactor = factor,
                Order = nextOrder++,
                IsAttached = attachNow
            });
        }

        /// <summary>
        /// Keeps only nearby static visuals in the Canvas visual tree.
        /// Uses screen-space bounds, so parallax layers are culled correctly too.
        /// </summary>
        public void Update(double cameraX)
        {
            double viewportWidth = canvas.ActualWidth;
            if (viewportWidth <= 0)
                return;

            double minScreenX = -preloadDistance;
            double maxScreenX = viewportWidth + preloadDistance;

            foreach (var item in elements)
            {
                double screenLeft = item.WorldX - (cameraX * item.ParallaxFactor);
                double screenRight = screenLeft + item.Width;
                bool shouldBeAttached = screenRight >= minScreenX && screenLeft <= maxScreenX;

                if (shouldBeAttached)
                {
                    // Position before attaching, so a newly streamed chunk never flashes at an old X.
                    Canvas.SetLeft(item.Element, screenLeft);

                    if (!item.IsAttached)
                    {
                        canvas.Children.Add(item.Element);
                        item.IsAttached = true;
                    }
                }
                else if (item.IsAttached)
                {
                    canvas.Children.Remove(item.Element);
                    item.IsAttached = false;
                }
            }
        }

        public bool Manages(FrameworkElement element)
        {
            return elements.Any(x => ReferenceEquals(x.Element, element));
        }
    }
}
