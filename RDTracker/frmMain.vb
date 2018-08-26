Imports System.ComponentModel
Imports System.Runtime.InteropServices
Public Class frmMain
    Private strWndClass As String = "MAINWNDMOAC"

    ' 1.5.2
    ' improved performance
    ' fixed 64 bit bug when closing ugaris client
    ' cleaned up code a bit


    ' version 1.5.1
    ' added support for ugaris
    ' keys:
    '   a3res   4625240
    '   ugaris  2241092
    '   



    ' version 1.5.0
    ' ported back from 64 bit
    ' fixed label showing wrong rd number
    ' added Catamode
    ' added Area to info label
    '       you can now leave area without messing up the map
    ' added Yendor to info label


    ' version 1.4.1 64bit invicta only
    ' rewrote core
    ' added functionality to handle invictica (64 bit shenannigans)
    ' improved error detection
    ' 

    ' version 1.4.0
    ' added multi setting
    ' extra functionality to label 
    '       shows Lobby # When In lobby region 
    '             Enter # when in rd
    '             Error # alt idled or key might be wrong
    ' fixed changing key wouldn't take effect until program restart
    '
    '
    Private _memManager As New MemoryManager


    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Sub GetClassName(ByVal hWnd As System.IntPtr, ByVal lpClassName As System.Text.StringBuilder, ByVal nMaxCount As Integer)
    End Sub

    Public Function GetWindowClass(ByVal hwnd As Long) As String
        Dim sClassName As New System.Text.StringBuilder("", 256)
        Call GetClassName(hwnd, sClassName, 256)
        Return sClassName.ToString
    End Function

    Public Maze As Maze = New Maze(My.Settings.Cata)


    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Location = My.Settings.Location
        Me.TopMost = My.Settings.Topmost

        Me.BackgroundImage = maze.display

        Dim strAltName As String = ""
        Dim intFound As Integer = 0
        For Each p As Process In Process.GetProcesses
            If GetWindowClass(p.MainWindowHandle) = strWndClass Then
                strAltName = Strings.Left(p.MainWindowTitle, p.MainWindowTitle.IndexOf(" - "))
                If strAltName <> "Someone" Then
                    cboAlt.Items.Add(strAltName)
                    intFound += 1
                End If
            End If
        Next

        cboAlt.SelectedIndex = If(intFound > 0, 1, 0)

        Me.Text = If(My.Settings.Cata, "CATA", "RD") & " Tracker"

    End Sub
    Private Sub cboAlt_DropDown(sender As Object, e As EventArgs) Handles cboAlt.DropDown
        Dim lstAst As New List(Of String)
        Dim Name As String = ""
        lstAst.Add("Someone")
        For Each p As Process In Process.GetProcesses
            If p.MainWindowTitle IsNot Nothing AndAlso
            GetWindowClass(p.MainWindowHandle) = strWndClass AndAlso
            Not p.MainWindowTitle.StartsWith("Someone") Then
                Name = Strings.Left(p.MainWindowTitle, p.MainWindowTitle.IndexOf(" - "))
                lstAst.Add(Name)
                If Not cboAlt.Items.Contains(Name) Then
                    cboAlt.Items.Add(Name)
                End If
            End If
        Next

        '    clean ComboBox of idled clients
        Dim i As Integer = 1
        Do While i < cboAlt.Items.Count
            If Not lstAst.Contains(cboAlt.Items(i)) Then
                If cboAlt.SelectedIndex = i Then
                    cboAlt.SelectedIndex = 0
                End If
                cboAlt.Items.RemoveAt(i)
                i -= 1
            End If
            i += 1
        Loop

    End Sub
    Private strSock As String
    Private Sub cboAlt_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboAlt.SelectedIndexChanged

        tmrTick.Enabled = False
        _memManager.DetachFromProcess()
        If cboAlt.SelectedIndex <> 0 Then
            If _memManager.TryAttachToProcess(cboAlt.SelectedItem & " - ", strWndClass) Then
                strSock = Strings.Right(_memManager.targetProcess.MainWindowTitle, 8)
                wasArea = False
                tmrTick.Enabled = True
            End If

        End If

    End Sub
    Dim brushWhite As New SolidBrush(Color.White)
    Dim brushRed As New SolidBrush(Color.Red)
    Dim brushCyan As New SolidBrush(Color.Cyan)
    Dim ptZero As New Point(0, 0)
    Dim prevP As New Point(0, 0)
    Dim gameX, gameY As Integer
    Dim wasArea As Boolean = False
    Private reader As New MemoryManager
    Private Function readMemPoint(pp As Process)
        Dim gameX, gameY As Integer
        reader.TryAttachToProcess(pp)

        If Not My.Settings.Ugaris Then
            gameX = reader.ReadInt32(My.Settings.Mem)
            gameY = reader.ReadInt32(My.Settings.Mem + 4)
        Else
            Dim base As Integer = pp.MainModule.BaseAddress
            gameY = reader.ReadInt32(base + My.Settings.Mem) 'note: Ugaris has X and Y swapped in memory
            gameX = reader.ReadInt32(base + My.Settings.Mem + 4)
        End If
        reader.DetachFromProcess()

        Return New Point(gameX, gameY)

    End Function
    Dim mainRdNum As Integer = 0
    Private Sub tmrTick_Tick(sender As Object, e As EventArgs) Handles tmrTick.Tick

        If cboAlt.SelectedIndex = 0 Then
            tmrTick.Enabled = False
            Exit Sub
        End If

        If Not My.Settings.Ugaris Then
            gameX = _memManager.ReadInt32(My.Settings.Mem)
            gameY = _memManager.ReadInt32(My.Settings.Mem + 4)
        Else
            Dim base As Integer = 0
            Try
                base = _memManager.targetProcess.MainModule.BaseAddress
            Catch ex As Exception
                lblEnter.Text = "Error " & mainRdNum
                prevP = ptZero
                tmrTick.Enabled = False
                Exit Sub
            End Try
            gameY = _memManager.ReadInt32(base + My.Settings.Mem)
            gameX = _memManager.ReadInt32(base + My.Settings.Mem + 4)
        End If


        If wasArea AndAlso Not Maze.isLobby(gameX, gameY) Then
            ' hack to prevent messing up map when changing area
            Exit Sub
        End If
        If gameX <= 0 OrElse gameY <= 0 OrElse gameX >= 255 OrElse gameY >= 255 Then
            'error in reading gamecoords
            lblEnter.Text = "Error " & mainRdNum
            prevP = ptZero
            Exit Sub
        End If
        _memManager.targetProcess.Refresh()
        If Not _memManager.targetProcess.MainWindowTitle.Contains(strSock) Then
            lblEnter.Text = "Area " & mainRdNum
            wasArea = True
            Exit Sub
        End If
        If maze.isLobby(gameX, gameY) Then
            lblEnter.Text = "Lobby " & mainRdNum
            prevP = ptZero
            wasArea = False
            Exit Sub
        End If
        If maze.isYendor(gameX, gameY) Then
            lblEnter.Text = "Yendor " & mainRdNum
            prevP = ptZero
            Exit Sub
        End If

        mainRdNum = Maze.getNum(gameX, gameY)

        lblEnter.Text = "Enter " & mainRdNum
        Dim newP As New Point(gameX, gameY)
        If prevP <> ptZero AndAlso prevP <> newP Then
            Maze.plotMaze(prevP.X, prevP.Y)
        End If
        prevP = New Point(gameX, gameY)

        Maze.update()

        If My.Settings.Multi Then
            Dim altP As Point
            Dim altNum As Integer
            For Each pp As Process In Process.GetProcesses
                If pp.MainWindowTitle IsNot Nothing AndAlso
                   pp.Id <> _memManager.targetProcess.Id AndAlso
                   Not pp.MainWindowTitle.StartsWith("Someone") AndAlso
                   GetWindowClass(pp.MainWindowHandle) = strWndClass AndAlso
                   pp.MainWindowTitle.Contains(strSock) Then
                    altP = readMemPoint(pp)
                    altNum = Maze.getNum(altP.X, altP.Y)
                    If altP <> ptZero AndAlso
                       altNum = mainRdNum Then
                        Maze.plotMaze(altP.X, altP.Y)
                        Maze.plotPlayer(altP.X, altP.Y, Color.Cyan)
                    End If
                End If
            Next
        End If

        Maze.plotPlayer(gameX, gameY, Color.Red)
        Me.BackgroundImage = Maze.display
        Me.Refresh()

    End Sub

    Private Sub btnSettings_Click(sender As Object, e As EventArgs) Handles btnSettings.Click
        frmSettings.Show()
        frmSettings.Focus()
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        tmrTick.Enabled = False
        _memManager.DetachFromProcess()
        If cboAlt.SelectedIndex <> 0 Then
            If _memManager.TryAttachToProcess(cboAlt.SelectedItem, strWndClass) Then
                strSock = Strings.Right(_memManager.targetProcess.MainWindowTitle, 8)
            End If
        End If
        lblEnter.Text = "Enter 0"
        mainRdNum = 0
        prevP = ptZero
        wasArea = False
        Maze.Clear(Color.Black)
        tmrTick.Enabled = True
        Me.Refresh()
    End Sub

    Private Sub frmMain_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If Me.WindowState = FormWindowState.Normal Then
            My.Settings.Location = Me.Location
        End If
    End Sub
End Class
