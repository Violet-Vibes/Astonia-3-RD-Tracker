Public Class Maze
    Public display As New Bitmap(255, 255)
    Private buffer As New Bitmap(255, 255)
    Private grDisp As Graphics = Graphics.FromImage(display)
    Private grBuff As Graphics = Graphics.FromImage(buffer)

    Public cata As Boolean
    Private size, div, bound As Integer
    Private Boxes(,,) As Rectangle

    Public Sub New(Optional cata As Boolean = False)
        Clear(Color.Black)
        Me.cata = cata
        size = If(cata, 3, 4)
        div = 141 - 20 * size '81 or 61
        bound = 34 - 5 * size '19 or 14
        Boxes = New Rectangle(bound, bound, 2) {}
        For Y = 0 To bound
            For X = 0 To bound
                Boxes(X, Y, 0) = New Rectangle(size * (4 * X + 1), size * (4 * Y + 1), size * 3, size * 3)
                Boxes(X, Y, 1) = New Rectangle(size * (4 * X + 1), size * 4 * Y, size * 3, size)
                Boxes(X, Y, 2) = New Rectangle(size * 4 * X, size * (4 * Y + 1), size, size * 3)
            Next
        Next
    End Sub

    Private Function toMaz(GameX As Integer, GameY As Integer) As Point
        Return New Point(((GameX - 2) Mod div) * size, ((GameY - 2) Mod div) * size)
    End Function

    Public Function getNum(GameX As Integer, GameY As Integer) As Integer
        Return ((GameX - 2) \ div + 1) + (((GameY - 2) \ div) * size)
    End Function

    Private brushWhite As New SolidBrush(Color.White)
    Public Sub plotMaze(GameX As Integer, GameY As Integer)
        Dim player As Point = toMaz(GameX, GameY)
        For Each box As Rectangle In Boxes
            If box.Contains(player) Then
                grBuff.FillRectangle(brushWhite, box)
                Exit Sub
            End If
        Next
    End Sub

    Public Sub plotPlayer(GameX As Integer, GameY As Integer, col As Color)
        Dim Player As Point = toMaz(GameX, GameY)
        grDisp.FillRectangle(New SolidBrush(col), Player.X, Player.Y, size, size)
    End Sub
    Private ptZero As New Point(0, 0)

    Public Sub Update()
        grDisp.DrawImage(buffer, ptZero)
    End Sub
    Public Sub Clear(col As Color)
        grDisp.Clear(col)
        grBuff.Clear(col)
    End Sub
    Public Function isLobby(GameX As Integer, GameY As Integer) As Boolean
        If cata Then
            If (GameX >= 245 OrElse GameY >= 245) Then
                Return True
            End If
        Else
            If (GameX >= 226 And GameX <= 253 And GameY <= 253 And GameY >= 247) Or
               (GameX >= 247 And GameX <= 253 And GameY <= 246 And GameY >= 226) Then
                Return True
            End If
        End If
        Return False
    End Function
    Public Function isYendor(GameX As Integer, GameY As Integer) As Boolean
        If Not cata AndAlso (GameX >= 2 And GameX <= 43 And GameY >= 248 And GameY <= 252) Then
            Return True
        End If
        Return False
    End Function
End Class
