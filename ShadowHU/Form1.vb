Imports System.Diagnostics
Imports System.IO
Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class Form1
    Dim historyFile As String = Path.Combine(Application.StartupPath, "history.txt")

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If File.Exists(historyFile) Then
            Dim lines = File.ReadAllLines(historyFile).ToList()
            ComboBox1.Items.AddRange(lines.ToArray())
        End If
    End Sub

    Private Sub SaveToHistory(pcName As String)
        If Not ComboBox1.Items.Contains(pcName) Then
            ComboBox1.Items.Insert(0, pcName)
        Else
            ComboBox1.Items.Remove(pcName)
            ComboBox1.Items.Insert(0, pcName)
        End If

        While ComboBox1.Items.Count > 15
            ComboBox1.Items.RemoveAt(ComboBox1.Items.Count - 1)
        End While

        File.WriteAllLines(historyFile, ComboBox1.Items.Cast(Of String)())
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim pcName As String = ComboBox1.Text.Trim()
        If String.IsNullOrEmpty(pcName) Then
            MessageBox.Show("Por favor, insira o nome ou IP do PC.")
            Return
        End If

        SaveToHistory(pcName)

        Try
            Dim psi As New ProcessStartInfo With {
                .FileName = "cmd.exe",
                .Arguments = $"/c query session /server:{pcName}",
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .UseShellExecute = False,
                .CreateNoWindow = True
            }

            Dim proc As Process = Process.Start(psi)
            Dim output As String = proc.StandardOutput.ReadToEnd()
            Dim [error] As String = proc.StandardError.ReadToEnd()
            proc.WaitForExit()

            If Not String.IsNullOrEmpty([error]) Then
                MessageBox.Show("Erro ao consultar sessões:" & vbCrLf & [error])
                Return
            End If

            Dim linhasValidas As New List(Of String)
            For Each linha In output.Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                If Not linha.ToLower().Contains("disc") AndAlso Not linha.ToLower().Contains("services") Then
                    linhasValidas.Add(linha.Trim())
                End If
            Next

            If linhasValidas.Count = 0 Then
                MessageBox.Show("Nenhuma sessão conectada encontrada. Abrindo RDP padrão...")
                Process.Start("mstsc.exe", $"/v:{pcName}")
                Return
            End If

            Dim outputFiltrado As String = String.Join(vbCrLf, linhasValidas)
            Dim input As String = Microsoft.VisualBasic.Interaction.InputBox(
                $"Sessões conectadas encontradas no {pcName}:" & vbCrLf & vbCrLf & outputFiltrado & vbCrLf & vbCrLf & "Digite o ID da sessão:",
                "Selecionar ID"
            )

            If String.IsNullOrEmpty(input) OrElse Not Integer.TryParse(input, Nothing) Then
                MessageBox.Show("ID da sessão inválido ou não informado.")
                Return
            End If

            Dim mstscCommand As String = $"mstsc /shadow:{input} /v:{pcName} /control /noConsentPrompt"
            Process.Start("cmd.exe", $"/c start {mstscCommand}")

        Catch ex As Exception
            MessageBox.Show("Erro: " & ex.Message)
        End Try
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim pcName As String = ComboBox1.Text.Trim()
        If String.IsNullOrEmpty(pcName) Then
            MessageBox.Show("Por favor, insira o nome ou IP do PC.")
            Return
        End If

        SaveToHistory(pcName)

        Try
            Dim psi As New ProcessStartInfo With {
                .FileName = "cmd.exe",
                .Arguments = $"/c query session /server:{pcName}",
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .UseShellExecute = False,
                .CreateNoWindow = True
            }

            Dim proc As Process = Process.Start(psi)
            Dim output As String = proc.StandardOutput.ReadToEnd()
            Dim [error] As String = proc.StandardError.ReadToEnd()
            proc.WaitForExit()

            If Not String.IsNullOrEmpty([error]) Then
                MessageBox.Show("Erro ao consultar sessões:" & vbCrLf & [error])
                Return
            End If

            Dim linhasValidas As New List(Of String)
            For Each linha In output.Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                If Not linha.ToLower().Contains("disc") AndAlso Not linha.ToLower().Contains("services") Then
                    linhasValidas.Add(linha.Trim())
                End If
            Next

            If linhasValidas.Count = 0 Then
                MessageBox.Show("Nenhuma sessão conectada encontrada.")
                Return
            End If

            Dim outputFiltrado As String = String.Join(vbCrLf, linhasValidas)
            Dim input As String = Microsoft.VisualBasic.Interaction.InputBox(
                $"Sessões conectadas encontradas no {pcName}:" & vbCrLf & vbCrLf & outputFiltrado & vbCrLf & vbCrLf & "Digite o ID da sessão:",
                "Selecionar ID"
            )

            If String.IsNullOrEmpty(input) OrElse Not Integer.TryParse(input, Nothing) Then
                MessageBox.Show("ID da sessão inválido ou não informado.")
                Return
            End If

            Dim mstscCommand As String = $"mstsc /shadow:{input} /v:{pcName} /noConsentPrompt"
            Process.Start("cmd.exe", $"/c start {mstscCommand}")

        Catch ex As Exception
            MessageBox.Show("Erro: " & ex.Message)
        End Try
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim pcName As String = ComboBox1.Text.Trim()
        If String.IsNullOrEmpty(pcName) Then
            MessageBox.Show("Por favor, insira o nome ou IP do PC.")
            Return
        End If

        Try
            Dim regCommand As String = $"reg add ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services"" /v Shadow /t REG_DWORD /d 2 /f"
            Dim psexecCommand As String = $"\\{pcName} {regCommand}"

            Dim psi As New ProcessStartInfo With {
                .FileName = "psexec.exe",
                .Arguments = psexecCommand,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .UseShellExecute = False,
                .CreateNoWindow = True
            }

            Dim proc As Process = Process.Start(psi)
            Dim output As String = proc.StandardOutput.ReadToEnd()
            Dim [error] As String = proc.StandardError.ReadToEnd()
            proc.WaitForExit()

            If proc.ExitCode = 0 Then
                MessageBox.Show("Regra aplicada com sucesso!" & vbCrLf & output)
            Else
                MessageBox.Show("Erro ao aplicar a regra (ExitCode: " & proc.ExitCode & "):" & vbCrLf & output & vbCrLf & [error])
            End If

        Catch ex As Exception
            MessageBox.Show("Erro: " & ex.Message)
        End Try
    End Sub

    Private Sub Acessar_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Button3_Click_1(sender As Object, e As EventArgs) Handles Button3.Click

    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click

    End Sub

    Private Sub Label3_Click_1(sender As Object, e As EventArgs) Handles Label3.Click

    End Sub
End Class
