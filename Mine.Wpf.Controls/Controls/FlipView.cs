using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Specialized;

namespace Mine.Wpf.Controls.Controls
{
    public class FlipView : Selector
    {
        static FlipView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlipView), new FrameworkPropertyMetadata(typeof(FlipView)));
        }

        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }

        // use this control's ItemContainerGenerator
        private int _lastSelectedIndex = -1;

        // Animation dependency properties
        public static readonly DependencyProperty TransitionDurationProperty = DependencyProperty.Register(
            nameof(TransitionDuration), typeof(Duration), typeof(FlipView), new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(320))));

        public Duration TransitionDuration
        {
            get => (Duration)GetValue(TransitionDurationProperty);
            set => SetValue(TransitionDurationProperty, value);
        }

        public static readonly DependencyProperty TransitionOffsetRatioProperty = DependencyProperty.Register(
            nameof(TransitionOffsetRatio), typeof(double), typeof(FlipView), new PropertyMetadata(0.6));

        public double TransitionOffsetRatio
        {
            get => (double)GetValue(TransitionOffsetRatioProperty);
            set => SetValue(TransitionOffsetRatioProperty, value);
        }

        public static readonly DependencyProperty TransitionEasingModeProperty = DependencyProperty.Register(
            nameof(TransitionEasingMode), typeof(EasingMode), typeof(FlipView), new PropertyMetadata(EasingMode.EaseOut));

        public EasingMode TransitionEasingMode
        {
            get => (EasingMode)GetValue(TransitionEasingModeProperty);
            set => SetValue(TransitionEasingModeProperty, value);
        }

        public FlipView()
        {
            NextCommand = new RelayCommand(_ => Next());
            PreviousCommand = new RelayCommand(_ => Previous());
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            EnsureAllContainers();
            // initialize last selected to current so first transition has a valid baseline
            _lastSelectedIndex = SelectedIndex;
        }

        public void Next()
        {
            if (Items.Count <= 1) return;
            var target = (SelectedIndex + 1) % Items.Count;
            EnsureAllContainers();
            UpdateLayout();
            SelectedIndex = target;
            // animation handled in OnSelectionChanged
        }

        public void Previous()
        {
            if (Items.Count <= 1) return;
            var target = (SelectedIndex - 1 + Items.Count) % Items.Count;
            EnsureAllContainers();
            UpdateLayout();
            SelectedIndex = target;
        }

        protected override DependencyObject GetContainerForItemOverride() => new FlipViewItem();
        protected override bool IsItemItsOwnContainerOverride(object item) => item is FlipViewItem;

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            AnimateTransition(_lastSelectedIndex, SelectedIndex);
            _lastSelectedIndex = SelectedIndex;
        }

        private void EnsureAllContainers()
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var container = ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (container == null) continue;
                if (container.RenderTransform is not TransformGroup)
                {
                    var tg = new TransformGroup();
                    tg.Children.Add(new TranslateTransform(0, 0));
                    container.RenderTransform = tg;
                    container.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                // set initial visibility: only selected visible
                container.Visibility = (i == SelectedIndex) ? Visibility.Visible : Visibility.Collapsed;
                container.Opacity = (i == SelectedIndex) ? 1 : 0;
            }
        }

        private void AnimateTransition(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return;
            var count = Items.Count;
            if (count == 0) return;

            // normalize indices
            if (oldIndex < 0 || oldIndex >= count) oldIndex = -1;
            if (newIndex < 0 || newIndex >= count) newIndex = -1;

            var oldContainer = oldIndex >= 0 ? ItemContainerGenerator.ContainerFromIndex(oldIndex) as FrameworkElement : null;
            var newContainer = newIndex >= 0 ? ItemContainerGenerator.ContainerFromIndex(newIndex) as FrameworkElement : null;

            var width = ActualWidth > 0 ? ActualWidth : 400;
            double direction = 0;
            if (oldIndex >= 0 && newIndex >= 0)
                direction = newIndex > oldIndex ? 1 : -1;

            var ds = TransitionDuration;
            var ease = new CubicEase { EasingMode = TransitionEasingMode };

            if (oldContainer != null)
            {
                EnsureContainerTransform(oldContainer);
                var tg = oldContainer.RenderTransform as TransformGroup;
                var tt = tg?.Children[0] as TranslateTransform;
                if (tt != null)
                {
                    var anim = new DoubleAnimation(-direction * width * TransitionOffsetRatio, ds) { EasingFunction = ease };
                    tt.BeginAnimation(TranslateTransform.XProperty, anim);
                }
                oldContainer.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, ds));
                // after animation start, collapse others immediately except old/new
                for (int i = 0; i < count; i++)
                {
                    if (i == oldIndex || i == newIndex) continue;
                    var c = ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    if (c != null)
                    {
                        c.Visibility = Visibility.Collapsed;
                        c.Opacity = 0;
                    }
                }
            }

            if (newContainer != null)
            {
                EnsureContainerTransform(newContainer);
                var tg = newContainer.RenderTransform as TransformGroup;
                var tt = tg?.Children[0] as TranslateTransform;
                if (tt != null)
                {
                    // start from outside
                    tt.X = direction * width * TransitionOffsetRatio;
                    var anim = new DoubleAnimation(0, ds) { EasingFunction = ease };
                    tt.BeginAnimation(TranslateTransform.XProperty, anim);
                }
                newContainer.Visibility = Visibility.Visible;
                newContainer.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, ds));
                // ensure other containers are collapsed (already done above for non old/new)
                for (int i = 0; i < count; i++)
                {
                    if (i == newIndex) continue;
                    var c = ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    if (c != null && c != newContainer)
                    {
                        c.Visibility = Visibility.Collapsed;
                        c.Opacity = 0;
                    }
                }
            }
        }

        private void EnsureContainerTransform(FrameworkElement container)
        {
            if (container.RenderTransform is TransformGroup == false)
            {
                var tg = new TransformGroup();
                tg.Children.Add(new TranslateTransform(0, 0));
                container.RenderTransform = tg;
                container.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }
    }
}
