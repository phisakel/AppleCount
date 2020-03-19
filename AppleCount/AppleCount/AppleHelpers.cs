using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using System.Threading;
using Xamanimation;

namespace AppleCount
{
    static class AppleHelpers
    {
        public static double GetLeft(this View e)
        {
            return AbsoluteLayout.GetLayoutBounds(e).X;
        }

        public static double GetTop(this View e)
        {
            return AbsoluteLayout.GetLayoutBounds(e).Y;
        }

        public static Rectangle Bounds(this View e)
        {
            return new Rectangle(e.GetLeft(), e.GetTop(), e.Width, e.Height);
        }
        /*
                public async static Task OpenAsync(this MediaElement media, Uri uri, CancellationToken cancel = default(CancellationToken))
                {
                    // TODO: This code still needs auditing for the case "media Is Nothing"

                    if (cancel.IsCancellationRequested)
                        return;

                    if (media.CurrentState == MediaElementState.Buffering || media.CurrentState == MediaElementState.Opening || media.CurrentState == MediaElementState.Playing)
                        // Throw New Exception("MediaElement not ready to open")
                        return;
                    else if ((media.CurrentState == MediaElementState.Paused || media.CurrentState == MediaElementState.Stopped) && uri == null)
                        return;

                    TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

                    Action lambdaOpened = () => tcs.TrySetResult(null);

        Input: 
                Dim lambdaChanged As RoutedEventHandler = Sub() If media.CurrentState = MediaElementState.Closed Then tcs.TrySetResult(Nothing)

        
        Action lambdaFailed = (s, e) => tcs.TrySetException(new Exception(e.ErrorMessage));
            CancellationTokenRegistration? cancelReg = default(CancellationTokenRegistration?);

            try
            {
                media.MediaOpened += lambdaOpened;
                media.MediaFailed += lambdaFailed;
                media.CurrentStateChanged += lambdaChanged;

                if (uri != null)
                    media.Source = uri;
                else
                    // It's possible that it changed state either before we added the handlers (or after...)
                    // NB. No way to distinguish "closed" due to xaml-not-yet-loaded-the-source vs closed due to xaml-loaded-a-failed-source
                    if (media.CurrentState == MediaElementState.Paused || media.CurrentState == MediaElementState.Stopped)
                    return;

                if (!tcs.Task.IsCompleted)
                {
                    cancelReg = cancel.Register(() =>
                    {
                        var dummy = media.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => media.ClearValue(MediaElement.SourceProperty));
                    });
                }

                await tcs.Task;
            }
            finally
            {
                media.MediaOpened -= lambdaOpened;
                media.MediaFailed -= lambdaFailed;
                media.CurrentStateChanged -= lambdaChanged;
                if (cancelReg.HasValue)
                    cancelReg.Value.Dispose();
            }
        }
       
        public async static Task PlayAsync(this MediaElement media, CancellationToken cancel = default(CancellationToken))
        {
            if (cancel.IsCancellationRequested)
                return;

            // TODO: This code still needs auditing for this call to OpenAsync.
            // Q. Under what circumstances will a call to media.Play *enqueue* that request?
            // Q. What is the best behavior for when the element is already playing something else?
            await OpenAsync(media, null/* TODO Change to default(_) if this is not a reference type , cancel);

            if (media.CurrentState != MediaElementState.Paused && media.CurrentState != MediaElementState.Stopped)
                return;

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            RoutedEventHandler lambdaEnded = () => tcs.TrySetResult(null);
   

Input: 
        Dim lambdaChanged As RoutedEventHandler = Sub() If media.CurrentState = MediaElementState.Stopped Then tcs.TrySetResult(Nothing)

 
            CancellationTokenRegistration? cancelReg = default(CancellationTokenRegistration?);

            try
            {
                media.MediaEnded += lambdaEnded;
                media.CurrentStateChanged += lambdaChanged;

                media.Play();

                if (!tcs.Task.IsCompleted)
                {
                    cancelReg = cancel.Register(() =>
                    {
                        var dummy = media.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => media.Stop());
                    });
                }

                await tcs.Task;
            }
            finally
            {
                media.MediaEnded -= lambdaEnded;
                media.CurrentStateChanged -= lambdaChanged;
                if (cancelReg.HasValue)
                    cancelReg.Value.Dispose();
            }
        }
 */
        public static Task PlayAsync(this StoryBoard sb, CancellationToken cancel = default(CancellationToken))
        {
            if (cancel.IsCancellationRequested)
                return Task.CompletedTask;
            return sb.Begin();
            /*
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            EventHandler<object> lambda = () => tcs.TrySetResult(null);

            sb.Completed += lambda;
            try
            {
                sb.Begin();
                using (var cancelReg = cancel.Register(() =>
                {
                    var dummyTask = sb.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => sb.Stop());
                    tcs.TrySetResult(null);
                }))
                {
                    await tcs.Task;
                }
            }
            finally
            {
                sb.Completed -= lambda;
            }
            */
        }

        public async static void FireAndForget(this Task t)
        {
            await t;
        }

        public async static Task WhenClicked(this Button btn, CancellationToken cancel = default(CancellationToken))
        {
            if (cancel.IsCancellationRequested)
                return;
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            EventHandler lambda = (s, e) => tcs.TrySetResult(null);
            try
            {
                using (var cancelReg = cancel.Register(() => tcs.TrySetResult(null)))
                {
                    btn.Clicked += lambda;
                    await tcs.Task;
                }
            }
            finally
            {
                btn.Clicked -= lambda;
            }
        }
    }
}
