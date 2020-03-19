Public Class Position
    Private _x As Double
    Public Property X() As Double
        Get
            Return _x
        End Get
        Set(ByVal value As Double)
            _x = value
        End Set
    End Property

    Private _y As Double
    Public Property Y() As Double
        Get
            Return _y
        End Get
        Set(ByVal value As Double)
            _y = value
        End Set
    End Property

    Private _r As Double
    Public Property R() As Double
        Get
            Return _r
        End Get
        Set(ByVal value As Double)
            _r = value
        End Set
    End Property

    Private _a As Integer

    Public Sub New(x As Double, y As Double, r As Double, a As Integer)
        _x = x
        _y = y
        _r = r
        _a = a
    End Sub

    Public Property A() As Integer
        Get
            Return _a
        End Get
        Set(ByVal value As Integer)
            _a = value
        End Set
    End Property
End Class
