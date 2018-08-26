
Public Class frmSettings

    Private Sub frmSettings_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        txtKey.Text = My.Settings.Mem

        chkTopmost.Checked = My.Settings.Topmost
        chkMulti.Checked = My.Settings.Multi
        chkCata.Checked = My.Settings.Cata


    End Sub
    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Try
            My.Settings.Mem = Long.Parse(txtKey.Text)
        Catch ex As Exception
            MessageBox.Show("Error in key")
            txtKey.Text = My.Settings.Mem
            txtKey.Focus()
            Exit Sub
        End Try

        My.Settings.Multi = chkMulti.Checked
        My.Settings.Topmost = chkTopmost.Checked
        frmMain.TopMost = chkTopmost.Checked



        If Not chkCata.Checked = My.Settings.Cata Then
            frmMain.tmrTick.Enabled = False
            My.Settings.Cata = chkCata.Checked
            frmMain.Maze = New Maze(My.Settings.Cata)
            frmMain.Text = If(My.Settings.Cata, "CATA", "RD") & " Tracker"
            frmMain.tmrTick.Enabled = True
        End If

        My.Settings.Save()
        Me.Close()
    End Sub

    Private Sub txtKey_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtKey.KeyPress
        If Not (Char.IsDigit(e.KeyChar) Or Char.IsControl(e.KeyChar)) Then
            e.Handled = True
        End If
    End Sub

    Private Sub chkUgaris_CheckedChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub chkMulti_CheckedChanged(sender As Object, e As EventArgs) Handles chkMulti.CheckedChanged

    End Sub

    Private Sub chkCata_CheckedChanged(sender As Object, e As EventArgs) Handles chkCata.CheckedChanged

    End Sub
End Class