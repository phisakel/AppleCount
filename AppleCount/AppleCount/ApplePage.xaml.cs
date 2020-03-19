using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Threading;
using Xamanimation;

namespace AppleCount
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ApplePage : ContentPage
    {
        public ApplePage()
        {
            InitializeComponent();
        }


        private const int ZFirstFreeApple = 3;
        private int highestZIndex = ZFirstFreeApple;
        private FileImageSource appleImage = new FileImageSource { File = "apple.png" };
        private Random RND = new Random();

        public static readonly BindableProperty ZIndexProperty = BindableProperty.CreateAttached("ZIndex", typeof(int), typeof(View), 1,
            propertyChanged: OnZIndexChanged);
        public static void OnZIndexChanged(BindableObject bindable, object oldValue, object newValue)
        {
        }

        public static int GetZIndex(View view)
        { return (int)view.GetValue(ZIndexProperty); }
        public static void SetZIndex(View view, int zindex)
        {
            view.SetValue(ZIndexProperty, zindex);
        }

        protected async void OnNavigatedTo(EventArgs e)
        {
            if (FreeApples.Count() == 0)
                StartGameAsync().FireAndForget();
            else
            {
                var rustleSoundTask = Sounds.RustleSound.PlayAsync();
                using (CancellationTokenSource wobbleCancel = new CancellationTokenSource())
                {
                    IEnumerable<Task> wobbleAppleTasks;
                    wobbleAppleTasks = (from apple in FreeApples
                                        select AnimateAppleWobbleAsync(apple, wobbleCancel.Token)).ToList();
                    await rustleSoundTask;
                    wobbleCancel.Cancel();
                    await Task.WhenAll(wobbleAppleTasks);
                }
            }
        }


        public IEnumerable<Image> FreeApples
        {
            get
            {
                return (from i in BasketCanvas.Children.OfType<Image>()
                        where ((FileImageSource)i.Source).File == appleImage.File && ApplePage.GetZIndex(i) >= ZFirstFreeApple
                        select i).ToList();
            }
        }

        public IEnumerable<Image> AllApples
        {
            get
            {
                return (from i in BasketCanvas.Children.OfType<Image>()
                        where ((FileImageSource)i.Source).File == appleImage.File
                        select i).ToList();
            }
        }
        private async void ImageTappGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var apple = sender as Image;
            if (!FreeApples.Contains(apple))
                return;
            highestZIndex += 1;
            ApplePage.SetZIndex(apple, highestZIndex);
            var endpt = await DragAsync(apple, Point.Zero, BasketCatchmentArea.Bounds);
            if (!BasketCatchmentArea.Bounds.Contains(endpt))
                return;

            var soundTask = Sounds.WhooshSound.PlayAsync();
            var animateTask = AnimateAppleWhooshAsync(apple);
            await Task.WhenAll(soundTask, animateTask);

            if (FreeApples.Count() == 0)
                EndGameAsync().FireAndForget();
        }

        private async Task StartGameAsync()
        {
            List<Position> positions = new List<Position>();
            var bounds = TreeArea;
            var xrad = bounds.Width / 2;
            var yrad = bounds.Height / 2;
            var rect = AbsoluteLayout.GetLayoutBounds(bounds);
            var cx = rect.X + xrad;
            var cy = rect.Y + yrad;
            for (var i = 0; i <= 6; i++)
            {
                var a = (RND.Next() % 360) / (double)360 * 2 * Math.PI;
                var x = cx + Math.Cos(a) * xrad * RND.NextDouble();
                var y = cy + Math.Sin(a) * yrad * RND.NextDouble();
                var r = System.Convert.ToDouble((RND.Next() % 110) - 30);
                positions.Add(new Position(x, y, r, ZFirstFreeApple));
            }

            CreateAppleImages(positions);

            using (var soundCancel = new CancellationTokenSource())
            {
                var soundTask = Sounds.DropSound.PlayAsync();

                var startDelay = 0;
                List<Task> insertTasks = new List<Task>();
                foreach (var apple in FreeApples)
                {
                    insertTasks.Add(AnimateAppleInsertAsync(apple, startDelay));
                    startDelay += 100;
                }
                await Task.WhenAll(insertTasks);
                soundCancel.Cancel();
                await soundTask;
            }
        }

        public async Task EndGameAsync()
        {
            var soundTask = Sounds.AwaySound.PlayAsync();
            var sbTask = ((StoryBoard)Resources["BasketAwayStoryboard"]).Begin();
            await Task.WhenAll(soundTask, sbTask);
            /*
            EventHandler lambda = (s, e) =>
            {
                FinishGrid.Width = Window.Current.Bounds.Width;
                FinishGrid.Height = Window.Current.Bounds.Height;
            };
            lambda(null);
            SizeChanged += lambda;
            FinishPopup.Visibility = Windows.UI.Xaml.Visibility.Visible;
            FinishPopup.IsOpen = true;

            await (StoryBoard)System.Resources("FinishInStoryboard").PlayAsync();
            var againTask = btnPlayAgain.WhenClicked();
            var homeTask = btnHome.WhenClicked();
            await Task.WhenAny(againTask, homeTask);
            if (homeTask.IsCompleted)
            {
                GoHome(this, null); return;
            }

            var ct = (CompositeTransform)BasketCanvas.RenderTransform;
            ct.SkewX = 0;
            ct.TranslateX = 0;
            FinishPopup.IsOpen = false;
            FinishPopup.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            SizeChanged -= lambda;
            await StartGameAsync();
            */
        }

        public async Task AnimateAppleWobbleAsync(Image apple, CancellationToken cancel)
        {
            double r = 0; //current rotation
            var sb = new StoryBoard(); // double animation
            var an = new RotateToAnimation() { Rotation = r - 30, Duration = TimeSpan.FromMilliseconds(200).ToString(), RepeatForever=true };
            sb.Animations.Add(an);
            var an2 = new RotateToAnimation() { Rotation = r + 30, Duration = TimeSpan.FromMilliseconds(200).ToString(), RepeatForever = true };
            sb.Animations.Add(an2);
            sb.Target = apple;
            await sb.Begin();
        }

        public async Task AnimateAppleInsertAsync(Image apple, int delayMs)
        {
            apple.IsVisible = false;
            StoryBoard sb = new StoryBoard(); //From = 4,
            var anx = new ScaleToAnimation() {  Scale = 1, Duration = TimeSpan.FromMilliseconds(500).ToString() };
            sb.Target = apple;
            sb.Animations.Add(anx);
            await Task.Delay(delayMs);
            apple.IsVisible = false;
            await sb.PlayAsync();
        }

        public async Task AnimateAppleWhooshAsync(Image apple)
        {
            var ax = AppleHelpers.GetLeft(apple);
            var ay = AppleHelpers.GetTop(apple);
            var bx = AppleHelpers.GetLeft(BasketBottomArea) + RND.Next() % (BasketBottomArea.Width - apple.Width);
            var by = AppleHelpers.GetTop(BasketBottomArea) + RND.Next() % (BasketBottomArea.Height - apple.Height);
            //Canvas.SetLeft(apple, bx); Canvas.SetTop(apple, by);
            AbsoluteLayout.SetLayoutBounds(apple, new Rectangle(bx, by, apple.Width, apple.Height));
            SetZIndex(apple, 1);
            var sb = new StoryBoard(); //From = ax - bx, From = ay - by
            var an = new TranslateToAnimation {  TranslateX = 0, TranslateY = 0, Duration = TimeSpan.FromMilliseconds(200).ToString() };
            sb.Target = apple;
            sb.Animations.Add(an);
            await sb.PlayAsync();
        }

        public void GoBack(object s, EventArgs e)
        {

        }

        public bool CanGoBack => true;
        public async Task<Point> DragAsync(View shape, Point origE, Rectangle bounds)
        {
            TaskCompletionSource<Point> tcs = new TaskCompletionSource<Point>();
           // var parent = (Windows.UI.Xaml.UIElement)VisualTreeHelper.GetParent(shape);
            var origPos = AbsoluteLayout.GetLayoutBounds(shape).Location;
            var origPt = Point.Zero; // origE.GetCurrentPoint(parent).Position;
            // PointerEventHandler lambdaReleased = null/* TODO Change to default(_) if this is not a reference type */;
            // PointerEventHandler lambdaMoved = null/* TODO Change to default(_) if this is not a reference type */;
            Point p = Point.Zero;
            EventHandler<Point> lambdaReleased = (s, e) => tcs.TrySetResult(p); // e.GetCurrentPoint(parent).Position);

            EventHandler<Point> lambdaMoved = (s, e) =>
            {
                var pt = Point.Zero; // e.GetCurrentPoint(parent).Position;
                AbsoluteLayout.SetLayoutBounds(shape, new Rectangle(origPos.X + pt.X - origPt.X, origPos.Y + pt.Y - origPt.Y, shape.Width, shape.Height));
                //Canvas.SetTop(shape,);
                if (bounds.Contains(pt))
                    tcs.TrySetResult(pt);
            };

            //shape.CapturePointer(origE.Pointer);
            //shape.PointerMoved += lambdaMoved;
            //shape.PointerReleased += lambdaReleased;
            //shape.PointerCanceled += lambdaReleased;
            //shape.PointerCaptureLost += lambdaReleased;
            try
            {
                return await tcs.Task;
            }
            finally
            {
                //shape.ReleasePointerCapture(origE.Pointer);
                //shape.PointerMoved -= lambdaMoved;
                //shape.PointerReleased -= lambdaReleased;
                //shape.PointerCanceled -= lambdaReleased;
                //shape.PointerReleased -= lambdaReleased;
            }
        }

        /// <summary>
        ///     ''' Populates the page with content passed during navigation.  Any saved state is also
        ///     ''' provided when recreating a page from a prior session.
        ///     ''' </summary>
        ///     ''' <param name="navigationParameter">The parameter value passed to
        ///     ''' <see cref="Frame.Navigate"/> when this page was initially requested.
        ///     ''' </param>
        ///     ''' <param name="pageState">A dictionary of state preserved by this page during an earlier
        ///     ''' session.  This will be null the first time a page is visited.</param>
        protected void LoadState(object navigationParameter, Dictionary<string, object> pageState)
        {
            if (pageState == null || !pageState.ContainsKey("positions"))
                return;
            var positions = (List<Position>)pageState["positions"];
            CreateAppleImages(positions);
        }

        protected void SaveState(Dictionary<string, object> pageState, List<Position> positions)
        {
              pageState["positions"] = positions;
        }

        private void CreateAppleImages(List<Position> positions)
        {
            foreach (var apple in AllApples)
                BasketCanvas.Children.Remove(apple);

            foreach (var position in positions)
            {
                var apple = new Image() { Source = appleImage, WidthRequest = 80, HeightRequest = 80 };
                var tapRec = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
                apple.GestureRecognizers.Add(tapRec);
                tapRec.Tapped += ImageTappGestureRecognizer_Tapped;
                var panRec = new PanGestureRecognizer();
                apple.GestureRecognizers.Add(panRec);
                // apple.RenderTransform = new CompositeTransform() { Rotation = position.Item3, CenterX = 40, CenterY = 40 };
                apple.Rotation = position.R;
                BasketCanvas.Children.Add(apple);
                AbsoluteLayout.SetLayoutBounds(apple, new Rectangle(position.X, position.Y, 80, 80));
                //Canvas.SetLeft(apple, position.Item1);Canvas.SetTop(apple, position.Item2);
                SetZIndex(apple, position.Z);
            }
        }

       
    }
}
