Imports System.Threading
Imports Xamarin.Forms

' The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

''' <summary>
''' A basic page that provides characteristics common to most applications.
''' </summary>
Public NotInheritable Class ApplePage
    Inherits ContentPage

    Private Const ZFirstFreeApple = 3
    Private highestZIndex As Integer = ZFirstFreeApple
    Private appleImage As New Image(New Uri("ms-appx:///Assets/apple.png"))
    Private RND As New Random

    Protected Async Sub OnNavigatedTo(e As EventArgs)

        If FreeApples.Count = 0 Then
            StartGameAsync().FireAndForget()
        Else

            Dim rustleSoundTask = RustleSound.PlayAsync()
            Using wobbleCancel As New CancellationTokenSource
                Dim wobbleAppleTasks As IEnumerable(Of Task)
                wobbleAppleTasks = (From apple In FreeApples
                                    Select AnimateAppleWobbleAsync(apple, wobbleCancel.Token)).ToList()
                Await rustleSoundTask
                wobbleCancel.Cancel()
                Await Task.WhenAll(wobbleAppleTasks)
            End Using
        End If

    End Sub


    Public ReadOnly Property FreeApples As IEnumerable(Of Image)
        Get
            Return (From i In BasketCanvas.Children.OfType(Of Image)()
                    Where i.Source Is appleImage AndAlso Canvas.GetZIndex(i) >= ZFirstFreeApple).ToList()
        End Get
    End Property

    Public ReadOnly Property AllApples As IEnumerable(Of Image)
        Get
            Return (From i In BasketCanvas.Children.OfType(Of Image)() Where i.Source Is appleImage).ToList()
        End Get
    End Property

    Protected Overrides Async Sub OnPointerPressed(e As PointerRoutedEventArgs)
        Dim apple = TryCast(e.OriginalSource, Image)
        If Not FreeApples.Contains(apple) Then Return
        highestZIndex += 1
        Canvas.SetZIndex(apple, highestZIndex)
        Dim endpt = Await DragAsync(apple, e, BasketCatchmentArea.Bounds)
        If Not BasketCatchmentArea.Bounds.Contains(endpt) Then Return

        Dim soundTask = WhooshSound.PlayAsync()
        Dim animateTask = AnimateAppleWhooshAsync(apple)
        Await Task.WhenAll(soundTask, animateTask)

        If FreeApples.Count = 0 Then
            EndGameAsync().FireAndForget()
        End If
    End Sub

    Private Async Function StartGameAsync() As Task
        Dim positions As New List(Of Position)
        Dim bounds = TreeArea
        Dim xrad = bounds.Width / 2, yrad = bounds.Height / 2
        Dim cx = Canvas.GetLeft(bounds) + xrad, cy = Canvas.GetTop(bounds) + yrad
        For i = 0 To 6
            Dim a = (RND.Next() Mod 360) / 360 * 2 * Math.PI
            Dim x = cx + Math.Cos(a) * xrad * RND.NextDouble()
            Dim y = cy + Math.Sin(a) * yrad * RND.NextDouble()
            Dim r = CDbl((RND.Next() Mod 110) - 30)
            positions.Add(New Position(x, y, r, ZFirstFreeApple))
        Next

        CreateAppleImages(positions)

        Using soundCancel = New CancellationTokenSource
            Dim soundTask = DropSound.PlayAsync(soundCancel.Token)

            Dim startDelay = 0
            Dim insertTasks As New List(Of Task)
            For Each apple In FreeApples
                insertTasks.Add(AnimateAppleInsertAsync(apple, startDelay))
                startDelay += 100
            Next
            Await Task.WhenAll(insertTasks)
            soundCancel.Cancel()
            Await soundTask
        End Using
    End Function



    Async Function EndGameAsync() As Task
        Dim soundTask = AwaySound.PlayAsync()
        Dim sbTask = CType(Resources("BasketAwayStoryboard"), Storyboard).PlayAsync()
        Await Task.WhenAll(soundTask, sbTask)

        Dim lambda As SizeChangedEventHandler = Sub(s, e)
                                                    FinishGrid.Width = Window.Current.Bounds.Width
                                                    FinishGrid.Height = Window.Current.Bounds.Height
                                                End Sub
        lambda(Nothing, Nothing)
        AddHandler SizeChanged, lambda
        FinishPopup.Visibility = Windows.UI.Xaml.Visibility.Visible
        FinishPopup.IsOpen = True

        Await CType(Resources("FinishInStoryboard"), Storyboard).PlayAsync()
        Dim againTask = btnPlayAgain.WhenClicked()
        Dim homeTask = btnHome.WhenClicked()
        Await Task.WhenAny(againTask, homeTask)
        If homeTask.IsCompleted Then GoHome(Me, Nothing) : Return

        Dim ct = CType(BasketCanvas.RenderTransform, CompositeTransform)
        ct.SkewX = 0
        ct.TranslateX = 0
        FinishPopup.IsOpen = False
        FinishPopup.Visibility = Windows.UI.Xaml.Visibility.Collapsed
        RemoveHandler SizeChanged, lambda
        Await StartGameAsync()
    End Function

    Async Function AnimateAppleWobbleAsync(apple As Image, cancel As CancellationToken) As Task
        Dim r = CType(apple.RenderTransform, CompositeTransform).Rotation
        Dim sb As New Storyboard
        Dim an As New DoubleAnimation With {.From = r - 30, .To = r + 30, .Duration = TimeSpan.FromMilliseconds(200), .AutoReverse = True, .RepeatBehavior = RepeatBehavior.Forever}
        sb.Children.Add(an)
        Storyboard.SetTarget(an, apple.RenderTransform)
        Storyboard.SetTargetProperty(an, "Rotation")
        Await sb.PlayAsync(cancel)
    End Function

    Async Function AnimateAppleInsertAsync(apple As Image, delayMs As Integer) As Task
        apple.Visibility = Windows.UI.Xaml.Visibility.Collapsed
        Dim sb As New Storyboard
        Dim anx As New DoubleAnimation With {.From = 4, .To = 1, .Duration = TimeSpan.FromMilliseconds(500)}
        Dim any As New DoubleAnimation With {.From = 4, .To = 1, .Duration = TimeSpan.FromMilliseconds(500)}
        Storyboard.SetTarget(sb, apple.RenderTransform)
        Storyboard.SetTargetProperty(anx, "ScaleX")
        Storyboard.SetTargetProperty(any, "ScaleY")
        sb.Children.Add(anx)
        sb.Children.Add(any)
        Await Task.Delay(delayMs)
        apple.Visibility = Windows.UI.Xaml.Visibility.Visible
        Await sb.PlayAsync()
    End Function

    Async Function AnimateAppleWhooshAsync(apple As Image) As Task
        Dim ax = Canvas.GetLeft(apple)
        Dim ay = Canvas.GetTop(apple)
        Dim bx = Canvas.GetLeft(BasketBottomArea) + RND.Next() Mod (BasketBottomArea.Width - apple.Width)
        Dim by = Canvas.GetTop(BasketBottomArea) + RND.Next() Mod (BasketBottomArea.Height - apple.Height)
        Canvas.SetLeft(apple, bx)
        Canvas.SetTop(apple, by)
        Canvas.SetZIndex(apple, 1)
        Dim sb As New Storyboard
        Dim anx = New DoubleAnimation With {.From = ax - bx, .To = 0, .Duration = TimeSpan.FromMilliseconds(200)}
        Dim any = New DoubleAnimation With {.From = ay - by, .To = 0, .Duration = TimeSpan.FromMilliseconds(200)}
        Storyboard.SetTarget(sb, apple.RenderTransform)
        Dim x As CompositeTransform = Nothing
        Storyboard.SetTargetProperty(anx, "TranslateX")
        Storyboard.SetTargetProperty(any, "TranslateY")
        sb.Children.Add(anx)
        sb.Children.Add(any)
        Await sb.PlayAsync()
    End Function


    Async Function DragAsync(shape As UIElement, origE As PointerRoutedEventArgs, bounds As Rect) As Task(Of Point)
        Dim tcs As New TaskCompletionSource(Of Point)
        Dim parent = CType(VisualTreeHelper.GetParent(shape), Windows.UI.Xaml.UIElement)
        Dim origPos = New Point(Canvas.GetLeft(shape), Canvas.GetTop(shape))
        Dim origPt = origE.GetCurrentPoint(parent).Position
        Dim lambdaReleased As PointerEventHandler = Nothing
        Dim lambdaMoved As PointerEventHandler = Nothing

        lambdaReleased = Sub(s, e) tcs.TrySetResult(e.GetCurrentPoint(parent).Position)

        lambdaMoved = Sub(s, e)
                          Dim pt = e.GetCurrentPoint(parent).Position
                          Canvas.SetLeft(shape, origPos.X + pt.X - origPt.X)
                          Canvas.SetTop(shape, origPos.Y + pt.Y - origPt.Y)
                          If bounds.Contains(pt) Then tcs.TrySetResult(pt)
                      End Sub

        shape.CapturePointer(origE.Pointer)
        AddHandler shape.PointerMoved, lambdaMoved
        AddHandler shape.PointerReleased, lambdaReleased
        AddHandler shape.PointerCanceled, lambdaReleased
        AddHandler shape.PointerCaptureLost, lambdaReleased
        Try
            Return Await tcs.Task
        Finally
            shape.ReleasePointerCapture(origE.Pointer)
            RemoveHandler shape.PointerMoved, lambdaMoved
            RemoveHandler shape.PointerReleased, lambdaReleased
            RemoveHandler shape.PointerCanceled, lambdaReleased
            RemoveHandler shape.PointerReleased, lambdaReleased
        End Try
    End Function

    ''' <summary>
    ''' Populates the page with content passed during navigation.  Any saved state is also
    ''' provided when recreating a page from a prior session.
    ''' </summary>
    ''' <param name="navigationParameter">The parameter value passed to
    ''' <see cref="Frame.Navigate"/> when this page was initially requested.
    ''' </param>
    ''' <param name="pageState">A dictionary of state preserved by this page during an earlier
    ''' session.  This will be null the first time a page is visited.</param>
    Protected Overrides Sub LoadState(navigationParameter As Object, pageState As Dictionary(Of String, Object))
        If pageState Is Nothing OrElse Not pageState.ContainsKey("positions") Then Return
        Dim positions = CType(pageState("positions"), List(Of Position))
        CreateAppleImages(positions)
    End Sub

    ''' <summary>
    ''' Preserves state associated with this page in case the application is suspended or the
    ''' page is discarded from the navigation cache.  Values must conform to the serialization
    ''' requirements of <see cref="Common.SuspensionManager.SessionState"/>.
    ''' </summary>
    ''' <param name="pageState">An empty dictionary to be populated with serializable state.</param>
    Protected Overrides Sub SaveState(pageState As Dictionary(Of String, Object))
        Dim positions = (From apple In AllApples
                         Let x = Canvas.GetLeft(apple), y = Canvas.GetTop(apple)
                         Let r = CType(apple.RenderTransform, CompositeTransform).Rotation
                         Let z = Canvas.GetZIndex(apple)
                         Select New Position(x, y, r, z)).ToList()

        pageState("positions") = positions
    End Sub


    Private Sub CreateAppleImages(positions As List(Of Position))
        For Each apple In AllApples
            BasketCanvas.Children.Remove(apple)
        Next

        For Each position In positions
            Dim apple = New Image With {.Source = appleImage, .Width = 80, .Height = 80}
            apple.RenderTransform = New CompositeTransform With {.Rotation = position.Item3, .CenterX = 40, .CenterY = 40}
            BasketCanvas.Children.Add(apple)
            Canvas.SetLeft(apple, position.Item1)
            Canvas.SetTop(apple, position.Item2)
            Canvas.SetZIndex(apple, position.Item4)
        Next
    End Sub

End Class


Module AppleHelpers
    <Extension> Function Left(e As UIElement) As Double
        Return Canvas.GetLeft(e)
    End Function

    <Extension> Function Top(e As UIElement) As Double
        Return Canvas.GetTop(e)
    End Function

    <Extension> Function Bounds(e As FrameworkElement) As Rect
        Return New Rect(e.Left, e.Top, e.Width, e.Height)
    End Function

    <Extension> Async Function OpenAsync(media As MediaElement, uri As Uri,
                               Optional cancel As CancellationToken = Nothing) As Task
        ' TODO: This code still needs auditing for the case "media Is Nothing"

        If cancel.IsCancellationRequested Then
            Return
        End If

        If media.CurrentState = MediaElementState.Buffering OrElse
                         media.CurrentState = MediaElementState.Opening OrElse
                         media.CurrentState = MediaElementState.Playing Then
            'Throw New Exception("MediaElement not ready to open")
            Return
        ElseIf (media.CurrentState = MediaElementState.Paused OrElse media.CurrentState = MediaElementState.Stopped) AndAlso uri Is Nothing Then
            Return
        End If

        Dim tcs As New TaskCompletionSource(Of Object)

        Dim lambdaOpened As RoutedEventHandler = Sub() tcs.TrySetResult(Nothing)
        Dim lambdaChanged As RoutedEventHandler = Sub() If media.CurrentState = MediaElementState.Closed Then tcs.TrySetResult(Nothing)
        Dim lambdaFailed As ExceptionRoutedEventHandler = Sub(s, e) tcs.TrySetException(New Exception(e.ErrorMessage))
        Dim cancelReg As CancellationTokenRegistration? = Nothing

        Try
            AddHandler media.MediaOpened, lambdaOpened
            AddHandler media.MediaFailed, lambdaFailed
            AddHandler media.CurrentStateChanged, lambdaChanged

            If uri IsNot Nothing Then
                media.Source = uri
            Else
                ' It's possible that it changed state either before we added the handlers (or after...)
                ' NB. No way to distinguish "closed" due to xaml-not-yet-loaded-the-source vs closed due to xaml-loaded-a-failed-source
                If media.CurrentState = MediaElementState.Paused OrElse media.CurrentState = MediaElementState.Stopped Then Return
            End If

            If Not tcs.Task.IsCompleted Then
                cancelReg = cancel.Register(
                    Sub()
                        Dim dummy = media.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Sub() media.ClearValue(MediaElement.SourceProperty))
                    End Sub)
            End If

            Await tcs.Task
        Finally
            RemoveHandler media.MediaOpened, lambdaOpened
            RemoveHandler media.MediaFailed, lambdaFailed
            RemoveHandler media.CurrentStateChanged, lambdaChanged
            If cancelReg.HasValue Then cancelReg.Value.Dispose()
        End Try
    End Function

    <Extension> Async Function PlayAsync(media As MediaElement,
                               Optional cancel As CancellationToken = Nothing) As Task
        If cancel.IsCancellationRequested Then Return

        ' TODO: This code still needs auditing for this call to OpenAsync.
        ' Q. Under what circumstances will a call to media.Play *enqueue* that request?
        ' Q. What is the best behavior for when the element is already playing something else?
        Await OpenAsync(media, Nothing, cancel)

        If media.CurrentState <> MediaElementState.Paused AndAlso media.CurrentState <> MediaElementState.Stopped Then
            Return
            'Throw New Exception("MediaElement not ready to play")
        End If

        Dim tcs As New TaskCompletionSource(Of Object)

        Dim lambdaEnded As RoutedEventHandler = Sub() tcs.TrySetResult(Nothing)
        Dim lambdaChanged As RoutedEventHandler = Sub() If media.CurrentState = MediaElementState.Stopped Then tcs.TrySetResult(Nothing)
        Dim cancelReg As CancellationTokenRegistration? = Nothing

        Try
            AddHandler media.MediaEnded, lambdaEnded
            AddHandler media.CurrentStateChanged, lambdaChanged

            media.Play()

            If Not tcs.Task.IsCompleted Then
                cancelReg = cancel.Register(
                    Sub()
                        Dim dummy = media.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Sub() media.Stop())
                    End Sub)
            End If

            Await tcs.Task
        Finally
            RemoveHandler media.MediaEnded, lambdaEnded
            RemoveHandler media.CurrentStateChanged, lambdaChanged
            If cancelReg.HasValue Then cancelReg.Value.Dispose()
        End Try

    End Function

    <Extension> Async Function PlayAsync(sb As Animation.Storyboard, Optional cancel As CancellationToken = Nothing) As Task
        If cancel.IsCancellationRequested Then Return
        Dim tcs As New TaskCompletionSource(Of Object)
        Dim lambda As EventHandler(Of Object) = Sub() tcs.TrySetResult(Nothing)

        AddHandler sb.Completed, lambda
        Try
            sb.Begin()
            Using cancelReg = cancel.Register(Sub()
                                                  Dim dummyTask = sb.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Sub() sb.Stop())
                                                  tcs.TrySetResult(Nothing)
                                              End Sub)
                Await tcs.Task
            End Using
        Finally
            RemoveHandler sb.Completed, lambda
        End Try
    End Function

    <Extension> Async Sub FireAndForget(t As Task)
        Await t
        ' If "t" ends with an exception, this will post it to the UI thread.
        ' I could chose to catch it here if I wanted.
    End Sub

    <Extension> Async Function WhenClicked(btn As Button, Optional cancel As CancellationToken = Nothing) As Task
        If cancel.IsCancellationRequested Then Return
        Dim tcs As New TaskCompletionSource(Of Object)
        Dim lambda As RoutedEventHandler = Sub(s, e) tcs.TrySetResult(Nothing)
        Try
            Using cancelReg = cancel.Register(Sub() tcs.TrySetResult(Nothing))
                AddHandler btn.Click, lambda
                Await tcs.Task
            End Using
        Finally
            RemoveHandler btn.Click, lambda
        End Try
    End Function

End Module
